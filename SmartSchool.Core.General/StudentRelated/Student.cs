using System;
using System.Collections.Generic;
using System.Text;
using FISCA.Presentation;
using System.Windows.Forms;
using SmartSchool.StudentRelated.NavigationPlanner;
using SmartSchool.Adaatper;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Drawing;
using SmartSchool.TagManage;
using SmartSchool.ClassRelated;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.ExtendedColumn;
using SmartSchool.StudentRelated.Palmerworm;
using SmartSchool.AccessControl;
using SmartSchool.Customization.PlugIn.ExtendedContent;
using SmartSchool.ExceptionHandler;
using System.Xml;
using System.Data;

namespace SmartSchool.StudentRelated
{
    public class Student : CacheManager<BriefStudentData>, IManager<IColumnItem>, IManager<IContentItem>, Customization.PlugIn.IManager<ButtonAdapter>
    {
        protected override Dictionary<string, BriefStudentData> GetAllData()
        {
            Dictionary<string, BriefStudentData> items = new Dictionary<string, BriefStudentData>();
            foreach (var item in SmartSchool.Feature.QueryStudent.GetAbstractList().GetContent().GetElements("Student"))
            {
                BriefStudentData newdata = new BriefStudentData(item);
                items.Add(newdata.ID, newdata);
            }
            return items;
        }
        protected override Dictionary<string, BriefStudentData> GetData(IEnumerable<string> primaryKeys)
        {
            Dictionary<string, BriefStudentData> items = new Dictionary<string, BriefStudentData>();
            foreach (var item in SmartSchool.Feature.QueryStudent.GetAbstractInfo(new List<string>(primaryKeys).ToArray()).GetContent().GetElements("Student"))
            {
                BriefStudentData newdata = new BriefStudentData(item);
                items.Add(newdata.ID, newdata);
            }
            return items;
        }

        private static Student _Instance = null;
        public static Student Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Student();
                return _Instance;
            }
        }

        internal void SetupSynchronization()
        {
            //同步更新資料
            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate { SmartSchool.Broadcaster.Events.Items["學生/選取變更"].Invoke(); };
            SmartSchool.Broadcaster.Events.Items["學生/選取變更"].Handler += delegate
            {
                MotherForm.SetStatusBarMessage("已選取" + this.SelectionStudents.Count + "名學生");
            };
            StudentDeleted += delegate(object sender, StudentDeletedEventArgs e)
            {
                SyncDataBackground(e.ID);
            };
            //BriefDataChanged += new EventHandler<BriefDataChangedEventArgs>(Instance_BriefDataChanged);
            #region 更新快取資料 CacheManger
            SmartSchool.Broadcaster.Events.Items["學生/資料變更"].AddBeforeInvokeHandler(delegate(object sender, SmartSchool.Broadcaster.EventArguments e)
            {
                if (e.Items.Length == 0) return;
                string[] studentIdList = new string[e.Items.Length];
                for (int i = 0; i < e.Items.Length; i++)
                {
                    studentIdList[i] = "" + e.Items[i];
                }
                //取得SERVER上最新資料
                SyncData(studentIdList);
            });
            #endregion
            StudentInserted += delegate { SyncAllBackground(); };

            //K12.Data 資料同步。
            SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Handler += delegate(object sender, Broadcaster.EventArguments e)
            {
                List<string> ids = new List<string>();
                foreach (string id in e.Items)
                    ids.Add(id);
                K12.Data.Student.RemoveByIDs(ids);
            };


            Class.Instance.ItemLoaded += delegate { if (Loaded)SortItems(); this.SetSource(); };
            Class.Instance.ItemUpdated += delegate { if (Loaded)SortItems(); this.SetSource(); };

        }

        private bool _Initilized = false;
        //private bool _UseDefaultContent = true;
        //private List<StudentControls.DetailContent> _DetialContents = new List<K12.StudentControls.DetailContent>();

        private Dictionary<string, List<BriefStudentData>> _ClassStudents = new Dictionary<string, List<BriefStudentData>>();

        private MenuButton btn所有學生 = null;
        private MenuButton btn在校學生 = null;
        private MenuButton btn學籍在校學生 = null;
        private MenuButton btn一般生 = null;
        private MenuButton btn休學生 = null;
        private MenuButton btn延修生 = null;
        private MenuButton btn畢業及離校生 = null;
        private MenuButton btn刪除 = null;
        private bool _FilterButtonChanging = false;

        private Student()
        {
            this.ItemLoaded += delegate
            {
                lock (_ClassStudents)
                {
                    _ClassStudents.Clear();
                    foreach (var item in this.Items)
                    {
                        if (!_ClassStudents.ContainsKey(item.RefClassID))
                            _ClassStudents.Add(item.RefClassID, new List<BriefStudentData>());
                        _ClassStudents[item.RefClassID].Add(item);
                    }
                }
            };
            this.ItemUpdated += delegate(object sender, ItemUpdatedEventArgs e)
            {
                lock (_ClassStudents)
                {
                    List<string> keys = new List<string>(e.PrimaryKeys);
                    keys.Sort();
                    foreach (var cid in _ClassStudents.Keys)
                    {
                        List<BriefStudentData> removeItems = new List<BriefStudentData>();
                        foreach (var item in _ClassStudents[cid])
                        {
                            if (keys.BinarySearch(item.ID) >= 0)
                            {
                                removeItems.Add(item);
                            }
                        }
                        foreach (var item in removeItems)
                        {
                            _ClassStudents[cid].Remove(item);
                        }
                    }
                    foreach (var key in e.PrimaryKeys)
                    {
                        var item = Items[key];
                        if (item != null)
                        {
                            if (!_ClassStudents.ContainsKey(item.RefClassID))
                                _ClassStudents.Add(item.RefClassID, new List<BriefStudentData>());
                            _ClassStudents[item.RefClassID].Add(item);
                        }
                    }
                }
            };
        }

        public List<BriefStudentData> GetClassStudents(SmartSchool.ClassRelated.ClassInfo classRec)
        {
            lock (_ClassStudents)
            {
                if (_ClassStudents.ContainsKey(classRec.ClassID))
                {
                    return new List<BriefStudentData>(_ClassStudents[classRec.ClassID]);
                }
                else
                    return new List<BriefStudentData>();
            }
        }

        /// <summary>
        /// 依據學生編號取得學生資料物件。
        /// </summary>
        /// <param name="primaryKeys">要取得資料的學生編號。</param>
        /// <returns></returns>
        public List<BriefStudentData> GetStudents(params string[] primaryKeys)
        {
            List<BriefStudentData> list = new List<BriefStudentData>();
            foreach (string each in primaryKeys)
            {
                BriefStudentData record = this[each];
                if (record == null)
                    throw new ArgumentException(string.Format("指定的學生編號不存在：「{0}」", each), "primaryKeys");
                else
                    list.Add(record);
            }

            return list;
        }

        /// <summary>
        /// 新增學生到待處理。
        /// </summary>
        /// <param name="primaryKeys">學生編號清單。</param>
        public void AddToTemporal(List<string> primaryKeys)
        {
            K12.Presentation.NLDPanels.Student.AddToTemp(primaryKeys);
        }

        /// <summary>
        /// 將學生移出待處理。
        /// </summary>
        /// <param name="primaryKeys">學生編號清單。</param>
        public void RemoveFromTemporal(List<string> primaryKeys)
        {
            K12.Presentation.NLDPanels.Student.RemoveFromTemp(primaryKeys);
        }

        public void SetupPresentation()
        {
            UseFilter = true;
            this.ItemUpdated += delegate(object sender, ItemUpdatedEventArgs e)
            {
                SetSource();
                //K12.Presentation.NLDPanels.Student.RefillListPane();
            };
            this.ItemLoaded += delegate(object sender, EventArgs e)
            {
                K12.Presentation.NLDPanels.Student.ShowLoading = false;
                SetSource();
                //K12.Presentation.NLDPanels.Student.RefillListPane();
            };

            K12.Presentation.NLDPanels.Student.CompareSource += delegate(object sender, CompareEventArgs e)
            {
                e.Result = this.QuickCompare(e.Value1, e.Value2);
            };
            if (_student_tag_manager == null)
                _student_tag_manager = new StudentTagManager(this);

            #region List Panel Fields
            DataGridViewImageColumn colStatus = new System.Windows.Forms.DataGridViewImageColumn();
            colStatus.FillWeight = 1F;
            colStatus.HeaderText = "狀態";
            colStatus.MinimumWidth = 63;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            colStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colStatus.ToolTipText = "在校狀態";
            colStatus.Visible = false;
            colStatus.Width = 66;
            ListPaneField statusField = new ListPaneField(colStatus);
            statusField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                Image pic = null;
                string statusD = "";
                #region 判斷在學狀態並對應成圖片
                var var = Student.Instance.Items[e.Key];
                if (var.IsExtending)
                {
                    pic = global::SmartSchool.Properties.Resources.延修;
                    pic.Tag = 2;
                    statusD = "延修";
                }
                else if (var.IsDiscontinued)
                {
                    pic = global::SmartSchool.Properties.Resources.輟學;
                    pic.Tag = 3;
                    statusD = "輟學";
                }
                else if (var.IsOnLeave)
                {
                    pic = global::SmartSchool.Properties.Resources.休學;
                    pic.Tag = 2;
                    statusD = "休學";
                }
                else if (var.IsGraduated)
                {
                    pic = global::SmartSchool.Properties.Resources.離校;
                    pic.Tag = 1;
                    statusD = "畢業或離校";
                }
                else if (var.IsDeleted)
                {
                    pic = global::SmartSchool.Properties.Resources.刪除;
                    pic.Tag = 4;
                    statusD = "已刪除";
                }
                else if (var.IsNormal)
                {
                    pic = global::SmartSchool.Properties.Resources.一般;
                    pic.Tag = 0;
                    statusD = "在校";
                }
                else
                {
                    pic = null;
                    statusD = "我也不知道";
                }
                #endregion
                e.Value = pic;
                e.Tooltip = statusD;
            };

            K12.Presentation.NLDPanels.Student.AddListPaneField(statusField);

            ListPaneField classNameField = new ListPaneField("班級");
            classNameField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Student.Instance.Items[e.Key] != null)
                {
                    e.Value = Student.Instance.Items[e.Key].ClassName;
                }
            };
            K12.Presentation.NLDPanels.Student.AddListPaneField(classNameField);

            ListPaneField seatNoField = new ListPaneField("座號");
            seatNoField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Student.Instance.Items[e.Key].SeatNo;
            };

            seatNoField.CompareValue += delegate(object sender, CompareValueEventArgs e)
            {
                int x, y;

                if (!int.TryParse(e.Value1.ToString(), out x))
                    x = int.MaxValue;

                if (!int.TryParse(e.Value2.ToString(), out y))
                    y = int.MaxValue;

                e.Result = x.CompareTo(y);
            };

            K12.Presentation.NLDPanels.Student.AddListPaneField(seatNoField);

            ListPaneField nameField = new ListPaneField("姓名");
            nameField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Items[e.Key].Name;
            };
            K12.Presentation.NLDPanels.Student.AddListPaneField(nameField);

            ListPaneField studnumberField = new ListPaneField("學號");
            studnumberField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Student.Instance.Items[e.Key].StudentNumber;
            };
            K12.Presentation.NLDPanels.Student.AddListPaneField(studnumberField);

            ListPaneField genderField = new ListPaneField("性別");
            genderField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Student.Instance.Items[e.Key].Gender;
            };
            K12.Presentation.NLDPanels.Student.AddListPaneField(genderField);

            ListPaneField birthday = new ListPaneField("身分證號");
            birthday.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Student.Instance.Items[e.Key].IDNumber;
            };
            if (FISCA.Permission.UserAcl.Current["Student.Field.身分證號"].Executable)
                K12.Presentation.NLDPanels.Student.AddListPaneField(birthday);

            FISCA.Permission.Catalog ribbonField = FISCA.Permission.RoleAclSource.Instance["學生"]["清單欄位"];
            ribbonField.Add(new FISCA.Permission.RibbonFeature("Student.Field.身分證號", "身分證號"));

            ListPaneField tel1 = new ListPaneField("戶籍電話");
            tel1.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Student.Instance.Items[e.Key].PermanentPhone;
            };
            if (FISCA.Permission.UserAcl.Current["Student.Field.戶籍電話"].Executable)
                K12.Presentation.NLDPanels.Student.AddListPaneField(tel1);

            ribbonField = FISCA.Permission.RoleAclSource.Instance["學生"]["清單欄位"];
            ribbonField.Add(new FISCA.Permission.RibbonFeature("Student.Field.戶籍電話", "戶籍電話"));

            ListPaneField tel2 = new ListPaneField("聯絡電話");
            tel2.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                e.Value = Student.Instance.Items[e.Key].ContactPhone;
            };
            if (FISCA.Permission.UserAcl.Current["Student.Field.聯絡電話"].Executable)
                K12.Presentation.NLDPanels.Student.AddListPaneField(tel2);

            ribbonField = FISCA.Permission.RoleAclSource.Instance["學生"]["清單欄位"];
            ribbonField.Add(new FISCA.Permission.RibbonFeature("Student.Field.聯絡電話", "聯絡電話"));

            //DataGridViewTagCellColumn colTag = new SmartSchool.DataGridViewTagCellColumn();
            //colTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            //colTag.FillWeight = 65F;
            //colTag.HeaderText = "類別";
            //colTag.MinimumWidth = 42;
            //colTag.Name = "colTag";
            //colTag.ReadOnly = true;
            //colTag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            //ListPaneField tag = new ListPaneField(colTag);
            //tag.GetVariable += delegate(object sender, GetVariableEventArgs e)
            //{
            //    e.Value = Student.Instance.Items[e.Key].Tags;
            //    string fullTag = "";
            //    foreach ( SmartSchool.TagManage.TagInfo t in Student.Instance.Items[e.Key].Tags )
            //    {
            //        fullTag += ( fullTag == "" ? "" : "\n" ) + t.FullName;
            //    }
            //    e.Tooltip = fullTag;
            //};
            //K12.Presentation.NLDPanels.Student.AddListPaneField(tag);
            #endregion

            #region Student Views
            K12.Presentation.NLDPanels.Student.AddView(new SmartSchool.Adaatper.NavigationPlannerAdatper(new StudentDividerProvider(new SmartSchool.StudentRelated.Divider.ClassDivider())));
            //            K12.Presentation.NLDPanels.Student.AddView(new SmartSchool.Adaatper.NavigationPlannerAdatper(new StudentDividerProvider(new SmartSchool.StudentRelated.Divider.CategoryDivider())));
            K12.Presentation.NLDPanels.Student.AddView(new SmartSchool.Adaatper.NavigationPlannerAdatper(new StudentDividerProvider(new SmartSchool.StudentRelated.Divider.AttendanceDivider())));
            K12.Presentation.NLDPanels.Student.AddView(new SmartSchool.Adaatper.NavigationPlannerAdatper(new StudentDividerProvider(new SmartSchool.StudentRelated.Divider.DisciplineDivider())));
            #endregion

            #region Student Detail Items
            //Student.Instance.AddDetailBulider(new ContentItemBulider<SmartSchool.StudentRelated.Palmerworm.TagBar>());
            //foreach ( Customization.PlugIn.ExtendedContent.IContentItem var in PalmerwormFactory.Instence.Load() )
            //{
            //    K12.Presentation.NLDPanels.Student.AddDetailBulider(new ContentItemBulider(var));
            //} 
            List<Customization.PlugIn.ExtendedContent.IContentItem> _items = new List<Customization.PlugIn.ExtendedContent.IContentItem>();

            List<Type> _type_list = new List<Type>(new Type[]{
                typeof(BaseInfoPalmerwormItem),
                typeof(ClassInfoPalmerwormItem),   
                typeof(ParentInfoPalmerwormItem),
                typeof(PhonePalmerwormItem),
                typeof(AddressPalmerwormItem),

                //缺曠獎懲舊功能註解(dylan 10/25)
                //typeof(AbsencePalmerwormItem),
                //typeof(MeritPalmerwormItem),
                //typeof(DemeritPalmerwormItem),

                typeof(TeacherBiasItem),
                typeof(SemesterHistoryPalmerworm),
                typeof(DiplomaInfoPalmerworm),
                typeof(ExtensionValuesPalmerwormItem),
                //typeof(ElectronicPaperPalmerworm), //移除電子報表功能
                typeof(WordCommentPalmerworm),
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
                K12.Presentation.NLDPanels.Student.AddDetailBulider(new ContentItemBulider(var));
            }
            #endregion

            #region Search Conditions
            //ConfigData cd = User.Configuration["StudentSearchOptionPreference"];
            XmlElement preference = SmartSchool.CurrentUser.Instance.Preference["StudentSearchOptionPreference"];
            if (preference == null) preference = new XmlDocument().CreateElement("StudentSearchOptionPreference");

            SearchName = K12.Presentation.NLDPanels.Student.SearchConditionMenu["姓名"];
            SearchName.AutoCheckOnClick = true;
            SearchName.AutoCollapseOnClick = false;
            SearchName.Checked = preference.GetAttribute("SearchName") != "False";

            //SearchClass = K12.Presentation.NLDPanels.Student.SearchConditionMenu["班級"];
            //SearchClass.AutoCheckOnClick = true;
            //SearchClass.AutoCollapseOnClick = false;
            //SearchClass.Checked = preference.GetAttribute("SearchClass") != "False";

            SearchStudentNumber = K12.Presentation.NLDPanels.Student.SearchConditionMenu["學號"];
            SearchStudentNumber.AutoCheckOnClick = true;
            SearchStudentNumber.AutoCollapseOnClick = false;
            SearchStudentNumber.Checked = preference.GetAttribute("SearchStudentNumber") != "False";

            SearchIDNumber = K12.Presentation.NLDPanels.Student.SearchConditionMenu["身分證號"];
            SearchIDNumber.AutoCheckOnClick = true;
            SearchIDNumber.AutoCollapseOnClick = false;
            SearchIDNumber.Checked = preference.GetAttribute("SearchIDNumber") != "False";

            SearchParentName = K12.Presentation.NLDPanels.Student.SearchConditionMenu["父母監護人姓名"];
            SearchParentName.AutoCheckOnClick = true;
            SearchParentName.AutoCollapseOnClick = false;
            SearchParentName.Checked = preference.GetAttribute("SearchParentName") != "False";

            SearchStudentLoginID = K12.Presentation.NLDPanels.Student.SearchConditionMenu["登入帳號"];
            SearchStudentLoginID.AutoCheckOnClick = true;
            SearchStudentLoginID.AutoCollapseOnClick = false;
            SearchStudentLoginID.Checked = preference.GetAttribute("SearchStudentLoginID") != "False";

            SearchEnglishName = K12.Presentation.NLDPanels.Student.SearchConditionMenu["英文姓名"];
            SearchEnglishName.AutoCheckOnClick = true;
            SearchEnglishName.AutoCollapseOnClick = false;
            SearchEnglishName.Checked = preference.GetAttribute("SearchEnglishName") != "False";

            K12.Presentation.NLDPanels.Student.Search += new EventHandler<SearchEventArgs>(Student_Search);
            K12.Presentation.NLDPanels.Student.SearchConditionMenu.PopupClose += delegate
            {
                preference.SetAttribute("SearchName", "" + SearchName.Checked);
                //preference.SetAttribute("SearchClass", "" + SearchClass.Checked);
                preference.SetAttribute("SearchStudentNumber", "" + SearchStudentNumber.Checked);
                preference.SetAttribute("SearchIDNumber", "" + SearchIDNumber.Checked);
                preference.SetAttribute("SearchParentName", "" + SearchParentName.Checked);
                preference.SetAttribute("SearchStudentLoginID", "" + SearchStudentLoginID.Checked);
                preference.SetAttribute("SearchEnglishName", "" + SearchEnglishName.Checked);
                SmartSchool.CurrentUser.Instance.Preference["StudentSearchOptionPreference"] = preference;
            };
            #endregion

            #region Filter
            K12.Presentation.NLDPanels.Student.FilterMenu.PopupClose += delegate { SetSource(); };
            btn所有學生 = K12.Presentation.NLDPanels.Student.FilterMenu["所有學生"];
            btn在校學生 = K12.Presentation.NLDPanels.Student.FilterMenu["在校學生"];
            btn學籍在校學生 = K12.Presentation.NLDPanels.Student.FilterMenu["學籍在校學生"];
            btn一般生 = K12.Presentation.NLDPanels.Student.FilterMenu["一般生"];
            btn休學生 = K12.Presentation.NLDPanels.Student.FilterMenu["休學生"];
            btn延修生 = K12.Presentation.NLDPanels.Student.FilterMenu["延修生"];
            btn畢業及離校生 = K12.Presentation.NLDPanels.Student.FilterMenu["畢業及離校生"];
            btn刪除 = K12.Presentation.NLDPanels.Student.FilterMenu["刪除"];

            btn所有學生.AutoCheckOnClick =
                btn在校學生.AutoCheckOnClick =
                btn學籍在校學生.AutoCheckOnClick =
                btn一般生.AutoCheckOnClick =
                btn休學生.AutoCheckOnClick =
                btn延修生.AutoCheckOnClick =
                btn畢業及離校生.AutoCheckOnClick =
                btn刪除.AutoCheckOnClick = true;

            btn所有學生.AutoCollapseOnClick = btn在校學生.AutoCollapseOnClick = btn學籍在校學生.AutoCollapseOnClick = true;
            btn一般生.AutoCollapseOnClick = btn休學生.AutoCollapseOnClick = btn延修生.AutoCollapseOnClick = btn畢業及離校生.AutoCollapseOnClick = btn刪除.AutoCollapseOnClick = false;

            btn所有學生.CheckedChanged += delegate
            {
                if (!_FilterButtonChanging)
                {
                    _FilterButtonChanging = true;
                    if (btn所有學生.Checked)
                    {
                        btn在校學生.Checked = btn學籍在校學生.Checked = false;
                        btn一般生.Checked = btn休學生.Checked = btn刪除.Checked = btn延修生.Checked = btn畢業及離校生.Checked = true;
                    }
                    _FilterButtonChanging = false;
                }
            };
            btn在校學生.CheckedChanged += delegate
            {
                if (!_FilterButtonChanging)
                {
                    _FilterButtonChanging = true;
                    if (btn在校學生.Checked)
                    {
                        btn所有學生.Checked = btn學籍在校學生.Checked = false;
                        btn一般生.Checked = btn延修生.Checked = true;
                        btn休學生.Checked = btn刪除.Checked = btn畢業及離校生.Checked = false;
                    }
                    _FilterButtonChanging = false;
                }
            };
            btn學籍在校學生.CheckedChanged += delegate
            {
                if (!_FilterButtonChanging)
                {
                    _FilterButtonChanging = true;
                    if (btn學籍在校學生.Checked)
                    {
                        btn所有學生.Checked = btn在校學生.Checked = false;
                        btn一般生.Checked = btn延修生.Checked = btn休學生.Checked = true;
                        btn刪除.Checked = btn畢業及離校生.Checked = false;
                    }
                    _FilterButtonChanging = false;
                }
            };
            btn一般生.CheckedChanged += new EventHandler(checkBack);
            btn延修生.CheckedChanged += new EventHandler(checkBack);
            btn休學生.CheckedChanged += new EventHandler(checkBack);
            btn刪除.CheckedChanged += new EventHandler(checkBack);
            btn畢業及離校生.CheckedChanged += new EventHandler(checkBack);
            btn一般生.BeginGroup = true;
            btn在校學生.Checked = true;
            #endregion

            K12.Presentation.NLDPanels.Student.NavPaneContexMenu.GetChild("重新整理").Click += delegate { this.SyncAllBackground(); };
            K12.Data.Student.AfterDelete += delegate { this.SyncAllBackground(); };

            SmartSchool.Customization.PlugIn.ExtendedColumn.ExtendStudentColumn.SetManager(this);
            SmartSchool.Customization.PlugIn.ContextMenu.StudentMenuButton.SetManager(this);

            //K12.Presentation.NLDPanels.Student.SetDescriptionPaneBulider<SmartSchool.StudentRelated.Palmerworm.StudentDescriptionPane>();
            //K12.Presentation.NLDPanels.Student.RequiredDescription += delegate(object sender, RequiredDescriptionEventArgs e)
            //{
            //    e.Result = ( Items[e.PrimaryKey].ClassName == "" ? "未分班級" : ( Items[e.PrimaryKey].ClassName + ( ( Items[e.PrimaryKey].SeatNo == "" ) ? "" : "(" + Items[e.PrimaryKey].SeatNo + "號)" ) ) ) + " " + Items[e.PrimaryKey].Name;
            //};

            MotherForm.AddPanel(K12.Presentation.NLDPanels.Student);
            _Initilized = true;
            SetSource();
        }

        void checkBack(object sender, EventArgs e)
        {
            if (!_FilterButtonChanging)
            {
                _FilterButtonChanging = true;
                bool b1 = false, b2 = false, b3 = false;
                b1 = (btn一般生.Checked == true && btn休學生.Checked == true && btn延修生.Checked == true && btn畢業及離校生.Checked == true && btn刪除.Checked == true);
                b2 = ((btn一般生.Checked == true && btn休學生.Checked == true && btn延修生.Checked == true) && (btn畢業及離校生.Checked == false && btn刪除.Checked == false));
                b3 = ((btn一般生.Checked == true && btn延修生.Checked == true) && (btn休學生.Checked == false && btn畢業及離校生.Checked == false && btn刪除.Checked == false));
                if (b1)
                {
                    btn所有學生.Checked = true;
                    btn學籍在校學生.Checked = btn在校學生.Checked = false;
                }
                else if (b2)
                {
                    btn學籍在校學生.Checked = true;
                    btn所有學生.Checked = btn在校學生.Checked = false;
                }
                else if (b3)
                {
                    btn在校學生.Checked = true;
                    btn所有學生.Checked = btn學籍在校學生.Checked = false;
                }
                else
                    btn在校學生.Checked = btn所有學生.Checked = btn學籍在校學生.Checked = false;
                _FilterButtonChanging = false;
            }
        }

        //private MenuButton SearchClass, SearchStudentNumber, SearchName,SearchIDNumber;
        private MenuButton SearchStudentNumber, SearchName, SearchIDNumber, SearchParentName, SearchStudentLoginID, SearchEnglishName;

        private void Student_Search(object sender, SearchEventArgs e)
        {
            try
            {
                FISCA.Data.QueryHelper _queryHelper = new FISCA.Data.QueryHelper();
                DataTable dr_1 = _queryHelper.Select("select id,name,student_number,id_number,sa_login_name,father_name,mother_name,custodian_name,english_name from student");
                Dictionary<string, SearchStudentRecord> studDict_1 = new Dictionary<string, SearchStudentRecord>();
                List<string> results = new List<string>();
                foreach (DataRow row_1 in dr_1.Rows)
                {
                    string id = "" + row_1[0];
                    if (!studDict_1.ContainsKey(id))
                    {
                        studDict_1.Add(id, new SearchStudentRecord(row_1));
                    }
                }

                Regex rx = new Regex(e.Condition, RegexOptions.IgnoreCase);

                // 搜尋父母監護人姓名
                if (SearchParentName.Checked)
                {
                    foreach (SearchStudentRecord each in studDict_1.Values)
                    {
                        if (rx.Match(each.Father_Name).Success)
                        {
                            if (!results.Contains(each.ID))
                                results.Add(each.ID);
                        }

                        if (rx.Match(each.Mother_Name).Success)
                        {
                            if (!results.Contains(each.ID))
                                results.Add(each.ID);
                        }

                        if (rx.Match(each.Custodian_Name).Success)
                        {
                            if (!results.Contains(each.ID))
                                results.Add(each.ID);
                        }
                    }
                }

                if (SearchStudentNumber.Checked)
                {
                    foreach (SearchStudentRecord each in studDict_1.Values)
                    {
                        if (rx.Match(each.StudentNumber).Success)
                        {
                            if (!results.Contains(each.ID))
                                results.Add(each.ID);
                        }
                    }
                }

                if (SearchIDNumber.Checked)
                {
                    foreach (SearchStudentRecord each in studDict_1.Values)
                    {
                        if (rx.Match(each.IDNumber).Success)
                        {
                            if (!results.Contains(each.ID))
                                results.Add(each.ID);
                        }
                    }
                }

                if (SearchName.Checked)
                {
                    foreach (SearchStudentRecord each in studDict_1.Values)
                    {
                        if (rx.Match(each.Name).Success)
                        {
                            if (!results.Contains(each.ID))
                                results.Add(each.ID);
                        }
                    }
                }

                if (SearchStudentLoginID.Checked)
                {
                    foreach (SearchStudentRecord each in studDict_1.Values)
                    {
                        if (rx.Match(each.SA_Login_Name).Success)
                        {
                            if (!results.Contains(each.ID))
                                results.Add(each.ID);
                        }
                    }
                }

                if (SearchEnglishName.Checked)
                {
                    foreach (SearchStudentRecord each in studDict_1.Values)
                    {
                        if (rx.Match(each.English_Name).Success)
                        {
                            if (!results.Contains(each.ID))
                                results.Add(each.ID);
                        }
                    }
                }

                FISCA.Presentation.MotherForm.SetStatusBarMessage("共搜尋到：" + results.Count + "名學生");

                e.Result.AddRange(results);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 取得或設定，使用Filter機制
        /// </summary>
        private bool _UseFilter = false;
        protected bool UseFilter { get { return _UseFilter; } set { _UseFilter = value; K12.Presentation.NLDPanels.Student.FilterMenu.Visible = value; } }
        private void SetSource()
        {
            //資料載入中或資料未載入或畫面沒有設定完成就什麼都不做
            if (!_Initilized || !Loaded || !Class.Instance.Loaded) return;
            if (_UseFilter)
                FillFilter();
            else
                K12.Presentation.NLDPanels.Student.SetFilteredSource(new List<string>(Items.Keys));
        }
        //public bool UseDefaultDetailContent
        //{
        //    get { return _UseDefaultContent; }
        //    set
        //    {
        //        _UseDefaultContent = value;
        //        foreach ( var item in _DetialContents )
        //        {
        //            item.Visible = value;
        //        }
        //    }
        //}
        protected virtual void FillFilter()
        {
            //畫面沒有設定完成就什麼都不做
            if (!_Initilized || !Loaded) return;
            List<string> list = new List<string>();
            foreach (var item in Items)
            {
                if (btn一般生.Checked && item.Status == "一般" ||
                    btn休學生.Checked && item.Status == "休學" ||
                    btn延修生.Checked && item.Status == "延修" ||
                    btn畢業及離校生.Checked && item.Status == "畢業或離校" ||
                    btn刪除.Checked && item.Status == "刪除"
                    )
                {
                    list.Add(item.ID);
                }
            }
            string s = "";
            if (btn一般生.Checked) s += (s == "" ? "" : "、") + "一般生";
            if (btn休學生.Checked) s += (s == "" ? "" : "、") + "休學生";
            if (btn延修生.Checked) s += (s == "" ? "" : "、") + "延修生";
            if (btn畢業及離校生.Checked) s += (s == "" ? "" : "、") + "畢業及離校生";
            if (btn刪除.Checked) s += (s == "" ? "" : "、") + "刪除";
            if (s == "") s = "無";
            K12.Presentation.NLDPanels.Student.FilterMenu.Text = "篩選-<b>" + (
                btn所有學生.Checked ? "所有學生" :
                btn在校學生.Checked ? "在校學生" :
                btn學籍在校學生.Checked ? "學籍在校學生" :
                "自訂") + "</b>";


            //List<string> primaryKeys = new List<string>();
            //foreach ( var item in Items )
            //{
            //    primaryKeys.Add(item.ID);
            //}
            K12.Presentation.NLDPanels.Student.SetFilteredSource(list);
        }

        internal void ReloadData()
        {
            this.SyncAllBackground();
        }

        private TagManager _tag_manager;
        public TagManager TagManager
        {
            get
            {
                if (_tag_manager == null)
                    _tag_manager = new TagManager(SmartSchool.Feature.Tag.TagCategory.Student);

                return _tag_manager;
            }
        }
        private StudentTagManager _student_tag_manager;
        public StudentTagManager SelectionTagManager
        {
            get
            {
                if (_student_tag_manager == null)
                    _student_tag_manager = new StudentTagManager(this);

                return _student_tag_manager;
            }
        }
        public event EventHandler TemporalChanged;
        private void TempStudentSourceProvider_SourceChanged(object sender, EventArgs e)
        {
            if (TemporalChanged != null)
                TemporalChanged(this, EventArgs.Empty);
        }

        public event EventHandler StudentInserted;
        public void InvokStudentInserted(EventArgs e)
        {
            if (StudentInserted != null)
                StudentInserted.Invoke(this, e);
        }

        public event EventHandler<StudentDeletedEventArgs> StudentDeleted;
        public void InvokStudentDeleted(StudentDeletedEventArgs e)
        {
            if (StudentDeleted != null)
                StudentDeleted.Invoke(this, e);
        }

        public event EventHandler NewUpdateRecord;
        public void InvokNewUpdateRecord(object sender, EventArgs e)
        {
            if (NewUpdateRecord != null)
                NewUpdateRecord.Invoke(sender, e);
        }

        public event EventHandler<StudentAttendanceChangedEventArgs> AttendanceChanged;
        public void InvokAttendanceChanged(params string[] studentIDList)
        {
            if (studentIDList.Length == 0) return;
            if (AttendanceChanged != null)
            {
                BriefStudentData[] students = new BriefStudentData[studentIDList.Length];
                for (int i = 0; i < studentIDList.Length; i++)
                {
                    students[i] = Items[studentIDList[i]];
                }
                StudentAttendanceChangedEventArgs args = new StudentAttendanceChangedEventArgs(students);
                AttendanceChanged.Invoke(this, args);
            }
        }

        public event EventHandler<StudentDisciplineChangedEventArgs> DisciplineChanged;
        public void InvokDisciplineChanged(params string[] studentIDList)
        {
            if (studentIDList.Length == 0) return;
            if (DisciplineChanged != null)
            {
                BriefStudentData[] students = new BriefStudentData[studentIDList.Length];
                for (int i = 0; i < studentIDList.Length; i++)
                {
                    students[i] = Items[studentIDList[i]];
                }
                StudentDisciplineChangedEventArgs args = new StudentDisciplineChangedEventArgs(students);
                DisciplineChanged.Invoke(this, args);
            }
        }

        public void InvokTagDefinitionChanged(params int[] tagIDs)
        {
            if (tagIDs.Length == 0) return;
            List<string> studentIDList = new List<string>();
            foreach (BriefStudentData student in Items)
            {
                foreach (int id in tagIDs)
                {
                    if (student.Tags.ContainsKey(id))
                    {
                        studentIDList.Add(student.ID);
                        break;
                    }
                }
            }
            //InvokBriefDataChanged(studentIDList.ToArray());
            SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(studentIDList.ToArray());
        }
        internal void EnsureStudent(IEnumerable<string> idlist)
        {
            List<string> loadList = new List<string>();
            foreach (string id in idlist)
            {
                if (!Items.ContainsKey(id) && !loadList.Contains(id))
                    loadList.Add(id);
            }
            if (loadList.Count > 0)
            {
                SyncData(loadList);
            }
        }
        internal List<BriefStudentData> GetClassStudent(string classID)
        {
            List<BriefStudentData> classList = new List<BriefStudentData>();
            if (_ClassStudents.ContainsKey(classID))
            {
                foreach (BriefStudentData student in _ClassStudents[classID])
                {
                    if (student.IsNormal)
                    {
                        classList.Add(student);
                    }
                }
            }
            classList.Sort();
            return classList;
        }
        public List<BriefStudentData> SelectionStudents
        {
            get
            {
                List<BriefStudentData> selectedList = new List<BriefStudentData>();
                foreach (var id in K12.Presentation.NLDPanels.Student.SelectedSource)
                {
                    selectedList.Add(Items[id]);
                }
                return selectedList;
            }
        }
        public List<BriefStudentData> TempStudent
        {
            get
            {
                List<BriefStudentData> list = new List<BriefStudentData>();
                foreach (string id in K12.Presentation.NLDPanels.Student.TempSource)
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
                foreach (BriefStudentData var in value)
                {
                    list.Add(var.ID);
                    if (!K12.Presentation.NLDPanels.Student.TempSource.Contains(var.ID))
                        insertList.Add(var.ID);
                }
                foreach (var item in K12.Presentation.NLDPanels.Student.TempSource)
                {
                    if (!list.Contains(item))
                        removeList.Add(item);
                }
                K12.Presentation.NLDPanels.Student.AddToTemp(insertList);
                K12.Presentation.NLDPanels.Student.RemoveFromTemp(removeList);
            }
        }

        public void ShowDetail(string id)
        {
            K12.Presentation.NLDPanels.Student.PopupDetailPane(id);
        }

        internal void ViewTemporaStudent()
        {
            K12.Presentation.NLDPanels.Student.DisplayStatus = DisplayStatus.Temp;
            if (K12.Presentation.NLDPanels.Student.DisplayStatus == DisplayStatus.Temp)
                K12.Presentation.NLDPanels.Student.SelectAll();
        }

        #region IManager<IColumnItem> 成員

        void IManager<IColumnItem>.Add(IColumnItem instance)
        {
            K12.Presentation.NLDPanels.Student.AddListPaneField(new Adaatper.ColumnItemAdapter(instance));
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
                _Adapter = new Adaatper.ButtonAdapterPlugInToMenuButton(K12.Presentation.NLDPanels.Student.ListPaneContexMenu);
                K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate
                {
                    foreach (var item in _AdapterItem)
                    {
                        K12.Presentation.NLDPanels.Student.ListPaneContexMenu[item].Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0;
                    }
                };
            }

            _Adapter.Add(instance);

            List<string> paths = new List<string>();
            if (instance.Path != "")
                paths.AddRange(instance.Path.Split('/'));
            paths.Add(instance.Text);
            _AdapterItem.Add(paths[0]);
            K12.Presentation.NLDPanels.Student.ListPaneContexMenu[paths[0]].Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0;
        }

        void IManager<ButtonAdapter>.Remove(ButtonAdapter instance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IManager<IContentItem> 成員

        void IManager<IContentItem>.Add(IContentItem instance)
        {
            K12.Presentation.NLDPanels.Student.AddDetailBulider(new ContentItemBulider(instance));
        }

        void IManager<IContentItem>.Remove(IContentItem instance)
        {
        }

        #endregion
    }
}
