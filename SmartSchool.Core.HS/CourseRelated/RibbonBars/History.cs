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
using FISCA.Permission;

namespace SmartSchool.CourseRelated.RibbonBars
{
    public partial class History : SmartSchool.CourseRelated.RibbonBars.RibbonBarBase
    {
       

        public History()
        {
        }

        internal void Setup()
        {
            // 2019/01/17 �o�~�睊�A��h�~�ק�ꤤ ���դW�� ���䴩 �פJ�ץX�ƧǺ޲z���\�� ���Ө찪����
            // ���F�@���u�ơB �v���޲z�B�z
            Catalog ribbon5 = RoleAclSource.Instance["�ҵ{"]["�\����s"];
            ribbon5.Add(new RibbonFeature("6D6E2A93-51AE-4A44-907F-0E2A9318F977", "���դW��"));

            var buttonItem1 = K12.Presentation.NLDPanels.Course.RibbonBarItems["�а�"]["���դW��"];
            buttonItem1.Image = Properties.Resources.meeting_refresh_64;
            buttonItem1.Click += new System.EventHandler(this.buttonItem1_Click);
            buttonItem1.Size = FISCA.Presentation.RibbonBarButton.MenuButtonSize.Medium;

            buttonItem1.Enable = UserAcl.Current["6D6E2A93-51AE-4A44-907F-0E2A9318F977"].Executable && SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count > 1;

            SmartSchool.CourseRelated.Course.Instance.SelectionChanged += delegate
            {
                bool isEnable = UserAcl.Current["6D6E2A93-51AE-4A44-907F-0E2A9318F977"].Executable && SmartSchool.CourseRelated.Course.Instance.SelectionCourse.Count > 1;
                buttonItem1.Enable = isEnable;
            };
        }

        private void buttonItem1_Click(object sender, EventArgs e)
        {
            new SwapAttendStudents(Course.Instance.SelectionCourse.Count).ShowDialog();
        }

    }
}

