using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using DevComponents.DotNetBar.Rendering;
using SmartSchool.Common;

namespace SmartSchool.ClassRelated.RibbonBars.Reports
{
    public partial class AbsenceNotificationConfigForm : BaseForm
    {
        private byte[] _buffer = null;
        private string base64 = null;
        private bool _isUpload = false;
        private bool _defaultTemplate;
        private bool _printHasRecordOnly;
        private DateRangeMode _mode = DateRangeMode.Month;

        public AbsenceNotificationConfigForm(bool defaultTemplate, bool printHasRecordOnly, DateRangeMode mode, byte[] buffer, string name, string address)
        {
            InitializeComponent();
            #region 如果系統的Renderer是Office2007Renderer，同化_ClassTeacherView,_CategoryView的顏色
            if (GlobalManager.Renderer is Office2007Renderer)
            {
                ((Office2007Renderer)GlobalManager.Renderer).ColorTableChanged += new EventHandler(ScoreCalcRuleEditor_ColorTableChanged);
                SetForeColor(this);
            }
            #endregion
            _defaultTemplate = defaultTemplate;
            _printHasRecordOnly = printHasRecordOnly;
            _mode = mode;

            if (buffer != null)
                _buffer = buffer;

            if (defaultTemplate)
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;

            checkBoxX1.Checked = printHasRecordOnly;

            switch (mode)
            {
                case DateRangeMode.Month:
                    radioButton3.Checked = true;
                    break;
                case DateRangeMode.Week:
                    radioButton4.Checked = true;
                    break;
                case DateRangeMode.Custom:
                    radioButton5.Checked = true;
                    break;
                default:
                    throw new Exception("Date Range Mode Error.");
            }

            comboBoxEx1.SelectedIndex = 0;
            comboBoxEx2.SelectedIndex = 0;

            foreach (DevComponents.Editors.ComboItem var in comboBoxEx1.Items)
            {
                if (var.Text == name)
                {
                    comboBoxEx1.SelectedIndex = comboBoxEx1.Items.IndexOf(var);
                    break;
                }
            }

            foreach (DevComponents.Editors.ComboItem var in comboBoxEx2.Items)
            {
                if (var.Text == address)
                {
                    comboBoxEx2.SelectedIndex = comboBoxEx2.Items.IndexOf(var);
                    break;
                }
            }
        }

        void ScoreCalcRuleEditor_ColorTableChanged(object sender, EventArgs e)
        {
            SetForeColor(this);
        }

        private void SetForeColor(Control parent)
        {
            foreach (Control var in parent.Controls)
            {
                if (var is RadioButton)
                    var.ForeColor = ((Office2007Renderer)GlobalManager.Renderer).ColorTable.CheckBoxItem.Default.Text;
                SetForeColor(var);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
                _defaultTemplate = true;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
                _defaultTemplate = false;
            }
        }

        private void checkBoxX1_CheckedChanged(object sender, EventArgs e)
        {
            _printHasRecordOnly = checkBoxX1.Checked;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "缺曠通知單.doc";
            sfd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    fs.Write(Properties.Resources.缺曠通知單, 0, Properties.Resources.缺曠通知單.Length);
                    fs.Close();
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "另存檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "另存新檔";
            sfd.FileName = "自訂缺曠通知單.doc";
            sfd.Filter = "Word檔案 (*.doc)|*.doc|所有檔案 (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    if (Aspose.Words.Document.DetectFileFormat(new MemoryStream(_buffer)) == Aspose.Words.LoadFormat.Doc)
                        fs.Write(_buffer, 0, _buffer.Length);
                    else
                        fs.Write(Properties.Resources.缺曠通知單, 0, Properties.Resources.缺曠通知單.Length);
                    fs.Close();
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "另存檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "選擇自訂的缺曠通知單範本";
            ofd.Filter = "Word檔案 (*.doc)|*.doc";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (Aspose.Words.Document.DetectFileFormat(ofd.FileName) == Aspose.Words.LoadFormat.Doc)
                    {
                        FileStream fs = new FileStream(ofd.FileName, FileMode.Open);

                        byte[] tempBuffer = new byte[fs.Length];
                        fs.Read(tempBuffer, 0, tempBuffer.Length);
                        base64 = Convert.ToBase64String(tempBuffer);
                        _isUpload = true;
                        fs.Close();
                        MsgBox.Show("上傳成功。");
                    }
                    else
                        MsgBox.Show("上傳檔案格式不符");
                }
                catch
                {
                    MsgBox.Show("指定路徑無法存取。", "開啟檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            #region 儲存 Preference

            XmlElement config = CurrentUser.Instance.Preference["缺曠通知單"];

            if (config == null)
            {
                config = new XmlDocument().CreateElement("缺曠通知單");
            }

            config.SetAttribute("Default", _defaultTemplate.ToString());

            XmlElement printSetup = config.OwnerDocument.CreateElement("PrintHasRecordOnly");
            XmlElement customize = config.OwnerDocument.CreateElement("CustomizeTemplate");
            XmlElement mode = config.OwnerDocument.CreateElement("DateRangeMode");
            XmlElement receive = config.OwnerDocument.CreateElement("Receive");

            printSetup.SetAttribute("Checked", _printHasRecordOnly.ToString());
            config.ReplaceChild(printSetup, config.SelectSingleNode("PrintHasRecordOnly"));

            if (_isUpload)
            {
                customize.InnerText = base64;
                config.ReplaceChild(customize, config.SelectSingleNode("CustomizeTemplate"));
            }

            mode.InnerText = ((int)_mode).ToString();
            config.ReplaceChild(mode, config.SelectSingleNode("DateRangeMode"));


            receive.SetAttribute("Name", ((DevComponents.Editors.ComboItem)comboBoxEx1.SelectedItem).Text);
            receive.SetAttribute("Address", ((DevComponents.Editors.ComboItem)comboBoxEx2.SelectedItem).Text);
            if (config.SelectSingleNode("Receive") == null)
                config.AppendChild(receive);
            else
                config.ReplaceChild(receive, config.SelectSingleNode("Receive"));

            CurrentUser.Instance.Preference["缺曠通知單"] = config;

            #endregion

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                radioButton4.Checked = false;
                radioButton5.Checked = false;
                _mode = DateRangeMode.Month;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                radioButton3.Checked = false;
                radioButton5.Checked = false;
                _mode = DateRangeMode.Week;
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                radioButton3.Checked = false;
                radioButton4.Checked = false;
                _mode = DateRangeMode.Custom;
            }
        }
    }
}