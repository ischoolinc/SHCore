using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SmartSchool.CourseRelated;
using SmartSchool.ApplicationLog.Forms;
using SmartSchool.ApplicationLog;
using SmartSchool.Common;
using SmartSchool.Security;
using System.IO;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class History : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase
    {
        FeatureAccessControl historyCtrl;

        public History()
        {
        }

        internal void Setup()
        {
            historyCtrl = new FeatureAccessControl("Button0620");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(History));

            var buttonItem1 = K12.Presentation.NLDPanels.Course.RibbonBarItems["其它"]["調換學生"];
            buttonItem1.Image = ( (System.Drawing.Image)( resources.GetObject("buttonItem1.Image") ) );
            buttonItem1.Click += new System.EventHandler(this.buttonItem1_Click);
            buttonItem1.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Large;
            buttonItem1.Visible = System.IO.File.Exists(Path.Combine(Application.StartupPath, "逃跑吧男孩"));
            buttonItem1.Enable = SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count > 0 && SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count <= 7;

            SmartSchool.CourseRelated.Course.Instance.SelectionChanged += delegate
            {
                bool isEnable = SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count > 0;
                buttonItem1.Enable = isEnable && SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count <= 7;
            };
        }

        private void buttonItem1_Click(object sender, EventArgs e)
        {
            new SwapAttendStudents().ShowDialog();
        }

    }
}

