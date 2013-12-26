using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.TeacherRelated.TeacherIUD;
using SmartSchool.Feature.Teacher;
using SmartSchool.Common;
using SmartSchool.Security;
using System.Linq;
using SHSchool.Data;

namespace SmartSchool.TeacherRelated.RibbonBars
{
    public partial class Manage : RibbonBarBase
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
                if (_Instance == null)
                    _Instance = new Manage();
                return _Instance;
            }
        }

        private Manage()
        {
            //InitializeComponent();

            //Teacher.Instance.SelectionChanged += new EventHandler(Instance_SelectionChanged);


            ////權限判斷
            //addCtrl = new FeatureAccessControl("Button0450");
            //saveCtrl = new FeatureAccessControl("Button0460");
            //delCtrl = new FeatureAccessControl("Button0470");

            //addCtrl.Inspect(btnAddTeacher);
            //saveCtrl.Inspect(btnSaveTeacher);
            //delCtrl.Inspect(btnDeleteTeacher);
            
            //讓外部程式呼叫重新整理資料的方法。
            FISCA.Features.TryRegister("教師.CacheManager", args =>
            {
                IEnumerable<string> ids = args["TeacherIDList"] as IEnumerable<string>;
                Teacher.Instance.InvokTeacherDataChanged(ids.ToArray());
            });
        }
        internal void Setup()
        {

            //權限判斷
            addCtrl = new FeatureAccessControl("Button0450");
            delCtrl = new FeatureAccessControl("Button0470");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Manage));
            var Bar = K12.Presentation.NLDPanels.Teacher.RibbonBarItems["編輯"];
            Bar.Index = 0;
            Bar.AutoOverflowEnabled = false;
            var btnAddTeacher = Bar["新增"];
            btnAddTeacher.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnAddTeacher.Image = Properties.Resources.administrator_add_64;
            //btnAddTeacher.Image = ( (System.Drawing.Image)( resources.GetObject("btnAddTeacher_Image") ) );
            btnAddTeacher.Click += new System.EventHandler(buttonItem83_Click);
            btnAddTeacher.Enable = addCtrl.Executable();
            var btnDeleteTeacher = Bar["刪除"];
            btnDeleteTeacher.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            btnDeleteTeacher.Image = Properties.Resources.administrator_remove_64;
            //btnDeleteTeacher.Image = ( (System.Drawing.Image)( resources.GetObject("btnDeleteTeacher_Image") ) );
            btnDeleteTeacher.Click += new System.EventHandler(buttonItem85_Click);
            btnDeleteTeacher.Enable = (K12.Presentation.NLDPanels.Teacher.SelectedSource.Count == 1) && delCtrl.Executable();
            K12.Presentation.NLDPanels.Teacher.SelectedSourceChanged += delegate
            {
                btnDeleteTeacher.Enable = (K12.Presentation.NLDPanels.Teacher.SelectedSource.Count == 1) && delCtrl.Executable();
            };
        }

        private void buttonItem83_Click(object sender, EventArgs e)
        {
            InsertTeacherWizard wizard = new InsertTeacherWizard();
            if (wizard.ShowDialog() == DialogResult.Yes)
            {
                //PopupPalmerwormTeacher.ShowPopupPalmerwormTeacher(wizard.NewTeacherID);
                K12.Presentation.NLDPanels.Teacher.PopupDetailPane(wizard.NewTeacherID);
            }
        }

        private void buttonItem85_Click(object sender, EventArgs e)
        {
            if (MsgBox.Show("是否將 \"" + Teacher.Instance.SelectionTeachers[0].TeacherName + "\" 移至已刪除？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // 找出刪除教師有同帳號、姓名或暱稱
                SHTeacher.RemoveAll();

                // 找出刪除教師
                List<SHTeacherRecord> DelTeacherRecList = SHTeacher.SelectAll().Where(x => x.Status == K12.Data.TeacherRecord.TeacherStatus.刪除).ToList();

                foreach (SHTeacherRecord TRec in DelTeacherRecList.Where(x => (x.TALoginName != "") || (x.Name == Teacher.Instance.SelectionTeachers[0].TeacherName && x.Nickname == Teacher.Instance.SelectionTeachers[0].Nickname)).ToList())
                {
                    TRec.TALoginName = "";
                    TRec.Nickname = TRec.Nickname + TRec.ID;
                }
                if (DelTeacherRecList.Count > 0)
                    SHTeacher.Update(DelTeacherRecList);

                RemoveTeacher.DeleteTeacher(Teacher.Instance.SelectionTeachers[0].ID);

                //Log
                CurrentUser.Instance.AppLog.Write(
                    SmartSchool.ApplicationLog.EntityType.Teacher,
                    "刪除教師",
                    Teacher.Instance.SelectionTeachers[0].ID,
                    "教師「" + Teacher.Instance.SelectionTeachers[0].TeacherName + "」已變更為刪除。",
                    "教師",
                    string.Format("教師姓名：{0}", Teacher.Instance.SelectionTeachers[0].TeacherName));

                Teacher.Instance.InvokTeacherDataChanged(Teacher.Instance.SelectionTeachers[0].ID);
                K12.Data.Teacher.RemoveByIDs(new string[] { Teacher.Instance.SelectionTeachers[0].ID });
                try
                {
                    FISCA.Features.Invoke("教師.Tag.Reload"); //重新整理教師的類別 Bar。
                }
                catch (Exception ex)
                {
                    FISCA.RTOut.WriteError(ex);
                }
            }
        }
    }
}

