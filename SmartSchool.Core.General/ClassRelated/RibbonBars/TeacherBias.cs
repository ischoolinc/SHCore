using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.ClassRelated.RibbonBars.DeXing;
using SmartSchool.Security;

namespace SmartSchool.ClassRelated.RibbonBars
{
    public partial class TeacherBiasRibbon : RibbonBarBase
    {
        FeatureAccessControl adjustCtrl;
        FeatureAccessControl textCtrl;

        public TeacherBiasRibbon()
        {
            //InitializeComponent();
            ////SmartSchool.ClassRelated.Class.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);
            //SmartSchool.Broadcaster.Events.Items["班級/選取變更"].Handler += delegate
            //{
            //    btnAdjust.Enabled = ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 1 );
            //    adjustCtrl.Inspect(btnAdjust);
            //    btnWordComment.Enabled = (SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 1);
            //    textCtrl.Inspect(btnWordComment);
            //};

            //#region 權限
            ////權限判斷 - 學務作業/德行加減分
            //adjustCtrl = new FeatureAccessControl("Button0370");
            //adjustCtrl.Inspect(btnAdjust);
            ////權限判斷 - 學務作業/文字評量
            //textCtrl = new FeatureAccessControl("Button0375");
            //textCtrl.Inspect(btnWordComment);
            //#endregion
        }

        internal void Setup()
        {

            #region 權限
            //權限判斷 - 學務作業/德行加減分
            adjustCtrl = new FeatureAccessControl("Button0370");
            //權限判斷 - 學務作業/文字評量
            textCtrl = new FeatureAccessControl("Button0375");
            #endregion

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TeacherBiasRibbon));
            var btnAdjust = K12.Presentation.NLDPanels.Class.RibbonBarItems["學務"]["德行加減分"];
            btnAdjust.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Medium;
            //btnAdjust.Image = ( (System.Drawing.Image)( resources.GetObject("btnAdjust.Image") ) );
            btnAdjust.Image = Properties.Resources.tutorial_fav_64;
            btnAdjust.Click += new System.EventHandler(this.buttonItem1_Click);
            btnAdjust.Enable = (SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 1) && adjustCtrl.Executable();
            
            var btnWordComment = K12.Presentation.NLDPanels.Class.RibbonBarItems["學務"]["文字評量"];
            btnWordComment.Image = Properties.Resources.subject_64;
            btnWordComment.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Medium;
            btnWordComment.Click += new System.EventHandler(this.btnWordComment_Click);
            btnWordComment.Enable = ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 1 ) && textCtrl.Executable();
            
            K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
            {
                btnAdjust.Enable = ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 1 ) && adjustCtrl.Executable();
                btnWordComment.Enable = ( SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 1 ) && textCtrl.Executable();
            };
        }

        //void Instance_SelectionChanged(object sender, EventArgs e)
        //{
        //    btnAdjust.Enabled = (SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count == 1);
        //    adjustCtrl.Inspect(btnAdjust);
        //}

        private void buttonItem1_Click(object sender, EventArgs e)
        {
            TeacherBias form = new TeacherBias();
            form.ShowDialog();
        }

        private void btnWordComment_Click(object sender, EventArgs e)
        {
            //WordNewForm form = new WordNewForm();
            WordComment form = new WordComment();
            form.ShowDialog();
        }
    }
}
