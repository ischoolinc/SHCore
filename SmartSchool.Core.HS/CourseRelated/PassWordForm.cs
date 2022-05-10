using FISCA.Presentation.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Authentication;

namespace SmartSchool.CourseRelated
{
    public partial class PassWordForm : BaseForm
    {
        public PassWordForm()
        {
            InitializeComponent();
        }

        private void PassWordForm_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.MinimumSize = this.Size;
            lblUserName.Text = DSAServices.UserAccount;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                MsgBox.Show("請輸入密碼!");
                return;
            }

            bool pass = false;
            try
            {
                pass = DSAServices.ConfirmPassword(txtPassword.Text, null);
            }
            catch (Exception ex)
            {
                MsgBox.Show(FISCA.ErrorReport.Generate(ex));
                return;
            }

            if (pass)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MsgBox.Show("密碼錯誤。");
                this.DialogResult = DialogResult.Cancel;
                return;
            }

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
