﻿namespace SmartSchool.CourseRelated.Forms
{
    partial class EditCourseScore
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該公開 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.score = new SmartSchool.CourseRelated.DetailPaneItem.DataGridViewItem();
            this.btnSave = new DevComponents.DotNetBar.ButtonX();
            this.lblCourseName = new DevComponents.DotNetBar.LabelX();
            this.btnExit = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // score
            // 
            this.score.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.score.BackColor = System.Drawing.Color.Transparent;
            this.score.Font = new System.Drawing.Font("微軟正黑體", 9.75F);
            this.score.Location = new System.Drawing.Point(0, 32);
            this.score.Margin = new System.Windows.Forms.Padding(4);
            this.score.Name = "score";
            this.score.RunningID = null;
            this.score.SaveButtonVisible = false;
            this.score.Size = new System.Drawing.Size(533, 424);
            this.score.TabIndex = 0;
            this.score.Title = "編輯成績";
            // 
            // btnSave
            // 
            this.btnSave.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.Transparent;
            this.btnSave.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnSave.Location = new System.Drawing.Point(374, 463);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "儲存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblCourseName
            // 
            this.lblCourseName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCourseName.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblCourseName.BackgroundStyle.Class = "";
            this.lblCourseName.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblCourseName.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
            this.lblCourseName.BorderType = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.lblCourseName.Location = new System.Drawing.Point(12, 6);
            this.lblCourseName.Name = "lblCourseName";
            this.lblCourseName.Size = new System.Drawing.Size(234, 23);
            this.lblCourseName.TabIndex = 2;
            this.lblCourseName.Text = "CourseName";
            // 
            // btnExit
            // 
            this.btnExit.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            this.btnExit.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnExit.Location = new System.Drawing.Point(455, 463);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "離開";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // EditCourseScore
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(539, 495);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.lblCourseName);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.score);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = true;
            this.Name = "EditCourseScore";
            this.Text = "";
            this.Load += new System.EventHandler(this.EditCourseScore_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private SmartSchool.CourseRelated.DetailPaneItem.DataGridViewItem score;
        private DevComponents.DotNetBar.ButtonX btnSave;
        private DevComponents.DotNetBar.LabelX lblCourseName;
        private DevComponents.DotNetBar.ButtonX btnExit;
    }
}