using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SmartSchool.ClassRelated.NavView
{
    public partial class GradeYearClassView : FISCA.Presentation.NavView
    {
        public GradeYearClassView()
        {
            InitializeComponent();
        }
        private SourceTreeNode _AllClassNode = new SourceTreeNode() { Text = "所有班級" };
        private SortedList<int, SourceTreeNode> _GradeYearTreeNodes = new SortedList<int, SourceTreeNode>();
        private SourceTreeNode _NoGradeNode = new SourceTreeNode() { Text = "未分年級" };

        private void GradeYearClassView_SourceChanged(object sender, EventArgs e)
        {
            List<string> all = new List<string>();
            List<string> nog = new List<string>();
            SortedList<int, List<string>> grades = new SortedList<int, List<string>>();
            foreach ( var item in Source )
            {
                all.Add(item);
                ClassInfo cinfo = Class.Instance[item];
                int gy = 0;
                if ( int.TryParse(cinfo.GradeYear, out gy) )
                {
                    if ( !grades.ContainsKey(gy) ) grades.Add(gy, new List<string>());
                    grades[gy].Add(item);
                }
                else
                {
                    nog.Add(item);
                }
            }

            _AllClassNode.Items.ReplaceAll(all);
            if ( !advTree1.Nodes.Contains(_AllClassNode) )
            {
                advTree1.Nodes.Add(_AllClassNode);
                _AllClassNode.Expand();
            }
            SortedList<int, SourceTreeNode> oldGN = _GradeYearTreeNodes;
            SortedList<int, SourceTreeNode> gradeYearTreeNodes = new SortedList<int, SourceTreeNode>();
            List<SourceTreeNode> order = new List<SourceTreeNode>();
            foreach ( var gyear in grades.Keys )
            {
                SourceTreeNode node = oldGN.ContainsKey(gyear) ? oldGN[gyear] : new SourceTreeNode() { Text = "" + gyear + "年級" };
                node.Items.ReplaceAll(grades[gyear]);
                gradeYearTreeNodes.Add(gyear, node);
                if ( !_AllClassNode.Nodes.Contains(node) )
                {
                    _AllClassNode.Nodes.Add(node);
                }
                order.Add(node);
            }
            _GradeYearTreeNodes = gradeYearTreeNodes;
            if ( nog.Count > 0 )
            {
                _NoGradeNode.Items.ReplaceAll(nog);
                if ( !_AllClassNode.Nodes.Contains(_NoGradeNode) )
                    _AllClassNode.Nodes.Add(_NoGradeNode);
                order.Add(_NoGradeNode);
            }

            _AllClassNode.Nodes.Sort(new MoveTargetsButtonSorter(order));
            for ( int i = 0 ; i < _AllClassNode.Nodes.Count - grades.Count - ( nog.Count > 0 ? 1 : 0 ) ; i++ )
            {
                if ( advTree1.SelectedNode == _AllClassNode.Nodes[0] )
                    advTree1.SelectedNode = null;
                _AllClassNode.Nodes.RemoveAt(0);
            }
            advTree1_AfterNodeSelect(null, new DevComponents.AdvTree.AdvTreeNodeEventArgs(DevComponents.AdvTree.eTreeAction.Mouse, advTree1.SelectedNode));
        }

        class MoveTargetsButtonSorter : System.Collections.IComparer
        {
            private List<SourceTreeNode> _Target;
            public MoveTargetsButtonSorter(List<SourceTreeNode> target)
            {
                _Target = target;
            }
            #region IComparer 成員

            public int Compare(object x, object y)
            {
                int indexX = _Target.IndexOf((SourceTreeNode)x);
                int indexY = _Target.IndexOf((SourceTreeNode)y);
                return indexX.CompareTo(indexY);
            }

            #endregion
        }
        class SourceTreeNode : DevComponents.AdvTree.Node
        {
            public SourceTreeNode()
            {
                Items = new FISCA.Presentation.PrimaryKeysCollection();
                Items.ItemsChanged += delegate
                {
                    UpdateText();
                };
            }
            private string _Text = "";
            public new string Text
            {
                get { return _Text; }
                set
                {
                    _Text = value;
                    UpdateText();
                }
            }
            //private FISCA.Presentation.PrimaryKeysCollection _Items = new FISCA.Presentation.PrimaryKeysCollection();
            public FISCA.Presentation.PrimaryKeysCollection Items { get; private set; }
            private void UpdateText()
            {
                base.Text = _Text + "(" + Items.Count + ")";
            }
        }

        private void advTree1_AfterNodeSelect(object sender, DevComponents.AdvTree.AdvTreeNodeEventArgs e)
        {
            if ( e.Node != null )
            {
                this.SetListPaneSource(( (SourceTreeNode)e.Node ).Items, ( Control.ModifierKeys & Keys.Control ) == Keys.Control, ( Control.ModifierKeys & Keys.Shift ) == Keys.Shift);
            }
            else
            {
                this.SetListPaneSource(new List<string>(), false, false);
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
