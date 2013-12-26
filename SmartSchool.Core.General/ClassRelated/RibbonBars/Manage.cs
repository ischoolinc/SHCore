using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.Feature.Class;
using SmartSchool.Security;
using SmartSchool.ApplicationLog;
using FISCA.Presentation;

namespace SmartSchool.ClassRelated.RibbonBars
{
    public partial class Manage : RibbonBarBase, IPalmerwormManager
    {
        //權限判斷
        FeatureAccessControl addCtrl;
        FeatureAccessControl saveCtrl;
        FeatureAccessControl delCtrl;

        static private Manage _Instance;
        static public Manage Instance
        {
            get
            {
                if ( _Instance == null )
                    _Instance = new Manage();
                return _Instance;
            }
        }

        private Manage()
        {
            //InitializeComponent();

        }

        internal void Setup()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Manage));

            //權限判斷
            addCtrl = new FeatureAccessControl("Button0330");
            delCtrl = new FeatureAccessControl("Button0350");

            RibbonBarItem rbItem = K12.Presentation.NLDPanels.Class.RibbonBarItems["編輯"];
            rbItem.AutoOverflowEnabled = false;
            var btnAddClass = rbItem["新增"];
            var btnDeleteClass = rbItem["刪除"];
            btnAddClass.Size = RibbonBarButton.MenuButtonSize.Large;
            btnAddClass.Enable = addCtrl.Executable();
            btnAddClass.Image = (System.Drawing.Image)Properties.Resources.btnaddclass_image;
            //btnAddClass.Image = ( (System.Drawing.Image)( resources.GetObject("btnDeleteClass_Image") ) );
            btnAddClass.Click += new System.EventHandler(this.buttonItem14_Click);

            btnDeleteClass.Size = RibbonBarButton.MenuButtonSize.Large;
            btnDeleteClass.Image = (System.Drawing.Image)Properties.Resources.btndeleteclass_image;
            //btnDeleteClass.Image = ( (System.Drawing.Image)( resources.GetObject("btnDeleteClass_Image") ) );
            btnDeleteClass.Enable = K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0 && delCtrl.Executable();
            btnDeleteClass.Click += new System.EventHandler(this.btnDelete_Click);

            K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate { btnDeleteClass.Enable = K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0 && delCtrl.Executable(); };
        }

        //void Instance_SelectionChanged(object sender, EventArgs e)
        //{
        //    btnDeleteClass.Enabled = (Class.Instance.SelectionClasses.Count == 1);

        //    delCtrl.Inspect(btnDeleteClass);
        //}

        private void buttonItem14_Click(object sender, EventArgs e)
        {
            InsertClassWizard wizard = new InsertClassWizard();
            if ( wizard.ShowDialog() == DialogResult.Yes )
                K12.Presentation.NLDPanels.Class.PopupDetailPane(wizard.NewClassID);
        }



        #region IPalmerwormManager 成員

        public bool EnableSave
        {
            get
            {
                return btnSaveClass.Enabled;
            }
            set
            {
                btnSaveClass.Enabled = value;

                saveCtrl.Inspect(btnSaveClass);
            }
        }

        public bool EnableCancel
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public event EventHandler Save;

        public event EventHandler Cacel;

        public event EventHandler Reflash;

        #endregion

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if ( MsgBox.Show("注意！此動作將會使該班學生列入未分班學生之中，您確定將刪除此班級？", "", MessageBoxButtons.YesNo) == DialogResult.Yes )
            {
                try
                {
                    ClassInfo info = Class.Instance.SelectionClasses[0];

                    DeleteClassEventArgs args = new DeleteClassEventArgs();
                    //args.DeleteClassIDArray.Add(Class.Instance.SelectionClasses[0].ClassID);
                    args.DeleteClassIDArray.Add(info.ClassID);

                    //RemoveClass.DeleteClass(Class.Instance.SelectionClasses[0].ClassID);
                    RemoveClass.DeleteClass(info.ClassID);
                    // 加這行主要防止當刪除班級在學生會有重新排序造成系統當機
                    SmartSchool.StudentRelated.Student.Instance.SyncAllBackground();
                    Class.Instance.InvokClassDeleted(args);

                    //寫入 Log
                    CurrentUser.Instance.AppLog.Write(EntityType.Class, "刪除班級", info.ClassID, string.Format("班級「{0}」已刪除", info.ClassName), "班級", info.ClassID);
                }
                catch ( Exception ex )
                {
                    CurrentUser.ReportError(ex);
                    MessageBox.Show("刪除班級失敗，錯誤訊息：" + ex.Message);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if ( Save != null )
                Save.Invoke(this, new EventArgs());
        }
    }
}
