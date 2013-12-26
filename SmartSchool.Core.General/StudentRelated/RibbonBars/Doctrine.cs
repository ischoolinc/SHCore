using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.StudentRelated.RibbonBars.Discipline;
using SmartSchool.Common;
using DevComponents.DotNetBar;
using SmartSchool.StudentRelated.RibbonBars.AttendanceEditor;
//using SmartSchool.SmartPlugIn.Common;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Security;
using FISCA.Presentation;

namespace SmartSchool.StudentRelated.RibbonBars
{
    public partial class Doctrine : RibbonBarBase
    {
        FeatureAccessControl meritCtrl;
        FeatureAccessControl awardCtrl;
        FeatureAccessControl attendanceCtrl;

        ButtonItemPlugInManager reportManager;

        public Doctrine()
        {
            //InitializeComponent();

            //superTooltip1.DefaultFont = FontStyles.General;

            //SmartSchool.StudentRelated.Student.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);

        }

        internal void Setup()
        {
            //SmartSchool.Broadcaster.Events.Items["學生/選取變更"].Handler += new EventHandler<SmartSchool.Broadcaster.EventArguments>(Award_Handler);
            //#region 設定為 "學生/學務作業" 的外掛處理者
            //reportManager = new ButtonItemPlugInManager(itemContainer1);
            //SmartSchool.Customization.PlugIn.GeneralizationPluhgInManager<ButtonItem>.Instance.Add("學生/學務作業", reportManager);
            //#endregion
            K12.Presentation.NLDPanels.Student.RibbonBarItems["學務作業"].Index = 1;
            K12.Presentation.NLDPanels.Student.RibbonBarItems["學務作業"].AutoOverflowEnabled = false;
            //權限判斷 - 獎勵	Button0060
            meritCtrl = new FeatureAccessControl("Button0060");
            //權限判斷 - 懲戒	Button0070
            awardCtrl = new FeatureAccessControl("Button0070");
            //權限判斷 - 缺曠	Button0080
            attendanceCtrl = new FeatureAccessControl("Button0080");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Doctrine));

            MenuButton btnMerit = K12.Presentation.NLDPanels.Student.RibbonBarItems["學務作業"]["獎勵"];
            btnMerit.Click += new System.EventHandler(this.bItemMerit_Click);
            btnMerit.Image = ( (System.Drawing.Image)( resources.GetObject("btnMerit.Image") ) );
            btnMerit.Enable = meritCtrl.Executable();

            MenuButton btnAward = K12.Presentation.NLDPanels.Student.RibbonBarItems["學務作業"]["懲戒"];
            btnAward.Image = ( (System.Drawing.Image)( resources.GetObject("btnAward.Image") ) );
            btnAward.Enable = awardCtrl.Executable();
            MenuButton btnDemerit = btnAward["懲戒"];
            btnDemerit.Image = ( (System.Drawing.Image)( resources.GetObject("btnAward.Image") ) );
            btnDemerit.Click += new System.EventHandler(this.btnDemerit_Click);
            MenuButton btnClearDemerit = btnAward["銷過"];
            btnClearDemerit.Click += new System.EventHandler(this.btnClearDemerit_Click);

            MenuButton btnAttendance = K12.Presentation.NLDPanels.Student.RibbonBarItems["學務作業"]["缺曠"];
            btnAttendance.Click += new System.EventHandler(this.btnAttendance_Click);
            btnAttendance.Image = ( (System.Drawing.Image)( resources.GetObject("btnAttendance.Image") ) );
            btnAttendance.Enable = attendanceCtrl.Executable();

            #region 加入登錄缺曠的右鍵功能
            if ( attendanceCtrl.Executable() )
            {
                MenuButton contexMenuButton = K12.Presentation.NLDPanels.Student.ListPaneContexMenu["登錄缺曠"];
                contexMenuButton.Image = btnAttendance.Image;
                contexMenuButton.Click += new EventHandler(btnAttendance_Click);
            }
            #endregion
            #region 加入記功過的右鍵功能
            if ( meritCtrl.Executable() || awardCtrl.Executable() )
            {
                MenuButton contexMenuButton = K12.Presentation.NLDPanels.Student.ListPaneContexMenu["登錄獎懲"];
                contexMenuButton.Image = btnMerit.Image;
            }
            if ( meritCtrl.Executable() )
            {
                MenuButton contexMenuButton = K12.Presentation.NLDPanels.Student.ListPaneContexMenu["登錄獎懲"]["獎勵"];
                contexMenuButton.Image = btnMerit.Image;
                contexMenuButton.Click += new EventHandler(bItemMerit_Click);
            }
            if ( awardCtrl.Executable() )
            {
                MenuButton contexMenuButton = K12.Presentation.NLDPanels.Student.ListPaneContexMenu["登錄獎懲"]["懲戒"];
                contexMenuButton.Image = btnDemerit.Image;
                contexMenuButton.Click += new EventHandler(btnDemerit_Click);

                contexMenuButton = K12.Presentation.NLDPanels.Student.ListPaneContexMenu["登錄獎懲"]["銷過"];
                contexMenuButton.Click += new EventHandler(btnClearDemerit_Click);
            }
            #endregion

            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += new EventHandler(Instance_SelectedListChanged);
            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate
            {
                bool selected = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0;
                btnAward.Enable = selected && awardCtrl.Executable();
                btnDemerit.Enable = selected && meritCtrl.Executable();
                btnAttendance.Enable = selected && attendanceCtrl.Executable();
            };

            Instance_SelectedListChanged(null, null);
        }

        void Instance_SelectedListChanged(object sender, EventArgs e)
        {
            bool selected = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0;

            K12.Presentation.NLDPanels.Student.ListPaneContexMenu["登錄缺曠"].Enable =
                K12.Presentation.NLDPanels.Student.ListPaneContexMenu["登錄獎懲"].Enable =
                K12.Presentation.NLDPanels.Student.RibbonBarItems["學務作業"]["缺曠"].Enable =
                K12.Presentation.NLDPanels.Student.RibbonBarItems["學務作業"]["獎勵"].Enable =
                K12.Presentation.NLDPanels.Student.RibbonBarItems["學務作業"]["懲戒"].Enable =
                selected;
            K12.Presentation.NLDPanels.Student.ListPaneContexMenu["登錄獎懲"]["銷過"].Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count == 1;

            
        }

        void Award_Handler(object sender, SmartSchool.Broadcaster.EventArguments e)
        {
            SuperTooltipInfo superTooltipInfo = new SuperTooltipInfo();
            int count = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count;
            superTooltipInfo.BodyText = "按此編輯學生缺曠紀錄";
            btnAttendance.Enabled = true;
            if ( count == 0 )
            {
                superTooltipInfo.BodyText = "請先選擇學生";
                btnAttendance.Enabled = false;
            }
            superTooltip1.SetSuperTooltip(btnAttendance, superTooltipInfo);

            SuperTooltipInfo info = new SuperTooltipInfo();
            SuperTooltipInfo info2 = new SuperTooltipInfo();
            if ( SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0 )
            {
                btnMerit.Enabled = true;
                btnAward.Enabled = true;
                info.BodyText = "登錄學生獎勵資料";
                info2.BodyText = "登錄學生懲戒資料";
            }
            else
            {
                btnMerit.Enabled = false;
                btnAward.Enabled = false;
                info.BodyText = "請先選擇至少一名學生";
                info2.BodyText = "請先選擇至少一名學生";
            }
            superTooltip1.SetSuperTooltip(btnMerit, info);
            superTooltip1.SetSuperTooltip(btnAward, info2);

            SuperTooltipInfo infoClear = new SuperTooltipInfo();
            if ( SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count == 1 )
            {
                btnClearDemerit.Enabled = true;
                infoClear.BodyText = "處理學生銷過作業";
            }
            else
            {
                btnClearDemerit.Enabled = false;
                infoClear.BodyText = "請選擇一名學生進行銷過作業";
            }
            superTooltip1.SetSuperTooltip(btnClearDemerit, infoClear);

            meritCtrl.Inspect(btnMerit);
            awardCtrl.Inspect(btnAward);
            attendanceCtrl.Inspect(btnAttendance);
        }

        public override string ProcessTabName
        {
            get
            {
                return "學生";
            }
        }

        private void bItemMerit_Click(object sender, EventArgs e)
        {
            InsertEditor editor = new InsertEditor(SmartSchool.StudentRelated.Student.Instance.SelectionStudents, true);
            editor.ShowDialog();
        }

        private void btnDemerit_Click(object sender, EventArgs e)
        {
            DemeritEditor editor = new DemeritEditor(SmartSchool.StudentRelated.Student.Instance.SelectionStudents, false);
            editor.ShowDialog();
        }

        private void btnClearDemerit_Click(object sender, EventArgs e)
        {
            if ( SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count > 0 )
            {
                ClearDemerit cd = new ClearDemerit(SmartSchool.StudentRelated.Student.Instance.SelectionStudents[0]);
                cd.ShowDialog();
            }
        }

        private void btnAttendance_Click(object sender, EventArgs e)
        {
            int count = SmartSchool.StudentRelated.Student.Instance.SelectionStudents.Count;
            if ( count == 1 )
            {
                SingleEditor editor = new SingleEditor(SmartSchool.StudentRelated.Student.Instance.SelectionStudents[0]);
                editor.ShowDialog();
            }
            else
            {
                MutiEditor editor = new MutiEditor(SmartSchool.StudentRelated.Student.Instance.SelectionStudents);
                editor.ShowDialog();
            }
        }
    }
}
