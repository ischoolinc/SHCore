using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace SmartSchool.CourseRelated.DetailPaneItem
{
    public partial class editPassMakupScoreForm : FISCA.Presentation.Controls.BaseForm
    {
        DataTable dtAttendInfoData = new DataTable();

        public editPassMakupScoreForm()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void editPassMakupScoreForm_Load(object sender, EventArgs e)
        {
            dgData.Rows.Clear();
            dgData.Columns.Clear();
            BindingSource sbi = new BindingSource();
            sbi.DataSource = dtAttendInfoData;
            dgData.DataSource = dtAttendInfoData;
            dgData.AutoGenerateColumns = true;
            for (int i = 0; i < dtAttendInfoData.Columns.Count; i++)
            {
                if (i == 0)
                    dgData.Columns[i].Visible = false;
                else if (i == 2)
                    dgData.Columns[i].Width = 60;
                else if (i == 4 || i == 5)
                    dgData.Columns[i].Width = 70;
                else
                    dgData.Columns[i].Width = 100;
                
                if (i == 1)
                    dgData.Columns[i].Width = 100;

                dgData.Columns[i].DataPropertyName = dtAttendInfoData.Columns[i].ColumnName;
                dgData.Columns[i].HeaderText = dtAttendInfoData.Columns[i].Caption;
            }
            dgData.Refresh();
        }

        public void SetAttendInfoData(DataTable data)
        {
            dtAttendInfoData = data;
        }

        public DataTable GetAttendInfoData()
        {
            return dtAttendInfoData;
        }

        public bool checkData()
        {
            bool value = true;

            foreach (DataGridViewRow drv in dgData.Rows)
            {
                foreach (DataGridViewCell cell in drv.Cells)
                {
                    if (cell.ErrorText != "")
                    {
                        value = false;
                        break;
                    }
                }
            }

            return value;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (checkData())
            {
                // 回到主畫面再一起處理儲存
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                FISCA.Presentation.Controls.MsgBox.Show("資料有問題無法儲存。");
            }
        }

        private void dgData_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int num;

            // 檢查畫面上及格與補考標準是否是數字
            if (e.ColumnIndex > 3 && e.RowIndex > -1)
            {
                if (dgData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                {
                    if (int.TryParse(dgData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out num))
                    {
                        dgData.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "";
                    }
                    else
                    {
                        dgData.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "必須整數!";
                    }
                }
            }
        }
    }
}
