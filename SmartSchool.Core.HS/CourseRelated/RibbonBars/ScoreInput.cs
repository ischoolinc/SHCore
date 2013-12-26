using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.CourseRelated.Forms;
using SmartSchool.CourseRelated;
using SmartSchool.Common;
using SmartSchool.Security;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class ScoreInput : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase
    {
        FeatureAccessControl editScoreCtrl;
        FeatureAccessControl calculateCtrl;

        public ScoreInput()
        {
        }

        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScoreInput));
            editScoreCtrl = new FeatureAccessControl("Button0550");
            calculateCtrl = new FeatureAccessControl("Button0560");

            var btnEditScore = K12.Presentation.NLDPanels.Course.RibbonBarItems["教務"]["成績輸入"];
            btnEditScore.Image = ( (System.Drawing.Image)( resources.GetObject("btnEditScore.Image") ) );
            btnEditScore.Click += new System.EventHandler(this.btnEditScore_Click);
            //btnEditScore.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnEditScore.Enable = editScoreCtrl.Executable() && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count == 1 );

            var btnCalculate = K12.Presentation.NLDPanels.Course.RibbonBarItems["教務"]["成績計算"];
            btnCalculate.Image = ( (System.Drawing.Image)( resources.GetObject("btnCalcuate.Image") ) );
            btnCalculate.Click += new System.EventHandler(this.btnCalcuate_Click);
            //btnCalculate.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnCalculate.Enable = calculateCtrl.Executable() && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );
            Course.Instance.SelectionChanged += delegate
            {
                btnEditScore.Enable = editScoreCtrl.Executable() && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count == 1 );
                btnCalculate.Enable = calculateCtrl.Executable() && ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 );
            };
        }
        private void btnEditScore_Click(object sender, EventArgs e)
        {
            if ( K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0 )
                EditCourseScore.DisplayCourseScore(Course.Instance.SelectionCourse[0]);
        }

        private void btnCalcuate_Click(object sender, EventArgs e)
        {

            SmartSchool.CourseRelated.RibbonBars.ScoresCalc.Forms.CalculateWizard wizard = new SmartSchool.CourseRelated.RibbonBars.ScoresCalc.Forms.CalculateWizard();
            wizard.ShowDialog();
        }

    }
}

