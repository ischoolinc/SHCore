using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using DevComponents.DotNetBar;
using SmartSchool.StudentRelated;
using SmartSchool.Feature.Class;
using FISCA.DSAUtil;
using System.Xml;
using System.Text.RegularExpressions;
using SmartSchool.ClassRelated.SourceProvider;
using SmartSchool.Properties;
using System.Threading;
using SmartSchool.TeacherRelated;
using SmartSchool.ClassRelated.Search;
using FISCA.Presentation;
using SmartSchool.Adaatper;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.ExtendedColumn;
using SmartSchool.Customization.PlugIn.ExtendedContent;
using SmartSchool.AccessControl;
using SmartSchool.ClassRelated.Palmerworm;
using SmartSchool.ExceptionHandler;

namespace SmartSchool.ClassRelated
{
    public class Class : CacheManager<ClassInfo>, IManager<IColumnItem>, IManager<IContentItem>, Customization.PlugIn.IManager<ButtonAdapter>
    {
        private static Class _Instance = null;
        public static Class Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Class();
                return _Instance;
            }
        }

        /// <summary>
        /// 新增班級到待處理。
        /// </summary>
        /// <param name="primaryKeys">班級編號清單。</param>
        public void AddToTemporal(List<string> primaryKeys)
        {
            K12.Presentation.NLDPanels.Class.AddToTemp(primaryKeys);
        }

        /// <summary>
        /// 將班級移出待處理。
        /// </summary>
        /// <param name="primaryKeys">班級編號清單。</param>
        public void RemoveFromTemporal(List<string> primaryKeys)
        {
            K12.Presentation.NLDPanels.Class.RemoveFromTemp(primaryKeys);
        }

        public void SetupPresentation()
        {
            UseFilter = false;
            this.ItemUpdated += delegate(object sender, ItemUpdatedEventArgs e)
            {
                SetSource();
                K12.Presentation.NLDPanels.Class.RefillListPane();
            };
            this.ItemLoaded += delegate(object sender, EventArgs e)
            {
                K12.Presentation.NLDPanels.Class.ShowLoading = false;
                SetSource();
                K12.Presentation.NLDPanels.Class.RefillListPane();
            };

            K12.Presentation.NLDPanels.Class.CompareSource += delegate(object sender, CompareEventArgs e)
            {
                e.Result = this.QuickCompare(e.Value1, e.Value2);
            };

            ListPaneField nameField = new ListPaneField("名稱");
            nameField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].ClassName;
            };

            ListPaneField studentCountField = new ListPaneField("人數");
            studentCountField.CompareValue += new EventHandler<CompareValueEventArgs>(class_CompareValue);
            studentCountField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Student.Instance.Loaded)
                    e.Value = Student.Instance.GetClassStudent(e.Key).Count;
                else
                    e.Value = "Loading...";
            };
            Student.Instance.ItemLoaded += delegate
            {
                studentCountField.Reload();
            };
            Student.Instance.ItemUpdated += delegate
            {
                studentCountField.Reload();
            };
            K12.Presentation.NLDPanels.Class.AddListPaneField(studentCountField);

            ListPaneField classTeacherField = new ListPaneField("班導師");
            classTeacherField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Teacher.Instance.Loaded)
                {
                    e.Value = Items[e.Key].TeacherUniqName;
                }
                else
                    e.Value = "Loading...";
            };
            Teacher.Instance.ItemUpdated += delegate { classTeacherField.Reload(); };
            Teacher.Instance.ItemLoaded += delegate { classTeacherField.Reload(); };
            K12.Presentation.NLDPanels.Class.AddListPaneField(classTeacherField);

            K12.Presentation.NLDPanels.Class.AddListPaneField(nameField);

            ListPaneField gradeYearField = new ListPaneField("年級");
            gradeYearField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].GradeYear;
            };
            K12.Presentation.NLDPanels.Class.AddListPaneField(gradeYearField);

            ListPaneField deptFiled = new ListPaneField("科別");
            deptFiled.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].Department;
            };
            K12.Presentation.NLDPanels.Class.AddListPaneField(deptFiled);

            ListPaneField classIndexField = new ListPaneField("排列序號");
            classIndexField.CompareValue += new EventHandler<CompareValueEventArgs>(class_CompareValue);
            classIndexField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                {
                    if (Items[e.Key].DisplayOrder != 2147483647)
                    {
                        e.Value = Items[e.Key].DisplayOrder;
                    }
                    else
                    {
                        e.Value = "";
                    }
                }
            };
            K12.Presentation.NLDPanels.Class.AddListPaneField(classIndexField);

            ListPaneField classNamingRuleField = new ListPaneField("班級命名規則");
            classNamingRuleField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].NamingRule;
            };
            K12.Presentation.NLDPanels.Class.AddListPaneField(classNamingRuleField);
            //K12.Presentation.NLDPanels.Class.AddView(new SmartSchool.Adaatper.NavigationPlannerAdatper(new ClassDividerProvider(new GradeYearDivider())));
            K12.Presentation.NLDPanels.Class.AddView(new NavView.GradeYearClassView());


            List<Customization.PlugIn.ExtendedContent.IContentItem> _items = new List<Customization.PlugIn.ExtendedContent.IContentItem>();

            List<Type> _type_list = new List<Type>(new Type[]{
                typeof(ClassBaseInfoItem),
                typeof(ClassStudentItem),
                //typeof(ElectronicPaperPalmerworm) //移除電子報表功能
            });

            foreach (Type type in _type_list)
            {
                if (!Attribute.IsDefined(type, typeof(FeatureCodeAttribute)) || CurrentUser.Acl[type].Viewable)
                {
                    try
                    {
                        IContentItem item = type.GetConstructor(Type.EmptyTypes).Invoke(null) as IContentItem;
                        _items.Add(item);
                    }
                    catch (Exception ex) { BugReporter.ReportException(ex, false); }
                }
            }
            foreach (Customization.PlugIn.ExtendedContent.IContentItem var in _items)
            {
                K12.Presentation.NLDPanels.Class.AddDetailBulider(new ContentItemBulider(var));
            }

            #region 增加班級搜尋條件鈕

            XmlElement preference = SmartSchool.CurrentUser.Instance.Preference["ClassSearchOptionPreference"];
            if (preference == null) preference = new XmlDocument().CreateElement("ClassSearchOptionPreference");

            SearchClassName = K12.Presentation.NLDPanels.Class.SearchConditionMenu["班級名稱"];
            SearchClassName.AutoCheckOnClick = true;
            SearchClassName.AutoCollapseOnClick = false;
            SearchClassName.Checked = preference.GetAttribute("SearchClassName") != "False";

            SearchClassTeacher = K12.Presentation.NLDPanels.Class.SearchConditionMenu["班級導師"];
            SearchClassTeacher.AutoCheckOnClick = true;
            SearchClassTeacher.AutoCollapseOnClick = false;
            SearchClassTeacher.Checked = preference.GetAttribute("SearchClassTeacher") != "False";

            K12.Presentation.NLDPanels.Class.Search += new EventHandler<SearchEventArgs>(Class_Search);
            K12.Presentation.NLDPanels.Class.SearchConditionMenu.PopupClose += delegate
            {
                preference.SetAttribute("SearchClassName", "" + SearchClassName.Checked);
                preference.SetAttribute("SearchClassTeacher", "" + SearchClassTeacher.Checked);
                SmartSchool.CurrentUser.Instance.Preference["ClassSearchOptionPreference"] = preference;
            };

            #endregion

            K12.Presentation.NLDPanels.Class.RequiredDescription += delegate(object sender, RequiredDescriptionEventArgs e)
            {
                e.Result = Items[e.PrimaryKey].ClassName;
            };

            SmartSchool.Customization.PlugIn.ExtendedColumn.ExtendClassColumn.SetManager(this);
            SmartSchool.Customization.PlugIn.ContextMenu.ClassMenuButton.SetManager(this);
            MotherForm.AddPanel(K12.Presentation.NLDPanels.Class);
            _Initilized = true;
            SetSource();

        }

        void class_CompareValue(object sender, CompareValueEventArgs e)
        {
            int x;
            int y;
            if (string.IsNullOrEmpty("" + e.Value1))
                x = 65536;
            else
                int.TryParse("" + e.Value1, out x);

            if (string.IsNullOrEmpty("" + e.Value2))
                y = 65536;
            else
                int.TryParse("" + e.Value2, out y);

            e.Result = x.CompareTo(y);
        }

        public List<ClassInfo> TempClass
        {
            get
            {
                List<ClassInfo> list = new List<ClassInfo>();
                foreach (string id in K12.Presentation.NLDPanels.Class.TempSource)
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
                foreach (ClassInfo var in value)
                {
                    list.Add(var.ClassID);
                    if (!K12.Presentation.NLDPanels.Class.TempSource.Contains(var.ClassID))
                        insertList.Add(var.ClassID);
                }
                foreach (var item in K12.Presentation.NLDPanels.Class.TempSource)
                {
                    if (!list.Contains(item))
                        removeList.Add(item);
                }
                K12.Presentation.NLDPanels.Class.AddToTemp(insertList);
                K12.Presentation.NLDPanels.Class.RemoveFromTemp(removeList);
            }
        }
        internal void ReloadData()
        {
            this.SyncAllBackground();
        }

        private Class()
        {
            this.ItemLoaded += delegate
            {
                lock (_TeacherSupervised)
                {
                    _TeacherSupervised.Clear();
                    foreach (var item in Items)
                    {
                        if (!_TeacherSupervised.ContainsKey(item.TeacherID))
                            _TeacherSupervised.Add(item.TeacherID, new List<ClassInfo>());
                        _TeacherSupervised[item.TeacherID].Add(item);
                    }
                }
            };
            this.ItemUpdated += delegate(object sender, ItemUpdatedEventArgs e)
            {
                lock (_TeacherSupervised)
                {
                    List<string> keys = new List<string>(e.PrimaryKeys);
                    keys.Sort();
                    foreach (var tid in _TeacherSupervised.Keys)
                    {
                        List<ClassInfo> removeItems = new List<ClassInfo>();
                        foreach (var item in _TeacherSupervised[tid])
                        {
                            if (keys.BinarySearch(item.ClassID) >= 0)
                            {
                                removeItems.Add(item);
                            }
                        }
                        foreach (var item in removeItems)
                        {
                            _TeacherSupervised[tid].Remove(item);
                        }
                    }
                    foreach (var key in e.PrimaryKeys)
                    {
                        var item = Items[key];
                        if (item != null)
                        {
                            if (!_TeacherSupervised.ContainsKey(item.TeacherID))
                                _TeacherSupervised.Add(item.TeacherID, new List<ClassInfo>());
                            _TeacherSupervised[item.TeacherID].Add(item);
                        }
                    }
                }
            };
        }



        #region 班級搜尋主功能

        private MenuButton SearchClassName, SearchClassTeacher; //SearchClassTeacher

        void Class_Search(object sender, SearchEventArgs e)
        {
            try
            {
                List<ClassInfo> classList = new List<ClassInfo>(Class.Instance.Items);
                Dictionary<string, ClassInfo> results = new Dictionary<string, ClassInfo>();
                Regex rx = new Regex(e.Condition, RegexOptions.IgnoreCase);

                if (SearchClassName.Checked)
                {
                    foreach (ClassInfo each in classList)
                    {
                        string name = (each.ClassName != null) ? each.ClassName : "";
                        if (rx.Match(name).Success)
                        {
                            if (!results.ContainsKey(each.ClassID))
                                results.Add(each.ClassID, each);
                        }
                    }
                }

                if (SearchClassTeacher.Checked)
                {
                    foreach (ClassInfo each in classList)
                    {
                        if (rx.Match(each.TeacherName).Success)
                        {
                            if (!results.ContainsKey(each.ClassID))
                                results.Add(each.ClassID, each);
                        }
                    }
                }

                e.Result.AddRange(results.Keys);
            }
            catch (Exception) { }
        }

        #endregion


        private bool _Initilized = false;
        private Dictionary<string, List<ClassInfo>> _TeacherSupervised = new Dictionary<string, List<ClassInfo>>();
        public List<ClassInfo> GetTecaherSupervisedClass(BriefTeacherData teacher)
        {
            lock (_TeacherSupervised)
            {
                if (_TeacherSupervised.ContainsKey(teacher.ID))
                {
                    return new List<ClassInfo>(_TeacherSupervised[teacher.ID]);
                }
                else
                    return new List<ClassInfo>();
            }
        }

        internal List<ClassInfo> GetSupervise(string _ID)
        {
            lock (_TeacherSupervised)
            {
                if (_TeacherSupervised.ContainsKey(_ID))
                {
                    return new List<ClassInfo>(_TeacherSupervised[_ID]);
                }
                else
                    return new List<ClassInfo>();
            }
        }
        /// <summary>
        /// 取得或設定，使用Filter機制
        /// </summary>
        private bool _UseFilter = false;
        protected bool UseFilter { get { return _UseFilter; } set { _UseFilter = value; K12.Presentation.NLDPanels.Class.FilterMenu.Visible = value; } }
        private void SetSource()
        {
            //資料載入中或資料未載入或畫面沒有設定完成就什麼都不做
            if (!_Initilized || !Loaded) return;
            if (_UseFilter)
                FillFilter();
            else
                K12.Presentation.NLDPanels.Class.SetFilteredSource(new List<string>(Items.Keys));
        }

        protected virtual void FillFilter()
        {
            //資料載入中或資料未載入或畫面沒有設定完成就什麼都不做
            if (!_Initilized || !Loaded) return;

            List<string> primaryKeys = new List<string>();
            foreach (var item in Items)
            {
                primaryKeys.Add(item.ClassID);
            }
            K12.Presentation.NLDPanels.Class.SetFilteredSource(primaryKeys);
        }

        protected override Dictionary<string, ClassInfo> GetAllData()
        {
            Dictionary<string, ClassInfo> items = new Dictionary<string, ClassInfo>();
            foreach (XmlElement element in SmartSchool.Feature.Class.QueryClass.GetClass().GetContent().GetElements("Class"))
            {
                ClassInfo info = new ClassInfo(element);
                items.Add(info.ClassID, info);
            }
            return items;
        }

        internal void SetupSynchronization()
        {
            //Instance.SelectionChanged+=new EventHandler(Instance_SelectionChanged);
            SmartSchool.Broadcaster.Events.Items["班級/選取變更"].Handler += delegate
            {
                MotherForm.SetStatusBarMessage("已選取" + this.SelectionClasses.Count + "個班級");
            };
            ClassDeleted += delegate { this.SetSource(); K12.Presentation.NLDPanels.Class.RefillListPane(); };
            K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
            {
                SmartSchool.Broadcaster.Events.Items["班級/選取變更"].Invoke();
            };
        }

        public List<ClassInfo> SelectionClasses
        {
            get
            {
                List<ClassInfo> selectedList = new List<ClassInfo>();
                foreach (var id in K12.Presentation.NLDPanels.Class.SelectedSource)
                {
                    selectedList.Add(Items[id]);
                }
                return selectedList;
                //return SelectedList;
            }
        }

        protected override Dictionary<string, ClassInfo> GetData(IEnumerable<string> primaryKeys)
        {
            Dictionary<string, ClassInfo> items = new Dictionary<string, ClassInfo>();
            foreach (XmlElement element in SmartSchool.Feature.Class.QueryClass.GetClass(new List<string>(primaryKeys).ToArray()).GetContent().GetElements("Class"))
            {
                ClassInfo info = new ClassInfo(element);
                items.Add(info.ClassID, info);
            }
            return items;
        }

        [Obsolete("程式碼轉移遺留下來的。")]
        internal bool ValidClassName(string classid, string className)
        {
            //_loadingWait.WaitOne();
            if (string.IsNullOrEmpty(className)) return false;
            foreach (ClassInfo classRec in this.Items)
            {
                if (classRec.ClassID != classid && classRec.ClassName == className)
                    return false;
            }
            return true;
        }

        [Obsolete("程式碼轉移遺留下來的。")]
        internal bool ValidateNamingRule(string namingRule)
        {
            return namingRule.IndexOf('{') < namingRule.IndexOf('}');
        }

        [Obsolete("程式碼轉移遺留下來的。")]
        internal string ParseClassName(string namingRule, int gradeYear)
        {
            gradeYear--;
            if (!ValidateNamingRule(namingRule))
                return namingRule;
            string classlist_firstname = "", classlist_lastname = "";
            if (namingRule.Length == 0) return "{" + (gradeYear + 1) + "}";

            string tmp_convert = namingRule;

            // 找出"{"之前文字 並放入 classlist_firstname , 並除去"{"
            if (tmp_convert.IndexOf('{') > 0)
            {
                classlist_firstname = tmp_convert.Substring(0, tmp_convert.IndexOf('{'));
                tmp_convert = tmp_convert.Substring(tmp_convert.IndexOf('{') + 1, tmp_convert.Length - (tmp_convert.IndexOf('{') + 1));
            }
            else tmp_convert = tmp_convert.TrimStart('{');

            // 找出 } 之後文字 classlist_lastname , 並除去"}"
            if (tmp_convert.IndexOf('}') > 0 && tmp_convert.IndexOf('}') < tmp_convert.Length - 1)
            {
                classlist_lastname = tmp_convert.Substring(tmp_convert.IndexOf('}') + 1, tmp_convert.Length - (tmp_convert.IndexOf('}') + 1));
                tmp_convert = tmp_convert.Substring(0, tmp_convert.IndexOf('}'));
            }
            else tmp_convert = tmp_convert.TrimEnd('}');

            // , 存入 array
            string[] listArray = new string[tmp_convert.Split(',').Length];
            listArray = tmp_convert.Split(',');

            // 檢查是否在清單範圍
            if (gradeYear >= 0 && gradeYear < listArray.Length)
            {
                tmp_convert = classlist_firstname + listArray[gradeYear] + classlist_lastname;
            }
            else
            {
                tmp_convert = classlist_firstname + "{" + (gradeYear + 1) + "}" + classlist_lastname;
            }
            return tmp_convert;
        }public event EventHandler<DeleteClassEventArgs> ClassDeleted;
        public void InvokClassDeleted(DeleteClassEventArgs args)
        {
            SyncData(args.DeleteClassIDArray);
            if (ClassDeleted != null)
                ClassDeleted.Invoke(this, args);
        }

        public event EventHandler<InsertClassEventArgs> ClassInserted;
        public void InvokClassInserted(InsertClassEventArgs args)
        {
            SyncData(args.InsertClassID);
            if (ClassInserted != null)
                ClassInserted.Invoke(this, args);
        }

        public event EventHandler<UpdateClassEventArgs> ClassUpdated;
        public void InvokClassUpdated(params string[] classIDList)
        {
            if (classIDList.Length == 0) return;
            SyncData(classIDList);
            if (ClassUpdated != null)
            {
                ClassUpdated.Invoke(this, new UpdateClassEventArgs(classIDList));
            }
        }

        #region IManager<IColumnItem> 成員

        void IManager<IColumnItem>.Add(IColumnItem instance)
        {
            K12.Presentation.NLDPanels.Class.AddListPaneField(new Adaatper.ColumnItemAdapter(instance));
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
                _Adapter = new Adaatper.ButtonAdapterPlugInToMenuButton(K12.Presentation.NLDPanels.Class.ListPaneContexMenu);
                K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
                  {
                      foreach (var item in _AdapterItem)
                      {
                          K12.Presentation.NLDPanels.Class.ListPaneContexMenu[item].Enable = K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0;
                      }
                  };
            }

            _Adapter.Add(instance);

            List<string> paths = new List<string>();
            if (instance.Path != "")
                paths.AddRange(instance.Path.Split('/'));
            paths.Add(instance.Text);
            _AdapterItem.Add(paths[0]);
            K12.Presentation.NLDPanels.Class.ListPaneContexMenu[paths[0]].Enable = K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0;
        }

        void IManager<ButtonAdapter>.Remove(ButtonAdapter instance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IManager<IContentItem> 成員

        void IManager<IContentItem>.Add(IContentItem instance)
        {

            K12.Presentation.NLDPanels.Class.AddDetailBulider(new ContentItemBulider(instance));
        }

        void IManager<IContentItem>.Remove(IContentItem instance)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class DeleteClassEventArgs : EventArgs
    {
        private List<string> _deleteClassIDArray;

        public DeleteClassEventArgs()
        {
            _deleteClassIDArray = new List<string>();
        }

        public List<string> DeleteClassIDArray
        {
            get { return _deleteClassIDArray; }
            set { _deleteClassIDArray = value; }
        }
    }

    public class InsertClassEventArgs : EventArgs
    {
        private string _insertClassID;
        public string InsertClassID
        {
            get { return _insertClassID; }
            set { _insertClassID = value; }
        }
    }
    public class UpdateClassEventArgs : EventArgs
    {
        List<string> _PrimaryKeys;
        public UpdateClassEventArgs()
        {
            _PrimaryKeys = new List<string>();
        }
        public UpdateClassEventArgs(IEnumerable<string> keys)
        {
            _PrimaryKeys = new List<string>(keys);
        }
        public List<string> Items
        {
            get { return _PrimaryKeys; }
        }
    }

    //public class UpdateClassContent
    //{
    //    private ClassInfo _OldClassInfo;
    //    private ClassInfo _NewClassInfo;
    //    internal UpdateClassContent(ClassInfo oldClassInfo, ClassInfo newClassInfo)
    //    {
    //        _OldClassInfo = oldClassInfo;
    //        _NewClassInfo = newClassInfo;
    //    }
    //    public ClassInfo OldClassInfo
    //    {
    //        get { return _OldClassInfo; }
    //    }
    //    public ClassInfo NewClassInfo
    //    {
    //        get { return _NewClassInfo; }
    //    }
    //}
}