using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.StudentRelated;
using DevComponents.DotNetBar;
using SmartSchool.ClassRelated;
using FISCA.DSAUtil;
using SmartSchool.Feature.Class;
//using SmartSchool.SmartPlugIn.Common;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Security;
using FISCA.Presentation;

namespace SmartSchool.ClassRelated.RibbonBars
{
    public partial class Assign : RibbonBarBase
    {
        FeatureAccessControl assignCtrl;
        //FeatureAccessControl planCtrl;
        //FeatureAccessControl calcCtrl;

        private ButtonItemPlugInManager reportManager;

        public Assign()
        {
            //InitializeComponent();
            //btnAssignStudent.Enabled = false;
            //SmartSchool.StudentRelated.Student.Instance.TemporalChanged += new EventHandler(Instance_TemporalChanged);
            ////SmartSchool.ClassRelated.Class.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);
            //SmartSchool.Broadcaster.Events.Items["班級/選取變更"].Handler += delegate
            //{
            //    IsButtonEnable();

            //    assignCtrl.Inspect(btnAssignStudent);
            //};
            //#region 設定為 "班級/指定" 的外掛處理者
            //reportManager = new ButtonItemPlugInManager(itemContainer2);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonItem>.Instance.Add("班級/指定", reportManager);
            //#endregion

            ////權限判斷 - 指定/學生
            //assignCtrl = new FeatureAccessControl("Button0380");
            ////planCtrl = new FeatureAccessControl("Button0390");
            ////calcCtrl = new FeatureAccessControl("Button0400");

            //assignCtrl.Inspect(btnAssignStudent);
            ////planCtrl.Inspect(btnPlan);
            ////calcCtrl.Inspect(btnCalcRule);
        }

        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Assign));
            var assignCtrl = new FeatureAccessControl("Button0380");
            var btnAssignStudent = K12.Presentation.NLDPanels.Class.RibbonBarItems["指定"]["學生"];
            btnAssignStudent.Enable = false;
            btnAssignStudent.Image = ( (System.Drawing.Image)( resources.GetObject("btnAssignStudent.Image") ) );
            btnAssignStudent.SupposeHasChildern = true;
            btnAssignStudent.PopupOpen += delegate(object sender, FISCA.Presentation.PopupOpenEventArgs e)
            {
                List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.TempStudent;
                if ( students.Count > 1 )
                {
                    var btnAll = e.VirtualButtons["加入所有待處理學生"];
                    btnAll.Click += new EventHandler(btnAll_Click);
                }
                bool isFirst = true;
                foreach ( BriefStudentData student in students )
                {
                    var item = e.VirtualButtons["【" + student.ClassName + "】" + student.Name];
                    item.Tag = student;
                    item.Click += new EventHandler(item_Click);
                    if ( isFirst )
                    {
                        item.BeginGroup = true;
                        isFirst = false;
                    }
                }

            };

            SmartSchool.StudentRelated.Student.Instance.TemporalChanged += delegate
            {
                int sCount = SmartSchool.StudentRelated.Student.Instance.TempStudent.Count;
                int cCount = SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count;
                bool enable = true;
                string tooltip = "";

                if ( sCount == 0 )
                    tooltip = "必須先選擇待處理學生";
                else if ( cCount == 0 )
                    tooltip = "必須先選擇一個班級";
                else if ( cCount > 1 )
                    tooltip = "只能選擇一個班級";

                enable = string.IsNullOrEmpty(tooltip);
                tooltip = enable ? "可按此處將待處理學生分配至所選擇班級" : tooltip;
                btnAssignStudent.Enable = enable && assignCtrl.Executable();
            };
            K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
            {
                int sCount = SmartSchool.StudentRelated.Student.Instance.TempStudent.Count;
                int cCount = SmartSchool.ClassRelated.Class.Instance.SelectionClasses.Count;
                bool enable = true;
                string tooltip = "";

                if ( sCount == 0 )
                    tooltip = "必須先選擇待處理學生";
                else if ( cCount == 0 )
                    tooltip = "必須先選擇一個班級";
                else if ( cCount > 1 )
                    tooltip = "只能選擇一個班級";

                enable = string.IsNullOrEmpty(tooltip);
                tooltip = enable ? "可按此處將待處理學生分配至所選擇班級" : tooltip;
                btnAssignStudent.Enable = enable && assignCtrl.Executable();            
            };

        }

        //void Instance_SelectionChanged(object sender, EventArgs e)
        //{
        //    IsButtonEnable();

        //    assignCtrl.Inspect(btnAssignStudent);
        //}


        //private void btnAssignStudent_PopupShowing(object sender, EventArgs e)
        //{
        //    List<BriefStudentData> students = SmartSchool.StudentRelated.Student.Instance.TemporaStudent;
        //    btnAssignStudent.SubItems.Clear();

        //    if ( students.Count > 1 )
        //    {
        //        ButtonItem btnAll = new ButtonItem("btnAll", "加入所有待處理學生");
        //        btnAll.AutoCheckOnClick = false;
        //        btnAll.Tooltip = "將目前待處理中所有學生,指派給所選班級";
        //        btnAll.Click += new EventHandler(btnAll_Click);
        //        btnAssignStudent.InsertItemAt(btnAll, 0, true);
        //    }

        //    foreach ( BriefStudentData student in students )
        //    {
        //        ButtonItem item = new ButtonItem(student.ID, "【" + student.ClassName + "】" + student.Name);
        //        item.AutoCheckOnClick = true;
        //        item.Tooltip = "將單一學生【" + student.Name + "】指派給所選班級";
        //        item.Click += new EventHandler(item_Click);
        //        item.Tag = student;
        //        btnAssignStudent.InsertItemAt(item, 0, false);
        //    }

        //    if ( btnAssignStudent.SubItems.Count > 1 )
        //        btnAssignStudent.SubItems[1].BeginGroup = true;
        //}

        void item_Click(object sender, EventArgs e)
        {
            ClassInfo classInfo = SmartSchool.ClassRelated.Class.Instance.SelectionClasses[0];
            MenuButton item = sender as MenuButton;
            BriefStudentData student = item.Tag as BriefStudentData;
            if ( classInfo.ClassID == student.RefClassID ) return;

            AssignSeatNoPicker assForm = new AssignSeatNoPicker(classInfo.ClassID, student.ID);
            assForm.ShowDialog();
        }

        void btnAll_Click(object sender, EventArgs e)
        {
            ClassInfo classInfo = SmartSchool.ClassRelated.Class.Instance.SelectionClasses[0];
            List<BriefStudentData> list = SmartSchool.StudentRelated.Student.Instance.TempStudent;
            AssignConfirm confirm = new AssignConfirm(classInfo, list);
            confirm.StartPosition = FormStartPosition.CenterParent;
            confirm.ShowDialog();
        }
    }
}
