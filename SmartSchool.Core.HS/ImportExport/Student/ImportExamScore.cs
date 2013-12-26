using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data;
using SmartSchool.API.PlugIn;
using FISCA.DSAUtil;
using SmartSchool.Feature.Course;
using SmartSchool.Common;
using System.Threading;
using SmartSchool.AccessControl;
using System.ComponentModel;

namespace SmartSchool.ImportExport.Student
{
    [FeatureCode("高中系統/匯入評量成績")]
    class ImportExamScore : SmartSchool.API.PlugIn.Import.Importer
    {
        public ImportExamScore()
        {
            this.Image = null;
            this.Text = "匯入評量成績";
        }
        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            Dictionary<string, string> exams = new Dictionary<string, string>();
            XmlElement list = SmartSchool.Feature.Exam.QueryExam.GetAbstractList();
            List<XmlElement> nodes = new List<XmlElement>();
            foreach (XmlElement node in list.SelectNodes("Exam"))
            {
                nodes.Add(node);
            }
            nodes.Sort(delegate(XmlElement node1, XmlElement node2)
            {
                int i1 = int.MinValue;
                int i2 = int.MinValue;
                int.TryParse(node1.SelectSingleNode("DisplayOrder").InnerText, out i1);
                int.TryParse(node2.SelectSingleNode("DisplayOrder").InnerText, out i2);
                return i1.CompareTo(i2);
            });
            foreach (XmlElement node in nodes)
            {
                string id = node.GetAttribute("ID");
                string examName = node.SelectSingleNode("ExamName").InnerText;
                exams.Add(examName, id);
            }
            wizard.RequiredFields.AddRange("學年度", "學期", "課程名稱");
            wizard.ImportableFields.Add("總成績");
            wizard.ImportableFields.AddRange(exams.Keys);
            Dictionary<string, string> courseIDMapping = new Dictionary<string, string>();
            Dictionary<string, Dictionary<string, string>> courseStudentAttend = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, Dictionary<string, string>> courseStudentAttendScore = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, List<string>> courseExams = new Dictionary<string, List<string>>();
            Dictionary<string, ManualResetEvent> waitHandlers = new Dictionary<string, ManualResetEvent>();
            ManualResetEvent waitCourseExamLoader = null;
            wizard.PackageLimit = 320;
            MultiThreadBackgroundWorker<string> examLoader = null;
            #region 驗證資料
            MultiThreadBackgroundWorker<List<string>> loader = new MultiThreadBackgroundWorker<List<string>>();
            wizard.ValidateStart += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
            {
                lock (wizard)
                {
                    courseIDMapping.Clear();
                    foreach (var item in CourseRelated.Course.Instance.Items)
                    {
                        courseIDMapping.Add("" + item.SchoolYear + "__" + item.Semester + "__" + item.CourseName, "" + item.Identity);
                    }
                    lock (courseStudentAttendScore)
                    {
                        courseStudentAttend.Clear();
                        courseStudentAttendScore.Clear();
                    }
                    lock (courseExams)
                    {
                        courseExams.Clear();
                    }
                    waitHandlers.Clear();
                    waitCourseExamLoader = new ManualResetEvent(false);
                    var waitCourseExamLoader2 = waitCourseExamLoader;
                    if (examLoader != null && examLoader.IsBusy)
                    {
                        examLoader.CancelAsync();
                    }
                    int totalCount = courseIDMapping.Count;
                    examLoader = new MultiThreadBackgroundWorker<string>();
                    examLoader.PackageSize = 300;
                    examLoader.Loading = MultiThreadLoading.Heavy;
                    examLoader.WorkerSupportsCancellation = true;
                    examLoader.DoWork += delegate(object sender1, PackageDoWorkEventArgs<string> e1)
                    {
                        MultiThreadBackgroundWorker<string> worker = (MultiThreadBackgroundWorker<string>)sender1;
                        DSXmlHelper xmlHelper = Feature.Course.QueryCourse.GetCourseExam(new List<string>(e1.Items).ToArray()).GetContent();
                        foreach (string id in e1.Items)
                        {
                            if (worker.CancellationPending)
                            {
                                waitCourseExamLoader2.Set();
                                return;
                            }
                            var course = CourseRelated.Course.Instance.Items[id];
                            string key = "" + course.SchoolYear + "__" + course.Semester + "__" + course.CourseName;
                            courseExams.Add(key, new List<string>());
                            foreach (XmlElement element in xmlHelper.GetElements("Course[@ID='" + id + "']/ExamName"))
                            {
                                if (worker.CancellationPending)
                                {
                                    waitCourseExamLoader2.Set();
                                    return;
                                }
                                lock (courseExams)
                                {
                                    courseExams[key].Add(element.InnerText);
                                }
                            }
                        }

                        totalCount -= e1.Items.Count;
                        if (totalCount == 0)
                            waitCourseExamLoader2.Set();
                    };

                    examLoader.RunWorkerAsync(courseIDMapping.Values);

                    List<List<string>> packages = new List<List<string>>();
                    Dictionary<List<string>, ManualResetEvent> packageWaitHandlers = new Dictionary<List<string>, ManualResetEvent>();
                    List<string> currentPackage = null;
                    ManualResetEvent currentHandler = null;
                    int packageSize = 0;
                    int maxSize = 80;
                    foreach (var sid in e.List)
                    {
                        if (packageSize == 0)
                        {
                            packageSize = maxSize;
                            currentPackage = new List<string>();
                            packages.Add(currentPackage);
                            currentHandler = new ManualResetEvent(false);
                            currentHandler.Reset();
                            packageWaitHandlers.Add(currentPackage, currentHandler);
                        }
                        waitHandlers.Add(sid, currentHandler);
                        currentPackage.Add(sid);
                        packageSize--;
                    }

                    if (loader.IsBusy)
                    {
                        loader.CancelAsync();
                    }
                    loader = new MultiThreadBackgroundWorker<List<string>>();
                    loader.PackageSize = 1;
                    loader.Loading = MultiThreadLoading.Heavy;
                    loader.WorkerSupportsCancellation = true;
                    loader.DoWork += delegate(object sender1, PackageDoWorkEventArgs<List<string>> e1)
                    {
                        Dictionary<List<string>, ManualResetEvent> handlers = (Dictionary<List<string>, ManualResetEvent>)e1.Argument;
                        MultiThreadBackgroundWorker<List<string>> worker = (MultiThreadBackgroundWorker<List<string>>)sender1;
                        if (worker.CancellationPending)
                        {
                            foreach (var handler in handlers.Values)
                                handler.Set();
                            return;
                        }
                        foreach (var sidList in e1.Items)
                        {
                            if (worker.CancellationPending)
                            {
                                foreach (var handler in handlers.Values)
                                    handler.Set();
                                return;
                            }
                            #region 下載資料
                            DSXmlHelper helper = new DSXmlHelper("SelectRequest");
                            helper.AddElement("Field");
                            helper.AddElement("Field", "All");
                            helper.AddElement("Condition");
                            foreach (string cid in courseIDMapping.Values)
                            {
                                helper.AddElement("Condition", "CourseID", cid);
                            }
                            foreach (var sid in sidList)
                            {
                                helper.AddElement("Condition", "StudentID", sid);
                            }
                            helper.AddElement("Order");
                            DSRequest dsreq = new DSRequest(helper);
                            DSResponse rsp = Feature.Course.QueryCourse.GetSCAttend(new DSRequest(helper));
                            foreach (XmlElement scElement in rsp.GetContent().GetElements("Student"))
                            {
                                if (worker.CancellationPending)
                                {
                                    foreach (var handler in handlers.Values)
                                        handler.Set();
                                    return;
                                }
                                lock (courseStudentAttendScore)
                                {
                                    helper = new DSXmlHelper(scElement);
                                    string sid = helper.GetText("RefStudentID");
                                    CourseRelated.CourseInformation cinfo = CourseRelated.Course.Instance.Items[helper.GetText("RefCourseID")];
                                    string key = "" + cinfo.SchoolYear + "__" + cinfo.Semester + "__" + cinfo.CourseName;
                                    string attendID = scElement.GetAttribute("ID");
                                    if (!courseStudentAttend.ContainsKey(key))
                                        courseStudentAttend.Add(key, new Dictionary<string, string>());
                                    courseStudentAttend[key].Add(sid, attendID);
                                    if (!courseStudentAttendScore.ContainsKey(key))
                                        courseStudentAttendScore.Add(key, new Dictionary<string, string>());
                                    courseStudentAttendScore[key].Add(sid, helper.GetText("Score"));
                                }
                            }
                            #endregion
                            handlers[sidList].Set();
                        }
                    };
                    loader.RunWorkerAsync(packages, packageWaitHandlers);
                }
            };
            wizard.ValidateRow += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
            {
                #region 驗證資料
                waitHandlers[e.Data.ID].WaitOne();
                waitCourseExamLoader.WaitOne();
                var row = e.Data;
                string key = "" + row["學年度"] + "__" + row["學期"] + "__" + row["課程名稱"];
                if (courseStudentAttend.ContainsKey(key) && courseStudentAttend[key].ContainsKey(row.ID))
                {
                    if (e.SelectFields.Contains("總成績"))
                    {
                        string value = "" + row["總成績"];
                        decimal d;
                        if (value != "" && !decimal.TryParse(value, out d))
                            e.ErrorFields.Add("總成績", "總成績必需輸入數字或保留空白");
                    }
                    foreach (var exam in exams.Keys)
                    {
                        if (e.SelectFields.Contains(exam))
                        {
                            string value = "" + row[exam];
                            decimal d;
                            if (value != "" && value != "缺" && !decimal.TryParse(value, out d))
                                e.ErrorFields.Add(exam, "評量成績只允許空白、缺、或數字");
                            else if (!courseExams[key].Contains(exam) && value != "")
                            {
                                e.WarningFields.Add(exam, "該課程沒有這次考試");
                            }
                        }
                    }
                }
                else
                {
                    e.ErrorMessage = "學生沒有修這堂課";
                }
                #endregion
            };
            #endregion

            wizard.ImportPackage += delegate(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
            {
                Dictionary<string, List<RowData>> id_Rows = new Dictionary<string, List<RowData>>();
                #region 分包裝
                foreach (RowData data in e.Items)
                {
                    if (!id_Rows.ContainsKey(data.ID))
                        id_Rows.Add(data.ID, new List<RowData>());
                    id_Rows[data.ID].Add(data);
                }
                #endregion
                List<string> courseList = new List<string>();
                foreach (var key in courseStudentAttend.Keys)
                {
                    courseList.Add(courseIDMapping[key]);
                }
                var rsp = Feature.Course.QueryCourse.GetSECTake(courseList, new List<string>(id_Rows.Keys));
                bool hasInsert = false, hasUpdate = false, hasDelete = false, hasAttendUpdate = false;
                DSXmlHelper insertHelper = new DSXmlHelper("Request");
                DSXmlHelper updateHelper = new DSXmlHelper("Request");
                DSXmlHelper deleteHelper = new DSXmlHelper("Request");
                DSXmlHelper usHelper = new DSXmlHelper("Request");
                insertHelper.AddElement("ScoreSheetList");
                updateHelper.AddElement("ScoreSheetList");
                deleteHelper.AddElement("ScoreSheet");
                foreach (var row in e.Items)
                {
                    string key = "" + row["學年度"] + "__" + row["學期"] + "__" + row["課程名稱"];
                    string courseID = courseIDMapping[key];
                    if (e.ImportFields.Contains("總成績") && courseStudentAttendScore[key][row.ID] != ("" + row["總成績"]))
                    {
                        usHelper.AddElement("Attend");
                        usHelper.AddElement("Attend", "Score", "" + row["總成績"]);
                        usHelper.AddElement("Attend", "ID", courseStudentAttend[key][row.ID]);
                        hasAttendUpdate = true;
                    }
                    foreach (var examName in exams.Keys)
                    {
                        if (e.ImportFields.Contains(examName))
                        {
                            XmlElement examScoreElement = rsp.GetContent().GetElement("Score[RefStudentID='" + row.ID + "' and RefCourseID='" + courseID + "' and ExamName='" + examName + "']");
                            if (examScoreElement == null)
                            {
                                if (("" + row[examName]) != "")
                                {
                                    insertHelper.AddElement("ScoreSheetList", "ScoreSheet");
                                    insertHelper.AddElement("ScoreSheetList/ScoreSheet", "ExamID", exams[examName]);
                                    insertHelper.AddElement("ScoreSheetList/ScoreSheet", "AttendID", courseStudentAttend[key][row.ID]);
                                    insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", "" + row[examName]);
                                    hasInsert = true;
                                }
                            }
                            else
                            {
                                if (("" + row[examName]) == "")
                                {
                                    deleteHelper.AddElement("ScoreSheet", "ID", examScoreElement.GetAttribute("ID"));
                                    hasDelete = true;
                                }
                                else
                                {
                                    updateHelper.AddElement("ScoreSheetList", "ScoreSheet");
                                    updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", row[examName]);
                                    updateHelper.AddElement("ScoreSheetList/ScoreSheet", "ID", examScoreElement.GetAttribute("ID"));
                                    hasUpdate = true;
                                }
                            }
                        }
                    }
                }
                if (hasAttendUpdate)
                    EditCourse.UpdateAttend(usHelper);
                if (hasInsert)
                    EditCourse.InsertSCEScore(new DSRequest(insertHelper));
                if (hasUpdate)
                    EditCourse.UpdateSCEScore(new DSRequest(updateHelper));
                if (hasDelete)
                    EditCourse.DeleteSCEScore(new DSRequest(deleteHelper));
            };
        }
    }
}
