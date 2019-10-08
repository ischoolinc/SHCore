using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using FISCA.DSAUtil;
using FISCA.Presentation;
using SmartSchool.AccessControl;
using SmartSchool.Customization.PlugIn;
using SmartSchool.Customization.PlugIn.ExtendedColumn;
using SmartSchool.Customization.PlugIn.ExtendedContent;
using SmartSchool.ExceptionHandler;
using SmartSchool.Feature.Course;

namespace SmartSchool.CourseRelated
{

    public class Course : CacheManager<CourseInformation>, IManager<IColumnItem>, IManager<IContentItem>, Customization.PlugIn.IManager<ButtonAdapter>
    {
        private static Course _Instance = null;
        public static Course Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Course();
                return _Instance;
            }
        }

        /// <summary>
        /// 新增課程到待處理。
        /// </summary>
        /// <param name="primaryKeys">課程編號清單。</param>
        public void AddToTemporal(List<string> primaryKeys)
        {
            K12.Presentation.NLDPanels.Course.AddToTemp(primaryKeys);
        }

        /// <summary>
        /// 將課程移出待處理。
        /// </summary>
        /// <param name="primaryKeys">課程編號清單。</param>
        public void RemoveFromTemporal(List<string> primaryKeys)
        {
            K12.Presentation.NLDPanels.Course.RemoveFromTemp(primaryKeys);
        }

        private SmartSchool.SemesterInfo FiltedSemester = new SmartSchool.SemesterInfo() { SchoolYear = CurrentUser.Instance.SchoolYear, Semester = CurrentUser.Instance.Semester };

        public void SetupPresentation()
        {
            UseFilter = true;
            this.ItemUpdated += delegate(object sender, ItemUpdatedEventArgs e)
            {
                SetSource();
                K12.Presentation.NLDPanels.Course.RefillListPane();
            };
            this.ItemLoaded += delegate(object sender, EventArgs e)
            {
                K12.Presentation.NLDPanels.Course.ShowLoading = false;
                SetSource();
                K12.Presentation.NLDPanels.Course.RefillListPane();
            };

            K12.Presentation.NLDPanels.Course.CompareSource += delegate(object sender, CompareEventArgs e)
            {
                e.Result = this.QuickCompare(e.Value1, e.Value2);
            };

            FISCA.Features.Register("CourseSyncAllBackground", x =>
            {
                this.SyncAllBackground();
            });

            //處理產品社團,當使用轉入課程功能時
            //需要引發課程的更新事件
            //2019/9/10 - Dylan
            FISCA.InteractionService.SubscribeEvent("課程/重新整理", (sender, args) =>
            {
                this.SyncAllBackground();
            });

            ListPaneField nameField = new ListPaneField("名稱");
            nameField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].CourseName;
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(nameField);

            ListPaneField schoolYearField = new ListPaneField("學年度");
            schoolYearField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].SchoolYear;
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(schoolYearField);

            ListPaneField semesterField = new ListPaneField("學期");
            semesterField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].Semester;
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(semesterField);

            ListPaneField creditField = new ListPaneField("學分數");
            creditField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].CreditDec;
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(creditField);

            ListPaneField subjectField = new ListPaneField("科目");
            subjectField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].Subject;
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(subjectField);

            ListPaneField levelField = new ListPaneField("級別");
            levelField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].SubjectLevel;
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(levelField);

            ListPaneField classField = new ListPaneField("所屬班級");
            classField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    if (Items[e.Key].ClassName != null)
                        e.Value = Items[e.Key].ClassName;
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(classField);

            ListPaneField teacherField = new ListPaneField("授課教師");
            teacherField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                {
                    string teacher = "";
                    foreach (var item in Items[e.Key].Teachers)
                    {
                        teacher += (teacher == "" ? "" : "、") + item.TeacherName;
                    }
                    e.Value = teacher;
                }
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(teacherField);

            // 評分樣板
            ListPaneField ExamTemplateField = new ListPaneField("評分樣版");
            ExamTemplateField.GetVariable += delegate(object sender, GetVariableEventArgs e)
            {
                if (Items[e.Key] != null)
                    e.Value = Items[e.Key].ExamTemplateName;
            };
            K12.Presentation.NLDPanels.Course.AddListPaneField(ExamTemplateField);

            K12.Presentation.NLDPanels.Course.AddView(new SmartSchool.CourseRelated.NavViews.GradeYear_Class_View());
            K12.Presentation.NLDPanels.Course.AddView(new SmartSchool.CourseRelated.NavViews.SubjectView());
            K12.Presentation.NLDPanels.Course.AddView(new SmartSchool.CourseRelated.NavViews.TeacherView());

            List<Customization.PlugIn.ExtendedContent.IContentItem> _items = new List<Customization.PlugIn.ExtendedContent.IContentItem>();

            List<Type> _type_list = new List<Type>(new Type[]{
                typeof(DetailPaneItem.BasicInfo),
                typeof(DetailPaneItem.SCAttendInfo),   
                //typeof(DetailPaneItem.ElectronicPaperPalmerworm) //移除電子報表功能
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
                K12.Presentation.NLDPanels.Course.AddDetailBulider(new SmartSchool.Adaatper.ContentItemBulider(var));
            }

            K12.Presentation.NLDPanels.Course.NavPaneContexMenu.GetChild("重新整理").Click += delegate { this.SyncAllBackground(); };

            K12.Presentation.NLDPanels.Course.RequiredDescription += delegate(object sender, RequiredDescriptionEventArgs e)
            {
                e.Result = Items[e.PrimaryKey].CourseName + " [" + Items[e.PrimaryKey].SchoolYear + "][" + Items[e.PrimaryKey].Semester + "]";
            };

            K12.Presentation.NLDPanels.Course.FilterMenu.SupposeHasChildern = true;
            K12.Presentation.NLDPanels.Course.FilterMenu.PopupOpen += delegate(object sender, PopupOpenEventArgs e)
            {
                List<SmartSchool.SemesterInfo> semesterList = new List<SmartSchool.SemesterInfo>();
                foreach (var item in Items)
                {
                    SmartSchool.SemesterInfo seme = new SmartSchool.SemesterInfo() { SchoolYear = item.SchoolYear, Semester = item.Semester };
                    if (!semesterList.Contains(seme))
                        semesterList.Add(seme);
                }
                semesterList.Sort();
                foreach (var item in semesterList)
                {
                    MenuButton btn = e.VirtualButtons[item.ToString()];
                    btn.AutoCheckOnClick = true;
                    btn.AutoCollapseOnClick = true;
                    btn.Checked = (item == FiltedSemester);
                    btn.Tag = item;
                    btn.CheckedChanged += delegate
                    {
                        if (btn.Checked)
                        {
                            FiltedSemester = (SmartSchool.SemesterInfo)btn.Tag;
                            SetSource();
                        }
                    };
                }
            };

            // 搜尋
            XmlElement preference = SmartSchool.CurrentUser.Instance.Preference["CourseSearchOptionPreference"];
            if (preference == null) preference = new XmlDocument().CreateElement("CourseSearchOptionPreference");

            SearchCourseName = K12.Presentation.NLDPanels.Course.SearchConditionMenu["課程名稱"];
            SearchCourseName.AutoCheckOnClick = true;
            SearchCourseName.AutoCollapseOnClick = false;
            SearchCourseName.Checked = preference.GetAttribute("SearchCourseName") != "False";

            SearchClassName = K12.Presentation.NLDPanels.Course.SearchConditionMenu["班級"];
            SearchClassName.AutoCheckOnClick = true;
            SearchClassName.AutoCollapseOnClick = false;
            SearchClassName.Checked = preference.GetAttribute("SearchClassName") != "False";

            SearchTeacherName = K12.Presentation.NLDPanels.Course.SearchConditionMenu["授課教師"];
            SearchTeacherName.AutoCheckOnClick = true;
            SearchTeacherName.AutoCollapseOnClick = false;
            SearchTeacherName.Checked = preference.GetAttribute("SearchTeacherName") != "False";


            K12.Presentation.NLDPanels.Course.Search += new EventHandler<SearchEventArgs>(Course_Search);
            K12.Presentation.NLDPanels.Course.SearchConditionMenu.PopupClose += delegate
            {
                preference.SetAttribute("SearchCourseName", "" + SearchCourseName.Checked);
                preference.SetAttribute("SearchClassName", "" + SearchClassName.Checked);
                preference.SetAttribute("SearchTeacherName", "" + SearchTeacherName.Checked);
                SmartSchool.CurrentUser.Instance.Preference["CourseSearchOptionPreference"] = preference;
            };

            SmartSchool.Customization.PlugIn.ExtendedColumn.ExtendCourseColumn.SetManager(this);
            SmartSchool.Customization.PlugIn.ContextMenu.CourseMenuButton.SetManager(this);
            MotherForm.AddPanel(K12.Presentation.NLDPanels.Course);
            _Initilized = true;
            this.UseFilter = true;
            SetSource();
        }
        // 課程名稱、班級、授課教師
        private MenuButton SearchCourseName, SearchClassName, SearchTeacherName;

        void Course_Search(object sender, SearchEventArgs e)
        {
            try
            {
                List<CourseInformation> CourseData = new List<CourseInformation>(Course.Instance.Items);
                Dictionary<string, CourseInformation> results = new Dictionary<string, CourseInformation>();
                Regex rx = new Regex(e.Condition, RegexOptions.IgnoreCase);

                // 課程名稱
                if (SearchCourseName.Checked)
                {
                    foreach (CourseInformation each in CourseData)
                    {
                        string name = each.CourseName;
                        if (rx.Match(name).Success)
                        {
                            string ID = each.Identity.ToString();
                            if (!results.ContainsKey(ID))
                                results.Add(ID, each);
                        }
                    }
                }

                // 班級
                if (SearchClassName.Checked)
                {
                    foreach (CourseInformation each in CourseData)
                    {
                        string name = each.ClassName;
                        if (rx.Match(name).Success)
                        {
                            string ID = each.Identity.ToString();
                            if (!results.ContainsKey(ID))
                                results.Add(ID, each);
                        }
                    }
                }
                // 授課教師
                if (SearchTeacherName.Checked)
                {
                    foreach (CourseInformation each in CourseData)
                    {
                        foreach (CourseInformation.Teacher tr in each.Teachers)
                        {
                            if (rx.Match(tr.TeacherName).Success)
                            {
                                string ID = each.Identity.ToString();
                                if (!results.ContainsKey(ID))
                                    results.Add(ID, each);
                            }
                        }
                    }
                }

                e.Result.AddRange(results.Keys);
            }
            catch (Exception ex) { }
        }



        private bool _Initilized = false;

        private Course()
        {
            SmartSchool.Broadcaster.Events.Items["課程/新增"].Handler += delegate
            {
                if (Loaded)
                    SyncAllBackground();
            };
            K12.Presentation.NLDPanels.Course.SelectedSourceChanged += delegate
            {
                //2012/11/27 - dylan新增
                MotherForm.SetStatusBarMessage("已選取" + K12.Presentation.NLDPanels.Course.SelectedSource.Count + "個課程");
                
                InvokeSelectionChanged();
            };
            K12.Presentation.NLDPanels.Course.TempSourceChanged += delegate
            {
                if (TemporalChanged != null)
                    TemporalChanged(this, new EventArgs());
            };
        }

        protected override Dictionary<string, CourseInformation> GetAllData()
        {
            Dictionary<string, CourseInformation> items = new Dictionary<string, CourseInformation>();
            foreach (var item in Feature.Course.QueryCourse.GetCourseById().GetContent().GetElements("Course"))
            {
                CourseInformation c = new CourseInformation(item);
                items.Add(c.Identity.ToString(), c);
            }
            return items;
        }

        protected override Dictionary<string, CourseInformation> GetData(IEnumerable<string> primaryKeys)
        {
            Dictionary<string, CourseInformation> items = new Dictionary<string, CourseInformation>();
            foreach (var item in Feature.Course.QueryCourse.GetCourseById(new List<string>(primaryKeys).ToArray()).GetContent().GetElements("Course"))
            {
                CourseInformation c = new CourseInformation(item);
                items.Add(c.Identity.ToString(), c);
            }
            return items;
        }

        /// <summary>
        /// 取得或設定，使用Filter機制
        /// </summary>
        private bool _UseFilter = false;
        protected bool UseFilter { get { return _UseFilter; } set { _UseFilter = value; K12.Presentation.NLDPanels.Course.FilterMenu.Visible = value; } }
        private void SetSource()
        {
            //資料載入中或資料未載入或畫面沒有設定完成就什麼都不做
            if (!_Initilized || !Loaded) return;
            if (_UseFilter)
                FillFilter();
            else
                K12.Presentation.NLDPanels.Course.SetFilteredSource(new List<string>(Items.Keys));
        }
        protected virtual void FillFilter()
        {
            //資料載入中或資料未載入或畫面沒有設定完成就什麼都不做
            if (!_Initilized || !Loaded) return;
            K12.Presentation.NLDPanels.Course.FilterMenu.Text = FiltedSemester.ToString();
            List<string> primaryKeys = new List<string>();
            foreach (var item in Items)
            {
                if (item.Semester == FiltedSemester.Semester && item.SchoolYear == FiltedSemester.SchoolYear)
                    primaryKeys.Add(item.Identity.ToString());
            }
            K12.Presentation.NLDPanels.Course.SetFilteredSource(primaryKeys);
        }
        public event EventHandler<CourseDeleteEventArgs> CourseDeleted;

        public event EventHandler<CourseChangeEventArgs> CourseChanged;

        public event EventHandler SelectionChanged;

        public event EventHandler TemporalChanged;

        public event EventHandler ForeignTableChanged;

        public void InvokeAfterCourseDelete(int courseId)
        {
            SyncData("" + courseId);

            if (CourseDeleted != null)
                CourseDeleted(this, new CourseDeleteEventArgs(courseId));
        }

        public void InvokeAfterCourseChange(params int[] courseId)
        {
            if (courseId.Length == 0) return;
            List<int> courses = new List<int>();
            courses.AddRange(courseId);
            List<string> ids = new List<string>();
            foreach (var id in courseId)
            {
                ids.Add("" + id);
            }
            SyncData(ids);

            if (CourseChanged != null)
                CourseChanged(this, new CourseChangeEventArgs(courses));
        }

        public void InvokeSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        public void InvokeForeignTableChanged()
        {
            if (ForeignTableChanged != null)
                ForeignTableChanged(this, EventArgs.Empty);
        }

        public List<CourseInformation> SelectionCourse
        {
            get
            {

                List<CourseInformation> selectedList = new List<CourseInformation>();
                foreach (var id in K12.Presentation.NLDPanels.Course.SelectedSource)
                {
                    selectedList.Add(Items[id]);
                }
                return selectedList;
            }
        }
        /// <summary>
        /// 待處理的課程。
        /// </summary>
        //public List<CourseInformation> TemporalCourse { get { return TemporaList; } }

        public List<CourseInformation> TempCourse
        {
            get
            {
                List<CourseInformation> list = new List<CourseInformation>();
                foreach (string id in K12.Presentation.NLDPanels.Course.TempSource)
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
                foreach (CourseInformation var in value)
                {
                    list.Add("" + var.Identity);
                    if (!K12.Presentation.NLDPanels.Course.TempSource.Contains("" + var.Identity))
                        insertList.Add("" + var.Identity);
                }
                foreach (var item in K12.Presentation.NLDPanels.Course.TempSource)
                {
                    if (!list.Contains(item))
                        removeList.Add(item);
                }
                K12.Presentation.NLDPanels.Course.AddToTemp(insertList);
                K12.Presentation.NLDPanels.Course.RemoveFromTemp(removeList);
            }
        }
        internal void EnsureCourse(int schoolYear, int semester)
        {

        }

        //暫時的解。檢查課程是否重複。2008/3/14
        public bool ValidateCourse(string course_name, string schoolyear, string semester)
        {
            if (string.IsNullOrEmpty(course_name)) return false;
            foreach (CourseInformation info in Items)
            {
                bool name_check = (info.CourseName == course_name);
                bool schoolyear_check = (info.SchoolYear.ToString() == schoolyear);
                bool semester_check = (info.Semester.ToString() == semester);

                if (name_check && schoolyear_check && semester_check)
                    return false;
            }
            return true;
        }

        public int FindSCAttendByCourseID(int courseId)
        {
            int attendCount = 0;

            DSResponse rsp = QueryCourse.GetSCAttend(courseId.ToString());
            foreach (XmlElement var in rsp.GetContent().GetElements("Student"))
            {
                attendCount++;
            }

            return attendCount;

        }

        #region IManager<IColumnItem> 成員

        void IManager<IColumnItem>.Add(IColumnItem instance)
        {
            K12.Presentation.NLDPanels.Course.AddListPaneField(new Adaatper.ColumnItemAdapter(instance));
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
                _Adapter = new Adaatper.ButtonAdapterPlugInToMenuButton(K12.Presentation.NLDPanels.Course.ListPaneContexMenu);
                K12.Presentation.NLDPanels.Course.SelectedSourceChanged += delegate
                 {
                     foreach (var item in _AdapterItem)
                     {
                         K12.Presentation.NLDPanels.Course.ListPaneContexMenu[item].Enable = K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0;
                     }
                 };
            }

            _Adapter.Add(instance);

            List<string> paths = new List<string>();
            if (instance.Path != "")
                paths.AddRange(instance.Path.Split('/'));
            paths.Add(instance.Text);
            _AdapterItem.Add(paths[0]);
            K12.Presentation.NLDPanels.Course.ListPaneContexMenu[paths[0]].Enable = K12.Presentation.NLDPanels.Course.SelectedSource.Count > 0;
        }

        void IManager<ButtonAdapter>.Remove(ButtonAdapter instance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IManager<IContentItem> 成員

        void IManager<IContentItem>.Add(IContentItem instance)
        {
            K12.Presentation.NLDPanels.Course.AddDetailBulider(new SmartSchool.Adaatper.ContentItemBulider(instance));
        }

        void IManager<IContentItem>.Remove(IContentItem instance)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
