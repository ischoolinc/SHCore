using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation;
using SmartSchool.ClassRelated;

namespace SmartSchool.CourseRelated.NavViews
{
    public partial class GradeYear_Class_View : UserControl, INavView
    {
        private List<string> _PrimaryKeys = new List<string>();
        public GradeYear_Class_View()
        {
            InitializeComponent();
            this.Text = "依班級檢視";
            Class.Instance.ItemUpdated += delegate
            {
                if ( Active )
                    Layout(_PrimaryKeys);
            };
            Source = new PrimaryKeysCollection();
            Source.ItemsChanged += delegate
            {
                Layout(new List<string>(Source));
            };
        }

        #region NavView 成員

        public string NavText
        {
            get { return this.Text; }
        }

        public PrimaryKeysCollection Source
        {
            get;
            private set;
        }

        public bool Active
        {
            get;
            set;
        }

        public string Description
        {
            get { return ""; }
        }

        public Control DisplayPane
        {
            get { return this; }
        }

        public new void Layout(List<string> PrimaryKeys)
        {
            _PrimaryKeys = PrimaryKeys;
            #region 記錄顯示情形
            List<string> selectPath = new List<string>();
            var _VScrollValue = advTree1.VScrollBar == null ? 0 : advTree1.VScrollBar.Value;
            var _HScrollValue = advTree1.HScrollBar == null ? 0 : advTree1.HScrollBar.Value;
            var selectNode = advTree1.SelectedNode;
            if ( selectNode != null )
            {
                while ( selectNode != null )
                {
                    selectPath.Insert(0, ( (SourceTreeNode)selectNode ).Text);
                    selectNode = selectNode.Parent;
                }
            }
            Dictionary<string, bool> pathExpand = new Dictionary<string, bool>();
            SavePathExpand("", pathExpand, advTree1.Nodes);
            #endregion
            advTree1.Nodes.Clear();
            SortedList<int?, List<string>> gradeYearList = new SortedList<int?, List<string>>();
            List<string> nullGradeList = new List<string>();
            List<string> nullClassList = new List<string>();
            Dictionary<ClassInfo, List<string>> classList = new Dictionary<ClassInfo, List<string>>();
            Dictionary<ClassInfo, int?> classGradeYear = new Dictionary<ClassInfo, int?>();
            List<ClassInfo> classes = new List<ClassInfo>();

            SourceTreeNode rootNode = new SourceTreeNode();

            rootNode.Text = "所有科目";
            rootNode.Courses.AddRange(PrimaryKeys);

            foreach ( var key in PrimaryKeys )
            {
                var courseRec = Course.Instance[key];

                ClassInfo classRec = Class.Instance.Items.ContainsKey("" + courseRec.ClassID) ? Class.Instance["" + courseRec.ClassID] : null;

                int? g;
                if ( classRec != null )
                {
                    string gradeYear = classRec.GradeYear;
                    int gyear = 0;
                    if ( int.TryParse(gradeYear, out gyear) )
                    {
                        g = gyear;
                        if ( !gradeYearList.ContainsKey(g) )
                            gradeYearList.Add(g, new List<string>());
                        gradeYearList[g].Add(key);
                    }
                    else
                    {
                        g = null;
                        nullGradeList.Add(key);
                    }
                    if ( !classList.ContainsKey(classRec) )
                    {
                        classList.Add(classRec, new List<string>());
                        classes.Add(classRec);
                    }
                    classList[classRec].Add(key);
                    if ( !classGradeYear.ContainsKey(classRec) )
                        classGradeYear.Add(classRec, g);
                }
                else
                {
                    g = null;
                    nullGradeList.Add(key);
                    nullClassList.Add(key);
                }
            }
            classes.Sort();

            foreach ( var gyear in gradeYearList.Keys )
            {
                SourceTreeNode gyearNode = new SourceTreeNode();
                switch ( gyear )
                {
                    case 1:
                        gyearNode.Text = "一年級";
                        break;
                    case 2:
                        gyearNode.Text = "二年級";
                        break;
                    case 3:
                        gyearNode.Text = "三年級";
                        break;
                    case 4:
                        gyearNode.Text = "四年級";
                        break;
                    case 5:
                        gyearNode.Text = "五年級";
                        break;
                    case 6:
                        gyearNode.Text = "六年級";
                        break;
                    case 7:
                        gyearNode.Text = "七年級";
                        break;
                    case 8:
                        gyearNode.Text = "八年級";
                        break;
                    case 9:
                        gyearNode.Text = "九年級";
                        break;
                    default:
                        gyearNode.Text = "" + gyear + "年級";
                        break;

                }
                gyearNode.Courses.AddRange(gradeYearList[gyear]);
                gyearNode.UpdateText();
                rootNode.Nodes.Add(gyearNode);

                foreach ( var classRec in classes )
                {
                    if ( classGradeYear[classRec] == gyear )
                    {
                        SourceTreeNode classNode = new SourceTreeNode();
                        classNode.CreditCount = true;
                        classNode.Text = classRec.ClassName;

                        classNode.Courses.AddRange(classList[classRec]);
                        classNode.UpdateText();
                        gyearNode.Nodes.Add(classNode);
                    }
                }
            }
            if ( nullGradeList.Count > 0 )
            {
                SourceTreeNode gyearNode = new SourceTreeNode();

                gyearNode.Text = "未分年級";

                gyearNode.Courses.AddRange(nullGradeList);
                gyearNode.UpdateText();
                rootNode.Nodes.Add(gyearNode);

                foreach ( var classRec in classes )
                {
                    if ( classGradeYear[classRec] == null )
                    {
                        SourceTreeNode classNode = new SourceTreeNode();
                        classNode.CreditCount = true;
                        classNode.Text = classRec.ClassName + "(" + classList[classRec].Count + ")";

                        classNode.Courses.AddRange(classList[classRec]);
                        classNode.UpdateText();
                        gyearNode.Nodes.Add(classNode);
                    }
                }
                if ( nullClassList.Count > 0 )
                {
                    SourceTreeNode classNode = new SourceTreeNode();

                    classNode.Text = "未分班";
                    classNode.CreditCount = true;
                    classNode.Courses.AddRange(nullClassList);
                    classNode.UpdateText();
                    gyearNode.Nodes.Add(classNode);
                }
            }

            rootNode.Expand();
            rootNode.UpdateText();
            advTree1.Nodes.Add(rootNode);

            #region 還原顯示狀態
            LoadPathExpand("", pathExpand, advTree1.Nodes);
            advTree1.RecalcLayout();
            if ( advTree1.VScrollBar != null && advTree1.VScrollBar.Enabled )
            {
                if ( advTree1.VScrollBar.Maximum >= _VScrollValue )
                    advTree1.VScrollBar.Value = _VScrollValue;
            }
            if ( advTree1.HScrollBar != null && advTree1.HScrollBar.Enabled )
            {
                if ( advTree1.HScrollBar.Maximum >= _HScrollValue )
                    advTree1.HScrollBar.Value = _HScrollValue;
            }
            advTree1.Refresh();
            if ( selectPath.Count != 0 )
            {
                selectNode = SelectNode(selectPath, 0, advTree1.Nodes);
                if ( selectNode != null )
                    advTree1.SelectedNode = selectNode;
            }
            #endregion
        }

        private void LoadPathExpand(string p, Dictionary<string, bool> pathExpand, DevComponents.AdvTree.NodeCollection nodeCollection)
        {
            foreach ( SourceTreeNode item in nodeCollection )
            {
                string path = p + "@//'" + item.Text;
                if ( pathExpand.ContainsKey(path) )
                    item.Expanded = pathExpand[path];
                LoadPathExpand(path, pathExpand, item.Nodes);
            }
        }

        private void SavePathExpand(string p, Dictionary<string, bool> pathExpand, DevComponents.AdvTree.NodeCollection nodeCollection)
        {
            foreach ( SourceTreeNode item in nodeCollection )
            {
                string path = p + "@//'" + item.Text;
                pathExpand.Add(path, item.Expanded);
                SavePathExpand(path, pathExpand, item.Nodes);
            }
        }

        private DevComponents.AdvTree.Node SelectNode(List<string> selectPath, int level, DevComponents.AdvTree.NodeCollection nodeCollection)
        {
            foreach ( var item in nodeCollection )
            {
                if ( item is SourceTreeNode )
                {
                    var node = (SourceTreeNode)item;
                    if ( node.Text == selectPath[level] )
                    {
                        if ( selectPath.Count - 1 == level )
                            return node;
                        else
                        {
                            var childNode = SelectNode(selectPath, level + 1, node.Nodes);
                            if ( childNode == null )
                                return node;
                            else
                                return childNode;
                        }
                    }
                }
            }
            return null;
        }

        public event EventHandler<ListPaneSourceChangedEventArgs> ListPaneSourceChanged;

        #endregion
        private void advTree1_AfterNodeSelect(object sender, DevComponents.AdvTree.AdvTreeNodeEventArgs e)
        {
            if ( ListPaneSourceChanged != null )
            {
                if ( e.Node != null )
                {
                    ListPaneSourceChangedEventArgs args = new ListPaneSourceChangedEventArgs(( (SourceTreeNode)e.Node ).Courses);
                    args.SelectedAll = ( Control.ModifierKeys & Keys.Control ) == Keys.Control;
                    args.AddToTemp = ( Control.ModifierKeys & Keys.Shift ) == Keys.Shift;
                    ListPaneSourceChanged(this, args);
                }
                else
                {
                    ListPaneSourceChangedEventArgs args = new ListPaneSourceChangedEventArgs(new List<string>());
                    args.SelectedAll = false;
                    args.AddToTemp = false;
                    ListPaneSourceChanged(this, args);
                }
            }
        }

        private void advTree1_NodeClick(object sender, DevComponents.AdvTree.TreeNodeMouseEventArgs e)
        {

            if ( e.Node == advTree1.SelectedNode )
                advTree1_AfterNodeSelect(null, new DevComponents.AdvTree.AdvTreeNodeEventArgs(DevComponents.AdvTree.eTreeAction.Mouse, advTree1.SelectedNode));
        }

        private void advTree1_NodeDoubleClick(object sender, DevComponents.AdvTree.TreeNodeMouseEventArgs e)
        {

            if ( e.Node == advTree1.SelectedNode )
                advTree1_AfterNodeSelect(null, new DevComponents.AdvTree.AdvTreeNodeEventArgs(DevComponents.AdvTree.eTreeAction.Mouse, advTree1.SelectedNode));
        }
    }
}
