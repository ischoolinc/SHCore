using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool;
using System.IO;
using System.Xml;
using Aspose.Cells;
using SmartSchool.Security;
using SmartSchool.Common;
using FISCA.Presentation;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class Report : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase, SmartSchool.Customization.PlugIn.Report.IReportManager
    {
        #region FeatureAccessControl

        //課程修課學生清單	Report0290
        //FeatureAccessControl buttonItem1Ctrl;

        #endregion

        BackgroundWorker _BGWCourseAttendList;

        public Report()
        {
        }

        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Report));
            //buttonItem1Ctrl = new FeatureAccessControl("Report0290");

            var btnReports = K12.Presentation.NLDPanels.Course.RibbonBarItems["資料統計"]["報表"];
            //btnReports.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem99.Image")));
            btnReports.Image = Properties.Resources.paste_64;
            btnReports.Size = RibbonBarButton.MenuButtonSize.Large;
            btnReports.Enable = (SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count > 0);

            var btnOutputAttends = btnReports["課程修課學生清單"];
            btnOutputAttends.Click += new System.EventHandler(this.btnOutputAttends_Click);

            SmartSchool.CourseRelated.Course.Instance.SelectionChanged += delegate
            {
                bool selected = (SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count > 0);
                btnReports.Enable = selected;
                btnOutputAttends.Enable = selected && CurrentUser.Acl["Report0290"].Executable;
            };

            reportManager = new Adaatper.ButtonAdapterPlugInToMenuButton(btnReports);
            SmartSchool.Customization.PlugIn.Report.CourseReport.SetManager(this);

        }

        #region IReportManager 成員

        private Adaatper.ButtonAdapterPlugInToMenuButton reportManager;
        public void AddButton(SmartSchool.Customization.PlugIn.ButtonAdapter button)
        {
            reportManager.Add(button);
        }

        public void RemoveButton(SmartSchool.Customization.PlugIn.ButtonAdapter button)
        {
            reportManager.Remove(button);
        }
        #endregion


        void Report_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MotherForm.SetStatusBarMessage("" + e.UserState + "產生中...", e.ProgressPercentage);
        }

        void Report_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string reportName;
            string path;
            Workbook wb;

            object[] result = (object[])e.Result;
            reportName = (string)result[0];
            path = (string)result[1];
            wb = (Workbook)result[2];

            if (File.Exists(path))
            {
                int i = 1;
                while (true)
                {
                    string newPath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + (i++) + Path.GetExtension(path);
                    if (!File.Exists(newPath))
                    {
                        path = newPath;
                        break;
                    }
                }
            }

            try
            {
                wb.Save(path, FileFormatType.Excel2003);
                MotherForm.SetStatusBarMessage(reportName + "產生完成");
                System.Diagnostics.Process.Start(path);
            }
            catch
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Title = "另存新檔";
                sd.FileName = reportName + ".xls";
                sd.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";
                if (sd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        wb.Save(sd.FileName, FileFormatType.Excel2003);
                    }
                    catch
                    {
                        MsgBox.Show("指定路徑無法存取。", "建立檔案失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
        }

        #region 課程修課學生清單
        private void btnOutputAttends_Click(object sender, EventArgs e)
        {
            if (SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count == 0)
                return;

            MotherForm.SetStatusBarMessage("正在初始化課程修課學生清單...");

            _BGWCourseAttendList = new BackgroundWorker();
            _BGWCourseAttendList.DoWork += new DoWorkEventHandler(_BGWCourseAttendList_DoWork);
            _BGWCourseAttendList.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Report_RunWorkerCompleted);
            _BGWCourseAttendList.ProgressChanged += new ProgressChangedEventHandler(Report_ProgressChanged);
            _BGWCourseAttendList.WorkerReportsProgress = true;
            _BGWCourseAttendList.RunWorkerAsync();
        }

        void _BGWCourseAttendList_DoWork(object sender, DoWorkEventArgs e)
        {
            string reportName = "課程修課學生清單";

            #region 快取所需要的資訊

            List<SmartSchool.CourseRelated.CourseInformation> selectedCourses = SmartSchool.CourseRelated.Course.Instance.SelectionCourse;
            Dictionary<string, List<AttendInfo>> attendList = new Dictionary<string, List<AttendInfo>>();

            List<string> allCourseId = new List<string>();

            int currentStudentNumber = 0;
            int allStudentNumber = 0;

            foreach (SmartSchool.CourseRelated.CourseInformation var in selectedCourses)
            {
                allCourseId.Add(var.Identity.ToString());
            }

            foreach (XmlElement var in (SmartSchool.Feature.Course.QueryCourse.GetSCAttend(allCourseId.ToArray())).GetContent().GetElements("Student"))
            {
                string courseId = var.SelectSingleNode("RefCourseID").InnerText;
                string studentId = var.SelectSingleNode("RefStudentID").InnerText;

                AttendInfo attendInfo = new AttendInfo(var.SelectSingleNode("ClassName").InnerText,
                    var.SelectSingleNode("SeatNumber").InnerText,
                    var.SelectSingleNode("StudentNumber").InnerText,
                    var.SelectSingleNode("Name").InnerText,
                    var.SelectSingleNode("IsRequired").InnerText,
                    var.SelectSingleNode("RequiredBy").InnerText);

                if (!attendList.ContainsKey(courseId))
                {
                    attendList.Add(courseId, new List<AttendInfo>());
                }

                if (SmartSchool.StudentRelated.Student.Instance.Items[studentId] != null && SmartSchool.StudentRelated.Student.Instance.Items[studentId].IsNormal)
                {
                    attendList[courseId].Add(attendInfo);
                    allStudentNumber++;
                }
            }

            #endregion

            #region 產生報表

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.課程修課學生清單), FileFormatType.Excel2003);

            Range tempRange = template.Worksheets[0].Cells.CreateRange(0, 53, false);

            Dictionary<string, int> sheets = new Dictionary<string, int>();
            Dictionary<string, int> sheetRowDict = new Dictionary<string, int>();

            Workbook wb = new Workbook();
            wb.Open(new MemoryStream(Properties.Resources.課程修課學生清單), FileFormatType.Excel2003);

            Worksheet currentWorksheet;
            wb.Worksheets.Clear();

            int sheetRowIndex;
            int pageRow = 50;
            int pageCol = 8;
            int pageData = 45;

            foreach (SmartSchool.CourseRelated.CourseInformation var in selectedCourses)
            {
                string subject;
                if (var.Subject != "")
                {
                    subject = var.Subject;
                    subject = subject.Replace('/', '_');
                }
                else
                    subject = "未分科目";

                List<AttendInfo> students = (attendList.ContainsKey(var.Identity.ToString())) ? attendList[var.Identity.ToString()] : new List<AttendInfo>();

                if (!sheets.ContainsKey(subject))
                {
                    int index;
                    //新增 sheet
                    index = wb.Worksheets.Add();
                    wb.Worksheets[index].Name = subject;
                    //sheet 列印設定
                    wb.Worksheets[index].PageSetup.Orientation = PageOrientationType.Portrait;
                    wb.Worksheets[index].PageSetup.TopMargin = 0.5;
                    wb.Worksheets[index].PageSetup.RightMargin = 0.8;
                    wb.Worksheets[index].PageSetup.BottomMargin = 0.5;
                    wb.Worksheets[index].PageSetup.LeftMargin = 0.8;
                    wb.Worksheets[index].PageSetup.CenterHorizontally = true;
                    wb.Worksheets[index].PageSetup.HeaderMargin = 0.0;
                    wb.Worksheets[index].PageSetup.FooterMargin = 0.0;

                    sheets.Add(subject, index);
                    sheetRowDict.Add(subject, 0);

                    //複製 Template Column 寬度
                    for (int i = 0; i < pageCol; i++)
                    {
                        wb.Worksheets[index].Cells.CopyColumn(template.Worksheets[0].Cells, i, i);
                    }
                }

                //指定 sheet
                currentWorksheet = wb.Worksheets[sheets[subject]];
                sheetRowIndex = sheetRowDict[subject];

                int currentPage = 1;
                int totalPage = (int)Math.Ceiling(((double)students.Count / (double)pageData));

                //至少會產生空白頁，頁數為1
                if (totalPage <= 0)
                    totalPage = 1;

                int studentCount = 0;

                if (students.Count > 0)
                {
                    while (studentCount < students.Count)
                    {
                        //複製 Template
                        currentWorksheet.Cells.CreateRange(sheetRowIndex, pageRow, false).Copy(tempRange);

                        currentWorksheet.Cells[sheetRowIndex, 0].PutValue(var.SchoolYear + " 學年度 第 " + var.Semester + " 學期 課程修課學生清單");
                        currentWorksheet.Cells[sheetRowIndex + 1, 1].PutValue(var.CourseName);
                        currentWorksheet.Cells[sheetRowIndex + 1, 5].PutValue(var.Credit);
                        currentWorksheet.Cells[sheetRowIndex + 1, 7].PutValue(var.MajorTeacherName);
                        currentWorksheet.Cells[sheetRowIndex + 2, 1].PutValue(var.Subject);
                        currentWorksheet.Cells[sheetRowIndex + 2, 5].PutValue(var.SubjectLevel);
                        currentWorksheet.Cells[sheetRowIndex + 2, 7].PutValue(students.Count);

                        int dataIndex = sheetRowIndex + 4;

                        for (int i = 0; i < pageData && studentCount < students.Count; studentCount++, i++)
                        {
                            currentWorksheet.Cells[dataIndex + i, 0].PutValue(students[studentCount].ClassName);
                            currentWorksheet.Cells[dataIndex + i, 1].PutValue(students[studentCount].SeatNumber);
                            currentWorksheet.Cells[dataIndex + i, 2].PutValue(students[studentCount].StudentNumber);
                            currentWorksheet.Cells[dataIndex + i, 3].PutValue(students[studentCount].Name);
                            currentWorksheet.Cells[dataIndex + i, 5].PutValue(students[studentCount].IsRequired + "修");
                            currentWorksheet.Cells[dataIndex + i, 6].PutValue(students[studentCount].RequiredBy);
                            //回報進度
                            _BGWCourseAttendList.ReportProgress((int)(((double)++currentStudentNumber * 100.0) / (double)allStudentNumber), reportName);
                        }


                        sheetRowIndex += pageRow;

                        //填寫頁數
                        currentWorksheet.Cells[sheetRowIndex - 1, 6].PutValue("第 " + (currentPage++) + " 頁 / 共 " + totalPage + " 頁");

                        //設定分頁
                        currentWorksheet.HPageBreaks.Add(sheetRowIndex, pageCol);

                    }
                }
                else
                {
                    //複製 Template
                    currentWorksheet.Cells.CreateRange(sheetRowIndex, pageRow, false).Copy(tempRange);

                    currentWorksheet.Cells[sheetRowIndex, 0].PutValue(var.SchoolYear + " 學年度 第 " + var.Semester + " 學期 課程修課學生清單");
                    currentWorksheet.Cells[sheetRowIndex + 1, 1].PutValue(var.CourseName);
                    currentWorksheet.Cells[sheetRowIndex + 1, 5].PutValue(var.Credit);
                    currentWorksheet.Cells[sheetRowIndex + 1, 7].PutValue(var.MajorTeacherName);
                    currentWorksheet.Cells[sheetRowIndex + 2, 1].PutValue(var.Subject);
                    currentWorksheet.Cells[sheetRowIndex + 2, 5].PutValue(var.SubjectLevel);
                    currentWorksheet.Cells[sheetRowIndex + 2, 7].PutValue(students.Count);

                    sheetRowIndex += pageRow;

                    //填寫頁數
                    currentWorksheet.Cells[sheetRowIndex - 1, 6].PutValue("第 " + (currentPage++) + " 頁 / 共 " + totalPage + " 頁");

                    //設定分頁
                    currentWorksheet.HPageBreaks.Add(sheetRowIndex, pageCol);
                }

                sheetRowDict[subject] = sheetRowIndex;
            }

            wb.Worksheets.SortNames();

            #endregion

            string path = Path.Combine(Application.StartupPath, "Reports");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xlt");
            e.Result = new object[] { reportName, path, wb };
        }
        #endregion

    }

    //用來暫存學生修課紀錄
    internal class AttendInfo
    {
        private string _className;
        private string _seatNumber;
        private string _studentNumber;
        private string _name;
        private string _isRequired;
        private string _requiredBy;

        public AttendInfo(string className, string seatNumber, string studentNumber, string name, string isRequired, string requiredBy)
        {
            _className = className;
            _seatNumber = seatNumber;
            _studentNumber = studentNumber;
            _name = name;
            _isRequired = isRequired;
            _requiredBy = requiredBy;
        }

        public string ClassName
        {
            get { return _className; }
        }
        public string SeatNumber
        {
            get { return _seatNumber; }
        }
        public string StudentNumber
        {
            get { return _studentNumber; }
        }
        public string Name
        {
            get { return _name; }
        }
        public string IsRequired
        {
            get { return _isRequired; }
        }
        public string RequiredBy
        {
            get { return _requiredBy; }
        }
    }
}