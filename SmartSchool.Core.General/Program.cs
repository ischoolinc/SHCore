using System;
using FISCA.Presentation;
using SmartSchool.ClassRelated;
using SmartSchool.Others.Configuration.AbsenceMapping;
using SmartSchool.Others.Configuration.DegreeMapping;
using SmartSchool.Others.Configuration.DisciplineMapping;
using SmartSchool.Others.Configuration.IdentityMapping;
using SmartSchool.Others.Configuration.MDReduceMapping;
using SmartSchool.Others.Configuration.MoralityMapping;
using SmartSchool.Others.Configuration.PeriodMapping;
using SmartSchool.Others.Configuration.Setup;
using SmartSchool.Others.Configuration.WordCommentMapping;
using SmartSchool.StudentRelated;
using SmartSchool.TeacherRelated;
using SmartSchool.StudentRelated.RibbonBars.Export;
using SmartSchool.StudentRelated.RibbonBars.Import;
using SmartSchool.API.PlugIn.Export;
using SmartSchool.API.PlugIn.Import;
using System.Collections.Generic;
using FISCA.Permission;

namespace SmartSchool
{
    public static class Core_General_Program
    {
        public static void Init_Student_Class_Teacher_Load()
        {
            //設定同步更新事件
            Student.Instance.SetupSynchronization();
            Class.Instance.SetupSynchronization();
            Teacher.Instance.SetupSynchronization();

            Class.Instance.SyncAllBackground();
            Student.Instance.SyncAllBackground();
            Teacher.Instance.SyncAllBackground();
        }
        public static void Init_Student_Class_Teacher()
        {
            //CreateInstance 要先做
            Student.Instance.SetupPresentation();
            Class.Instance.SetupPresentation();
            Teacher.Instance.SetupPresentation();

            //新增刪除學生
            SmartSchool.StudentRelated.Process.StudentIUD.StudentIDUProcess.Instance.Setup();
            //成績
            new StudentRelated.RibbonBars.EducationalAdministration().Setup();
            ////類別
            //new StudentRelated.RibbonBars.Assign().Setup();
            //報表
            new StudentRelated.RibbonBars.Report().Setup();
            //匯出匯入
            new SmartSchool.StudentRelated.RibbonBars.Import.ImportExport().Setup();
            //其它
            new StudentRelated.RibbonBars.Others().Setup();

            //匯出
            RibbonBarButton rbItemExport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯出"];
            rbItemExport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            rbItemExport.SupposeHasChildern = true;
            rbItemExport.Image = Properties.Resources.Export_Image;

            #region 匯出(1000708)
            rbItemExport["學籍相關匯出"]["匯出學生基本資料"].Enable = CurrentUser.Acl["Button0130"].Executable;
            rbItemExport["學籍相關匯出"]["匯出學生基本資料"].Click += delegate
            {
                SmartSchool.StudentRelated.RibbonBars.Import.ExportWizard export = new SmartSchool.StudentRelated.RibbonBars.Import.ExportWizard();
                export.ShowDialog();
            };

            //20131216 - dylan 註解
            //rbItemExport["學籍相關匯出"]["匯出學生照片"].Enable = CurrentUser.Acl["Button0290.5"].Executable;
            //rbItemExport["學籍相關匯出"]["匯出學生照片"].Click += delegate
            //{
            //    new K12.Form.Photo.PhotosBatchExportForm().ShowDialog();
            //};

            
            rbItemExport["學籍相關匯出"]["匯出離校資訊"].Enable = CurrentUser.Acl["SHSchool.Student.Ribbon0171"].Executable;
            rbItemExport["學籍相關匯出"]["匯出離校資訊"].Click += delegate
            {
                Exporter exporter = new ExportLeaveInfo();
                ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };

            //rbItemExport["學籍相關匯出"]["匯出學生類別"].Enable = CurrentUser.Acl["Button0205"].Executable;
            //rbItemExport["學籍相關匯出"]["匯出學生類別"].Click += delegate
            //{
            //    Exporter exporter = new ExportCategory();
            //    ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
            //    exporter.InitializeExport(wizard);
            //    wizard.ShowDialog();
            //};

            //將匯出匯入缺曠獎懲,移至高中學務模組 - 20140312(dylan)
            //rbItemExport["學務相關匯出"]["匯出缺曠紀錄"].Enable = CurrentUser.Acl["Button0180"].Executable;
            //rbItemExport["學務相關匯出"]["匯出缺曠紀錄"].Click += delegate
            //{
            //    new ExportStudent(new ExportAbsence()).ShowDialog();
            //};

            //rbItemExport["學務相關匯出"]["匯出獎懲紀錄"].Enable = CurrentUser.Acl["Button0190"].Executable;
            //rbItemExport["學務相關匯出"]["匯出獎懲紀錄"].Click += delegate
            //{
            //    new ExportStudent(new ExportDiscipline()).ShowDialog();
            //};

            // 2018.09.27 [ischoolKingdom] Vicky依據 [H成績][H學務][06] 功能沒有設定權限管理 項目，將各功能按鈕註冊時Enable設定與系統權限綁定，權限Code使用GUID。
            rbItemExport["其它相關匯出"]["匯出自訂欄位"].Enable = CurrentUser.Acl["B2B63AFC-2019-4596-823A-BA044C8203F1"].Executable;
            rbItemExport["其它相關匯出"]["匯出自訂欄位"].Click += delegate
            {
                Exporter exporter = new ExportExtandField();
                ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };
            #endregion

            RibbonBarButton rbItemImport = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯入"];
            rbItemImport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            rbItemImport.SupposeHasChildern = true;
            rbItemImport.Image = Properties.Resources.Import_Image;

            #region 匯入(1000708)

            rbItemImport["學籍相關匯入"]["匯入學生基本資料"].Enable = CurrentUser.Acl["Button0210"].Executable;
            rbItemImport["學籍相關匯入"]["匯入學生基本資料"].Click += delegate
            {
                StudentImportWizard wizard = new StudentImportWizard();
                wizard.ShowDialog();
            };

            //20131216 - dylan 註解
            //rbItemImport["學籍相關匯入"]["匯入學生照片"].Enable = CurrentUser.Acl["Button0290"].Executable;
            //rbItemImport["學籍相關匯入"]["匯入學生照片"].Click += delegate
            //{
            //    new K12.Form.Photo.PhotosBatchImportForm().ShowDialog();
            //};

            //rbItemImport["學籍相關匯入"]["匯入學生類別"].Enable = CurrentUser.Acl["Button0285"].Executable;
            //rbItemImport["學籍相關匯入"]["匯入學生類別"].Click += delegate
            //{
            //    SmartSchool.API.PlugIn.Import.Importer importer = new ImportCategory();
            //    ImportStudentV2 wizard = new ImportStudentV2(importer.Text, importer.Image);
            //    importer.InitializeImport(wizard);
            //    wizard.ShowDialog();
            //};

            //將匯出匯入缺曠獎懲,移至高中學務模組 - 20140312(dylan)
            //rbItemImport["學務相關匯入"]["匯入獎懲紀錄"].Enable = CurrentUser.Acl["Button0270"].Executable;
            //rbItemImport["學務相關匯入"]["匯入獎懲紀錄"].Click += delegate
            //{
            //    SmartSchool.API.PlugIn.Import.Importer importer = new ImportDiscipline();
            //    ImportStudentV2 wizard = new ImportStudentV2(importer.Text, importer.Image);
            //    importer.InitializeImport(wizard);
            //    wizard.ShowDialog();
            //};

            //rbItemImport["學務相關匯入"]["匯入缺曠紀錄"].Enable = CurrentUser.Acl["Button0260"].Executable;
            //rbItemImport["學務相關匯入"]["匯入缺曠紀錄"].Click += delegate
            //{
            //    new ImportStudent(new ImportAbsence()).ShowDialog();
            //};

            // 2018.09.27 [ischoolKingdom] Vicky依據 [H成績][H學務][06] 功能沒有設定權限管理 項目，將各功能按鈕註冊時Enable設定與系統權限綁定，權限Code使用GUID。
            rbItemImport["其它相關匯入"]["匯入自訂欄位"].Enable = CurrentUser.Acl["F1D8F0DD-AC8B-442A-88DB-75A4E4208156"].Executable;
            rbItemImport["其它相關匯入"]["匯入自訂欄位"].Click += delegate
            {
                SmartSchool.API.PlugIn.Import.Importer importer = new ImportExtandField();
                ImportStudentV2 wizard = new ImportStudentV2(importer.Text, importer.Image);
                importer.InitializeImport(wizard);
                wizard.ShowDialog();
            };
            #endregion

            //本權限控制來自 - MOD_Tagging (Dylan - 20130315)
            if (CurrentUser.Acl["JHSchool.Student.Ribbon04150.Change20130315"].Executable)
            {
                ChangeStatusBatch.Init();//右鍵變更學生狀態
            }

            //班級相關 Ribbon
            SmartSchool.ClassRelated.RibbonBars.Manage.Instance.Setup();
            new ClassRelated.RibbonBars.Upgrade().Setup();
            new ClassRelated.RibbonBars.TeacherBiasRibbon().Setup();
            //            new ClassRelated.RibbonBars.Assign().Setup();
            new ClassRelated.RibbonBars.Report().Setup();
            new ClassRelated.RibbonBars.ImportExport().Setup();

            //教師相關 Ribbon
            SmartSchool.TeacherRelated.RibbonBars.Manage.Instance.Setup();
            new TeacherRelated.RibbonBars.Report().Setup();
            new TeacherRelated.RibbonBars.ImportExport().Setup();

            //設定Customization資料介面使用的的InformationProvider
            Customization.Data.AccessHelper.SetStudentProvider(new API.Provider.StudentProvider());
            Customization.Data.AccessHelper.SetClassProvider(new API.Provider.ClassProvider());
            Customization.Data.AccessHelper.SetTeacherProvider(new API.Provider.TeacherProvider());
            Customization.PlugIn.ExtendedContent.ExtendStudentContent.SetManager(Student.Instance);
            Customization.PlugIn.ExtendedContent.ExtendTeacherContent.SetManager(Class.Instance);
            Customization.PlugIn.ExtendedContent.ExtendTeacherContent.SetManager(Teacher.Instance);

            #region 匯出匯入學期對照表
            rbItemExport["成績相關匯出"]["匯出學期對照表"].Enable = UserAcl.Current["SHSchool.Student.Ribbon0169"].Executable;
            rbItemExport["成績相關匯出"]["匯出學期對照表"].Click += delegate
            {
                SmartSchool.API.PlugIn.Export.Exporter exporter = new SmartSchool.StudentRelated.RibbonBars.SemesterHistory.ExportSemesterHistory();
                SmartSchool.StudentRelated.RibbonBars.SemesterHistory.ExportStudentV2 wizard = new SmartSchool.StudentRelated.RibbonBars.SemesterHistory.ExportStudentV2(exporter.Text, exporter.Image);
                exporter.InitializeExport(wizard);
                wizard.ShowDialog();
            };


            rbItemImport["成績相關匯入"]["匯入學期對照表"].Enable = UserAcl.Current["SHSchool.Student.Ribbon0170"].Executable;
            rbItemImport["成績相關匯入"]["匯入學期對照表"].Click += delegate
            {
                SmartSchool.API.PlugIn.Import.Importer importer = new SmartSchool.StudentRelated.RibbonBars.SemesterHistory.ImportSemesterHistory();
                SmartSchool.StudentRelated.RibbonBars.SemesterHistory.ImportStudentV2 wizard = new SmartSchool.StudentRelated.RibbonBars.SemesterHistory.ImportStudentV2(importer.Text, importer.Image);
                importer.InitializeImport(wizard);
                wizard.ShowDialog();
            };

            Catalog ribbon = RoleAclSource.Instance["學生"]["功能按鈕"];
            ribbon.Add(new RibbonFeature("SHSchool.Student.Ribbon0169", "匯出學期對照表"));
            ribbon.Add(new RibbonFeature("SHSchool.Student.Ribbon0170", "匯入學期對照表"));
            ribbon.Add(new RibbonFeature("SHSchool.Student.Ribbon0171", "匯出離校資訊"));
            // 2018.09.27 [ischoolKingdom] Vicky依據 [H成績][H學務][06] 功能沒有設定權限管理 項目，將各功能按鈕註冊時Enable設定與系統權限綁定，權限Code使用GUID。
            ribbon.Add(new RibbonFeature("B2B63AFC-2019-4596-823A-BA044C8203F1", "匯出自訂欄位"));
            ribbon.Add(new RibbonFeature("F1D8F0DD-AC8B-442A-88DB-75A4E4208156", "匯入自訂欄位"));
            
            #endregion
        }

        public static void Init_Core_Others()
        {
            var departmentSetting = MotherForm.RibbonBarItems["教務作業", "基本設定"]["管理"];
            departmentSetting.Image = Properties.Resources.network_lock_64;
            departmentSetting.Size = RibbonBarButton.MenuButtonSize.Large;
            departmentSetting["科別對照管理"].Enable = CurrentUser.Acl["Button0790"].Executable;
            departmentSetting["科別對照管理"].Click += new EventHandler(departmentSetting_OnShown);

            var subjectChineseToEnglish = MotherForm.RibbonBarItems["教務作業", "基本設定"]["對照/代碼"];
            subjectChineseToEnglish.Size = RibbonBarButton.MenuButtonSize.Large;
            subjectChineseToEnglish.Image = Properties.Resources.notepad_lock_64;
            subjectChineseToEnglish["科目中英文對照表"].Enable = CurrentUser.Acl["Button0820"].Executable;
            subjectChineseToEnglish["科目中英文對照表"].Click += new EventHandler(subjectChineseToEnglishForm_OnShown);

            var DeptChineseToEnglish = MotherForm.RibbonBarItems["教務作業", "基本設定"]["對照/代碼"];
            DeptChineseToEnglish["科別中英文對照表"].Enable = CurrentUser.Acl["Button08201"].Executable;
            DeptChineseToEnglish["科別中英文對照表"].Click += new EventHandler(DeptChineseToEnglish_Click);

            var identityMappingTable = MotherForm.RibbonBarItems["教務作業", "基本設定"]["對照/代碼"];
            identityMappingTable["學籍身分對照表"].Enable = CurrentUser.Acl["Button0795"].Executable;
            identityMappingTable["學籍身分對照表"].Click += new EventHandler(identityMappingTable_OnShown);

            var degreeForm = MotherForm.RibbonBarItems["教務作業", "基本設定"]["管理"];
            degreeForm.Image = Properties.Resources.network_lock_64;
            degreeForm.Size = RibbonBarButton.MenuButtonSize.Large;
            degreeForm["等第對照管理"].Enable = CurrentUser.Acl["Button0720"].Executable;
            degreeForm["等第對照管理"].Click += new EventHandler(degreeForm_OnShown);

            var moralityForm = MotherForm.RibbonBarItems["學務作業", "基本設定"]["對照/代碼"];
            moralityForm.Image = Properties.Resources.notepad_lock_64;
            moralityForm.Size = RibbonBarButton.MenuButtonSize.Large;
            moralityForm["德行評語代碼表"].Enable = CurrentUser.Acl["Button0760"].Executable;
            moralityForm["德行評語代碼表"].Click += new EventHandler(moralityForm_OnShown);

            var wordCommentForm = MotherForm.RibbonBarItems["學務作業", "基本設定"]["對照/代碼"];
            wordCommentForm["文字評量代碼表"].Enable = CurrentUser.Acl["Button0765"].Executable;
            wordCommentForm["文字評量代碼表"].Click += new EventHandler(wordCommentForm_OnShown);

            //預設"學務作業"畫面的圖和大小
            var defConfig = MotherForm.RibbonBarItems["學務作業", "基本設定"]["管理"];
            defConfig.Image = Properties.Resources.network_lock_64;
            defConfig.Size = RibbonBarButton.MenuButtonSize.Large;
        }

        static void DeptChineseToEnglish_Click(object sender, EventArgs e)
        {
            DeptChineseToEnglishForm dcte = new DeptChineseToEnglishForm();
            dcte.ShowDialog();
        }

        private static void wordCommentForm_OnShown(object sender, EventArgs e)
        {
            TextCommentForm form = new TextCommentForm();
            form.ShowDialog();
        }

        private static void identityMappingTable_OnShown(object sender, EventArgs e)
        {
            IdentityForm form = new IdentityForm();
            form.ShowDialog();
        }

        static void moralityForm_OnShown(object sender, EventArgs e)
        {
            MoralityForm form = new MoralityForm();
            form.ShowDialog();
        }

        static void degreeForm_OnShown(object sender, EventArgs e)
        {
            DegreeForm form = new DegreeForm();
            form.ShowDialog();
        }

        static void subjectChineseToEnglishForm_OnShown(object sender, EventArgs e)
        {
            SubjectChineseToEnglishForm form = new SubjectChineseToEnglishForm();
            form.ShowDialog();
        }

        static void departmentSetting_OnShown(object sender, EventArgs e)
        {
            DeptSetup form = new DeptSetup();
            form.ShowDialog();
        }
    }
}
