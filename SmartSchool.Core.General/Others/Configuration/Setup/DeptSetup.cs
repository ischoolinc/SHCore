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
        // 需要被刪除
        List<SHDepartmentRecord> _delRecList = new List<SHDepartmentRecord>();
        // 班級使用科別ID
        List<string> _classUseDeptIDList = new List<string>();

        // 科別隸屬部別ID
        List<string> _deptUseDeptGroupIDList = new List<string>();

        // 比對用
        Dictionary<string, string> _TeacherID_NameDict = new Dictionary<string, string>();
        Dictionary<string, string> _TeacherName_IDDict = new Dictionary<string, string>();

        // 比對用
        Dictionary<string, string> _DeptGroupID_NameDict = new Dictionary<string, string>();
        Dictionary<string, string> _DeptGroupName_IDDict = new Dictionary<string, string>();


        List<SHDepartmentRecord> _oldData = new List<SHDepartmentRecord>();
        // 重複時檢查
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

            // 建立教師選單
            colDeptTeacher.Items.Clear();
            colDeptTeacher.Items.Add("");
            colDeptTeacher.Items.AddRange((from data in _TeacherName_IDDict.Keys orderby data select data).ToArray());

            // 建立部別選單
            colDeptGroup.Items.Clear();
            colDeptGroup.Items.Add("");
            colDeptGroup.Items.AddRange((from data in _DeptGroupName_IDDict.Keys orderby data select data).ToArray());
        }

        void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _delRecList.Clear();
            _oldData.Clear();
            // 取得所有科別,依科別代碼排序
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

            // 一般狀態教師,索引建立
            _TeacherID_NameDict.Clear();
            _TeacherName_IDDict.Clear();
            List<SHTeacherRecord> tRecList = (from data in SHTeacher.SelectAll() where data.Status == K12.Data.TeacherRecord.TeacherStatus.一般 select data).ToList();
            foreach (SHTeacherRecord tr in tRecList)
            {
                string tName = tr.Name;
                if (!string.IsNullOrWhiteSpace(tr.Nickname))
                    tName += "(" + tr.Nickname + ")";

                _TeacherID_NameDict.Add(tr.ID, tName);
                if (!_TeacherName_IDDict.ContainsKey(tName))
                    _TeacherName_IDDict.Add(tName, tr.ID);
            }

            // 部別名稱ID索引建立
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


            // 班級使用科別ID
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
                //            dr.Cells[colCode.Index].ErrorText = "科別代碼重複!";
                //        else
                //            _checkDeptCode.Add(code);
                //    }
                //}

                if (dr.Cells[colChiName.Index].Value != null)
                {
                    string name = dr.Cells[colChiName.Index].Value.ToString();
                    if (_checkDeptName.Contains(name))
                        dr.Cells[colChiName.Index].ErrorText = "科別名稱重複!";
                    else
                        _checkDeptName.Add(name);
                }
                else
                    dr.Cells[colChiName.Index].ErrorText = "科別名稱不能空白!";
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
                MsgBox.Show("輸入資料有誤，請修正後再行儲存。", "驗證失敗", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;            
            }

            // 刪除需要刪除的紀錄
            if (_delRecList.Count > 0)
            {
                // log
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("== 刪除科別 ==");
                foreach (SHDepartmentRecord rec in _delRecList)
                {
                    sb.AppendLine("科別代碼：" + rec.Code);
                    sb.AppendLine("科別名稱：" + rec.FullName);
                    if (_TeacherID_NameDict.ContainsKey(rec.RefTeacherID))
                        sb.AppendLine("科主任：" + _TeacherID_NameDict[rec.RefTeacherID]);
                    if (_DeptGroupID_NameDict.ContainsKey(rec.RefDeptGroupID))
                        sb.AppendLine("部別：" + _DeptGroupID_NameDict[rec.RefDeptGroupID]);
                    sb.AppendLine();
                }

                FISCA.LogAgent.ApplicationLog.Log("教務作業>科別對照管理","刪除", sb.ToString());

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

                // 當要清空資料
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
                sb.AppendLine("== 新增科別 ==");
                foreach (SHDepartmentRecord rec in insertList)
                {
                    sb.AppendLine("科別代碼：" + rec.Code);
                    sb.AppendLine("科別名稱：" + rec.FullName);
                    if(_TeacherID_NameDict.ContainsKey(rec.RefTeacherID))
                        sb.AppendLine("科主任：" + _TeacherID_NameDict[rec.RefTeacherID]);
                    if (_DeptGroupID_NameDict.ContainsKey(rec.RefDeptGroupID))
                        sb.AppendLine("部別：" + _DeptGroupID_NameDict[rec.RefDeptGroupID]);
                    sb.AppendLine();
                }
                FISCA.LogAgent.ApplicationLog.Log("教務作業>科別對照管理","新增", sb.ToString());
                SHDepartment.Insert(insertList);
            }
            if (updateList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sbs = null;
                StringBuilder sbs1 = null;
                sb.AppendLine("== 修改科別 ==");
                
                // log
                foreach (SHDepartmentRecord recNew in updateList)
                {
                    sbs = new StringBuilder();
                    foreach (SHDepartmentRecord rec in _oldData.Where(x => x.ID == recNew.ID))
                    {                        
                        sbs1 = new StringBuilder();                      
                        if (rec.Code != recNew.Code)
                            sbs1.AppendLine("科別代碼：由 「 " + rec.Code + " 」修改成「 " + recNew.Code + " 」");

                        if (rec.FullName != recNew.FullName)
                            sbs1.AppendLine("科別名稱：由 「 " + rec.FullName + " 」修改成「 " + recNew.FullName + " 」");

                        if (rec.RefTeacherID != recNew.RefTeacherID)
                        {
                            string t1 = "";
                            if (_TeacherID_NameDict.ContainsKey(rec.RefTeacherID))
                                t1 = _TeacherID_NameDict[rec.RefTeacherID];

                            string t2 = "";
                            if (_TeacherID_NameDict.ContainsKey(recNew.RefTeacherID))
                                t2 = _TeacherID_NameDict[recNew.RefTeacherID];

                            sbs1.AppendLine("科主任：由 「 " + t1 + " 」修改成「 " + t2 + " 」");
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
                            sbs1.AppendLine("部別：由 「 " + d1 + " 」修改成「 " + d2 + " 」");
                        }


                        if (sbs1.Length > 0)
                        {
                           sbs.AppendLine("== 原科別代碼：" + rec.Code + ",原科別名稱：" + rec.FullName + "==");
                           sbs.Append(sbs1.ToString());
                        }
                    }
                    if(sbs.Length>0)
                        sb.Append(sbs.ToString());
                }
                if (sb.Length > 0)          
                {   
                    FISCA.LogAgent.ApplicationLog.Log("教務作業>科別對照管理","修改", sb.ToString());                
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
                MsgBox.Show("已有班級使用【" + dept.FullName + "】，無法刪除！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                e.Cancel = true;
            }

            _delRecList.Add(dept);
        }

        private void dgDept_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            //dgDept.EndEdit();
           
            // 檢查空白            
            if (dgDept.CurrentCell.ColumnIndex == colChiName.Index)
            {
                dgDept.CurrentCell.ErrorText = "";
                if (dgDept.CurrentCell.Value == null)
                {
                    dgDept.CurrentCell.ErrorText = "不可空白!";
                }
                else
                { 
                    if(string.IsNullOrWhiteSpace(dgDept.CurrentCell.Value.ToString()))
                        dgDept.CurrentCell.ErrorText = "不可空白!";         
                }
            }

            // 檢查科主任
            if (dgDept.CurrentCell.ColumnIndex == colDeptTeacher.Index)
            {
                dgDept.EndEdit();
                dgDept.CurrentCell.ErrorText = "";
                if (dgDept.CurrentCell.Value != null)
                { 
                    if(!_TeacherName_IDDict.ContainsKey(dgDept.CurrentCell.Value.ToString()))
                        dgDept.CurrentCell.ErrorText="教師不在系統內";
                }
                dgDept.BeginEdit(false);
            }

            // 檢查部別
            if (dgDept.CurrentCell.ColumnIndex == colDeptGroup.Index)
            {
                dgDept.EndEdit();
                dgDept.CurrentCell.ErrorText = "";
                if (dgDept.CurrentCell.Value != null)
                {
                    if (!_DeptGroupName_IDDict.ContainsKey(dgDept.CurrentCell.Value.ToString()))
                        dgDept.CurrentCell.ErrorText = "部別不在系統內";
                }
                dgDept.BeginEdit(false);
            }

            // dgDept.BeginEdit(false);           

        }
    }
}