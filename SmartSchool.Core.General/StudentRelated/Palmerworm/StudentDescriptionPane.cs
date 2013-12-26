using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using FISCA.Presentation;
using SmartSchool.Common;

namespace SmartSchool.StudentRelated.Palmerworm
{
    public partial class StudentDescriptionPane : DescriptionPane
    {
        public StudentDescriptionPane()
        {
            InitializeComponent();
        }

        private void StudentDescriptionPane_PrimaryKeyChanged(object sender, EventArgs e)
        {
            var StudentInfo = Student.Instance[PrimaryKey];
            if ( StudentInfo != null )
            {
                lblStudent.Text = string.Format("{0}  {1}  {2}", StudentInfo.ClassName, StudentInfo.Name, StudentInfo.StudentNumber);
                SyncStatus(StudentInfo.Status);

                //_summary.LoadById(_StudentInfo.ID);
                //_marks.LoadContent(PrimaryKey);
            }
        }

        private void SyncStatus(string p)
        {
            Color s;
            string msg = p;
            switch ( p )
            {
                case "一般":
                    s = Color.FromArgb(255, 255, 255);
                    break;
                case "畢業或離校":
                    s = Color.FromArgb(156, 220, 128);
                    break;
                case "休學":
                    s = Color.FromArgb(254, 244, 128);
                    break;
                case "延修":
                    s = Color.FromArgb(224, 254, 210);
                    break;
                case "輟學":
                    s = Color.FromArgb(254, 244, 128);
                    break;
                case "刪除":
                    s = Color.FromArgb(254, 128, 155);
                    break;
                default:
                    msg = "未知";
                    s = Color.Transparent;
                    break;
            }

            pxStatus.Text = p;
            pxStatus.Style.BackColor1.Color = s;
            pxStatus.Style.BackColor2.Color = s;
        }

        private void pxStatus_Click(object sender, EventArgs e)
        {
            ctxChangeStatus.Popup(Control.MousePosition);
        }

        private void ChangeStatus_Click(object sender, EventArgs e)
        {
            ButtonItem objStatus = sender as ButtonItem;
            string strNewStatus = objStatus.Tag.ToString();
            string strOrigStatus = Student.Instance[PrimaryKey].Status;
            List<string> checkIDNumberList = new List<string>();
            List<string> chekcStudentNumList = new List<string>();
            
            // 取得將要變更的身分證號,學號List
            foreach (BriefStudentData studRec in Student.Instance.Items)
            {
                if (studRec.Status == strNewStatus)
                {
                    checkIDNumberList.Add(studRec.IDNumber.ToUpper());
                    chekcStudentNumList.Add(studRec.StudentNumber);
                }
            }
            try
            {
                // 修改學生的身分證號
                string strOrigIDNumber = Student.Instance[PrimaryKey].IDNumber.ToUpper();
                string strOrigStudentNum = Student.Instance[PrimaryKey].StudentNumber;

                if (!string.IsNullOrEmpty(strOrigIDNumber))
                {
                    // 已有相同身分證號
                    if (checkIDNumberList.Contains(strOrigIDNumber.ToUpper()))
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("在[" + strNewStatus + "]狀態已有相同身分證號學生,請變更身分證號後再變更狀態.");
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(strOrigStudentNum))
                {
                    // 已有相同學號
                    if (chekcStudentNumList.Contains(strOrigStudentNum))
                    {
                        FISCA.Presentation.Controls.MsgBox.Show("在[" + strNewStatus + "]狀態已有相同學號學生,請變更學號後再變更狀態.");
                        return;
                    }
                }



                Feature.EditStudent.ChangeStudentStatus(PrimaryKey, strNewStatus);
                SyncStatus(strNewStatus);

                #region 修改學生狀態 Log

                if ( !strOrigStatus.Equals(strNewStatus) )
                {
                    StringBuilder desc = new StringBuilder("");
                    desc.AppendLine("學生姓名：" + Student.Instance[PrimaryKey].Name + " ");
                    desc.AppendLine("狀態由「" + strOrigStatus + "」變更為「" + strNewStatus + "」");
                    CurrentUser.Instance.AppLog.Write(SmartSchool.ApplicationLog.EntityType.Student, "變更狀態", PrimaryKey, desc.ToString(), "學生", "");
                }

                #endregion

                //Student.Instance.InvokBriefDataChanged(StudentInfo.ID);
                SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(PrimaryKey);
            }
            catch ( Exception )
            {
                MsgBox.Show("變更狀態失敗，請檢查服務是否正常。");
            }
        }
    }
}
