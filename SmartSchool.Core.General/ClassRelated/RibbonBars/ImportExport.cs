using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.ClassRelated.RibbonBars.Export;
using SmartSchool.ClassRelated.RibbonBars.Import;
using SmartSchool.Security;

namespace SmartSchool.ClassRelated.RibbonBars
{
    public partial class ImportExport : RibbonBarBase
    {
        FeatureAccessControl exportCtrl;
        FeatureAccessControl importCtrl;
        ButtonItemPlugInManager buttonManager;

        public ImportExport()
        {
            //InitializeComponent();

            //#region 設定為 "班級/匯出匯入" 的外掛處理者
            //buttonManager = new ButtonItemPlugInManager(itemContainer1);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<DevComponents.DotNetBar.ButtonItem>.Instance.Add("班級/匯出匯入", buttonManager);
            //#endregion

        }
        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExport));
            //權限判斷 - 匯出班級
            exportCtrl = new FeatureAccessControl("Button0420");
            //權限判斷 - 匯入班級
            importCtrl = new FeatureAccessControl("Button0430");
            var Bar = K12.Presentation.NLDPanels.Class.RibbonBarItems["資料統計"];
            var btnExport = Bar["匯出"];
            btnExport.Image = (System.Drawing.Image)Properties.Resources.Export_Image;
            //btnExport.Image = ( (System.Drawing.Image)( resources.GetObject("btnExport.Image") ) );
            btnExport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnExport.SupposeHasChildern = true;
            btnExport["匯出班級基本資料"].Click += new System.EventHandler(this.btnExportClass_Click);

            var btnImport = Bar["匯入"];
            btnImport.Image = (System.Drawing.Image)Properties.Resources.Import_Image;
            //btnImport.Image = ( (System.Drawing.Image)( resources.GetObject("btnImport.Image") ) );
            btnImport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnImport.SupposeHasChildern = true;
            btnImport["匯入班級基本資料"].Click += new System.EventHandler(this.btnImportClass_Click);

            btnExport["匯出班級基本資料"].Enable = exportCtrl.Executable();
            btnImport["匯入班級基本資料"].Enable = importCtrl.Executable();
        }

        private void btnExportClass_Click(object sender, EventArgs e)
        {
            ExportClass form = new ExportClass();
            form.ShowDialog();
        }

        private void btnImportClass_Click(object sender, EventArgs e)
        {
            ClassImportWizard wizard = new ClassImportWizard();
            wizard.ShowDialog();
        }


    }
}
