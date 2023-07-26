using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//using SmartSchool.Common;
using FISCA.DSAUtil;
using System.Xml;
using DevComponents.DotNetBar;
using FISCA.Presentation.Controls;
//using SmartSchool.Feature.Basic;
using SHSchool.Data;
using System.Linq;

namespace SmartSchool.Others.Configuration.Setup
{
    public partial class DeptSetup : BaseForm
    {        
        
        BackgroundWorker _bgWorker;
        List<SHDepartmentRecord> _deptList = new List<SHDepartmentRecord>();
        // �ݭn�Q�R��
        List<SHDepartmentRecord> _delRecList = new List<SHDepartmentRecord>();
        // �Z�Ũϥά�OID
        List<string> _classUseDeptIDList = new List<string>();

        // ��O���ݳ��OID
        List<string> _deptUseDeptGroupIDList = new List<string>();

        // ����
        Dictionary<string, string> _TeacherID_NameDict = new Dictionary<string, string>();
        Dictionary<string, string> _TeacherName_IDDict = new Dictionary<string, string>();

        // ����
        Dictionary<string, string> _DeptGroupID_NameDict = new Dictionary<string, string>();
        Dictionary<string, string> _DeptGroupName_IDDict = new Dictionary<string, string>();


        List<SHDepartmentRecord> _oldData = new List<SHDepartmentRecord>();
        // ���Ʈ��ˬd
        List<string> _checkDeptCode = new List<string>();
        List<string> _checkDeptName = new List<string>();

        public DeptSetup()
        {   
            InitializeComponent();
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += new DoWorkEventHandler(_bgWorker_DoWork);
            _bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgWorker_RunWorkerCompleted);
        }

        void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dgDept.Rows.Clear();
            foreach (SHDepartmentRecord rec in _deptList)
            {
                int rowIdx = dgDept.Rows.Add();
                dgDept.Rows[rowIdx].Tag = rec;
                dgDept.Rows[rowIdx].Cells[colCode.Index].Value = rec.Code;
                dgDept.Rows[rowIdx].Cells[colChiName.Index].Value = rec.FullName;
                if (_TeacherID_NameDict.ContainsKey(rec.RefTeacherID))
                    dgDept.Rows[rowIdx].Cells[colDeptTeacher.Index].Value = _TeacherID_NameDict[rec.RefTeacherID];
                else
                    dgDept.Rows[rowIdx].Cells[colDeptTeacher.Index].Value = "";
                if (_DeptGroupID_NameDict.ContainsKey(rec.RefDeptGroupID))
                    dgDept.Rows[rowIdx].Cells[colDeptGroup.Index].Value = _DeptGroupID_NameDict[rec.RefDeptGroupID];
                else
                    dgDept.Rows[rowIdx].Cells[colDeptGroup.Index].Value = "";
            }

            // �إ߱Юv���
            colDeptTeacher.Items.Clear();
            colDeptTeacher.Items.Add("");
            colDeptTeacher.Items.AddRange((from data in _TeacherName_IDDict.Keys orderby data select data).ToArray());

            // �إ߳��O���
            colDeptGroup.Items.Clear();
            colDeptGroup.Items.Add("");
            colDeptGroup.Items.AddRange((from data in _DeptGroupName_IDDict.Keys orderby data select data).ToArray());
        }

        void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _delRecList.Clear();
            _oldData.Clear();
            // ���o�Ҧ���O,�̬�O�N�X�Ƨ�
            _deptList = (from data in SHDepartment.SelectAll() orderby data.Code select data).ToList();

            foreach (SHDepartmentRecord rec in _deptList)
            {
                SHDepartmentRecord recData = new SHDepartmentRecord();
                recData.ID = rec.ID;
                recData.Code = rec.Code;
                recData.FullName = rec.FullName;
                recData.RefTeacherID = rec.RefTeacherID;
                _oldData.Add(recData);
            }

            // �@�몬�A�Юv,���ޫإ�
            _TeacherID_NameDict.Clear();
            _TeacherName_IDDict.Clear();
            List<SHTeacherRecord> tRecList = (from data in SHTeacher.SelectAll() where data.Status == K12.Data.TeacherRecord.TeacherStatus.�@�� select data).ToList();
            foreach (SHTeacherRecord tr in tRecList)
            {
                string tName = tr.Name;
                if (!string.IsNullOrWhiteSpace(tr.Nickname))
                    tName += "(" + tr.Nickname + ")";

                _TeacherID_NameDict.Add(tr.ID, tName);
                if (!_TeacherName_IDDict.ContainsKey(tName))
                    _TeacherName_IDDict.Add(tName, tr.ID);
            }

            // ���O�W��ID���ޫإ�
            _DeptGroupID_NameDict.Clear();
            _DeptGroupName_IDDict.Clear();
            List<SHDeptGroupRecord> dGRecList = (from data in SHDeptGroup.SelectAll() select data).ToList();
            foreach (SHDeptGroupRecord tr in dGRecList)
            {
                string tName = tr.Name;

                _DeptGroupID_NameDict.Add(tr.ID, tName);
                if (!_DeptGroupName_IDDict.ContainsKey(tName))
                    _DeptGroupName_IDDict.Add(tName, tr.ID);
            }


            // �Z�Ũϥά�OID
            _classUseDeptIDList.Clear();
            foreach (SHClassRecord rec in SHClass.SelectAll())
            {
                if (!string.IsNullOrWhiteSpace(rec.RefDepartmentID))
                    if (!_classUseDeptIDList.Contains(rec.RefDepartmentID))
                        _classUseDeptIDList.Add(rec.RefDepartmentID);
            }



        }

        private void DeptSetup_Load(object sender, EventArgs e)
        {
            _bgWorker.RunWorkerAsync();            

        }


        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CheckCodeName()
        {
            _checkDeptCode.Clear();
            _checkDeptName.Clear();
            foreach (DataGridViewRow dr in dgDept.Rows)
            {
                if (dr.IsNewRow)
                    continue;
                //if (dr.Cells[colCode.Index].Value != null)
                //{
                //    string code = dr.Cells[colCode.Index].Value.ToString();
                //    if (!string.IsNullOrEmpty(code))
                //    {
                //        if (_checkDeptCode.Contains(code))
                //            dr.Cells[colCode.Index].ErrorText = "��O�N�X����!";
                //        else
                //            _checkDeptCode.Add(code);
                //    }
                //}

                if (dr.Cells[colChiName.Index].Value != null)
                {
                    string name = dr.Cells[colChiName.Index].Value.ToString();
                    if (_checkDeptName.Contains(name))
                        dr.Cells[colChiName.Index].ErrorText = "��O�W�٭���!";
                    else
                        _checkDeptName.Add(name);
                }
                else
                    dr.Cells[colChiName.Index].ErrorText = "��O�W�٤���ť�!";
            }        
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool checkError = false;                        
            CheckCodeName();
            foreach (DataGridViewRow dr in dgDept.Rows)
            {
                foreach (DataGridViewCell cel in dr.Cells)
                    if (cel.ErrorText != "")
                        checkError = true;            
            }

            if (checkError)
            {
                MsgBox.Show("��J��Ʀ��~�A�Эץ���A���x�s�C", "���ҥ���", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;            
            }

            // �R���ݭn�R��������
            if (_delRecList.Count > 0)
            {
                // log
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("== �R����O ==");
                foreach (SHDepartmentRecord rec in _delRecList)
                {
                    sb.AppendLine("��O�N�X�G" + rec.Code);
                    sb.AppendLine("��O�W�١G" + rec.FullName);
                    if (_TeacherID_NameDict.ContainsKey(rec.RefTeacherID))
                        sb.AppendLine("��D���G" + _TeacherID_NameDict[rec.RefTeacherID]);
                    if (_DeptGroupID_NameDict.ContainsKey(rec.RefDeptGroupID))
                        sb.AppendLine("���O�G" + _DeptGroupID_NameDict[rec.RefDeptGroupID]);
                    sb.AppendLine();
                }

                FISCA.LogAgent.ApplicationLog.Log("�аȧ@�~>��O��Ӻ޲z","�R��", sb.ToString());

                SHDepartment.Delete(_delRecList);
            }

            List<SHDepartmentRecord> insertList = new List<SHDepartmentRecord>();
            List<SHDepartmentRecord> updateList = new List<SHDepartmentRecord>();

            foreach (DataGridViewRow dr in dgDept.Rows)
            {
                if (dr.IsNewRow)
                    continue;
                bool isNew = false;
                SHDepartmentRecord rec = dr.Tag as SHDepartmentRecord;
                if (rec == null)
                {
                    rec = new SHDepartmentRecord();
                    isNew = true;
                }
                if (dr.Cells[colCode.Index].Value == null)
                    rec.Code = "";
                else
                    rec.Code = dr.Cells[colCode.Index].Value.ToString();

                rec.FullName = dr.Cells[colChiName.Index].Value.ToString();
                string tName="";
                if(dr.Cells[colDeptTeacher.Index].Value !=null)
                    tName=dr.Cells[colDeptTeacher.Index].Value.ToString();
                if (_TeacherName_IDDict.ContainsKey(tName))
                    rec.RefTeacherID = _TeacherName_IDDict[tName];

                string dGName = "";
                if (dr.Cells[colDeptGroup.Index].Value != null)
                    dGName = dr.Cells[colDeptGroup.Index].Value.ToString();
                if (_DeptGroupName_IDDict.ContainsKey(dGName))
                    rec.RefDeptGroupID = _DeptGroupName_IDDict[dGName];

                // ��n�M�Ÿ��
                if (string.IsNullOrEmpty(tName))
                    rec.RefTeacherID = "";

                if (string.IsNullOrEmpty(dGName))
                    rec.RefDeptGroupID = "";

                if (isNew)                
                    insertList.Add(rec);
                
                else
                    updateList.Add(rec);

            }

            if (insertList.Count > 0)
            {
                // log
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("== �s�W��O ==");
                foreach (SHDepartmentRecord rec in insertList)
                {
                    sb.AppendLine("��O�N�X�G" + rec.Code);
                    sb.AppendLine("��O�W�١G" + rec.FullName);
                    if(_TeacherID_NameDict.ContainsKey(rec.RefTeacherID))
                        sb.AppendLine("��D���G" + _TeacherID_NameDict[rec.RefTeacherID]);
                    if (_DeptGroupID_NameDict.ContainsKey(rec.RefDeptGroupID))
                        sb.AppendLine("���O�G" + _DeptGroupID_NameDict[rec.RefDeptGroupID]);
                    sb.AppendLine();
                }
                FISCA.LogAgent.ApplicationLog.Log("�аȧ@�~>��O��Ӻ޲z","�s�W", sb.ToString());
                SHDepartment.Insert(insertList);
            }
            if (updateList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sbs = null;
                StringBuilder sbs1 = null;
                sb.AppendLine("== �ק��O ==");
                
                // log
                foreach (SHDepartmentRecord recNew in updateList)
                {
                    sbs = new StringBuilder();
                    foreach (SHDepartmentRecord rec in _oldData.Where(x => x.ID == recNew.ID))
                    {                        
                        sbs1 = new StringBuilder();                      
                        if (rec.Code != recNew.Code)
                            sbs1.AppendLine("��O�N�X�G�� �u " + rec.Code + " �v�ק令�u " + recNew.Code + " �v");

                        if (rec.FullName != recNew.FullName)
                            sbs1.AppendLine("��O�W�١G�� �u " + rec.FullName + " �v�ק令�u " + recNew.FullName + " �v");

                        if (rec.RefTeacherID != recNew.RefTeacherID)
                        {
                            string t1 = "";
                            if (_TeacherID_NameDict.ContainsKey(rec.RefTeacherID))
                                t1 = _TeacherID_NameDict[rec.RefTeacherID];

                            string t2 = "";
                            if (_TeacherID_NameDict.ContainsKey(recNew.RefTeacherID))
                                t2 = _TeacherID_NameDict[recNew.RefTeacherID];

                            sbs1.AppendLine("��D���G�� �u " + t1 + " �v�ק令�u " + t2 + " �v");
                        }
                        if (rec.RefDeptGroupID != recNew.RefDeptGroupID)
                        {
                            string d1 = "";
                            if (rec.RefDeptGroupID == null)
                                d1 = "";
                            else
                            {
                                if (_DeptGroupID_NameDict.ContainsKey(rec.RefDeptGroupID))
                                    d1 = _DeptGroupID_NameDict[rec.RefDeptGroupID];
                            }
                            string d2 = "";
                            if (_DeptGroupID_NameDict.ContainsKey(recNew.RefDeptGroupID))
                                d2 = _DeptGroupID_NameDict[recNew.RefDeptGroupID];
                            sbs1.AppendLine("���O�G�� �u " + d1 + " �v�ק令�u " + d2 + " �v");
                        }


                        if (sbs1.Length > 0)
                        {
                           sbs.AppendLine("== ���O�N�X�G" + rec.Code + ",���O�W�١G" + rec.FullName + "==");
                           sbs.Append(sbs1.ToString());
                        }
                    }
                    if(sbs.Length>0)
                        sb.Append(sbs.ToString());
                }
                if (sb.Length > 0)          
                {   
                    FISCA.LogAgent.ApplicationLog.Log("�аȧ@�~>��O��Ӻ޲z","�ק�", sb.ToString());                
                }
                SHDepartment.Update(updateList);
            }
            this.Close();
        }
                

        private void dgDept_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Tag == null)
                return;

            SHDepartmentRecord dept = e.Row.Tag as SHDepartmentRecord;
            if (_classUseDeptIDList.Contains(dept.ID))
            {
                MsgBox.Show("�w���Z�ŨϥΡi" + dept.FullName + "�j�A�L�k�R���I", "����", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                e.Cancel = true;
            }

            _delRecList.Add(dept);
        }

        private void dgDept_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            //dgDept.EndEdit();
           
            // �ˬd�ť�            
            if (dgDept.CurrentCell.ColumnIndex == colChiName.Index)
            {
                dgDept.CurrentCell.ErrorText = "";
                if (dgDept.CurrentCell.Value == null)
                {
                    dgDept.CurrentCell.ErrorText = "���i�ť�!";
                }
                else
                { 
                    if(string.IsNullOrWhiteSpace(dgDept.CurrentCell.Value.ToString()))
                        dgDept.CurrentCell.ErrorText = "���i�ť�!";         
                }
            }

            // �ˬd��D��
            if (dgDept.CurrentCell.ColumnIndex == colDeptTeacher.Index)
            {
                dgDept.EndEdit();
                dgDept.CurrentCell.ErrorText = "";
                if (dgDept.CurrentCell.Value != null)
                { 
                    if(!_TeacherName_IDDict.ContainsKey(dgDept.CurrentCell.Value.ToString()))
                        dgDept.CurrentCell.ErrorText="�Юv���b�t�Τ�";
                }
                dgDept.BeginEdit(false);
            }

            // �ˬd���O
            if (dgDept.CurrentCell.ColumnIndex == colDeptGroup.Index)
            {
                dgDept.EndEdit();
                dgDept.CurrentCell.ErrorText = "";
                if (dgDept.CurrentCell.Value != null)
                {
                    if (!_DeptGroupName_IDDict.ContainsKey(dgDept.CurrentCell.Value.ToString()))
                        dgDept.CurrentCell.ErrorText = "���O���b�t�Τ�";
                }
                dgDept.BeginEdit(false);
            }

            // dgDept.BeginEdit(false);           

        }
    }
}