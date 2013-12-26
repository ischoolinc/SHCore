using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.CourseRelated;
using SmartSchool.CourseRelated.Forms;
using SmartSchool.Feature.Course;
using FISCA.DSAUtil;
using SmartSchool.Security;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class Manage : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase, IPalmerwormManager
    {
        //權限判斷
        FeatureAccessControl addCtrl;
        FeatureAccessControl saveCtrl;
        FeatureAccessControl delCtrl;

        static private Manage _Instance;
        static public Manage Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Manage();
                return _Instance;
            }
        }
        private Manage()
        {
            //InitializeComponent();

            //Course.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);

            ////權限判斷
            //addCtrl = new FeatureAccessControl("Button0520");
            //saveCtrl = new FeatureAccessControl("Button0530");
            //delCtrl = new FeatureAccessControl("Button0540");

            //addCtrl.Inspect(btnAddCourse);
            //saveCtrl.Inspect(btnSaveCourse);
            //delCtrl.Inspect(btnDeleteCourse);
        }

        internal void Setup()
        {

            //權限判斷
            addCtrl = new FeatureAccessControl("Button0520");
            delCtrl = new FeatureAccessControl("Button0540");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Manage));
            var Bar = K12.Presentation.NLDPanels.Course.RibbonBarItems["編輯"];
            Bar.AutoOverflowEnabled = false;
            var btnAdd = Bar["新增"];
            btnAdd.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnAdd.Image = (System.Drawing.Image)Properties.Resources.btnAddCourse_image;
            //btnAdd.Image = ( (System.Drawing.Image)( resources.GetObject("btnAddCourse.Image") ) );
            btnAdd.Click += new System.EventHandler(btnAdd_Click);
            btnAdd.Enable = addCtrl.Executable();
            var btnDelete = Bar["刪除"];
            btnDelete.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnDelete.Image = (System.Drawing.Image)Properties.Resources.btnremoveCourse_image;
            //btnDelete.Image = ( (System.Drawing.Image)( resources.GetObject("btnAddCourse.Image") ) );
            btnDelete.Click += new System.EventHandler(btnDeleteCourse_Click);
            btnDelete.Enable = ( K12.Presentation.NLDPanels.Course.SelectedSource.Count == 1 ) && delCtrl.Executable();
            K12.Presentation.NLDPanels.Course.SelectedSourceChanged += delegate
            {
                btnDelete.Enable = ( K12.Presentation.NLDPanels.Course.SelectedSource.Count == 1 ) && delCtrl.Executable();
            };
        }

        void Instance_SelectionChanged(object sender, EventArgs e)
        {
            btnDeleteCourse.Enabled = (Course.Instance.SelectionCourse.Count == 1);
            
            delCtrl.Inspect(btnDeleteCourse);
        }

        #region IPalmerwormManager 成員

        public bool EnableSave
        {
            get
            {
                return btnSaveCourse.Enabled;
            }
            set
            {
                btnSaveCourse.Enabled = value;

                saveCtrl.Inspect(btnSaveCourse);
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            InsertCourse wizard = new InsertCourse();
            if (wizard.ShowDialog() == DialogResult.Yes)
            {
                K12.Presentation.NLDPanels.Course.PopupDetailPane(wizard.NewCourseID);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (Save != null)
                Save.Invoke(this, new EventArgs());
        }

        private void btnDeleteCourse_Click(object sender, EventArgs e)
        {
            if (Course.Instance.SelectionCourse.Count <= 0) return;

            int deleteCourseID = Course.Instance.SelectionCourse[0].Identity;
            int attendCount = Course.Instance.FindSCAttendByCourseID(deleteCourseID);

            if (attendCount > 0)
            {
                MsgBox.Show("該課程目前修課學生人數為 " + attendCount + " 人，若要刪除課程請先移除修課學生。");
                return;
            }

            if (MsgBox.Show("您確定將刪除此課程？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    DeleteCourseHideAttend(deleteCourseID);

                    CourseDeleteEventArgs args = new CourseDeleteEventArgs(deleteCourseID);
                    string course_name = Course.Instance.SelectionCourse[0].CourseName;
                    RemoveCourse.DeleteCourse(deleteCourseID.ToString());
                    Course.Instance.InvokeAfterCourseDelete(deleteCourseID);

                    //Log
                    CurrentUser.Instance.AppLog.Write(SmartSchool.ApplicationLog.EntityType.Course, "刪除課程", deleteCourseID.ToString(), string.Format("課程「{0}」已刪除", course_name), "課程", deleteCourseID.ToString());
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }

            //btnDeleteCourse.Enabled = false;
        }

        private static void DeleteCourseHideAttend(int deleteCourseID)
        {
            DSXmlHelper hlpAttend = new DSXmlHelper("Request");
            hlpAttend.AddElement("Attend");
            hlpAttend.AddElement("Attend", "RefCourseID", deleteCourseID.ToString());
            EditCourse.DeleteAttend(hlpAttend);
        }
    }
}

