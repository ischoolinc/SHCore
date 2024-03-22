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
using System.Security.Principal;
using Aspose.Cells;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class Assign : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase
    {
        //�v���P�_
        FeatureAccessControl attendStudentCtrl;
        FeatureAccessControl assignTeacherCtrl;
        FeatureAccessControl scoresCtrl;

        Dictionary<string, string> TempNameDic { get; set; }

        private List<ButtonItem> _items;
        public Assign()
        {
        }

        internal void Setup()
        {
            //�v���P�_
            attendStudentCtrl = new FeatureAccessControl("Button0570");
            assignTeacherCtrl = new FeatureAccessControl("Button0580");
            scoresCtrl = new FeatureAccessControl("Button0590");

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Assign));
            var btnAttendStudent = K12.Presentation.NLDPanels.Course.RibbonBarItems["���w"]["�׽Ҿǥ�"];
            btnAttendStudent.Image = Properties.Resources.group_64;
            btnAttendStudent.SupposeHasChildern = true;
            btnAttendStudent.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(btnAttendStudent_PopupOpen);
            btnAttendStudent.Size = RibbonBarButton.MenuButtonSize.Medium;
            btnAttendStudent.Enable = attendStudentCtrl.Executable() && (K12.Presentation.NLDPanels.Student.TempSource.Count > 0) && (K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0);

            var btnAssignTeacher = K12.Presentation.NLDPanels.Course.RibbonBarItems["���w"]["�����Юv"];
            btnAssignTeacher.Image = Properties.Resources.admin_64;
            btnAssignTeacher.SupposeHasChildern = true;
            btnAssignTeacher.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(btnAssignTeacher_PopupOpen);
            btnAssignTeacher.Size = RibbonBarButton.MenuButtonSize.Medium;
            btnAssignTeacher.Enable = assignTeacherCtrl.Executable() && (K12.Presentation.NLDPanels.Teacher.TempSource.Count > 0) && (K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0);

            var btnScores = K12.Presentation.NLDPanels.Course.RibbonBarItems["���w"]["�����˪�"];
            btnScores.Image = ((System.Drawing.Image)(resources.GetObject("btnScores.Image")));
            btnScores.SupposeHasChildern = true;
            btnScores.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(btnScores_PopupOpen);
            btnScores.Size = RibbonBarButton.MenuButtonSize.Medium;
            btnScores.Enable = scoresCtrl.Executable() && (K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0);


            SmartSchool.StudentRelated.Student.Instance.TemporalChanged += delegate
            {
                btnAttendStudent.Enable = attendStudentCtrl.Executable() && (K12.Presentation.NLDPanels.Student.TempSource.Count > 0) && (K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0);
            };
            K12.Presentation.NLDPanels.Teacher.TempSourceChanged += delegate
            {
                btnAssignTeacher.Enable = assignTeacherCtrl.Executable() && (K12.Presentation.NLDPanels.Teacher.TempSource.Count > 0) && (K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0);
            };
            Course.Instance.ForeignTableChanged += delegate
            {
                _items = null;
            };
            K12.Presentation.NLDPanels.Course.SelectedSourceChanged += delegate
            {
                btnAttendStudent.Enable = attendStudentCtrl.Executable() && (K12.Presentation.NLDPanels.Student.TempSource.Count > 0) && (K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0);
                btnAssignTeacher.Enable = assignTeacherCtrl.Executable() && (K12.Presentation.NLDPanels.Teacher.TempSource.Count > 0) && (K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0);
                btnScores.Enable = scoresCtrl.Executable() && (K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0);
            };

        }

        void btnScores_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            try
            {
                if (_items == null)
                {
                    _items = new List<ButtonItem>();

                    XmlElement templates = QueryTemplate.GetAbstractList();
                    TempNameDic = new Dictionary<string, string>();
                    foreach (XmlElement each in templates.SelectNodes("ExamTemplate"))
                    {
                        TemplateButton button = new TemplateButton(each);

                        string Text = each.SelectSingleNode("TemplateName").InnerText;
                        string Identity = each.GetAttribute("ID");

                        if (!TempNameDic.ContainsKey(Identity))
                            TempNameDic.Add(Identity, Text);

                        button.Click += new EventHandler(TemplateButton_Click);
                        _items.Add(button);
                    }
                }
                foreach (TemplateButton button in _items)
                {
                    var temp = e.VirtualButtons[button.Text];
                    temp.Tag = button;
                    temp.Click += new EventHandler(temp_Click);
                }
            }
            catch (Exception ex)
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
                TemplateButton button = (sender as MenuButton).Tag as TemplateButton;
                string templateId = button.Identity;

                List<CourseInformation> courses = Course.Instance.SelectionCourse;
                StringBuilder sb_log = new StringBuilder();
                sb_log.AppendLine(string.Format("�ҵ{�����˪��u{0}�v�w���w���H�U�ҵ{", button.Text));
                sb_log.AppendLine("");
                if (courses.Count > 0)
                {
                    DSXmlHelper req = new DSXmlHelper("UpdateRequest");

                    foreach (CourseInformation each in courses)
                    {
                        sb_log.Append(string.Format("�ҵ{�W�١u{0}�v", each.CourseName));
                        sb_log.Append(string.Format("�Ǧ~�סu{0}�v", each.SchoolYear));
                        sb_log.Append(string.Format("�Ǵ��u{0}�v", each.Semester));

                        if (TempNameDic.ContainsKey("" + each.RefExamTemplateID))
                        {
                            string name = TempNameDic["" + each.RefExamTemplateID];
                            sb_log.AppendLine(string.Format("(��ҵ{�����˪��u{0}�v)", name));
                        }
                        else
                        {
                            sb_log.AppendLine("����q�W�١u���]�w�v");
                        }


                        req.AddElement("Course");
                        req.AddElement("Course", "Field", "<RefExamTemplateID>" + templateId + "</RefExamTemplateID>", true);
                        req.AddElement("Course", "Condition", "<ID>" + each.Identity + "</ID>", true);
                    }

                    EditCourse.UpdateCourse(new DSRequest(req));

                    List<int> courseids = new List<int>();
                    foreach (CourseInformation each in courses)
                        courseids.Add(each.Identity);

                    Course.Instance.InvokeAfterCourseChange(courseids.ToArray());
                    FISCA.LogAgent.ApplicationLog.Log("�ҵ{�����˪�", "���w", sb_log.ToString());
                    MsgBox.Show("�ҵ{�����˪����w�����C\n���w�ҵ{�ơG" + courses.Count, Application.ProductName);
                }
            }
            catch (Exception ex)
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);

                MsgBox.Show(ex.Message, Application.ProductName);
            }

        }

        void btnAssignTeacher_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            List<BriefTeacherData> teacherList = SmartSchool.TeacherRelated.Teacher.Instance.TempTeacher;

            foreach (BriefTeacherData teacher in teacherList)
            {
                var item = e.VirtualButtons[teacher.UniqName];
                item.Tag = teacher.ID;
                item.Click += new EventHandler(item_Click);
            }
        }

        void btnAttendStudent_PopupOpen(object sender, FISCA.Presentation.PopupOpenEventArgs e)
        {
            List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.TempStudent;
            if (students.Count > 1)
            {
                var btnStudentAll = e.VirtualButtons["�[�J�Ҧ��ݳB�z�ǥ�"];
                btnStudentAll.AutoCheckOnClick = false;
                btnStudentAll.Click += new EventHandler(btnStudentAll_Click);
            }
            bool b = true;
            foreach (BriefStudentData student in students)
            {
                var sitem = e.VirtualButtons["�i" + student.ClassName + "�j" + student.Name];
                sitem.Tag = student;
                sitem.Click += new EventHandler(sitem_Click);
                if (b)
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
            if (courses.Count == 0)
            {
                MsgBox.Show("�z������ܽҵ{");
                return;
            }

            DSXmlHelper removeBySequence = new DSXmlHelper("Request");
            foreach (CourseInformation info in courses)
            {
                removeBySequence.AddElement("Course");
                removeBySequence.AddElement("Course", "CourseID", info.Identity.ToString());
                removeBySequence.AddElement("Course", "Sequence", "1");
            }

            DSXmlHelper removeByTeacher = new DSXmlHelper("Request");
            foreach (CourseInformation info in courses)
            {
                removeByTeacher.AddElement("Course");
                removeByTeacher.AddElement("Course", "CourseID", info.Identity.ToString());
                removeByTeacher.AddElement("Course", "RefTeacherID", teacherid);
            }

            DSXmlHelper addnew = new DSXmlHelper("Request");
            foreach (CourseInformation info in courses)
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
                foreach (CourseInformation each in courses)
                    courseIdList.Add(each.Identity);

                Course.Instance.InvokeAfterCourseChange(courseIdList.ToArray());

                MsgBox.Show("��������");
            }
            catch (Exception ex)
            {
                MsgBox.Show("��������:" + ex.Message);
            }
        }


        void btnStudentAll_Click(object sender, EventArgs e)
        {
            List<CourseInformation> collection = SmartSchool.CourseRelated.Course.Instance.SelectionCourse;
            if (collection.Count == 0)
            {
                MsgBox.Show("�z������ܦܤ֤@���ҵ{");
                return;
            }
            //List<BriefStudentData> invalidStudents = SmartSchool.StudentRelated.Student.Instance.TemporaStudent;
            List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.TempStudent;

            //���Ҿǥ͸��
            //#region ���Ҿǥ͸��
            //AbstractValidateStudent validate = new ValidateBasic(new ValidateGradeYear(), new ValidateGraduationPlan());
            //ErrorViewer viewer = new ErrorViewer();
            //viewer.Text = "����ǥ͸�ƿ��~�L�k�[�J�׽�";
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
            foreach (CourseInformation info in collection)
            {
                foreach (BriefStudentData student in students)
                {
                    helper.AddElement("Condition/Or", "And");
                    helper.AddElement("Condition/Or/And", "StudentID", student.ID);
                    helper.AddElement("Condition/Or/And", "CourseID", info.Identity.ToString());
                }
            }
            DSResponse dsrsp = SmartSchool.Feature.Course.QueryCourse.GetSCAttend(new DSRequest(helper));

            int insertCount = 0;
            helper = new DSXmlHelper("InsertSCAttend");


            foreach (CourseInformation info in collection)
            {
                foreach (BriefStudentData student in students)
                {
                    //�ǥͥ����O�b�եͤ~��[�J�ҵ{�C
                    if (!student.IsNormal) continue;

                    XmlElement element = null;
                    foreach (XmlElement ele in dsrsp.GetContent().GetElements("Student"))
                    {
                        string courseid = info.Identity.ToString();
                        string studentid = student.ID;
                        if (courseid == ele.SelectSingleNode("RefCourseID").InnerText && studentid == ele.SelectSingleNode("RefStudentID").InnerText)
                            element = ele;
                    }

                    if (element == null)
                    {
                        // �S��Element ��ܸӾǥͨS���׹L�o�ӽҵ{�A�n�s�W
                        helper.AddElement("Attend");
                        helper.AddElement("Attend", "RefCourseID", info.Identity.ToString());
                        helper.AddElement("Attend", "RefStudentID", student.ID);

                        //// �d�X�����
                        //string required = "��";
                        //string requiredby = "�խq";
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
            if (insertCount > 0)
                SmartSchool.Feature.Course.AddCourse.AttendCourse(helper);
            MsgBox.Show("�B�z����");
            SmartSchool.Broadcaster.Events.Items["�ҵ{/�ǥͭ׽�"].Invoke(collection);
        }

        void sitem_Click(object sender, EventArgs e)
        {
            MenuButton item = sender as MenuButton;
            BriefStudentData data = item.Tag as BriefStudentData;
            string studentid = data.ID; ;

            List<CourseInformation> collection = SmartSchool.CourseRelated.Course.Instance.SelectionCourse;
            if (collection.Count == 0)
            {
                MsgBox.Show("�z������ܦܤ֤@���ҵ{");
                return;
            }

            //���ҳ浧�ǥ͸��
            //#region ���Ҿǥ͸��
            //AbstractValidateStudent validate = new ValidateBasic(new ValidateGradeYear(), new ValidateGraduationPlan());
            //ErrorViewer viewer = new ErrorViewer();
            //viewer.Text = "�ǥ͸�ƿ��~�L�k�[�J�׽�";
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
            foreach (CourseInformation info in collection)
            {
                helper.AddElement("Condition/Or", "And");
                helper.AddElement("Condition/Or/And", "StudentID", studentid);
                helper.AddElement("Condition/Or/And", "CourseID", info.Identity.ToString());
            }
            DSResponse dsrsp = SmartSchool.Feature.Course.QueryCourse.GetSCAttend(new DSRequest(helper));

            int insertCount = 0;
            helper = new DSXmlHelper("InsertSCAttend");

            foreach (CourseInformation info in collection)
            {
                XmlElement element = dsrsp.GetContent().GetElement("Student[RefCourseID='" + info.Identity + "']");
                if (element == null)
                {
                    // �S��Element ��ܸӾǥͨS���׹L�o�ӽҵ{�A�n�s�W
                    helper.AddElement("Attend");
                    helper.AddElement("Attend", "RefCourseID", info.Identity.ToString());
                    helper.AddElement("Attend", "RefStudentID", studentid);

                    //// �d�X�����
                    //string required = "��";
                    //string requiredby = "�խq";
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
            if (insertCount > 0)
                SmartSchool.Feature.Course.AddCourse.AttendCourse(helper);
            MsgBox.Show("�B�z����");
            SmartSchool.Broadcaster.Events.Items["�ҵ{/�ǥͭ׽�"].Invoke(collection);
        }

        private string GetRequiredString(string input)
        {
            if (input == "��" || input == "��")
                return input;
            else
            {
                if (input == "����")
                    return "��";
                else if (input == "���")
                    return "��";
                else
                    throw new ArgumentException("�u���\�u���v�Ρu��v�A�������u" + input + "�v");
            }
        }

        private void TemplateButton_Click(object sender, EventArgs e)
        {
            try
            {
                TemplateButton button = sender as TemplateButton;
                string templateId = button.Identity;

                List<CourseInformation> courses = Course.Instance.SelectionCourse;

                if (courses.Count > 0)
                {
                    DSXmlHelper req = new DSXmlHelper("UpdateRequest");

                    foreach (CourseInformation each in courses)
                    {
                        req.AddElement("Course");
                        req.AddElement("Course", "Field", "<RefExamTemplateID>" + templateId + "</RefExamTemplateID>", true);
                        req.AddElement("Course", "Condition", "<ID>" + each.Identity + "</ID>", true);
                    }

                    EditCourse.UpdateCourse(new DSRequest(req));

                    List<int> courseids = new List<int>();
                    foreach (CourseInformation each in courses)
                        courseids.Add(each.Identity);

                    Course.Instance.InvokeAfterCourseChange(courseids.ToArray());

                    MsgBox.Show("�ҵ{�����˪����w�����C\n���w�ҵ{�ơG" + courses.Count, Application.ProductName);
                }
            }
            catch (Exception ex)
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

