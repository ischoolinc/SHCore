using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.Feature.Basic;
using FISCA.DSAUtil;
using Aspose.Cells;
using System.IO;

namespace SmartSchool.Others.Configuration.WordCommentMapping
{
    public partial class WordCommentForm : BaseForm
    {
        private Dictionary<string, string> _origList = new Dictionary<string, string>();
        private bool _isSave = true;
        private int _SelectedRowIndex;

        public WordCommentForm()
        {
            InitializeComponent();
            InitialList();
        }

        private void InitialList()
        {
            DSResponse dsrsp = Config.GetWordCommentList();
            foreach (XmlElement var in dsrsp.GetContent().GetElements("Morality"))
            {
                int index = dataGridViewX1.Rows.Add();
                DataGridViewRow row = dataGridViewX1.Rows[index];
                row.Cells[Code.Name].Value = var.GetAttribute("Code");
                row.Cells[WordComment.Name].Value = var.GetAttribute("WordComment");
                _origList.Add(var.GetAttribute("Code"), var.GetAttribute("WordComment"));
            }
        }

        private bool ValidateList()
        {
            dataGridViewX1.EndEdit();
            bool valid = true;
            _isSave = true;
            List<string> codeList = new List<string>();

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;
                row.Cells[Code.Index].ErrorText = "";
                if (row.Cells[Code.Index].Value == null)
                {
                    row.Cells[Code.Index].ErrorText = "不能為空白";
                    valid = false;
                    break;
                }

                string codeValue = row.Cells[Code.Name].Value.ToString();

                if (!codeList.Contains(codeValue))
                    codeList.Add(codeValue);
                else
                {
                    row.Cells[Code.Name].ErrorText = "名稱重複";
                    valid = false;
                    break;
                }

                //檢查資料是否變動
                if (_isSave)
                {
                    if (_origList.ContainsKey(codeValue))
                    {
                        if (_origList[codeValue] != ((row.Cells[WordComment.Name].Value != null) ? row.Cells[WordComment.Name].Value.ToString() : ""))
                            _isSave = false;
                    }
                    else
                        _isSave = false;
                }
            }

            if (_isSave)
            {
                if (dataGridViewX1.Rows.Count - 1 != _origList.Keys.Count)
                    _isSave = false;
            }

            return valid;
        }

        private bool Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement content = doc.CreateElement("Content");

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;
                XmlElement morality = doc.CreateElement("Morality");
                morality.SetAttribute("Code", row.Cells[Code.Name].Value.ToString());
                morality.SetAttribute("WordComment", (row.Cells[WordComment.Name].Value != null) ? row.Cells[WordComment.Name].Value.ToString() : "");
                content.AppendChild(morality);

                if (!_origList.ContainsKey(row.Cells[Code.Name].Value.ToString()))
                    _origList.Add(row.Cells[Code.Name].Value.ToString(), (row.Cells[WordComment.Name].Value != null) ? row.Cells[WordComment.Name].Value.ToString() : "");
            }

            try
            {
                Config.SetWordCommentList(content);
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
            {
                MsgBox.Show("儲存成功。");
                this.DialogResult = DialogResult.OK;
            }
            else
                MsgBox.Show("儲存失敗。");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Workbook wb = new Workbook();
            Dictionary<string, string> importCodeList = new Dictionary<string, string>();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "選擇要匯入的文字評量代碼表";
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

            if (wb.Worksheets[0].Cells[0, 0].StringValue != "代碼" || wb.Worksheets[0].Cells[0, 1].StringValue != "文字評量")
            {
                MsgBox.Show("匯入格式不符合。");
                return;
            }

            ImportConfirm form = new ImportConfirm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                int rowIndex = 1;
                Worksheet ws = wb.Worksheets[0];

                while (!string.IsNullOrEmpty(ws.Cells[rowIndex, 0].StringValue))
                {
                    string code = ws.Cells[rowIndex, 0].StringValue;
                    string comment = ws.Cells[rowIndex, 1].StringValue;

                    if (!importCodeList.ContainsKey(code))
                        importCodeList.Add(code, comment);
                    else
                        importCodeList[code] = comment;
                    rowIndex++;
                }

                if (form.Overwrite)
                {
                    dataGridViewX1.Rows.Clear();
                    foreach (string key in importCodeList.Keys)
                    {
                        int index = dataGridViewX1.Rows.Add();
                        DataGridViewRow row = dataGridViewX1.Rows[index];
                        row.Cells[Code.Name].Value = key;
                        row.Cells[WordComment.Name].Value = importCodeList[key];
                    }
                }
                else
                {
                    Dictionary<string, int> OriginalCodeListIndex = new Dictionary<string, int>();
                    List<int> delete = new List<int>();

                    foreach (DataGridViewRow row in dataGridViewX1.Rows)
                    {
                        if (row.IsNewRow) continue;
                        if (row.Cells[Code.Name].Value != null)
                        {
                            string code = row.Cells[Code.Name].Value.ToString();
                            if (!OriginalCodeListIndex.ContainsKey(code))
                                OriginalCodeListIndex.Add(code, row.Index);
                            else
                            {
                                delete.Add(OriginalCodeListIndex[code]);
                                OriginalCodeListIndex[code] = row.Index;
                            }
                        }
                    }

                    foreach (string key in importCodeList.Keys)
                    {
                        if (OriginalCodeListIndex.ContainsKey(key))
                            dataGridViewX1.Rows[OriginalCodeListIndex[key]].Cells[WordComment.Name].Value = importCodeList[key];
                        else
                        {
                            int index = dataGridViewX1.Rows.Add();
                            DataGridViewRow row = dataGridViewX1.Rows[index];
                            row.Cells[Code.Name].Value = key;
                            row.Cells[WordComment.Name].Value = importCodeList[key];
                        }
                    }

                    foreach (int var in delete)
                    {
                        dataGridViewX1.Rows.RemoveAt(var);
                    }
                }

                //MsgBox.Show("匯入成功。\n注意：系統尚未將資料儲存，請您選擇『確定』後，至評語代碼表中確認匯入資料後選取『儲存』。");
                ValidateList();
                MsgBox.Show("匯入完成");
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (!ValidateList())
                return;

            Workbook wb = new Workbook();
            wb.Worksheets.Clear();
            Worksheet ws = wb.Worksheets[wb.Worksheets.Add()];
            ws.Name = "文字評量代碼表";

            ws.Cells.CreateRange(0, 1, true).ColumnWidth = 10;
            ws.Cells.CreateRange(1, 1, true).ColumnWidth = 40;

            ws.Cells[0, 0].PutValue("代碼");
            ws.Cells[0, 1].PutValue("文字評量");

            int rowIndex = 1;

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;

                ws.Cells[rowIndex, 0].PutValue(row.Cells[Code.Name].Value.ToString());
                ws.Cells[rowIndex, 1].PutValue((row.Cells[WordComment.Name].Value != null) ? row.Cells[WordComment.Name].Value.ToString() : "");
                rowIndex++;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "文字評量代碼表.xls";
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

        private void WordCommentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isSave)
            {
                if (MsgBox.Show("資料尚未儲存，您確定要離開？", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
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

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            dataGridViewX1.Rows.Insert(_SelectedRowIndex, new DataGridViewRow());
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (_SelectedRowIndex >= 0 && dataGridViewX1.Rows.Count - 1 > _SelectedRowIndex)
                dataGridViewX1.Rows.RemoveAt(_SelectedRowIndex);
        }

        private void dataGridViewX1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex < 0 && e.Button == MouseButtons.Right)
            {
                dataGridViewX1.EndEdit();
                _SelectedRowIndex = e.RowIndex;
                foreach (DataGridViewRow var in dataGridViewX1.SelectedRows)
                {
                    if (var.Index != _SelectedRowIndex)
                        var.Selected = false;
                }
                dataGridViewX1.Rows[_SelectedRowIndex].Selected = true;
                contextMenuStrip1.Show(dataGridViewX1, dataGridViewX1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location);
            }
        }
    }
}