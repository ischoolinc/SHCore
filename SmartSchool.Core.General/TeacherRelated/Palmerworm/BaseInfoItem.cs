using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SmartSchool.Common;
using SmartSchool.Feature.Teacher;
using FISCA.DSAUtil;
using System.IO;
using DevComponents.DotNetBar;
using SmartSchool.ApplicationLog;
using SmartSchool.AccessControl;
using System.Linq;
using SHSchool.Data;
using SmartSchool.StudentRelated;
using System.Drawing.Imaging;

namespace SmartSchool.TeacherRelated.Palmerworm
{
    [FeatureCode("Content0170")]
    internal partial class BaseInfoItem : PalmerwormItem
    {
        ErrorProvider epName = new ErrorProvider();
        ErrorProvider epNick = new ErrorProvider();
        ErrorProvider epGender = new ErrorProvider();

        private SHTeacherRecord _TeacherRec;
        PermRecLogProcess prlp;

        public BaseInfoItem()
        {
            InitializeComponent();
            Title = "教師基本資料";
        }

        #region Log 用到的物件
        TeacherBaseLogMachine machine = new TeacherBaseLogMachine();
        #endregion
        public override object Clone()
        {
            return new BaseInfoItem();
        }
        public override void Save()
        {
            // 移除快取
            SHTeacher.RemoveAll();

            // 取得教師資料
            List<SHTeacherRecord> TeacherList = SHTeacher.SelectAll().ToList();

            // 讀取非自己同帳號
            List<SHTeacherRecord> HasLoginNameList = TeacherList.Where(x => (x.TALoginName == txtSTLoginAccount.Text && x.ID !=RunningID)).Where(y => y.TALoginName !="").ToList();

            // 讀取非自己同姓名與暱稱
            List<SHTeacherRecord> HasNameAndNickNameList = TeacherList.Where(x => (x.Name == txtName.Text && x.Nickname == txtNickname.Text && x.ID != RunningID)).ToList();

            // 讀取非自己同教師編號-cyn
            List<SHTeacherRecord> HasTeacherNumberList = TeacherList.Where(x => (x.TeacherNumber == txtTeacherNumber.Text && x.ID != RunningID)).Where(y => y.TeacherNumber != "").ToList();


            // 檢查一般狀態
            if (HasLoginNameList.Where(x => x.Status == K12.Data.TeacherRecord.TeacherStatus.一般).Count() > 0)
            {
                FISCA.Presentation.Controls.MsgBox.Show("有相同帳號無法儲存");
                return;           
            }

            if (HasNameAndNickNameList.Where(x => x.Status == K12.Data.TeacherRecord.TeacherStatus.一般).Count() > 0)
            {
                FISCA.Presentation.Controls.MsgBox.Show("有相同姓名或暱稱無法儲存");
                return;            
            }

            if (HasTeacherNumberList.Where(x => x.Status == K12.Data.TeacherRecord.TeacherStatus.一般).Count() > 0)
            {
                FISCA.Presentation.Controls.MsgBox.Show("有重複的「教師編號」，無法儲存。");
                return;
            }

            // 當有刪除狀態修改刪除的
            List<SHTeacherRecord> UpdateTeacherRec = new List<SHTeacherRecord>();

            foreach (SHTeacherRecord TRec in HasLoginNameList.Where(x=>x.Status == K12.Data.TeacherRecord.TeacherStatus.刪除))
            {
                TRec.TALoginName = "";
                UpdateTeacherRec.Add(TRec);
            }

            foreach (SHTeacherRecord TRec in HasNameAndNickNameList.Where(x => x.Status == K12.Data.TeacherRecord.TeacherStatus.刪除))
            {
                TRec.Nickname = TRec.Nickname + TRec.ID;
                UpdateTeacherRec.Add(TRec); 
            }

            if (UpdateTeacherRec.Count > 0)
                SHTeacher.Update(UpdateTeacherRec);


            //K12.Data.TeacherRecord teacherRecord = K12.Data.Teacher.SelectByID(RunningID);
            //teacherRecord.TeacherNumber = "ABC";
            //K12.Data.Teacher.Update(teacherRecord);

            if (IsValid())
            {
                DSXmlHelper helper = new DSXmlHelper("UpdateRequest");
                helper.AddElement("Teacher");
                helper.AddElement("Teacher", "Field");
                foreach (string key in _valueManager.GetDirtyItems().Keys)
                    helper.AddElement("Teacher/Field", key, _valueManager.GetDirtyItems()[key]);
                helper.AddElement("Teacher", "Condition");
                helper.AddElement("Teacher/Condition", "ID", RunningID);

                //計算密碼雜~!
                if (helper.PathExist("Teacher/Field/SmartTeacherPassword"))
                {
                    if (!string.IsNullOrEmpty(helper.GetElement("Teacher/Field/SmartTeacherPassword").InnerText))
                        helper.GetElement("Teacher/Field/SmartTeacherPassword").InnerText = PasswordHash.Compute(helper.GetElement("Teacher/Field/SmartTeacherPassword").InnerText);
                }

                EditTeacher.Update(new DSRequest(helper));

                #region Log

                #region Log 記錄修改後的資料
                machine.AddAfter(label1.Text.Replace("　", ""), txtName.Text);
                machine.AddAfter(label2.Text.Replace("　", ""), cboGender.Text);
                machine.AddAfter(label3.Text.Replace("　", ""), txtIDNumber.Text);
                machine.AddAfter(label4.Text.Replace("　", ""), txtPhone.Text);
                machine.AddAfter(label5.Text.Replace("　", ""), txtEmail.Text);
                machine.AddAfter(label6.Text.Replace("　", ""), txtCategory.Text);
                machine.AddAfter(label7.Text.Replace("　", ""), txtSTLoginAccount.Text);
                machine.AddAfter(label8.Text.Replace("　", ""), txtSTLoginPwd.Text);
                machine.AddAfter(label9.Text.Replace("　", ""), cboAccountType.Text);
                machine.AddAfter(label10.Text.Replace("　", ""), txtNickname.Text);
                machine.AddAfter(label11.Text.Replace("　", ""), txtTeacherNumber.Text);
                #endregion

                #region Log 登入密碼不顯示詳細修改記錄
                machine.HideSomething(label8.Text.Replace("　", ""));
                #endregion

                StringBuilder desc = new StringBuilder("");
                desc.AppendLine("教師姓名：" + Teacher.Instance.Items[RunningID].TeacherName + " ");
                desc.AppendLine(machine.GetDescription());

                CurrentUser.Instance.AppLog.Write(EntityType.Teacher, EntityAction.Update, RunningID, desc.ToString(), "教師基本資料", "");

                #endregion

                Teacher.Instance.InvokTeacherDataChanged(RunningID);
                SaveButtonVisible = false;
            }
            else
            {
                MsgBox.Show("輸入資料有誤，請重新整理後再儲存");
            }
        }

        private bool IsValid()
        {
            bool valid = true;
            foreach (Control c in Controls)
            {
                if (c.Tag != null && c.Tag.ToString() == "error")
                    valid = false;
            }
            return valid;
        }

        protected override object OnBackgroundWorkerWorking()
        {
            return QueryTeacher.GetTeacherDetail(RunningID);
        }

        protected override void OnBackgroundWorkerCompleted(object result)
        {
            cboGender.Tag = null;
            epGender.Clear();
            txtName.Tag = null;
            epName.Clear();
            epNick.Clear();
            errors.SetError(txtSTLoginAccount, "");
            txtSTLoginAccount.Tag = null;

            DSResponse dsrsp = result as DSResponse;
            DSXmlHelper helper = dsrsp.GetContent();

            // 讀取教師資料
            string id = helper.GetElement("Teacher").GetAttribute("ID");
            _TeacherRec = SHTeacher.SelectByID(id);
            prlp = new PermRecLogProcess();


            txtCategory.Text = helper.GetText("Teacher/Category");
            txtEmail.Text = helper.GetText("Teacher/Email");
            txtIDNumber.Text = helper.GetText("Teacher/IDNumber");
            txtName.Text = helper.GetText("Teacher/TeacherName");
            txtNickname.Text = helper.GetText("Teacher/Nickname");
            txtPhone.Text = helper.GetText("Teacher/ContactPhone");
            cboAccountType.Text = helper.GetText("Teacher/RemoteAccount");
            txtSTLoginAccount.Text = helper.GetText("Teacher/SmartTeacherLoginName");
            txtSTLoginPwd.Text = helper.GetText("Teacher/SmartTeacherPassword");
            cboGender.Text = helper.GetText("Teacher/Gender");
            txtTeacherNumber.Text = helper.GetText("Teacher/TeacherNumber");

            string picString = helper.GetText("Teacher/Photo");
            //byte[] bs = Convert.FromBase64String(picString);
            //MemoryStream ms = new MemoryStream(bs);
            //try
            //{
            //    pictureBox.Image = Bitmap.FromStream(ms);
            //}
            //catch (Exception)
            //{
            //    pictureBox.Image = Properties.Resources.studentsPic;
            //}

            if (!string.IsNullOrWhiteSpace(picString))
            {
                pictureBox.Image = Photo.ConvertFromBase64Encoding(picString, pictureBox.Width, pictureBox.Height);
            }
            else
            {
                pictureBox.Image = Properties.Resources.studentsPic;
            }

            _valueManager.AddValue("Category", txtCategory.Text);
            _valueManager.AddValue("Email", txtEmail.Text);
            _valueManager.AddValue("IDNumber", txtIDNumber.Text);
            _valueManager.AddValue("TeacherName", txtName.Text);
            _valueManager.AddValue("Nickname", txtNickname.Text);
            _valueManager.AddValue("ContactPhone", txtPhone.Text);
            _valueManager.AddValue("RemoteAccount", cboAccountType.Text);
            _valueManager.AddValue("SmartTeacherLoginName", txtSTLoginAccount.Text);
            _valueManager.AddValue("SmartTeacherPassword", txtSTLoginPwd.Text);
            _valueManager.AddValue("Gender", cboGender.Text);
            _valueManager.AddValue("TeacherNumber", txtTeacherNumber.Text);
            SaveButtonVisible = false;

            #region Log 記錄修改前的資料
            machine.AddBefore(label1.Text.Replace("　", ""), txtName.Text);
            machine.AddBefore(label2.Text.Replace("　", ""), cboGender.Text);
            machine.AddBefore(label3.Text.Replace("　", ""), txtIDNumber.Text);
            machine.AddBefore(label4.Text.Replace("　", ""), txtPhone.Text);
            machine.AddBefore(label5.Text.Replace("　", ""), txtEmail.Text);
            machine.AddBefore(label6.Text.Replace("　", ""), txtCategory.Text);
            machine.AddBefore(label7.Text.Replace("　", ""), txtSTLoginAccount.Text);
            machine.AddBefore(label8.Text.Replace("　", ""), txtSTLoginPwd.Text);
            machine.AddBefore(label9.Text.Replace("　", ""), cboAccountType.Text);
            machine.AddBefore(label10.Text.Replace("　", ""), txtNickname.Text);
            machine.AddBefore(label11.Text.Replace("　", ""), txtTeacherNumber.Text);
            #endregion
        }

        private void txtSTLoginAccount_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("SmartTeacherLoginName", txtSTLoginAccount.Text);
        }

        private void cboAccountType_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("RemoteAccount", cboAccountType.Text);
        }

        private void txtSTLoginPwd_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("SmartTeacherPassword", txtSTLoginPwd.Text);
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("TeacherName", txtName.Text);
        }

        private void txtNickname_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("Nickname", txtNickname.Text);
        }

        private void txtPhone_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("ContactPhone", txtPhone.Text);
        }

        private void cboGender_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("Gender", cboGender.Text);
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("Email", txtEmail.Text);
        }

        private void txtIDNumber_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("IDNumber", txtIDNumber.Text);
        }

        private void txtCategory_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("Category", txtCategory.Text);
        }

        private void txtTeacherNumber_TextChanged(object sender, EventArgs e)
        {
            OnValueChanged("TeacherNumber", txtTeacherNumber.Text);

        }
        private void txtName_Validated(object sender, EventArgs e)
        {
            try
            {
                epName.Clear();
                txtName.Tag = null;
                epNick.Clear();
                txtNickname.Tag = null;

                if (string.IsNullOrEmpty(txtName.Text))
                {
                    epName.SetError(txtName, "姓名不可空白。");
                    txtName.Tag = "error";
                }

                if (QueryTeacher.NameExists(RunningID, txtName.Text, txtNickname.Text))
                {
                    epName.SetError(txtName, "「姓名+暱稱」不可與其他人相同。");
                    txtName.Tag = "error";
                    epNick.SetError(txtNickname, "「姓名+暱稱」不可與其他人相同。");
                    txtNickname.Tag = "error";
                }
            }
            catch (Exception ex)
            {
                epName.SetError(txtName, string.Format("{0}", ex.Message));
            }

        }

        private void txtNickname_Validated(object sender, EventArgs e)
        {
            try
            {
                epName.Clear();
                txtName.Tag = null;
                epNick.Clear();
                txtNickname.Tag = null;

                if (QueryTeacher.NameExists(RunningID, txtName.Text, txtNickname.Text))
                {
                    epName.SetError(txtName, "「姓名+暱稱」不可與其他人相同。");
                    txtName.Tag = "error";
                    epNick.SetError(txtNickname, "「姓名+暱稱」不可與其他人相同。");
                    txtNickname.Tag = "error";
                }
            }
            catch (Exception ex)
            {
                epNick.SetError(txtNickname, string.Format("{0}", ex.Message));
            }
        }

        private void cboGender_Validated(object sender, EventArgs e)
        {
            try
            {
                epGender.Clear();
                cboGender.Tag = null;

                if (string.IsNullOrEmpty(cboGender.Text)) return;

                if (cboGender.Text != "男" && cboGender.Text != "女")
                {
                    epGender.SetError(cboGender, "性別只能填『男』或『女』。");
                    cboGender.Tag = "error";
                }
            }
            catch (Exception ex)
            {
                epGender.SetError(cboGender, string.Format("{0}", ex.Message));
            }
        }

        private void txtSTLoginAccount_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                errors.SetError(txtSTLoginAccount, "");
                txtSTLoginAccount.Tag = string.Empty;

                if (string.IsNullOrEmpty(txtSTLoginAccount.Text)) return;

                if (QueryTeacher.LoginNameExists(RunningID, txtSTLoginAccount.Text))
                {
                    errors.SetError(txtSTLoginAccount, "帳號名稱重覆。");
                    txtSTLoginAccount.Tag = "error";
                }
            }
            catch
            {
                errors.SetError(txtSTLoginAccount, "檢查帳號重覆失敗");
            }
        }

        private void buttonItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.Filter = "所有影像(*.jpg,*.jpeg,*.gif,*.png)|*.jpg;*.jpeg;*.gif;*.png;";
            if (od.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(od.FileName, FileMode.Open);
                    Bitmap orgBmp = new Bitmap(fs);
                    fs.Close();

                    Bitmap newBmp = new Bitmap(orgBmp, pictureBox.Size);
                    pictureBox.Image = newBmp;

                    _TeacherRec.Photo = ToBase64String(Photo.Resize(new Bitmap(orgBmp)));
                    SHTeacher.Update(_TeacherRec);
                    prlp.SaveLog("學籍系統-教師基本資料", "變更教師照片", "teacher", _TeacherRec.ID, "變更教師:" + _TeacherRec.Name + "的照片");
                }
                catch (Exception ex)
                {
                    FISCA.Presentation.Controls.MsgBox.Show(ex.Message);
                }
            }
        }

        private void buttonItem2_Click(object sender, EventArgs e)
        {
            SavePicture(_TeacherRec.Photo);
        }

        private void buttonItem3_Click(object sender, EventArgs e)
        {
            if (FISCA.Presentation.Controls.MsgBox.Show("您確定要清除此教師的照片嗎？", "", MessageBoxButtons.YesNo) == DialogResult.No) return;

            try
            {
                _TeacherRec.Photo = string.Empty;
                pictureBox.Image = pictureBox.InitialImage;
                SHTeacher.Update(_TeacherRec);
                prlp.SaveLog("學籍系統-教師基本資料", "變更教師照片", "teacher", _TeacherRec.ID, "變更教師:" + _TeacherRec.Name + "的照片");
            }
            catch (Exception ex)
            {
                FISCA.Presentation.Controls.MsgBox.Show(ex.Message);
            }
        }

        private void SavePicture(string imageString)
        {
            if (imageString == string.Empty)
                return;

            SaveFileDialog sd = new SaveFileDialog();
            sd.Filter = "PNG 影像|*.png;";
            string filename = string.IsNullOrWhiteSpace(txtIDNumber.Text) ? txtName.Text : txtIDNumber.Text;
            sd.FileName = filename + ".png";

            if (sd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sd.FileName, FileMode.Create);
                    byte[] imageData = Convert.FromBase64String(imageString);
                    fs.Write(imageData, 0, imageData.Length);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    FISCA.Presentation.Controls.MsgBox.Show(ex.Message);
                }
            }
        }

        private static string ToBase64String(Bitmap newBmp)
        {
            MemoryStream ms = new MemoryStream();
            newBmp.Save(ms, ImageFormat.Jpeg);
            ms.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[ms.Length];
            ms.Read(bytes, 0, (int)ms.Length);
            ms.Close();

            return Convert.ToBase64String(bytes);
        }

        private void txtTeacherNumber_Validating(object sender, CancelEventArgs e)
        {

        }
    }

    /// <summary>
    /// 記錄教師基本資料的兵器
    /// </summary>
    class TeacherBaseLogMachine
    {
        Dictionary<string, string> beforeData = new Dictionary<string, string>();
        Dictionary<string, string> afterData = new Dictionary<string, string>();
        List<string> hidden = new List<string>();

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

        public void HideSomething(string key)
        {
            if (!hidden.Contains(key))
                hidden.Add(key);
        }

        public string GetDescription()
        {
            //「」
            StringBuilder desc = new StringBuilder("");

            foreach (string key in beforeData.Keys)
            {
                if (afterData.ContainsKey(key) && afterData[key] != beforeData[key])
                {
                    if (hidden.Contains(key))
                        desc.AppendLine("欄位「" + key + "」已變更");
                    else
                        desc.AppendLine("欄位「" + key + "」由「" + beforeData[key] + "」變更為「" + afterData[key] + "」");
                }
            }

            return desc.ToString();
        }
    }
}
