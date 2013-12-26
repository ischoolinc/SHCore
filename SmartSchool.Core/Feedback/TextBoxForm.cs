using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using System.IO;
using System.Xml;
using FISCA.DSAUtil;

namespace SmartSchool.Feedback
{
    public partial class TextBoxForm : BaseForm
    {
        string url;

        public TextBoxForm(DSXmlHelper dsx)
        {
            InitializeComponent();

            string tx = dsx.GetText("Message");
            textBoxX1.Text = tx.ToString().Replace("\n", "\r\n");

            if (string.IsNullOrEmpty(dsx.GetText("Url")))
            {
                linkLabel1.Visible = false;
            }
            else
            {
                linkLabel1.Visible = true;
                url = dsx.GetText("Url");
            }
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}
