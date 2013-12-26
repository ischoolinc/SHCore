using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Aspose.Cells;
using System.IO;
using SmartSchool.Common;
using SmartSchool;
using FISCA.DSAUtil;
using System.Xml;
using SmartSchool.ClassRelated.RibbonBars.Reports;
using System.Threading;
using AW = Aspose.Words;
using SmartSchool.ClassRelated;
using SmartSchool.ClassRelated.RibbonBars.DeXing;
using SmartSchool.StudentRelated;
//using SmartSchool.SmartPlugIn.Common;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Security;
using FISCA.Presentation;

namespace SmartSchool.ClassRelated.RibbonBars
{
    public partial class Report : RibbonBarBase, SmartSchool.Customization.PlugIn.Report.IReportManager
    {
        #region FeatureAccessControl

        //統計
        FeatureAccessControl statisticsCtrl;

        #region 報表 (非Plugin)

        //班級名條	Report0070
        FeatureAccessControl buttonItem2Ctrl;

        //班級點名表	Report0090
        FeatureAccessControl buttonItem8Ctrl;
        //班級通訊錄	Report0100
        FeatureAccessControl buttonItem1Ctrl;

        //班級考試成績單	Report0110
        FeatureAccessControl buttonItem7Ctrl;
        //班級考試成績單(Word)	Report0120
        FeatureAccessControl buttonItem3Ctrl;
        //德行表現特殊學生名單	Report0190
        FeatureAccessControl btnSearchAttendanceCtrl;
        //缺曠通知單	Report0200
        //FeatureAccessControl buttonItem13Ctrl;
        //獎懲通知單	Report0210
        //FeatureAccessControl buttonItem10Ctrl;
        //班級學生缺曠明細	Report0220
        FeatureAccessControl buttonItem9Ctrl;
        //班級學生獎懲明細	Report0230
        //FeatureAccessControl buttonItem11Ctrl;
        //缺曠週報表 (依缺曠別統計)	Report0240
        //FeatureAccessControl buttonItem4Ctrl;
        //缺曠週報表 (依節次統計)	Report0250
        //FeatureAccessControl buttonItem6Ctrl;
        //獎懲週報表	Report0260
        //FeatureAccessControl buttonItem5Ctrl;

        #endregion

        #endregion

        #region BackgroundWorker

        BackgroundWorker _BGWClassContactList;
        BackgroundWorker _BGWClassNameList;
        BackgroundWorker _BGWAbsenceWeekListByAbsence;
        BackgroundWorker _BGWAbsenceWeekListByPeriod;
        BackgroundWorker _BGWDisciplineWeekList;
        BackgroundWorker _BGWClassStudentAttendance;
        BackgroundWorker _BGWClassStudentAbsenceDetail;


        BackgroundWorker _BGWClassStudentDisciplineDetail;

        #endregion

        private Adaatper.ButtonAdapterPlugInToMenuButton reportManager;

        public Report()
        {
            //InitializeComponent();

            ////SmartSchool.ClassRelated.Class.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);
            //SmartSchool.Broadcaster.Events.Items["班級/選取變更"].Handler += delegate
            //{
            //    buttonItem131.Enabled = (SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0);
            //};
            //#region 設定為班級的報表外掛處理者
            //reportManager = new ButtonAdapterPlugInManager(this.buttonItem131);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonAdapter>.Instance.Add("班級/統計報表/報表", reportManager);
            //SmartSchool.Customization.PlugIn.Report.ClassReport.SetManager(this);
            //#endregion
            //#region 設定為班級的統計外掛處理者
            //ButtonAdapterPlugInManager 統計管理 = new ButtonAdapterPlugInManager(btnStatistics);
            //統計管理.ItemsChanged += new EventHandler(統計管理_ItemsChanged);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonAdapter>.Instance.Add("班級/統計報表/統計", 統計管理);
            //#endregion

            //statisticsCtrl = new FeatureAccessControl("Button0410");
            //statisticsCtrl.Inspect(btnStatistics);

            ////班級名條	Report0070
            //buttonItem2Ctrl = new FeatureAccessControl("Report0070");

            ////班級點名表	Report0090
            //buttonItem8Ctrl = new FeatureAccessControl("Report0090");
            ////班級通訊錄	Report0100
            //buttonItem1Ctrl = new FeatureAccessControl("Report0100");

            ////班級考試成績單	Report0110
            //buttonItem7Ctrl = new FeatureAccessControl("Report0110");
            ////班級考試成績單(Word)	Report0120
            //buttonItem3Ctrl = new FeatureAccessControl("Report0120");
            ////德行表現特殊學生名單	Report0190
            //btnSearchAttendanceCtrl = new FeatureAccessControl("Report0190");
            ////缺曠通知單	Report0200
            //buttonItem13Ctrl = new FeatureAccessControl("Report0200");
            ////獎懲通知單	Report0210
            //buttonItem10Ctrl = new FeatureAccessControl("Report0210");
            ////班級學生缺曠明細	Report0220
            //buttonItem9Ctrl = new FeatureAccessControl("Report0220");
            ////班級學生獎懲明細	Report0230
            //buttonItem11Ctrl = new FeatureAccessControl("Report0230");

            //buttonItem2Ctrl.Inspect(buttonItem2);
            //buttonItem8Ctrl.Inspect(buttonItem8);
            //buttonItem1Ctrl.Inspect(buttonItem1);

            //buttonItem7Ctrl.Inspect(buttonItem7);
            //buttonItem3Ctrl.Inspect(buttonItem3);
            //btnSearchAttendanceCtrl.Inspect(btnSearchAttendance);
            //buttonItem13Ctrl.Inspect(buttonItem13);
            //buttonItem10Ctrl.Inspect(buttonItem10);
            //buttonItem9Ctrl.Inspect(buttonItem9);
            //buttonItem11Ctrl.Inspect(buttonItem11);
            //buttonItem4Ctrl.Inspect(buttonItem4);
            //buttonItem6Ctrl.Inspect(buttonItem6);
            //buttonItem5Ctrl.Inspect(buttonItem5);
        }

        internal void Setup()
        {
            statisticsCtrl = new FeatureAccessControl("Button0410");
            //班級名條	Report0070
            buttonItem2Ctrl = new FeatureAccessControl("Report0070");
            //班級點名表	Report0090
            buttonItem8Ctrl = new FeatureAccessControl("Report0090");
            //班級通訊錄	Report0100
            buttonItem1Ctrl = new FeatureAccessControl("Report0100");
            //班級考試成績單	Report0110
            buttonItem7Ctrl = new FeatureAccessControl("Report0110");
            //班級考試成績單(Word)	Report0120
            buttonItem3Ctrl = new FeatureAccessControl("Report0120");
            //德行表現特殊學生名單	Report0190
            //btnSearchAttendanceCtrl = new FeatureAccessControl("Report0190");
            //缺曠通知單	Report0200
            //buttonItem13Ctrl = new FeatureAccessControl("Report0200");
            //獎懲通知單	Report0210
            //buttonItem10Ctrl = new FeatureAccessControl("Report0210");
            //班級學生缺曠明細	Report0220
            buttonItem9Ctrl = new FeatureAccessControl("Report0220");

            //20130114 - 拿掉
            //班級學生獎懲明細	Report0230
            //buttonItem11Ctrl = new FeatureAccessControl("Report0230");

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Report));
            var Bar = K12.Presentation.NLDPanels.Class.RibbonBarItems["資料統計"];

            //var btnStatistics = Bar["統計"];
            //btnStatistics.Image = ( (System.Drawing.Image)( resources.GetObject("btnStatistics.Image") ) );
            //btnStatistics.Enable = false;

            var buttonItem131 = Bar["報表"];
            buttonItem131.Image = Properties.Resources.paste_64;
            buttonItem131.Enable = false;
            buttonItem131.Size = RibbonBarButton.MenuButtonSize.Large;

            K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
            {
                buttonItem131.Enable = ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 );
                //var hasChild = false;
                //foreach ( var item in btnStatistics.Items )
                //{
                //    hasChild = true;
                //}
                //btnStatistics.Enable = ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 ) && statisticsCtrl.Executable() && hasChild;
            };

            #region 設定為班級的報表外掛處理者
            reportManager = new Adaatper.ButtonAdapterPlugInToMenuButton(buttonItem131);
            SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonAdapter>.Instance.Add("班級/統計報表/報表", reportManager);
            SmartSchool.Customization.PlugIn.Report.ClassReport.SetManager(this);
            #endregion
            #region 設定為班級的統計外掛處理者
            //Adaatper.ButtonAdapterPlugInToMenuButton 統計管理 = new Adaatper.ButtonAdapterPlugInToMenuButton(btnStatistics);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonAdapter>.Instance.Add("班級/統計報表/統計", 統計管理);
            //統計管理.Changed += delegate
            //{
            //    var hasChild = false;
            //    foreach ( var item in btnStatistics.Items )
            //    {
            //        hasChild = true;
            //    }
            //    btnStatistics.Enable = ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count > 0 ) && statisticsCtrl.Executable() && hasChild;
            //};
            #endregion

            var buttonItem14 = buttonItem131["學籍相關報表"];
            var buttonItem15 = buttonItem131["成績相關報表"];
            var buttonItem16 = buttonItem131["學務相關報表"];

            var buttonItem2 = buttonItem14["班級名條"];
            buttonItem2.Click += new System.EventHandler(this.buttonItem2_Click);
            buttonItem2.Enable = buttonItem2Ctrl.Executable();
            var buttonItem8 = buttonItem14["班級點名表"];
            buttonItem8.Click += new System.EventHandler(this.buttonItem8_Click);
            buttonItem8.Enable = buttonItem8Ctrl.Executable();
            var buttonItem1 = buttonItem14["班級通訊錄"];
            buttonItem1.Click += new System.EventHandler(this.buttonItem1_Click);
            buttonItem1.Enable = buttonItem1Ctrl.Executable();


            var buttonItem7 = buttonItem15["班級考試成績單"];
            buttonItem7.Click += new System.EventHandler(this.buttonItem7_Click);
            buttonItem7.Enable = buttonItem7Ctrl.Executable();
            var buttonItem3 = buttonItem15["班級考試成績單(Word)"];
            buttonItem3.Click += new System.EventHandler(this.buttonItem3_Click);
            buttonItem3.Enable = buttonItem3Ctrl.Executable();

            //var btnSearchAttendance = buttonItem16["德行表現特殊學生名單"];
            //btnSearchAttendance.Click += new System.EventHandler(this.btnSearchAttendance_Click);
            //btnSearchAttendance.Enable = btnSearchAttendanceCtrl.Executable();
            //var buttonItem13 = buttonItem16["缺曠通知單"];
            //buttonItem13.Click += new System.EventHandler(this.buttonItem13_Click);
            //buttonItem13.Enable = buttonItem13Ctrl.Executable();
            //var buttonItem10 = buttonItem16["獎懲通知單"];
            //buttonItem10.Click += new System.EventHandler(this.buttonItem10_Click);
            //buttonItem10.Enable = buttonItem10Ctrl.Executable();
            var buttonItem9 = buttonItem16["班級學生缺曠明細"];
            buttonItem9.Click += new System.EventHandler(this.buttonItem9_Click);
            buttonItem9.Enable = buttonItem9Ctrl.Executable();
            var buttonItem11 = buttonItem16["班級學生獎懲明細"];
            buttonItem11.Click += new System.EventHandler(this.buttonItem11_Click);

        }

        #region IReportManager 成員

        public void AddButton(SmartSchool.Customization.PlugIn.ButtonAdapter button)
        {
            reportManager.Add(button);
        }

        public void RemoveButton(SmartSchool.Customization.PlugIn.ButtonAdapter button)
        {
            reportManager.Remove(button);
        }

        #endregion

        #region 班級通訊錄
        private void buttonItem1_Click(object sender, EventArgs e)
        {
            if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 0 )
                return;

            MotherForm.SetStatusBarMessage("正在初始化班級通訊錄...");

            _BGWClassContactList = new BackgroundWorker();
            _BGWClassContactList.DoWork += new DoWorkEventHandler(_BGWClassContactList_DoWork);
            _BGWClassContactList.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CommonMethods.ExcelReport_RunWorkerCompleted);
            _BGWClassContactList.ProgressChanged += new ProgressChangedEventHandler(CommonMethods.Report_ProgressChanged);
            _BGWClassContactList.WorkerReportsProgress = true;
            _BGWClassContactList.RunWorkerAsync();
        }

        void _BGWClassContactList_DoWork(object sender, DoWorkEventArgs e)
        {
            string reportName = "班級通訊錄";

            #region 快取所需要的資訊

            List<SmartSchool.ClassRelated.ClassInfo> selectedClasses = SmartSchool.ClassRelated.Class.Instance.SelectionClasses;

            Dictionary<string, string> custodianName = new Dictionary<string, string>();
            Dictionary<string, string> mailingAddress = new Dictionary<string, string>();
            Dictionary<string, string> smsPhone = new Dictionary<string, string>();

            List<string> allClassId = new List<string>();

            int currentStudentNumber = 0;
            int allStudentNumber = 0;

            foreach ( SmartSchool.ClassRelated.ClassInfo var in selectedClasses )
            {
                allStudentNumber += var.Students.Count;
                allClassId.Add(var.ClassID);
            }

            foreach ( XmlElement element in ( SmartSchool.Feature.QueryStudent.GetDetailListByClassID(allClassId.ToArray()) as DSResponse ).GetContent().GetElements("Student") )
            {
                string studentId = element.SelectSingleNode("@ID").InnerText;
                StringBuilder mailingAddressString = new StringBuilder();

                XmlElement addressElement = (XmlElement)element.SelectSingleNode("MailingAddress/AddressList/Address");
                if ( addressElement != null )
                {
                    mailingAddressString.Append(addressElement.SelectSingleNode("ZipCode").InnerText);
                    mailingAddressString.Append(" ");
                    mailingAddressString.Append(addressElement.SelectSingleNode("County").InnerText);
                    mailingAddressString.Append(addressElement.SelectSingleNode("Town").InnerText);
                    mailingAddressString.Append(addressElement.SelectSingleNode("DetailAddress").InnerText);
                }

                custodianName.Add(studentId, element.SelectSingleNode("CustodianName").InnerText);
                mailingAddress.Add(studentId, mailingAddressString.ToString());

                smsPhone.Add(studentId, element.SelectSingleNode("SMSPhone").InnerText);
            }

            #endregion

            #region 產生報表

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.班級通訊錄), FileFormatType.Excel2003);

            Range tempRange = template.Worksheets[0].Cells.CreateRange(0, 32, false);

            Dictionary<string, int> sheets = new Dictionary<string, int>();
            Dictionary<string, int> sheetPageIndex = new Dictionary<string, int>();

            Workbook wb = new Workbook();
            wb.Open(new MemoryStream(Properties.Resources.班級通訊錄), FileFormatType.Excel2003);

            Worksheet currentWorksheet;
            wb.Worksheets.Clear();

            int sheetIndex;
            int pageRow = 32;
            int pageCol = 8;
            int pageData = 28;

            foreach ( SmartSchool.ClassRelated.ClassInfo var in selectedClasses )
            {
                string gradeYear = var.GradeYear;
                List<SmartSchool.StudentRelated.BriefStudentData> classStudent = var.Students;

                if ( !sheets.ContainsKey(gradeYear) )
                {
                    int index;
                    int a;
                    //新增 sheet
                    index = wb.Worksheets.Add();
                    if ( int.TryParse(gradeYear, out a) )
                        wb.Worksheets[index].Name = gradeYear + " 年級";
                    else
                        wb.Worksheets[index].Name = gradeYear;
                    //sheet 列印設定
                    wb.Worksheets[index].PageSetup.Orientation = PageOrientationType.Landscape;
                    wb.Worksheets[index].PageSetup.TopMargin = 1.0;
                    wb.Worksheets[index].PageSetup.RightMargin = 0.6;
                    wb.Worksheets[index].PageSetup.BottomMargin = 1.0;
                    wb.Worksheets[index].PageSetup.LeftMargin = 0.6;
                    wb.Worksheets[index].PageSetup.CenterHorizontally = true;
                    wb.Worksheets[index].PageSetup.HeaderMargin = 0.8;
                    wb.Worksheets[index].PageSetup.FooterMargin = 0.8;
                    sheets.Add(gradeYear, index);
                    sheetPageIndex.Add(gradeYear, 0);

                    //複製 Template Column 寬度
                    for ( int i = 0 ; i < pageCol ; i++ )
                    {
                        wb.Worksheets[index].Cells.CopyColumn(template.Worksheets[0].Cells, i, i);
                    }
                }

                //指定 sheet
                currentWorksheet = wb.Worksheets[sheets[gradeYear]];
                sheetIndex = sheetPageIndex[gradeYear];

                int currentPage = 1;
                int totalPage = (int)Math.Ceiling(( (double)classStudent.Count / (double)pageData ));

                int studentCount = 0;

                for ( ; currentPage <= totalPage ; currentPage++ )
                {
                    //複製 Template
                    currentWorksheet.Cells.CreateRange(sheetIndex, pageRow, false).Copy(tempRange);

                    //填寫班級基本資料
                    currentWorksheet.Cells[sheetIndex, 0].PutValue(CurrentUser.Instance.SchoolChineseName + " 學生通訊錄");
                    currentWorksheet.Cells[sheetIndex + 1, 1].PutValue(var.ClassName);
                    currentWorksheet.Cells[sheetIndex + 1, 6].PutValue(var.TeacherName);

                    int dataIndex = sheetIndex + 3;

                    //填寫學生資料
                    for ( int i = 0 ; i < pageData && studentCount < classStudent.Count ; studentCount++, i++ )
                    {
                        currentStudentNumber++;

                        currentWorksheet.Cells[dataIndex, 0].PutValue(classStudent[studentCount].StudentNumber);
                        currentWorksheet.Cells[dataIndex, 1].PutValue(classStudent[studentCount].SeatNo);
                        currentWorksheet.Cells[dataIndex, 2].PutValue(classStudent[studentCount].Name);
                        currentWorksheet.Cells[dataIndex, 3].PutValue(custodianName[classStudent[studentCount].ID]);
                        currentWorksheet.Cells[dataIndex, 4].PutValue(mailingAddress[classStudent[studentCount].ID]);
                        currentWorksheet.Cells[dataIndex, 5].PutValue(classStudent[studentCount].PermanentPhone);
                        currentWorksheet.Cells[dataIndex, 6].PutValue(classStudent[studentCount].ContactPhone);
                        currentWorksheet.Cells[dataIndex, 7].PutValue(smsPhone[classStudent[studentCount].ID]);

                        dataIndex++;

                        //回報進度
                        _BGWClassContactList.ReportProgress((int)( (double)currentStudentNumber * 100.0 / (double)allStudentNumber ));
                    }

                    //填寫頁數
                    currentWorksheet.Cells[sheetIndex + pageRow - 1, 4].PutValue("第 " + currentPage + " 頁 / 共 " + totalPage + " 頁");
                    //設定分頁
                    currentWorksheet.HPageBreaks.Add(sheetIndex + pageRow, pageCol);

                    //下一頁
                    sheetIndex += pageRow;
                }

                //把 sheet index 儲存起來
                sheetPageIndex[gradeYear] = sheetIndex;
            }
            wb.Worksheets.SortNames();

            #endregion

            string path = Path.Combine(Application.StartupPath, "Reports");
            if ( !Directory.Exists(path) )
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xlt");
            e.Result = new object[] { reportName, path, wb };
        }
        #endregion

        #region 班級名條
        private void buttonItem2_Click(object sender, EventArgs e)
        {
            if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 0 )
                return;

            MotherForm.SetStatusBarMessage("正在初始化班級名條...");

            _BGWClassNameList = new BackgroundWorker();
            _BGWClassNameList.DoWork += new DoWorkEventHandler(_BGWClassNameList_DoWork);
            _BGWClassNameList.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CommonMethods.ExcelReport_RunWorkerCompleted);
            _BGWClassNameList.ProgressChanged += new ProgressChangedEventHandler(CommonMethods.Report_ProgressChanged);
            _BGWClassNameList.WorkerReportsProgress = true;
            _BGWClassNameList.RunWorkerAsync();
        }

        void _BGWClassNameList_DoWork(object sender, DoWorkEventArgs e)
        {
            string reportName = "班級名條";

            #region 快取所需要的資訊

            List<SmartSchool.ClassRelated.ClassInfo> selectedClasses = SmartSchool.ClassRelated.Class.Instance.SelectionClasses;

            int currentStudentNumber = 1;
            int allStudentNumber = 0;

            foreach ( SmartSchool.ClassRelated.ClassInfo var in selectedClasses )
            {
                allStudentNumber += var.Students.Count;
            }

            #endregion

            #region 產生報表

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.班級名條), FileFormatType.Excel2003);

            Range tempRange = template.Worksheets[0].Cells.CreateRange(0, 63, false);

            Dictionary<string, int> sheets = new Dictionary<string, int>();
            Dictionary<string, int> sheetRowDict = new Dictionary<string, int>();
            Dictionary<string, int> sheetColDict = new Dictionary<string, int>();

            Workbook wb = new Workbook();
            wb.Copy(template);

            Worksheet currentWorksheet = wb.Worksheets[0];
            wb.Worksheets.Clear();

            int sheetRowIndex = 0;
            int sheetColIndex = 0;
            int pageRow = 63;
            int pageCol = 11;
            int pageData = 60;
            int shift = 4;

            foreach ( SmartSchool.ClassRelated.ClassInfo var in selectedClasses )
            {
                if ( var.Students.Count <= 0 )
                    continue;

                string gradeYear = var.GradeYear;
                List<SmartSchool.StudentRelated.BriefStudentData> classStudent = var.Students;

                if ( !sheets.ContainsKey(gradeYear) )
                {
                    int index;
                    int a;
                    //新增 sheet
                    index = wb.Worksheets.Add();
                    if ( int.TryParse(gradeYear, out a) )
                        wb.Worksheets[index].Name = gradeYear + " 年級";
                    else
                        wb.Worksheets[index].Name = gradeYear;
                    //sheet 列印設定
                    wb.Worksheets[index].PageSetup.Orientation = PageOrientationType.Portrait;
                    wb.Worksheets[index].PageSetup.TopMargin = 0.9;
                    wb.Worksheets[index].PageSetup.RightMargin = 1.0;
                    wb.Worksheets[index].PageSetup.BottomMargin = 0.9;
                    wb.Worksheets[index].PageSetup.LeftMargin = 1.0;
                    wb.Worksheets[index].PageSetup.HeaderMargin = 0.0;
                    wb.Worksheets[index].PageSetup.FooterMargin = 0.0;
                    wb.Worksheets[index].PageSetup.CenterHorizontally = true;
                    wb.Worksheets[index].PageSetup.Zoom = 75;
                    sheets.Add(gradeYear, index);
                    sheetRowDict.Add(gradeYear, 0);
                    sheetColDict.Add(gradeYear, 0);

                    //複製 Template Column 寬度
                    for ( int i = 0 ; i < pageCol ; i++ )
                    {
                        wb.Worksheets[index].Cells.CopyColumn(template.Worksheets[0].Cells, i, i);
                    }
                }

                //指定 sheet
                currentWorksheet = wb.Worksheets[sheets[gradeYear]];
                sheetRowIndex = sheetRowDict[gradeYear];
                sheetColIndex = sheetColDict[gradeYear];

                int studentCount = 0;
                while ( studentCount < classStudent.Count )
                {
                    if ( sheetColIndex == 0 )
                    {
                        //複製 Template
                        currentWorksheet.Cells.CreateRange(sheetRowIndex, pageRow, false).Copy(tempRange);
                        if ( sheetRowIndex > 0 )
                        {
                            //加上上一頁的底線
                            currentWorksheet.Cells.CreateRange(sheetRowIndex - 1, 0, 1, 3).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);
                            currentWorksheet.Cells.CreateRange(sheetRowIndex - 1, 4, 1, 3).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);
                            currentWorksheet.Cells.CreateRange(sheetRowIndex - 1, 8, 1, 3).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);
                        }
                    }

                    currentWorksheet.Cells[sheetRowIndex, sheetColIndex].PutValue(var.ClassName + " 班級名條");

                    int dataIndex = sheetRowIndex + 2;

                    for ( int i = 0 ; i < pageData && studentCount < classStudent.Count ; studentCount++, i++ )
                    {
                        currentWorksheet.Cells[dataIndex + i, sheetColIndex].PutValue(classStudent[studentCount].SeatNo);
                        currentWorksheet.Cells[dataIndex + i, sheetColIndex + 1].PutValue(classStudent[studentCount].Name);
                        currentWorksheet.Cells[dataIndex + i, sheetColIndex + 2].PutValue(classStudent[studentCount].StudentNumber);
                        //回報進度
                        _BGWClassNameList.ReportProgress((int)( ( (double)currentStudentNumber++ * 100.0 ) / (double)allStudentNumber ));
                    }

                    //往右邊 shift
                    sheetColIndex += shift;

                    if ( sheetColIndex > pageCol )
                    {
                        sheetColIndex = 0;
                        sheetRowIndex += pageRow - 1;

                        //設定分頁
                        currentWorksheet.HPageBreaks.Add(sheetRowIndex, pageCol);
                    }

                }

                sheetRowDict[gradeYear] = sheetRowIndex;
                sheetColDict[gradeYear] = sheetColIndex;
            }

            if ( sheetRowIndex > 0 )
            {
                //加上最後一頁的底線
                currentWorksheet.Cells.CreateRange(sheetRowIndex - 1, 0, 1, 3).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);
                currentWorksheet.Cells.CreateRange(sheetRowIndex - 1, 4, 1, 3).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);
                currentWorksheet.Cells.CreateRange(sheetRowIndex - 1, 8, 1, 3).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);
            }

            wb.Worksheets.SortNames();

            if ( allStudentNumber <= 0 )
                wb = new Workbook();

            #endregion

            string path = Path.Combine(Application.StartupPath, "Reports");
            if ( !Directory.Exists(path) )
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xlt");
            e.Result = new object[] { reportName, path, wb };
        }
        #endregion

        #region 班級考試成績單
        private void buttonItem7_Click(object sender, EventArgs e)
        {
            new ExamScoreListSubjectSelector().ShowDialog();
        }
        #endregion

        #region 班級點名表
        private void buttonItem8_Click(object sender, EventArgs e)
        {
            if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 0 )
                return;

            List<string> config = new List<string>();

            ClassStudentAttendanceSelectPeriodForm form = new ClassStudentAttendanceSelectPeriodForm("班級點名表_節次設定");

            if ( form.ShowDialog() == DialogResult.OK )
            {
                XmlElement preferenceData = CurrentUser.Instance.Preference["班級點名表_節次設定"];

                if ( preferenceData == null )
                {
                    MsgBox.Show("第一次使用班級點名表請先設定節次。");
                    return;
                }
                else
                {
                    foreach ( XmlElement period in preferenceData.SelectNodes("Period") )
                    {
                        string name = period.GetAttribute("Name");
                        config.Add(name);
                    }
                }

                MotherForm.SetStatusBarMessage("正在初始化班級點名表...");

                object[] args = new object[] { config };

                _BGWClassStudentAttendance = new BackgroundWorker();
                _BGWClassStudentAttendance.DoWork += new DoWorkEventHandler(_BGWClassStudentAttendance_DoWork);
                _BGWClassStudentAttendance.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CommonMethods.ExcelReport_RunWorkerCompleted);
                _BGWClassStudentAttendance.ProgressChanged += new ProgressChangedEventHandler(CommonMethods.Report_ProgressChanged);
                _BGWClassStudentAttendance.WorkerReportsProgress = true;
                _BGWClassStudentAttendance.RunWorkerAsync(args);
            }
        }

        void _BGWClassStudentAttendance_DoWork(object sender, DoWorkEventArgs e)
        {
            string reportName = "班級點名表";

            object[] args = e.Argument as object[];

            List<string> config = args[0] as List<string>;

            #region 快取資訊

            List<SmartSchool.ClassRelated.ClassInfo> selectedClass = SmartSchool.ClassRelated.Class.Instance.SelectionClasses;

            Dictionary<string, List<SmartSchool.StudentRelated.BriefStudentData>> classStudentList = new Dictionary<string, List<SmartSchool.StudentRelated.BriefStudentData>>();

            //學生人數
            int currentStudentCount = 1;
            int totalStudentNumber = 0;

            //紀錄每一個 Column 的 Index
            Dictionary<string, int> columnTable = new Dictionary<string, int>();

            //使用者定義的節次列表
            List<string> userDefinedPeriodList = new List<string>();

            //節次對照表
            List<string> periodList = new List<string>();

            //依據 ClassID 建立班級學生清單
            foreach ( SmartSchool.ClassRelated.ClassInfo aClass in selectedClass )
            {
                List<SmartSchool.StudentRelated.BriefStudentData> classStudent = aClass.Students;
                if ( !classStudentList.ContainsKey(aClass.ClassID) )
                    classStudentList.Add(aClass.ClassID, classStudent);
                totalStudentNumber += classStudent.Count;
            }

            //取得 Period List
            DSResponse dsrsp = SmartSchool.Feature.Basic.Config.GetPeriodList();
            foreach ( XmlElement var in dsrsp.GetContent().GetElements("Period") )
            {
                if ( !periodList.Contains(var.GetAttribute("Name")) )
                    periodList.Add(var.GetAttribute("Name"));
            }

            //套用使用者的設定
            userDefinedPeriodList = config;

            #endregion

            #region 產生範本

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.班級點名表), FileFormatType.Excel2003);

            Range tempStudent = template.Worksheets[0].Cells.CreateRange(0, 4, true);
            Range tempEachColumn = template.Worksheets[0].Cells.CreateRange(4, 1, true);

            Workbook prototype = new Workbook();
            prototype.Copy(template);

            prototype.Worksheets[0].Cells.CreateRange(0, 4, true).Copy(tempStudent);

            int titleRow = 2;
            int colIndex = 4;

            int startIndex = colIndex;
            int endIndex;
            int columnNumber;

            //根據使用者定義的節次動態產生欄位
            foreach ( string period in userDefinedPeriodList )
            {
                prototype.Worksheets[0].Cells.CreateRange(colIndex, 1, true).Copy(tempEachColumn);
                prototype.Worksheets[0].Cells[titleRow, colIndex].PutValue(period);
                columnTable.Add(period, colIndex - 4);
                colIndex++;
            }

            endIndex = colIndex;
            columnNumber = endIndex - startIndex;
            if ( columnNumber == 0 )
                columnNumber = 1;

            prototype.Worksheets[0].Cells.CreateRange(0, 0, 1, endIndex).Merge();
            if ( endIndex - 3 > 0 )
                prototype.Worksheets[0].Cells.CreateRange(1, 3, 1, endIndex - 3).Merge();

            Range prototypeRow = prototype.Worksheets[0].Cells.CreateRange(3, 1, false);
            Range prototypeHeader = prototype.Worksheets[0].Cells.CreateRange(0, 3, false);

            #endregion

            #region 產生報表

            Workbook wb = new Workbook();
            wb.Copy(prototype);

            int index = 0;
            int dataIndex = 0;

            foreach ( SmartSchool.ClassRelated.ClassInfo classInfo in selectedClass )
            {
                List<SmartSchool.StudentRelated.BriefStudentData> classStudent = classStudentList[classInfo.ClassID];

                //如果不是第一頁，就在上一頁的資料列下邊加黑線
                if ( index != 0 )
                    wb.Worksheets[0].Cells.CreateRange(index - 1, 0, 1, endIndex).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);

                //複製 Header
                wb.Worksheets[0].Cells.CreateRange(index, 5, false).Copy(prototypeHeader);

                //填寫基本資料
                wb.Worksheets[0].Cells[index, 0].PutValue(CurrentUser.Instance.SchoolChineseName + " 班級點名表");
                wb.Worksheets[0].Cells[index + 1, 0].PutValue("班級：" + classInfo.ClassName);
                wb.Worksheets[0].Cells[index + 1, 3].PutValue("年　　　月　　　日　星期：　　　");

                dataIndex = index + 3;

                int studentCount = 0;
                while ( studentCount < classStudent.Count )
                {
                    //複製每一個 row
                    wb.Worksheets[0].Cells.CreateRange(dataIndex, 1, false).Copy(prototypeRow);
                    //if (studentCount % 5 == 0 && studentCount != 0)
                    //{
                    //    Range eachFiveRow = wb.Worksheets[0].Cells.CreateRange(dataIndex, 0, 1, dayStartIndex);
                    //    eachFiveRow.SetOutlineBorder(BorderType.TopBorder, CellBorderType.Double, Color.Black);
                    //}

                    //填寫學生資料
                    SmartSchool.StudentRelated.BriefStudentData student = classStudent[studentCount];
                    wb.Worksheets[0].Cells[dataIndex, 0].PutValue(student.SeatNo);
                    wb.Worksheets[0].Cells[dataIndex, 1].PutValue(student.StudentNumber);
                    wb.Worksheets[0].Cells[dataIndex, 2].PutValue(student.Name);
                    wb.Worksheets[0].Cells[dataIndex, 3].PutValue(student.Gender);

                    studentCount++;
                    dataIndex++;
                    _BGWClassStudentAttendance.ReportProgress((int)( ( (double)currentStudentCount++ * 100.0 ) / (double)totalStudentNumber ));
                }

                //資料列上邊加上黑線
                wb.Worksheets[0].Cells.CreateRange(index + 2, 0, 1, endIndex).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);

                //表格最右邊加上黑線
                wb.Worksheets[0].Cells.CreateRange(index + 2, endIndex - 1, studentCount + 1, 1).SetOutlineBorder(BorderType.RightBorder, CellBorderType.Medium, Color.Black);

                index = dataIndex;

                //設定分頁
                wb.Worksheets[0].HPageBreaks.Add(index, endIndex);
            }

            //最後一頁的資料列下邊加上黑線
            wb.Worksheets[0].Cells.CreateRange(dataIndex - 1, 0, 1, endIndex).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);

            #endregion

            string path = Path.Combine(Application.StartupPath, "Reports");
            if ( !Directory.Exists(path) )
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xlt");
            e.Result = new object[] { reportName, path, wb };
        }
        #endregion

        #region 班級學生缺曠明細
        private void buttonItem9_Click(object sender, EventArgs e)
        {
            if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 0 )
                return;

            AbsenceDetailForm form = new AbsenceDetailForm();
            if ( form.ShowDialog() == DialogResult.OK )
            {
                MotherForm.SetStatusBarMessage("正在初始化班級學生缺曠明細...");

                object[] args = new object[] { form.StartDate, form.EndDate, form.PaperSize };

                _BGWClassStudentAbsenceDetail = new BackgroundWorker();
                _BGWClassStudentAbsenceDetail.DoWork += new DoWorkEventHandler(_BGWClassStudentAbsenceDetail_DoWork);
                _BGWClassStudentAbsenceDetail.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CommonMethods.ExcelReport_RunWorkerCompleted);
                _BGWClassStudentAbsenceDetail.ProgressChanged += new ProgressChangedEventHandler(CommonMethods.Report_ProgressChanged);
                _BGWClassStudentAbsenceDetail.WorkerReportsProgress = true;
                _BGWClassStudentAbsenceDetail.RunWorkerAsync(args);
            }
        }

        void _BGWClassStudentAbsenceDetail_DoWork(object sender, DoWorkEventArgs e)
        {
            string reportName = "班級學生缺曠明細";

            object[] args = e.Argument as object[];

            DateTime startDate = (DateTime)args[0];
            DateTime endDate = (DateTime)args[1];
            int size = (int)args[2];

            #region 快取資料

            List<SmartSchool.ClassRelated.ClassInfo> selectedClass = SmartSchool.ClassRelated.Class.Instance.SelectionClasses;

            //學生ID查詢班級ID
            Dictionary<string, string> studentClassDict = new Dictionary<string, string>();

            //學生ID查詢學生資訊
            Dictionary<string, SmartSchool.StudentRelated.BriefStudentData> studentInfoDict = new Dictionary<string, SmartSchool.StudentRelated.BriefStudentData>();

            //缺曠明細，Key為 ClassID -> StudentID -> OccurDate -> Period
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>> allAbsenceDetail = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>();

            //所有學生ID
            List<string> allStudentID = new List<string>();

            //缺曠筆數
            int currentCount = 1;
            int totalNumber = 0;

            //紀錄每一個 Column 的 Index
            Dictionary<string, int> columnTable = new Dictionary<string, int>();

            //節次對照表
            List<string> periodList = new List<string>();

            //建立學生班級對照表
            foreach ( SmartSchool.ClassRelated.ClassInfo aClass in selectedClass )
            {
                List<SmartSchool.StudentRelated.BriefStudentData> classStudent = aClass.Students;

                foreach ( SmartSchool.StudentRelated.BriefStudentData student in classStudent )
                {
                    allStudentID.Add(student.ID);
                    studentClassDict.Add(student.ID, aClass.ClassID);
                    studentInfoDict.Add(student.ID, student);
                }

                allAbsenceDetail.Add(aClass.ClassID, new Dictionary<string, Dictionary<string, Dictionary<string, string>>>());
            }

            //取得 Period List
            DSResponse dsrsp = SmartSchool.Feature.Basic.Config.GetPeriodList();
            foreach ( XmlElement var in dsrsp.GetContent().GetElements("Period") )
            {
                if ( !periodList.Contains(var.GetAttribute("Name")) )
                    periodList.Add(var.GetAttribute("Name"));
            }

            //取得缺曠明細，產生 DSRequest
            DSXmlHelper helper = new DSXmlHelper("Request");
            helper.AddElement("Field");
            helper.AddElement("Field", "All");
            helper.AddElement("Condition");
            foreach ( string var in allStudentID )
            {
                helper.AddElement("Condition", "RefStudentID", var);
            }
            helper.AddElement("Condition", "StartDate", startDate.ToShortDateString());
            helper.AddElement("Condition", "EndDate", endDate.ToShortDateString());
            helper.AddElement("Order");
            helper.AddElement("Order", "OccurDate", "asc");
            dsrsp = SmartSchool.Feature.Student.QueryAttendance.GetAttendance(new DSRequest(helper));

            foreach ( XmlElement var in dsrsp.GetContent().GetElements("Attendance") )
            {
                string studentID = var.SelectSingleNode("RefStudentID").InnerText;
                string occurDate = DateTime.Parse(var.SelectSingleNode("OccurDate").InnerText).ToShortDateString();
                string classID = studentClassDict[studentID];

                if ( !allAbsenceDetail.ContainsKey(classID) )
                    allAbsenceDetail.Add(classID, new Dictionary<string, Dictionary<string, Dictionary<string, string>>>());

                if ( !allAbsenceDetail[classID].ContainsKey(studentID) )
                    allAbsenceDetail[classID].Add(studentID, new Dictionary<string, Dictionary<string, string>>());

                if ( !allAbsenceDetail[classID][studentID].ContainsKey(occurDate) )
                    allAbsenceDetail[classID][studentID].Add(occurDate, new Dictionary<string, string>());

                foreach ( XmlElement period in var.SelectNodes("Detail/Attendance/Period") )
                {
                    //節次
                    string innertext = period.InnerText;
                    //缺曠別
                    string absence = period.GetAttribute("AbsenceType");

                    if ( !allAbsenceDetail[classID][studentID][occurDate].ContainsKey(innertext) )
                        allAbsenceDetail[classID][studentID][occurDate].Add(innertext, absence);
                }

                //累計筆數
                totalNumber++;
            }

            #endregion

            #region 產生範本

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.班級學生缺曠明細), FileFormatType.Excel2003);

            Range tempStudent = template.Worksheets[0].Cells.CreateRange(0, 3, true);
            Range tempEachColumn = template.Worksheets[0].Cells.CreateRange(3, 1, true);

            Workbook prototype = new Workbook();
            prototype.Copy(template);

            prototype.Worksheets[0].Cells.CreateRange(0, 3, true).Copy(tempStudent);

            int colIndex = 3;

            int startIndex = colIndex;
            int endIndex;
            int columnNumber;

            int splitLineIndex = 0;

            //根據節次對照表產生欄位，並且隨著節次字數調整欄寬
            foreach ( string period in periodList )
            {
                Range each = prototype.Worksheets[0].Cells.CreateRange(colIndex, 1, true);
                each.Copy(tempEachColumn);
                if ( period.Length > 2 )
                {
                    double width = 4.5;
                    width += (double)( ( period.Length - 2 ) * 2 );
                    each.ColumnWidth = width;
                }
                prototype.Worksheets[0].Cells[1, colIndex].PutValue(period);
                columnTable.Add(period, colIndex - 3);
                colIndex++;
            }

            endIndex = colIndex;
            splitLineIndex = ( colIndex + 1 ) / 2;

            columnNumber = endIndex - startIndex;
            if ( columnNumber == 0 )
                columnNumber = 1;

            prototype.Worksheets[0].Cells.CreateRange(0, 0, 1, splitLineIndex).Merge();
            prototype.Worksheets[0].Cells.CreateRange(0, splitLineIndex, 1, endIndex - splitLineIndex).Merge();

            Range prototypeRow = prototype.Worksheets[0].Cells.CreateRange(2, 1, false);
            Range prototypeHeader = prototype.Worksheets[0].Cells.CreateRange(0, 2, false);

            #endregion

            #region 產生報表

            Workbook wb = new Workbook();
            wb.Copy(prototype);
            Worksheet ws = wb.Worksheets[0];

            #region 判斷紙張大小
            if ( size == 0 )
            {
                ws.PageSetup.PaperSize = PaperSizeType.PaperA3;
                ws.PageSetup.Zoom = 145;
            }
            else if ( size == 1 )
            {
                ws.PageSetup.PaperSize = PaperSizeType.PaperA4;
                ws.PageSetup.Zoom = 100;
            }
            else if ( size == 2 )
            {
                ws.PageSetup.PaperSize = PaperSizeType.PaperB4;
                ws.PageSetup.Zoom = 125;
            }
            #endregion

            int index = 0;
            int dataIndex = 0;

            foreach ( SmartSchool.ClassRelated.ClassInfo classInfo in selectedClass )
            {
                Dictionary<string, Dictionary<string, Dictionary<string, string>>> classAbsenceDetail = allAbsenceDetail[classInfo.ClassID];

                if ( classAbsenceDetail.Count <= 0 )
                    continue;

                //如果不是第一頁，就在上一頁的資料列下邊加黑線
                if ( index != 0 )
                    ws.Cells.CreateRange(index - 1, 0, 1, endIndex).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);

                //複製 Header
                wb.Worksheets[0].Cells.CreateRange(index, 2, false).Copy(prototypeHeader);

                //填寫基本資料
                ws.Cells[index, 0].PutValue(classInfo.ClassName + " 班級學生缺曠明細表");
                ws.Cells[index, splitLineIndex].PutValue("統計日期：" + startDate.ToShortDateString() + " ~ " + endDate.ToShortDateString());
                ws.Cells[index, splitLineIndex].Style.Font.Size = 10;
                ws.Cells[index, splitLineIndex].Style.HorizontalAlignment = TextAlignmentType.Left;
                ws.Cells[index, splitLineIndex].Style.VerticalAlignment = TextAlignmentType.Bottom;

                dataIndex = index + 2;

                List<string> list = new List<string>(classAbsenceDetail.Keys);
                list.Sort(new SeatNoComparer(studentInfoDict));

                foreach ( string studentID in list )
                {
                    //填寫資料
                    Dictionary<string, Dictionary<string, string>> studentAbsenceDetail = classAbsenceDetail[studentID];
                    foreach ( string occurDate in studentAbsenceDetail.Keys )
                    {
                        //先檢查他的缺曠節次是否都不存在於對照表中，不存在則不印該筆資料
                        bool printable = false;
                        foreach ( string period in studentAbsenceDetail[occurDate].Keys )
                        {
                            if ( columnTable.ContainsKey(period) )
                                printable = true;
                        }
                        if ( !printable )
                            continue;

                        //複製每一個 row
                        ws.Cells.CreateRange(dataIndex, 1, false).Copy(prototypeRow);

                        ws.Cells[dataIndex, 0].PutValue(studentInfoDict[studentID].SeatNo);
                        ws.Cells[dataIndex, 1].PutValue(studentInfoDict[studentID].Name);
                        ws.Cells[dataIndex, 2].PutValue(occurDate);

                        foreach ( string period in studentAbsenceDetail[occurDate].Keys )
                        {
                            if ( columnTable.ContainsKey(period) )
                                ws.Cells[dataIndex, columnTable[period] + 3].PutValue(studentAbsenceDetail[occurDate][period]);
                        }

                        dataIndex++;
                        _BGWClassStudentAbsenceDetail.ReportProgress((int)( ( (double)currentCount++ * 100.0 ) / (double)totalNumber ));
                    }
                }

                //資料列上邊加上黑線
                //wb.Worksheets[0].Cells.CreateRange(index + 1, 0, 1, endIndex).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);

                //表格最右邊加上黑線
                ws.Cells.CreateRange(index + 1, endIndex - 1, ( dataIndex - index - 1 ), 1).SetOutlineBorder(BorderType.RightBorder, CellBorderType.Medium, Color.Black);

                index = dataIndex;

                //設定分頁
                ws.HPageBreaks.Add(index, endIndex);
            }

            if ( dataIndex > 0 )
            {
                //最後一頁的資料列下邊加上黑線
                ws.Cells.CreateRange(dataIndex - 1, 0, 1, endIndex).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);
            }
            else
                wb = new Workbook();

            #endregion

            string path = Path.Combine(Application.StartupPath, "Reports");
            if ( !Directory.Exists(path) )
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xlt");
            e.Result = new object[] { reportName, path, wb };
        }
        #endregion

        #region 班級學生獎懲明細
        private void buttonItem11_Click(object sender, EventArgs e)
        {
            if ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 0 )
                return;

            DisciplineDetailForm form = new DisciplineDetailForm();
            if ( form.ShowDialog() == DialogResult.OK )
            {
                MotherForm.SetStatusBarMessage("正在初始化班級學生獎懲明細...");

                object[] args = new object[] { form.StartDate, form.EndDate, form.PaperSize };

                _BGWClassStudentDisciplineDetail = new BackgroundWorker();
                _BGWClassStudentDisciplineDetail.DoWork += new DoWorkEventHandler(_BGWClassStudentDisciplineDetail_DoWork);
                _BGWClassStudentDisciplineDetail.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CommonMethods.ExcelReport_RunWorkerCompleted);
                _BGWClassStudentDisciplineDetail.ProgressChanged += new ProgressChangedEventHandler(CommonMethods.Report_ProgressChanged);
                _BGWClassStudentDisciplineDetail.WorkerReportsProgress = true;
                _BGWClassStudentDisciplineDetail.RunWorkerAsync(args);
            }
        }

        void _BGWClassStudentDisciplineDetail_DoWork(object sender, DoWorkEventArgs e)
        {
            string reportName = "班級學生獎懲明細";

            object[] args = e.Argument as object[];

            DateTime startDate = (DateTime)args[0];
            DateTime endDate = (DateTime)args[1];
            int size = (int)args[2];

            #region 快取資料

            List<SmartSchool.ClassRelated.ClassInfo> selectedClass = SmartSchool.ClassRelated.Class.Instance.SelectionClasses;

            //學生ID查詢班級ID
            Dictionary<string, string> studentClassDict = new Dictionary<string, string>();

            //學生ID查詢學生資訊
            Dictionary<string, SmartSchool.StudentRelated.BriefStudentData> studentInfoDict = new Dictionary<string, SmartSchool.StudentRelated.BriefStudentData>();

            //獎懲明細，Key為 ClassID -> StudentID -> OccurDate -> DisciplineType
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>> allDisciplineDetail = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>();

            //所有學生ID
            List<string> allStudentID = new List<string>();

            //獎懲筆數
            int currentCount = 1;
            int totalNumber = 0;

            //獎勵項目
            Dictionary<string, string> meritTable = new Dictionary<string, string>();
            meritTable.Add("大功", "A");
            meritTable.Add("小功", "B");
            meritTable.Add("嘉獎", "C");

            //懲戒項目
            Dictionary<string, string> demeritTable = new Dictionary<string, string>();
            demeritTable.Add("大過", "A");
            demeritTable.Add("小過", "B");
            demeritTable.Add("警告", "C");

            //紀錄每一個 Column 的 Index
            Dictionary<string, int> columnTable = new Dictionary<string, int>();

            //建立學生班級對照表
            foreach ( SmartSchool.ClassRelated.ClassInfo aClass in selectedClass )
            {
                List<SmartSchool.StudentRelated.BriefStudentData> classStudent = aClass.Students;

                foreach ( SmartSchool.StudentRelated.BriefStudentData student in classStudent )
                {
                    allStudentID.Add(student.ID);
                    studentClassDict.Add(student.ID, aClass.ClassID);
                    studentInfoDict.Add(student.ID, student);
                }

                allDisciplineDetail.Add(aClass.ClassID, new Dictionary<string, Dictionary<string, Dictionary<string, string>>>());
            }

            //取得獎懲資料 日期區間
            DSXmlHelper helper = new DSXmlHelper("Request");
            helper.AddElement("Field");
            helper.AddElement("Field", "All");
            helper.AddElement("Condition");
            foreach ( string var in allStudentID )
            {
                helper.AddElement("Condition", "RefStudentID", var);
            }
            helper.AddElement("Condition", "StartDate", startDate.ToShortDateString());
            helper.AddElement("Condition", "EndDate", endDate.ToShortDateString());
            helper.AddElement("Order");
            helper.AddElement("Order", "OccurDate", "asc");
            DSResponse dsrsp = SmartSchool.Feature.Student.QueryDiscipline.GetDiscipline(new DSRequest(helper));

            foreach ( XmlElement var in dsrsp.GetContent().GetElements("Discipline") )
            {
                string studentID = var.SelectSingleNode("RefStudentID").InnerText;
                DateTime occurDate = DateTime.Parse(var.SelectSingleNode("OccurDate").InnerText);
                string disciplineID = var.GetAttribute("ID");
                string occurDateID = occurDate.ToShortDateString() + "_" + disciplineID;
                string reason = var.SelectSingleNode("Reason").InnerText;
                string classID = studentClassDict[studentID];

                if ( !allDisciplineDetail.ContainsKey(classID) )
                    allDisciplineDetail.Add(classID, new Dictionary<string, Dictionary<string, Dictionary<string, string>>>());

                if ( !allDisciplineDetail[classID].ContainsKey(studentID) )
                    allDisciplineDetail[classID].Add(studentID, new Dictionary<string, Dictionary<string, string>>());

                if ( !allDisciplineDetail[classID][studentID].ContainsKey(occurDateID) )
                    allDisciplineDetail[classID][studentID].Add(occurDateID, new Dictionary<string, string>());

                //加入事由
                if ( !allDisciplineDetail[classID][studentID][occurDateID].ContainsKey("事由") )
                    allDisciplineDetail[classID][studentID][occurDateID].Add("事由", reason);

                if ( var.SelectSingleNode("MeritFlag").InnerText == "1" )
                {
                    XmlElement meritElement = (XmlElement)var.SelectSingleNode("Detail/Discipline/Merit");
                    if ( meritElement == null ) continue;

                    foreach ( string merit in meritTable.Keys )
                    {
                        string times = meritElement.GetAttribute(meritTable[merit]);
                        if ( times != "0" )
                        {
                            if ( !allDisciplineDetail[classID][studentID][occurDateID].ContainsKey(merit) )
                                allDisciplineDetail[classID][studentID][occurDateID].Add(merit, "0");

                            allDisciplineDetail[classID][studentID][occurDateID][merit] = times;
                        }
                    }
                }
                else
                {
                    XmlElement demeritElement = (XmlElement)var.SelectSingleNode("Detail/Discipline/Demerit");
                    if ( demeritElement == null ) continue;

                    string clearDate = "";
                    if ( demeritElement.GetAttribute("Cleared") == "是" )
                    {
                        clearDate = demeritElement.GetAttribute("ClearDate");
                        if ( !allDisciplineDetail[classID][studentID][occurDateID].ContainsKey("銷過") )
                            allDisciplineDetail[classID][studentID][occurDateID].Add("銷過", "是");
                        if ( !allDisciplineDetail[classID][studentID][occurDateID].ContainsKey("銷過日期") )
                            allDisciplineDetail[classID][studentID][occurDateID].Add("銷過日期", clearDate);
                    }

                    foreach ( string demerit in demeritTable.Keys )
                    {
                        string times = demeritElement.GetAttribute(demeritTable[demerit]);
                        if ( times != "0" )
                        {
                            if ( !allDisciplineDetail[classID][studentID][occurDateID].ContainsKey(demerit) )
                                allDisciplineDetail[classID][studentID][occurDateID].Add(demerit, "0");

                            allDisciplineDetail[classID][studentID][occurDateID][demerit] = times;
                        }
                    }
                }

                totalNumber++;
            }

            #endregion

            #region 產生範本

            Workbook template = new Workbook();
            template.Open(new MemoryStream(Properties.Resources.班級學生獎懲明細), FileFormatType.Excel2003);

            Range tempStudent = template.Worksheets[0].Cells.CreateRange(0, 10, true);

            Workbook prototype = new Workbook();
            prototype.Copy(template);

            prototype.Worksheets[0].Cells.CreateRange(0, 10, true).Copy(tempStudent);

            int colIndex = 3;
            int endIndex = colIndex;
            foreach ( string var in meritTable.Keys )
            {
                columnTable.Add(var, colIndex++);
            }
            foreach ( string var in demeritTable.Keys )
            {
                columnTable.Add(var, colIndex++);
            }
            columnTable.Add("銷過", colIndex++);
            columnTable.Add("銷過日期", colIndex++);
            columnTable.Add("事由", colIndex++);
            endIndex = colIndex;

            //prototype.Worksheets[0].Cells.CreateRange(0, 0, 1, endIndex).Merge();

            Range prototypeRow = prototype.Worksheets[0].Cells.CreateRange(2, 1, false);
            Range prototypeHeader = prototype.Worksheets[0].Cells.CreateRange(0, 2, false);

            #endregion

            #region 產生報表

            Workbook wb = new Workbook();
            wb.Copy(prototype);
            Worksheet ws = wb.Worksheets[0];

            #region 判斷紙張大小
            if ( size == 0 )
            {
                ws.PageSetup.PaperSize = PaperSizeType.PaperA3;
                ws.PageSetup.Zoom = 140;
            }
            else if ( size == 1 )
            {
                ws.PageSetup.PaperSize = PaperSizeType.PaperA4;
                ws.PageSetup.Zoom = 100;
            }
            else if ( size == 2 )
            {
                ws.PageSetup.PaperSize = PaperSizeType.PaperB4;
                ws.PageSetup.Zoom = 122;
            }
            #endregion

            int index = 0;
            int dataIndex = 0;

            foreach ( SmartSchool.ClassRelated.ClassInfo classInfo in selectedClass )
            {
                Dictionary<string, Dictionary<string, Dictionary<string, string>>> classDisciplineDetail = allDisciplineDetail[classInfo.ClassID];

                if ( classDisciplineDetail.Count <= 0 )
                    continue;

                //如果不是第一頁，就在上一頁的資料列下邊加黑線
                if ( index != 0 )
                    ws.Cells.CreateRange(index - 1, 0, 1, endIndex).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);

                //複製 Header
                ws.Cells.CreateRange(index, 2, false).Copy(prototypeHeader);

                //填寫基本資料
                ws.Cells[index, 0].PutValue(classInfo.ClassName + " 班級學生獎懲明細表");
                ws.Cells[index, 11].PutValue("統計日期：" + startDate.ToShortDateString() + " ~ " + endDate.ToShortDateString());

                dataIndex = index + 2;

                List<string> list = new List<string>();
                list.AddRange(classDisciplineDetail.Keys);
                list.Sort(new SeatNoComparer(studentInfoDict));

                foreach ( string studentID in list )
                {
                    //填寫資料
                    Dictionary<string, Dictionary<string, string>> studentDisciplineDetail = classDisciplineDetail[studentID];
                    foreach ( string occurDateID in studentDisciplineDetail.Keys )
                    {
                        //複製每一個 row
                        ws.Cells.CreateRange(dataIndex, 1, false).Copy(prototypeRow);

                        ws.Cells[dataIndex, 0].PutValue(studentInfoDict[studentID].SeatNo);
                        ws.Cells[dataIndex, 1].PutValue(studentInfoDict[studentID].Name);

                        string[] occurDateAndID = occurDateID.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

                        ws.Cells[dataIndex, 2].PutValue(occurDateAndID[0]);

                        foreach ( string type in studentDisciplineDetail[occurDateID].Keys )
                        {
                            if ( columnTable.ContainsKey(type) )
                                ws.Cells[dataIndex, columnTable[type]].PutValue(studentDisciplineDetail[occurDateID][type]);
                        }

                        dataIndex++;
                        _BGWClassStudentDisciplineDetail.ReportProgress((int)( ( (double)currentCount++ * 100.0 ) / (double)totalNumber ));
                    }
                }

                index = dataIndex;

                //設定分頁
                ws.HPageBreaks.Add(index, endIndex);
            }

            if ( dataIndex > 0 )
            {
                //最後一頁的資料列下邊加上黑線
                ws.Cells.CreateRange(dataIndex - 1, 0, 1, endIndex).SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Medium, Color.Black);
            }
            else
                wb = new Workbook();

            #endregion

            string path = Path.Combine(Application.StartupPath, "Reports");
            if ( !Directory.Exists(path) )
                Directory.CreateDirectory(path);
            path = Path.Combine(path, reportName + ".xlt");
            e.Result = new object[] { reportName, path, wb };
        }
        #endregion

        #region 德行表現特殊學生
        private void btnSearchAttendance_Click(object sender, EventArgs e)
        {
            //List<ClassInfo> list = SmartSchool.ClassRelated.Class.Instance.SelectionClasses;
            //List<string> idList = new List<string>();
            //foreach ( ClassInfo info in list )
            //    idList.Add(info.ClassID);
            //DeXingStatistic statistic = new DeXingStatistic(idList.ToArray());
            //statistic.ShowDialog();
        }
        #endregion

        #region 班級考試成績單(Word)
        private void buttonItem3_Click(object sender, EventArgs e)
        {
            new ExamScoreListSubjectSelectorNew().ShowDialog();
        }
        #endregion

    }

    class SeatNoComparer : IComparer<string>
    {
        private Dictionary<string, BriefStudentData> _mapping;

        public SeatNoComparer(Dictionary<string, BriefStudentData> mapping)
        {
            _mapping = mapping;
        }

        #region IComparer<string> 成員

        public int Compare(string x, string y)
        {
            BriefStudentData X = _mapping[x];
            BriefStudentData Y = _mapping[y];

            int tryX, tryY;
            int intX = ( int.TryParse(X.SeatNo, out tryX) ) ? tryX : 99999;
            int intY = ( int.TryParse(Y.SeatNo, out tryY) ) ? tryY : 99999;

            return intX.CompareTo(intY);
        }

        #endregion
    }
}
