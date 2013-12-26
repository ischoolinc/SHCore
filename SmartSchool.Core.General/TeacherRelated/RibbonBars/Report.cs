using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SmartSchool.TeacherRelated.RibbonBars
{
    public partial class Report : SmartSchool.TeacherRelated.RibbonBars.RibbonBarBase, SmartSchool.Customization.PlugIn.Report.IReportManager
    {
        public Report()
        {
            //InitializeComponent();
        }

        private Adaatper.ButtonAdapterPlugInToMenuButton reportManager;
        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Report));
            var buttonItem114 = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["資料統計"]["報表"];
            buttonItem114.Image = Properties.Resources.paste_64;
            buttonItem114.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            buttonItem114.SupposeHasChildern = true;
            buttonItem114.Enable = false;
            K12.Presentation.NLDPanels.Teacher.SelectedSourceChanged += delegate
            {
                bool hasChildern = false;
                foreach ( var item in buttonItem114.Items )
                {
                    hasChildern = true;
                }
                buttonItem114.Enable = ( K12.Presentation.NLDPanels.Teacher.SelectedSource.Count > 0 && hasChildern );
            };
            #region 設定為班級的報表外掛處理者
            reportManager = new Adaatper.ButtonAdapterPlugInToMenuButton(buttonItem114);
            SmartSchool.Customization.PlugIn.Report.TeacherReport.SetManager(this);
            #endregion
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
    }
}

