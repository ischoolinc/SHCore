using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.CourseRelated.RibbonBars.Export;
using SmartSchool.CourseRelated.RibbonBars.Import;
using SmartSchool.Common;
using SmartSchool.Security;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class ImportExport : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase
    {
        FeatureAccessControl exportCtrl;
        FeatureAccessControl importCtrl;

        public ImportExport()
        {
        }

        internal void Setup()
        {
            //權限判斷 - 匯出課程
            exportCtrl = new FeatureAccessControl("Button0600");
            //權限判斷 - 匯入課程
            importCtrl = new FeatureAccessControl("Button0610");

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportExport));
            
            var btnExport = K12.Presentation.NLDPanels.Course.RibbonBarItems["資料統計"]["匯出"];
            btnExport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnExport.Image = (System.Drawing.Image)Properties.Resources.Export_Image;
            //btnExport.Image = ( (System.Drawing.Image)( resources.GetObject("btnExport.Image") ) );
            btnExport["匯出課程基本資料"].Click += new System.EventHandler(this.buttonItem109_Click);
            btnExport["匯出課程基本資料"].Enable = exportCtrl.Executable();

            var btnImport = K12.Presentation.NLDPanels.Course.RibbonBarItems["資料統計"]["匯入"];
            btnImport.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnImport.Image = (System.Drawing.Image)Properties.Resources.Import_Image;
            //btnImport.Image = ( (System.Drawing.Image)( resources.GetObject("btnImport.Image") ) );
            btnImport["匯入課程基本資料"].Click += new System.EventHandler(this.buttonItem102_Click);
            btnImport["匯入課程基本資料"].Enable = importCtrl.Executable();
        }

        private void buttonItem109_Click(object sender, EventArgs e)
        {
            ExportForm form = new ExportForm();
            form.ShowDialog();
        }

        private void buttonItem102_Click(object sender, EventArgs e)
        {
            CourseImportWizard form = new CourseImportWizard();
            form.ShowDialog();
        }
    }
}

