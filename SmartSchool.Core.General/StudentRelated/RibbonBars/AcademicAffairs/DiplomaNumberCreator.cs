using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using SmartSchool.Customization.Data;
using FISCA.DSAUtil;
using System.Xml;
using SmartSchool.Common;

namespace SmartSchool.StudentRelated.RibbonBars.AcademicAffairs
{
    public partial class DiplomaNumberCreator : Office2007Form
    {
        List<SmartSchool.Customization.Data.StudentRecord> selectedStudent = null;

        public DiplomaNumberCreator()
        {
            InitializeComponent();
            InitStudentData();
        }

        private void InitStudentData()
        {
            AccessHelper helper = new AccessHelper();
            selectedStudent = helper.StudentHelper.GetSelectedStudent();
            selectedStudent.Sort(new StudentNumberComparer());

            //��J���~�ҮѦr��
            helper.StudentHelper.FillField("DiplomaNumber", selectedStudent);

            int studentCount = selectedStudent.Count;
            int withDNCount = 0;  //DN = DiplomaNumber ���~�ҮѦr��

            dataGridViewX1.Rows.Clear();

            foreach (SmartSchool.Customization.Data.StudentRecord var in selectedStudent)
            {
                int index = dataGridViewX1.Rows.Add();
                DataGridViewRow row = dataGridViewX1.Rows[index];
                row.Tag = var;
                row.Cells[StudentID.Name].Value = var.StudentID;
                row.Cells[ClassName.Name].Value = (var.RefClass != null) ? var.RefClass.ClassName : "";
                row.Cells[StudentNumber.Name].Value = var.StudentNumber;
                row.Cells[StudentName.Name].Value = var.StudentName;
                row.Cells[Status.Name].Value = var.Status;

                if ( var.Fields.ContainsKey("DiplomaNumber") && var.Fields["DiplomaNumber"] != null )
                {
                    DSXmlHelper dnElement = new DSXmlHelper(var.Fields["DiplomaNumber"] as XmlElement);
                    string dnString = dnElement.GetText("DiplomaNumber");
                    if ( !string.IsNullOrEmpty(dnString) )
                    {
                        row.Cells[DiplomaNumber.Name].Value = dnString;
                        withDNCount++;
                    }
                }
                else
                {
                    XmlElement emptyElement = new XmlDocument().CreateElement("DiplomaNumber");
                    if ( var.Fields.ContainsKey("DiplomaNumber") )
                    {
                        var.Fields["DiplomaNumber"] = emptyElement;
                    }
                    else
                        var.Fields.Add("DiplomaNumber",emptyElement);
                }
            }

            StringBuilder sum = new StringBuilder("");
            sum.AppendLine("�`�@ " + studentCount + " ��ǥ�");
            sum.AppendLine("�䤤 " + withDNCount + " ��ǥͤw�����~�ҮѦr��");
            lblSummary.Text = sum.ToString();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (CheckInputWordNumber() == false)
                return;

            StringBuilder zero = new StringBuilder("");
            for (int i = 0; i < textSerialNumber.Text.Length; i++)
                zero.Append("0");

            int serialNumber = int.Parse(textSerialNumber.Text);

            Dictionary<SmartSchool.Customization.Data.StudentRecord, string> tempDiplomaNumber = new Dictionary<SmartSchool.Customization.Data.StudentRecord, string>();
            foreach (SmartSchool.Customization.Data.StudentRecord var in selectedStudent)
            {
                tempDiplomaNumber.Add(var, textWord.Text + "�r��" + serialNumber.ToString(zero.ToString()) + "��");
                serialNumber++;
            }

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                string dnString = tempDiplomaNumber[row.Tag as SmartSchool.Customization.Data.StudentRecord];
                row.Cells[DiplomaNumber.Name].Value = dnString;
            }
        }

        private bool CheckInputWordNumber()
        {
            bool result = true;
            errorProvider1.Clear();

            //�ˬd �r
            if (string.IsNullOrEmpty(textWord.Text))
            {
                errorProvider1.SetError(textWord, "�п�J��r");
                result = false;
            }

            //�ˬd ��
            int a;
            if (string.IsNullOrEmpty(textSerialNumber.Text) || !int.TryParse(textSerialNumber.Text, out a))
            {
                errorProvider1.SetError(textSerialNumber, "�п�J�Ʀr");
                result = false;
            }

            return result;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DSXmlHelper helper = new DSXmlHelper("UpdateStudentList");

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                string dn = (row.Cells[DiplomaNumber.Name].Value != null) ? row.Cells[DiplomaNumber.Name].Value + "" : "";
                string id = row.Cells[StudentID.Name].Value + "";

                XmlElement element = ( row.Tag as SmartSchool.Customization.Data.StudentRecord ).Fields["DiplomaNumber"] as XmlElement;
                if ( element.SelectSingleNode("DiplomaNumber") != null )
                    element.SelectSingleNode("DiplomaNumber").InnerText = dn;
                else
                {
                    element.AppendChild(element.OwnerDocument.CreateElement("DiplomaNumber")).InnerText = dn;
                }
                helper.AddElement("Student");
                helper.AddElement("Student", "Field");
                helper.AddElement("Student/Field", element);
                //helper.AddElement("Student/Field", "DiplomaNumber", "<DiplomaNumber>" + dn + "</DiplomaNumber>", true);
                helper.AddElement("Student", "Condition");
                helper.AddElement("Student/Condition", "ID", id);
            }

            DSRequest req = new DSRequest(helper);

            try
            {
                SmartSchool.Feature.EditStudent.Update(req);
                MsgBox.Show("�x�s����");
            }
            catch (Exception ex)
            {
                MsgBox.Show("�x�s���ѡA���~�T���G" + ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.Selected)
                    row.Cells[DiplomaNumber.Name].Value = "";
            }
        }
    }

    class StudentNumberComparer : IComparer<SmartSchool.Customization.Data.StudentRecord>
    {
        #region IComparer<StudentRecord> ����

        public int Compare(SmartSchool.Customization.Data.StudentRecord x, SmartSchool.Customization.Data.StudentRecord y)
        {
            return x.StudentNumber.CompareTo(y.StudentNumber);
        }

        #endregion
    }
}