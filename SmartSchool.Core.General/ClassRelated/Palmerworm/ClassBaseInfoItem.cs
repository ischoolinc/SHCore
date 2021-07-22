using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using FISCA.DSAUtil;
using SmartSchool.Feature.Class;
using SmartSchool.Feature.Teacher;
using System.Xml;
using DevComponents.DotNetBar;
using SmartSchool.ApplicationLog;
using SmartSchool.TeacherRelated;
using SmartSchool.AccessControl;

namespace SmartSchool.ClassRelated.Palmerworm
{
    [FeatureCode("Content0150")]
    internal partial class ClassBaseInfoItem : ClassPalmerwormItem
    {
        private TeacherCollection _teacherList;
        private DSXmlHelper _dept_list;
        private DSXmlHelper _gradeYearList;
        private ErrorProvider epTeacher = new ErrorProvider();
        private ErrorProvider epDept = new ErrorProvider();
        private ErrorProvider epDisplayOrder = new ErrorProvider();
        private ErrorProvider epGradeYear = new ErrorProvider();
        private ErrorProvider epClassName = new ErrorProvider();
        private ErrorProvider epClassNumber = new ErrorProvider();

        private string _NamingRule = "";
        private bool _StopEvent = false;
        #region Log �ݭn�Ψ쪺�ܼ�

        private ClassBaseLogMachine machine = new ClassBaseLogMachine();

        #endregion
        public override object Clone()
        {
            return new ClassBaseInfoItem();
        }
        public ClassBaseInfoItem()
            : base()
        {
            Font = FontStyles.General;
            InitializeComponent();

            if (Site != null && Site.DesignMode)
                return;

            Initialize();
            Title = "�Z�Ű򥻸��";
        }


        private void Initialize()
        {
            TeacherRelated.Teacher.Instance.TeacherDataChanged += new EventHandler<TeacherDataChangedEventArgs>(Instance_TeacherDataChanged);
            TeacherRelated.Teacher.Instance.TeacherDeleted += new EventHandler<TeacherDeletedEventArgs>(Instance_TeacherDeleted);
            TeacherRelated.Teacher.Instance.TeacherInserted += new EventHandler(Instance_TeacherInserted);

            // ���o�Ҧ��Юv�W��
            InitializeTeacherList();

            _dept_list = Feature.Basic.Config.GetDepartment().GetContent();
            InitializeDeptList();

            // ���o�Ҧ��~�ŲM��
            _gradeYearList = QueryClass.GetGradeYearList().GetContent();
            List<string> gList = new List<string>();
            foreach (XmlNode node in _gradeYearList.GetElements("GradeYear"))
            {
                string gradeYear = node.SelectSingleNode("GradeYear").InnerText;
                if (!gList.Contains(gradeYear))
                {
                    gList.Add(gradeYear);
                    //cboGradeYear.Items.Add(gradeYear);
                }
            }

            gList.Sort(GradeYearSort);
            cboGradeYear.Items.AddRange(gList.ToArray());
        }

        private int GradeYearSort(string x, string y)
        {
            string xx = x.PadLeft(10, '0');
            string yy = y.PadLeft(10, '0');

            return xx.CompareTo(yy);
        }

        private void InitializeTeacherList()
        {
            //if (TeacherRelated.Teacher.Instance.Items.Count <= 0)
            //{
            //    //�z�פW�o�̪��{�����Ӥ��|�Q����...

            //    DSXmlHelper hlpTeacher = QueryTeacher.GetTeacherDetailTest().GetContent(); //������O Test ?
            //    Dictionary<string, BriefTeacherData> dicTeacher = new Dictionary<string, BriefTeacherData>();

            //    foreach (XmlElement eachTeacher in hlpTeacher.GetElements("Teacher"))
            //    {
            //        BriefTeacherData objTeacher = new BriefTeacherData(eachTeacher);

            //        if (objTeacher.Status == "�@��")
            //            dicTeacher.Add(objTeacher.ID, objTeacher);
            //    }

            //    _teacherList = new TeacherCollection(dicTeacher);
            //}
            //else
            {
                Dictionary<string, BriefTeacherData> temp = new Dictionary<string, BriefTeacherData>();
                foreach ( BriefTeacherData each in TeacherRelated.Teacher.Instance.Items )
                {
                    if ( each.Status == "�@��" )
                        temp.Add(each.ID, each);
                }

                _teacherList = new TeacherCollection(temp);
            }

            cboTeacher.SelectedItem = null;
            cboTeacher.Items.Clear();
            foreach (BriefTeacherData each in _teacherList)
                cboTeacher.Items.Add(each);
        }

        private void Instance_TeacherInserted(object sender, EventArgs e)
        {
            InitializeTeacherList();
        }

        private void Instance_TeacherDeleted(object sender, TeacherDeletedEventArgs e)
        {
            InitializeTeacherList();
        }

        private void Instance_TeacherDataChanged(object sender, TeacherDataChangedEventArgs e)
        {
            InitializeTeacherList();
        }

        private void InitializeDeptList()
        {
            // ���o�Ҧ���O�M��
            cboDept.SelectedItem = null;
            cboDept.Items.Clear();
            cboDept.Items.Add(new KeyValuePair<string, string>("", ""));
            foreach (XmlElement node in _dept_list.GetElements("Department"))
            {
                string id = node.GetAttribute("ID");
                string name = node.SelectSingleNode("Name").InnerText;
                cboDept.Items.Add(new KeyValuePair<string, string>(id, name));
            }
            cboDept.ValueMember = "Key";
            cboDept.DisplayMember = "Value";
        }

        protected override object OnBackgroundWorkerWorking()
        {
            _dept_list = Feature.Basic.Config.GetDepartment().GetContent();
            return QueryClass.GetClass(RunningID);
        }

        protected override void OnBackgroundWorkerCompleted(object result)
        {
            InitializeDeptList();

            //�M���¸��
            epTeacher.Clear();
            cboTeacher.Tag = null;
            epDept.Clear();
            cboDept.Tag = null;
            epDisplayOrder.Clear();
            txtSortOrder.Tag = null;
            epGradeYear.Clear();
            cboGradeYear.Tag = null;
            epClassName.Clear();
            txtClassName.Tag = null;
            epClassNumber.Clear();
            txtClassNumber.Tag = null;

            DSResponse dsrsp = result as DSResponse;
            DSXmlHelper helper = dsrsp.GetContent();
            txtClassName.Text = helper.GetText("Class/ClassName");
            _NamingRule = helper.GetText("Class/NamingRule");
            string deptId = helper.GetText("Class/RefDepartmentID");
            string deptName = helper.GetText("Class/Department");
            cboDept.SelectedItem = new KeyValuePair<string, string>(deptId, deptName);

            cboTeacher.SelectedItem = GetTeacherObject(helper.GetText("Class/RefTeacherID"));
            cboGradeYear.Text = helper.GetText("Class/GradeYear");
            txtSortOrder.Text = helper.GetText("Class/DisplayOrder");
            txtClassNumber.Text = helper.GetText("Class/ClassNumber");

            _valueManager.AddValue("ClassName", txtClassName.Text, "�Z�ŦW��");
            _valueManager.AddValue("NamingRule", _NamingRule, "�R�W�W�h");
            _valueManager.AddValue("RefDepartmentID", GetDepartmentValue(), "��O");
            _valueManager.AddValue("GradeYear", cboGradeYear.Text, "�~��");
            _valueManager.AddValue("RefTeacherID", GetTeacherID(cboTeacher.SelectedItem), "�Z�ɮv");
            _valueManager.AddValue("DisplayOrder", txtSortOrder.Text, "�ƦC�Ǹ�");
            _valueManager.AddValue("ClassNumber", txtClassNumber.Text, "�Z�Žs��");
            SaveButtonVisible = false;

            #region Log �O���ק�e�����

            machine.AddBefore(labelX1.Text.Replace("�@", ""), txtClassName.Text);
            machine.AddBefore("�R�W�W�h", _NamingRule);
            machine.AddBefore(labelX2.Text.Replace("�@", ""), deptName);
            machine.AddBefore(labelX3.Text.Replace("�@", ""), cboTeacher.Text);
            machine.AddBefore(labelX4.Text.Replace("�@", ""), cboGradeYear.Text);
            machine.AddBefore(labelX6.Text.Replace("�@", ""), txtSortOrder.Text);
            machine.AddBefore(labelX5.Text.Replace("�@", ""), txtClassNumber.Text);
            #endregion
        }

        private string GetDepartmentValue()
        {
            KeyValuePair<string, string>? kv = cboDept.SelectedItem as KeyValuePair<string, string>?;

            if (kv == null)
                return string.Empty;
            else
                return kv.Value.Key;
        }

        protected string GetTeacherID(object teacher)
        {
            BriefTeacherData objTeacher = teacher as BriefTeacherData;

            if (objTeacher == null)
                return string.Empty;
            else
                return objTeacher.ID;
        }

        protected BriefTeacherData GetTeacherObject(string id)
        {
            if (_teacherList == null)
                return null;

            if (_teacherList.ContainsKey(id))
                return _teacherList[id];
            else
                return null;
        }

        protected void cboTeacher_TextChanged(object sender, EventArgs e)
       {
            int index = cboTeacher.FindStringExact(cboTeacher.Text);
            string id = "";

            if (index >= 0)
                id = GetTeacherID(cboTeacher.Items[index]);
            else
                id = "";
            //cboTeacher.FindString(cboTeacher.Text);
            OnValueChanged("RefTeacherID", id);
        }

        protected void txtClassName_TextChanged(object sender, EventArgs e)
        {
            if ( !_StopEvent )
            {
                if ( txtClassName.ContainsFocus && Class.Instance.ValidateNamingRule(txtClassName.Text) )
                {
                    OnValueChanged("NamingRule", txtClassName.Text);
                    int gradeYear;
                    if(int.TryParse(cboGradeYear.Text,out gradeYear))
                        OnValueChanged("ClassName", Class.Instance.ParseClassName(txtClassName.Text, gradeYear));
                }
                else
                    OnValueChanged("ClassName", txtClassName.Text);

                //�p�G�Z�ŦW�٬O�Ѩ䥦�������� (��ܦ~�šA�Z�ŦW�٦۰��ܴ�)
                if (!txtClassName.ContainsFocus)
                    txtClassName_Validated(null, null);
            }
        }

        protected void cboDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnValueChanged("RefDepartmentID", GetDepartmentValue());
        }

        protected void cboGradeYear_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("GradeYear", cboGradeYear.Text);
            if ( Class.Instance.ValidateNamingRule(_NamingRule) )
            {
                int gradeYear = 0;
                if ( int.TryParse(cboGradeYear.Text, out gradeYear) )
                {
                    txtClassName.Text = Class.Instance.ParseClassName(_NamingRule, gradeYear);
                }
                else
                    txtClassName.Text = _NamingRule;
            }
        }

        protected void txtSortOrder_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("DisplayOrder", txtSortOrder.Text);
        }

        private void txtClassNumber_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("ClassNumber", txtClassNumber.Text);
            epClassNumber.Clear();
        }

        public override bool IsValid()
        {
            // �Z�ŦW������
            bool valid = true;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.Tag != null)
                {
                    if (ctrl.Tag.ToString() == "error")
                        valid = false;
                }
            }
            return valid;
        }

        private void txtClassName_Validated(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtClassName.Text))
            {
                epClassName.SetError(txtClassName, "�Z�ŦW�٤��i�ť�");
                txtClassName.Tag = "error";
                return;
            }
            if (!Class.Instance.ValidClassName(RunningID, txtClassName.Text))
            {
                epClassName.SetError(txtClassName, "�Z�Ť��i�P�䥦�Z�ŭ���");
                txtClassName.Tag = "error";
                return;
            }
            epClassName.Clear();
            txtClassName.Tag = null;
        }

        private void cboTeacher_Validated(object sender, EventArgs e)
        {
            string id = GetTeacherID(cboTeacher.SelectedItem);

            if (!string.IsNullOrEmpty(cboTeacher.Text) && id == "")
            {
                epTeacher.SetError(cboTeacher, "�d�L���Юv");
                cboTeacher.Tag = "error";
                return;
            }
            else
            {
                epTeacher.Clear();
                cboTeacher.Tag = id;
            }
        }

        private void cboTeacher_Validating(object sender, CancelEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            int index = combo.FindStringExact(combo.Text);
            if (index >= 0)
            {
                combo.SelectedItem = combo.Items[index];
            }

        }

        private void cboDept_Validated(object sender, EventArgs e)
        {
            //string name = cboDept.Text;
            //XmlElement element = null;
            //foreach (XmlElement ele in _dept_list.GetElements("Department"))
            //{
            //    if (ele.SelectSingleNode("Name").InnerText == name)
            //        element = ele;
            //}

            //if (!string.IsNullOrEmpty(name) && element == null)
            //{
            //    epDept.Icon = Properties.Resources.warning;
            //    epDept.SetError(cboDept, "�d�L����O");
            //    cboDept.Tag = null;
            //    return;
            //}
            //else
            //{
            //    epDept.Clear();
            //    cboDept.Tag = null;
            //    int index = cboDept.FindString(name);
            //}
        }

        private void txtSortOrder_Validated(object sender, EventArgs e)
        {
            string text = txtSortOrder.Text;
            int i;
            if (!string.IsNullOrEmpty(text) && !int.TryParse(text, out i))
            {
                epDisplayOrder.SetError(txtSortOrder, "�п�J���");
                txtSortOrder.Tag = "error";
                return;
            }
            epDisplayOrder.Clear();
            txtSortOrder.Tag = null;
        }

        private void cboGradeYear_Validated(object sender, EventArgs e)
        {
            string text = cboGradeYear.Text;
            bool hasGradeYear = false;
            foreach (XmlElement ele in _gradeYearList.GetElements("GradeYear"))
            {
                if (ele.SelectSingleNode("GradeYear").InnerText == text)
                    hasGradeYear = true;
            }

            int i;
            if (!string.IsNullOrEmpty(text) && !int.TryParse(text, out i))
            {
                epGradeYear.Icon = Properties.Resources.error;
                epGradeYear.SetError(cboGradeYear, "�~�ť������Ʀr");
                cboGradeYear.Tag = "error";
                return;
            }

            if (!string.IsNullOrEmpty(text) && !hasGradeYear)
            {
                epGradeYear.Icon = Properties.Resources.warning;
                epGradeYear.SetError(cboGradeYear, "�L���~��");
                cboGradeYear.Tag = null;
            }
            else
            {
                epGradeYear.Clear();
                cboGradeYear.Tag = null;
            }
        }

        private void txtClassNumber_Validated(object sender, EventArgs e)
        {


        }

        public override void Save()
        {
            if (!IsValid())
            {
                MsgBox.Show("��J��ƥ��q�L���ҡA�Эץ���A���x�s");
                return;
            }

            #region �ˬd�Z�Žs��
            //FISCA.Data.QueryHelper queryHelper = new FISCA.Data.QueryHelper();
            //string sql = "SELECT * FROM class where id <> " + RunningID + " AND class_number='" + txtClassNumber.Text + "'";
            //try
            //{
            //    DataTable dataTable = queryHelper.Select(sql);
            //    if (dataTable.Rows.Count > 0)
            //    {
            //        epClassNumber.SetError(txtClassNumber, "�Z�Žs�����ơA���ˬd�C");
            //        return;
            //    }
            //    else
            //        epClassNumber.Clear();
            //}
            //catch (Exception ex)
            //{
            //    MsgBox.Show(ex.Message);
            //}

            #endregion




            DSXmlHelper helper = new DSXmlHelper("UpdateRequest");
            helper.AddElement("Class");
            helper.AddElement("Class", "Field");
            Dictionary<string, string> items = _valueManager.GetDirtyItems();
            foreach (string key in items.Keys)
            {
                helper.AddElement("Class/Field", key, items[key]);
            }
                
            helper.AddElement("Class", "Condition");
            helper.AddElement("Class/Condition", "ID", RunningID);
            EditClass.Update(new DSRequest(helper));

            //if (_valueManager.IsDirtyItem("ClassName"))
            //    Class.Instance.InvokClassUpdated(RunningID);

            #region Log

            #region Log �O���ק�᪺���

            machine.AddAfter(labelX1.Text.Replace("�@", ""), txtClassName.Text);
            machine.AddAfter("�R�W�W�h", _NamingRule);
            machine.AddAfter(labelX2.Text.Replace("�@", ""), cboDept.Text);
            machine.AddAfter(labelX3.Text.Replace("�@", ""), cboTeacher.Text);
            machine.AddAfter(labelX4.Text.Replace("�@", ""), cboGradeYear.Text);
            machine.AddAfter(labelX6.Text.Replace("�@", ""), txtSortOrder.Text);
            machine.AddAfter(labelX5.Text.Replace("�@", ""), txtClassNumber.Text);
            #endregion

            StringBuilder desc = new StringBuilder("");
            desc.AppendLine("�Z�ŦW�١G" + Class.Instance.Items[RunningID].ClassName + " ");
            desc.AppendLine(machine.GetDescription());

            CurrentUser.Instance.AppLog.Write(EntityType.Class, EntityAction.Update, RunningID, desc.ToString(), "�Z�Ű򥻸��", "");

            #endregion

            Class.Instance.InvokClassUpdated(RunningID);

            SaveButtonVisible = false;
        }

        private void txtClassName_Enter(object sender, EventArgs e)
        {
            if ( Class.Instance.ValidateNamingRule(_NamingRule) )
            {
                _StopEvent =true;
                txtClassName.Text = _NamingRule;
                _StopEvent = false;
            }
        }

        private void txtClassName_Leave(object sender, EventArgs e)
        {
            if ( Class.Instance.ValidateNamingRule(txtClassName.Text) )
            {
                _NamingRule = txtClassName.Text;
                OnValueChanged("NamingRule", _NamingRule);
                int gradeYear = 0;
                if ( int.TryParse(cboGradeYear.Text, out gradeYear) )
                {
                    txtClassName.Text = Class.Instance.ParseClassName(_NamingRule, gradeYear);
                }
            }
        }

        private void cboDept_TextChanged(object sender, EventArgs e)
        {
            string name = cboDept.Text;
            XmlElement element = null;
            foreach (XmlElement ele in _dept_list.GetElements("Department"))
            {
                if (ele.SelectSingleNode("Name").InnerText == name)
                    element = ele;
            }

            if (!string.IsNullOrEmpty(name) && element == null)
            {
                epDept.Icon = Properties.Resources.warning;
                epDept.SetError(cboDept, "�d�L����O");
                cboDept.Tag = null;
                return;
            }
            else
            {
                epDept.Clear();
                cboDept.Tag = null;
                int index = cboDept.FindString(name);
            }
        }
    }

    /// <summary>
    /// �O���Z�Ű򥻸��
    /// </summary>
    class ClassBaseLogMachine
    {
        Dictionary<string, string> beforeData = new Dictionary<string, string>();
        Dictionary<string, string> afterData = new Dictionary<string, string>();

        public void AddBefore(string key, string value)
        {
            if (!beforeData.ContainsKey(key))
                beforeData.Add(key, value);
            else
                beforeData[key] = value;
        }

        public void AddAfter(string key, string value)
        {
            if (!afterData.ContainsKey(key))
                afterData.Add(key, value);
            else
                afterData[key] = value;
        }

        public string GetDescription()
        {
            //�u�v
            StringBuilder desc = new StringBuilder("");

            foreach (string key in beforeData.Keys)
            {
                if (afterData.ContainsKey(key) && afterData[key] != beforeData[key])
                    desc.AppendLine("���u" + key + "�v�ѡu" + beforeData[key] + "�v�ܧ󬰡u" + afterData[key] + "�v");
            }

            return desc.ToString();
        }
    }
}
