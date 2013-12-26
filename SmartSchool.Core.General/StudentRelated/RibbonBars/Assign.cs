using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using StudentEntity = SmartSchool.StudentRelated.Student;
using SmartSchool.StudentRelated;
using DevComponents.DotNetBar;
using SmartSchool.TagManage;
using SmartSchool.Feature.Tag;
using SmartSchool.Common;
//using SmartSchool.CourseRelated;
using SmartSchool.StudentRelated.Validate;
using SmartSchool.Common.Validate;
using FISCA.DSAUtil;
using System.Xml;
//using SmartSchool.SmartPlugIn.Common;
using SmartSchool.ApplicationLog;
using SmartSchool.Security;
using FISCA.Presentation;

namespace SmartSchool.StudentRelated.RibbonBars
{
    public partial class Assign : RibbonBarBase
    {
        FeatureAccessControl tagCtrl;
        FeatureAccessControl attendCtrl;

        private ButtonItemPlugInManager reportManager;
        private ButtonItem ClearItem, AllTagItem, ShortcutItem;
        private TagManager _tag_manager;
        private StudentTagManager _selection_tags;
        private bool _is_selection;

        public Assign()
        {
        }


        internal void Setup()
        {
            //K12.Presentation.NLDPanels.Student.RibbonBarItems["指定"].Index = 3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Assign));
            //K12.Presentation.NLDPanels.Student.RibbonBarItems["指定"].OverflowButtonImage = ( (System.Drawing.Image)( resources.GetObject("MainRibbonBar.OverflowButtonImage") ) );
            //K12.Presentation.NLDPanels.Student.RibbonBarItems["指定"].ResizeOrderIndex = 50;
            K12.Presentation.NLDPanels.Student.RibbonBarItems["指定"].AutoOverflowEnabled = false;
            MenuButton ctxTag = K12.Presentation.NLDPanels.Student.RibbonBarItems["指定"]["類別"];
            //ctxTag.Image = ( (System.Drawing.Image)( resources.GetObject("ctxTag.Image") ) );
            ctxTag.Image = Properties.Resources.ctxTag_Image;
            ctxTag.SupposeHasChildern = true;
            ctxTag.Text = "類別";
            ctxTag.PopupOpen += delegate(object sender, FISCA.Presentation.PopupOpenEventArgs e)
            {
                var ClearItem = e.VirtualButtons["<b><i>清除所有類別</i></b>"];
                ClearItem.Click += new EventHandler(ClearItem_Click);

                if ( _tag_manager == null )
                {
                    _tag_manager = StudentEntity.Instance.TagManager;
                }

                if ( _selection_tags == null )
                    _selection_tags = StudentEntity.Instance.SelectionTagManager;

                _selection_tags.RefreshSelectionReference();
                if ( !_selection_tags.IsSynchronized )
                    _selection_tags.SynchorizeSelection();

                Dictionary<TagInfo, bool> tagSelection = new Dictionary<TagInfo, bool>();
                foreach ( TagInfo each in _tag_manager.Tags.Values )
                {
                    tagSelection.Add(each, true);
                }
                foreach ( EntityItem each in _selection_tags.EntityItems.Values )
                {
                    foreach ( var key in _tag_manager.Tags.Values )
                    {
                        if ( tagSelection[key] && !each.Tags.Contains(key.Identity) )
                        {
                            tagSelection[key] = false;
                        }
                    }
                }

                foreach ( Prefix each in _tag_manager.Prefixes.Values )
                {
                    if ( string.IsNullOrEmpty(each.Name) )
                        continue;
                    MenuButton item = e.VirtualButtons[each.Name];
                }

                foreach ( TagInfo each in _tag_manager.Tags.Values )
                {
                    MenuButton btnTag = string.IsNullOrEmpty(each.Prefix) ? btnTag = e.VirtualButtons[each.Name] : btnTag = e.VirtualButtons[each.Prefix][each.Name];
                    btnTag.Checked = tagSelection[each];
                    btnTag.AutoCheckOnClick = true;
                    btnTag.AutoCollapseOnClick = false;
                    btnTag.Tag = each;
                    btnTag.CheckedChanged += new EventHandler(btnTag_CheckedChanged);
                }


                var AllTagItem = e.VirtualButtons["所有學生類別…"];
                AllTagItem.BeginGroup = true;
                AllTagItem.Click += new EventHandler(AllTagItem_Click);

                var ShortcutItem = e.VirtualButtons["設定快速點選類別"];
                ShortcutItem.Enable = false;
                ShortcutItem.Click += new EventHandler(ShortcutItem_Click);
            };
            //權限判斷 - 指定/類別	Button0100
            tagCtrl = new FeatureAccessControl("Button0100");
            ctxTag.Enable = tagCtrl.Executable();
        }

        void btnTag_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                TagManager tags = StudentEntity.Instance.TagManager;
                MenuButton item = sender as MenuButton;
                TagInfo tag = (TagInfo)item.Tag;
                int identity = tag.Identity;
                List<int> students = new List<int>();

                if ( item.Checked ) //將沒有此 Tag 的學生加上此 Tag。
                {
                    foreach ( EntityItem each in _selection_tags.EntityItems.Values )
                    {
                        if ( !each.Tags.Contains(identity) )
                            students.Add(each.Identity);
                    }

                    if ( students.Count > 0 )
                    {
                        EditStudentTag.Add(students, identity);

                        //log
                        LogIt(tag, students, "新增");

                        List<string> studentIDList = new List<string>();
                        foreach ( int id in students )
                        {
                            studentIDList.Add("" + id);
                        }
                        //SmartSchool.StudentRelated.Student.Instance.InvokBriefDataChanged(studentIDList.ToArray());
                        SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(studentIDList.ToArray());
                    }
                }
                else //將有此 Tag  的學生移除此 Tag。
                {
                    foreach ( EntityItem each in _selection_tags.EntityItems.Values )
                        students.Add(each.Identity);

                    if ( students.Count > 0 )
                    {
                        EditStudentTag.Remove(students, identity);

                        //log
                        LogIt(tag, students, "移除");

                        List<string> studentIDList = new List<string>();
                        foreach ( int id in students )
                        {
                            studentIDList.Add("" + id);
                        }
                        //SmartSchool.StudentRelated.Student.Instance.InvokBriefDataChanged(studentIDList.ToArray());
                        SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(studentIDList.ToArray());
                    }
                }

                //ctxTag.Enabled = false;
                _selection_tags.SynchorizeSelection();
                tags.Refresh();
            }
            catch ( Exception ex )
            {
                MsgBox.Show("更新分類資料發生錯誤，請「重新整理」後再試一次。(" + ex.Message + ")");
            }
        }

        private void ShortcutItem_Click(object sender, EventArgs e)
        {
        }

        private void AllTagItem_Click(object sender, EventArgs e)
        {
            StudentTagForm tagForm = new StudentTagForm();
            tagForm.ShowDialog();
        }

        private void ClearItem_Click(object sender, EventArgs e)
        {
            TagManager tags = _tag_manager;
            List<int> students = new List<int>();

            foreach ( EntityItem each in _selection_tags.EntityItems.Values )
                students.Add(each.Identity);

            try
            {
                EditStudentTag.Remove(students);


                List<string> studentIDList = new List<string>();
                foreach ( int id in students )
                {
                    studentIDList.Add("" + id);
                }
                //SmartSchool.StudentRelated.Student.Instance.InvokBriefDataChanged(studentIDList.ToArray());
                SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(studentIDList.ToArray());

                _selection_tags.SynchorizeSelection();
                tags.Refresh();
            }
            catch ( Exception ex )
            {
                MsgBox.Show("更新分類資料發生錯誤。(" + ex.Message + ")");
            }
        }

        //private void ctxTag_PopupShowing(object sender, EventArgs e)
        //{
        //    AllTagItem.BeginGroup = _is_selection;

        //    if ( !_is_selection )
        //    {
        //        ctxTag.SubItems.Clear();
        //        ctxTag.InsertItemAt(AllTagItem, ctxTag.SubItems.Count, true);
        //        ctxTag.InsertItemAt(ShortcutItem, ctxTag.SubItems.Count, true);
        //        return;
        //    }

        //    if ( _tag_manager == null )
        //    {
        //        _tag_manager = StudentEntity.Instance.TagManager;
        //        _tag_manager.TagRefresh += new EventHandler(TagManager_TagRefresh);
        //    }

        //    if ( _selection_tags == null )
        //        _selection_tags = StudentEntity.Instance.SelectionTagManager;

        //    ctxTag.SubItems.Clear();
        //    ctxTag.SubItems.Clear();

        //    ctxTag.SubItems.Add(ClearItem);

        //    foreach ( Prefix each in _tag_manager.Prefixes.Values )
        //    {
        //        if ( string.IsNullOrEmpty(each.Name) )
        //            continue;

        //        ButtonItem item = new ButtonItem();
        //        item.Text = each.Name;
        //        item.Tag = each;
        //        item.Name = each.Name;

        //        ctxTag.InsertItemAt(item, ctxTag.SubItems.Count, true);
        //    }

        //    Dictionary<int, TagMenuItem> menus = new Dictionary<int, TagMenuItem>();
        //    foreach ( TagInfo each in _tag_manager.Tags.Values )
        //    {
        //        ButtonItem parent;

        //        TagMenuItem item = new TagMenuItem(each);
        //        item.Text = each.Name;
        //        item.Click += new EventHandler(TagItem_Click);
        //        menus.Add(each.Identity, item);

        //        if ( string.IsNullOrEmpty(each.Prefix) )
        //        {
        //            ctxTag.InsertItemAt(item, 1, true);
        //            continue;
        //        }
        //        else if ( ctxTag.SubItems.Contains(each.Prefix) )
        //            parent = ctxTag.SubItems[each.Prefix] as ButtonItem;
        //        else
        //            continue; //避免錯誤。

        //        parent.SubItems.Add(item);
        //    }

        //    _selection_tags.RefreshSelectionReference();
        //    if ( !_selection_tags.IsSynchronized )
        //        _selection_tags.SynchorizeSelection();

        //    foreach ( EntityItem each in _selection_tags.EntityItems.Values )
        //    {
        //        foreach ( int identity in each.Tags )
        //        {
        //            if ( menus.ContainsKey(identity) )
        //            {
        //                TagMenuItem item = menus[identity];
        //                item.AddStateCount();
        //            }
        //        }
        //    }

        //    foreach ( TagMenuItem item in menus.Values )
        //        item.CalcuateState(_selection_tags.EntityItems.Count);

        //    ctxTag.InsertItemAt(AllTagItem, ctxTag.SubItems.Count, true);
        //    ctxTag.InsertItemAt(ShortcutItem, ctxTag.SubItems.Count, true);

        //}

        //private void TagManager_TagRefresh(object sender, EventArgs e)
        //{
        //    ctxTag.Enabled = true;
        //}

        //private void TagItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        TagManager tags = StudentEntity.Instance.TagManager;
        //        if ( sender is TagMenuItem )
        //        {
        //            TagMenuItem item = sender as TagMenuItem;
        //            int identity = item.TagIdentity;
        //            item.Checked = !item.Checked;
        //            List<int> students = new List<int>();

        //            if ( item.Checked ) //將沒有此 Tag 的學生加上此 Tag。
        //            {
        //                foreach ( EntityItem each in _selection_tags.EntityItems.Values )
        //                {
        //                    if ( !each.Tags.Contains(identity) )
        //                        students.Add(each.Identity);
        //                }

        //                if ( students.Count > 0 )
        //                {
        //                    EditStudentTag.Add(students, identity);

        //                    //log
        //                    LogIt(item.StateOwner, students, "新增");

        //                    List<string> studentIDList = new List<string>();
        //                    foreach ( int id in students )
        //                    {
        //                        studentIDList.Add("" + id);
        //                    }
        //                    //SmartSchool.StudentRelated.Student.Instance.InvokBriefDataChanged(studentIDList.ToArray());
        //                    SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(studentIDList.ToArray());
        //                }
        //            }
        //            else //將有此 Tag  的學生移除此 Tag。
        //            {
        //                foreach ( EntityItem each in _selection_tags.EntityItems.Values )
        //                    students.Add(each.Identity);

        //                if ( students.Count > 0 )
        //                {
        //                    EditStudentTag.Remove(students, identity);

        //                    //log
        //                    LogIt(item.StateOwner, students, "移除");

        //                    List<string> studentIDList = new List<string>();
        //                    foreach ( int id in students )
        //                    {
        //                        studentIDList.Add("" + id);
        //                    }
        //                    //SmartSchool.StudentRelated.Student.Instance.InvokBriefDataChanged(studentIDList.ToArray());
        //                    SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(studentIDList.ToArray());
        //                }
        //            }

        //            ctxTag.Enabled = false;
        //            _selection_tags.SynchorizeSelection();
        //            tags.Refresh();
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        MsgBox.Show("更新分類資料發生錯誤，請「重新整理」後再試一次。(" + ex.Message + ")");
        //    }
        //}

        private void LogIt(TagInfo tag, List<int> students, string action)
        {
            int count = 0;

            StringBuilder builder = new StringBuilder("");
            builder.AppendLine("學生姓名：");

            foreach ( int each_id in students )
            {
                BriefStudentData data = StudentRelated.Student.Instance.Items[each_id + ""];
                if ( count % 5 == 0 && count > 0 )
                    builder.AppendLine("");
                builder.Append("　" + data.Name + "(" + data.StudentNumber + ")");
                count++;
            }

            builder.AppendLine();
            builder.AppendLine();
            builder.Append(string.Format("{0}類別「{1}」", action, string.IsNullOrEmpty(tag.Prefix) ? tag.Name : tag.Prefix + ":" + tag.Name));

            CurrentUser.Instance.AppLog.Write(EntityType.Student, action + "學生類別", tag.Identity + "", builder.ToString(), "學生類別", "");
        }

        #region Process Override
        public override string ProcessTabName
        {
            get
            {
                return "學生";
            }
        }
        #endregion
    }
}