using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using FISCA.DSAUtil;
using FISCA.Presentation;

namespace SmartSchool.TeacherRelated.Palmerworm
{
    public partial class TeacherDescriptionPane : DescriptionPane
    {
        public TeacherDescriptionPane()
        {
            InitializeComponent();
        }

        private void StudentDescriptionPane_PrimaryKeyChanged(object sender, EventArgs e)
        {
            var _TeacherInfo = Teacher.Instance[PrimaryKey];
            if ( _TeacherInfo != null )
            {
                lblStudent.Text = ( _TeacherInfo.UniqName == "" ? "" : _TeacherInfo.UniqName + " 老師" );
                Color s;
                string txt = _TeacherInfo.Status;
                switch ( txt )
                {
                    case "一般":
                        s = Color.FromArgb(255, 255, 255);
                        break;
                    case "刪除":
                        s = Color.FromArgb(254, 128, 155);
                        break;
                    default:
                        s = Color.Transparent;
                        break;
                }
                pxStatus.Text = txt;
                pxStatus.Style.BackColor1.Color = s;
                pxStatus.Style.BackColor2.Color = s;
            }
        }

        private void pxStatus_Click(object sender, EventArgs e)
        {
            ctxChangeStatus.Popup(Control.MousePosition);
        }

        private void buttonItem5_Click(object sender, EventArgs e)
        {
            SmartSchool.Feature.Teacher.RemoveTeacher.DeleteTeacher(PrimaryKey);
            Teacher.Instance.SyncDataBackground(PrimaryKey);
        }

        private void buttonItem2_Click(object sender, EventArgs e)
        {
            DSXmlHelper helper = new DSXmlHelper("UpdateRequest");
            helper.AddElement("Teacher");
            helper.AddElement("Teacher", "Field");
            helper.AddElement("Teacher/Field", "Status", "一般");
            helper.AddElement("Teacher", "Condition");
            helper.AddElement("Teacher/Condition", "ID", PrimaryKey);
            SmartSchool.Feature.Teacher.EditTeacher.Update(new DSRequest(helper));
            Teacher.Instance.SyncDataBackground(PrimaryKey);
        }
    }
}
