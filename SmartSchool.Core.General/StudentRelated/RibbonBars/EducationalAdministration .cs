using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using SmartSchool.Security;
using SmartSchool.StudentRelated.RibbonBars.AcademicAffairs;
using FISCA.Presentation;

namespace SmartSchool.StudentRelated.RibbonBars
{
    public partial class EducationalAdministration : RibbonBarBase
    {
        FeatureAccessControl placeCtrl;
        FeatureAccessControl diplomaCtrl;
        FeatureAccessControl lvlEduCtrl;

        ButtonItemPlugInManager reportManager;

        public EducationalAdministration()
        {
        }

        internal void Setup()
        {
            //K12.Presentation.NLDPanels.Student.RibbonBarItems["教務作業"].Index = 2;
            //K12.Presentation.NLDPanels.Student.RibbonBarItems["教務作業"].AutoOverflowEnabled = false;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EducationalAdministration));

            RibbonBarButton btnPlacing = K12.Presentation.NLDPanels.Student.RibbonBarItems["教務"]["排名作業"];
            //btnPlacing.Image = ( (System.Drawing.Image)( resources.GetObject("btnPlacing.Image") ) );
            btnPlacing.Size = RibbonBarButton.MenuButtonSize.Large;
            btnPlacing.Image = Properties.Resources.refresh_window_64;
            btnPlacing["排名"].Click += new System.EventHandler(this.buttonItem2_Click);

            RibbonBarButton btnInOut = K12.Presentation.NLDPanels.Student.RibbonBarItems["教務"]["新生作業"];
            btnInOut.Size = RibbonBarButton.MenuButtonSize.Large;
            btnInOut.Image = Properties.Resources.college_write_64;
            btnInOut.SupposeHasChildern = true;
            //btnInOut["產生教育程度資料檔"].Click += new System.EventHandler(this.btnEduLevel_Click);

            RibbonBarButton btnDiploma = K12.Presentation.NLDPanels.Student.RibbonBarItems["教務"]["畢業作業"];
            //btnDiploma.Image = ( (System.Drawing.Image)( resources.GetObject("btnDiploma.Image") ) );
            btnDiploma.Size = RibbonBarButton.MenuButtonSize.Large;
            btnDiploma.Image = Properties.Resources.graduated_write_64;
            btnDiploma["證書字號"].Click += new System.EventHandler(this.buttonItem1_Click);

            //MenuButton btnEduLevel = K12.Presentation.NLDPanels.Student.RibbonBarItems["教務作業"]["畢業作業"];
            btnDiploma["產生教育程度資料檔"].Click += new System.EventHandler(this.btnEduLevel_Click);

            //權限判斷 - 排名	Button0050
            placeCtrl = new FeatureAccessControl("Button0050");

            //權限判斷 - 證書字號	Button0090
            diplomaCtrl = new FeatureAccessControl("Button0090");

            //權限判斷 - 教育程度檔	Button0092
            lvlEduCtrl = new FeatureAccessControl("Button0092");

            //btnInOut["產生教育程度資料檔"].Enable = lvlEduCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
            btnDiploma["證書字號"].Enable = diplomaCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
            btnPlacing["排名"].Enable = placeCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
            btnDiploma["產生教育程度資料檔"].Enable = lvlEduCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;

            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate
            {
                //btnInOut.Enable = lvlEduCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
                btnDiploma["證書字號"].Enable = diplomaCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
                btnPlacing["排名"].Enable = placeCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
                btnDiploma["產生教育程度資料檔"].Enable = lvlEduCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
            };
        }

        void reportManager_ItemsChanged(object sender, EventArgs e)
        {
            itemContainer2.SubItems.Remove(this.btnPlacing);
            itemContainer2.SubItems.Remove(this.btnMetagenesis);
            itemContainer2.SubItems.Add(this.btnPlacing);
            itemContainer2.SubItems.Add(this.btnMetagenesis);
        }

        //void Instance_SelectionChanged(object sender, EventArgs e)
        //{
        //    btnDiploma.Enabled = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
        //    btnPlacing.Enabled = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
        //    btnEduLevel.Enabled = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;

        //    placeCtrl.Inspect(btnPlacing);
        //    diplomaCtrl.Inspect(btnDiploma);
        //    lvlEduCtrl.Inspect(btnEduLevel);
        //}

        public override string ProcessTabName
        {
            get { return "學生"; }
        }

        private void buttonItem1_Click(object sender, EventArgs e)
        {
            DiplomaNumberCreator creator = new DiplomaNumberCreator();
            creator.ShowDialog();
        }

        private void buttonItem2_Click(object sender, EventArgs e)
        {
            SmartSchool.StudentRelated.Placing.PlaceForm form = new SmartSchool.StudentRelated.Placing.PlaceForm();
            form.ShowDialog();
        }

        private void btnEduLevel_Click(object sender, EventArgs e)
        {
            LevelOfEducationForm form = new LevelOfEducationForm();
            form.ShowDialog();
        }
    }
}
