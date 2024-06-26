using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using FISCA.DSAUtil;
using SmartSchool.Feature.Basic;
using System.Xml;
using Aspose.Cells;

namespace SmartSchool.Others.Configuration.DisciplineMapping
{
    public partial class DisciplineForm : BaseForm
    {
        private int _currentComboxIndex = 2;

        private Dictionary<string, string> _origMerit = new Dictionary<string, string>();
        private Dictionary<string, string> _origDemerit = new Dictionary<string, string>();
        private Dictionary<string, string> _meritList = new Dictionary<string, string>();
        private Dictionary<string, string> _demeritList = new Dictionary<string, string>();

        private bool _meritIsSave = true;
        private bool _demeritIsSave = true;
        private bool _meritCanSave = true;
        private bool _demeritCanSave = true;

        private bool _isInititaled = false;

        public DisciplineForm()
        {
            InitializeComponent();
            InitialList();
        }

        private void InitialList()
        {
            _isInititaled = false;

            DSResponse dsrsp = Config.GetDisciplineReasonList();
            foreach (XmlElement var in dsrsp.GetContent().GetElements("Reason"))
            {
                string type = var.GetAttribute("Type");
                string code = var.GetAttribute("Code");
                string desc = var.GetAttribute("Description");

                if (type == "獎勵")
                {
                    if (!_origMerit.ContainsKey(code))
                    {
                        _origMerit.Add(code, desc);
                        _meritList.Add(code, desc);
                    }
                }
                else if (type == "懲戒" || type == "懲誡")
                {
                    if (!_origDemerit.ContainsKey(code))
                    {
                        _origDemerit.Add(code, desc);
                        _demeritList.Add(code, desc);
                    }
                }
            }

            comboBoxEx1.Items.Add("獎勵事由代碼表");
            comboBoxEx1.Items.Add("懲戒事由代碼表");

            comboBoxEx1.SelectedIndex = 0;
            //ValidateList();

            _isInititaled = true;
        }

        private void FillGrid(Dictionary<string, string> list)
        {
            dataGridViewX1.Rows.Clear();
            foreach (string code in list.Keys)
            {
                int index = dataGridViewX1.Rows.Add();
                DataGridViewRow row = dataGridViewX1.Rows[index];
                row.Cells[Code.Name].Value = code;
                row.Cells[Reason.Name].Value = list[code];
            }
        }

        private void SaveCurrentList(int index)
        {
            if (!_isInititaled)
                return;

            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;
                string code = "" + row.Cells[Code.Name].Value;
                string reason = (row.Cells[Reason.Name].Value != null) ? row.Cells[Reason.Name].Value.ToString() : "";

                if (!string.IsNullOrEmpty(code))
                {
                    if (!list.ContainsKey(code))
                        list.Add(code, reason);
                }
            }

            if (index == 0)
                _meritList = list;
            else
                _demeritList = list;
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInititaled && _currentComboxIndex == comboBoxEx1.SelectedIndex) return;

            if (comboBoxEx1.SelectedIndex == 0)
            {
                SaveCurrentList(1);
                FillGrid(_meritList);
            }
            else if (comboBoxEx1.SelectedIndex == 1)
            {
                SaveCurrentList(0);
                FillGrid(_demeritList);
            }
            ValidateList();

            _currentComboxIndex = comboBoxEx1.SelectedIndex;
        }

        private bool ValidateList()
        {
            dataGridViewX1.EndEdit();
            bool valid = true;
            bool isSave = true;

            List<string> codeList = new List<string>();
            Dictionary<string, string> origList;

            if (comboBoxEx1.SelectedIndex == 0)
                origList = _origMerit;
            else
                origList = _origDemerit;

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells[Code.Name].Value == null)
                {
                    row.Cells[Code.Name].ErrorText = "不能為空白";
                    valid = false;
                    break;
                }
                else
                    row.Cells[Code.Name].ErrorText = "";

                string codeValue = row.Cells[Code.Name].Value.ToString();

                if (!codeList.Contains(codeValue))
                {
                    codeList.Add(codeValue);
                    row.Cells[Code.Name].ErrorText = "";
                }
                else
                {
                    row.Cells[Code.Name].ErrorText = "名稱重複";
                    valid = false;
                    break;
                }

                //檢查資料是否變動
                if (isSave)
                {
                    if (origList.ContainsKey(codeValue))
                    {
                        if (origList[codeValue] != ((row.Cells[Reason.Name].Value != null) ? row.Cells[Reason.Name].Value.ToString() : ""))
                            isSave = false;
                    }
                    else
                        isSave = false;
                }
            }

            if (isSave)
            {
                if ((dataGridViewX1.Rows.Count - 1) != origList.Keys.Count)
                    isSave = false;
            }

            if (comboBoxEx1.SelectedIndex == 0)
            {
                _meritIsSave = isSave;
                _meritCanSave = valid;
            }
            else
            {
                _demeritIsSave = isSave;
                _demeritCanSave = valid;
            }

            return valid;
        }

        private void dataGridViewX1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ValidateList();
        }

        #region Log
        private StringBuilder _log_desc = new StringBuilder("");

        private void GenerateLogDescription()
        {
            StringBuilder merit = new StringBuilder("");
            StringBuilder demerit = new StringBuilder("");

            foreach (string code in _meritList.Keys)
                if (!_origMerit.ContainsKey(code))
                    merit.AppendLine("新增「" + code + "：" + _meritList[code] + "」");
            foreach (string code in _origMerit.Keys)
                if (_meritList.ContainsKey(code) && _origMerit[code] != _meritList[code])
                    merit.AppendLine("代碼「" + code + "」的事由由「" + _origMerit[code] + "」變更為「" + _meritList[code] + "」");
            foreach (string code in _origMerit.Keys)
                if (!_meritList.ContainsKey(code))
                    merit.AppendLine("刪除「" + code + "：" + _origMerit[code] + "」");

            foreach (string code in _demeritList.Keys)
                if (!_origDemerit.ContainsKey(code))
                    demerit.AppendLine("新增「" + code + "：" + _demeritList[code] + "」");
            foreach (string code in _origDemerit.Keys)
                if (_demeritList.ContainsKey(code) && _origDemerit[code] != _demeritList[code])
                    demerit.AppendLine("代碼「" + code + "」的事由由「" + _origDemerit[code] + "」變更為「" + _demeritList[code] + "」");
            foreach (string code in _origDemerit.Keys)
                if (!_demeritList.ContainsKey(code))
                    demerit.AppendLine("刪除「" + code + "：" + _origDemerit[code] + "」");

            if (!string.IsNullOrEmpty(merit.ToString()))
            {
                _log_desc.AppendLine("獎勵事由表：");
                _log_desc.Append(merit.ToString());
            }
            if (!string.IsNullOrEmpty(demerit.ToString()))
            {
                if (!string.IsNullOrEmpty(_log_desc.ToString())) _log_desc.AppendLine("");
                _log_desc.AppendLine("懲戒事由表：");
                _log_desc.Append(demerit.ToString());
            }
        }

        private void WriteLog()
        {
            if (!string.IsNullOrEmpty(_log_desc.ToString()))
                CurrentUser.Instance.AppLog.Write("修改獎懲事由表", _log_desc.ToString(), "獎懲事由表", "");
        }
        #endregion

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_meritCanSave && _demeritCanSave)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement content = doc.CreateElement("Content");

                SaveCurrentList(comboBoxEx1.SelectedIndex);
                GenerateLogDescription();

                _origMerit.Clear();
                _origDemerit.Clear();

                foreach (string code in _meritList.Keys)
                {
                    XmlElement reason = doc.CreateElement("Reason");
                    reason.SetAttribute("Code", code);
                    reason.SetAttribute("Description", _meritList[code]);
                    reason.SetAttribute("Type", "獎勵");
                    content.AppendChild(reason);
                    _origMerit.Add(code, _meritList[code]);
                }

                foreach (string code in _demeritList.Keys)
                {
                    XmlElement reason = doc.CreateElement("Reason");
                    reason.SetAttribute("Code", code);
                    reason.SetAttribute("Description", _demeritList[code]);
                    reason.SetAttribute("Type", "懲戒");
                    content.AppendChild(reason);
                    _origDemerit.Add(code, _demeritList[code]);
                }

                try
                {
                    Config.SetDisciplineReasonList(content);
                    WriteLog();
                    _meritIsSave = true;
                    _demeritIsSave = true;

                    MsgBox.Show("儲存成功。");
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    CurrentUser.ReportError(ex);
                    MsgBox.Show("儲存失敗。\n" + ex.Message);
                    return;
                }
            }
            else
            {
                if (!_meritCanSave && !_demeritCanSave)
                    MsgBox.Show("資料有誤。");
                else if (!_meritCanSave)
                    MsgBox.Show("獎勵事由表資料有誤。");
                else if (!_demeritCanSave)
                    MsgBox.Show("懲戒事由表資料有誤。");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            Workbook wb = new Workbook();
            wb.Worksheets.Clear();
            Worksheet ws = wb.Worksheets[wb.Worksheets.Add()];
            ws.Name = "獎懲事由代碼表";

            ws.Cells.CreateRange(0, 1, true).ColumnWidth = 10;
            ws.Cells.CreateRange(1, 1, true).ColumnWidth = 8;
            ws.Cells.CreateRange(2, 1, true).ColumnWidth = 40;

            ws.Cells[0, 0].PutValue("事由代碼");
            ws.Cells[0, 1].PutValue("類別");
            ws.Cells[0, 2].PutValue("事由");

            int rowIndex = 1;

            foreach (string code in _meritList.Keys)
            {
                ws.Cells[rowIndex, 0].PutValue(code);
                ws.Cells[rowIndex, 1].PutValue("獎勵");
                ws.Cells[rowIndex, 2].PutValue(_meritList[code]);
                rowIndex++;
            }

            foreach (string code in _demeritList.Keys)
            {
                ws.Cells[rowIndex, 0].PutValue(code);
                ws.Cells[rowIndex, 1].PutValue("懲戒");
                ws.Cells[rowIndex, 2].PutValue(_demeritList[code]);
                rowIndex++;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "獎懲事由代碼表.xls";
            sfd.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    wb.Save(sfd.FileName, FileFormatType.Excel2003);
                    MsgBox.Show("匯出完成。");
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "另存檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            Workbook wb = new Workbook();
            Dictionary<string, string> importMeritList = new Dictionary<string, string>();
            Dictionary<string, string> importDemeritList = new Dictionary<string, string>();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "選擇要匯入的獎懲事由代碼表";
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

            if (wb.Worksheets[0].Cells[0, 0].StringValue != "事由代碼" ||
                wb.Worksheets[0].Cells[0, 1].StringValue != "類別" ||
                wb.Worksheets[0].Cells[0, 2].StringValue != "事由")
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
                    string type = ws.Cells[rowIndex, 1].StringValue;
                    string reason = ws.Cells[rowIndex, 2].StringValue;

                    rowIndex++;

                    if (!importMeritList.ContainsKey(code) && !importDemeritList.ContainsKey(code))
                    {
                        if (type == "獎勵")
                            importMeritList.Add(code, reason);
                        else if (type == "懲戒" || type == "懲誡")
                            importDemeritList.Add(code, reason);
                    }
                    else
                    {
                        if (type == "獎勵")
                        {
                            importMeritList[code] = reason;
                            if (importDemeritList.ContainsKey(code))
                                importDemeritList.Remove(code);
                        }
                        else if (type == "懲戒" || type == "懲誡")
                        {
                            importDemeritList[code] = reason;
                            if (importMeritList.ContainsKey(code))
                                importMeritList.Remove(code);
                        }
                    }
                }

                if (form.Overwrite)
                {
                    _meritList = importMeritList;
                    _demeritList = importDemeritList;
                    if (comboBoxEx1.SelectedIndex == 0)
                        FillGrid(_meritList);
                    else
                        FillGrid(_demeritList);
                }
                else
                {
                    foreach (string code in importMeritList.Keys)
                    {
                        if (!_meritList.ContainsKey(code) && !_demeritList.ContainsKey(code))
                            _meritList.Add(code, importMeritList[code]);
                        else if (_meritList.ContainsKey(code) && !_demeritList.ContainsKey(code))
                            _meritList[code] = importMeritList[code];
                        else if (_demeritList.ContainsKey(code) && !_meritList.ContainsKey(code))
                        {
                            _demeritList.Remove(code);
                            _meritList.Add(code, importMeritList[code]);
                        }
                        else
                        {
                            _demeritList.Remove(code);
                            _meritList[code] = importMeritList[code];
                        }
                    }
                    foreach (string code in importDemeritList.Keys)
                    {
                        if (!_demeritList.ContainsKey(code) && !_meritList.ContainsKey(code))
                            _demeritList.Add(code, importDemeritList[code]);
                        else if (_demeritList.ContainsKey(code) && !_meritList.ContainsKey(code))
                            _demeritList[code] = importDemeritList[code];
                        else if (_meritList.ContainsKey(code) && !_demeritList.ContainsKey(code))
                        {
                            _meritList.Remove(code);
                            _demeritList.Add(code, importDemeritList[code]);
                        }
                        else
                        {
                            _meritList.Remove(code);
                            _demeritList[code] = importDemeritList[code];
                        }
                    }

                    if (comboBoxEx1.SelectedIndex == 0)
                        FillGrid(_meritList);
                    else
                        FillGrid(_demeritList);
                }

                MsgBox.Show("匯入完成。");
            }
        }

        private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            ValidateList();
        }

        private void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void DisciplineForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_demeritIsSave && !_meritIsSave)
            {
                if (MsgBox.Show("資料尚未儲存，您確定要離開？", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
            else if (!_meritIsSave)
            {
                if (MsgBox.Show("獎勵事由表資料尚未儲存，您確定要離開？", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
            else if (!_demeritIsSave)
            {
                if (MsgBox.Show("懲戒事由表資料尚未儲存，您確定要離開？", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
        }
    }
}