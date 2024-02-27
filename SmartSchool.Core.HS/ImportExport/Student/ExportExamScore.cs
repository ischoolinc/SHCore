using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.API.PlugIn;
using SmartSchool.AccessControl;

namespace SmartSchool.ImportExport.Student
{
    [FeatureCode("高中系統/匯出評量成績")]
    class ExportExamScore : SmartSchool.API.PlugIn.Export.Exporter
    {
        public ExportExamScore()
        {
            this.Image = null;
            this.Text = "匯出評量成績";
        }
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            SmartSchool.API.PlugIn.VirtualRadioButton exportCurrentSemester = new SmartSchool.API.PlugIn.VirtualRadioButton("僅匯出本學期", true);
            wizard.Options.Add(exportCurrentSemester);
            SmartSchool.API.PlugIn.VirtualRadioButton exportPreSemester = new SmartSchool.API.PlugIn.VirtualRadioButton("僅匯出上學期", false);
            wizard.Options.Add(exportPreSemester);
            SmartSchool.API.PlugIn.VirtualRadioButton exportAll = new SmartSchool.API.PlugIn.VirtualRadioButton("匯出所有學期", false);
            wizard.Options.Add(exportAll);
            wizard.PackageLimit = 85;
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
            // 2018.09.06 [ischoolKingdom] Vicky依據 [12-01][01] 多學期成績排名 項目，增加 "分項類別", "科目", "學分" 項目資料。
            List<string> fieldList = new List<string>(new string[] { "學年度", "學期", "課程名稱", "領域", "分項類別", "科目", "學分", "總成績" });
            fieldList.AddRange(exams.Keys);
            wizard.ExportableFields.AddRange(fieldList);
            bool courseCkecked = false;
            List<string> courses = new List<string>();
            List<string> checkedCourses = new List<string>();
            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                lock (sender)
                {
                    if (!courseCkecked)
                    {
                        #region 依選項填入所有可能的課程ID
                        foreach (var item in CourseRelated.Course.Instance.Items)
                        {
                            if (exportAll.Checked)//全部
                            { courses.Add("" + item.Identity); }
                            else if (exportCurrentSemester.Checked)//僅本學期
                            {
                                if (item.SchoolYear == CurrentUser.Instance.SchoolYear && item.Semester == CurrentUser.Instance.Semester)
                                {
                                    courses.Add("" + item.Identity);
                                }
                            }
                            else if (exportPreSemester.Checked)//僅上學期
                            {
                                if (CurrentUser.Instance.Semester == 2)
                                {
                                    if (item.SchoolYear == CurrentUser.Instance.SchoolYear && item.Semester == 1)
                                    {
                                        courses.Add("" + item.Identity);
                                    }
                                }
                                else
                                {
                                    if (item.SchoolYear == CurrentUser.Instance.SchoolYear - 1 && item.Semester == 2)
                                    {
                                        courses.Add("" + item.Identity);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region 先將所有可能課程中有選取試別的加入attendRow
                        foreach (List<string> courseList in SplitPackage<string>(courses))
                        {
                            DSXmlHelper helper = Feature.Course.QueryCourse.GetCourseExam(courseList.ToArray()).GetContent();
                            foreach (string var in courseList)
                            {
                                foreach (XmlElement element in helper.GetElements("Course[@ID='" + var + "']/ExamName"))
                                {
                                    if (e.ExportFields.Contains(element.InnerText))
                                    {
                                        checkedCourses.Add(var);
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion
                        courseCkecked = true;
                    }
                }
                Dictionary<string, RowData> attendRow = new Dictionary<string, RowData>();
                if (courses.Count > 0)
                {
                    #region checkedCourse中只要學生有修課就加入attendRow(不管是否存在該課程的評量記錄，該修課都會被列出)

                    #region 取得修課資料
                    DSXmlHelper helper = new DSXmlHelper("SelectRequest");
                    helper.AddElement("Field");
                    helper.AddElement("Field", "All");
                    helper.AddElement("Condition");
                    foreach (string cid in checkedCourses)
                    {
                        helper.AddElement("Condition", "CourseID", cid);
                    }
                    foreach (string sid in e.List)
                    {
                        helper.AddElement("Condition", "StudentID", sid);
                    }
                    helper.AddElement("Order");
                    DSRequest dsreq = new DSRequest(helper);
                    DSResponse rsp = Feature.Course.QueryCourse.GetSCAttend(new DSRequest(helper));
                    #endregion
                    foreach (string studentID in e.List)
                    {
                        foreach (string cid in checkedCourses)
                        {
                            XmlElement scElement = rsp.GetContent().GetElement("Student[RefStudentID='" + studentID + "' and RefCourseID='" + cid + "']");
                            if (scElement != null)
                            {
                                var cinfo = CourseRelated.Course.Instance[cid];
                                string key = studentID + "__" + cinfo.Identity;
                                RowData row;
                                #region 取回已建立的修課記錄或新增修課記錄
                                if (!attendRow.ContainsKey(key))
                                {
                                    row = new RowData();
                                    row.ID = studentID;
                                    attendRow.Add(key, row);
                                    if (e.ExportFields.Contains("學年度"))
                                    {
                                        row.Add("學年度", "" + cinfo.SchoolYear);
                                    }
                                    if (e.ExportFields.Contains("學期"))
                                    {
                                        row.Add("學期", "" + cinfo.Semester);
                                    }
                                    if (e.ExportFields.Contains("課程名稱"))
                                    {
                                        row.Add("課程名稱", cinfo.CourseName);                                     
                                    }
                                    if (e.ExportFields.Contains("領域"))
                                    {
                                        row.Add("領域", cinfo.Domain);
                                    }
                                    // 2018.09.06 [ischoolKingdom] Vicky依據 [12-01][01] 多學期成績排名 項目，增加 "分項類別", "科目", "學分" 項目資料。
                                    if (e.ExportFields.Contains("分項類別"))
                                    {
                                        row.Add("分項類別", cinfo.Entry);
                                    }
                                    if (e.ExportFields.Contains("科目"))
                                    {
                                        row.Add("科目", cinfo.Subject);
                                    }
                                    if (e.ExportFields.Contains("學分"))
                                    {
                                        row.Add("學分", "" + cinfo.CreditDec);
                                    }
                                    if (e.ExportFields.Contains("總成績"))
                                    {
                                        XmlElement scoreElement = (XmlElement)scElement.SelectSingleNode("Score");
                                        row.Add("總成績", scoreElement == null ? "" : scoreElement.InnerText);
                                    }
                                    

                                }
                                #endregion
                            }
                        }
                    }
                    #endregion

                    List<string> studentid = new List<string>(e.List);
                    rsp = Feature.Course.QueryCourse.GetSECTake(courses, studentid);
                    foreach (XmlElement scElement in rsp.GetContent().GetElements("Score"))
                    {
                        helper = new DSXmlHelper(scElement);
                        StudentRelated.BriefStudentData sinfo = StudentRelated.Student.Instance.Items[helper.GetText("RefStudentID")];
                        CourseRelated.CourseInformation cinfo = CourseRelated.Course.Instance.Items[helper.GetText("RefCourseID")];
                        if (sinfo != null && cinfo != null)
                        {
                            string examName = helper.GetText("ExamName");
                            string score = helper.GetText("Score");

                            // 因為有使用缺考設定寫入UseText，先判斷再判斷Score
                            string UseText = helper.GetText("Extension/Extension/UseText");
                            if (!string.IsNullOrEmpty(UseText))
                                score = UseText;

                            if (e.ExportFields.Contains(examName))
                            {
                                string key = sinfo.ID + "__" + cinfo.Identity;
                                RowData row;
                                #region 取回已建立的修課記錄或新增修課記錄
                                if (!attendRow.ContainsKey(key))
                                {
                                    row = new RowData();
                                    row.ID = sinfo.ID;
                                    attendRow.Add(key, row);
                                    if (e.ExportFields.Contains("學年度"))
                                    {
                                        row.Add("學年度", "" + cinfo.SchoolYear);
                                    }
                                    if (e.ExportFields.Contains("學期"))
                                    {
                                        row.Add("學期", "" + cinfo.Semester);
                                    }
                                    if (e.ExportFields.Contains("課程名稱"))
                                    {
                                        row.Add("課程名稱", cinfo.CourseName);
                                    }
                                    if (e.ExportFields.Contains("總成績"))
                                    {
                                        row.Add("總成績", "");
                                    }
                                }
                                else
                                {
                                    row = attendRow[key];
                                }
                                #endregion
                                if (!row.ContainsKey(examName))
                                    row.Add(examName, score);
                                else
                                    row[examName] = score;
                            }
                        }
                    }
                }
                foreach (var item in attendRow.Values)
                {
                    e.Items.Add(item);
                }
            };
        }


        private static List<T>[] SplitPackage<T>(List<T> list)
        {
            int _PackageLimit = 200;
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
    }
}
