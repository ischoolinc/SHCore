using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.Feature;
using SmartSchool.Security;
using FISCA.Presentation;

namespace SmartSchool.StudentRelated.Process.StudentIUD
{
    public partial class StudentIDUProcess : UserControl, IProcess, IPalmerwormManager
    {
        FeatureAccessControl addCtrl;
        FeatureAccessControl saveCtrl;
        FeatureAccessControl delCtrl;

        static private StudentIDUProcess _Instance;
        static public StudentIDUProcess Instance
        {
            get
            {
                if ( _Instance == null ) _Instance = new StudentIDUProcess();
                return _Instance;
            }
        }

        private StudentIDUProcess()
        {
            //InitializeComponent();

            //superTooltip1.DefaultFont = FontStyles.General;

            ////buttonItem14.Enabled = CurrentUser.Instance.HasPermission(typeof(AddStudent));

            ////Student.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);
            //SmartSchool.Broadcaster.Events.Items["學生/選取變更"].Handler += delegate
            //{
            //    btnDeleteStudent.Enabled = (Student.Instance.SelectionStudents.Count == 1);
            //    delCtrl.Inspect(btnDeleteStudent);
            //};
            ////權限判斷 - 新增修改刪除 學生

            //addCtrl.Inspect(btnAddStudent);
            //saveCtrl.Inspect(btnSaveStudent);
            //delCtrl.Inspect(btnDeleteStudent);
        }

        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StudentIDUProcess));

            addCtrl = new FeatureAccessControl("Button0010");
            saveCtrl = new FeatureAccessControl("Button0020");
            delCtrl = new FeatureAccessControl("Button0030");

            RibbonBarItem rbItem = K12.Presentation.NLDPanels.Student.RibbonBarItems["編輯"];
            rbItem.AutoOverflowEnabled = false;
            var btnAddClass = rbItem["新增"];
            var btnDeleteClass = rbItem["刪除"];
            btnAddClass.Size = RibbonBarButton.MenuButtonSize.Large;
            btnAddClass.Enable = addCtrl.Executable();
            btnAddClass.Image = ( (System.Drawing.Image)( resources.GetObject("btnAddStudent_Image") ) );
            btnAddClass.Click += new System.EventHandler(this.buttonItem14_Click);

            btnDeleteClass.Size = RibbonBarButton.MenuButtonSize.Large;
            btnDeleteClass.Image = ( (System.Drawing.Image)( resources.GetObject("btnDeleteStudent_Image") ) );
            btnDeleteClass.Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0 && delCtrl.Executable();
            btnDeleteClass.Click += new System.EventHandler(this.btnDelete_Click);

            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate { btnDeleteClass.Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0 && delCtrl.Executable(); };
        }

        //void Instance_SelectionChanged(object sender, EventArgs e)
        //{
        //    btnDeleteStudent.Enabled = (Student.Instance.SelectionStudents.Count == 1);

        //    delCtrl.Inspect(btnDeleteStudent);
        //}

        #region IProcess 成員

        public string ProcessTabName
        {
            get { return "學生"; }
        }

        public DevComponents.DotNetBar.RibbonBar ProcessRibbon
        {
            get { return ribbonBar1; }
        }


        private double _Level = 0;
        public double Level
        {
            get { return _Level; }
            set { _Level = value; }
        }
        #endregion

        private void buttonItem14_Click(object sender, EventArgs e)
        {
            InsertStudentWizard wizard = new InsertStudentWizard();
            if ( wizard.ShowDialog() == DialogResult.Yes )
            {
                //PopupPalmerwormStudent.ShowPopupPalmerwormStudent(wizard.NewStudentID);]
                K12.Presentation.NLDPanels.Student.PopupDetailPane(wizard.NewStudentID);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if ( MsgBox.Show("是否將 \"" + Student.Instance.SelectionStudents[0].Name + "\" 移至已刪除？", "", MessageBoxButtons.YesNo) == DialogResult.Yes )
            {
                try
                {
                    BriefStudentData stu = Student.Instance.SelectionStudents[0];
                    string stu_id = stu.ID;
                    string stu_student_number = stu.StudentNumber;                    
                    string stu_name = stu.Name;
                    List<string> checkIDNumberList = new List<string>();
                    List<string> chekcStudentNumList = new List<string>();
                    
                    // 取得將要變更的身分證號,學號List
                    foreach (BriefStudentData studRec in Student.Instance.Items)
                    {
                        if (studRec.Status=="刪除")
                        {
                            checkIDNumberList.Add(studRec.IDNumber);
                            chekcStudentNumList.Add(studRec.StudentNumber);
                        }
                    }

                    if (!string.IsNullOrEmpty(stu.IDNumber))
                    if (checkIDNumberList.Contains(stu.IDNumber))
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("在刪除狀態已有相同身分證號學生,請變更身分證號後再變更狀態.");
                        return;
                    }

                    if(!string.IsNullOrEmpty(stu_student_number ))
                    if (chekcStudentNumList.Contains(stu_student_number))
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("在刪除狀態已有相同學號學生,請變更學號後再變更狀態.");
                        return;                    
                    }

                    RemoveStudent.DeleteStudent(stu_id);

                    //Log
                    CurrentUser.Instance.AppLog.Write(
                        SmartSchool.ApplicationLog.EntityType.Student,
                        "刪除學生",
                        stu_id,
                        "學生「" + stu_name + ( string.IsNullOrEmpty(stu_student_number) ? "" : " (" + stu_student_number + ")" ) + "」已變更為刪除。",
                        "學生",
                        string.Format("學生姓名：{0}，學號：{1}", stu_name, stu_student_number));

                    //Student.Instance.InvokBriefDataChanged(Student.Instance.SelectionStudents[0].ID);
                    SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(stu_id);
                }
                catch ( Exception ex )
                {
                    CurrentUser.ReportError(ex);
                    MsgBox.Show("刪除學生失敗。\n" + ex.Message);
                }
            }
        }

        #region IPalmerwormManager 成員

        public bool EnableSave
        {
            get
            {
                return btnSaveStudent.Enabled;
            }
            set
            {
                btnSaveStudent.Enabled = value;
            }
        }

        public bool EnableCancel
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public event EventHandler Save;

        public event EventHandler Cacel;

        public event EventHandler Reflash;

        #endregion

        private void btnSave_Click(object sender, EventArgs e)
        {
            if ( Save != null )
                Save.Invoke(this, new EventArgs());
        }
    }
}
