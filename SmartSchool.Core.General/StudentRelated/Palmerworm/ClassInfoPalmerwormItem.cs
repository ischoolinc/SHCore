using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using FISCA.DSAUtil;
using SmartSchool.AccessControl;
using SmartSchool.Common;
using SmartSchool.Feature;
using SmartSchool.Feature.Basic;
using SmartSchool.Feature.Class;
using SmartSchool.Properties;
using System.Data;

namespace SmartSchool.StudentRelated.Palmerworm
{
    [FeatureCode("Content0020")]
    internal partial class ClassInfoPalmerwormItem : PalmerwormItem
    {
        private bool _isInitialized = false;
        private BackgroundWorker _getDepartmentBackgroundWorker;
        //private BackgroundWorker _getClassBackgroundWorker;
        private BackgroundWorker _getSeatNoBackgroundWorker;

        private string _WaitToGetSeatNo;
        private string _WaitToGetClass;
        //private string test;
        //private AutoResetEvent _getDepartmentWait;

        private DSResponse dsrsp;
        //private Dictionary<string, string> classList;
        private List<int> seatNoList;

        private EnhancedErrorProvider _errors = new EnhancedErrorProvider();

        public ClassInfoPalmerwormItem()
            : base()
        {
            InitializeComponent();
            //_isLoading = true;           
            this.Title = "�ǥͯZ�Ÿ�T";

            warnings.Icon = Resources.warning;
            _errors.Icon = Resources.error;
            //_getClassBackgroundWorker = new BackgroundWorker();
            //_getClassBackgroundWorker.DoWork += new DoWorkEventHandler(_getClassBackgroundWorker_DoWork);
            //_getClassBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getClassBackgroundWorker_RunWorkerCompleted);

            _getSeatNoBackgroundWorker = new BackgroundWorker();
            _getSeatNoBackgroundWorker.DoWork += new DoWorkEventHandler(_getSeatNoBackgroundWorker_DoWork);
            _getSeatNoBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getSeatNoBackgroundWorker_RunWorkerCompleted);

            //_getDepartmentWait = new AutoResetEvent(true);
            //_getDepartmentBackgroundWorker = new BackgroundWorker();
            //_getDepartmentBackgroundWorker.DoWork += new DoWorkEventHandler(_getDepartmentBackgroundWorker_DoWork);
            //_getDepartmentBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_getDepartmentBackgroundWorker_RunWorkerCompleted);

            //_getDepartmentBackgroundWorker.RunWorkerAsync();
            //_getDepartmentWait.Reset();
        }
        public override object Clone()
        {
            return new ClassInfoPalmerwormItem();
        }

        //void _getClassBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if ("" + e.Result != _WaitToGetClass)
        //    {
        //        GetGradeYearClassList(_WaitToGetClass);
        //    }
        //    else
        //    {
        //        foreach (string key in classList.Keys)
        //        {
        //            cboClass.AddItem(key, classList[key]);
        //        }
        //        //cboClass.SetComboBoxValue(test);

        //    }
        //}

        //void _getClassBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    string gradeyearID = e.Argument as string;
        //    classList = Class.GetClassListByGradeYear(gradeyearID);
        //    e.Result = gradeyearID;
        //}

        private void Initialize()
        {
            warnings.Clear();
            _errors.Clear();

            //�~��
            cboGradeYear.SelectedItem = null;
            cboGradeYear.Items.Clear();
            cboGradeYear.AddItem("", "");
            cboGradeYear.AddItem("1", "�@�~��");
            cboGradeYear.AddItem("2", "�G�~��");
            cboGradeYear.AddItem("3", "�T�~��");
            cboGradeYear.AddItem("4", "�|�~��");
            cboGradeYear.AddItem("5", "���~��");
            cboGradeYear.AddItem("6", "���~��");
            cboGradeYear.AddItem("7", "�C�~��");
            cboGradeYear.AddItem("8", "�K�~��");
            cboGradeYear.AddItem("9", "�E�~��");
            cboGradeYear.AddItem("10", "�Q�~��");
            cboGradeYear.AddItem("11", "�Q�@�~��");
            cboGradeYear.AddItem("12", "�Q�G�~��");

            if (_isInitialized) return;

            //��O
            // 2008/04/30 ��ģ��
            DSResponse dsrsp = SmartSchool.Feature.Basic.Config.GetDepartment();
            foreach (XmlNode node in dsrsp.GetContent().GetElements("Department"))
            {
                cboDept.AddItem(node.SelectSingleNode("@ID").InnerText, node.SelectSingleNode("Name").InnerText);
            }

            _isInitialized = true;
        }

        #region IPalmerwormItem ����

        protected override object OnBackgroundWorkerWorking()
        {
            if (RunningID != null)
                return QueryStudent.GetClassInfo(RunningID);
            return null;
        }

        //void _getDepartmentBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    foreach (XmlNode node in dsrsp.GetContent().GetElements("Department"))
        //    {
        //        cboProgram.AddItem(node.SelectSingleNode("Name").InnerText, node.SelectSingleNode("Name").InnerText);
        //    }
        //}

        //void _getDepartmentBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    dsrsp = SmartSchool.Feature.Basic.Config.GetDepartment();
        //    //_getDepartmentWait.Set();
        //}

        protected override void OnBackgroundWorkerCompleted(object result)
        {
            Initialize();
            XmlNode classInfoNode = result as XmlNode;
            bool hasClass = classInfoNode.SelectSingleNode("GradeYear") != null;
            string selectedValue;

            //���J�~��
            string gradeYear = hasClass ? classInfoNode.SelectSingleNode("GradeYear").InnerText : "";
            cboGradeYear.SetComboBoxValue(gradeYear);
            //_valueManager.AddValue("GradeYear", gradeYear); //�~�Ť��s�b��Ʈw�A�u��L�o�Z�ťΡC

            //���J�Z��
            string classID = hasClass ? classInfoNode.SelectSingleNode("RefClassID").InnerText : "";
            cboClass.SetComboBoxValue(classID);
            _valueManager.AddValue("RefClassID", classID, "�Z��");

            TipClassSelection(); //���ܯZ�ſ�ܤ覡�C

            //���J��O 
            //2008/04/30 ��ģ��
            cboDept.SelectedIndex = -1;
            cboDept.WatermarkText = string.Empty;
            string overrideDeptId = hasClass ? classInfoNode.SelectSingleNode("OverrideDeptID").InnerText : "";
            string inhertDeptName = hasClass ? classInfoNode.SelectSingleNode("InhertDeptName").InnerText : "";
            if (!string.IsNullOrEmpty(overrideDeptId))
                cboDept.SetComboBoxValue(overrideDeptId);
            else
                cboDept.WatermarkText = inhertDeptName;

            selectedValue = cboDept.GetValue();
            _valueManager.AddValue("Department", selectedValue, "��O");

            //���J�Ǹ�
            string studentID = hasClass ? classInfoNode.SelectSingleNode("StudentNumber").InnerText : "";
            this.txtStudentID.Text = studentID;
            _valueManager.AddValue("StudentNumber", studentID, "�Ǹ�");

            //���J�y��
            string seatNo = hasClass ? classInfoNode.SelectSingleNode("SeatNo").InnerText : "";
            //SetSeatNo(seatNo);
            cboSeatNo.Text = seatNo;
            _valueManager.AddValue("SeatNo", seatNo, "�y��");
        }

        void _getSeatNoBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string classID = e.Argument as string;

            //added by Cloud 2014.2.20
            if(!string.IsNullOrWhiteSpace(classID))
            {
                seatNoList = new List<int>();
                FISCA.Data.QueryHelper Q = new FISCA.Data.QueryHelper();

                string cmd = string.Format("SELECT seat_no FROM student WHERE ref_class_id={0} AND seat_no IS NOT NULL AND status=1 ORDER BY seat_no", classID);

                DataTable dt = Q.Select(cmd);

                foreach (DataRow row in dt.Rows)
                {
                    string no = row["seat_no"].ToString();
                    int value = 0;
                    int.TryParse(no, out value);

                    if (!seatNoList.Contains(value) && value != 0)
                    {
                        seatNoList.Add(value);
                    }
                }
            }

            //�I�s��service�����D
            //seatNoList = Class.ListSeatNo(classID);
            e.Result = classID;
        }

        void _getSeatNoBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ("" + e.Result != _WaitToGetSeatNo)
            {
                GetClassSeatNoList(_WaitToGetSeatNo);
            }
            else
            {
                int maxNo = 0;
                if (seatNoList.Count > 0)
                    maxNo = seatNoList[seatNoList.Count - 1];
                List<int> needSeatNoList = new List<int>();
                for (int i = 1; i <= maxNo + 1; i++)
                {
                    if (!seatNoList.Contains(i))
                        needSeatNoList.Add(i);
                }
                foreach (int sn in needSeatNoList)
                {
                    cboSeatNo.Items.Add(sn);
                }
            }
        }

        private void GetClassSeatNoList(string classID)
        {
            _WaitToGetSeatNo = classID;
            cboSeatNo.SelectedItem = null;
            cboSeatNo.Items.Clear();
            if (!_getSeatNoBackgroundWorker.IsBusy)
            {
                _getSeatNoBackgroundWorker.RunWorkerAsync(classID);
            }
        }

        //private void GetGradeYearClassList(string gradeyearID)
        //{
        //    _WaitToGetClass = gradeyearID;
        //    cboClass.Items.Clear();
        //    if (!_getClassBackgroundWorker.IsBusy)
        //    {
        //        _getClassBackgroundWorker.RunWorkerAsync(gradeyearID);
        //    }
        //}

        public override void Save()
        {
            ValidateStudentNumber(); //�b��@���ǥͬO�_���СA�]���b�Y�Ǫ��A���|�Ұ����ҵ{���C

            if (_errors.HasError)
            {
                MsgBox.Show("��J��ƥ��q�L���ҡA�Эץ���A�x�s");
                return;
            }

            //EditStudent.Update(_valueManager.GetRequest("StudentList", "Student", "Field", "Condition", "ID", RunningID));
            DSXmlHelper helper = new DSXmlHelper("StudentList");
            helper.AddElement("Student");
            helper.AddElement("Student", "Field");
            foreach (string key in _valueManager.GetDirtyItems().Keys)
            {
                string elementName = (key == "Department" ? "OverrideDeptID" : key);
                string value = _valueManager.GetDirtyItems()[key];
                helper.AddElement("Student/Field", elementName, value);
            }
            helper.AddElement("Student", "Condition");
            helper.AddElement("Student/Condition", "ID", RunningID);
            EditStudent.Update(new DSRequest(helper));

            LogUtility.LogChange(_valueManager, RunningID, GetStudentName());

            //Student.Instance.InvokBriefDataChanged(RunningID);
            SmartSchool.Broadcaster.Events.Items["�ǥ�/����ܧ�"].Invoke(RunningID);
            SaveButtonVisible = false;
        }

        private string GetStudentName()
        {
            try
            {
                BriefStudentData student = Student.Instance.Items[RunningID];
                return student.Name;
            }
            catch (Exception)
            {
                return "<" + RunningID + ">";
            }
        }

        #endregion

        private void SetComboBoxValue(ComboBox combo, string value)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i].ToString() == value)
                {
                    combo.SelectedIndex = i;
                    break;
                }
            }
            if (combo.SelectedIndex == -1)
                combo.Text = "";
        }

        private string GetComboValue(ComboBox combo)
        {
            return ((KeyValuePair<string, string>)combo.SelectedItem).Key;
        }


        private void txtStudentID_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("StudentNumber", txtStudentID.Text.Trim());
        }

        private void cboGradeYear_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            cboClass.SelectedItem = null;
            cboClass.Items.Clear();
            cboSeatNo.SelectedItem = null;
            cboSeatNo.Items.Clear();
            cboSeatNo.Text = "";

            //if (cboGradeYear.GetValue() != "")
            //{
            Dictionary<string, string> classList = Class.GetClassListByGradeYear(cboGradeYear.GetValue());
            foreach (string key in classList.Keys)
                cboClass.AddItem(key, classList[key]);
            //cboClass.Enabled = false;
            //}
            //else
            //    GetGradeYearClassList(cboGradeYear.GetValue());

            OnValueChanged("GradeYear", cboGradeYear.GetValue());
            OnValueChanged("RefClassID", cboClass.GetValue());
        }

        private void cboClass_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            OnValueChanged("RefClassID", cboClass.GetValue());
            //string seatNo = cboSeatNo.Text;
            //SetSeatNo(seatNo);
            cboSeatNo.Text = "";

            //if (cboClass.GetValue() == "")
            //    cboSeatNo.Enabled = false;
            //else
            GetClassSeatNoList(cboClass.GetValue());

            try
            {
                KeyValuePair<string, string> kv = ((KeyValuePair<string, string>)cboClass.SelectedItem);
                DSResponse rsp = QueryClass.GetClass(kv.Key);

                // (string.IsNullOrEmpty(cboDept.Text))
                cboDept.WatermarkText = rsp.GetContent().GetText("Class/Department");
            }
            catch { }
        }

        //private void cboProgram_SelectedIndexChanged_1(object sender, EventArgs e)
        //{
        //    OnValueChanged("StudentDepartment", cboProgram.GetValue());
        //}

        private void cboSeatNo_TextChanged(object sender, EventArgs e)
        {
            int sn;
            if (int.TryParse(cboSeatNo.Text, out sn) || string.IsNullOrEmpty(cboSeatNo.Text))
            {
                OnValueChanged("SeatNo", cboSeatNo.Text);
            }
            else
            {
                MsgBox.Show("�y���������Ʀr");
            }
        }

        private void cboDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnValueChanged("Department", cboDept.GetValue());
        }

        private void SetSeatNo(string seatNo)
        {

            //���J�y��
            bool hasClass = cboClass.GetValue() != "";
            //string seatNo = hasClass ? classInfoNode.SelectSingleNode("SeatNo").InnerText : "";


            cboSeatNo.SelectedItem = null;
            cboSeatNo.Items.Clear();
            if (cboClass.GetValue() != "")
            {
                if (seatNo != "")
                    cboSeatNo.Items.Add(seatNo);
                //List<int> seatNoList = Class.ListSeatNo(cboClass.GetValue());

                int maxNo = 0;
                if (seatNoList.Count > 0)
                    maxNo = seatNoList[seatNoList.Count - 1];
                List<int> needSeatNoList = new List<int>();
                for (int i = 1; i <= maxNo + 1; i++)
                {
                    if (!seatNoList.Contains(i))
                        needSeatNoList.Add(i);
                }
                foreach (int sn in needSeatNoList)
                {
                    cboSeatNo.Items.Add(sn);
                }
                //if (seatNo != "")
                SetComboBoxValue(cboSeatNo, seatNo);
                cboSeatNo.Enabled = true;
            }
            else
            {
                cboSeatNo.Enabled = false;
            }
        }

        private void cboSeatNo_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            OnValueChanged("SeatNo", cboSeatNo.Text);
        }

        private void label40_Click(object sender, EventArgs e)
        {

        }

        private void ClassInfoPalmerwormItem_Load(object sender, EventArgs e)
        {

        }

        private void txtStudentID_Validating(object sender, CancelEventArgs e)
        {
            ValidateStudentNumber();
        }

        private void ValidateStudentNumber()
        {
            if (string.IsNullOrEmpty(txtStudentID.Text))
            {
                _errors.SetError(txtStudentID, string.Empty);
                return; //�ťդ��ˬd�C
            }

            if (QueryStudent.StudentNumberExists(RunningID, txtStudentID.Text))
                _errors.SetError(txtStudentID, "�Ǹ����СA�нT�{��ơC");
            else
                _errors.SetError(txtStudentID, string.Empty);
        }

        private void TipClassSelection()
        {
            if (string.IsNullOrEmpty(cboClass.GetValue()))
                warnings.SetError(cboClass, "�п�ܡu�~�šv��A��ܡu�Z�šv�C");
            else
                warnings.SetError(cboClass, "");
        }

        private void cboClass_Validating(object sender, CancelEventArgs e)
        {
            TipClassSelection();
        }
    }
}
