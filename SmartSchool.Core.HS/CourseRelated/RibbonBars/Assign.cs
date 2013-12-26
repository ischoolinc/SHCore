using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.TeacherRelated;
using DevComponents.DotNetBar;
using FISCA.DSAUtil;
using SmartSchool.CourseRelated;
using SmartSchool.StudentRelated;
using System.Xml;
using SmartSchool.StudentRelated.Validate;
using SmartSchool.Common.Validate;
using SmartSchool.Feature.Course;
using SmartSchool.Feature.ExamTemplate;
using SmartSchool.Common;
using SmartSchool.Security;
using FISCA.Presentation;
using SmartSchool.ExceptionHandler;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class Assign : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase
    {
        //權限判斷
        FeatureAccessControl attendStudentCtrl;
        FeatureAccessControl assignTeacherCtrl;
        FeatureAccessControl scoresCtrl;

        private List<ButtonItem> _items;
        public Assign()
        {
        }

        internal void Setup()
        {
            //權限判斷
            attendStudentCtrl = new FeatureAccessControl("Button0570");
            assignTeacherCtrl = new FeatureAccessControl("Button0580");
            scoresCtrl = new FeatureAccessControl("Button0590");

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Assign));
            var btnAttendStudent = K12.Presentation.NLDPanels.Course.RibbonBarItems["指定"]["修課學生"];
            btnAttendStudent.Image = Properties.Resources.group_64;
            btnAttendStudent.SupposeHasChildern = true;
            btnAttendStudent.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(btnAttendStudent_PopupOpen);
            btnAttendStudent.Size = RibbonBarButton.MenuButtonSize.Medium;
            btnAttendStudent.Enable = attendStudentCtrl.Executable() && ( K12.Presentation.NLDPanels.Student.TempSource.Count > 0 ) && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );

            var btnAssignTeacher = K12.Presentation.NLDPanels.Course.RibbonBarItems["指定"]["評分教師"];
            btnAssignTeacher.Image = Properties.Resources.admin_64;
            btnAssignTeacher.SupposeHasChildern = true;
            btnAssignTeacher.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(btnAssignTeacher_PopupOpen);
            btnAssignTeacher.Size = RibbonBarButton.MenuButtonSize.Medium;
            btnAssignTeacher.Enable = assignTeacherCtrl.Executable() && ( K12.Presentation.NLDPanels.Teacher.TempSource.Count > 0 ) && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );

            var btnScores = K12.Presentation.NLDPanels.Course.RibbonBarItems["指定"]["評分樣版"];
            btnScores.Image = ( (System.Drawing.Image)( resources.GetObject("btnScores.Image") ) );
            btnScores.SupposeHasChildern = true;
            btnScores.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(btnScores_PopupOpen);
            btnScores.Size = RibbonBarButton.MenuButtonSize.Medium;
            btnScores.Enable = scoresCtrl.Executable() && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );


            SmartSchool.StudentRelated.Student.Instance.TemporalChanged += delegate
            {
                btnAttendStudent.Enable = attendStudentCtrl.Executable() && ( K12.Presentation.NLDPanels.Student.TempSource.Count > 0 ) && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );
            };
            K12.Presentation.NLDPanels.Teacher.TempSourceChanged += delegate
            {
                btnAssignTeacher.Enable = assignTeacherCtrl.Executable() && ( K12.Presentation.NLDPanels.Teacher.TempSource.Count > 0 ) && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );
            };
            Course.Instance.ForeignTableChanged += delegate
            {
                _items = null;
            };
            K12.Presentation.NLDPanels.Course.SelectedSourceChanged += delegate
            {
                btnAttendStudent.Enable = attendStudentCtrl.Executable() && ( K12.Presentation.NLDPanels.Student.TempSource.Count > 0 ) && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );
                btnAssignTeacher.Enable = assignTeacherCtrl.Executable() && ( K12.Presentation.NLDPanels.Teacher.TempSource.Count > 0 ) && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );
                btnScores.Enable = scoresCtrl.Executable() && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );
            };

        }

        void btnScores_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            try
            {
                if ( _items == null )
                {
                    _items = new List<ButtonItem>();

                    XmlElement templates = QueryTemplate.GetAbstractList();
                    foreach ( XmlElement each in templates.SelectNodes("ExamTemplate") )
                    {
                        TemplateButton button = new TemplateButton(each);
                        button.Click += new EventHandler(TemplateButton_Click);
                        _items.Add(button);
                    }
                }
                foreach ( TemplateButton button in _items )
                {
                    var temp = e.VirtualButtons[button.Text];
                    temp.Tag = button;
                    temp.Click += new EventHandler(temp_Click);
                }
            }
            catch ( Exception ex )
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);

                MsgBox.Show(ex.Message, Application.ProductName);
            }
        }

        void temp_Click(object sender, EventArgs e)
        {
            try
            {
                TemplateButton button = ( sender as MenuButton ).Tag as TemplateButton;
                string templateId = button.Identity;

                List<CourseInformation> courses = Course.Instance.SelectionCourse;

                if ( courses.Count > 0 )
                {
                    DSXmlHelper req = new DSXmlHelper("UpdateRequest");

                    foreach ( CourseInformation each in courses )
                    {
                        req.AddElement("Course");
                        req.AddElement("Course", "Field", "<RefExamTemplateID>" + templateId + "</RefExamTemplateID>", true);
                        req.AddElement("Course", "Condition", "<ID>" + each.Identity + "</ID>", true);
                    }

                    EditCourse.UpdateCourse(new DSRequest(req));

                    List<int> courseids = new List<int>();
                    foreach ( CourseInformation each in courses )
                        courseids.Add(each.Identity);

                    Course.Instance.InvokeAfterCourseChange(courseids.ToArray());

                    MsgBox.Show("課程評分樣版指定完成。\n指定課程數：" + courses.Count, Application.ProductName);
                }
            }
            catch ( Exception ex )
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);

                MsgBox.Show(ex.Message, Application.ProductName);
            }

        }

        void btnAssignTeacher_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            List<BriefTeacherData> teacherList = SmartSchool.TeacherRelated.Teacher.Instance.TempTeacher;

            foreach ( BriefTeacherData teacher in teacherList )
            {
                var item = e.VirtualButtons[teacher.UniqName];
                item.Tag = teacher.ID;
                item.Click += new EventHandler(item_Click);
            }
        }

        void btnAttendStudent_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.TempStudent;
            if ( students.Count > 1 )
            {
                var btnStudentAll = e.VirtualButtons["加入所有待處理學生"];
                btnStudentAll.AutoCheckOnClick = false;
                btnStudentAll.Click += new EventHandler(btnStudentAll_Click);
            }
            bool b = true;
            foreach ( BriefStudentData student in students )
            {
                var sitem = e.VirtualButtons["【" + student.ClassName + "】" + student.Name];
                sitem.Tag = student;
                sitem.Click += new EventHandler(sitem_Click);
                if ( b )
                {
                    sitem.BeginGroup = true;
                    b = false;
                }
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            MenuButton item = sender as MenuButton;
            string teacherid = "" + item.Tag;

            List<CourseInformation> courses = SmartSchool.CourseRelated.Course.Instance.SelectionCourse;
            if ( courses.Count == 0 )
            {
                MsgBox.Show("您必須選擇課程");
                return;
            }

            DSXmlHelper removeBySequence = new DSXmlHelper("Request");
            foreach ( CourseInformation info in courses )
            {
                removeBySequence.AddElement("Course");
                removeBySequence.AddElement("Course", "CourseID", info.Identity.ToString());
                removeBySequence.AddElement("Course", "Sequence", "1");
            }

            DSXmlHelper removeByTeacher = new DSXmlHelper("Request");
            foreach ( CourseInformation info in courses )
            {
                removeByTeacher.AddElement("Course");
                removeByTeacher.AddElement("Course", "CourseID", info.Identity.ToString());
                removeByTeacher.AddElement("Course", "RefTeacherID", teacherid);
            }

            DSXmlHelper addnew = new DSXmlHelper("Request");
            foreach ( CourseInformation info in courses )
            {
                addnew.AddElement("CourseTeacher");
                addnew.AddElement("CourseTeacher", "RefCourseID", info.Identity.ToString());
                addnew.AddElement("CourseTeacher", "RefTeacherID", teacherid);
                addnew.AddElement("CourseTeacher", "Sequence", "1");
            }

            try
            {
                EditCourse.RemoveCourseTeachers(removeBySequence);
                EditCourse.RemoveCourseTeachers(removeByTeacher);
                EditCourse.AddCourseTeacher(addnew);

                List<int> courseIdList = new List<int>();
                foreach ( CourseInformation each in courses )
                    courseIdList.Add(each.Identity);

                Course.Instance.InvokeAfterCourseChange(courseIdList.ToArray());

                MsgBox.Show("指派完成");
            }
            catch ( Exception ex )
            {
                MsgBox.Show("指派失敗:" + ex.Message);
            }
        }


        void btnStudentAll_Click(object sender, EventArgs e)
        {
            List<CourseInformation> collection = SmartSchool.CourseRelated.Course.Instance.SelectionCourse;
            if ( collection.Count == 0 )
            {
                MsgBox.Show("您必須選擇至少一筆課程");
                return;
            }
            //List<BriefStudentData> invalidStudents = SmartSchool.StudentRelated.Student.Instance.TemporaStudent;
            List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.TempStudent;

            //驗證學生資料
            //#region 驗證學生資料
            //AbstractValidateStudent validate = new ValidateBasic(new ValidateGradeYear(), new ValidateGraduationPlan());
            //ErrorViewer viewer = new ErrorViewer();
            //viewer.Text = "選取學生資料錯誤無法加入修課";
            //bool pass = true;
            //foreach (BriefStudentData var in students)
            //{
            //    pass &= validate.Validate(var, viewer);
            //}
            //if (!pass)
            //{
            //    viewer.Show();
            //    return;
            //}
            //#endregion


            DSXmlHelper helper = new DSXmlHelper("SelectRequest");
            helper.AddElement("Field");
            helper.AddElement("Field", "RefStudentID");
            helper.AddElement("Field", "RefCourseID");
            helper.AddElement("Condition");
            helper.AddElement("Condition", "Or");
            foreach ( CourseInformation info in collection )
            {
                foreach ( BriefStudentData student in students )
                {
                    helper.AddElement("Condition/Or", "And");
                    helper.AddElement("Condition/Or/And", "StudentID", student.ID);
                    helper.AddElement("Condition/Or/And", "CourseID", info.Identity.ToString());
                }
            }
            DSResponse dsrsp = SmartSchool.Feature.Course.QueryCourse.GetSCAttend(new DSRequest(helper));

            int insertCount = 0;
            helper = new DSXmlHelper("InsertSCAttend");


            foreach ( CourseInformation info in collection )
            {
                foreach ( BriefStudentData student in students )
                {
                    //學生必須是在校生才能加入課程。
                    if ( !student.IsNormal ) continue;

                    XmlElement element = null;
                    foreach ( XmlElement ele in dsrsp.GetContent().GetElements("Student") )
                    {
                        string courseid = info.Identity.ToString();
                        string studentid = student.ID;
                        if ( courseid == ele.SelectSingleNode("RefCourseID").InnerText && studentid == ele.SelectSingleNode("RefStudentID").InnerText )
                            element = ele;
                    }

                    if ( element == null )
                    {
                        // 沒有Element 表示該學生沒有修過這個課程，要新增
                        helper.AddElement("Attend");
                        helper.AddElement("Attend", "RefCourseID", info.Identity.ToString());
                        helper.AddElement("Attend", "RefStudentID", student.ID);

                        //// 查出必選修
                        //string required = "選";
                        //string requiredby = "校訂";
                        ////foreach (GraduationPlanSubject subject in student.GraduationPlanInfo.Subjects)
                        ////{
                        ////    if (info.Subject == subject.SubjectName && info.SubjectLevel == subject.Level)
                        ////    {
                        ////        required = subject.Required;
                        ////        requiredby = subject.RequiredBy;
                        ////    }
                        ////}
                        //GraduationPlanSubject subject = student.GraduationPlanInfo.GetSubjectInfo(info.Subject, info.SubjectLevel);
                        //required = subject.Required;
                        //requiredby = subject.RequiredBy;
                        //helper.AddElement("Attend", "IsRequired", GetRequiredString(required));
                        //helper.AddElement("Attend", "RequiredBy", requiredby);
                        //helper.AddElement("Attend", "GradeYear", student.GradeYear);
                        insertCount++;
                    }
                }
            }
            if ( insertCount > 0 )
                SmartSchool.Feature.Course.AddCourse.AttendCourse(helper);
            MsgBox.Show("處理完畢");
            SmartSchool.Broadcaster.Events.Items["課程/學生修課"].Invoke(collection);
        }

        void sitem_Click(object sender, EventArgs e)
        {
            MenuButton item = sender as MenuButton;
            BriefStudentData data = item.Tag as BriefStudentData;
            string studentid = data.ID; ;

            List<CourseInformation> collection = SmartSchool.CourseRelated.Course.Instance.SelectionCourse;
            if ( collection.Count == 0 )
            {
                MsgBox.Show("您必須選擇至少一筆課程");
                return;
            }

            //驗證單筆學生資料
            //#region 驗證學生資料
            //AbstractValidateStudent validate = new ValidateBasic(new ValidateGradeYear(), new ValidateGraduationPlan());
            //ErrorViewer viewer = new ErrorViewer();
            //viewer.Text = "學生資料錯誤無法加入修課";
            //bool pass = true;
            //pass &= validate.Validate(data, viewer);
            //if (!pass)
            //{
            //    viewer.Show();
            //    return;
            //}
            //#endregion

            DSXmlHelper helper = new DSXmlHelper("SelectRequest");
            helper.AddElement("Field");
            helper.AddElement("Field", "RefCourseID");
            helper.AddElement("Condition");
            helper.AddElement("Condition", "Or");
            foreach ( CourseInformation info in collection )
            {
                helper.AddElement("Condition/Or", "And");
                helper.AddElement("Condition/Or/And", "StudentID", studentid);
                helper.AddElement("Condition/Or/And", "CourseID", info.Identity.ToString());
            }
            DSResponse dsrsp = SmartSchool.Feature.Course.QueryCourse.GetSCAttend(new DSRequest(helper));

            int insertCount = 0;
            helper = new DSXmlHelper("InsertSCAttend");

            foreach ( CourseInformation info in collection )
            {
                XmlElement element = dsrsp.GetContent().GetElement("Student[RefCourseID='" + info.Identity + "']");
                if ( element == null )
                {
                    // 沒有Element 表示該學生沒有修過這個課程，要新增
                    helper.AddElement("Attend");
                    helper.AddElement("Attend", "RefCourseID", info.Identity.ToString());
                    helper.AddElement("Attend", "RefStudentID", studentid);

                    //// 查出必選修
                    //string required = "選";
                    //string requiredby = "校訂";
                    ////foreach (GraduationPlanSubject subject in data.GraduationPlanInfo.Subjects)
                    ////{
                    ////    if (info.Subject == subject.SubjectName && info.SubjectLevel == subject.Level)
                    ////    {
                    ////        required = subject.Required;
                    ////        requiredby = subject.RequiredBy;
                    ////    }
                    ////}
                    //GraduationPlanSubject subject = data.GraduationPlanInfo.GetSubjectInfo(info.Subject, info.SubjectLevel);
                    //required = subject.Required;
                    //requiredby = subject.RequiredBy;
                    //helper.AddElement("Attend", "IsRequired", GetRequiredString(required));
                    //helper.AddElement("Attend", "RequiredBy", requiredby);
                    //helper.AddElement("Attend", "GradeYear", data.GradeYear);
                    insertCount++;
                }
            }
            if ( insertCount > 0 )
                SmartSchool.Feature.Course.AddCourse.AttendCourse(helper);
            MsgBox.Show("處理完畢");
            SmartSchool.Broadcaster.Events.Items["課程/學生修課"].Invoke(collection);
        }

        private string GetRequiredString(string input)
        {
            if ( input == "必" || input == "選" )
                return input;
            else
            {
                if ( input == "必修" )
                    return "必";
                else if ( input == "選修" )
                    return "選";
                else
                    throw new ArgumentException("只允許「必」或「選」，不接受「" + input + "」");
            }
        }

        private void TemplateButton_Click(object sender, EventArgs e)
        {
            try
            {
                TemplateButton button = sender as TemplateButton;
                string templateId = button.Identity;

                List<CourseInformation> courses = Course.Instance.SelectionCourse;

                if ( courses.Count > 0 )
                {
                    DSXmlHelper req = new DSXmlHelper("UpdateRequest");

                    foreach ( CourseInformation each in courses )
                    {
                        req.AddElement("Course");
                        req.AddElement("Course", "Field", "<RefExamTemplateID>" + templateId + "</RefExamTemplateID>", true);
                        req.AddElement("Course", "Condition", "<ID>" + each.Identity + "</ID>", true);
                    }

                    EditCourse.UpdateCourse(new DSRequest(req));

                    List<int> courseids = new List<int>();
                    foreach ( CourseInformation each in courses )
                        courseids.Add(each.Identity);

                    Course.Instance.InvokeAfterCourseChange(courseids.ToArray());

                    MsgBox.Show("課程評分樣版指定完成。\n指定課程數：" + courses.Count, Application.ProductName);
                }
            }
            catch ( Exception ex )
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);

                MsgBox.Show(ex.Message, Application.ProductName);
            }
        }

        private class TemplateButton : ButtonItem
        {
            private string _identity;

            public TemplateButton(XmlElement templateInfo)
            {
                Text = templateInfo.SelectSingleNode("TemplateName").InnerText;
                _identity = templateInfo.GetAttribute("ID");
            }

            public string Identity
            {
                get { return _identity; }
            }
        }
    }
}

