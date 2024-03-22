using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.Feature;
using FISCA.DSAUtil;
using System.Xml;
using DevComponents.DotNetBar;
using SmartSchool.ApplicationLog;
using SmartSchool.Feature.Course;
using SmartSchool.AccessControl;
using SmartSchool.StudentRelated;
using System.Linq;
using DevComponents.DotNetBar.Controls;
using FISCA.Data;
using SmartSchool.CourseRelated.ScoreDataGridView;
using System.Xml.Linq;

namespace SmartSchool.CourseRelated.DetailPaneItem.OtherEntity
{
    [FeatureCode("Content0110")]
    internal partial class CourseScorePalmerwormItem : PalmerwormItem
    {
        private List<int> _schoolYearList;
        private DSResponse _examList;
        private ErrorProvider _errorProvider;
        private bool _initialized;
        private bool _loading;
        private ConditionArg _currentArg;
        private DataGetter _data_getter;
        private ScoreType _process_score_type;

        // ���q���Z extension ���e
        Dictionary<string, string> sce_take_extDDict;

        // �ʦҳ]�w��T
        List<SmartSchool.DAO.ScoreValueMangInfo> ScoreValueMangInfoList;

        // �ʦҳ]�w��r��ӭ�
        Dictionary<string, string> ScoreValueMangTextDict;


        public CourseScorePalmerwormItem()
        {
            InitializeComponent();
            Title = "�ҵ{�������Z";
            _errorProvider = new ErrorProvider();
            ScoreValueMangInfoList = new List<DAO.ScoreValueMangInfo>();
            ScoreValueMangTextDict = new Dictionary<string, string>();
            sce_take_extDDict = new Dictionary<string, string>();
            _initialized = false;
            _loading = false;
        }

        #region Initialize Jobs
        protected override object OnBackgroundWorkerWorking()
        {
            _schoolYearList = new List<int>();
            foreach (var item in Course.Instance.Items)
            {
                if (!_schoolYearList.Contains(item.SchoolYear))
                    _schoolYearList.Add(item.SchoolYear);
            }
            _schoolYearList.Sort();
            _examList = SmartSchool.Feature.Course.QueryCourse.GetExamList();
            // ���o�ʦҳ]�w���
            ScoreValueMangInfoList = QueryData.GetScoreValueMangInfoList();

            // �ʦҳ]�w����            
            ScoreValueMangTextDict.Clear();
            foreach (SmartSchool.DAO.ScoreValueMangInfo info in ScoreValueMangInfoList)
            {
                if (!ScoreValueMangTextDict.ContainsKey(info.UseText))
                    ScoreValueMangTextDict.Add(info.UseText, info.UseValue);
            }
            return null;
        }

        protected override void OnBackgroundWorkerCompleted(object result)
        {
            //�N�e���W������ﶵ���m���w�]���A�C
            ResetConditionOptions();

            BindData();
        }

        private void ResetConditionOptions()
        {
            cboSchoolYear.SelectedItem = null;
            cboSchoolYear.Items.Clear();
            cboSemester.SelectedItem = null;
            cboSemester.Items.Clear();
            cboExam.SelectedItem = null;
            cboExam.Items.Clear();

            //��J�Ǧ~�סB�Ǵ�����ﶵ�C
            IninialSemesterConditions();

            //��J�էO����ﶵ�C
            InitialExamConditions();

            if (cboExam.Items.Count > 0)
                cboExam.SelectedIndex = 0;

            cboSchoolYear.Text = CurrentUser.Instance.SchoolYear.ToString();
            cboSemester.Text = CurrentUser.Instance.Semester.ToString();
            _initialized = true;
        }

        private void InitialExamConditions()
        {
            //�Q�� ExamId ���S���ơA�N��ҵ{���Z�C
            KeyValuePair<string, string> courseItem = new KeyValuePair<string, string>("CourseScore", "(�ҵ{���Z)");
            cboExam.Items.Add(courseItem);

            foreach (XmlElement element in _examList.GetContent().GetElements("Exam"))
            {
                string id = element.GetAttribute("ID");
                string name = element.SelectSingleNode("ExamName").InnerText;
                KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(id, name);
                cboExam.Items.Add(kvp);
            }
            cboExam.DisplayMember = "Value";
            cboExam.ValueMember = "Key";
        }

        private void IninialSemesterConditions()
        {
            foreach (int schoolYear in _schoolYearList)
                cboSchoolYear.Items.Add(schoolYear);

            cboSemester.Items.Add("1");
            cboSemester.Items.Add("2");
        }
        #endregion

        # region BindData Code
        private void BindData()
        {
            if (!IsValid()) return;
            if (!_initialized) return;
            if (_loading) return;
            if (cboExam.SelectedItem == null) return;

            dgvScore.EndEdit();
            dgvScore.Rows.Clear();
            dgvScore.Enabled = false;
            picWaiting.Show();

            ConditionArg arg = new ConditionArg();
            arg.Examid = ((KeyValuePair<string, string>)cboExam.SelectedItem).Key;
            arg.SchoolYear = cboSchoolYear.Text;
            arg.Semester = cboSemester.Text;
            _currentArg = arg;
            _loading = true;

            if (arg.Examid == "CourseScore")
                _process_score_type = ScoreType.Course;
            else
                _process_score_type = ScoreType.Exam;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync(arg);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _data_getter = new DataGetter(RunningID, _process_score_type);
            e.Result = _data_getter.Evaluate(e.Argument as ConditionArg);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // �p�G���J�������ˬd�Ǧ~�׾Ǵ��ҸդT�ذѼƤw�g�M��ɸ��J�ɩҳ]�w�����P����, ���s���o���
            if (_currentArg.SchoolYear != cboSchoolYear.Text
                || _currentArg.Semester != cboSemester.Text
                || _currentArg.Examid != ((KeyValuePair<string, string>)cboExam.SelectedItem).Key)
            {
                _loading = false;
                BindData();
                return;
            }
            // �p�G��������
            if (e.Error != null)
            {
                MsgBox.Show(e.Error.Message);
                return;
            }

            dgvScore.Enabled = true;
            _valueManager.ResetValues();
            dgvScore.EndEdit();
            dgvScore.Rows.Clear();
            if (dgvScore.Columns.Count > 0)
            {
                RowInfoCollection collection = e.Result as RowInfoCollection;
                foreach (RowInfo info in collection.Items)
                {
                    int rowIndex = dgvScore.Rows.Add();
                    DataGridViewRow row = dgvScore.Rows[rowIndex];
                    row.Cells[colCourse.Name].Value = info.CourseName;
                    row.Cells[colCourse.Name].Tag = info.CourseID;

                    string strValue = info.Score;
                    if (info.Score == "-1" || info.Score == "-2")
                    {
                        // Ū���ʦҹ��
                        if (_data_getter.sce_take_extDDict.ContainsKey(info.SecID))
                        {
                            try
                            {
                                XElement elmRoot = XElement.Parse(_data_getter.sce_take_extDDict[info.SecID]);
                                if (elmRoot.Element("UseText") != null)
                                    strValue = elmRoot.Element("UseText").Value;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        }
                    }

                    //row.Cells[colScore.Name].Value = info.Score == "-1" ? "��" : info.Score;
                    row.Cells[colScore.Name].Value = strValue;
                    row.Cells[colScore.Name].Tag = info.Score;
                    row.Tag = info;
                    //_valueManager.AddValue(info.CourseID, info.Score == "-1" ? "��" : info.Score);
                    _valueManager.AddValue(info.CourseID, strValue);
                }
            }
            dgvScore.SuspendLayout();
            picWaiting.Hide();
            _loading = false;
        }

        #region Inner Classes
        private class DataGetter
        {
            private string _running_id;
            private DSXmlHelper _currentResponse;
            private DSResponse rsp_temp;
            private ScoreType _score_type;
            // ���q���Z extension ���e
            public Dictionary<string, string> sce_take_extDDict;

            public DataGetter(string runningId, ScoreType scoreType)
            {
                _running_id = runningId;
                _score_type = scoreType;
                sce_take_extDDict = new Dictionary<string, string>();
            }

            private string StudentID
            {
                get { return _running_id; }
            }

            public DSXmlHelper CurrentResponse
            {
                get { return _currentResponse; }
                private set { _currentResponse = value; }
            }

            public RowInfoCollection Evaluate(ConditionArg arg)
            {
                //�����o�Ӿǥͩ���w�Ǧ~�׾Ǵ��ײߤ��Ҧ��ҵ{�M��
                DSXmlHelper xml_temp = GetMajorCourse(arg);

                DSXmlHelper h2 = new DSXmlHelper("MappingRequest");
                h2.AddElement("Field");
                h2.AddElement("Field", "All");
                h2.AddElement("Condition");
                Dictionary<string, string> _courseList = new Dictionary<string, string>();
                Dictionary<string, string> _attendidList = new Dictionary<string, string>();

                foreach (XmlElement element in xml_temp.GetElements("Course"))
                {
                    string courseName = element.SelectSingleNode("CourseName").InnerText;
                    string courseID = element.GetAttribute("ID");
                    string attendID = element.SelectSingleNode("AttendID").InnerText;

                    _courseList.Add(courseID, courseName);
                    _attendidList.Add(courseID, attendID);
                    h2.AddElement("Condition", "RefCourseID", courseID);
                }

                if (_courseList.Count == 0)
                    return new RowInfoCollection();

                // ���o�ثe�C�X�ҵ{�Ҧ����Ҹչ�Ӫ�
                try
                {
                    DSResponse dsrsp = SmartSchool.Feature.Course.QueryCourse.GetExamMapping(new DSRequest(h2));
                    h2 = dsrsp.GetContent();
                }
                catch (Exception ex)
                {
                    throw new Exception("�d�߽ҵ{�Ҧ����Ҹչ�Ӫ���:" + ex.Message, ex);
                }

                // �̾�cboExam �ҿ諸�Ҹն��عL�o���S���ѻP�Ҹժ��ҵ{, �M����ܦb�e���W                    
                RowInfoCollection collection = new RowInfoCollection();
                foreach (string courseid in _courseList.Keys)
                {
                    string courseName = _courseList[courseid];

                    if (_score_type != ScoreType.Course) //�p�G���O�ҵ{���Z�A�~�ݭn�P�_�O�_���ϥίS�w���q�C
                    {
                        bool found = false;
                        foreach (XmlElement element in h2.GetElements("Mapping"))
                        {
                            if (element.GetAttribute("CourseID") != courseid || element.GetAttribute("ExamID") != arg.Examid) continue;
                            found = true;
                        }
                        if (!found) continue;
                    }

                    RowInfo info = new RowInfo();
                    info.CourseID = courseid;
                    info.CourseName = courseName;
                    info.AttendID = _attendidList[courseid];
                    collection.Items.Add(info);
                }

                if (_score_type == ScoreType.Course) //�էO��쪺�S���ơA�N��n��ܽҵ{���Z�C
                    GetSCAttendScore(arg, collection);
                else
                    GetSCETakeScore(arg, collection);

                return collection;
            }

            private void GetSCETakeScore(ConditionArg arg, RowInfoCollection rows)
            {
                //���o�Ӿǥͩ���w�Ǧ~�׾Ǵ����Y���Ҹժ��Ҧ����Z�M��
                DSXmlHelper xmlTemp = new DSXmlHelper("Request");
                xmlTemp.AddElement("Field");
                xmlTemp.AddElement("Field", "All");
                xmlTemp.AddElement("Condition");
                xmlTemp.AddElement("Condition", "RefExamID", arg.Examid);
                xmlTemp.AddElement("Condition", "SchoolYear", arg.SchoolYear);
                xmlTemp.AddElement("Condition", "Semester", arg.Semester);
                xmlTemp.AddElement("Condition", "RefStudentID", StudentID);

                try
                {
                    rsp_temp = SmartSchool.Feature.QueryStudent.GetStudentExamScore(new DSRequest(xmlTemp));
                }
                catch (Exception ex)
                {
                    throw new Exception("�d�߾ǥͦ��Z����:" + ex.Message, ex);
                }
                xmlTemp = rsp_temp.GetContent();
                CurrentResponse = xmlTemp;

                //  ���o���q���Z extension ���
                sce_take_extDDict.Clear();

                // �̷Ӭd�ߦ^�Ӫ����Z�M���^�ҹ������ҵ{
                foreach (XmlElement element in xmlTemp.GetElements("Score"))
                {
                    string courseName = element.SelectSingleNode("CourseName").InnerText;
                    string courseID = element.SelectSingleNode("RefCourseID").InnerText;
                    string attendid = element.SelectSingleNode("RefAttendID").InnerText;
                    string sce_id = element.GetAttribute("ID");
                    string score = element.SelectSingleNode("Score").InnerText;

                    foreach (RowInfo info in rows.Items)
                    {
                        if (info.CourseID != courseID) continue;

                        info.SecID = sce_id;
                        info.Score = score;
                        if (!sce_take_extDDict.ContainsKey(sce_id))
                            sce_take_extDDict.Add(sce_id, "");
                    }
                }

                if (sce_take_extDDict.Count > 0)
                {
                    string strSQL = string.Format(@"
                    SELECT
                        id,
                        extension
                    FROM
                        sce_take
                    WHERE
                        extension <>'' AND id IN({0})", string.Join(",", sce_take_extDDict.Keys.ToArray()));

                    sce_take_extDDict.Clear();

                    QueryHelper qh1 = new QueryHelper();
                    DataTable dt = qh1.Select(strSQL);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string id = dr["id"] + "";
                        string ext = dr["extension"] + "";
                        if (!sce_take_extDDict.ContainsKey(id))
                            sce_take_extDDict.Add(id, ext);
                    }
                }


            }

            private void GetSCAttendScore(ConditionArg arg, RowInfoCollection rows)
            {
                //���o�Ӿǥͩ���w�Ǧ~�׾Ǵ����Y���Ҹժ��Ҧ����Z�M��
                DSXmlHelper xmlTemp = new DSXmlHelper("Request");
                xmlTemp.AddElement(".", "Field", "<ID/><RefStudentID/><RefCourseID/><Score/>", true);
                xmlTemp.AddElement("Condition");
                xmlTemp.AddElement("Condition", "StudentID", StudentID);

                try
                {
                    rsp_temp = QueryCourse.GetSCAttend(new DSRequest(xmlTemp));
                }
                catch (Exception ex)
                {
                    throw new Exception("�d�߾ǥͦ��Z����:" + ex.Message, ex);
                }
                xmlTemp = rsp_temp.GetContent();
                CurrentResponse = xmlTemp;

                // �̷Ӭd�ߦ^�Ӫ����Z�M���^�ҹ������ҵ{
                foreach (XmlElement element in xmlTemp.GetElements("Student"))
                {
                    DSXmlHelper helper = new DSXmlHelper(element);
                    string courseID = helper.GetText("RefCourseID");
                    string attendid = helper.GetText("@ID");
                    string score = helper.GetText("Score");

                    foreach (RowInfo info in rows.Items)
                    {
                        if (info.CourseID != courseID) continue;

                        info.SecID = "-1";
                        info.Score = score;
                    }
                }
            }

            private DSXmlHelper GetMajorCourse(ConditionArg arg)
            {
                DSXmlHelper xmlTemp = new DSXmlHelper("Request");
                xmlTemp.AddElement("Field");
                xmlTemp.AddElement("Field", "All");
                xmlTemp.AddElement("Condition");
                xmlTemp.AddElement("Condition", "SchoolYear", arg.SchoolYear);
                xmlTemp.AddElement("Condition", "Semester", arg.Semester);
                xmlTemp.AddElement("Condition", "RefStudentID", StudentID);
                try
                {
                    rsp_temp = SmartSchool.Feature.QueryStudent.GetStudentMajorCourse(new DSRequest(xmlTemp));
                    xmlTemp = rsp_temp.GetContent();
                }
                catch (Exception ex)
                {
                    throw new Exception("�d�߾ǥͩ���w�Ǧ~�׾Ǵ��ײߤ��Ҧ��ҵ{�M�楢��:" + ex.Message, ex);
                }
                return xmlTemp;
            }
        }
        #endregion

        #endregion

        #region User Interactions

        private bool IsValid()
        {
            if (!_initialized) return true;

            _errorProvider.SetError(cboSchoolYear, null);
            _errorProvider.SetError(cboSemester, null);
            bool valid = true;
            int a;
            if (!int.TryParse(cboSchoolYear.Text, out a))
            {
                _errorProvider.SetError(cboSchoolYear, "�������Ʀr");
                valid = false;
            }
            _errorProvider.SetError(cboSemester, null);
            if (cboSemester.Text != "1" && cboSemester.Text != "2")
            {
                _errorProvider.SetError(cboSemester, "�������Ʀr 1 �� 2 ");
                valid = false;
            }
            return valid;
        }

        private void cboSchoolYear_Validated(object sender, EventArgs e)
        {

            if (!IsValid()) return;
            if ("" + cboSchoolYear.Tag != cboSchoolYear.Text)
            {
                cboSchoolYear.Tag = cboSchoolYear.Text;
                BindData();
            }
        }

        private void cboSemester_Validated(object sender, EventArgs e)
        {
            if (!IsValid()) return;
            if ("" + cboSemester.Tag != cboSemester.Text)
            {
                cboSemester.Tag = cboSemester.Text;
                BindData();
            }
        }

        private void cboExam_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsValid()) return;
            if ("" + cboExam.Tag != cboExam.Text)
            {
                cboExam.Tag = cboExam.Text;
            }
            BindData();
        }

        private void dgvScore_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dgvScore.Rows[e.RowIndex];
            DataGridViewCell cell = row.Cells[e.ColumnIndex];
            cell.Style.BackColor = Color.White;
            cell.Style.ForeColor = Color.Black;
            cell.ErrorText = string.Empty;
            cell.ToolTipText = string.Empty;
            RowInfo info = row.Tag as RowInfo;

            string id = info.CourseID;
            //string oldValue = info.Score == "-1" ? "��" : info.Score;
            string oldValue = info.Score;

            if (info.Score == "-1" || info.Score == "-2")
            {
                // Ū���ʦҹ��
                if (_data_getter.sce_take_extDDict.ContainsKey(info.SecID))
                {
                    try
                    {
                        XElement elmRoot = XElement.Parse(_data_getter.sce_take_extDDict[info.SecID]);
                        if (elmRoot.Element("UseText") != null)
                            oldValue = elmRoot.Element("UseText").Value;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }


            string value = cell.Value == null ? null : cell.Value.ToString();

            decimal score;
            if (_process_score_type == ScoreType.Exam)
            {
                // �B�z�ʦҳ]�w
                if (!string.IsNullOrEmpty(value) && !decimal.TryParse(value, out score) && !ScoreValueMangTextDict.ContainsKey(value))
                {
                    //cell.ErrorText = "���ƥ������Ʀr�Ρy�ʡz�C";
                    cell.ErrorText = "���ƥ������Ʀr�Ρy" + string.Join("�B", ScoreValueMangTextDict.Keys.ToArray()) + "�z�C";
                    return;
                }

            }
            else
            {
                if (!string.IsNullOrEmpty(value) && !decimal.TryParse(value, out score))
                {
                    cell.ErrorText = "���ƥ������Ʀr�C";
                    return;
                }
            }

            OnValueChanged(id, value);
            if (value != oldValue)
            {
                cell.Style.BackColor = Color.Yellow;
                cell.Style.ForeColor = Color.Blue;
                cell.ToolTipText = "�w�ѡy" + oldValue + "�z�ܦ��y" + value + "�z�C";
            }
        }

        public override void Save()
        {
            if (!isAllValid())
            {
                MsgBox.Show("��ƿ�J�����T�A�Эץ���A���x�s�G" + Title);
                return;
            }

            if (_process_score_type == ScoreType.Course)
                SaveCourseScore();
            else
                SaveExamScore();

            SaveButtonVisible = false;
            LoadContent(RunningID);
        }

        private void SaveCourseScore()
        {
            bool updateRequired = false;
            DSXmlHelper updateHelper = new DSXmlHelper("Request");

            StringBuilder updateDesc = new StringBuilder("");
            updateDesc.Append("�ǥ͡G").Append(Student.Instance.Items[RunningID].Name);
            updateDesc.Append(", �Ǧ~�סG").Append(cboSchoolYear.Text);
            updateDesc.Append(", �Ǵ��G").AppendLine(cboSemester.Text);

            foreach (DataGridViewRow row in dgvScore.Rows)
            {
                RowInfo info = row.Tag as RowInfo;
                string newScore = row.Cells[colScore.Name].Value + string.Empty;

                if (info.Score == newScore) continue;

                updateDesc.AppendLine("�ҵ{���Z�y" + info.CourseName + "�z�ѡy" + info.Score + "�z�קאּ�y" + newScore + "�z�C");

                updateHelper.AddElement("Attend");
                updateHelper.AddElement("Attend", "ID", info.AttendID);
                updateHelper.AddElement("Attend", "Score", newScore);

                updateRequired = true;
            }

            if (updateRequired)
            {
                try
                {
                    EditCourse.UpdateAttend(updateHelper);
                    CurrentUser.Instance.AppLog.Write(EntityType.Student, EntityAction.Update, RunningID, updateDesc.ToString(), "�ǥͽҵ{���Z", updateHelper.GetRawXml());
                }
                catch (Exception ex)
                {
                    CurrentUser.ReportError(ex);
                    MsgBox.Show(ex.Message);
                }
            }
        }

        private void SaveExamScore()
        {
            DSXmlHelper updateHelper = new DSXmlHelper("Request");
            DSXmlHelper insertHelper = new DSXmlHelper("Request");
            DSXmlHelper deleteHelper = new DSXmlHelper("Request");

            //  �]�� Service �^�g extension �����D�A��ϥ� sql �覡�B�z
            List<string> updateExtensionList = new List<string>();
            List<string> insertExtensionLList = new List<string>();


            updateHelper.AddElement("ScoreSheetList");
            insertHelper.AddElement("ScoreSheetList");
            deleteHelper.AddElement("ScoreSheet");

            StringBuilder updateDesc = new StringBuilder("");
            updateDesc.Append("�ǥ͡G").Append(Student.Instance.Items[RunningID].Name);
            updateDesc.Append(", �Ǧ~�סG").Append(cboSchoolYear.Text);
            updateDesc.Append(", �Ǵ��G").Append(cboSemester.Text);
            updateDesc.Append(", �ҸաG").Append(cboExam.Text).Append("\n");

            StringBuilder insertDesc = new StringBuilder(updateDesc.ToString());
            StringBuilder deleteDesc = new StringBuilder(updateDesc.ToString());

            bool hasInsert = false, hasUpdate = false, hasDelete = false;

            foreach (DataGridViewRow row in dgvScore.Rows)
            {
                RowInfo info = row.Tag as RowInfo;
                DataGridViewCell cell = row.Cells[colScore.Name];

                /** 
                 * �ק�
                 * 1.���ƳQ�ܧ�
                 * 2.Sce_ID �����ŭ�
                 * 3.�ܧ����Ƥ����ŭ�
                 */
                if (cell.Style.BackColor == Color.Yellow && cell.Value != null && !string.IsNullOrEmpty(info.SecID))
                {
                    updateHelper.AddElement("ScoreSheetList", "ScoreSheet");
                    updateHelper.AddElement("ScoreSheetList/ScoreSheet", "ID", info.SecID);

                    // ��Ӽg�J���Z
                    string strValue = cell.Value == null ? "" : cell.Value.ToString();
                    string strScore = strValue;

                    //updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", cell.Value == null ? "" : cell.Value.ToString());

                    string xmlExtension = "";

                    if (_data_getter.sce_take_extDDict.ContainsKey(info.SecID))
                    {
                        xmlExtension = _data_getter.sce_take_extDDict[info.SecID];
                    }

                    if (ScoreValueMangTextDict.ContainsKey(strValue))
                    {
                        strScore = ScoreValueMangTextDict[strValue];
                        XElement elmRoot = null;
                        if (xmlExtension == "")
                        {
                            // �S���
                            elmRoot = new XElement("Extension");
                            elmRoot.SetElementValue("UseText", strValue);
                        }
                        else
                        {
                            try
                            {
                                // �w����L���
                                elmRoot = XElement.Parse(xmlExtension);
                                elmRoot.SetElementValue("UseText", strValue);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        }

                        if (elmRoot != null)
                            xmlExtension = elmRoot.ToString();
                    }


                    updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", strScore);

                    if (xmlExtension != "")
                    {
                        //updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Extension");
                        //updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Extension", xmlExtension);
                        string strSQL = string.Format(@"
                        UPDATE
                            sce_take
                        SET
                            extension = '{0}'
                        WHERE
                            id = {1};
                        ", xmlExtension, info.SecID);
                        updateExtensionList.Add(strSQL);
                    }

                    updateDesc.Append("�ҵ{�y" + info.CourseName + "�z���Z�ѡy" + info.Score + "�z�קאּ�y" + cell.Value.ToString() + "�z�C\n");
                    hasUpdate = true;
                }
                /** 
                * �s�W
                * 1.���ƳQ�ܧ�
                * 2.Sce_ID ���ŭ�
                * 3.�ܧ����Ƥ����ŭ�
                */
                else if (cell.Style.BackColor == Color.Yellow && cell.Value != null && string.IsNullOrEmpty(info.SecID))
                {
                    insertHelper.AddElement("ScoreSheetList", "ScoreSheet");
                    insertHelper.AddElement("ScoreSheetList/ScoreSheet", "AttendID", info.AttendID);
                    insertHelper.AddElement("ScoreSheetList/ScoreSheet", "ExamID", ((KeyValuePair<string, string>)cboExam.SelectedItem).Key);

                    string xmlExtension = "";
                    string strValue = cell.Value == null ? "" : cell.Value.ToString();
                    string strScore = strValue;
                    if (ScoreValueMangTextDict.ContainsKey(strValue))
                    {
                        strScore = ScoreValueMangTextDict[strValue];
                        XElement elmRoot = new XElement("Extension");
                        elmRoot.SetElementValue("UseText", strValue);
                        xmlExtension = elmRoot.ToString();
                    }


                    //insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", cell.Value == null ? "" : cell.Value.ToString());

                    insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", strScore);

                    // �g�J extension
                    if (xmlExtension != "")
                    {
                        //insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Extension");
                        //insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Extension", xmlExtension);

                        string strSQL = string.Format(@"
                        UPDATE
                            sce_take
                        SET
                            extension = '{0}'
                        WHERE
                            ref_exam_id = {1}
                            AND ref_sc_attend_id = {2};
                        ", xmlExtension, ((KeyValuePair<string, string>)cboExam.SelectedItem).Key, info.AttendID);
                        insertExtensionLList.Add(strSQL);
                    }


                    insertDesc.Append("�ҵ{�y" + info.CourseName + "�z�s�W���Z�y" + cell.Value.ToString() + "�z�C\n");
                    hasInsert = true;
                }
                /** 
                * �R��
                * 1.���ƭ쥻�����ŭ�
                * 2.Sce_ID �����ŭ�
                * 3.�ܧ����Ƭ��ŭ�
                */
                else if (cell.Tag != null && !string.IsNullOrEmpty(info.SecID) && cell.Value == null)
                {
                    deleteHelper.AddElement("ScoreSheet", "ID", info.SecID);

                    deleteDesc.Append("�N�ҵ{�y" + info.CourseName + "�z�즨�Z�y" + info.Score + "�z�R���C\n");
                    hasDelete = true;
                }
            }
            try
            {
                if (hasInsert)
                {
                    SmartSchool.Feature.Course.EditCourse.InsertSCEScore(new DSRequest(insertHelper));
                    CurrentUser.Instance.AppLog.Write(EntityType.Student, EntityAction.Insert, RunningID, insertDesc.ToString(), Title, insertHelper.GetRawXml());

                    if (insertExtensionLList.Count > 0)
                    {
                        // ��s extension
                        K12.Data.UpdateHelper uh = new K12.Data.UpdateHelper();
                        uh.Execute(insertExtensionLList);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MsgBox.Show("�s�W���Z���~:" + ex.Message);
            }

            try
            {
                if (hasUpdate)
                {
                    SmartSchool.Feature.Course.EditCourse.UpdateSCEScore(new DSRequest(updateHelper));
                    CurrentUser.Instance.AppLog.Write(EntityType.Student, EntityAction.Update, RunningID, updateDesc.ToString(), Title, updateHelper.GetRawXml());

                    if (updateExtensionList.Count > 0)
                    {
                        // ��s extension
                        K12.Data.UpdateHelper uh = new K12.Data.UpdateHelper();
                        uh.Execute(updateExtensionList);
                    }
                }
            }
            catch (Exception ex)
            {
                MsgBox.Show("�ק令�Z���~:" + ex.Message);
            }

            try
            {
                if (hasDelete)
                {
                    SmartSchool.Feature.Course.EditCourse.DeleteSCEScore(new DSRequest(deleteHelper));
                    CurrentUser.Instance.AppLog.Write(EntityType.Student, EntityAction.Delete, RunningID, deleteDesc.ToString(), Title, deleteHelper.GetRawXml());
                }
            }
            catch (Exception ex)
            {
                MsgBox.Show("�R�����Z���~:" + ex.Message);
            }
        }

        private bool isAllValid()
        {
            if (!IsValid()) return false;
            foreach (DataGridViewRow row in dgvScore.Rows)
            {
                DataGridViewCell cell = row.Cells[colScore.Name];
                //if (cell.Style.BackColor == Color.Red)
                if (!string.IsNullOrEmpty(cell.ErrorText))
                    return false;
            }
            return true;
        }

        private void panel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Shift)
                XmlBox.ShowXml(_data_getter.CurrentResponse.BaseElement);
        }

        private void dgvScore_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dgvScore.Rows[e.RowIndex];
            DataGridViewCell cell = row.Cells[e.ColumnIndex];
            cell.Style.BackColor = Color.White;
            cell.Style.ForeColor = Color.Black;
            cell.ToolTipText = "";
            RowInfo info = row.Tag as RowInfo;

            string id = info.CourseID;
            string oldValue = info.Score;
            string value = cell.Value == null ? "" : cell.Value.ToString();

            decimal score;
            if (!string.IsNullOrEmpty(value) && !decimal.TryParse(value, out score))
            {
                cell.Style.BackColor = Color.Red;
                cell.Style.ForeColor = Color.White;
                cell.ToolTipText = "���ƥ������Ʀr";
                return;
            }

            OnValueChanged(id, value);
            if (value != oldValue)
            {
                cell.Style.BackColor = Color.Yellow;
                cell.Style.ForeColor = Color.Blue;
                cell.ToolTipText = "�w�ѡy" + oldValue + "�z�ܦ��y" + value + "�z";
            }
        }

        #endregion
        public override object Clone()
        {
            return new CourseScorePalmerwormItem();
        }
    }

    #region Internal Classes

    internal enum ScoreType
    {
        Exam, Course
    }

    internal class RowInfo
    {
        private string _secID;

        public RowInfo()
        {
        }

        public string SecID
        {
            get { return _secID; }
            set { _secID = value; }
        }
        private string _courseID;

        public string CourseID
        {
            get { return _courseID; }
            set { _courseID = value; }
        }
        private string _courseName;

        public string CourseName
        {
            get { return _courseName; }
            set { _courseName = value; }
        }
        private string _score;

        public string Score
        {
            get { return _score; }
            set { _score = value; }
        }

        private string _attendID;

        public string AttendID
        {
            get { return _attendID; }
            set { _attendID = value; }
        }
    }

    internal class RowInfoCollection
    {
        private List<RowInfo> _items;
        public RowInfoCollection()
        {
            _items = new List<RowInfo>();
        }
        internal List<RowInfo> Items
        {
            get { return _items; }
            set { _items = value; }
        }
    }

    internal class ConditionArg
    {
        private string _semester;

        public string Semester
        {
            get { return _semester; }
            set { _semester = value; }
        }
        private string _schoolYear;

        public string SchoolYear
        {
            get { return _schoolYear; }
            set { _schoolYear = value; }
        }
        private string _examid;

        public string Examid
        {
            get { return _examid; }
            set { _examid = value; }
        }
    }
    #endregion
}
