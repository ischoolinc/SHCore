using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.IO;
using SmartSchool.SysAdmin;
using FISCA.DSAUtil;
using System.Security.Cryptography;
using SmartSchool.Common;
using Framework;

namespace SmartSchool
{
    internal partial class LoginForm : //Office2007RibbonForm
        Form
    {
        private DSNSForm _dsnsForm;
        public event EventHandler LoginSucceed;
        private string _user_name;

        public LoginForm()
        {
            InitializeComponent();

            try
            {
                string fileName = Path.Combine(Application.StartupPath, "LoginLogo.png");
                if (File.Exists(fileName))
                    pictureBox1.Image = new Bitmap(fileName);
                else
                {
                    fileName = Path.Combine(Application.StartupPath, "LoginLogo.jpg");
                    if (File.Exists(fileName))
                        pictureBox1.Image = new Bitmap(fileName);
                }
            }
            catch { }

            _dsnsForm = new DSNSForm(this);
            //SetCaption();

            //if (string.IsNullOrEmpty(_dsnsForm.GetDomainName()))
            //{
            //    _dsnsForm.ShowDialog();
            //    SetCaption();
            //}

            List<string> loginNames = _dsnsForm.GetHistoryLoginNames();

            foreach (string name in loginNames)
                cboAccount.Items.Add(name);

            if (cboAccount.Items.Count > 0)
                cboAccount.SelectedIndex = 0;
        }

        private static LicenseInfo _license;
        public static LicenseInfo License
        {
            get { return _license; }
        }

        private void SetCaption(string msg)
        {
            Text = string.Format("�ϥΪ̵n�J ({0})", msg);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cboAccount.Text))
            {
                cboAccount.Select(0, 0);
                txtPassword.Focus();
                txtPassword.Select();
            }
            else
            {
                cboAccount.Focus();
            }

            Activate();

            LicenseInfo lic = new LicenseInfo();

            if (lic.LicenseExists())
            {
                LoadLicense(lic);
                lnkViewLicense.Enabled = true;
            }
            else
            {
                SetCaption("���w�˱��v��");
                txtPassword.Enabled = false;
                cboAccount.Enabled = false;
                btnLogin.Enabled = false;
                checkBoxX1.Enabled = false;
            }
        }

        private void lnkSetDsns_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //_dsnsForm.ShowDialog();
            //SetCaption();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboAccount_TextChanged(object sender, EventArgs e)
        {
            //cboAccount.ForeColor = Color.Black;
        }

        private void cboAccount_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:

                    if (cboAccount.SelectedItem == null) return;

                    string value = cboAccount.SelectedItem.ToString();
                    cboAccount.Items.RemoveAt(cboAccount.SelectedIndex);
                    _dsnsForm.RemoveHistoryLoginName(value);
                    break;
                case Keys.Enter:
                    Login();
                    break;
                default:
                    break;
            }
        }

        private async void Login()
        {

            if (cboAccount.Text.Length > 0 && txtPassword.Text.Length > 0)
            {
                this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                string originalText = btnLogin.Text;
                btnLogin.Text = "登入中...";
                cboAccount.Enabled = false;
                txtPassword.Enabled = false;
                btnLogin.Enabled = false;

                string account = cboAccount.Text;
                string pwd = txtPassword.Text;

                await System.Threading.Tasks.Task.Run(() =>
                {
                    if ( CurrentUser.Instance.SetConnection(_license) )
                        //CurrentUser.Instance.CheckUserPassword(cboAccount.Text, txtPassword.Text);
                        DSAServices.Login(account, pwd);
                });

                this.Cursor = System.Windows.Forms.Cursors.Default;
                cboAccount.Enabled = true;
                txtPassword.Enabled = true;
                btnLogin.Enabled = true;
                btnLogin.Text = originalText;

                if (CurrentUser.Instance.IsLogined)
                {
                    if (checkBoxX1.Checked)
                        _dsnsForm.AddHistoryLoginName(account);
                    if (LoginSucceed != null)
                    {
                        this.Hide();
                        Application.DoEvents();
                        LoginSucceed(this, null);
                    }
                    this.Close();
                }
                else
                    this.txtPassword.SelectAll();
            }
            else
            {
                Framework.MsgBox.Show("пJbKX");
            }
        }

        public string UserName
        {
            get { return _user_name; }
        }

        public string AccessPoint
        {
            get
            {
                if (_license != null)
                    return _license.AccessPoint;
                else
                    return string.Empty;
            }
        }

        private void lnkSysAdmin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SetLicense sl = new SetLicense();

            DialogResult dr = sl.ShowDialog();
            sl.Close();

            if (dr == DialogResult.OK)
            {
                LoadLicense(new LicenseInfo());
                lnkViewLicense.Enabled = true;
            }
        }


        private void lnkViewLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LicenseInfoForm form = new LicenseInfoForm(_license);
            form.ShowDialog();
        }

        private void LoadLicense(LicenseInfo license)
        {
            try
            {
                _license = license;
                license.DecryptLicense();
                SetCaption(string.Format("�w���v�n�J {0}", license.AccessPoint));

                txtPassword.Enabled = true;
                cboAccount.Enabled = true;
                btnLogin.Enabled = true;
                checkBoxX1.Enabled = true;
            }
            catch (Exception ex)
            {
                _license = null;
                SetCaption(ex.Message);
                txtPassword.Enabled = false;
                cboAccount.Enabled = false;
                btnLogin.Enabled = false;
                checkBoxX1.Enabled = false;
            }
        }

        private string ArrangeExceptionMessage(Exception ex)
        {
            string msg = string.Empty;
            int level = 0;
            Exception temp = ex;

            while (temp != null)
            {
                if (msg != string.Empty)
                    msg += "\n".PadRight(level * 5, ' ') + temp.Message;
                else
                    msg = temp.Message;

                temp = temp.InnerException;
                level++;
            }

            return msg;
        }

        private void cboAccount_Enter(object sender, EventArgs e)
        {
            //cboAccount.SelectAll(
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            lnkViewLicense.Visible = true;
        }
    }
}