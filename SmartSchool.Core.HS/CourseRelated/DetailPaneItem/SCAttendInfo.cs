using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.StudentRelated;
using FISCA.DSAUtil;
using SmartSchool.Feature.Course;
using System.Xml;
using DevComponents.DotNetBar;
using SmartSchool.StudentRelated.Validate;
using SmartSchool.Common.Validate;
using SmartSchool.ApplicationLog;
using SmartSchool.AccessControl;
using SmartSchool.ExceptionHandler;
using FISCA.Data;

namespace SmartSchool.CourseRelated.DetailPaneItem
{
    [FeatureCode("Content0210")]
    internal partial class SCAttendInfo : PalmerwormItem
    {
        private List<AttendInfo> _delete_list;
        private Dictionary<string, Dictionary<string, string>> oldLogValueDict = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, Dictionary<string, string>> newLogValueDict = new Dictionary<string, Dictionary<string, string>>();
        private DSXmlHelper _current_response;

        public SCAttendInfo()
        {
            InitializeComponent();

            Title = "修課學生";

            SmartSchool.Broadcaster.Events.Items["課程/學生修課"].Handler += new EventHandler<SmartSchool.Broadcaster.EventArguments>(SCAttendInfo_Handler);

            if (Attribute.IsDefined(GetType(), typeof(FeatureCodeAttribute)))
            {
                try
                {
                    if (!CurrentUser.Acl[GetType()].Editable)
                    {
                        btnAdd.Enabled = false;
                        btnRemove.Enabled = false;
                        contextMenuBar1.SetContextMenuEx(lvStudents, null);
                    }
                }
                catch (Exception) { }
            }
        }

        public override object Clone()
        {
            return new SCAttendInfo();
        }
        private void SCAttendInfo_Handler(object sender, SmartSchool.Broadcaster.EventArguments e)
        {

            foreach (CourseInformation info in e.Items)
            {
                if (info.Identity.ToString() == RunningID)
                {
                    LoadContent(RunningID);
                    break;
                }
            }
        }

        protected override object OnBackgroundWorkerWorking()
        {
            try
            {

                List<AttendInfo> attends = new List<AttendInfo>();


                // --- 舊寫法
                //DSResponse rsp = QueryCourse.GetSCAttend(RunningID);
                //_current_response = rsp.GetContent();

                //foreach (XmlElement each in rsp.GetContent().GetElements("Student"))                                  
                //    attends.Add(new AttendInfo(each));
                // --- 舊寫法


                // 取得學生修課紀錄
                QueryHelper qhScattend = new QueryHelper();
                string qryScattend = "SELECT sc_attend.id AS sc_attend_id" +
                    ",student.name AS student_name" +
                    ",(CASE sc_attend.required_by  WHEN 1 THEN '部訂' WHEN 2 THEN '校訂' ELSE  '' END) AS required_by" +
                    ",(CASE sc_attend.is_required  WHEN '1' THEN '必' WHEN '0' THEN '選' ELSE  '' END) AS is_required" +
                    ",class.class_name" +
                    ",student.student_number" +
                    ",student.seat_no" +
                    ",student.id AS student_id" +
                    ",class.grade_year" +
                    ",sc_attend.passing_standard" +
                    ",sc_attend.makeup_standard" +
                    ",sc_attend.remark" +
                    ",sc_attend.designate_final_score" +
                    ",sc_attend.score" +
                    ",sc_attend.subject_code " +
                    "FROM " +
                    "course INNER JOIN sc_attend " +
                    "ON course.id = sc_attend.ref_course_id " +
                    "INNER JOIN student " +
                    "ON sc_attend.ref_student_id = student.id " +
                    "LEFT JOIN class " +
                    "ON student.ref_class_id = class.id " +
                    "WHERE course.id = " + RunningID + " " +
                    "ORDER BY class.class_name,student.seat_no,student.student_number;";

                DataTable dtScattend = qhScattend.Select(qryScattend);

                foreach (DataRow dr in dtScattend.Rows)
                {
                    attends.Add(new AttendInfo(dr));
                }

                return attends;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        protected override void OnBackgroundWorkerCompleted(object result)
        {
            if (!(result is Exception))
            {
                List<AttendInfo> attends = result as List<AttendInfo>;
                _delete_list = new List<AttendInfo>();

                oldLogValueDict.Clear();
                newLogValueDict.Clear();

                foreach (AttendInfo ai in attends)
                {
                    if (!oldLogValueDict.ContainsKey(ai.RefStudentID))
                    {
                        Dictionary<string, string> value = new Dictionary<string, string>();
                        value.Add("必選修", ai.IsRequired);
                        value.Add("校部訂", ai.RequiredBy);
                        if (ai.PassingStandard == null)
                            value.Add("及格標準", "");
                        else
                            value.Add("及格標準", ai.PassingStandard);

                        if (ai.MakeupStandard == null)
                            value.Add("補考標準", "");
                        else
                            value.Add("補考標準", ai.MakeupStandard);

                        oldLogValueDict.Add(ai.RefStudentID, value);
                    }
                }

                lvStudents.Items.Clear();
                lvStudents.Items.AddRange(attends.ToArray());
                label2.Text = lvStudents.Items.Count.ToString();
                _valueManager.AddValue("IsDirty", "False");
                SaveButtonVisible = false;
            }
            else
                Enabled = false;
        }

        private bool RemoveSelected()
        {
            DSResponse rsp = null;
            try
            {
                rsp = QueryCourse.GetSECTake(RunningID);
            }
            catch (Exception ex)
            {
                CurrentUser user = CurrentUser.Instance;
                BugReporter.ReportException(user.SystemName, user.SystemVersion, ex, false);
                MsgBox.Show("移除學生修課記錄失敗：" + ex.Message, Application.ProductName);
                return false;
            }

            List<AttendInfo> cannot = new List<AttendInfo>();
            List<AttendInfo> can = new List<AttendInfo>();
            int index = lvStudents.Items.Count - 1;
            for (; index >= 0; index--)
            {
                AttendInfo each = lvStudents.Items[index] as AttendInfo;
                if (each.Selected)
                {
                    if (rsp.GetContent().GetElement("Score/AttendID[.='" + each.Identity + "']") == null)
                        can.Add(each);
                    else
                        cannot.Add(each);
                }
            }

            if (cannot.Count > 0)
            {
                string names = string.Empty;
                foreach (AttendInfo each in cannot)
                    names += string.Format("{0,-12}  {1}", each.Class, each.StudentName + "\n");
                MsgBox.Show("下列學生已經有評量成績，請先刪除評量成績再移除學生修課記錄。\n\n" + names, Application.ProductName);
                return false;
            }
            else
            {
                _delete_list.AddRange(can);
                foreach (AttendInfo each in can)
                    each.Remove();
                return true;
            }
        }

        private void ChangeSelectedItemsRequiredBy(string value)
        {
            if (lvStudents.SelectedItems.Count > 0)
                OnValueChanged("IsDirty", "True");

            foreach (ListViewItem each in lvStudents.SelectedItems)
            {
                AttendInfo info = each as AttendInfo;
                info.RequiredBy = value;
            }
        }

        private void ChangeSelectedItemsPassingStandard()
        {

            DataTable dtSc = new DataTable();
            DataColumn dc1 = new DataColumn();
            dc1.ColumnName = "sc_attend_id";
            dc1.Caption = "sc_attend_id";
            dc1.ReadOnly = true;
            dtSc.Columns.Add(dc1);
            DataColumn dc2 = new DataColumn();
            dc2.ColumnName = "class_name";
            dc2.Caption = "班級";
            dc2.ReadOnly = true;
            dtSc.Columns.Add(dc2);
            DataColumn dc3 = new DataColumn();
            dc3.ColumnName = "seat_no";
            dc3.Caption = "座號";
            dc3.ReadOnly = true;
            dtSc.Columns.Add(dc3);
            DataColumn dc4 = new DataColumn();
            dc4.ColumnName = "student_name";
            dc4.Caption = "姓名";
            dc4.ReadOnly = true;
            dtSc.Columns.Add(dc4);
            DataColumn dc5 = new DataColumn();
            dc5.ColumnName = "passing_standard";
            dc5.Caption = "及格標準";
            dtSc.Columns.Add(dc5);
            DataColumn dc6 = new DataColumn();
            dc6.ColumnName = "makeup_standard";
            dc6.Caption = "補考標準";
            dtSc.Columns.Add(dc6);
            DataColumn dc7 = new DataColumn();
            dc7.ColumnName = "subject_code";
            dc7.Caption = "科目代碼";
            dc7.ReadOnly = true;
            dtSc.Columns.Add(dc7);

            foreach (ListViewItem each in lvStudents.SelectedItems)
            {
                AttendInfo info = each as AttendInfo;
                //                info.RequiredBy = value;
                DataRow dr = dtSc.NewRow();
                dr["sc_attend_id"] = info.Identity;
                dr["class_name"] = info.Class;
                dr["seat_no"] = info.SeatNumber;
                dr["student_name"] = info.StudentName;
                dr["passing_standard"] = info.PassingStandard;
                dr["makeup_standard"] = info.MakeupStandard;
                dr["subject_code"] = info.SubjectCode;
                dtSc.Rows.Add(dr);
            }

            editPassMakupScoreForm pmsf = new editPassMakupScoreForm();
            pmsf.SetAttendInfoData(dtSc);
            if (pmsf.ShowDialog() == DialogResult.OK)
            {
                OnValueChanged("IsDirty", "True");

                DataTable newDt = pmsf.GetAttendInfoData();
                // 處理及格與補考
                foreach (DataRow dr in newDt.Rows)
                {
                    int sc_id = int.Parse(dr["sc_attend_id"].ToString());
                    foreach (ListViewItem each in lvStudents.SelectedItems)
                    {
                        AttendInfo info = each as AttendInfo;
                        if (sc_id == info.Identity)
                        {
                            info.PassingStandard = dr["passing_standard"].ToString();
                            info.MakeupStandard = dr["makeup_standard"].ToString();
                            info.SetIsDirty(true);
                        }

                    }
                }
            }

        }

        private void ChangeSelectedItemsRequired(string value)
        {
            if (lvStudents.SelectedItems.Count > 0)
                OnValueChanged("IsDirty", "True");

            foreach (ListViewItem each in lvStudents.SelectedItems)
            {
                AttendInfo info = each as AttendInfo;
                info.IsRequired = value;
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lvStudents.SelectedItems.Count > 0)
            {
                if (RemoveSelected())
                    OnValueChanged("IsDirty", "True");

                try
                {
                    lvStudents.Select();
                }
                catch { }
            }
            label2.Text = lvStudents.Items.Count.ToString();
        }

        private void lvStudents_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (lvStudents.FocusedItem == null) return;

            if (Control.ModifierKeys == Keys.Control && e.Item.Selected)
            {
                e.Item.Selected = false;
            }
        }

        private void lvStudents_DoubleClick(object sender, EventArgs e)
        {
            if (lvStudents.FocusedItem != null)
            {
                AttendInfo info = lvStudents.FocusedItem as AttendInfo;
                Student.Instance.ShowDetail(info.RefStudentID.ToString());
            }
        }

        private void btnAdd_PopupOpen(object sender, EventArgs e)
        {
            //依學生的「待處理」清單建立功能表項目。
            CreateStudentMenuItem();
            //同步功能表狀態，將已存在於 ListView 中的項目  Disable。
            SyncStudentMenuItemStatus();
        }

        private void CreateStudentMenuItem()
        {
            btnAdd.SubItems.Clear();

            if (Student.Instance.TempStudent.Count == 0)
            {
                LabelItem item = new LabelItem("No", "沒有任何學生在待處理");
                btnAdd.SubItems.Add(item);
                return;
            }

            foreach (BriefStudentData each in Student.Instance.TempStudent)
            {
                ButtonItem item = new ButtonItem(each.ID, each.Name + " (" + each.ClassName + ")");
                item.Tooltip = "班級：" + each.ClassName + "\n座號：" + each.SeatNo + "\n學號：" + each.StudentNumber;
                item.Tag = each;
                item.Click += new EventHandler(AttendStudent_Click);

                btnAdd.SubItems.Add(item);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            CreateStudentMenuItem();
            SyncStudentMenuItemStatus();

            //#region 驗證資料
            //AbstractValidateStudent validate = new ValidateBasic(new ValidateGradeYear(), new ValidateGraduationPlan());
            //ErrorViewer viewer = new ErrorViewer();
            //viewer.Text = "學生資料錯誤無法加入修課";
            //bool pass = true;
            //foreach (object obj in btnAdd.SubItems)
            //{
            //    ButtonItem each = obj as ButtonItem;
            //    if (each != null && each.Enabled)
            //    {
            //        BriefStudentData student = each.Tag as BriefStudentData;
            //        if (student != null)
            //        {
            //            //驗證單筆學生資料
            //            pass &= validate.Validate(student, viewer);
            //        }
            //    }
            //}
            //if (!pass)
            //{
            //    viewer.Show();
            //    return;
            //}
            //#endregion


            foreach (object obj in btnAdd.SubItems)
            {
                ButtonItem each = obj as ButtonItem;
                if (each != null && each.Enabled)
                {
                    BriefStudentData student = each.Tag as BriefStudentData;
                    if (student != null)
                    {
                        AddAddend(student);
                    }
                }
            }
            SyncStudentMenuItemStatus();
        }

        private void SyncStudentMenuItemStatus()
        {
            Dictionary<string, ButtonItem> _students = new Dictionary<string, ButtonItem>();
            foreach (object obj in btnAdd.SubItems)
            {
                ButtonItem each = obj as ButtonItem;
                if (each == null) continue;

                BriefStudentData stu = each.Tag as BriefStudentData;

                if (stu != null)
                {
                    if (!stu.IsNormal)
                    {
                        each.Enabled = false;
                        each.Tooltip = "需為在校生才能加入課程。";
                    }
                    else
                        _students.Add(stu.ID, each);
                }
            }
            foreach (ListViewItem each in lvStudents.Items)
            {
                AttendInfo info = each as AttendInfo;
                if (_students.ContainsKey(info.RefStudentID.ToString()))
                {
                    _students[info.RefStudentID.ToString()].Enabled = false;
                    _students[info.RefStudentID.ToString()].Tooltip = "此學生已在修課清單中";
                }
            }
        }

        private void AttendStudent_Click(object sender, EventArgs e)
        {
            BriefStudentData student = (sender as ButtonItem).Tag as BriefStudentData;
            //ErrorViewer viewer = new ErrorViewer();
            //viewer.Text = "學生資料錯誤無法加入修課";
            if (student != null)
            {
                ////驗證單筆學生資料
                //AbstractValidateStudent validate = new ValidateBasic(new ValidateGradeYear(), new ValidateGraduationPlan());
                //if (!validate.Validate(student, viewer))
                //{
                //    viewer.Show();
                //}
                //else
                AddAddend(student);
            }
        }

        private void AddAddend(BriefStudentData student)
        {
            AttendInfo newItem = new AttendInfo(student);
            CourseInformation info = Course.Instance[RunningID];
            //GraduationPlanSubject subject = student.GraduationPlanInfo.GetSubjectInfo(
            //    info.Subject,
            //    info.SubjectLevel
            //    );

            //if (subject.Required == "必修")
            //    newItem.IsRequired = "必";
            //if (subject.Required == "選修")
            //    newItem.IsRequired = "選";
            //newItem.RequiredBy = subject.RequiredBy;

            lvStudents.Items.Add(newItem);
            newItem.EnsureVisible();
            OnValueChanged("IsDirty", "True");
            label2.Text = lvStudents.Items.Count.ToString();
        }

        private void chkRequired_Click(object sender, EventArgs e)
        {
            ChangeSelectedItemsRequired("必");
        }

        private void chkUnRequired_Click(object sender, EventArgs e)
        {
            ChangeSelectedItemsRequired("選");
        }

        private void buttonItem4_Click(object sender, EventArgs e)
        {
            ChangeSelectedItemsRequired("");
        }

        private void buttonItem2_Click(object sender, EventArgs e)
        {
            ChangeSelectedItemsRequiredBy("校訂");
        }

        private void buttonItem3_Click(object sender, EventArgs e)
        {
            ChangeSelectedItemsRequiredBy("部訂");
        }

        private void buttonItem5_Click(object sender, EventArgs e)
        {
            ChangeSelectedItemsRequiredBy("");
        }

        public override void Save()
        {
            List<AttendInfo> insertList = new List<AttendInfo>();     //新增清單
            List<AttendInfo> updateList = new List<AttendInfo>();  //更新清單
            List<AttendInfo> deleteList = new List<AttendInfo>();    //刪除清單

            GetList(insertList, updateList, deleteList);


            try
            {
                if (insertList.Count > 0)
                {
                    List<string> insertSc_atttendList = new List<string>();
                    foreach (AttendInfo ai in insertList)
                    {
                        string is_required = "null", required_by = "null", grade_year = "null", passing_standard = "null", makeup_standard = "null";

                        if (ai.IsRequired != "")
                        {
                            if (ai.IsRequired == "必")
                                is_required = "'1'";
                            else
                                is_required = "'0'";
                        }
                        if (ai.RequiredBy != "")
                        {
                            if (ai.RequiredBy == "部訂")
                                required_by = "1";

                            if (ai.RequiredBy == "校訂")
                                required_by = "2";

                        }
                        if (ai.GradeYear != "")
                            grade_year = ai.GradeYear;

                        if (ai.PassingStandard != null && ai.PassingStandard != "")
                            passing_standard = ai.PassingStandard;

                        if (ai.MakeupStandard != null && ai.MakeupStandard != "")
                            makeup_standard = ai.MakeupStandard;

                        string cmd = "INSERT INTO " +
                            "sc_attend(ref_student_id" +
                            ",ref_course_id" +
                            ",is_required" +
                            ",required_by" +
                            ",grade_year" +
                            ",passing_standard" +
                            ",makeup_standard) " +
                            "VALUES(" + ai.RefStudentID + "" +
                            "," + RunningID + "" +
                            "," + is_required +
                            "," + required_by + "" +
                            "," + grade_year + "" +
                            "," + passing_standard + "" +
                            "," + makeup_standard + ");";
                        insertSc_atttendList.Add(cmd);
                    }

                    if (insertSc_atttendList.Count > 0)
                    {
                        K12.Data.UpdateHelper insertUh = new K12.Data.UpdateHelper();
                        insertUh.Execute(insertSc_atttendList);
                    }

                }

                if (updateList.Count > 0)
                {
                    newLogValueDict.Clear();
                    // 處理更新log
                    foreach (AttendInfo ai in updateList)
                    {
                        if (!newLogValueDict.ContainsKey(ai.RefStudentID))
                        {
                            Dictionary<string, string> value = new Dictionary<string, string>();
                            value.Add("必選修", ai.IsRequired);
                            value.Add("校部訂", ai.RequiredBy);
                            if (ai.PassingStandard == null)
                                value.Add("及格標準", "");
                            else
                                value.Add("及格標準", ai.PassingStandard);

                            if (ai.MakeupStandard == null)
                                value.Add("補考標準", "");
                            else
                                value.Add("補考標準", ai.MakeupStandard);

                            newLogValueDict.Add(ai.RefStudentID, value);
                        }
                    }

                    List<string> updateSc_atttendList = new List<string>();
                    foreach (AttendInfo ai in updateList)
                    {
                        string is_required = "null", required_by = "null", grade_year = "null", passing_standard = "null", makeup_standard = "null";

                        if (ai.IsRequired != "")
                        {
                            if (ai.IsRequired == "必")
                                is_required = "'1'";
                            else
                                is_required = "'0'";
                        }
                        if (ai.RequiredBy != "")
                        {
                            if (ai.RequiredBy == "部訂")
                                required_by = "1";

                            if (ai.RequiredBy == "校訂")
                                required_by = "2";

                        }
                        if (ai.GradeYear != "")
                            grade_year = ai.GradeYear;

                        if (ai.PassingStandard != "")
                            passing_standard = ai.PassingStandard;

                        if (ai.MakeupStandard != "")
                            makeup_standard = ai.MakeupStandard;

                        string cmd = "UPDATE " +
                            "sc_attend " +
                            "SET " +
                            "ref_student_id = " + ai.RefStudentID +
                            ",ref_course_id = " + RunningID +
                            ",is_required = " + is_required +
                            ",required_by = " + required_by +
                            ",grade_year = " + grade_year +
                            ",passing_standard = " + passing_standard +
                            ",makeup_standard = " + makeup_standard +
                            " WHERE id = " + ai.Identity;

                        updateSc_atttendList.Add(cmd);
                    }

                    if (updateSc_atttendList.Count > 0)
                    {
                        K12.Data.UpdateHelper updateUh = new K12.Data.UpdateHelper();
                        updateUh.Execute(updateSc_atttendList);
                    }

                }

                if (deleteList.Count > 0)
                {
                    List<int> sc_attend_idList = new List<int>();
                    foreach (AttendInfo ai in deleteList)
                        sc_attend_idList.Add(ai.Identity);
                    if (sc_attend_idList.Count > 0)
                    {
                        string delCmd = "DELETE FROM sc_attend WHERE id IN(" + string.Join(",", sc_attend_idList.ToArray()) + ");";
                        K12.Data.UpdateHelper delUh = new K12.Data.UpdateHelper();
                        delUh.Execute(delCmd);
                    }

                }


                // 舊寫法 ---
                //DSXmlHelper helper = new DSXmlHelper("InsertSCAttend");
                //foreach (AttendInfo each in insertList)
                //{
                //    XmlElement attend = helper.AddElement("Attend");
                //    DSXmlHelper.AppendChild(attend, "<RefStudentID>" + each.RefStudentID.ToString() + "</RefStudentID>");
                //    DSXmlHelper.AppendChild(attend, "<RefCourseID>" + RunningID + "</RefCourseID>");
                //    DSXmlHelper.AppendChild(attend, "<IsRequired>" + each.IsRequired + "</IsRequired>");
                //    DSXmlHelper.AppendChild(attend, "<RequiredBy>" + each.RequiredBy + "</RequiredBy>");
                //    //DSXmlHelper.AppendChild(attend, "<GradeYear>" + each.GradeYear + "</GradeYear>");
                //    helper.AddElement(".", attend);
                //}
                //if (insertList.Count > 0)
                //    AddCourse.AttendCourse(helper);

                //helper = new DSXmlHelper("UpdateSCAttend");
                //foreach (AttendInfo each in updateList)
                //{
                //    XmlElement attend = helper.AddElement("Attend");
                //    DSXmlHelper.AppendChild(attend, "<ID>" + each.Identity + "</ID>");
                //    DSXmlHelper.AppendChild(attend, "<IsRequired>" + each.IsRequired + "</IsRequired>");
                //    DSXmlHelper.AppendChild(attend, "<RequiredBy>" + each.RequiredBy + "</RequiredBy>");

                //    helper.AddElement(".", attend);
                //}
                //if (updateList.Count > 0)
                //    EditCourse.UpdateAttend(helper);

                //helper = new DSXmlHelper("DeleteSCAttendRequest");
                //foreach (AttendInfo each in deleteList)
                //{
                //    XmlElement attend = helper.AddElement("Attend");
                //    DSXmlHelper.AppendChild(attend, "<ID>" + each.Identity + "</ID>");

                //    helper.AddElement(".", attend);
                //}
                //if (deleteList.Count > 0)
                //    EditCourse.DeleteAttend(helper);
                // 舊寫法 ---




                #region Log

                StringBuilder desc = new StringBuilder("");
                List<string> desc_msgList = new List<string>();
                desc.AppendLine("課程名稱：" + ((CourseInformation)Course.Instance.Items[RunningID]).CourseName + " ");
                if (insertList.Count > 0)
                    desc.AppendLine("加入修課學生：");
                foreach (AttendInfo info in insertList)
                    desc.AppendLine("- " + info.Class + " " + info.StudentNumber + " " + info.StudentName + " ");
                if (deleteList.Count > 0)
                    desc.AppendLine("移除課程修課學生：");
                foreach (AttendInfo info in deleteList)
                    desc.AppendLine("- " + info.Class + " " + info.StudentNumber + " " + info.StudentName + " ");
                if (updateList.Count > 0)
                    desc.AppendLine("課程修課學生修課資訊調整：");
                foreach (AttendInfo info in updateList)
                {
                    desc_msgList.Clear();
                    if (newLogValueDict.ContainsKey(info.RefStudentID) && oldLogValueDict.ContainsKey(info.RefStudentID))
                    {
                        foreach (string key in newLogValueDict[info.RefStudentID].Keys)
                        {
                            if (newLogValueDict[info.RefStudentID][key] != oldLogValueDict[info.RefStudentID][key])
                            {
                                string msg = "欄位「" + key + "」由「" + oldLogValueDict[info.RefStudentID][key] + "」變更為「" + newLogValueDict[info.RefStudentID][key] + "」";
                                desc_msgList.Add(msg);
                            }
                        }
                    }

                    desc.AppendLine("- " + info.Class + " " + info.StudentNumber + " " + info.StudentName + "  " + string.Join(",", desc_msgList.ToArray()));
                }

                CurrentUser.Instance.AppLog.Write(EntityType.Course, "修改課程修課學生", RunningID, desc.ToString(), "課程", "");

                #endregion

                LoadContent(RunningID);
            }
            catch (Exception ex)
            {
                MsgBox.Show("儲存失敗：" + ex.Message);
            }

        }

        private void GetList(List<AttendInfo> insertList, List<AttendInfo> updateList, List<AttendInfo> deleteList)
        {
            foreach (ListViewItem each in lvStudents.Items)
            {
                AttendInfo info = each as AttendInfo;
                if (info.Identity == -1) //新增
                    insertList.Add(info);
                else if (info.IsDirty) //修改
                {
                    updateList.Add(info);
                }
            }

            foreach (AttendInfo each in _delete_list)
            {
                if (each.Identity != -1) //刪除
                {
                    deleteList.Add(each);
                }
            }
        }

        #region AttendInfo
        private class AttendInfo : ListViewItem
        {
            private int _identity;
            private string _name;
            private string _is_required;
            private string _class;
            private string _snum;
            private string _requiredby;
            private string _seatno;
            private string _ref_studentid;
            private bool _is_dirty;
            private string _gradeyear;

            private string _passing_standard;
            private string _makeup_standard;
            private string _subject_code;
            //remark
            //designate_final_score
            //score
            //subject_code

            public AttendInfo(DataRow attend)
            {
                int sc_attend_id = -1;
                int.TryParse(attend["sc_attend_id"].ToString(), out sc_attend_id);

                _identity = sc_attend_id;
                _name = attend["student_name"].ToString();
                _is_required = attend["is_required"].ToString();
                _requiredby = attend["required_by"].ToString();
                _class = attend["class_name"].ToString();
                _snum = attend["student_number"].ToString();
                _seatno = attend["seat_no"].ToString();
                _ref_studentid = attend["student_id"].ToString();
                _gradeyear = attend["grade_year"].ToString();
                _passing_standard = attend["passing_standard"].ToString();
                _makeup_standard = attend["makeup_standard"].ToString();
                _subject_code = attend["subject_code"].ToString();
                Text = Class;
                CreateSubItem();

                _is_dirty = false;
            }


            public AttendInfo(XmlElement attend)
            {
                _identity = GetIntValue(attend, "@ID");
                _name = GetStringValue(attend, "Name");
                _is_required = GetStringValue(attend, "IsRequired");
                _requiredby = GetStringValue(attend, "RequiredBy");
                _class = GetStringValue(attend, "ClassName");
                _snum = GetStringValue(attend, "StudentNumber");
                _seatno = GetStringValue(attend, "SeatNumber");
                _ref_studentid = GetStringValue(attend, "RefStudentID");
                _gradeyear = GetStringValue(attend, "GradeYear");
                Text = Class;
                CreateSubItem();

                _is_dirty = false;
            }

            public AttendInfo(BriefStudentData newStudent)
            {
                _identity = -1;
                _name = newStudent.Name;
                _is_required = "";
                _requiredby = "";
                _class = newStudent.ClassName;
                _snum = newStudent.StudentNumber;
                _seatno = newStudent.SeatNo;
                _ref_studentid = newStudent.ID;
                _gradeyear = newStudent.GradeYear;
                Text = Class;
                CreateSubItem();

                _is_dirty = false;

                foreach (ListViewSubItem each in SubItems)
                    each.ForeColor = Color.Blue;
            }

            private void CreateSubItem()
            {
                AddSubItem("SeatNumber", _seatno.ToString());
                AddSubItem("StudentName", _name);
                AddSubItem("StudentNumber", _snum);
                AddSubItem("IsRequired", _is_required);
                AddSubItem("RequiredBy", _requiredby);
                AddSubItem("PassingStandard", _passing_standard);
                AddSubItem("MakeupStandard", _makeup_standard);
                AddSubItem("SubjectCode", _subject_code);

            }

            private void AddSubItem(string name, string value)
            {
                ListViewSubItem sub = new ListViewSubItem();
                sub.Text = value;
                sub.Name = name;
                if ((name == "IsRequired" || name == "RequiredBy" || name == "PassingStandard" || name == "MakeupStandard") && value == "")
                {
                    sub.Text = "未指定";
                    sub.ForeColor = Color.LightGray;
                }

                SubItems.Add(sub);
            }

            public int Identity
            {
                get { return _identity; }
            }

            public string StudentName
            {
                get { return _name; }
            }

            public string GradeYear
            {
                get { return _gradeyear; }
            }

            public void SetIsDirty(bool value)
            {
                _is_dirty = value;
            }


            public string IsRequired
            {
                get { return _is_required; }
                set
                {
                    _is_dirty = (IsRequired != value);
                    _is_required = value;
                    if (value == "")
                        SubItems["IsRequired"].Text = "未指定";
                    else
                        SubItems["IsRequired"].Text = _is_required;

                    if (_is_dirty)
                    {
                        SubItems["IsRequired"].ForeColor = Color.Blue;
                    }
                }
            }

            public string RequiredBy
            {
                get { return _requiredby; }
                set
                {
                    _is_dirty = (_requiredby != value);
                    _requiredby = value;
                    if (value == "")
                        SubItems["RequiredBy"].Text = "未指定";
                    else
                        SubItems["RequiredBy"].Text = _requiredby;

                    if (_is_dirty)
                    {
                        SubItems["RequiredBy"].ForeColor = Color.Blue;
                    }
                }
            }

            public string Class
            {
                get { return _class; }
            }

            public string StudentNumber
            {
                get { return _snum; }
            }

            public string SeatNumber
            {
                get { return _seatno; }
            }

            public string RefStudentID
            {
                get { return _ref_studentid; }
            }

            public bool IsDirty
            {
                get { return _is_dirty; }
            }

            // 及格標準
            public string PassingStandard
            {
                get { return _passing_standard; }
                set
                {
                    _is_dirty = (_passing_standard != value);
                    _passing_standard = value;
                    if (value == "")
                        SubItems["PassingStandard"].Text = "未指定";
                    else
                        SubItems["PassingStandard"].Text = _passing_standard;

                    if (_is_dirty)
                    {
                        SubItems["PassingStandard"].ForeColor = Color.Blue;
                    }
                }
            }
            // 補考標準
            public string MakeupStandard
            {
                get { return _makeup_standard; }
                set
                {
                    _is_dirty = (_makeup_standard != value);
                    _makeup_standard = value;
                    if (value == "")
                        SubItems["MakeupStandard"].Text = "未指定";
                    else
                        SubItems["MakeupStandard"].Text = _makeup_standard;

                    if (_is_dirty)
                    {
                        SubItems["MakeupStandard"].ForeColor = Color.Blue;
                    }
                }
            }
            /// <summary>
            /// 科目代碼，ReadOnly
            /// </summary>
            public string SubjectCode
            {
                get { return _subject_code; }
            }

            private int GetIntValue(XmlElement rawData, string xpath)
            {
                XmlNode value = rawData.SelectSingleNode(xpath);

                ThrowResultNullException(xpath, value);

                if (string.IsNullOrEmpty(value.InnerText))
                    return -1;

                int result;
                if (int.TryParse(value.InnerText, out result))
                    return result;
                else
                    throw new ArgumentException("欄位資料型態不正確，XPath 路徑：" + xpath);
            }

            private string GetStringValue(XmlElement rawData, string xpath)
            {
                XmlNode value = rawData.SelectSingleNode(xpath);

                ThrowResultNullException(xpath, value);

                return value.InnerText;
            }

            private static void ThrowResultNullException(string xpath, XmlNode value)
            {
                if (value == null)
                    throw new ArgumentException("欄位資料不存在，XPath 路徑：" + xpath);
            }

        }
        #endregion

        private void SCAttendInfo_DoubleClick(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Shift)
            {
                XmlBox.ShowXml(_current_response.BaseElement);
            }
        }

        private void btnEditPMScore_Click(object sender, EventArgs e)
        {
            ChangeSelectedItemsPassingStandard();
        }
    }
}
