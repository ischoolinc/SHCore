using Aspose.Cells;
using FISCA.DSAUtil;
using FISCA.Presentation.Controls;
using Framework.Feature;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SmartSchool
{
    public partial class ClassRoomConfig : BaseForm
    {
        FISCA.Data.QueryHelper querySQL = new FISCA.Data.QueryHelper();

        K12.Data.UpdateHelper updateHelper = new K12.Data.UpdateHelper();

        List<ClassroomRecord> ClassroomList { get; set; }

        public ClassRoomConfig()
        {
            InitializeComponent();
        }

        private void ClassRoomConfig_Load(object sender, EventArgs e)
        {
            GetConfig();

            BindForm();
        }

        private void GetConfig()
        {
            DataTable dt = querySQL.Select("select * from classroom order by code,name");

            ClassroomList = new List<ClassroomRecord>();
            foreach (DataRow row in dt.Rows)
            {
                ClassroomRecord cr = new ClassroomRecord(row);
                ClassroomList.Add(cr);

            }

        }

        /// <summary>
        /// 建立畫面
        /// </summary>
        private void BindForm()
        {
            foreach (ClassroomRecord record in ClassroomList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewX1);
                row.Tag = record.id;
                row.Cells[0].Value = record.code;
                row.Cells[1].Value = record.name;
                row.Cells[2].Value = record.comment;

                dataGridViewX1.Rows.Add(row);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //檢查是否資料正確
            List<string> ErrorMessageList = RunChecked();
            if (ErrorMessageList != null)
            {
                MsgBox.Show("發生錯誤:\n" + string.Join("\n", ErrorMessageList));
                return;
            }

            //開始儲存
            try
            {
                string logvalue = RunSave();
                FISCA.LogAgent.ApplicationLog.Log("上課地點管理", "修改", logvalue);
            }
            catch (Exception ex)
            {
                MsgBox.Show("儲存發生錯誤:\n" + ex.Message);
            }

            MsgBox.Show("儲存完成");
        }

        private List<string> RunChecked()
        {
            List<string> codeList = new List<string>();
            List<string> ErrorMessageList = new List<string>();
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                //驗證代碼欄位
                if (!codeList.Contains("" + row.Cells[0].Value))
                {
                    codeList.Add("" + row.Cells[0].Value);
                    row.Cells[0].ErrorText = "";
                }
                else
                {
                    ErrorMessageList.Add("代碼欄位資料重複!");
                    row.Cells[0].ErrorText = "代碼欄位資料重複!";
                }
            }

            if (ErrorMessageList.Count > 0)
            {
                return ErrorMessageList;
            }
            else
                return null;

        }

        private string RunSave()
        {
            List<string> updateSQLList = new List<string>();
            int countID = 1;
            StringBuilder sb_log = new StringBuilder();
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string id = "" + countID;
                string code = "" + row.Cells[0].Value;
                string name = "" + row.Cells[1].Value;
                string comment = "" + row.Cells[2].Value;

                sb_log.AppendLine(string.Format("代碼「{0}」上課地點「{1}」備註「{2}」", code, name, comment));

                string sql = string.Format(@"INSERT INTO classroom (id ,code ,name ,comment )
VALUES ('{0}','{1}','{2}','{3}')", id, code, name, comment);

                updateSQLList.Add(sql);

                countID++;
            }

            //先刪除資料
            updateHelper.Execute("DELETE FROM classroom");

            //批次更新資料
            updateHelper.Execute(updateSQLList);

            return sb_log.ToString();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridViewX1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            RunChecked();
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            #region 匯出
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "匯出上課地點清單";
            saveFileDialog1.Filter = "Excel (*.xls)|*.xls";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            DataGridViewExport export = new DataGridViewExport(dataGridViewX1);
            export.Save(saveFileDialog1.FileName);

            if (new CompleteForm().ShowDialog() == DialogResult.Yes)
                System.Diagnostics.Process.Start(saveFileDialog1.FileName);
            #endregion
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            #region 確認畫面
            DialogResult dr = FISCA.Presentation.Controls.MsgBox.Show("匯入上課地點設定\n將覆蓋目前之資料狀態\n(建議可將原資料匯出備份)\n\n請確認繼續?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
            if (dr != DialogResult.Yes)
                return;

            Workbook wb = new Workbook();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "選擇要匯入的匯入上課地點檔案";
            ofd.Filter = "Excel檔案 (*.xls)|*.xls";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wb.Open(ofd.FileName);
                }
                catch
                {
                    FISCA.Presentation.Controls.MsgBox.Show("指定路徑無法存取。", "開啟檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
                return;

            //必要欄位
            List<string> requiredHeaders = new List<string>(new string[] { "代碼", "上課地點", "備註" });
            //欄位標題的索引
            Dictionary<string, int> headers = new Dictionary<string, int>();
            Worksheet ws = wb.Worksheets[0];
            for (int i = 0; i <= ws.Cells.MaxDataColumn; i++)
            {
                string header = ws.Cells[0, i].StringValue;
                if (requiredHeaders.Contains(header))
                    headers.Add(header, i);
            }

            //如果使用者匯入檔的欄位與必要欄位不符，則停止匯入
            if (headers.Count != requiredHeaders.Count)
            {
                StringBuilder builder = new StringBuilder(string.Empty);
                builder.AppendLine("匯入格式不符合。");
                builder.AppendLine("匯入資料標題必須包含：");
                builder.AppendLine(string.Join(",", requiredHeaders.ToArray()));
                FISCA.Presentation.Controls.MsgBox.Show(builder.ToString());
                return;
            }

            #endregion

            #region 匯入

            #region 匯入重覆問題
            List<string> NameList1 = new List<string>();
            StringBuilder NameSb = new StringBuilder();
            for (int x = 1; x <= wb.Worksheets[0].Cells.MaxDataRow; x++) //每一Row
            {
                string name = ws.Cells[x, headers["代碼"]].StringValue;

                if (string.IsNullOrEmpty(name.Trim())) //沒有缺曠名稱則跳過
                    continue;

                if (!NameList1.Contains(name.Trim()))
                {
                    NameList1.Add(name);
                }
                else
                {
                    NameSb.AppendLine("代碼重覆:" + name);
                }
            }

            if (!string.IsNullOrEmpty(NameSb.ToString()))
            {
                FISCA.Presentation.Controls.MsgBox.Show("匯入上課地點發生錯誤:\n" + NameSb.ToString());
                return;
            }
            #endregion

            //開始儲存
            try
            {
                string logvalue = RunExcelSave(wb, headers);
                FISCA.LogAgent.ApplicationLog.Log("上課地點管理", "匯入", logvalue);
            }
            catch (Exception ex)
            {
                MsgBox.Show("儲存發生錯誤:\n" + ex.Message);
            }

            FISCA.Presentation.Controls.MsgBox.Show("匯入成功!\n新設定將於畫面重新開啟時生效!", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();

            #endregion
        }

        private string RunExcelSave(Workbook wb, Dictionary<string, int> headers)
        {
            Worksheet ws = wb.Worksheets[0];

            List<string> updateSQLList = new List<string>();
            StringBuilder sb_log = new StringBuilder();
            for (int x = 1; x <= wb.Worksheets[0].Cells.MaxDataRow; x++) //每一Row
            {
                string code = ws.Cells[x, headers["代碼"]].StringValue;
                string name = ws.Cells[x, headers["上課地點"]].StringValue;
                string comment = ws.Cells[x, headers["備註"]].StringValue;

                if (string.IsNullOrEmpty(name.Trim()) || string.IsNullOrEmpty(code.Trim())) //沒有 代碼/上課地點 則跳過
                    continue;

                sb_log.AppendLine(string.Format("代碼「{0}」上課地點「{1}」備註「{2}」", code, name, comment));

                string sql = string.Format(@"INSERT INTO classroom (id ,code ,name ,comment )
VALUES ('{0}','{1}','{2}','{3}')", x, code, name, comment);

                updateSQLList.Add(sql);

            }


            //先刪除資料
            updateHelper.Execute("DELETE FROM classroom");

            //批次更新資料
            updateHelper.Execute(updateSQLList);

            return sb_log.ToString();
        }
    }

    class ClassroomRecord
    {
        public ClassroomRecord(DataRow row)
        {
            id = "" + row["id"];
            name = "" + row["name"];
            code = "" + row["code"];
            comment = "" + row["comment"];

        }

        public string id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string comment { get; set; }

    }
}
