using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.ApplicationLog;
using SmartSchool.AccessControl;

namespace SmartSchool.StudentRelated.Palmerworm
{
    [FeatureCode("Content0130")]
    public partial class SemesterHistoryPalmerworm : PalmerwormItemBase
    {
        private string _CurrentID;

        private string _RunningID;

        private bool _Pass;

        private Dictionary<string, List<string>> _semesterValues;

        private BackgroundWorker _Loader = new BackgroundWorker();

        private LogRecorder logger = new LogRecorder();

        public override object Clone()
        {
            return new SemesterHistoryPalmerworm();
        }
        public SemesterHistoryPalmerworm()
        {
            InitializeComponent();
            _Loader.DoWork += new DoWorkEventHandler(_Loader_DoWork);
            _Loader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_Loader_RunWorkerCompleted);
            Application.Idle += new EventHandler(Application_Idle);
        }

        void Application_Idle(object sender, EventArgs e)
        {
            bool hasChanged = false;
            #region 驗資料變更
            Dictionary<string, List<string>> semesterValues = new Dictionary<string, List<string>>();
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (!row.IsNewRow)
                {
                    // SchoolYear,Semester,GradeYear,ClassName,SeatNo,SchoolDayCount,Teacher,DeptName
                    string semester = ("" + row.Cells[colSchoolYear.Index].Value).Trim() + "_" + ("" + row.Cells[colSemester.Index].Value).Trim();
                    if (!semesterValues.ContainsKey(semester))
                    {
                        string str = ("" + row.Cells[colGradeYear.Index].Value + "_" + row.Cells[colClassName.Index].Value + "_" + row.Cells[colSeatNo.Index].Value + "_" + row.Cells[colSchoolDayCount.Index].Value + "_" + row.Cells[colTeacher.Index].Value + "_" + row.Cells[colDeptName.Index].Value+ "_" + row.Cells[colCourseGroupCode.Index].Value).Trim();
                        semesterValues.Add(semester, new List<string>(new string[] { str }));
                    }
                    else
                        _Pass &= false;
                }
            }
            hasChanged = (_semesterValues != null && semesterValues.Count != _semesterValues.Count);
            if (!hasChanged)
            {
                foreach (string key in semesterValues.Keys)
                {
                    if (!_semesterValues.ContainsKey(key))
                    {
                        hasChanged = true;
                        break;
                    }
                    else
                    {
                        for (int i = 0; i < semesterValues[key].Count; i++)
                        {
                            if (semesterValues[key][i] != _semesterValues[key][i])
                            {
                                hasChanged = true;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion
            SaveButtonVisible = _Pass & hasChanged;
            CancelButtonVisible = hasChanged;
        }

        void _Loader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.IsDisposed)
                return;
            if (_RunningID != _CurrentID)
            {
                _RunningID = _CurrentID;
                _Loader.RunWorkerAsync(_RunningID);
                return;
            }
            this.WaitingPicVisible = false;
            XmlElement[] elements = (XmlElement[])e.Result;
            _semesterValues = new Dictionary<string, List<string>>();
            foreach (XmlElement element in elements)
            {
                string schoolYear, semester, gradeyear, ClassName, SeatNo, SchoolDayCount, Teacher, DeptName, CourseGroupCode;
                schoolYear = element.GetAttribute("SchoolYear");
                semester = element.GetAttribute("Semester");
                gradeyear = element.GetAttribute("GradeYear");
                ClassName = element.GetAttribute("ClassName");
                SeatNo = element.GetAttribute("SeatNo");
                SchoolDayCount = element.GetAttribute("SchoolDayCount");
                Teacher = element.GetAttribute("Teacher");
                DeptName = element.GetAttribute("DeptName");
                CourseGroupCode = element.GetAttribute("CourseGroupCode");

                int row = dataGridViewX1.Rows.Add();
                dataGridViewX1.Rows[row].Cells[colSchoolYear.Index].Value = schoolYear;
                dataGridViewX1.Rows[row].Cells[colSemester.Index].Value = semester;
                dataGridViewX1.Rows[row].Cells[colGradeYear.Index].Value = gradeyear;
                dataGridViewX1.Rows[row].Cells[colClassName.Index].Value = ClassName;
                dataGridViewX1.Rows[row].Cells[colSeatNo.Index].Value = SeatNo;
                dataGridViewX1.Rows[row].Cells[colSchoolDayCount.Index].Value = SchoolDayCount;
                dataGridViewX1.Rows[row].Cells[colTeacher.Index].Value = Teacher;
                dataGridViewX1.Rows[row].Cells[colDeptName.Index].Value = DeptName;
                dataGridViewX1.Rows[row].Cells[colCourseGroupCode.Index].Value = CourseGroupCode;


                string str = gradeyear + "_" + ClassName + "_" + SeatNo + "_" + SchoolDayCount + "_" + Teacher + "_" + DeptName+"_"+ CourseGroupCode;
                _semesterValues.Add(schoolYear.Trim() + "_" + semester.Trim(), new List<string>(new string[] { str.Trim() }));

                string keyIdx = schoolYear.Trim() + "_" + semester.Trim() + "_";
                logger.AddBefore(keyIdx + "GradeYear", gradeyear);
                logger.AddBefore(keyIdx + "ClassName", ClassName);
                logger.AddBefore(keyIdx + "SeatNo", SeatNo);
                logger.AddBefore(keyIdx + "SchoolDayCount", SchoolDayCount);
                logger.AddBefore(keyIdx + "Teacher", Teacher);
                logger.AddBefore(keyIdx + "DeptName", DeptName);
                logger.AddBefore(keyIdx + "CourseGroupCode", CourseGroupCode);

            }
            dataGridViewX1.EndEdit();
            CheckAll();
        }

        void _Loader_DoWork(object sender, DoWorkEventArgs e)
        {
            string id = "" + e.Argument;
            e.Result = Feature.QueryStudent.GetDetailList(new string[] { "ID", "SemesterHistory" }, id).GetContent().GetElements("Student[@ID='" + id + "']/SemesterHistory/History");
        }
        public override void LoadContent(string id)
        {
            _CurrentID = id;
            dataGridViewX1.EndEdit();
            if (_semesterValues != null) _semesterValues.Clear();
            dataGridViewX1.Rows.Clear();
            SaveButtonVisible = false;
            CancelButtonVisible = false;
            if (!_Loader.IsBusy)
            {
                _RunningID = _CurrentID;
                _Loader.RunWorkerAsync(_RunningID);
                WaitingPicVisible = true;
            }
        }
        public override void Save()
        {
            dataGridViewX1.EndEdit();
            if (CheckAll())
            {
                DSXmlHelper helper = new DSXmlHelper("UpdateStudentList");
                helper.AddElement("Student");
                helper.AddElement("Student", "Field");
                helper.AddElement("Student/Field", "SemesterHistory");
                helper.AddElement("Student", "Condition");
                helper.AddElement("Student/Condition", "ID", _CurrentID);
                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        XmlElement element = helper.AddElement("Student/Field/SemesterHistory", "History");
                        element.SetAttribute("SchoolYear", "" + row.Cells[colSchoolYear.Index].Value);
                        element.SetAttribute("Semester", "" + row.Cells[colSemester.Index].Value);
                        element.SetAttribute("GradeYear", "" + row.Cells[colGradeYear.Index].Value);
                        element.SetAttribute("ClassName", "" + row.Cells[colClassName.Index].Value);
                        element.SetAttribute("SeatNo", "" + row.Cells[colSeatNo.Index].Value);
                        element.SetAttribute("SchoolDayCount", "" + row.Cells[colSchoolDayCount.Index].Value);
                        element.SetAttribute("Teacher", "" + row.Cells[colTeacher.Index].Value);
                        element.SetAttribute("DeptName", "" + row.Cells[colDeptName.Index].Value);
                        element.SetAttribute("CourseGroupCode", "" + row.Cells[colCourseGroupCode.Index].Value);

                        string keyIdx = "";
                        if (row.Cells[colSchoolYear.Index].Value != null && row.Cells[colSemester.Index].Value != null)
                            keyIdx = row.Cells[colSchoolYear.Index].Value.ToString() + "_" + row.Cells[colSemester.Index].Value.ToString() + "_";

                        if (row.Cells[colGradeYear.Index].Value != null)
                            logger.AddAfter(keyIdx + "GradeYear", row.Cells[colGradeYear.Index].Value.ToString());

                        if (row.Cells[colClassName.Index].Value != null)
                            logger.AddAfter(keyIdx + "ClassName", row.Cells[colClassName.Index].Value.ToString());

                        if (row.Cells[colSeatNo.Index].Value != null)
                            logger.AddAfter(keyIdx + "SeatNo", row.Cells[colSeatNo.Index].Value.ToString());

                        if (row.Cells[colSchoolDayCount.Index].Value != null)
                            logger.AddAfter(keyIdx + "SchoolDayCount", row.Cells[colSchoolDayCount.Index].Value.ToString());

                        if (row.Cells[colTeacher.Index].Value != null)
                            logger.AddAfter(keyIdx + "Teacher", row.Cells[colTeacher.Index].Value.ToString());

                        if (row.Cells[colDeptName.Index].Value != null)
                            logger.AddAfter(keyIdx + "DeptName", row.Cells[colDeptName.Index].Value.ToString());

                        if (row.Cells[colCourseGroupCode.Index].Value != null)
                            logger.AddAfter(keyIdx + "CourseGroupCode", row.Cells[colCourseGroupCode.Index].Value.ToString());

                    }
                }
                Feature.EditStudent.Update(new DSRequest(helper));

                #region Log

                Dictionary<string, string[]> difference = logger.GetDifference();
                StringBuilder desc = new StringBuilder("");
                desc.AppendLine("學生姓名：" + Student.Instance.Items[_CurrentID].Name + " ");

                foreach (string key in difference.Keys)
                {
                    string schoolyear = key.Split('_')[0];
                    string semester = key.Split('_')[1];
                    string keyIdx = schoolyear + "_" + semester + "_";

                    if (difference.ContainsKey(keyIdx + "GradeYear"))
                    {
                        string GradeYearBefore = difference[keyIdx + "GradeYear"][0];
                        string GradeYearAfter = difference[keyIdx + "GradeYear"][1];
                        if (!string.IsNullOrEmpty(GradeYearBefore) && !string.IsNullOrEmpty(GradeYearAfter))
                        {
                            desc.AppendLine("修改 「" + schoolyear + "」學年度 第「" + semester + "」學期 年級由「" + GradeYearBefore + "」年級變更為「" + GradeYearAfter + "」年級");
                        }
                        else if (string.IsNullOrEmpty(GradeYearBefore) && !string.IsNullOrEmpty(GradeYearAfter))
                        {
                            desc.AppendLine("新增 「" + schoolyear + "」學年度 第「" + semester + "」學期 年級為「" + GradeYearAfter + "」年級");
                        }
                        else if (!string.IsNullOrEmpty(GradeYearBefore) && string.IsNullOrEmpty(GradeYearAfter))
                        {
                            desc.AppendLine("刪除 「" + schoolyear + "」學年度 第「" + semester + "」學期「" + GradeYearBefore + "」年級");
                        }
                    }

                    if (difference.ContainsKey(keyIdx + "ClassName"))
                    {
                        string ClassNameBefore = difference[keyIdx + "ClassName"][0];
                        string ClassNameAfter = difference[keyIdx + "ClassName"][1];
                        if (!string.IsNullOrEmpty(ClassNameBefore) && !string.IsNullOrEmpty(ClassNameAfter))
                        {
                            desc.AppendLine("修改 「" + schoolyear + "」學年度 第「" + semester + "」學期 班級由「" + ClassNameBefore + "」班級變更為「" + ClassNameAfter + "」班級");
                        }
                        else if (string.IsNullOrEmpty(ClassNameBefore) && !string.IsNullOrEmpty(ClassNameAfter))
                        {
                            desc.AppendLine("新增 「" + schoolyear + "」學年度 第「" + semester + "」學期 班級為「" + ClassNameAfter + "」班級");
                        }
                        else if (!string.IsNullOrEmpty(ClassNameBefore) && string.IsNullOrEmpty(ClassNameAfter))
                        {
                            desc.AppendLine("刪除 「" + schoolyear + "」學年度 第「" + semester + "」學期「" + ClassNameBefore + "」班級");
                        }
                    }

                    if (difference.ContainsKey(keyIdx + "SeatNo"))
                    {
                        string SeatNoBefore = difference[keyIdx + "SeatNo"][0];
                        string SeatNoAfter = difference[keyIdx + "SeatNo"][1];
                        if (!string.IsNullOrEmpty(SeatNoBefore) && !string.IsNullOrEmpty(SeatNoAfter))
                        {
                            desc.AppendLine("修改 「" + schoolyear + "」學年度 第「" + semester + "」學期 座號由「" + SeatNoBefore + "」座號變更為「" + SeatNoAfter + "」座號");
                        }
                        else if (string.IsNullOrEmpty(SeatNoBefore) && !string.IsNullOrEmpty(SeatNoAfter))
                        {
                            desc.AppendLine("新增 「" + schoolyear + "」學年度 第「" + semester + "」學期 座號為「" + SeatNoAfter + "」座號");
                        }
                        else if (!string.IsNullOrEmpty(SeatNoBefore) && string.IsNullOrEmpty(SeatNoAfter))
                        {
                            desc.AppendLine("刪除 「" + schoolyear + "」學年度 第「" + semester + "」學期「" + SeatNoBefore + "」座號");
                        }
                    }

                    if (difference.ContainsKey(keyIdx + "SchoolDayCount"))
                    {
                        string SchoolDayCountBefore = difference[keyIdx + "SchoolDayCount"][0];
                        string SchoolDayCountAfter = difference[keyIdx + "SchoolDayCount"][1];
                        if (!string.IsNullOrEmpty(SchoolDayCountBefore) && !string.IsNullOrEmpty(SchoolDayCountAfter))
                        {
                            desc.AppendLine("修改 「" + schoolyear + "」學年度 第「" + semester + "」學期 上課天數由「" + SchoolDayCountBefore + "」上課天數變更為「" + SchoolDayCountAfter + "」上課天數");
                        }
                        else if (string.IsNullOrEmpty(SchoolDayCountBefore) && !string.IsNullOrEmpty(SchoolDayCountAfter))
                        {
                            desc.AppendLine("新增 「" + schoolyear + "」學年度 第「" + semester + "」學期 上課天數為「" + SchoolDayCountAfter + "」上課天數");
                        }
                        else if (!string.IsNullOrEmpty(SchoolDayCountBefore) && string.IsNullOrEmpty(SchoolDayCountAfter))
                        {
                            desc.AppendLine("刪除 「" + schoolyear + "」學年度 第「" + semester + "」學期「" + SchoolDayCountBefore + "」上課天數");
                        }
                    }

                    if (difference.ContainsKey(keyIdx + "Teacher"))
                    {
                        string TeacherBefore = difference[keyIdx + "Teacher"][0];
                        string TeacherAfter = difference[keyIdx + "Teacher"][1];
                        if (!string.IsNullOrEmpty(TeacherBefore) && !string.IsNullOrEmpty(TeacherAfter))
                        {
                            desc.AppendLine("修改 「" + schoolyear + "」學年度 第「" + semester + "」學期 班導師由「" + TeacherBefore + "」班導師變更為「" + TeacherAfter + "」班導師");
                        }
                        else if (string.IsNullOrEmpty(TeacherBefore) && !string.IsNullOrEmpty(TeacherAfter))
                        {
                            desc.AppendLine("新增 「" + schoolyear + "」學年度 第「" + semester + "」學期 班導師為「" + TeacherAfter + "」班導師");
                        }
                        else if (!string.IsNullOrEmpty(TeacherBefore) && string.IsNullOrEmpty(TeacherAfter))
                        {
                            desc.AppendLine("刪除 「" + schoolyear + "」學年度 第「" + semester + "」學期「" + TeacherBefore + "」班導師");
                        }
                    }

                    if (difference.ContainsKey(keyIdx + "DeptName"))
                    {
                        string DeptNameBefore = difference[keyIdx + "DeptName"][0];
                        string DeptNameAfter = difference[keyIdx + "DeptName"][1];
                        if (!string.IsNullOrEmpty(DeptNameBefore) && !string.IsNullOrEmpty(DeptNameAfter))
                        {
                            desc.AppendLine("修改 「" + schoolyear + "」學年度 第「" + semester + "」學期 科別由「" + DeptNameBefore + "」科別變更為「" + DeptNameAfter + "」科別");
                        }
                        else if (string.IsNullOrEmpty(DeptNameBefore) && !string.IsNullOrEmpty(DeptNameAfter))
                        {
                            desc.AppendLine("新增 「" + schoolyear + "」學年度 第「" + semester + "」學期 科別為「" + DeptNameAfter + "」科別");
                        }
                        else if (!string.IsNullOrEmpty(DeptNameBefore) && string.IsNullOrEmpty(DeptNameAfter))
                        {
                            desc.AppendLine("刪除 「" + schoolyear + "」學年度 第「" + semester + "」學期「" + DeptNameBefore + "」科別");
                        }
                    }

                    if (difference.ContainsKey(keyIdx + "CourseGroupCode"))
                    {
                        string CourseGroupCodeBefore = difference[keyIdx + "CourseGroupCode"][0];
                        string CourseGroupCodeAfter = difference[keyIdx + "CourseGroupCode"][1];
                        if (!string.IsNullOrEmpty(CourseGroupCodeBefore) && !string.IsNullOrEmpty(CourseGroupCodeAfter))
                        {
                            desc.AppendLine("修改 「" + schoolyear + "」學年度 第「" + semester + "」學期 課程群組代碼由「" + CourseGroupCodeBefore + "」課程群組代碼變更為「" + CourseGroupCodeAfter + "」課程群組代碼");
                        }
                        else if (string.IsNullOrEmpty(CourseGroupCodeBefore) && !string.IsNullOrEmpty(CourseGroupCodeAfter))
                        {
                            desc.AppendLine("新增 「" + schoolyear + "」學年度 第「" + semester + "」學期 課程群組代碼為「" + CourseGroupCodeAfter + "」課程群組代碼");
                        }
                        else if (!string.IsNullOrEmpty(CourseGroupCodeBefore) && string.IsNullOrEmpty(CourseGroupCodeAfter))
                        {
                            desc.AppendLine("刪除 「" + schoolyear + "」學年度 第「" + semester + "」學期「" + CourseGroupCodeBefore + "」課程群組代碼");
                        }
                    }

                    //「」
                }

                CurrentUser.Instance.AppLog.Write(EntityType.Student, "修改學期對照表", _CurrentID, desc.ToString(), "", "");

                #endregion

                LoadContent(_CurrentID);
            }
            else
                MsgBox.Show("學期歷程資料輸入錯誤，儲存失敗。");
        }
        public override void Undo()
        {
            LoadContent(_CurrentID);
        }

        private void dataGridViewX1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
            DataGridViewCell cell = dataGridViewX1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string message = "儲存格值：" + cell.Value + "。\n發生錯誤： " + e.Exception.Message + "。";
            if (cell.ErrorText != message)
            {
                cell.ErrorText = message;
                dataGridViewX1.UpdateCellErrorText(e.ColumnIndex, e.RowIndex);
            }
        }

        private void dataGridViewX1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            //dataGridViewX1.EndEdit();
            //CheckAll();
        }

        private bool CheckAll()
        {
            _Pass = true;
            #region 驗資料輸入正確
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (!row.IsNewRow)
                {
                    int x;
                    for (int i = 0; i < 3; i++)
                    {
                        #region 驗空白
                        if ("" + row.Cells[i].Value == "")
                        {
                            row.Cells[i].ErrorText = "不得空白";
                            _Pass &= false;
                            dataGridViewX1.UpdateCellErrorText(i, row.Index);
                        }
                        else if (row.Cells[i].ErrorText == "不得空白")
                        {
                            row.Cells[i].ErrorText = "";
                            dataGridViewX1.UpdateCellErrorText(i, row.Index);
                        }
                        #endregion
                        #region 驗數字
                        if (!int.TryParse("" + row.Cells[i].Value, out x))
                        {
                            row.Cells[i].ErrorText = "必須輸入數字";
                            _Pass &= false;
                            dataGridViewX1.UpdateCellErrorText(i, row.Index);
                        }
                        else if (row.Cells[i].ErrorText == "必須輸入數字")
                        {
                            row.Cells[i].ErrorText = "";
                            dataGridViewX1.UpdateCellErrorText(i, row.Index);
                        }
                        #endregion
                    }
                    #region 檢查學期輸入範圍
                    if (int.TryParse("" + row.Cells[1].Value, out x) && (x > 2 || x < 1))
                    {
                        row.Cells[1].ErrorText = "只允許1或2";
                        _Pass &= false;
                        dataGridViewX1.UpdateCellErrorText(1, row.Index);
                    }
                    else if (row.Cells[1].ErrorText == "只允許1或2")
                    {
                        row.Cells[1].ErrorText = "";
                        dataGridViewX1.UpdateCellErrorText(1, row.Index);
                    }
                    #endregion
                }
            }
            #endregion
            return _Pass;
        }

        private void dataGridViewX1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            dataGridViewX1.EndEdit();
            CheckAll();
        }

        private void dataGridViewX1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridViewX1.EndEdit();
        }

        private void dataGridViewX1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewX1.SelectedCells.Count == 1)
                dataGridViewX1.BeginEdit(true);
        }

        private void dataGridViewX1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataGridViewX1.EndEdit();
            CheckAll();
            dataGridViewX1.BeginEdit(false);
        }
    }

    /// <summary>
    /// 記錄 Log 用的
    /// </summary>
    class LogRecorder
    {
        Dictionary<string, string> beforeData = new Dictionary<string, string>();
        Dictionary<string, string> afterData = new Dictionary<string, string>();

        public LogRecorder()
        {
        }

        public void AddBefore(string k, string v)
        {
            if (!beforeData.ContainsKey(k))
                beforeData.Add(k, v);
            else
                beforeData[k] = v;
        }

        public void AddAfter(string k, string v)
        {
            if (!afterData.ContainsKey(k))
                afterData.Add(k, v);
            else
                afterData[k] = v;
        }

        public Dictionary<string, string[]> GetDifference()
        {
            Dictionary<string, string[]> difference = new Dictionary<string, string[]>();

            foreach (string key in beforeData.Keys)
            {
                if (afterData.ContainsKey(key) && beforeData[key] != afterData[key])
                {
                    difference.Add(key, new string[] { beforeData[key], afterData[key] });
                    afterData.Remove(key);
                }
                else if (afterData.ContainsKey(key) && beforeData[key] == afterData[key])
                {
                    afterData.Remove(key);
                }
                else
                {
                    difference.Add(key, new string[] { beforeData[key], "" });
                }
            }

            foreach (string key in afterData.Keys)
            {
                difference.Add(key, new string[] { "", afterData[key] });
            }

            beforeData.Clear();
            afterData.Clear();
            return difference;
        }
    }
}
