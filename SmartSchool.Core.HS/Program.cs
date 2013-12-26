using System;
using System.Collections.Generic;
using System.Text;
using SmartSchool.Common;
using FISCA.DSAUtil;
using System.Xml;
using SmartSchool.Customization.Data;
using System.Collections;
using DevComponents.DotNetBar;
using SmartSchool.Others.Configuration.ExamTemplate;
using SmartSchool.Others.Configuration;
using FISCA.Presentation;
using SmartSchool.Customization.PlugIn.ExtendedContent;
using SmartSchool.ExceptionHandler;
using SmartSchool.Adaatper;
using SmartSchool.AccessControl;
using FISCA.Permission;
using SmartSchool.API.PlugIn.Export;
using SmartSchool.API.PlugIn.Import;
using SmartSchool.ImportExport.Student;
using SmartSchool.StudentRelated.RibbonBars.Import;

namespace SmartSchool
{
    public static class Core_HS_Program
    {

        private const int _PackageLimit = 500;


        private static List<T>[] SplitPackage<T>(List<T> list)
        {
            if (list.Count > 0)
            {
                int packageCount = (list.Count / _PackageLimit + 1);
                int packageSize = list.Count / packageCount + list.Count % packageCount;
                packageCount = 0;
                List<List<T>> packages = new List<List<T>>();
                List<T> packageCurrent = new List<T>();
                foreach (T var in list)
                {
                    packageCurrent.Add(var);
                    packageCount++;
                    if (packageCount == packageSize)
                    {
                        packageCount = 0;
                        packages.Add(packageCurrent);
                        packageCurrent = new List<T>();
                    }
                }
                if (packageCount > 0)
                    packages.Add(packageCurrent);
                return packages.ToArray();
            }
            else
                return new List<T>[0];
        }

        private static List<T> GetList<T>(IEnumerable<T> items)
        {
            List<T> list = new List<T>();
            list.AddRange(items);
            return list;
        }
        public static void Init_Course_Load()
        {
            CourseRelated.Course.Instance.SyncAllBackground();
        }
        public static void Init_Course()
        {
            //CourseRelated.CourseEntity.CreateInstance();
            CourseRelated.Course.Instance.SetupPresentation();

            //新增刪除課程
            SmartSchool.CourseRelated.RibbonBars.Manage.Instance.Setup();
            //教務 - 成績
            new SmartSchool.CourseRelated.RibbonBars.ScoreInput().Setup();
            //教務 - 學生/教師/樣版
            new SmartSchool.CourseRelated.RibbonBars.Assign().Setup();
            //報表
            new SmartSchool.CourseRelated.RibbonBars.Report().Setup();
            //匯出匯入
            new SmartSchool.CourseRelated.RibbonBars.ImportExport().Setup();
            //其他 - 歷程
            new SmartSchool.CourseRelated.RibbonBars.History().Setup();
            //加入加選課按鈕至學生/指定
            new CourseRelated.RibbonBars.OtherTab.AssignStudentAttendCourse().Setup();
            new SmartSchool.CourseRelated.RibbonBars.OtherTab.AssignTeacherTeachCourse().Setup();

            //處裡修課資料取得事件
            Customization.Data.StudentHelper.FillingAttendCourse += new EventHandler<SmartSchool.Customization.Data.FillSemesterInfoEventArgs<SmartSchool.Customization.Data.StudentRecord>>(StudentHelper_FillingAttendCourse);
            Customization.Data.StudentHelper.FillingExamScore += new EventHandler<FillSemesterInfoEventArgs<StudentRecord>>(StudentHelper_FillingExamScore);
            Customization.Data.TeacherHelper.GettingLectureTeacher += new EventHandler<GettingLectureTeacherEventArgs>(TeacherHelper_GettingLectureTeacher);

            //SmartSchool.CourseRelated.DetailPaneItem.OtherEntity
            //SmartSchool.Customization.PlugIn.ExtendedContent.ExtendStudentContent.AddItem(new CourseRelated.DetailPaneItem.OtherEntity.CourseScorePalmerwormItem());
            //SmartSchool.Customization.PlugIn.ExtendedContent.ExtendTeacherContent.AddItem(new CourseRelated.DetailPaneItem.OtherEntity.TeachCourseItem());

            #region Course Extend Detail Items
            List<Customization.PlugIn.ExtendedContent.IContentItem> _items = new List<Customization.PlugIn.ExtendedContent.IContentItem>();

            List<Type> _type_list = new List<Type>(new Type[]{
                typeof(SmartSchool.CourseRelated.DetailPaneItem.OtherEntity.CourseScorePalmerwormItem),
                //typeof(SmartSchool.CourseRelated.DetailPaneItem.OtherEntity.TeachCourseItem)
            });

            foreach (Type type in _type_list)
            {
                if (!Attribute.IsDefined(type, typeof(SmartSchool.AccessControl.FeatureCodeAttribute)) || CurrentUser.Acl[type].Viewable)
                {
                    try
                    {
                        IContentItem item = type.GetConstructor(Type.EmptyTypes).Invoke(null) as IContentItem;
                        _items.Add(item);
                    }
                    catch (Exception ex) { BugReporter.ReportException(ex, false); }
                }
            }
            foreach (Customization.PlugIn.ExtendedContent.IContentItem var in _items)
            {
                K12.Presentation.NLDPanels.Student.AddDetailBulider(new ContentItemBulider(var));
            }
            #endregion

            SmartSchool.Customization.Data.AccessHelper.SetCourseProvider(new SmartSchool.API.Provider.CourseProvider());
            SmartSchool.Customization.PlugIn.ExtendedContent.ExtendCourseContent.SetManager(SmartSchool.CourseRelated.Course.Instance);
            RoleAclSource.Instance["學生"]["功能按鈕"].Add(new ReportFeature("高中系統/匯出評量成績", "匯出評量成績"));
            RoleAclSource.Instance["學生"]["功能按鈕"].Add(new ReportFeature("高中系統/匯入評量成績", "匯入評量成績"));

            #region 匯出/匯入(1000708)
            RibbonBarButton rbItemExport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯出"];
            rbItemExport["成績相關匯出"]["匯出評量成績"].Enable = CurrentUser.Acl["高中系統/匯出評量成績"].Executable;
            rbItemExport["成績相關匯出"]["匯出評量成績"].Click += delegate
            {
                Exporter exporter = new ExportExamScore();
                ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };

            RibbonBarButton rbItemImport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯入"];
            rbItemImport["成績相關匯入"]["匯入評量成績"].Enable = CurrentUser.Acl["高中系統/匯入評量成績"].Executable;
            rbItemImport["成績相關匯入"]["匯入評量成績"].Click += delegate
            {
                Importer importer = new ImportExamScore();
                ImportStudentV2 wizard = new ImportStudentV2(importer.Text, importer.Image);
                importer.InitializeImport(wizard);
                wizard.ShowDialog();
            };

            //SmartSchool.API.PlugIn.PlugInManager.Student.Exporters.Add(new ImportExport.Student.ExportExamScore());
            //SmartSchool.API.PlugIn.PlugInManager.Student.Importers.Add(new ImportExport.Student.ImportExamScore()); 
            #endregion
        }

        public static void Init_Course_Others()
        {
            //SimplyConfigure scoreMappingTable = new SimplyConfigure();
            //scoreMappingTable.Caption = "評量名稱管理";
            //scoreMappingTable.Category = "成績作業";
            //scoreMappingTable.TabGroup = "教務作業";
            //scoreMappingTable.OnShown += new EventHandler(scoreMappingTable_OnShown);
            //if (CurrentUser.Acl["Button0800"].Executable)
            //    SmartSchool.Customization.PlugIn.Configure.SystemConfiguration.AddConfigurationItem(scoreMappingTable);
            var scoreMappingTable = MotherForm.RibbonBarItems["教務作業", "基本設定"]["管理"];
            scoreMappingTable.Size = RibbonBarButton.MenuButtonSize.Large;
            scoreMappingTable.Image = Properties.Resources.network_lock_64;
            scoreMappingTable["評量名稱管理"].Enable = CurrentUser.Acl["Button0800"].Executable;
            scoreMappingTable["評量名稱管理"].Click += new EventHandler(scoreMappingTable_OnShown);
                

            //SimplyConfigure templateManager = new SimplyConfigure();
            //templateManager.Caption = "評分樣版管理";
            //templateManager.Category = "成績作業";
            //templateManager.TabGroup = "教務作業";
            //templateManager.OnShown += new EventHandler(templateManager_OnShown);
            //if (CurrentUser.Acl["Button0810"].Executable)
            //    SmartSchool.Customization.PlugIn.Configure.SystemConfiguration.AddConfigurationItem(templateManager);
            var templateManager = MotherForm.RibbonBarItems["教務作業", "基本設定"]["設定"];
            templateManager.Size = RibbonBarButton.MenuButtonSize.Large;
            templateManager.Image = Properties.Resources.sandglass_unlock_64;
            templateManager["評分樣版設定"].Enable = CurrentUser.Acl["Button0810"].Executable;
            templateManager["評分樣版設定"].Click += new EventHandler(templateManager_OnShown);
                

            new Others.RibbonBars.UnfinishScore().Setup();
        }

        static void templateManager_OnShown(object sender, EventArgs e)
        {
            SmartSchool.Others.Configuration.ScoresTemplate.TemplateManager obj = new SmartSchool.Others.Configuration.ScoresTemplate.TemplateManager();
            obj.ShowDialog();
        }

        static void scoreMappingTable_OnShown(object sender, EventArgs e)
        {
            ExamManager em = new ExamManager();
            em.ShowDialog();
        }

        static void TeacherHelper_GettingLectureTeacher(object sender, GettingLectureTeacherEventArgs e)
        {
            e.Teachers = GetLectureTeacher(e.AccessHelper, e.Course);
        }


        private static List<SmartSchool.Customization.Data.TeacherRecord> GetLectureTeacher(AccessHelper accessHelper, SmartSchool.Customization.Data.CourseRecord course)
        {
            SmartSchool.CourseRelated.CourseInformation cinfo = CourseRelated.Course.Instance.Items["" + course.CourseID];
            if (cinfo != null)
            {
                List<string> teacheridList = new List<string>();
                foreach (CourseRelated.CourseInformation.Teacher t in cinfo.Teachers)
                {
                    teacheridList.Add("" + t.TeacherID);
                }
                return accessHelper.TeacherHelper.GetTeacher(teacheridList);
            }
            return new List<SmartSchool.Customization.Data.TeacherRecord>();
        }

        static void StudentHelper_FillingExamScore(object sender, FillSemesterInfoEventArgs<StudentRecord> e)
        {
            FillExamScore(e.AccessHelper, e.SchoolYear, e.Semester, e.List);
        }
        static void FillExamScore(AccessHelper accessHelper, int schoolYear, int semester, IEnumerable<SmartSchool.Customization.Data.StudentRecord> students)
        {
            Hashtable CachePool = accessHelper.CachePool;
            //先清除學生修課資訊
            foreach (SmartSchool.Customization.Data.StudentRecord var in students)
            {
                var.ExamScoreList.Clear();
            }

            //確保快取課程
            CourseRelated.Course.Instance.EnsureCourse(schoolYear, semester);

            //分批次處理
            foreach (List<SmartSchool.Customization.Data.StudentRecord> studentList in SplitPackage<Customization.Data.StudentRecord>(GetList<SmartSchool.Customization.Data.StudentRecord>(students)))
            {
                #region 下載及填入學生修課資料
                Dictionary<string, SmartSchool.Customization.Data.StudentRecord> studentMapping = new Dictionary<string, SmartSchool.Customization.Data.StudentRecord>();
                foreach (SmartSchool.Customization.Data.StudentRecord var in studentList)
                {
                    if (!studentMapping.ContainsKey(var.StudentID))
                        studentMapping.Add(var.StudentID, var);
                }
                #region 取得修課資料
                List<string> courseid = new List<string>();
                List<string> studentid = new List<string>();
                bool hasCourse = false;
                foreach (SmartSchool.CourseRelated.CourseInformation cinfo in CourseRelated.Course.Instance.Items)
                {
                    if (cinfo.SchoolYear == schoolYear && cinfo.Semester == semester)
                    {
                        if (!courseid.Contains("" + cinfo.Identity))
                            courseid.Add("" + cinfo.Identity);
                        hasCourse = true;
                    }
                }
                foreach (StudentRecord sinfo in studentList)
                {
                    if (!studentid.Contains(sinfo.StudentID))
                        studentid.Add(sinfo.StudentID);
                }
                //如果該學期本來就沒有開任何課程就不用抓了
                if (!hasCourse)
                    break;
                DSResponse rsp = Feature.Course.QueryCourse.GetSECTake(courseid, studentid);
                #endregion
                foreach (XmlElement scElement in rsp.GetContent().GetElements("Score"))
                {
                    DSXmlHelper helper = new DSXmlHelper(scElement);
                    Customization.Data.CourseRecord course;
                    Customization.Data.StudentRecord student = accessHelper.StudentHelper.GetStudents(helper.GetText("RefStudentID")).Count > 0 ? accessHelper.StudentHelper.GetStudents(helper.GetText("RefStudentID"))[0] : null;
                    //Customization.Data.StudentRecord student;
                    //StudentRelated.BriefStudentData sinfo = StudentRelated.Student.Instance.Items[helper.GetText("RefStudentID")];
                    CourseRelated.CourseInformation cinfo = CourseRelated.Course.Instance.Items[helper.GetText("RefCourseID")];
                    if (student != null && cinfo != null)
                    {
                        #region 產生修課紀錄
                        #region 抓課程
                        lock (cinfo)
                        {
                            if (CachePool.ContainsKey(cinfo))
                            {
                                course = (Customization.Data.CourseRecord)CachePool[cinfo];
                            }
                            else
                            {
                                course = new API.CourseRecord(cinfo);
                                CachePool.Add(cinfo, course);
                            }
                        }
                        #endregion

                        decimal tryParsedecimal = 0;
                        int gradeyear = 0;
                        bool required = false;
                        string requiredby = "";
                        int.TryParse(helper.GetText("GradeYear"), out gradeyear);
                        required = (helper.GetText("IsRequired") == "必");
                        requiredby = helper.GetText("RequiredBy");
                        decimal? aScore = (decimal.TryParse(helper.GetText("AttendScore"), out tryParsedecimal) ? (decimal?)tryParsedecimal : null);

                        string examName = helper.GetText("ExamName");
                        string score = helper.GetText("Score");
                        decimal examScore;
                        string specialCase = "";
                        if (decimal.TryParse(score, out tryParsedecimal))
                        {
                            examScore = tryParsedecimal;
                        }
                        else
                        {
                            examScore = 0;
                            specialCase = score;
                        }
                        API.ExamScore escore = new API.ExamScore(student, course, aScore, required, requiredby, examName, examScore, specialCase);
                        #endregion
                        //加到學生資料中
                        studentMapping[student.StudentID].ExamScoreList.Add(escore);
                    }
                }
                #endregion
            }
        }

        static void StudentHelper_FillingAttendCourse(object sender, SmartSchool.Customization.Data.FillSemesterInfoEventArgs<SmartSchool.Customization.Data.StudentRecord> e)
        {
            FillAttendCourse(e.AccessHelper, e.SchoolYear, e.Semester, e.List);
        }
        static void FillAttendCourse(AccessHelper accessHelper, int schoolYear, int semester, IEnumerable<SmartSchool.Customization.Data.StudentRecord> students)
        {
            Hashtable CachePool = accessHelper.CachePool;
            //先清除學生修課資訊
            foreach (SmartSchool.Customization.Data.StudentRecord var in students)
            {
                var.AttendCourseList.Clear();
            }
            //分批次處理
            foreach (List<SmartSchool.Customization.Data.StudentRecord> studentList in SplitPackage<Customization.Data.StudentRecord>(GetList<SmartSchool.Customization.Data.StudentRecord>(students)))
            {
                #region 下載及填入學生修課資料
                //確保快取課程
                CourseRelated.Course.Instance.EnsureCourse(schoolYear, semester);
                Dictionary<string, SmartSchool.Customization.Data.StudentRecord> studentMapping = new Dictionary<string, SmartSchool.Customization.Data.StudentRecord>();
                foreach (SmartSchool.Customization.Data.StudentRecord var in studentList)
                {
                    var.AttendCourseList.Clear();
                    if (!studentMapping.ContainsKey(var.StudentID))
                        studentMapping.Add(var.StudentID, var);
                }
                #region 取得修課資料
                DSXmlHelper helper = new DSXmlHelper("SelectRequest");
                helper.AddElement("Field");
                helper.AddElement("Field", "All");
                helper.AddElement("Condition");
                bool hasCourse = false;
                foreach (SmartSchool.CourseRelated.CourseInformation cinfo in CourseRelated.Course.Instance.Items)
                {
                    if (cinfo.SchoolYear == schoolYear && cinfo.Semester == semester)
                    {
                        helper.AddElement("Condition", "CourseID", "" + cinfo.Identity);
                        hasCourse = true;
                    }
                }
                //如果該學期本來就沒有開任何課程就不用抓了
                if (!hasCourse)
                    break;
                foreach (SmartSchool.Customization.Data.StudentRecord sinfo in studentList)
                {
                    helper.AddElement("Condition", "StudentID", sinfo.StudentID);
                }
                helper.AddElement("Order");
                DSRequest dsreq = new DSRequest(helper);
                DSResponse rsp = Feature.Course.QueryCourse.GetSCAttend(new DSRequest(helper));
                #endregion
                foreach (XmlElement scElement in rsp.GetContent().GetElements("Student"))
                {
                    helper = new DSXmlHelper(scElement);
                    Customization.Data.CourseRecord course;
                    Customization.Data.StudentRecord student = accessHelper.StudentHelper.GetStudents(helper.GetText("RefStudentID")).Count > 0 ? accessHelper.StudentHelper.GetStudents(helper.GetText("RefStudentID"))[0] : null;
                    //Customization.Data.StudentRecord student;
                    //StudentRelated.BriefStudentData sinfo = StudentRelated.Student.Instance.Items[helper.GetText("RefStudentID")];
                    CourseRelated.CourseInformation cinfo = CourseRelated.Course.Instance.Items[helper.GetText("RefCourseID")];
                    if (student != null && cinfo != null)
                    {
                        #region 產生修課紀錄
                        #region 抓課程
                        lock (cinfo)
                        {
                            if (CachePool.ContainsKey(cinfo))
                            {
                                course = (Customization.Data.CourseRecord)CachePool[cinfo];
                            }
                            else
                            {
                                course = new API.CourseRecord(cinfo);
                                CachePool.Add(cinfo, course);
                            }
                        }
                        #endregion
                        decimal finalscore = 0;
                        int gradeyear = 0;
                        int.TryParse(helper.GetText("GradeYear"), out gradeyear);
                        bool? required = null;
                        string requiredby = null;
                        switch (helper.GetText("IsRequired"))
                        {
                            case "必":
                                required = true;
                                break;
                            case "選":
                                required = false;
                                break;
                            default:
                                required = null;
                                break;
                        }
                        switch (helper.GetText("RequiredBy"))
                        {
                            case "部訂":
                            case "校訂":
                                requiredby = helper.GetText("RequiredBy");
                                break;
                            default:
                                requiredby = null;
                                break;
                        }
                        API.StudentAttendCourseRecord record = new API.StudentAttendCourseRecord(student, course, (decimal.TryParse(helper.GetText("Score"), out finalscore) ? (decimal?)finalscore : null), required, requiredby);
                        #endregion
                        //加到學生資料中
                        studentMapping[student.StudentID].AttendCourseList.Add(record);
                    }
                }
                #endregion
            }
        }

    }
}
