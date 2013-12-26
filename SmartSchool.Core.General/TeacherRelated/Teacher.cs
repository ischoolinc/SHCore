using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using DevComponents.DotNetBar;
using System.Drawing;
using FISCA.DSAUtil;
using System.Xml;
using SmartSchool.TeacherRelated.SourceProvider;
using SmartSchool.Properties;
using SmartSchool.TeacherRelated.Search;
using SmartSchool.Feature.Teacher;
using DevComponents.DotNetBar.Rendering;
using SmartSchool.TeacherRelated.Divider;
using SmartSchool.ClassRelated;
using System.Threading;
using SmartSchool.Common;
using System.Text.RegularExpressions;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.ExtendedColumn;
using FISCA.Presentation;
using SmartSchool.Customization.PlugIn.ExtendedContent;
using SmartSchool.TeacherRelated.Palmerworm;
using SmartSchool.ExceptionHandler;
using FISCA.Permission;
using System.Data;

namespace SmartSchool.TeacherRelated
{
    public class Teacher : CacheManager<BriefTeacherData>, IManager<IColumnItem>, IManager<IContentItem>, Customization.PlugIn.IManager<ButtonAdapter>
    {
        private static Teacher _Instance = null;
        public static Teacher Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Teacher();
                return _Instance;
            }
        }

        /// <summary>
        /// 新增教師到待處理。
        /// </summary>
        /// <param name="primaryKeys">教師編號清單。</param>
        public void AddToTemporal(List<string> primaryKeys)
        {
            K12.Presentation.NLDPanels.Teacher.AddToTemp(primaryKeys);
        }

        /// <summary>
        /// 將教師移出待處理。
        /// </summary>
        /// <param name="primaryKeys">教師編號清單。</param>
        public void RemoveFromTemporal(List<string> primaryKeys)
        {
            K12.Presentation.NLDPanels.Teacher.RemoveFromTemp(primaryKeys);
        }

        public void SetupPresentation()
        {

            UseFilter = false;
            this.ItemUpdated += delegate(object sender, ItemUpdatedEventArgs e)
            {
                SetSource();
                K12.Presentation.NLDPanels.Teacher.RefillListPane();
            };
            this.ItemLoaded += delegate(object sender, EventArgs e)
            {
                K12.Presentation.NLDPanels.Teacher.ShowLoading = false;
                SetSource();
                K12.Presentation.NLDPanels.Teacher.RefillListPane();
            };

            K12.Presentation.NLDPanels.Teacher.CompareSource += delegate(object sender, CompareEventArgs e)
            {
                e.Result = this.QuickCompare(e.Value1, e.Value2);
            };

            System.Windows.Forms.DataGridViewImageColumn colStatus = new System.Windows.Forms.DataGridViewImageColumn();

            colStatus.FillWeight = 1F;
            colStatus.HeaderText = "狀態";
            colStatus.MinimumWidth = 70;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            colStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colStatus.ToolTipText = "在校狀態";
            colStatus.Visible = false;
            colStatus.Width = 70;
            ListPaneField statusField = new ListPaneField(colStatus);
            statusField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                Image pic = null;
                string statusD = "";
                #region 判斷在學狀態並對應成圖片
                switch (Items[e.Key].Status)
                {
                    case "一般":
                        pic = global::SmartSchool.Properties.Resources.一般;
                        pic.Tag = 0;
                        statusD = "一般";
                        break;
                    case "刪除":
                        pic = global::SmartSchool.Properties.Resources.刪除;
                        pic.Tag = 4;
                        statusD = "已刪除";
                        break;
                    default:
                        pic = null;
                        statusD = "我也不知道";
                        break;
                }
                #endregion
                e.Value = pic;
                e.Tooltip = statusD;
            };
            K12.Presentation.NLDPanels.Teacher.AddListPaneField(statusField);

            ListPaneField nameField = new ListPaneField("姓名");
            nameField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Items[e.Key].UniqName;
            };
            K12.Presentation.NLDPanels.Teacher.AddListPaneField(nameField);

            ListPaneField genderField = new ListPaneField("性別");
            genderField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Items[e.Key].Gender;
            };
            K12.Presentation.NLDPanels.Teacher.AddListPaneField(genderField);

            ListPaneField idNumberField = new ListPaneField("身分證號");
            idNumberField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Items[e.Key].IDNumber;
            };
            if (UserAcl.Current["Teacher.Field.身分證號"].Executable)
                K12.Presentation.NLDPanels.Teacher.AddListPaneField(idNumberField);

            Catalog ribbonField = RoleAclSource.Instance["教師"]["清單欄位"];
            ribbonField.Add(new RibbonFeature("Teacher.Field.身分證號", "身分證號"));

            ListPaneField superviseClassField = new ListPaneField("帶班班級");
            superviseClassField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Class.Instance.Loaded)
                {
                    var superviseClass = "";
                    foreach (var item in Class.Instance.GetTecaherSupervisedClass(Items[e.Key]))
                    {
                        superviseClass += (superviseClass == "" ? "" : "、") + item.ClassName;
                    }
                    e.Value = superviseClass;
                }
                else
                    e.Value = "Loading...";
            };
            Class.Instance.ItemUpdated += delegate { superviseClassField.Reload(); };
            Class.Instance.ItemLoaded += delegate { superviseClassField.Reload(); };
            K12.Presentation.NLDPanels.Teacher.AddListPaneField(superviseClassField);

            ListPaneField telField = new ListPaneField("聯絡電話");
            telField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Items[e.Key].ContactPhone;
            };
            if (UserAcl.Current["Teacher.Field.聯絡電話"].Executable)
                K12.Presentation.NLDPanels.Teacher.AddListPaneField(telField);

            ribbonField = RoleAclSource.Instance["教師"]["清單欄位"];
            ribbonField.Add(new RibbonFeature("Teacher.Field.聯絡電話", "聯絡電話"));


            SmartSchool.Customization.PlugIn.ExtendedColumn.ExtendTeacherColumn.SetManager(this);
            SmartSchool.Customization.PlugIn.ContextMenu.TeacherMenuButton.SetManager(this);

            K12.Presentation.NLDPanels.Teacher.AddView(new SmartSchool.Adaatper.NavigationPlannerAdatper(new TeacherDividerProvider(new ClassDivider())));
            K12.Presentation.NLDPanels.Teacher.AddView(new SmartSchool.Adaatper.NavigationPlannerAdatper(new TeacherDividerProvider(new CategoryDivider())));
            K12.Data.Teacher.AfterDelete += delegate { this.SyncAllBackground(); };

            List<Customization.PlugIn.ExtendedContent.IContentItem> _items = new List<Customization.PlugIn.ExtendedContent.IContentItem>();

            List<Type> _type_list = new List<Type>(new Type[]{
                        typeof(BaseInfoItem),
                        typeof(TeachStudentItem),
                        //typeof(ElectronicPaperPalmerworm), //移除電子報表功能
                        typeof(TeachCourseItem),
            });

            foreach (Type type in _type_list)
            {
                if (CurrentUser.Acl[type].Viewable)
                {
                    try
                    {
                        System.Windows.Forms.UserControl uc = type.GetConstructor(Type.EmptyTypes).Invoke(null) as System.Windows.Forms.UserControl;

                        IContentItem item = type.GetConstructor(Type.EmptyTypes).Invoke(null) as IContentItem;
                        _items.Add(item);
                    }
                    catch (Exception ex) { BugReporter.ReportException(ex, false); }
                }
            }
            foreach (Customization.PlugIn.ExtendedContent.IContentItem var in _items)
            {
                K12.Presentation.NLDPanels.Teacher.AddDetailBulider(new SmartSchool.Adaatper.ContentItemBulider(var));
            }


            #region 增加導師搜尋條件鈕

            //ConfigData cd = User.Configuration["TeacherSearchOptionPreference"];

            XmlElement preference = SmartSchool.CurrentUser.Instance.Preference["TeacherSearchOptionPreference"];
            if (preference == null) preference = new XmlDocument().CreateElement("TeacherSearchOptionPreference");

            SearchTeacherName = K12.Presentation.NLDPanels.Teacher.SearchConditionMenu["姓名"];
            SearchTeacherName.AutoCheckOnClick = true;
            SearchTeacherName.AutoCollapseOnClick = false;
            SearchTeacherName.Checked = preference.GetAttribute("SearchTeacherName") != "False";

            SearchTeacherRefId = K12.Presentation.NLDPanels.Teacher.SearchConditionMenu["身分證號"];
            SearchTeacherRefId.AutoCheckOnClick = true;
            SearchTeacherRefId.AutoCollapseOnClick = false;
            SearchTeacherRefId.Checked = preference.GetAttribute("SearchTeacherRefId") != "False";

            SearchTeacherLoginID = K12.Presentation.NLDPanels.Teacher.SearchConditionMenu["登入帳號"];
            SearchTeacherLoginID.AutoCheckOnClick = true;
            SearchTeacherLoginID.AutoCollapseOnClick = false;
            SearchTeacherLoginID.Checked = preference.GetAttribute("SearchTeacherLoginID") != "False";

            K12.Presentation.NLDPanels.Teacher.SearchConditionMenu.PopupClose += delegate
            {
                preference.SetAttribute("SearchTeacherName", "" + SearchTeacherName.Checked);
                preference.SetAttribute("SearchTeacherRefId", "" + SearchTeacherRefId.Checked);
                preference.SetAttribute("SearchTeacherLoginID", "" + SearchTeacherLoginID.Checked);
                SmartSchool.CurrentUser.Instance.Preference["TeacherSearchOptionPreference"] = preference;
            };


            K12.Presentation.NLDPanels.Teacher.Search += new EventHandler<SearchEventArgs>(Teacher_Search);




            #endregion
            //K12.Presentation.NLDPanels.Teacher.RequiredDescription += delegate(object sender, RequiredDescriptionEventArgs e)
            //{
            //    e.Result = Items[e.PrimaryKey].TeacherName + "老師";
            //};
            //K12.Presentation.NLDPanels.Teacher.SetDescriptionPaneBulider<SmartSchool.TeacherRelated.Palmerworm.TeacherDescriptionPane>();

            MotherForm.AddPanel(K12.Presentation.NLDPanels.Teacher);
            _Initilized = true;
            SetSource();
        }

        public event EventHandler SelectionChanged;
        internal void SetupSynchronization()
        {
            //同步更新資料
            K12.Presentation.NLDPanels.Teacher.SelectedSourceChanged += delegate
            {
                if (SelectionChanged != null)
                    SelectionChanged.Invoke(this, new EventArgs());
                MotherForm.SetStatusBarMessage("已選取" + this.SelectionTeachers.Count + "名教師");
            };
            TeacherDataChanged += delegate(object sender, TeacherDataChangedEventArgs e)
            {
                SyncDataBackground(e.Items);
            };
            TeacherDeleted += delegate(object sender, TeacherDeletedEventArgs e)
            {
                SyncDataBackground(e.ID);
            };
            TeacherInserted += delegate { SyncAllBackground(); };
            Class.Instance.ItemLoaded += delegate { this.SetSource(); K12.Presentation.NLDPanels.Teacher.RefillListPane(); };
            Class.Instance.ItemUpdated += delegate { this.SetSource(); K12.Presentation.NLDPanels.Teacher.RefillListPane(); };
        }

        public List<BriefTeacherData> SelectionTeachers
        {
            get
            {
                List<BriefTeacherData> selectedList = new List<BriefTeacherData>();
                foreach (var id in K12.Presentation.NLDPanels.Teacher.SelectedSource)
                {
                    selectedList.Add(Items[id]);
                }
                return selectedList;
            }
        }

        public event EventHandler TeacherInserted;
        public void InvokTeacherInserted(EventArgs e)
        {
            if (TeacherInserted != null)
            {
                TeacherInserted.Invoke(this, e);
            }
        }

        public event EventHandler<TeacherDeletedEventArgs> TeacherDeleted;
        public void InvokTeacherDeleted(TeacherDeletedEventArgs e)
        {
            if (TeacherDeleted != null)
                TeacherDeleted.Invoke(this, e);
        }

        public event EventHandler<TeacherDataChangedEventArgs> TeacherDataChanged;
        public void InvokTeacherDataChanged(params string[] teacherIDList)
        {
            if (teacherIDList.Length == 0) return;
            TeacherDataChangedEventArgs e = new TeacherDataChangedEventArgs(teacherIDList);
            Dictionary<string, BriefTeacherData> _ChangedData = new Dictionary<string, BriefTeacherData>();
            //取得SERVER上最新資料
            //XmlElement[] elements = QueryTeacher.GetTeacherListWithSupervisedByClassInfo(teacherIDList).GetContent().GetElements("Teacher");
            XmlElement[] elements = QueryTeacher.GetTeacherDetailTest().GetContent().GetElements("Teacher");
            foreach (XmlElement ele in elements)
            {
                BriefTeacherData newData;
                string id = ele.SelectSingleNode("@ID").InnerText;
                if (_ChangedData.ContainsKey(id))
                {
                    newData = Items[id];
                }
                else
                {
                    newData = new BriefTeacherData(ele);
                    _ChangedData.Add(id, newData);
                }
            }
            if (TeacherDataChanged != null)
                TeacherDataChanged.Invoke(this, e);
        }
        private bool _Initilized = false;
        private Teacher()
        {

        }
        public List<BriefTeacherData> TempTeacher
        {
            get
            {
                List<BriefTeacherData> list = new List<BriefTeacherData>();
                foreach (string id in K12.Presentation.NLDPanels.Teacher.TempSource)
                {
                    list.Add(Items[id]);
                }
                return list;
            }
            set
            {
                List<string> list = new List<string>();
                List<string> insertList = new List<string>();
                List<string> removeList = new List<string>();
                foreach (BriefTeacherData var in value)
                {
                    list.Add(var.ID);
                    if (!K12.Presentation.NLDPanels.Teacher.TempSource.Contains(var.ID))
                        insertList.Add(var.ID);
                }
                foreach (var item in K12.Presentation.NLDPanels.Teacher.TempSource)
                {
                    if (!list.Contains(item))
                        removeList.Add(item);
                }
                K12.Presentation.NLDPanels.Teacher.AddToTemp(insertList);
                K12.Presentation.NLDPanels.Teacher.RemoveFromTemp(removeList);
            }
        }

        #region 教師搜尋主功能

        private MenuButton SearchTeacherName, SearchTeacherRefId, SearchTeacherLoginID;

        void Teacher_Search(object sender, SearchEventArgs e)
        {
            try
            {
                List<BriefTeacherData> TeacherList = new List<BriefTeacherData>(Teacher.Instance.Items);
                Dictionary<string, BriefTeacherData> results = new Dictionary<string, BriefTeacherData>();
                Regex rx = new Regex(e.Condition, RegexOptions.IgnoreCase);

                if (SearchTeacherName.Checked)
                {
                    foreach (BriefTeacherData each in TeacherList)
                    {
                        string nameAndNickname = (each.TeacherName != null) ? each.TeacherName : "";
                        nameAndNickname += (each.Nickname != null) ? each.Nickname : "";
                        if (rx.Match(nameAndNickname).Success)
                        {
                            if (!results.ContainsKey(each.ID))
                                results.Add(each.ID, each);
                        }
                    }
                }

                if (SearchTeacherRefId.Checked)
                {
                    foreach (BriefTeacherData each in TeacherList)
                    {
                        string name = (each.IDNumber != null) ? each.IDNumber : "";
                        if (rx.Match(name).Success)
                        {
                            if (!results.ContainsKey(each.ID))
                                results.Add(each.ID, each);
                        }
                    }
                }

                if (SearchTeacherLoginID.Checked)
                {
                    #region 取得老師ID與LoginName

                    FISCA.Data.QueryHelper _queryHelper = new FISCA.Data.QueryHelper();
                    DataTable dr = _queryHelper.Select("select id,st_login_name from teacher where st_login_name is not null");

                    Dictionary<string, BriefTeacherData> teacherDict = new Dictionary<string, BriefTeacherData>();
                    foreach (BriefTeacherData rec in TeacherList)
                        teacherDict.Add(rec.ID, rec);

                    foreach (DataRow row in dr.Rows)
                    {
                        string id = "" + row[0];
                        string loginName = "" + row[1];

                        if (rx.Match(loginName).Success)
                        {
                            if (teacherDict.ContainsKey(id))
                            {
                                if (!results.ContainsKey(id))
                                    results.Add(id, teacherDict[id]);
                            }
                        }
                    }

                    #endregion
                }


                e.Result.AddRange(results.Keys);
            }
            catch (Exception) { }
        }


        #endregion

        protected override Dictionary<string, BriefTeacherData> GetAllData()
        {
            Dictionary<string, BriefTeacherData> items = new Dictionary<string, BriefTeacherData>();
            DSResponse dsrsp = SmartSchool.Feature.Teacher.QueryTeacher.GetTeacherDetailTest();
            foreach (XmlElement var in dsrsp.GetContent().GetElements("Teacher"))
            {
                BriefTeacherData data;
                string id = var.SelectSingleNode("@ID").InnerText;
                if (items.ContainsKey(id))
                {
                    data = items[id];
                }
                else
                {
                    data = new BriefTeacherData(var);
                    items.Add(data.ID, data);
                }
            }
            return items;
        }

        protected override Dictionary<string, BriefTeacherData> GetData(IEnumerable<string> primaryKeys)
        {
            Dictionary<string, BriefTeacherData> items = new Dictionary<string, BriefTeacherData>();
            DSResponse dsrsp = SmartSchool.Feature.Teacher.QueryTeacher.GetTeacherDetailTest(new List<string>(primaryKeys).ToArray());
            foreach (XmlElement var in dsrsp.GetContent().GetElements("Teacher"))
            {
                BriefTeacherData data;
                string id = var.SelectSingleNode("@ID").InnerText;
                if (items.ContainsKey(id))
                {
                    data = items[id];
                }
                else
                {
                    data = new BriefTeacherData(var);
                    items.Add(data.ID, data);
                }
            }
            return items;
        }

        /// <summary>
        /// 取得或設定，使用Filter機制
        /// </summary>
        private bool _UseFilter = false;
        protected bool UseFilter { get { return _UseFilter; } set { _UseFilter = value; K12.Presentation.NLDPanels.Teacher.FilterMenu.Visible = value; } }
        private void SetSource()
        {
            //資料載入中或資料未載入或畫面沒有設定完成就什麼都不做
            if (!_Initilized || !Loaded) return;
            if (_UseFilter)
                FillFilter();
            else
                K12.Presentation.NLDPanels.Teacher.SetFilteredSource(new List<string>(Items.Keys));
        }

        protected virtual void FillFilter()
        {
            //資料載入中或資料未載入或畫面沒有設定完成就什麼都不做
            if (!_Initilized || !Loaded) return;

            List<string> primaryKeys = new List<string>();
            foreach (var item in Items)
            {
                primaryKeys.Add(item.ID);
            }
            K12.Presentation.NLDPanels.Teacher.SetFilteredSource(primaryKeys);
        }

        #region IManager<IColumnItem> 成員

        void IManager<IColumnItem>.Add(IColumnItem instance)
        {
            K12.Presentation.NLDPanels.Teacher.AddListPaneField(new Adaatper.ColumnItemAdapter(instance));
        }

        void IManager<IColumnItem>.Remove(IColumnItem instance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IManager<ButtonAdapter> 成員
        private Adaatper.ButtonAdapterPlugInToMenuButton _Adapter = null;
        private List<string> _AdapterItem = new List<string>();
        void IManager<ButtonAdapter>.Add(ButtonAdapter instance)
        {
            if (_Adapter == null)
            {
                _Adapter = new Adaatper.ButtonAdapterPlugInToMenuButton(K12.Presentation.NLDPanels.Teacher.ListPaneContexMenu);
                K12.Presentation.NLDPanels.Teacher.SelectedSourceChanged += delegate
                {
                    foreach (var item in _AdapterItem)
                    {
                        K12.Presentation.NLDPanels.Teacher.ListPaneContexMenu[item].Enable = K12.Presentation.NLDPanels.Teacher.SelectedSource.Count > 0;
                    }
                };
            }

            _Adapter.Add(instance);

            List<string> paths = new List<string>();
            if (instance.Path != "")
                paths.AddRange(instance.Path.Split('/'));
            paths.Add(instance.Text);
            _AdapterItem.Add(paths[0]);
            K12.Presentation.NLDPanels.Teacher.ListPaneContexMenu[paths[0]].Enable = K12.Presentation.NLDPanels.Teacher.SelectedSource.Count > 0;
        }

        void IManager<ButtonAdapter>.Remove(ButtonAdapter instance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IManager<IContentItem> 成員

        void IManager<IContentItem>.Add(IContentItem instance)
        {
            K12.Presentation.NLDPanels.Teacher.AddDetailBulider(new SmartSchool.Adaatper.ContentItemBulider(instance));
        }

        void IManager<IContentItem>.Remove(IContentItem instance)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class TeacherDataChangedEventArgs : EventArgs
    {
        private List<string> _Items;
        public TeacherDataChangedEventArgs(IEnumerable<string> items) { _Items = new List<string>(items); }
        public List<string> Items { get { return _Items; } }
    }


    public class TeacherDeletedEventArgs : EventArgs
    {
        string _id;
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}
