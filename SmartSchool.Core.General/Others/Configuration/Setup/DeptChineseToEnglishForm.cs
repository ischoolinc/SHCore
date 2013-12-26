using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using DevComponents.DotNetBar;
using FISCA.DSAUtil;
using SmartSchool.Feature.Basic;
using System.Xml;
using Aspose.Cells;

namespace SmartSchool.Others.Configuration.Setup
{
    public partial class DeptChineseToEnglishForm : BaseForm
    {
        private Dictionary<string, string> _origList = new Dictionary<string, string>();
        private bool _isSave = true;

        public DeptChineseToEnglishForm()
        {
            InitializeComponent();
            InitialList();
        }

        private void InitialList()
        {
            XmlElement Data = SmartSchool.Customization.Data.SystemInformation.Configuration["科別中英文對照表"];
            foreach (XmlElement var in Data)
            {
                int index = dataGridViewX1.Rows.Add();
                DataGridViewRow row = dataGridViewX1.Rows[index];
                row.Cells[Chinese.Name].Value = var.GetAttribute("Chinese");
                row.Cells[English.Name].Value = var.GetAttribute("English");
                //ValidatedRow(row);
                _origList.Add(var.GetAttribute("Chinese"), var.GetAttribute("English"));
            }
        }

        private bool ValidateList()
        {
            dataGridViewX1.EndEdit();
            bool valid = true;
            _isSave = true;
            List<string> chineseList = new List<string>();

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells[Chinese.Name].Value == null)
                {
                    row.Cells[Chinese.Name].ErrorText = "不能為空白";
                    valid = false;
                    break;
                }
                else
                    row.Cells[Chinese.Name].ErrorText = "";

                string chineseValue = row.Cells[Chinese.Name].Value.ToString();

                if (!chineseList.Contains(chineseValue))
                {
                    chineseList.Add(chineseValue);
                    row.Cells[Chinese.Name].ErrorText = "";
                }
                else
                {
                    row.Cells[Chinese.Name].ErrorText = "名稱重複";
                    valid = false;
                    break;
                }

                //檢查資料是否變動
                if (_isSave)
                {
                    if (_origList.ContainsKey(chineseValue))
                    {
                        if (_origList[chineseValue] != ((row.Cells[English.Name].Value != null) ? row.Cells[English.Name].Value.ToString() : ""))
                            _isSave = false;
                    }
                    else
                        _isSave = false;
                }
            }

            if (_isSave)
            {
                if ((dataGridViewX1.Rows.Count - 1) != _origList.Keys.Count)
                    _isSave = false;
            }

            return valid;
        }

        private bool Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement content = doc.CreateElement("科別中英文對照表");
            _origList.Clear();

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;
                XmlElement Department = doc.CreateElement("Department");
                Department.SetAttribute("Chinese", row.Cells[Chinese.Name].Value.ToString());
                Department.SetAttribute("English", (row.Cells[English.Name].Value != null) ? row.Cells[English.Name].Value.ToString() : "");
                content.AppendChild(Department);

                _origList.Add(row.Cells[Chinese.Name].Value.ToString(), (row.Cells[English.Name].Value != null) ? row.Cells[English.Name].Value.ToString() : "");
            }

            try
            {

                SmartSchool.Customization.Data.SystemInformation.Configuration["科別中英文對照表"] = content;

                //Config.SetSubjectChineseToEnglishList(content);
                _isSave = true;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateList())
            {
                MsgBox.Show("資料有誤，尚未儲存");
                return;
            }

            if (Save())
                MsgBox.Show("儲存成功。");
            else
                MsgBox.Show("儲存失敗。");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            ValidateList();
        }

        private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dataGridViewX1.SelectedCells.Count == 1)
                dataGridViewX1.BeginEdit(true);
        }

        private void dataGridViewX1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridViewX1.EndEdit();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            Workbook wb = new Workbook();
            wb.Worksheets.Clear();
            Worksheet ws = wb.Worksheets[wb.Worksheets.Add()];
            ws.Name = "科別中英文對照表";

            ws.Cells.CreateRange(0, 1, true).ColumnWidth = 20;
            ws.Cells.CreateRange(1, 1, true).ColumnWidth = 40;

            ws.Cells[0, 0].PutValue("科別中文名稱");
            ws.Cells[0, 1].PutValue("科別英文名稱");

            int rowIndex = 1;

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                string chinese = (row.Cells[Chinese.Name].Value != null) ? row.Cells[Chinese.Name].Value.ToString() : "";
                string english = (row.Cells[English.Name].Value != null) ? row.Cells[English.Name].Value.ToString() : "";
                ws.Cells[rowIndex, 0].PutValue(string.IsNullOrEmpty(chinese) ? "" : chinese);
                ws.Cells[rowIndex, 1].PutValue(string.IsNullOrEmpty(english) ? "" : english);
                rowIndex++;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "科別中英文對照表.xls";
            sfd.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wb.Save(sfd.FileName, FileFormatType.Excel2003);
                    MsgBox.Show("匯出成功。");
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "另存檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            Workbook wb = new Workbook();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "選擇要匯入的科別中英文對照表";
            ofd.Filter = "Excel檔案 (*.xls)|*.xls";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wb.Open(ofd.FileName);
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "開啟檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
                return;

            DeptChineseToEnglishImport import = new DeptChineseToEnglishImport();
            if (import.ShowDialog() == DialogResult.OK)
            {
                Worksheet ws = wb.Worksheets[0];

                //粗略檢查
                if (ws.Cells[0, 0].StringValue != "科別中文名稱" || ws.Cells[0, 1].StringValue != "科別英文名稱")
                {
                    MsgBox.Show("匯入格式不符合。");
                    return;
                }

                if (import.Overwrite == true)
                    ImportOverwrite(ws);
                else
                    ImportAppend(ws);

                //檢驗
                ValidateList();
                MsgBox.Show("匯入完成。");
            }
        }

        private void ImportOverwrite(Worksheet ws)
        {
            dataGridViewX1.Rows.Clear();

            int rowIndex = 1;
            int gridIndex = 0;

            while (!string.IsNullOrEmpty(ws.Cells[rowIndex, 0].StringValue))
            {
                string chinese = ws.Cells[rowIndex, 0].StringValue;
                string english = string.IsNullOrEmpty(ws.Cells[rowIndex, 1].StringValue) ? "" : ws.Cells[rowIndex, 1].StringValue;
                rowIndex++;

                gridIndex = dataGridViewX1.Rows.Add();
                DataGridViewRow row = dataGridViewX1.Rows[gridIndex];
                row.Cells[Chinese.Name].Value = chinese;
                row.Cells[English.Name].Value = english;
            }
        }

        private void ImportAppend(Worksheet ws)
        {
            Dictionary<string, string> import_list = new Dictionary<string, string>();

            int rowIndex = 1;
            int gridIndex = 0;

            while (!string.IsNullOrEmpty(ws.Cells[rowIndex, 0].StringValue))
            {
                string chinese = ws.Cells[rowIndex, 0].StringValue;
                string english = string.IsNullOrEmpty(ws.Cells[rowIndex, 1].StringValue) ? "" : ws.Cells[rowIndex, 1].StringValue;
                rowIndex++;

                if (!import_list.ContainsKey(chinese))
                    import_list.Add(chinese, english);
            }

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;
                if (!string.IsNullOrEmpty(row.Cells[Chinese.Name].ErrorText)) continue;

                string chinese = row.Cells[Chinese.Name].Value.ToString();
                if (import_list.ContainsKey(chinese))
                {
                    row.Cells[English.Name].Value = import_list[chinese];
                    import_list.Remove(chinese);
                }
            }

            foreach (string key in import_list.Keys)
            {
                gridIndex = dataGridViewX1.Rows.Add();
                DataGridViewRow row = dataGridViewX1.Rows[gridIndex];
                row.Cells[Chinese.Name].Value = key;
                row.Cells[English.Name].Value = import_list[key];
            }
        }

        private void DeptChineseToEnglishForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isSave)
            {
                if (MsgBox.Show("資料尚未儲存，您確定要離開？", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
        }
    }
}
