using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Security;
using FISCA.Presentation;
//using SmartSchool.Common;

namespace SmartSchool.Others.RibbonBars
{
    public partial class ScoreOpenTime : SmartSchool.Others.RibbonBars.RibbonBarBase
    {
        FeatureAccessControl setupCtrl;

        public ScoreOpenTime()
        {
            //InitializeComponent();

        }

        internal void Setup()
        {
            MotherForm.RibbonBarItems["學務作業", "其它"].Index = 9;

            var btnSetup = MotherForm.RibbonBarItems["學務作業", "其它"]["開放時間設定"];
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScoreOpenTime));
            btnSetup.Image = ( (System.Drawing.Image)( resources.GetObject("btnSetup.Image") ) );
            btnSetup.Click += new System.EventHandler(this.btnSetup_Click);
            btnSetup.Size = RibbonBarButton.MenuButtonSize.Large;

            ////權限判斷 - 其它/開放時間設定
            setupCtrl = new FeatureAccessControl("Button0710");
            btnSetup.Enable=setupCtrl.Executable();
        }

        public override string ProcessTabName
        {
            get
            {
                return "學務作業";
            }
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            TeacherDiffOpenConfig.Display();
        }
    }
}

