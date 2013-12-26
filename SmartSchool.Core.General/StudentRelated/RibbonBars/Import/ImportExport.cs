using System;
using System.Collections.Generic;
using FISCA.Presentation;
using SmartSchool.AccessControl;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.ImportExport;
using SmartSchool.Security;

namespace SmartSchool.StudentRelated.RibbonBars.Import
{
    public partial class ImportExport : RibbonBarBase, IManager<ImportProcess>, IManager<ExportProcess>
    {
        FeatureAccessControl exportStudentCtrl;
        FeatureAccessControl importStudentCtrl;
        FeatureAccessControl importPhotoCtrl;

        private Dictionary<string, object> _ImportItems = new Dictionary<string, object>();
        private Dictionary<string, object> _ExportItems = new Dictionary<string, object>();

        public ImportExport()
        {
            //InitializeComponent();
            //superTooltip1.DefaultFont = FontStyles.General;

            //#region 設定群組
            //_StudentRecord = new GalleryGroup();
            //_SemesterSubjectScore = new GalleryGroup();
            //_Photo = new GalleryGroup();
            //// 
            //// _StudentRecord
            //// 
            //_StudentRecord.Name = "學籍基本資料";
            //_StudentRecord.Text = "<b>學籍基本資料</b>";
            //// 
            //// _SemesterSubjectScore
            //// 
            //_SemesterSubjectScore.DisplayOrder = 1;
            //_SemesterSubjectScore.Name = "學期科目成績";
            //_SemesterSubjectScore.Text = "<b>學期科目成績</b>";
            //// 
            //// _Photo
            //// 
            //_Photo.DisplayOrder = int.MaxValue;
            //_Photo.Name = "照片";
            //_Photo.Text = "<b>照片</b>";

            //this.galleryContainer1.GalleryGroups.AddRange(new DevComponents.DotNetBar.GalleryGroup[] {
            //_StudentRecord,
            //_SemesterSubjectScore,_Photo});

            //_Group.Add("學籍基本資料", _StudentRecord);

            //_Group.Add("學期科目成績", _SemesterSubjectScore);

            //_Group.Add("照片", _Photo);

            //// 2008/4/9
            ////this.galleryContainer1.SetGalleryGroup(this.btnExport, _StudentRecord);
            ////this.galleryContainer1.SetGalleryGroup(this.btnImport, _StudentRecord); ;
            ////this.galleryContainer1.SetGalleryGroup(this.buttonItem1, _Photo);

            //#endregion
        }

        internal void Setup()
        {
            //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExport));

            //K12.Presentation.NLDPanels.Student.RibbonBarItems["匯出/匯入"].OverflowButtonImage = ( (System.Drawing.Image)( resources.GetObject("MainRibbonBar.OverflowButtonImage") ) );
            //K12.Presentation.NLDPanels.Student.RibbonBarItems["匯出/匯入"].ResizeOrderIndex = 55;


            var btnExportList = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯出"]["其它相關匯出"];
            //btnExportList.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            //btnExportList.SupposeHasChildern = true;
            //btnExportList.Image = Properties.Resources.Export_Image;
            //btnExportList.Image = ((System.Drawing.Image)(resources.GetObject("btnExportList.Image")));
            btnExportList.PopupOpen += new EventHandler<FISCA.Presentation.PopupOpenEventArgs>(btnExportList_PopupOpen);

            var btnImportList = K12.Presentation.NLDPanels.Student.RibbonBarItems["資料統計"]["匯入"]["其它相關匯入"];
            //btnImportList.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            //btnImportList.SupposeHasChildern = true;
            //btnImportList.Image = Properties.Resources.Import_Image;
            //btnImportList.Image = ((System.Drawing.Image)(resources.GetObject("btnImportList.Image")));
            btnImportList.PopupOpen += new EventHandler<PopupOpenEventArgs>(btnImportList_PopupOpen);


            SmartSchool.Customization.PlugIn.ImportExport.ImportStudent.SetManager(this);
            SmartSchool.Customization.PlugIn.ImportExport.ExportStudent.SetManager(this);

            foreach (SmartSchool.API.PlugIn.Import.Importer var in API.PlugIn.PlugInManager.Student.Importers)
            {
                AddImport(var);
            }
            API.PlugIn.PlugInManager.Student.Importers.ItemAdded += delegate(object sender, SmartSchool.API.PlugIn.ItemEventArgs<SmartSchool.API.PlugIn.Import.Importer> ea)
            {
                AddImport(ea.Item);
            };
            foreach (SmartSchool.API.PlugIn.Export.Exporter var in API.PlugIn.PlugInManager.Student.Exporters)
            {
                AddExport(var);
            }
            API.PlugIn.PlugInManager.Student.Exporters.ItemAdded += delegate(object sender, SmartSchool.API.PlugIn.ItemEventArgs<SmartSchool.API.PlugIn.Export.Exporter> ea)
            {
                AddExport(ea.Item);
            };
        }

        void btnExportList_PopupOpen(object sender, PopupOpenEventArgs e)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExport));
            List<string> items = new List<string>(_ExportItems.Keys);
            items.Add("匯出學籍資料");
            //items.Add("匯出照片");
            #region Sort
            items.Sort(delegate(string a, string b)
        {
            return SmartSchool.Common.StringComparer.Comparer(a, b,
                "匯出學籍資料",
                "匯出學期科目成績",
                "匯出學期分項成績",
                "匯出學年科目成績",
                "匯出學年分項成績",
                "匯出缺曠紀錄",
                "匯出獎懲紀錄",
                "匯出異動紀錄",
                "匯出新生異動",
                "匯出轉入異動",
                "匯出學籍異動",
                "匯出畢業異動",
                "匯出畢業成績",
                "匯出自訂欄位",
                "匯出學生類別",
                //"匯出照片",
                "匯出離校資訊",
                "匯入學籍資料",
                "匯入學期科目成績",
                "匯入學期分項成績",
                "匯入學年科目成績",
                "匯入學年分項成績",
                "匯入缺曠紀錄",
                "匯入獎懲紀錄",
                "匯入異動紀錄",
                "匯入新生異動",
                "匯入轉入異動",
                "匯入學籍異動",
                "匯入畢業異動",
                //"匯入照片",
                "匯入自訂欄位",
                "匯入學生類別");
        }); 
            #endregion
            foreach ( var path in items )
            {
                if ( path == "匯出學籍資料" )
                {
                    #region 權限
                    //權限判斷 - 匯出學籍資料	Button0130
                    exportStudentCtrl = new FeatureAccessControl("Button0130");
                    #endregion
                    var btnExport = e.VirtualButtons["匯出學籍資料"];
                    //btnExport.Image = ( (System.Drawing.Image)( resources.GetObject("btnExport.Image") ) );
                    btnExport.Enable = exportStudentCtrl.Executable();
                    btnExport.Click += new System.EventHandler(this.buttonItem88_Click);
                }
                else if ( path == "匯出照片")
                {
                    #region 權限
                    //權限判斷 - 匯入照片	Button0290
                    //importPhotoCtrl = new FeatureAccessControl("Button0290");
                    #endregion
                    //var btnExportPhoto = e.VirtualButtons["匯出照片"];
                    //btnExportPhoto.Image = ( (System.Drawing.Image)( resources.GetObject("btnImportPhoto.Image")));
                    //btnExportPhoto.Enable = exportStudentCtrl.Executable();
                    //btnExportPhoto.Click += (vsender, ve) => (new K12.Form.Photo.PhotosBatchExportForm()).ShowDialog();
                    //btnExportPhoto.Click += (vsender, ve) => (new JHSchool.Permrec.StudentExtendControls.PhotosBatchImportExports.PhotosBatchExportForm()).ShowDialog();
                    //btnExportPhoto.Click += new System.EventHandler(this.buttonItem1_Click_1);
                }
                else if ( _ExportItems[path] is SmartSchool.API.PlugIn.Export.Exporter )
                {
                    SmartSchool.API.PlugIn.Export.Exporter exporter = (SmartSchool.API.PlugIn.Export.Exporter)_ExportItems[path];
                    if ( !Attribute.IsDefined(exporter.GetType(), typeof(FeatureCodeAttribute)) || CurrentUser.Acl[exporter.GetType()].Executable )
                    {
                        var btn = e.VirtualButtons.Items[path.Split('/')];
                        btn.Image = exporter.Image;
                        btn.Tag = exporter;
                        btn.Click += delegate(object sender1, EventArgs e1)
                        {
                            var exp = ( (SmartSchool.API.PlugIn.Export.Exporter)( (MenuButton)sender1 ).Tag );
                            ExportStudentV2 wizard = new ExportStudentV2(exp.Text, exp.Image);
                            exp.InitializeExport(wizard);
                            wizard.ShowDialog();
                        };
                    }
                }
                else if ( _ExportItems[path] is ExportProcess )
                {
                    ExportProcess instance = (ExportProcess)_ExportItems[path];
                    var btn = e.VirtualButtons.Items[instance.Title];
                    if ( instance.Image != null )
                        btn.Image = instance.Image;
                    if ( Attribute.IsDefined(instance.GetType(), typeof(FeatureCodeAttribute)) )
                    {
                        if ( !CurrentUser.Acl[instance.GetType()].Executable )
                            btn.Enable = false;
                    }
                    btn.Tag = instance;
                    btn.Click += delegate(object s1, EventArgs e1)
                    {
                        new ExportStudent((ExportProcess)( (MenuButton)s1 ).Tag).ShowDialog();
                    };
                }
            }
        }

        void btnImportList_PopupOpen(object sender, PopupOpenEventArgs e)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExport));
            List<string> items = new List<string>(_ImportItems.Keys);
            items.Add("匯入學籍資料");
            //items.Add("匯入照片");
            items.Sort(
                delegate(string a, string b)
                {
                    return SmartSchool.Common.StringComparer.Comparer(a, b,
                        "匯出學籍資料",
                        "匯出學期科目成績",
                        "匯出學期分項成績",
                        "匯出學年科目成績",
                        "匯出學年分項成績",
                        "匯出缺曠紀錄",
                        "匯出獎懲紀錄",
                        "匯出異動紀錄",
                        "匯出新生異動",
                        "匯出轉入異動",
                        "匯出學籍異動",
                        "匯出畢業異動",
                        "匯出畢業成績",
                        "匯出自訂欄位",
                        //"匯出照片",
                        "匯出學生類別",
                        "匯出離校資訊",
                        "匯入學籍資料",
                        "匯入學期科目成績",
                        "匯入學期分項成績",
                        "匯入學年科目成績",
                        "匯入學年分項成績",
                        "匯入缺曠紀錄",
                        "匯入獎懲紀錄",
                        "匯入異動紀錄",
                        "匯入新生異動",
                        "匯入轉入異動",
                        "匯入學籍異動",
                        "匯入畢業異動",
                        //"匯入照片",
                        "匯入自訂欄位",
                        "匯入學生類別");
                });
            foreach ( var path in items )
            {
                if ( path == "匯入學籍資料" )
                {

                    #region 權限
                    //權限判斷 - 匯入學籍資料	Button0210
                    importStudentCtrl = new FeatureAccessControl("Button0210");
                    #endregion
                    var btnImport = e.VirtualButtons["匯入學籍資料"];
                    //btnImport.Image = ( (System.Drawing.Image)( resources.GetObject("btnImport.Image") ) );
                    btnImport.Enable = importStudentCtrl.Executable();
                    btnImport.Click += new System.EventHandler(this.btnImport_Click);
                }
                else if ( path == "匯入照片" )
                {
                    #region 權限
                    //權限判斷 - 匯入照片	Button0290
                    //importPhotoCtrl = new FeatureAccessControl("Button0290");
                    #endregion
                    //var btnImportPhoto = e.VirtualButtons["匯入照片"];
                    //btnImportPhoto.Image = ( (System.Drawing.Image)( resources.GetObject("btnImportPhoto.Image") ) );
                    //btnImportPhoto.Enable = importPhotoCtrl.Executable();
                    //btnImportPhoto.Click += (vsender, ve) => (new K12.Form.Photo.PhotosBatchImportForm()).ShowDialog();
                    //btnImportPhoto.Click += new System.EventHandler(this.buttonItem1_Click_1);
                }
                else if ( _ImportItems[path] is SmartSchool.API.PlugIn.Import.Importer )
                {
                    SmartSchool.API.PlugIn.Import.Importer importer = (SmartSchool.API.PlugIn.Import.Importer)_ImportItems[path];
                    if ( !Attribute.IsDefined(importer.GetType(), typeof(FeatureCodeAttribute)) || CurrentUser.Acl[importer.GetType()].Executable )
                    {
                        var btn = e.VirtualButtons.Items[path.Split('/')];
                        btn.Image = importer.Image;
                        btn.Tag = importer;
                        btn.Click += delegate(object sender1, EventArgs e1)
                        {
                            var imp = ( (SmartSchool.API.PlugIn.Import.Importer)( (MenuButton)sender1 ).Tag );
                            Import.ImportStudentV2 wizard = new ImportStudentV2(imp.Text, imp.Image);
                            imp.InitializeImport(wizard);
                            wizard.ShowDialog();
                        };
                    }
                }
                else if ( _ImportItems[path] is ImportProcess )
                {
                    ImportProcess instance = (ImportProcess)_ImportItems[path];
                    var btn = e.VirtualButtons.Items[instance.Title];
                    if ( instance.Image != null )
                        btn.Image = instance.Image;
                    if ( Attribute.IsDefined(instance.GetType(), typeof(FeatureCodeAttribute)) )
                    {
                        if ( !CurrentUser.Acl[instance.GetType()].Executable )
                            btn.Enable = false;
                    }
                    btn.Tag = instance;
                    btn.Click += delegate(object s1, EventArgs e1)
                    {
                        new ImportStudent((ImportProcess)( (MenuButton)s1 ).Tag).ShowDialog();
                    };
                }
            }
        }

        private void AddImport(SmartSchool.API.PlugIn.Import.Importer importer)
        {
            _ImportItems.Add(string.IsNullOrEmpty(importer.Path) ? importer.Text : importer.Path + "/" + importer.Text, importer);
        }

        private void AddExport(SmartSchool.API.PlugIn.Export.Exporter exporter)
        {
            _ExportItems.Add(string.IsNullOrEmpty(exporter.Path) ? exporter.Text : exporter.Path + "/" + exporter.Text, exporter);
        }

        public override string ProcessTabName
        {
            get
            {
                return "學生";
            }
        }


        #region IManager<ImportProcess> 成員
        public void Add(ImportProcess instance)
        {
            _ImportItems.Add(instance.Title, instance);
        }

        public void Remove(ImportProcess instance)
        {

        }

        #endregion

        #region IManager<ExportProcess> 成員

        public void Add(ExportProcess instance)
        {
            _ExportItems.Add(instance.Title, instance);
        }

        public void Remove(ExportProcess instance)
        {
        }

        #endregion


        private void btnImport_Click(object sender, EventArgs e)
        {
            StudentImportWizard wizard = new StudentImportWizard();
            wizard.ShowDialog();
        }
        private void buttonItem88_Click(object sender, EventArgs e)
        {
            //MsgBox.Show("開發中功能。"); 
            ExportWizard export = new ExportWizard();
            export.ShowDialog();
        }

        //已由K12.Form.Photo取代
        //private void buttonItem1_Click_1(object sender, EventArgs e)
        //{
        //    new SmartSchool.StudentRelated.RibbonBars.Import.BatchUploadPhotoForm().ShowDialog();
        //}
    }
}