﻿using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;
using SmartSchool.ClassRelated.Search;
using SmartSchool.TeacherRelated.Search;
using SmartSchool.Common;
using FISCA.Presentation;

namespace SmartSchool.TeacherRelated.Divider
{
    public class TeacherDividerProvider : SmartSchool.API.PlugIn.View.NavigationPlanner
    {
        private DragDropTreeView _TreeViewStudent;
        private ITeacherDivider _Divider;
        private TreeNode _SelectionNode;
        private ISourceProvider<BriefTeacherData, ISearchTeacher> _SourceProvider;

        internal TeacherDividerProvider(ITeacherDivider divider)
        {
            _Divider = divider;
            #region toolStripMenuItem1
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(93, 22);
            toolStripMenuItem1.Text = "重新整理";
            toolStripMenuItem1.Click += delegate { Teacher.Instance.SyncAllBackground(); K12.Data.Teacher.RemoveAll(); };
            #endregion

            #region contextMenuStrip1
            System.Windows.Forms.ContextMenuStrip contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip();
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripMenuItem1});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.ShowImageMargin = false;
            contextMenuStrip1.Size = new System.Drawing.Size(94, 26);
            #endregion

            this._TreeViewStudent = new DragDropTreeView();
            this._TreeViewStudent.BackColor = System.Drawing.Color.White;
            this._TreeViewStudent.ContextMenuStrip = contextMenuStrip1;
            this._TreeViewStudent.Cursor = System.Windows.Forms.Cursors.Default;
            this._TreeViewStudent.Dock = System.Windows.Forms.DockStyle.Fill;
            this._TreeViewStudent.ForeColor = System.Drawing.Color.Black;
            this._TreeViewStudent.HotTracking = true;
            this._TreeViewStudent.ItemHeight = 20;
            this._TreeViewStudent.Location = new System.Drawing.Point(0, 23);
            this._TreeViewStudent.Name = "treeViewStudent";
            this._TreeViewStudent.Size = new System.Drawing.Size(139, 410);
            this._TreeViewStudent.TabIndex = 1;
            this._TreeViewStudent.NodeMouseClick += new TreeNodeMouseClickEventHandler(_TreeViewStudent_NodeMouseClick);
            this._TreeViewStudent.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(_TreeViewStudent_NodeMouseClick);

            _Divider.TargetTreeView = _TreeViewStudent;
            this.Text = _Divider.Name;

            Application.Idle += new EventHandler(Application_Idle);
        }
        void _TreeViewStudent_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if ( Control.ModifierKeys == Keys.Shift && e.Node is ISourceProvider<BriefTeacherData, ISearchTeacher> && ( (ISourceProvider<BriefTeacherData, ISearchTeacher>)e.Node ).Source.Count > 0 )
            {
                List<BriefTeacherData> tempList = Teacher.Instance.TempTeacher;
                List<BriefTeacherData> insertList = new List<BriefTeacherData>();
                foreach ( BriefTeacherData var in ( (ISourceProvider<BriefTeacherData, ISearchTeacher>)e.Node ).Source )
                {
                    if ( !tempList.Contains(var) )
                        insertList.Add(var);
                }
                tempList.AddRange(insertList);
                Teacher.Instance.TempTeacher = tempList;
                MotherForm.SetStatusBarMessage("將\"" + e.Node.Text + "\"加入待處理");
            }
            if ( _SelectionNode == e.Node && e.Node is ISourceProvider<BriefTeacherData, ISearchTeacher> && ( (ISourceProvider<BriefTeacherData, ISearchTeacher>)e.Node ).Source.Count > 0 )
            {
                List<string> list = new List<string>(this.SelectedSource);
                this.SelectedSource.Clear();
                this.SelectedSource.AddRange(list);
            }
        }
        private void SetSourceProvider(ISourceProvider<BriefTeacherData, ISearchTeacher> value)
        {
            if ( _SourceProvider == value )
                return;
            //設定_SourceProvider為value
            if ( _SourceProvider != null )
            {
                _SourceProvider.SourceChanged -= new EventHandler(_SourceProvider_SourceChanged);
            }
            _SourceProvider = value;
            //dgvStudent.Rows.Clear();
            if ( _SourceProvider != null )
            {
                _SourceProvider.SourceChanged += new EventHandler(_SourceProvider_SourceChanged);
            }
        }

        void _SourceProvider_SourceChanged(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            foreach ( BriefTeacherData var in _SourceProvider.Source )
            {
                list.Add(var.ID);
            }
            this.SelectedSource.Clear();
            this.SelectedSource.AddRange(list);
        }

        void Application_Idle(object sender, EventArgs e)
        {
            if ( _SelectionNode != _TreeViewStudent.SelectedNode )
            {
                _SelectionNode = _TreeViewStudent.SelectedNode;
                if ( _TreeViewStudent.SelectedNode != null && _TreeViewStudent.SelectedNode is ISourceProvider<BriefTeacherData, ISearchTeacher> )
                {
                    SetSourceProvider((ISourceProvider<BriefTeacherData, ISearchTeacher>)_TreeViewStudent.SelectedNode);
                    List<string> list = new List<string>();
                    foreach ( BriefTeacherData var in ( (ISourceProvider<BriefTeacherData, ISearchTeacher>)_TreeViewStudent.SelectedNode ).Source )
                    {
                        list.Add(var.ID);
                    }
                    this.SelectedSource.Clear();
                    this.SelectedSource.AddRange(list);
                }
                else
                    this.SelectedSource.Clear();
            }
        }

        public override Control DisplayControl
        {
            get { return _TreeViewStudent; }
        }

        protected override void Layout(List<string> source)
        {
            Dictionary<string, BriefTeacherData> list = new Dictionary<string, BriefTeacherData>();
            foreach ( string id in source )
            {
                list.Add(id, Teacher.Instance.Items[id]);
            }
            _Divider.Divide(list);
        }
    }
}
