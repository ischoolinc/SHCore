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
            //K12.Presentation.NLDPanels.Student.RibbonBarItems["�аȧ@�~"].Index = 2;
            //K12.Presentation.NLDPanels.Student.RibbonBarItems["�аȧ@�~"].AutoOverflowEnabled = false;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EducationalAdministration));

            RibbonBarButton btnPlacing = K12.Presentation.NLDPanels.Student.RibbonBarItems["�а�"]["�ƦW�@�~"];
            //btnPlacing.Image = ( (System.Drawing.Image)( resources.GetObject("btnPlacing.Image") ) );
            btnPlacing.Size = RibbonBarButton.MenuButtonSize.Large;
            btnPlacing.Image = Properties.Resources.refresh_window_64;
            btnPlacing["�ƦW"].Click += new System.EventHandler(this.buttonItem2_Click);

            RibbonBarButton btnInOut = K12.Presentation.NLDPanels.Student.RibbonBarItems["�а�"]["�s�ͧ@�~"];
            btnInOut.Size = RibbonBarButton.MenuButtonSize.Large;
            btnInOut.Image = Properties.Resources.college_write_64;
            btnInOut.SupposeHasChildern = true;
            //btnInOut["���ͱШ|�{�׸����"].Click += new System.EventHandler(this.btnEduLevel_Click);

            RibbonBarButton btnDiploma = K12.Presentation.NLDPanels.Student.RibbonBarItems["�а�"]["���~�@�~"];
            //btnDiploma.Image = ( (System.Drawing.Image)( resources.GetObject("btnDiploma.Image") ) );
            btnDiploma.Size = RibbonBarButton.MenuButtonSize.Large;
            btnDiploma.Image = Properties.Resources.graduated_write_64;
            btnDiploma["�ҮѦr��"].Click += new System.EventHandler(this.buttonItem1_Click);

            //MenuButton btnEduLevel = K12.Presentation.NLDPanels.Student.RibbonBarItems["�аȧ@�~"]["���~�@�~"];
            btnDiploma["���ͱШ|�{�׸����"].Click += new System.EventHandler(this.btnEduLevel_Click);

            //�v���P�_ - �ƦW	Button0050
            placeCtrl = new FeatureAccessControl("Button0050");

            //�v���P�_ - �ҮѦr��	Button0090
            diplomaCtrl = new FeatureAccessControl("Button0090");

            //�v���P�_ - �Ш|�{����	Button0092
            lvlEduCtrl = new FeatureAccessControl("Button0092");

            //btnInOut["���ͱШ|�{�׸����"].Enable = lvlEduCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
            btnDiploma["�ҮѦr��"].Enable = diplomaCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
            btnPlacing["�ƦW"].Enable = placeCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
            btnDiploma["���ͱШ|�{�׸����"].Enable = lvlEduCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;

            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate
            {
                //btnInOut.Enable = lvlEduCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
                btnDiploma["�ҮѦr��"].Enable = diplomaCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
                btnPlacing["�ƦW"].Enable = placeCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
                btnDiploma["���ͱШ|�{�׸����"].Enable = lvlEduCtrl.Executable() && SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0;
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
            get { return "�ǥ�"; }
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
