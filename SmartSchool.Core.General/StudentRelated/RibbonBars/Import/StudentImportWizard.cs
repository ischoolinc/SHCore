﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using SmartSchool.Feature.Student;
using System.Xml;
using SmartSchool.StudentRelated.RibbonBars.Import.BulkModel;
using System.IO;
using Aspose.Cells;
using SmartSchool.StudentRelated.RibbonBars.Import.SheetModel;
using ProgramProcess = System.Diagnostics.Process;
using DocValidate;
using SmartSchool.StudentRelated.RibbonBars.Import.ValidateModel;
using FISCA.DSAUtil;
using SmartSchool.Common;
using SmartSchool.Feature.Class;
using SmartSchool.ApplicationLog;
using SmartSchool.ImportSupport.Validators;
using SmartSchool.Feature;
using DevComponents.DotNetBar.Rendering;
using SmartSchool.ExceptionHandler;
using System.Xml.Linq;
using FISCA.Data;

namespace SmartSchool.StudentRelated.RibbonBars.Import
{
    public partial class StudentImportWizard : BaseForm
    {
        private WizardContext _context;

        public StudentImportWizard()
        {
            InitializeComponent();

            #region 設定Wizard會跟著Style跑
            //this.wizard1.FooterStyle.ApplyStyle(( GlobalManager.Renderer as Office2007Renderer ).ColorTable.GetClass(ElementStyleClassKeys.RibbonFileMenuBottomContainerKey));
            this.ImportWizard.HeaderStyle.ApplyStyle((GlobalManager.Renderer as Office2007Renderer).ColorTable.GetClass(ElementStyleClassKeys.RibbonFileMenuBottomContainerKey));
            this.ImportWizard.FooterStyle.BackColorGradientAngle = -90;
            this.ImportWizard.FooterStyle.BackColorGradientType = eGradientType.Linear;
            this.ImportWizard.FooterStyle.BackColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.Start;
            this.ImportWizard.FooterStyle.BackColor2 = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.End;
            this.ImportWizard.BackColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TopBackground.Start;
            this.ImportWizard.BackgroundImage = null;
            for (int i = 0; i < 5; i++)
            {
                (this.ImportWizard.Controls[1].Controls[i] as ButtonX).ColorTable = eButtonColor.OrangeWithBackground;
            }
            (this.ImportWizard.Controls[0].Controls[1] as System.Windows.Forms.Label).ForeColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.MouseOver.TitleText;
            (this.ImportWizard.Controls[0].Controls[2] as System.Windows.Forms.Label).ForeColor = (GlobalManager.Renderer as Office2007Renderer).ColorTable.RibbonBar.Default.TitleText;
            this.ImportWizard.FooterStyle.BackgroundImage = null;
            #endregion

            _context = new WizardContext();
        }

        private WizardContext Context
        {
            get { return _context; }
        }

        #region Select File and Action Page
        private void lblInserDesc_Click(object sender, EventArgs e)
        {
            Context.ImportMode = ImportMode.Insert;
            chkInsert.Checked = true;
        }

        private void lblUpdateDesc_Click(object sender, EventArgs e)
        {
            Context.ImportMode = ImportMode.Update;
            chkUpdate.Checked = true;
        }

        private void chkInsert_CheckedChanged(object sender, EventArgs e)
        {
            Context.ImportMode = ImportMode.Insert;
        }

        private void chkUpdate_CheckedChanged(object sender, EventArgs e)
        {
            Context.ImportMode = ImportMode.Update;
        }

        private void txtSourceFile_TextChanged(object sender, EventArgs e)
        {
            Context.SourceFileName = txtSourceFile.Text;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            DialogResult dr = SelectSourceFileDialog.ShowDialog();

            if (dr == DialogResult.OK)
            {
                txtSourceFile.Text = SelectSourceFileDialog.FileName;
            }
        }

        private void wpSelectFileAndAction_NextButtonClick(object sender, CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;

                Context.SourceFileName = txtSourceFile.Text;
                if (string.IsNullOrEmpty(Context.SourceFileName))
                {
                    MsgBox.Show("您必須選擇匯入來源檔案。");
                    return;
                }

                if (!File.Exists(Context.SourceFileName))
                {
                    MsgBox.Show("您指定的來源檔案並不存在。");
                    return;
                }

                if (Context.ImportMode == ImportMode.None)
                {
                    MsgBox.Show("您必須決定一種匯入方式。");
                    return;
                }

                lblCollectMsg.Text = "讀取匯入規格描述資訊…";
                pProgram.Visible = true;
                pUser.Visible = false;
                Application.DoEvents();

                //XmlElement bulk = StudentBulkProcess.GetBulkDescription();
                //讀取XML欄位描述
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Properties.Resources.SH_S_BulkDescription);
                XmlElement bulk = doc.DocumentElement;
                Context.BulkDescription = new BulkDescription(bulk);

                Context.RefreshImportSource();
                Context.IdentifyField = string.Empty;
                Context.ShiftCheckField = string.Empty;

                if (Context.ImportMode == ImportMode.Insert)
                {
                    if (!Context.AcceptColumns.ContainsKey("姓名"))
                    {
                        MsgBox.Show("來源資料中缺少必要欄位「姓名」。", Application.ProductName);
                        return;
                    }
                }

                e.Cancel = false;
            }
            catch (Exception ex)
            {
                MsgBox.Show(ex.Message);
            }
            finally
            {
                pProgram.Visible = false;
                pUser.Visible = true;
            }
        }

        #endregion

        #region Collect Key Info Page
        private void wpCollectKeyInfo_BeforePageDisplayed(object sender, WizardCancelPageChangeEventArgs e)
        {
            if (e.PageChangeSource == eWizardPageChangeSource.NextButton)
            {
                if (Context.ImportMode == ImportMode.Insert)
                    e.NewPage = wpSelectField;
                else
                    e.NewPage = wpCollectKeyInfo;
            }
        }

        private void wpCollectKeyInfo_AfterPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            if (e.PageChangeSource == eWizardPageChangeSource.BackButton)
                return;

            cboIdField.SelectedItem = null;
            cboIdField.Items.Clear();
            cboIdField.DisplayMember = "DisplayText";

            cboValidateField.SelectedItem = null;
            cboValidateField.Items.Clear();
            cboValidateField.DisplayMember = "DisplayText";
            cboValidateField.Items.Add(Context.EmptyShiftCheckField);
            cboValidateField.SelectedIndex = 0;

            foreach (BulkColumn each in Context.AcceptColumns.Values)
            {
                if (each.Identifiable)
                    cboIdField.Items.Add(each);

                if (each.ShiftCheckable)
                    cboValidateField.Items.Add(each);
            }

            //檢查是否有提供識別欄位。
            if (cboIdField.Items.Count <= 0)
            {
                MsgBox.Show("您提供的資料來源中沒有任何可當作識別欄的欄位\n必須提供識別欄位才可使用更新匯入。");
                wpCollectKeyInfo.NextButtonEnabled = eWizardButtonState.False;
            }
            else
                wpCollectKeyInfo.NextButtonEnabled = eWizardButtonState.True;
        }

        private void wpCollectKeyInfo_NextButtonClick(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            if (cboIdField.SelectedIndex == -1)
            {
                MsgBox.Show("您必須要選擇識別欄位。");
                return;
            }

            BulkColumn column = cboIdField.SelectedItem as BulkColumn;
            if (column != null)
                Context.IdentifyField = column.DisplayText;

            column = cboValidateField.SelectedItem as BulkColumn;
            if (column != Context.EmptyShiftCheckField)
                Context.ShiftCheckField = column.DisplayText;

            if (Context.IdentifyField == Context.ShiftCheckField)
            {
                MsgBox.Show("「識別欄」與「驗證欄」必須是不同的欄位。");
                return;
            }

            e.Cancel = false;
        }
        #endregion

        #region Select Field Page
        private void wpSelectField_AfterPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            if (e.PageChangeSource == eWizardPageChangeSource.BackButton)
                return;

            Dictionary<string, ImportItem> fields = new Dictionary<string, ImportItem>();
            GroupSheetColumn(fields);

            Dictionary<string, BulkColumnCollection> bfields = new Dictionary<string, BulkColumnCollection>();
            GroupBulkColumn(bfields);

            //將欄位顯示於 ListView 中。
            DisplayColumns(fields, bfields);

            //檢查唯讀欄位，此類型欄位不可匯入。
            CheckReadOnlyField();

            //如果是 Insert 時，檢查必要欄位，此類型欄一定要選擇。
            if (Context.ImportMode == ImportMode.Insert)
                CheckRequiredField();
        }

        private void DisplayColumns(Dictionary<string, ImportItem> fields, Dictionary<string, BulkColumnCollection> bfields)
        {
            lvSourceFieldList.Items.Clear();

            //需遮蔽的欄位
            List<string> avoids = new List<string>(new string[] { "帳號類型", "登入密碼" });

            foreach (ImportItem each in fields.Values)
            {
                bool hide_column = false;

                if (each.IsGroupColumn)
                {
                    if (bfields.ContainsKey(each.Text))
                        each.CheckAccept(bfields[each.Text]);
                    else
                    { //不在群組欄位中時。
                        hide_column = true;
                    }
                }
                else
                {//非群欄位時。
                    if (avoids.Contains(each.Text))
                    {//遮蔽欄位
                        hide_column = true;
                    }
                    else if (!Context.AcceptColumns.ContainsKey(each.Text))
                    { //不在 AcceptColumns 中時。
                        each.Enabled = false;
                        each.ToolTipText = "系統不提供此欄位匯入。";
                    }
                    else if (each.Text == Context.IdentifyField)
                    {//「識別欄」不可以匯入。
                        each.Enabled = false;
                        each.ToolTipText = "識別欄不可以當作匯入欄位。";
                    }
                    else if (each.Text == Context.ShiftCheckField)
                    {//「驗證欄」不可以匯入。
                        each.Enabled = false;
                        each.ToolTipText = "驗證欄不可以當作匯入欄位。";
                    }
                    else
                        each.Enabled = true;
                }

                if (!hide_column)
                    lvSourceFieldList.Items.Add(each);
            }
        }

        private void CheckReadOnlyField()
        {
            foreach (ImportItem each in lvSourceFieldList.Items)
            {
                foreach (SheetColumn column in each.SheetColumns.Values)
                {
                    if (column.BindingBulkColumn != null && column.BindingBulkColumn.ReadOnly)
                    {
                        each.Enabled = false;
                        each.ToolTipText = "唯讀欄位不允許匯入。";
                    }
                }
            }
        }

        private void CheckRequiredField()
        {
            foreach (ImportItem each in lvSourceFieldList.Items)
            {
                foreach (SheetColumn column in each.SheetColumns.Values)
                {
                    if (column.BindingBulkColumn != null && column.BindingBulkColumn.IsRequired)
                    {
                        each.Checked = true;
                        each.Locked = true;
                        each.ToolTipText = "此欄位是必要欄位。";
                    }
                }
            }
        }

        private void GroupBulkColumn(Dictionary<string, BulkColumnCollection> bfields)
        {
            foreach (BulkColumn each in Context.BulkDescription.Columns.Values)
            {
                BulkColumnCollection item = null;

                if (bfields.ContainsKey(each.GroupName))
                    item = bfields[each.GroupName];
                else
                {
                    item = new BulkColumnCollection();
                    bfields.Add(each.GroupName, item);
                }

                item.Add(each.FullDisplayText, each);
            }
        }

        private void GroupSheetColumn(Dictionary<string, ImportItem> fields)
        {
            foreach (SheetColumn each in Context.SourceColumns.Values)
            {
                ImportItem item = null;
                if (fields.ContainsKey(each.GroupName))
                    item = fields[each.GroupName];
                else
                {
                    item = new ImportItem(each.GroupName);
                    fields.Add(each.GroupName, item);
                }

                item.AddSheetColumn(each);
            }
        }

        private void wpSelectField_NextButtonClick(object sender, CancelEventArgs e)
        {
            Context.SelectedFields = new SheetColumnCollection();
            foreach (ImportItem each in lvSourceFieldList.Items)
            {
                if (each.Checked)
                {
                    foreach (SheetColumn column in each.SheetColumns.Values)
                    {
                        Context.SelectedFields.Add(column.Name, column);
                        column.SetStyle(Context.TipStyle.Header);
                    }
                }
            }

            if (Context.SelectedFields.Count <= 0)
            {
                MsgBox.Show("您必須選擇要匯入的欄位。選擇欄位時，請在名稱前的方格中打勾。");
                e.Cancel = true;
            }
        }

        private void lvSourceFieldList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ImportItem item = lvSourceFieldList.Items[e.Index] as ImportItem;

            if (item != null)
            {
                if (item.Enabled == false)
                    e.NewValue = CheckState.Unchecked;

                if (item.Locked)
                    e.NewValue = e.CurrentValue;
            }
        }

        private void chkSelectAll_CheckedChanged(object sender, System.EventArgs e)
        {
            foreach (ImportItem each in lvSourceFieldList.Items)
            {
                if (each.Enabled == true)
                    each.Checked = chkSelectAll.Checked;
            }
        }

        private class ImportItem : ListViewItem
        {
            private SheetColumnCollection _columns;
            private bool _is_group_column;
            private bool _enabled;
            private bool _locked;

            public ImportItem(string displayName)
            {
                Text = displayName;
                Enabled = false;

                _columns = new SheetColumnCollection();
            }

            public void AddSheetColumn(SheetColumn column)
            {
                if (column.UsedValid)
                    Checked = true;

                _columns.Add(column.Name, column);

                if (_columns.Count > 1)
                    _is_group_column = true;
            }

            public bool IsGroupColumn
            {
                get { return _is_group_column; }
            }

            public void CheckAccept(BulkColumnCollection basis)
            {
                StringBuilder msg = new StringBuilder();
                bool success = true;

                msg.AppendLine("要匯入此欄位，必須下列欄位同時存在：");
                foreach (BulkColumn each in basis.Values)
                {
                    if (!SheetColumns.ContainsKey(each.FullDisplayText))
                    {
                        msg.AppendLine(each.FullDisplayText);
                        success = false;
                    }
                }

                if (success == false)
                {
                    Enabled = false;
                    ToolTipText = msg.ToString();
                }
                else
                    Enabled = true;
            }

            public bool Enabled
            {
                get { return _enabled; }
                set
                {
                    _enabled = value;

                    if (_enabled == false)
                    {
                        ForeColor = Color.Silver;
                        Checked = false; //設成 False 時，Checked 一定是 Unchecked;
                    }
                    else
                        ForeColor = Color.Black;
                }
            }

            public bool Locked
            {
                get { return _locked; }
                set
                {
                    _locked = value;

                    if (_locked)
                        ForeColor = Color.Blue;
                    else
                        ForeColor = Color.Black;
                }
            }

            public SheetColumnCollection SheetColumns
            {
                get { return _columns; }
            }
        }
        #endregion

        #region Validation Page
        private bool _cancel_validate;
        private int _correct_count = 0, _warning_count = 0, _error_count = 0;
        private RowMessage _current_row_message;

        private void lnkCancelValid_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MsgBox.Show("確定要取消資料檢查？", Application.ProductName, MessageBoxButtons.YesNo) == DialogResult.Yes)
                _cancel_validate = true;
        }

        private void wizardPage2_AfterPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            Context.ValidateComplete = false;
            //chkContinue.Checked = false;

            if (e.PageChangeSource == eWizardPageChangeSource.BackButton)
                return;

            pgValidProgress.Value = 0;

            //chkContinue.Visible = false;
            //lblContinueMsg.Visible = false;
            lnkCancelValid.Visible = false;
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            try
            {
                bool sepErrors = chkSepErrors.Checked;

                //搜集使用者選擇要匯入的欄位，建立成 ValidateColumn 集合。
                ValidateColumnCollection validColumns = CreateValidateColumns();

                ProgressMessage("載入資料檢查規則…");
                DocumentValidate validator = new DocumentValidate();
                validator.FieldValidatorList.AddValidatorFactory(new FieldValidatorFactory());
                validator.AutoCorrect += new DocumentValidate.AutoCorrectEventHandler(Validator_AutoCorrect);
                validator.ErrorCaptured += new DocumentValidate.ErrorCapturedEventHandler(Validator_ErrorCaptured);
                //讀取XML驗證規則
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Properties.Resources.SH_S_FieldValidationRule);

                foreach(XmlElement elem in doc.SelectNodes("//FieldValidator"))
                {
                    if(elem.GetAttribute("Name") == "合法班級")
                    {
                        QueryHelper Q = new QueryHelper();
                        DataTable dt = Q.Select("SELECT class_name FROM class WHERE status=1");
                        foreach(DataRow row in dt.Rows)
                        {
                            XmlElement item = doc.CreateElement("Item");
                            item.SetAttribute("Value", row["class_name"].ToString());
                            elem.AppendChild(item);
                        }
                        break;
                    }
                }
                
                XmlElement validRule = doc.DocumentElement;
                //XmlElement validRule = StudentBulkProcess.GetFieldValidationRule();
                //validator.RowValidatorList.AddValidatorFactory(new RowValidatorFactory(Context.IdentifyField));
                validator.InitFromXMLNode(validRule);

                ProgressMessage("載入識別欄驗證資訊…");

                // 舊寫法有問題先註解，Service 只會回傳一般生
                //XmlElement primaryKeys = StudentBulkProcess.GetPrimaryKeyList();
                ImportRecordCollection importRecords = new ImportRecordCollection();
                //foreach (XmlElement each in primaryKeys.SelectNodes("Item"))
                //    importRecords.Add(new ImportRecord(each));

                // 讀取所有學生資料
                List<SHSchool.Data.SHStudentRecord> tempStudRecList = SHSchool.Data.SHStudent.SelectAll();
                // 檢查是否重複登入
                Dictionary<string, bool> checkIDKeyDict = new Dictionary<string, bool>();
                foreach (SHSchool.Data.SHStudentRecord studRec in tempStudRecList)
                    if(!checkIDKeyDict.ContainsKey(studRec.ID))
                    {
                        try
                        {
                            checkIDKeyDict.Add(studRec.ID, true);
                            importRecords.Add(new ImportRecord(studRec));

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\n\r " + "學生：" + studRec.Name + " 在資料庫中有重複的key值，請檢查登入帳號、學號、身分證號是否有與其他人重複。","驗證提示");
                        }

                    }                    


                ShiftCheckList checkList = null;
                bool isShiftCheck = false;
                if (Context.ImportMode == ImportMode.Update && !string.IsNullOrEmpty(Context.ShiftCheckField))
                {
                    ProgressMessage("載入驗證欄檢查資訊…");
                    BulkColumn key = Context.BulkDescription.Columns[Context.IdentifyField];
                    BulkColumn shift = Context.BulkDescription.Columns[Context.ShiftCheckField];
                    checkList = new ShiftCheckList(key, shift);
                    checkList.LoadCheckList();
                    isShiftCheck = true;
                }

                ProgressMessage("初始化資料來源…");
                Context.RefreshImportSource();
                SheetRowSource rowSource = new SheetRowSource(Context.SourceReader, validColumns);

                ProgressMessage("初始化錯誤輸出程序…");
                Dictionary<int, RowMessage> rowMessages = new Dictionary<int, RowMessage>();
                CommentOutput msgOutput = new CommentOutput(Context.SourceReader, Context.TipStyle);
                CorrectSheetOutput msgCorrect = null;
                msgCorrect = new CorrectSheetOutput(Context.SourceBook, Context.SourceReader, Context.TipStyle, validColumns);

                InitValidStartScreen();

                ProgressMessage("開始檢查資料…");
                rowSource.Reset();
                while (rowSource.NextRow())
                {
                    _current_row_message = rowSource.CreateRowMessage();

                    //int t1 = Environment.TickCount;
                    validator.ValidateRow(rowSource);
                    //Console.WriteLine(Environment.TickCount - t1);

                    //檢查「驗證欄資料」。
                    CheckShiftColumn(checkList, isShiftCheck, rowSource);

                    rowMessages.Add(_current_row_message.RowIndex, _current_row_message);

                    if (Context.SourceReader.RelativelyIndex % 10 == 0)
                        RefreshValidProgress(Context.SourceReader.RelativelyIndex);

                    if (_cancel_validate)
                        return;
                }
                lnkCancelValid.Visible = false;

                //更新進度 Bar。
                RefreshValidProgress(Context.SourceReader.RelativelyIndex);

                ProgressMessage("檢查識別欄正確性資料…");
                ValidateKey(importRecords, rowMessages);

                //輸出錯誤資訊。
                ProgressMessage("輸出錯誤資訊…");
                pgValidProgress.Minimum = 0;
                pgValidProgress.Maximum = rowMessages.Count;
                pgValidProgress.Value = 0;
                foreach (RowMessage each in rowMessages.Values)
                {
                    if (each.HasMessage)
                    {
                        Context.SourceReader.MoveTo(each.RowIndex);

                        msgOutput.Output(each);

                        if (sepErrors) msgCorrect.Output(each);
                    }

                    if (each.RowIndex % 50 == 0)
                        RefreshValidProgress(each.RowIndex);
                }
                RefreshValidProgress(rowMessages.Count);

                msgCorrect.OutputComplete();

                RefreshCorrectCount();
                RefreshWarningCount();
                RefreshErrorCount();

                Context.ValidateComplete = (_error_count <= 0);

                if (Context.ValidateComplete)
                    ProgressMessage("資料檢查完成，沒有檢查出任何錯誤。");
                else
                    ProgressMessage("資料檢查完成，但有發現資料不正確。");

                validator.AutoCorrect -= new DocumentValidate.AutoCorrectEventHandler(Validator_AutoCorrect);
                validator.ErrorCaptured -= new DocumentValidate.ErrorCapturedEventHandler(Validator_ErrorCaptured);

                Context.SourceBook.Save(Context.SourceFileName);
            }
            catch (Exception ex)
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);

                MsgBox.Show(ex.Message);
                Context.ValidateComplete = false;
            }
            finally
            {
                btnValidate.Enabled = true;
                btnViewResult.Enabled = true;
                chkSepErrors.Enabled = true;
            }
        }

        private void CheckShiftColumn(ShiftCheckList checkList, bool isShiftCheck, SheetRowSource rowSource)
        {
            if (isShiftCheck)
            {
                string key = rowSource.GetFieldData(Context.IdentifyField);

                if (checkList.ContainKey(key))
                {
                    string srcValue = rowSource.GetFieldData(Context.ShiftCheckField);
                    string dbValue = checkList.GetCheckValue(key);

                    if (srcValue.Trim() != dbValue.Trim())
                    {
                        string message = "";
                        if (string.IsNullOrEmpty(dbValue))
                            message = "資料庫中此資料為空值，與此欄不符，請檢查。";
                        else
                            message = "資料庫中值為「" + dbValue + "」，與此欄不符，請檢查。";

                        _current_row_message.ReportMessage(Context.ShiftCheckField, MessageType.Error,
                            message);

                        _error_count++;
                    }
                }
            }
        }

        private void ValidateKey(ImportRecordCollection importRecords, Dictionary<int, RowMessage> rowMessages)
        {
            string id = "學生系統編號", snum = "學號", ssn = "身分證號", login = "登入帳號";

            SheetReader reader = Context.SourceReader;
            reader.Reset();
            if (Context.ImportMode == ImportMode.Insert)
            {
                while (reader.MoveNext())
                {
                    if (Context.SelectedFields.ContainsKey(snum))
                    {
                        if (importRecords.ContainStudentNumber(reader.GetValue(snum)))
                        {
                            rowMessages[reader.RelativelyIndex].ReportMessage(snum, MessageType.Error,
                                "此資料已存在於資料庫中，無法重複新增。");

                            _error_count++;
                        }
                    }

                    if (Context.SelectedFields.ContainsKey(ssn))
                    {
                        if (importRecords.ContainIdNumber(reader.GetValue(ssn)))
                        {
                            rowMessages[reader.RelativelyIndex].ReportMessage(ssn, MessageType.Error,
                                "此資料已存在於資料庫中，無法重複新增。");

                            _error_count++;
                        }
                    }

                    if (Context.SelectedFields.ContainsKey(login))
                    {
                        if (importRecords.ContainSALoginName(reader.GetValue(login).ToLower()))
                        {
                            rowMessages[reader.RelativelyIndex].ReportMessage(login, MessageType.Error,
                                "此資料已存在於資料庫中，無法重複新增。");

                            _error_count++;
                        }
                    }
                }
            }
            else
            {
                while (reader.MoveNext())
                {
                    if (Context.IdentifyField == id)
                    {
                        if (!importRecords.ContainIdentity(reader.GetValue(id)))
                        {
                            rowMessages[reader.RelativelyIndex].ReportMessage(id, MessageType.Error,
                                "此資料並不存在於資料庫中，無法更新此筆資料。");
                            _error_count++;
                        }
                    }
                    else if (Context.IdentifyField == snum)
                    {
                        if (!importRecords.ContainStudentNumber(reader.GetValue(snum)))
                        {
                            rowMessages[reader.RelativelyIndex].ReportMessage(snum, MessageType.Error,
                                "此資料學生狀態非一般，無法更新此筆資料，請使用學生系統編號更新。");
                            _error_count++;
                        }
                    }
                    else if (Context.IdentifyField == ssn)
                    {
                        if (!importRecords.ContainIdNumber(reader.GetValue(ssn)))
                        {
                            rowMessages[reader.RelativelyIndex].ReportMessage(ssn, MessageType.Error,
                                "此資料學生狀態非一般，無法更新此筆資料，請使用學生系統編號更新。");
                            _error_count++;
                        }
                    }
                }
                                
                if (_error_count <= 0) //當所有資料都正確時，才進行此檢查。
                {
                    ProgressMessage("檢查資料與識別欄正確性…");
                    reader.Reset(); //深度檢查…
                    while (reader.MoveNext())
                    {
                        ImportRecord record = null;
                        string key = reader.GetValue(Context.IdentifyField);

                        if (Context.IdentifyField == id && (importRecords.ContainIdentity(key)))
                            record = importRecords.GetByIdentity(key);
                        else if (Context.IdentifyField == snum && (importRecords.ContainStudentNumber(key)))
                            record = importRecords.GetByStudentNumber(key);
                        else if (Context.IdentifyField == ssn && (importRecords.ContainIdNumber(key)))
                            record = importRecords.GetByIdNumber(key);

                        if (record == null)
                            continue;

                        //更新回 Object Model，以便判斷資料是否重覆。(StudentNumber)
                        if (Context.SelectedFields.ContainsKey(snum))
                            record.StudentNumber = reader.GetValue(snum);

                        //更新回 Object Model，以便判斷資料是否重覆。(IDNumber)
                        if (Context.SelectedFields.ContainsKey(ssn))
                            record.IDNumber = reader.GetValue(ssn);

                        //更新回 Object Model，以便判斷資料是否重覆。(SALoginName)
                        if (Context.SelectedFields.ContainsKey(login))
                            record.SALoginName = reader.GetValue(login);

                        record.AbsoluteRowIndex = reader.AbsoluteIndex;
                        record.RelativelyRowIndex = reader.RelativelyIndex;
                        
                    }

                    // 是否用學生系統編號驗證
                    bool chkID = false;

                    if (Context.IdentifyField == id)
                        chkID = true;

                    // 檢查身分證號
                    if (Context.SelectedFields.ContainsKey(ssn))
                    {
                        List<ImportRecord> dupSSN = importRecords.GetDuplicateIdNumberList(chkID);
                        foreach (ImportRecord each in dupSSN)
                        {
                            if (each.RelativelyRowIndex != -1)
                            {
                                rowMessages[each.RelativelyRowIndex].ReportMessage(ssn, MessageType.Error, "更新資料後，會造成此欄位資料重複(可能是與資料庫中資料重覆)。");
                                _error_count++;
                            }
                        }
                    }

                    // 檢查學號
                    if (Context.SelectedFields.ContainsKey(snum))
                    {
                        List<ImportRecord> dupSNum = importRecords.GetDuplicateStudentNumberList(chkID);
                        foreach (ImportRecord each in dupSNum)
                        {
                            if (each.RelativelyRowIndex != -1)
                            {
                                rowMessages[each.RelativelyRowIndex].ReportMessage(snum, MessageType.Error, "更新資料後，會造成此欄位資料重複(可能是與資料庫中資料重覆)。");
                                _error_count++;
                            }
                        }
                    }
                    
                    // 檢查登入帳號重複
                    if (Context.SelectedFields.ContainsKey(login))
                    {
                        List<ImportRecord> dupLogin = importRecords.GetDuplicateSALoginNameList(chkID);
                        foreach (ImportRecord each in dupLogin)
                        {
                            if (each.RelativelyRowIndex != -1)
                            {
                                rowMessages[each.RelativelyRowIndex].ReportMessage(login, MessageType.Error, "更新資料後，會造成此欄位資料重複(可能是與資料庫中資料重覆)。");
                                _error_count++;
                            }
                        }
                    }
                }
               

            }
        }

        private void ProgressMessage(string msg)
        {
            lblValidMsg.Text = msg;
            Application.DoEvents();
        }

        private void InitValidStartScreen()
        {
            _correct_count = 0;
            RefreshCorrectCount();
            _warning_count = 0;
            RefreshWarningCount();
            _error_count = 0;
            RefreshErrorCount();
            _cancel_validate = false;
            pgValidProgress.Minimum = 0;
            pgValidProgress.Maximum = Context.SourceReader.RowCount;
            pgValidProgress.Value = 0;
            lnkCancelValid.Visible = true;
            btnValidate.Enabled = false;
            btnViewResult.Enabled = false;
            chkSepErrors.Enabled = false;
            Application.DoEvents();
        }

        private ValidateColumnCollection CreateValidateColumns()
        {
            ValidateColumnCollection validColumns = new ValidateColumnCollection();

            if (Context.ImportMode == ImportMode.Update)
            {
                validColumns.Add(Context.IdentifyField,
                    new ValidateColumn(Context.SourceColumns[Context.IdentifyField], (byte)validColumns.Count));

                if (!string.IsNullOrEmpty(Context.ShiftCheckField))
                {
                    validColumns.Add(Context.ShiftCheckField,
                        new ValidateColumn(Context.SourceColumns[Context.ShiftCheckField], (byte)validColumns.Count));
                }
            }

            foreach (SheetColumn each in Context.SelectedFields.Values)
                validColumns.Add(each.Name, new ValidateColumn(each, (byte)validColumns.Count));

            Context.ValidateColumns = validColumns;
            return validColumns;
        }

        private void RefreshValidProgress(int value)
        {
            pgValidProgress.Value = value;
        }

        private void Validator_ErrorCaptured(string FieldName, string ErrorType, string Description, IRowSource RowSource)
        {
            if (FieldName == Context.ShiftCheckField) //如果是驗證欄，就等於不檢查。
                return;

            if (ErrorType.ToUpper() == "WARNING")
            {
                _warning_count++;
                _current_row_message.ReportMessage(FieldName, MessageType.Warning, Description);
            }
            else if (ErrorType.ToUpper() == "ERROR")
            {
                _error_count++;
                _current_row_message.ReportMessage(FieldName, MessageType.Error, Description);
            }

            if (_error_count % 50 == 0)
                RefreshErrorCount();

            if (_warning_count % 50 == 0)
                RefreshWarningCount();
        }

        private void Validator_AutoCorrect(string FieldName, string OldValue, string NewValue, IRowSource RowSource)
        {
            if (FieldName == Context.ShiftCheckField) //如果是驗證欄，就等於不檢查。
                return;

            _correct_count++;

            _current_row_message.ReportMessage(FieldName, MessageType.Correct, "資料由「" + OldValue + "」更正為「" + NewValue + "」。");

            SheetRowSource source = RowSource as SheetRowSource;
            if (source != null)
                source.SetStringValue(FieldName, NewValue);

            if (_correct_count % 50 == 0)
                RefreshCorrectCount();
        }

        private void RefreshErrorCount()
        {
            lblErrorCount.Text = _error_count.ToString();
            Application.DoEvents();
        }

        private void RefreshWarningCount()
        {
            lblWarningCount.Text = _warning_count.ToString();
            Application.DoEvents();
        }

        private void RefreshCorrectCount()
        {
            lblCorrectCount.Text = _correct_count.ToString();
            Application.DoEvents();
        }

        private void btnViewResult_Click(object sender, EventArgs e)
        {
            try
            {
                ProgramProcess.Start(Context.SourceFileName);
            }
            catch (Exception ex)
            {
                MsgBox.Show(ex.Message);
            }
        }

        private void lblContinueMsg_Click(object sender, EventArgs e)
        {
            //chkContinue.Checked = true;
        }

        private void chkContinue_CheckedChanged(object sender, EventArgs e)
        {
            if (chkContinue.Checked == true)
                Context.ValidateComplete = true;
            else
                Context.ValidateComplete = false;
        }

        private void wizardPage2_NextButtonClick(object sender, CancelEventArgs e)
        {
            if (!Context.ValidateComplete)
            {
                e.Cancel = true;
                MsgBox.Show("請再次進行資料檢查，請確認資料完全正確再進行匯入動作。");
                return;
            }
        }
        #endregion

        #region Import Page
        private void wpImport_AfterPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            pgImport.Value = 0;
        }

        private void lnkCancelImport_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                XmlElement output = DSXmlHelper.LoadXml("<ImportStudentStudent/>");

                ImportMessage("產生匯入資料…");
                BulkColumnCollection columns = Context.BulkDescription.Columns.GetColumnList(Context.ValidateColumns.GetNames());

                Context.RefreshImportSource();
                Context.SourceBook.CalculateFormula(); //更新畫面上的公式資料。
                SheetReader reader = Context.SourceReader;
                reader.ConvertFormulaToValue();

                BulkDescription bulkdesc = Context.BulkDescription;

                pgImport.Minimum = 0;
                pgImport.Maximum = reader.RowCount;
                pgImport.Value = 0;
                btnImport.Enabled = false;

                reader.Reset();
                while (reader.MoveNext())
                {
                    XmlElement record = output.OwnerDocument.CreateElement("Student");

                    if (Context.ImportMode == ImportMode.Insert)
                        bulkdesc.GenerateInsertRequest(Context.SourceReader, columns, record);
                    else
                        bulkdesc.GenerateUpdateRequest(Context.SourceReader, columns, record, Context.IdentifyField, Context.ShiftCheckField);

                    output.AppendChild(record);

                    if (reader.RelativelyIndex % 10 == 0)
                        pgImport.Value = reader.RelativelyIndex;
                }
                pgImport.Value = reader.RelativelyIndex;

                //處理班級編號。
                ProcessClassLookup(output);

                //處理畢業證書字號。
                ProcessGraduateInfo(output);

                //output.OwnerDocument.Save(Context.SourceFileName + ".xml");

                //return;

                //計算密碼雜~!
                if (Context.SelectedFields.ContainsKey("登入密碼"))
                    HashPassword(output);

                ImportMessage("上傳資料到主機，請稍後…");
                GeneralActionLog log = new GeneralActionLog();
                log.Source = "匯入學生基本資料";
                log.Diagnostics = output.OuterXml;

                if (Context.ImportMode == ImportMode.Insert)
                {
                    StudentBulkProcess.InsertImportStudent(output);
                    log.ActionName = "新增匯入";
                    log.Description = "新增匯入 " + reader.RowCount + " 筆學生資料。";
                }
                else
                {
                    // 因為高中原本 Service 有問題，只能更新學生狀態 status=1，所以改用國中
                    //StudentBulkProcess.UpdateImportStudent(output);

                    StudentBulkProcess.UpdateImportStudentJH(output);
                    log.ActionName = "更新匯入";
                    log.Description = "更新匯入 " + reader.RowCount + " 筆學生資料。";
                }
                //int t1 = Environment.TickCount;

                //if (Context.SelectedFields.ContainsKey("新生:異動代號") &&
                //    Context.SelectedFields.ContainsKey("新生:異動日期") &&
                //    Context.SelectedFields.ContainsKey("新生:國中校名") &&
                //    Context.SelectedFields.ContainsKey("新生:國中縣市"))
                //    StudentBulkProcess.SyncEnrollmentInfoToUpdateRecord();

                //Console.WriteLine("SyncEnrollmentInfoToUpdateRecord:" + Environment.TickCount - t1);

                CurrentUser.Instance.AppLog.Write(log);

                ImportMessage("匯入完成。");
                wpImport.FinishButtonEnabled = eWizardButtonState.True;
                wpImport.BackButtonEnabled = eWizardButtonState.False;

                SmartSchool.StudentRelated.Student stu = SmartSchool.StudentRelated.Student.Instance;
                stu.ReloadData();
            }
            catch (Exception ex)
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);

                btnImport.Enabled = true;

                ImportMessage("上傳資料失敗");
                

                XElement errElm = XElement.Parse((ex as DSAServerException).WarpedError.Response);

                if (errElm != null)
                {
                    string msg = errElm.Element("Body").Element("CompleteErrorDetails").LastNode.ToString();
                    FISCA.Presentation.Controls.MsgBox.Show(msg);
                }
                else
                    FISCA.Presentation.Controls.MsgBox.Show(ex.Message);




                //MsgBox.Show(ex.Message);
            }
        }

        private void ProcessClassLookup(XmlElement output)
        {
            if (Context.SelectedFields.ContainsKey("班級"))
            {
                Dictionary<string, string> classes = GetClassCollection();
                foreach (XmlElement each in output.SelectNodes("Student"))
                {
                    string className = each.SelectSingleNode("ClassName").InnerText.Trim();

                    XmlNode elmClassId = each.OwnerDocument.CreateElement("ClassID");
                    elmClassId = each.AppendChild(elmClassId);

                    if (classes.ContainsKey(className))
                        elmClassId.InnerText = classes[className];
                }
            }
        }

        private void ProcessGraduateInfo(XmlElement output)
        {
            string gradefn = "畢結業證書字號";
            if (Context.SelectedFields.ContainsKey(gradefn))
            {
                if (Context.ImportMode == ImportMode.Update)
                {
                    DiplomaNumberLookup dnlook = new DiplomaNumberLookup(Context.IdentifyField);
                    foreach (XmlElement each in output.SelectNodes("Student"))
                    {
                        string identity = each.SelectSingleNode("Condition/" + dnlook.ExportName).InnerText.Trim();
                        XmlElement diplomaDB = dnlook.GetDiplomaElement(identity);
                        XmlElement diplomaNew = each.SelectSingleNode("DiplomaNumberRaw") as XmlElement;
                        if (diplomaDB == null)
                        {
                            XmlDocument xDoc = new XmlDocument();
                            diplomaDB = xDoc.CreateElement("DiplomaNumber");
                        }
                        if (diplomaDB.SelectSingleNode("DiplomaNumber") == null)
                        {
                            XmlElement dnNew = each.OwnerDocument.CreateElement("DiplomaNumber");
                            dnNew.InnerText = diplomaNew.InnerText;
                            dnNew = diplomaDB.OwnerDocument.ImportNode(dnNew, true) as XmlElement;
                            diplomaDB.AppendChild(dnNew);
                        }
                        else
                        {
                            XmlElement dnOld = diplomaDB.SelectSingleNode("DiplomaNumber") as XmlElement;
                            dnOld.InnerText = diplomaNew.InnerText;
                        }
                        diplomaDB = each.OwnerDocument.ImportNode(diplomaDB, true) as XmlElement;
                        each.ReplaceChild(diplomaDB, diplomaNew);
                    }
                }
            }
        }

        private void HashPassword(XmlElement output)
        {
            foreach (XmlElement each in output.SelectNodes("Student"))
            {
                if (!string.IsNullOrEmpty(each.SelectSingleNode("Password").InnerText.Trim()))
                    each.SelectSingleNode("Password").InnerText = PasswordHash.Compute(each.SelectSingleNode("Password").InnerText);
            }
        }

        private Dictionary<string, string> GetClassCollection()
        {
            Dictionary<string, string> classes = new Dictionary<string, string>();
            DSXmlHelper rsp = QueryClass.GetClassList().GetContent();

            foreach (XmlElement each in rsp.GetElements("Class"))
            {
                string className = each.SelectSingleNode("ClassName").InnerText.Trim();
                string classId = each.GetAttribute("ID");

                classes.Add(className, classId);
            }

            return classes;
        }

        private void ImportMessage(string msg)
        {
            lblImportProgress.Text = msg;
            Application.DoEvents();
        }
        #endregion

        private void ImportWizard_CancelButtonClick(object sender, CancelEventArgs e)
        {
            if (Context.AllowExit)
                Close();
        }

        private void ImportWizard_FinishButtonClick(object sender, CancelEventArgs e)
        {
            Close();
        }

        private class DiplomaNumberLookup
        {
            private Dictionary<string, XmlElement> _diploma_list;

            public DiplomaNumberLookup(string idFieldName)
            {
                _diploma_list = new Dictionary<string, XmlElement>();

                if (idFieldName == "學生系統編號")
                {
                    _id_fieldName = "ID";
                    _export_name = "StudentID";
                }
                else if (idFieldName == "學號")
                {
                    _id_fieldName = "StudentNumber";
                    _export_name = "StudentNumber";
                }
                else if (idFieldName == "身分證號")
                {
                    _id_fieldName = "IDNumber";
                    _export_name = "IDNumber";
                }

                string[] fields = new string[] { _id_fieldName, "DiplomaNumber","Status" };
                DSXmlHelper alldiploma = QueryStudent.GetDetailList(fields).GetContent();

                foreach (XmlElement each in alldiploma.GetElements("Student"))
                {
                    DSXmlHelper hlpeach = new DSXmlHelper(each);
                    string key = hlpeach.GetText(getFieldName());

                    if (hlpeach.GetText("Status") != "一般") continue;

                    if (string.IsNullOrEmpty(key.Trim())) continue;

                    _diploma_list.Add(key, each.SelectSingleNode("DiplomaNumber") as XmlElement);
                }
            }

            private string getFieldName()
            {
                if (_id_fieldName == "ID")
                    return "@ID";
                else
                    return _id_fieldName;
            }

            public XmlElement GetDiplomaElement(string identity)
            {
                if (_diploma_list.ContainsKey(identity))
                    return _diploma_list[identity];
                else
                    return null;
            }

            private string _id_fieldName;

            private string _export_name;
            public string ExportName
            {
                get { return _export_name; }
            }

        }

        private class FieldValidatorFactory : IFieldValidatorFactory
        {

            #region IFieldValidatorFactory 成員

            public IFieldValidator CreateFieldValidator(string TypeName)
            {
                if (TypeName.ToUpper() == "MixDateCSharp".ToUpper())
                {
                    return new MixDateFieldValidator();
                }
                else
                    return null;
            }

            #endregion
        }
    }
}