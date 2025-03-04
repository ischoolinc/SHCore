namespace SmartSchool.CourseRelated.RibbonBars.ScoresCalc.Forms
{
    partial class CalculateWizard
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
            this.lblTitle = new DevComponents.DotNetBar.LabelX();
            this.btnCalcuate = new DevComponents.DotNetBar.ButtonX();
            this.btnExit = new DevComponents.DotNetBar.ButtonX();
            this.lblCourseCount = new DevComponents.DotNetBar.LabelX();
            this.btnExport = new DevComponents.DotNetBar.ButtonX();
            this.AbsentEqualZero = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.labelCousreCount1 = new DevComponents.DotNetBar.LabelX();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.labelCousreCount2 = new DevComponents.DotNetBar.LabelX();
            this.labelX4 = new DevComponents.DotNetBar.LabelX();
            this.labelX5 = new DevComponents.DotNetBar.LabelX();
            this.labelX6 = new DevComponents.DotNetBar.LabelX();
            this.labelX7 = new DevComponents.DotNetBar.LabelX();
            this.labelCousreCount3 = new DevComponents.DotNetBar.LabelX();
            this.labelCousreCount4 = new DevComponents.DotNetBar.LabelX();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblTitle.BackgroundStyle.Class = "";
            this.lblTitle.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblTitle.Location = new System.Drawing.Point(12, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(87, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "計算課程成績";
            // 
            // btnCalcuate
            // 
            this.btnCalcuate.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnCalcuate.BackColor = System.Drawing.Color.Transparent;
            this.btnCalcuate.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnCalcuate.Location = new System.Drawing.Point(225, 315);
            this.btnCalcuate.Name = "btnCalcuate";
            this.btnCalcuate.Size = new System.Drawing.Size(75, 23);
            this.btnCalcuate.TabIndex = 3;
            this.btnCalcuate.Text = "計算";
            this.btnCalcuate.Click += new System.EventHandler(this.btnCalcuate_Click);
            // 
            // btnExit
            // 
            this.btnExit.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            this.btnExit.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Location = new System.Drawing.Point(310, 315);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "離開";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblCourseCount
            // 
            this.lblCourseCount.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lblCourseCount.BackgroundStyle.Class = "";
            this.lblCourseCount.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lblCourseCount.Location = new System.Drawing.Point(13, 258);
            this.lblCourseCount.Name = "lblCourseCount";
            this.lblCourseCount.Size = new System.Drawing.Size(317, 41);
            this.lblCourseCount.TabIndex = 1;
            this.lblCourseCount.Text = "已選擇課程總數：10\r\n不需評分之課程數：3\r\n未設定評分樣版課程數：3\r\n含有「不強制繳交」課程數：8\r\n含有「成績缺漏」之課程數：3\r\n\r\n";
            this.lblCourseCount.TextLineAlignment = System.Drawing.StringAlignment.Near;
            this.lblCourseCount.WordWrap = true;
            // 
            // btnExport
            // 
            this.btnExport.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnExport.BackColor = System.Drawing.Color.Transparent;
            this.btnExport.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnExport.Location = new System.Drawing.Point(12, 315);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(153, 23);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "匯出選取課程總覽明細";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // AbsentEqualZero
            // 
            this.AbsentEqualZero.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.AbsentEqualZero.BackgroundStyle.Class = "";
            this.AbsentEqualZero.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.AbsentEqualZero.Location = new System.Drawing.Point(225, 276);
            this.AbsentEqualZero.Name = "AbsentEqualZero";
            this.AbsentEqualZero.Size = new System.Drawing.Size(160, 23);
            this.AbsentEqualZero.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.AbsentEqualZero.TabIndex = 5;
            this.AbsentEqualZero.Text = "缺考以零分計算";
            this.AbsentEqualZero.Visible = false;
            this.AbsentEqualZero.CheckedChanged += new System.EventHandler(this.AbsentEqualZero_CheckedChanged);
            // 
            // labelX1
            // 
            this.labelX1.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.Class = "";
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Location = new System.Drawing.Point(13, 64);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(93, 23);
            this.labelX1.TabIndex = 6;
            this.labelX1.Text = "選取課程數：";
            // 
            // labelCousreCount1
            // 
            this.labelCousreCount1.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelCousreCount1.BackgroundStyle.Class = "";
            this.labelCousreCount1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelCousreCount1.Location = new System.Drawing.Point(96, 64);
            this.labelCousreCount1.Name = "labelCousreCount1";
            this.labelCousreCount1.Size = new System.Drawing.Size(75, 23);
            this.labelCousreCount1.TabIndex = 7;
            this.labelCousreCount1.Text = "10";
            // 
            // labelX2
            // 
            this.labelX2.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX2.BackgroundStyle.Class = "";
            this.labelX2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX2.Location = new System.Drawing.Point(13, 95);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(188, 23);
            this.labelX2.TabIndex = 8;
            this.labelX2.Text = "由教師繳交課程成績課程數：";
            // 
            // labelX3
            // 
            this.labelX3.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX3.BackgroundStyle.Class = "";
            this.labelX3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX3.Location = new System.Drawing.Point(13, 119);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(372, 23);
            this.labelX3.TabIndex = 9;
            this.labelX3.Text = "課程評分樣版設定為「由老師繳交」，則無需結算課程成績。";
            // 
            // labelCousreCount2
            // 
            this.labelCousreCount2.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelCousreCount2.BackgroundStyle.Class = "";
            this.labelCousreCount2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelCousreCount2.Location = new System.Drawing.Point(185, 95);
            this.labelCousreCount2.Name = "labelCousreCount2";
            this.labelCousreCount2.Size = new System.Drawing.Size(75, 23);
            this.labelCousreCount2.TabIndex = 10;
            this.labelCousreCount2.Text = "10";
            // 
            // labelX4
            // 
            this.labelX4.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX4.BackgroundStyle.Class = "";
            this.labelX4.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX4.Location = new System.Drawing.Point(13, 152);
            this.labelX4.Name = "labelX4";
            this.labelX4.Size = new System.Drawing.Size(198, 23);
            this.labelX4.TabIndex = 11;
            this.labelX4.Text = "未設定評分樣版課程：";
            // 
            // labelX5
            // 
            this.labelX5.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX5.BackgroundStyle.Class = "";
            this.labelX5.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX5.Location = new System.Drawing.Point(13, 173);
            this.labelX5.Name = "labelX5";
            this.labelX5.Size = new System.Drawing.Size(247, 23);
            this.labelX5.TabIndex = 12;
            this.labelX5.Text = "未設定評分樣版，則無法結算課程成績。";
            // 
            // labelX6
            // 
            this.labelX6.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX6.BackgroundStyle.Class = "";
            this.labelX6.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX6.Location = new System.Drawing.Point(13, 223);
            this.labelX6.Name = "labelX6";
            this.labelX6.Size = new System.Drawing.Size(360, 23);
            this.labelX6.TabIndex = 14;
            this.labelX6.Text = "評量成績缺漏視為「免試」，將依評量群組結算課程成績。";
            // 
            // labelX7
            // 
            this.labelX7.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX7.BackgroundStyle.Class = "";
            this.labelX7.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX7.Location = new System.Drawing.Point(13, 202);
            this.labelX7.Name = "labelX7";
            this.labelX7.Size = new System.Drawing.Size(143, 23);
            this.labelX7.TabIndex = 13;
            this.labelX7.Text = "含評量成績缺漏課程：";
            // 
            // labelCousreCount3
            // 
            this.labelCousreCount3.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelCousreCount3.BackgroundStyle.Class = "";
            this.labelCousreCount3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelCousreCount3.Location = new System.Drawing.Point(148, 152);
            this.labelCousreCount3.Name = "labelCousreCount3";
            this.labelCousreCount3.Size = new System.Drawing.Size(75, 23);
            this.labelCousreCount3.TabIndex = 15;
            this.labelCousreCount3.Text = "10";
            // 
            // labelCousreCount4
            // 
            this.labelCousreCount4.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelCousreCount4.BackgroundStyle.Class = "";
            this.labelCousreCount4.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelCousreCount4.Location = new System.Drawing.Point(149, 202);
            this.labelCousreCount4.Name = "labelCousreCount4";
            this.labelCousreCount4.Size = new System.Drawing.Size(75, 23);
            this.labelCousreCount4.TabIndex = 16;
            this.labelCousreCount4.Text = "10";
            // 
            // CalculateWizard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnExit;
            this.ClientSize = new System.Drawing.Size(399, 351);
            this.Controls.Add(this.labelCousreCount4);
            this.Controls.Add(this.labelCousreCount3);
            this.Controls.Add(this.labelX6);
            this.Controls.Add(this.labelX7);
            this.Controls.Add(this.labelX5);
            this.Controls.Add(this.labelX4);
            this.Controls.Add(this.labelCousreCount2);
            this.Controls.Add(this.labelX3);
            this.Controls.Add(this.labelX2);
            this.Controls.Add(this.labelCousreCount1);
            this.Controls.Add(this.labelX1);
            this.Controls.Add(this.AbsentEqualZero);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnCalcuate);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.lblCourseCount);
            this.Controls.Add(this.lblTitle);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimumSize = new System.Drawing.Size(405, 249);
            this.Name = "CalculateWizard";
            this.Text = "計算課程成績";
            this.Load += new System.EventHandler(this.CalculateWizard_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.DotNetBar.LabelX lblTitle;
        private DevComponents.DotNetBar.ButtonX btnCalcuate;
        private DevComponents.DotNetBar.ButtonX btnExit;
        private DevComponents.DotNetBar.LabelX lblCourseCount;
        private DevComponents.DotNetBar.ButtonX btnExport;
        private DevComponents.DotNetBar.Controls.CheckBoxX AbsentEqualZero;
        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.DotNetBar.LabelX labelCousreCount1;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.LabelX labelX3;
        private DevComponents.DotNetBar.LabelX labelCousreCount2;
        private DevComponents.DotNetBar.LabelX labelX4;
        private DevComponents.DotNetBar.LabelX labelX5;
        private DevComponents.DotNetBar.LabelX labelX6;
        private DevComponents.DotNetBar.LabelX labelX7;
        private DevComponents.DotNetBar.LabelX labelCousreCount3;
        private DevComponents.DotNetBar.LabelX labelCousreCount4;
    }
}