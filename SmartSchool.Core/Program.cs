using System;
using System.Xml;
using FISCA.DSAUtil;
using FISCA.Permission;
using FISCA.Presentation;
using SmartSchool.ElectronicPaperImp;
using SmartSchool.ePaper;

namespace SmartSchool
{
    public static class Core_Program
    {
        public static void Init_System()
        {
            Customization.Data.SystemInformation.SetProvider(new API.Provider.SystemProvider());

            #region 2012/11/26日 - DYLAN將國中(學校基本資料)搬至高中使用
            MotherForm.StartMenu["編輯學校資訊"].Image = Properties.Resources.school_fav_64;
            MotherForm.StartMenu["編輯學校資訊"].Click += delegate
            {
                SchoolInfoMangement editor = new SchoolInfoMangement();
                editor.ShowDialog();
            };

            //舊功能
            //MotherForm.StartMenu["編輯學校資訊(舊)"].Click += delegate
            //{
            //    SchoolInfoEditor editor = new SchoolInfoEditor();
            //    editor.ShowDialog();
            //}; 
            #endregion

            MotherForm.StartMenu["編輯學校資訊"].Enable = CurrentUser.Acl[typeof(SchoolInfoEditor)].Executable;

            ////電子報表相關功能註解,另建立電子報表模組 - dylan 20131218
            //MotherForm.StartMenu["電子報表管理"].Image = Properties.Resources.mail_ok_64;
            //MotherForm.StartMenu["電子報表管理"].Click += delegate
            //{
            //    new SmartSchool.ElectronicPaperImp.ElectronicPaperManager().ShowDialog();
            //};

            //MotherForm.StartMenu["問卷管理"].Click += delegate
            //{
            //    new SmartSchool.Survey.SurveyManager().ShowDialog();
            //};

            #region 已註解的最新消息相關功能

            //MotherForm.StartMenu["使用者回饋"]["問題回報與建議"].Click += delegate
            //{
            //    SmartSchool.Feedback.FeedbackForm form = new SmartSchool.Feedback.FeedbackForm();
            //    form.Show();
            //};

            //MotherForm.StartMenu["最新消息"].Image = Properties.Resources.speech_balloon_64;
            //MotherForm.StartMenu["最新消息"].Click += delegate
            //{
            //    SmartSchool.Feedback.NewsForm form = new SmartSchool.Feedback.NewsForm();
            //    form.ShowDialog();
            //};

            //new SmartSchool.Feedback.NewsNotice();

            //MotherForm.StartMenu["使用者回饋"]["功能投票"].Click += delegate
            //{
            //    SmartSchool.Feedback.VoteForm form = new SmartSchool.Feedback.VoteForm();
            //    form.Show();
            //}; 

            #endregion

            MotherForm.StartMenu["安全性"].BeginGroup = true;
            MotherForm.StartMenu["安全性"].Image = Properties.Resources.foreign_key_lock_64;
            MotherForm.StartMenu["安全性"]["變更密碼"].Click += delegate
            {
                SmartSchool.UserInfomation.UserInfoManager uimForm = new SmartSchool.UserInfomation.UserInfoManager();
                uimForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                uimForm.ShowDialog();
            };
            MotherForm.StartMenu["安全性"]["變更密碼"].Enable = CurrentUser.Acl[typeof(SmartSchool.UserInfomation.UserInfoManager)].Executable; ;

            MotherForm.StartMenu["安全性"]["使用者管理"].Click += delegate
            {
                new FISCA.Permission.UI.UserManager().ShowDialog();
            };
            MotherForm.StartMenu["安全性"]["使用者管理"].Enable = CurrentUser.Acl[typeof(SmartSchool.Security.UserManager)].Executable || CurrentUser.Instance.IsSysAdmin;
            
            MotherForm.StartMenu["安全性"]["角色權限管理"].Click += delegate
            {
                new FISCA.Permission.UI.RoleManager().ShowDialog();
            };
            MotherForm.StartMenu["安全性"]["角色權限管理"].Enable = CurrentUser.Acl[typeof(SmartSchool.Security.RoleManager)].Executable || CurrentUser.Instance.IsSysAdmin;

            //2011/8/4日 - dylan註解
            //if (FISCA.Authentication.DSAServices.IsSysAdmin)
            //{
            //    MotherForm.StartMenu["載入模組管理"].Image = Properties.Resources.spiral_lock_64;
            //    MotherForm.StartMenu["載入模組管理"].Click += delegate
            //    {
            //        new FISCA.Deployment.Administration.ModuleManager().ShowDialog();
            //    };
            //}

            MotherForm.StartMenu["重新登入"].Image = Properties.Resources.world_upload_64;
            MotherForm.StartMenu["重新登入"].BeginGroup = true;
            MotherForm.StartMenu["重新登入"].Click += delegate
            {
                if ( System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Shift )
                {
                    SkillSchool skill = new SkillSchool();
                    skill.ShowDialog();
                }

                System.Windows.Forms.Application.Restart();
            };

            var _feature_def = new DSXmlHelper(DSXmlHelper.LoadXml(Properties.Resources.FeatureDefinition));
            foreach ( XmlElement cat in _feature_def.GetElements("FeatureCatalog") )
            {
                bool b = true;
                Catalog ribbon=null;
                foreach ( var path in cat.GetAttribute("Path").Split("//".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ) )
                {
                    if ( b )
                    {
                        b = false;
                        ribbon = RoleAclSource.Instance[path];
                    }
                    else
                        ribbon=ribbon[path];
                }
                foreach ( XmlElement item in cat.SelectNodes("ButtonItem") )
                {
                    ribbon.Add(new RibbonFeature(item.GetAttribute("FeatureCode"), item.GetAttribute("Title")));
                }
                foreach ( XmlElement item in cat.SelectNodes("ContentItem") )
                {
                    ribbon.Add(new DetailItemFeature(item.GetAttribute("FeatureCode"), item.GetAttribute("Title")));
                }
                foreach ( XmlElement item in cat.SelectNodes("ReportItem") )
                {
                    ribbon.Add(new ReportFeature(item.GetAttribute("FeatureCode"), item.GetAttribute("Title")));
                }
                foreach ( XmlElement item in cat.SelectNodes("SystemItem") )
                {
                    ribbon.Add(new RibbonFeature(item.GetAttribute("FeatureCode"), item.GetAttribute("Title")));
                }
            }

            //電子報表相關功能註解,另建立電子報表模組 - dylan 20131218
            //電子報表的提供者。
            //DispatcherProvider.Register("ischool", new DispatcherImp(), true);
        }
    }
}
