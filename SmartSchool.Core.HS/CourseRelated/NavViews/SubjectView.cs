using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation;

namespace SmartSchool.CourseRelated.NavViews
{
    public partial class SubjectView : UserControl, INavView
    {
        public SubjectView()
        {
            InitializeComponent();
            this.Text = "依科目檢視";
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

            SourceTreeNode rootNode = new SourceTreeNode();
            rootNode.Text = "所有課程";
            rootNode.Courses.AddRange(PrimaryKeys);
            rootNode.UpdateText();

            SortedList<string, List<string>> categoryList = new SortedList<string, List<string>>();
            List<string> noCategroyList = new List<string>();

            foreach (var key in PrimaryKeys)
            {
                var courseRec = Course.Instance.Items[key];
                string category = courseRec.Subject;

                if (!string.IsNullOrEmpty(category))
                {
                    if (!categoryList.ContainsKey(category))
                        categoryList.Add(category, new List<string>());
                    categoryList[category].Add(key);
                }
                else
                {
                    noCategroyList.Add(key);
                }
            }

            foreach (var categoryKey in categoryList.Keys)
            {
                SourceTreeNode categoryNode = new SourceTreeNode();
                categoryNode.Text = categoryKey;
                categoryNode.Courses.AddRange(categoryList[categoryKey]);
                categoryNode.UpdateText();
                rootNode.Nodes.Add(categoryNode);
            }

            if (noCategroyList.Count > 0)
            {
                SourceTreeNode categoryNode = new SourceTreeNode();
                categoryNode.Text="未設定科目課程";
                categoryNode.Courses.AddRange(noCategroyList);
                categoryNode.UpdateText();                
                rootNode.Nodes.Add(categoryNode);
            }
            if (selectPath.Count != 0)
            {
                selectNode = SelectNode(selectPath, 0, advTree1.Nodes);
                if (selectNode != null)
                    advTree1.SelectedNode = selectNode;
            }

            rootNode.Expand();

            this.advTree1.Nodes.Add(rootNode);

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


        private void SavePathExpand(string p, Dictionary<string, bool> pathExpand, DevComponents.AdvTree.NodeCollection nodeCollection)
        {
            foreach ( SourceTreeNode item in nodeCollection )
            {
                string path = p + "@//'" + item.Text;
                pathExpand.Add(path, item.Expanded);
                SavePathExpand(path, pathExpand, item.Nodes);
            }
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
                    args.AddToTemp= ( Control.ModifierKeys & Keys.Shift ) == Keys.Shift;
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
