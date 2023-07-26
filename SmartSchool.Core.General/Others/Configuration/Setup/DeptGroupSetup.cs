using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SmartSchool.Common;
using System.Xml;
using SHSchool.Data;
using System.Reflection.Emit;
using DevComponents.DotNetBar.Controls;
using System.Text;
using System.Linq;
using FISCA.Data;
using System.Data;

namespace SmartSchool.Others.Configuration.Setup
{
    public partial class DeptGroupSetup : BaseForm
    {
        BackgroundWorker _bgWorker; // 背景模式

        List<SHDeptGroupRecord> _deptGroupList = new List<SHDeptGroupRecord>();

        List<SHDeptGroupRecord> _deptGroupDelList = new List<SHDeptGroupRecord>();

        List<SHDeptGroupRecord> _deptGroupOldList = new List<SHDeptGroupRecord>();

        // 科別使用部別ID
        List<string> _deptUseDeptGroupIDList = new List<string>();

        private bool _isValided = true;

        public DeptGroupSetup()
        {
            InitializeComponent();
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += new DoWorkEventHandler(_bgWorker_DoWork);
            _bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_bgWorker_RunWorkerCompleted);
        }

        // 處理資料顯示
        void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            dgDeptGroup.Rows.Clear();
            foreach (SHDeptGroupRecord rec in _deptGroupList)
            {
                int rowIdx = dgDeptGroup.Rows.Add();
                dgDeptGroup.Rows[rowIdx].Tag = rec;
                dgDeptGroup.Rows[rowIdx].Cells[colDeptGroupName.Index].Value = rec.Name;
                dgDeptGroup.Rows[rowIdx].Cells[colDeptGroupCode.Index].Value = rec.Code;

                _deptGroupOldList.Add(rec);
            }
        }

        void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _deptGroupList = (from data in SHDeptGroup.SelectAll() orderby data.ID select data).ToList();
            //loadDeptGroup();
            // 科別使用部別ID
            _deptUseDeptGroupIDList.Clear();
            foreach (SHDepartmentRecord rec in SHDepartment.SelectAll())
            {
                if (!string.IsNullOrWhiteSpace(rec.RefDeptGroupID))
                    if (!_deptUseDeptGroupIDList.Contains(rec.RefDeptGroupID))
                        _deptUseDeptGroupIDList.Add(rec.RefDeptGroupID);
            }
        }


        private bool ValidateList(int colIdx)
        {
            dgDeptGroup.EndEdit();
            bool valid = true;

            List<string> values = new List<string>();

            foreach (DataGridViewRow row in dgDeptGroup.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells[colIdx].Value == null)
                {
                    row.Cells[colIdx].ErrorText = "不能為空白";
                    valid = false;
                    break;
                }
                else
                {
                    string value = row.Cells[colIdx].Value?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (values.Contains(value))
                        {
                            row.Cells[colIdx].ErrorText = "不可重覆";
                            valid = false;
                        }
                        else
                            values.Add(value);

                    }
                    else
                        row.Cells[colIdx].ErrorText = "";
                }
            }
            return valid;
        }



        private void btnSave_Click(object sender, EventArgs e)
        {
            bool checkError = false;
            foreach (DataGridViewRow dr in dgDeptGroup.Rows)
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
            if (_deptGroupDelList.Count > 0)
            {
                // log
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("== 刪除部別 ==");
                foreach (SHDeptGroupRecord rec in _deptGroupDelList)
                {
                    sb.AppendLine("部別代碼：" + rec.Code);
                    sb.AppendLine("部別名稱：" + rec.Name);
                    sb.AppendLine();
                }

                FISCA.LogAgent.ApplicationLog.Log("教務作業>部別管理", "刪除", sb.ToString());

                SHDeptGroup.Delete(_deptGroupDelList);
            }

            List<SHDeptGroupRecord> insertList = new List<SHDeptGroupRecord>();
            List<SHDeptGroupRecord> updateList = new List<SHDeptGroupRecord>();

            foreach (DataGridViewRow dr in dgDeptGroup.Rows)
            {
                if (dr.IsNewRow)
                    continue;
                bool isNew = false;
                SHDeptGroupRecord rec = dr.Tag as SHDeptGroupRecord;
                if (rec == null)
                {
                    rec = new SHDeptGroupRecord();
                    isNew = true;
                }
                rec.Code = dr.Cells[colDeptGroupCode.Index].Value.ToString();
                rec.Name = dr.Cells[colDeptGroupName.Index].Value.ToString();
                rec.Comment = "";

                if (isNew)
                    insertList.Add(rec);

                else
                    updateList.Add(rec);
            }
            if (insertList.Count > 0)
            {
                // log
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("== 新增部別 ==");
                foreach (SHDeptGroupRecord rec in insertList)
                {
                    sb.AppendLine("部別代碼：" + rec.Code);
                    sb.AppendLine("部別名稱：" + rec.Name);
                    sb.AppendLine();
                }
                FISCA.LogAgent.ApplicationLog.Log("教務作業>部別管理", "新增", sb.ToString());
                SHDeptGroup.Insert(insertList);
            }

            if (updateList.Count > 0)
            {
                // log
                StringBuilder sb = new StringBuilder();
                StringBuilder sbs;
                List<SHDeptGroupRecord> realUpdateRecords = new List<SHDeptGroupRecord>();

                sb.AppendLine("== 修改部別 ==");
                // log
                foreach (SHDeptGroupRecord recNew in updateList)
                {
                    sbs = new StringBuilder();
                    foreach (SHDeptGroupRecord rec in _deptGroupOldList.Where(x => x.ID == recNew.ID))
                    {
                        if (rec.Code != recNew.Code)
                            sbs.AppendLine("部別代碼：由 「 " + rec.Code + " 」修改成「 " + recNew.Code + " 」");

                        if (rec.Name != recNew.Name)
                            sbs.AppendLine("部別名稱：由 「 " + rec.Name + " 」修改成「 " + recNew.Name + " 」");
                        if(!(rec.Code == recNew.Code && rec.Name == recNew.Name))
                            realUpdateRecords.Add(recNew);   
                            

                    }
                    if (sbs.Length > 0)
                        sb.Append(sbs.ToString());
                }
                if (sb.Length > 0)
                {
                    FISCA.LogAgent.ApplicationLog.Log("教務作業>科別對照管理", "修改", sb.ToString());
                }
                if(realUpdateRecords.Count > 0)
                {
                    SHDeptGroup.Update(realUpdateRecords);
                }
            }
            this.Close();

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgDeptGroup_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void dgDeptGroup_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (!ValidateList(e.ColumnIndex)) _isValided = false;
            else _isValided = true;
        }

        private void dgDeptGroup_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            dgDeptGroup.ImeMode = ImeMode.Off;  //關閉輸入法,避免輸入法影響到DataGridView的編輯

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgDeptGroup.SelectedCells.Count == 1)
            {
                dgDeptGroup.BeginEdit(true);
                dgDeptGroup.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "";

            }
        }

        private void dgDeptGroup_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            dgDeptGroup.EndEdit();
        }



        private void DeptGroupSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isValided)
            {
                if (MsgBox.Show("資料尚未儲存，您確定要離開？", "", MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
        }


        private void dgDeptGroup_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Tag == null)
                return;

            SHDeptGroupRecord dept = e.Row.Tag as SHDeptGroupRecord;
        //    if (_deptUseDeptGroupIDList.Contains(dept.ID))
        //    {
        //        MsgBox.Show("已有科別使用【" + dept.Name + "】，無法刪除！", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        //        e.Cancel = true;
        //    }

            _deptGroupDelList.Add(dept);
            Console.WriteLine("Delete ID:" + dept.ID.ToString());
        }

        private void dgDeptGroup_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }


        private void DeptGroupSetup_Load(object sender, EventArgs e)
        {
            _bgWorker.RunWorkerAsync();

        }
    }
}
