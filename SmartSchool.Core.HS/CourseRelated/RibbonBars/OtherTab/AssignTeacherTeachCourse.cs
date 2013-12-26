using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.CourseRelated;
using DevComponents.DotNetBar;
using SmartSchool.TeacherRelated;
using FISCA.DSAUtil;
using SmartSchool.Feature.Course;
using SmartSchool.Common;
using SmartSchool.Security;
using FISCA.Presentation;

namespace SmartSchool.CourseRelated.RibbonBars.OtherTab
{
    public partial class AssignTeacherTeachCourse : RibbonBarBase
    {
        FeatureAccessControl assignCtrl;

        public AssignTeacherTeachCourse()
        {
            //InitializeComponent();
            //btnAssign.Enabled = false;
            //SmartSchool.CourseRelated.Course.Instance.TemporalChanged += new EventHandler(Instance_TemporalChanged);
            //SmartSchool.TeacherRelated.Teacher.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);

            ////權限判斷 - 指定/授課
            //assignCtrl = new FeatureAccessControl("Button0480");
            //assignCtrl.Inspect(btnAssign);
        }
        internal void Setup()
        {
            assignCtrl = new FeatureAccessControl("Button0480");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AssignTeacherTeachCourse));
            var btnAssign = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["指定"]["授課"];
            K12.Presentation.NLDPanels.Teacher.RibbonBarItems["指定"].Index = 1;
            btnAssign.SupposeHasChildern = true;
            btnAssign.Image = ( (System.Drawing.Image)( resources.GetObject("btnAssign.Image") ) );
            btnAssign.Enable = assignCtrl.Executable() && K12.Presentation.NLDPanels.Teacher.SelectedSource.Count == 1 && Course.Instance.TempCourse.Count > 0;
            btnAssign.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(btnAssign_PopupOpen);

            Course.Instance.TemporalChanged += delegate
            {
                btnAssign.Enable = assignCtrl.Executable() && K12.Presentation.NLDPanels.Teacher.SelectedSource.Count == 1 && Course.Instance.TempCourse.Count > 0;
            };
            Teacher.Instance.SelectionChanged += delegate
            {
                btnAssign.Enable = assignCtrl.Executable() && K12.Presentation.NLDPanels.Teacher.SelectedSource.Count == 1 && Course.Instance.TempCourse.Count > 0;
            };
        }

        void btnAssign_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {

            List<CourseInformation> collection = SmartSchool.CourseRelated.Course.Instance.TempCourse;

            if ( collection.Count > 1 )
            {
                var btnCourseAll = e.VirtualButtons["加入所有待處理課程"];
                btnCourseAll.Click += new EventHandler(btnCourseAll_Click);
            }
            bool b = true;
            foreach ( CourseInformation info in collection )
            {
                var item = e.VirtualButtons[info.CourseName];
                item.Tag = info.Identity;
                item.Click += new EventHandler(item_Click);
                if ( b )
                {
                    item.BeginGroup = true;
                    b = false;
                }
            }
        }
        public override string ProcessTabName
        {
            get
            {
                return "教師";
            }
        }

        void btnCourseAll_Click(object sender, EventArgs e)
        {
            List<CourseInformation> collection = SmartSchool.CourseRelated.Course.Instance.TempCourse;
            List<BriefTeacherData> teachers = SmartSchool.TeacherRelated.Teacher.Instance.SelectionTeachers;
            BriefTeacherData teacher = teachers[0];

            DSXmlHelper helper = new DSXmlHelper("UpdateRequest");

            DSXmlHelper removeBySequence = new DSXmlHelper("Request");
            DSXmlHelper removeByTeacher = new DSXmlHelper("Request");
            DSXmlHelper addnew = new DSXmlHelper("Request");
            foreach ( CourseInformation info in collection )
            {
                removeBySequence.AddElement("Course");
                removeBySequence.AddElement("Course", "CourseID", info.Identity.ToString());
                removeBySequence.AddElement("Course", "Sequence", "1");

                removeByTeacher.AddElement("Course");
                removeByTeacher.AddElement("Course", "CourseID", info.Identity.ToString());
                removeByTeacher.AddElement("Course", "RefTeacherID", teacher.ID);

                addnew.AddElement("CourseTeacher");
                addnew.AddElement("CourseTeacher", "RefCourseID", info.Identity.ToString());
                addnew.AddElement("CourseTeacher", "RefTeacherID", teacher.ID);
                addnew.AddElement("CourseTeacher", "Sequence", "1");

            }
            try
            {
                EditCourse.RemoveCourseTeachers(removeBySequence);
                EditCourse.RemoveCourseTeachers(removeByTeacher);
                EditCourse.AddCourseTeacher(addnew);

                List<int> courseIdList = new List<int>();
                foreach ( CourseInformation each in collection )
                    courseIdList.Add(each.Identity);

                Course.Instance.InvokeAfterCourseChange(courseIdList.ToArray());

                //Log
                StringBuilder course_texts = new StringBuilder("");
                foreach ( CourseInformation info in collection )
                {
                    if ( !string.IsNullOrEmpty(course_texts.ToString()) ) course_texts.Append("、");
                    course_texts.Append(info.CourseName);
                }
                CurrentUser.Instance.AppLog.Write(
                    SmartSchool.ApplicationLog.EntityType.Teacher,
                    "教師授課",
                    teacher.ID,
                    "指定「" + teacher.TeacherName + ( string.IsNullOrEmpty(teacher.Nickname) ? "" : "(" + teacher.Nickname + ")" ) + "」課程：" + course_texts.ToString(),
                    "教師",
                    "");

                MsgBox.Show("指派完成");
            }
            catch ( Exception ex )
            {
                MsgBox.Show("指派失敗:" + ex.Message);
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            MenuButton item = sender as MenuButton;
            string courseid = "" + item.Tag;

            List<BriefTeacherData> teachers = SmartSchool.TeacherRelated.Teacher.Instance.SelectionTeachers;
            BriefTeacherData teacher = teachers[0];

            DSXmlHelper removeBySequence = new DSXmlHelper("Request");
            removeBySequence.AddElement("Course");
            removeBySequence.AddElement("Course", "CourseID", courseid);
            removeBySequence.AddElement("Course", "Sequence", "1");

            DSXmlHelper removeByTeacher = new DSXmlHelper("Request");
            removeByTeacher.AddElement("Course");
            removeByTeacher.AddElement("Course", "CourseID", courseid);
            removeByTeacher.AddElement("Course", "RefTeacherID", teacher.ID);

            DSXmlHelper addnew = new DSXmlHelper("Request");
            addnew.AddElement("CourseTeacher");
            addnew.AddElement("CourseTeacher", "RefCourseID", courseid);
            addnew.AddElement("CourseTeacher", "RefTeacherID", teacher.ID);
            addnew.AddElement("CourseTeacher", "Sequence", "1");

            try
            {
                EditCourse.RemoveCourseTeachers(removeBySequence);
                EditCourse.RemoveCourseTeachers(removeByTeacher);
                EditCourse.AddCourseTeacher(addnew);

                Course.Instance.InvokeAfterCourseChange(int.Parse(courseid));

                //Log
                CurrentUser.Instance.AppLog.Write(
                    SmartSchool.ApplicationLog.EntityType.Teacher,
                    "教師授課",
                    teacher.ID,
                    "指定「" + teacher.TeacherName + ( string.IsNullOrEmpty(teacher.Nickname) ? "" : "(" + teacher.Nickname + ")" ) + "」課程：" + item.Text,
                    "教師",
                    "");

                MsgBox.Show("指派完成");
            }
            catch ( Exception ex )
            {
                MsgBox.Show("指派失敗:" + ex.Message);
            }
        }

    }
}
