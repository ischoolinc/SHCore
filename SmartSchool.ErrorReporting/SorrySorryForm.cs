﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;

namespace SmartSchool.ErrorReporting
{
    partial class SorrySorryForm : BaseForm
    {
        private string _FilePath = "";

        public SorrySorryForm()
        {
            InitializeComponent();
            linkLabel1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Process.Start(Application.StartupPath + "\\Exception\\" + fileName);
        }
    }
}
