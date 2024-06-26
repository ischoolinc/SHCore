using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using DevComponents.DotNetBar;
using FISCA.DSAUtil;
using SmartSchool.AccessControl;

namespace SmartSchool.UserInfomation
{
    [FeatureCode("System0040")]
    public partial class UserInfoManager : BaseForm
    {
        private ErrorProvider _errorProvider;

        public UserInfoManager()
        {
            InitializeComponent();
            _errorProvider = new ErrorProvider();
            lblUserid.Text = "【 " + CurrentUser.Instance.UserName + " 】";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DoubleCheck();
            bool valid = true;
            foreach (Control control in Controls)
            {
                if (_errorProvider.GetError(control) != string.Empty)
                    valid = false;
            }
            if (!valid)
            {
                MsgBox.Show("密碼資料有誤，請先修正後再行儲存！");
                return;
            }

            try
            {
                //計算密碼雜~!
                SmartSchool.Feature.Personal.ChangePassword(PasswordHash.Compute(txtPassword.Text));
            }
            catch (Exception ex)
            {
                MsgBox.Show("密碼變更失敗 :" + ex.Message);
                return;
            }
            string accesspoint = CurrentUser.Instance.AccessPoint;
            string username = CurrentUser.Instance.UserName;
            try
            {
                //CurrentUser.Instance.SetConnection(accesspoint, username, txtPassword.Text);
                //CurrentUser.Instance.SetConnection(accesspoint, username, txtPassword.Text);
            }
            catch (Exception ex)
            {
                MsgBox.Show("重新建立連線失敗 : " + ex.Message);
                return;
            }
            MsgBox.Show("密碼變更完成！");
            this.Close();
        }

        private void txtPassword_Validated(object sender, EventArgs e)
        {
            _errorProvider.SetError(txtPassword, string.Empty);
            if (txtPassword.Text == string.Empty)
                _errorProvider.SetError(txtPassword, "密碼不可空白！");

            else if (txtPassword.Text.Length < 4)
                _errorProvider.SetError(txtPassword, "密碼長度不可少於4碼！");
        }

        private void DoubleCheck()
        {
            _errorProvider.SetError(txtConfirm, string.Empty);
            if (txtConfirm.Text == string.Empty)
                _errorProvider.SetError(txtConfirm, "請輸入確認密碼！");
            else if (txtConfirm.Text != txtPassword.Text)
                _errorProvider.SetError(txtConfirm, "確認密碼與新密碼不符！");
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}