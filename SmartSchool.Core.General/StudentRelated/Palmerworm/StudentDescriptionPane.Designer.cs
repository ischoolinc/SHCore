namespace SmartSchool.StudentRelated.Palmerworm
{
    partial class StudentDescriptionPane
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.ctxMenuBar = new DevComponents.DotNetBar.ContextMenuBar();
            this.ctxChangeStatus = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem2 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem3 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem4 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem5 = new DevComponents.DotNetBar.ButtonItem();
            this.pxStatus = new DevComponents.DotNetBar.PanelEx();
            this.lblStudent = new DevComponents.DotNetBar.LabelX();
            //this._marks = new SmartSchool.StudentRelated.Palmerworm.TagBar();
            ( (System.ComponentModel.ISupportInitialize)( this.ctxMenuBar ) ).BeginInit();
            this.SuspendLayout();
            // 
            // ctxMenuBar
            // 
            this.ctxMenuBar.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 136 ) ));
            this.ctxMenuBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.ctxChangeStatus});
            this.ctxMenuBar.Location = new System.Drawing.Point(98, 0);
            this.ctxMenuBar.Margin = new System.Windows.Forms.Padding(4);
            this.ctxMenuBar.Name = "ctxMenuBar";
            this.ctxMenuBar.Size = new System.Drawing.Size(104, 28);
            this.ctxMenuBar.Stretch = true;
            this.ctxMenuBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.ctxMenuBar.TabIndex = 188;
            this.ctxMenuBar.TabStop = false;
            this.ctxMenuBar.Text = "contextMenuBar1";
            // 
            // ctxChangeStatus
            // 
            this.ctxChangeStatus.AutoExpandOnClick = true;
            this.ctxChangeStatus.Name = "ctxChangeStatus";
            this.ctxChangeStatus.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem2,
            this.buttonItem3,
            this.buttonItem4,
            this.buttonItem1,
            this.buttonItem5});
            this.ctxChangeStatus.Text = "ChangeStatus";
            // 
            // buttonItem2
            // 
            this.buttonItem2.Name = "buttonItem2";
            this.buttonItem2.Tag = "一般";
            this.buttonItem2.Text = "一般";
            this.buttonItem2.Click += new System.EventHandler(this.ChangeStatus_Click);
            // 
            // buttonItem3
            // 
            this.buttonItem3.Name = "buttonItem3";
            this.buttonItem3.Tag = "畢業或離校";
            this.buttonItem3.Text = "畢業或離校";
            this.buttonItem3.Click += new System.EventHandler(this.ChangeStatus_Click);
            // 
            // buttonItem4
            // 
            this.buttonItem4.Name = "buttonItem4";
            this.buttonItem4.Tag = "休學";
            this.buttonItem4.Text = "休學";
            this.buttonItem4.Click += new System.EventHandler(this.ChangeStatus_Click);
            // 
            // buttonItem1
            // 
            this.buttonItem1.Name = "buttonItem1";
            this.buttonItem1.Tag = "延修";
            this.buttonItem1.Text = "延修";
            this.buttonItem1.Click += new System.EventHandler(this.ChangeStatus_Click);
            // 
            // buttonItem5
            // 
            this.buttonItem5.Name = "buttonItem5";
            this.buttonItem5.Tag = "輟學";
            this.buttonItem5.Text = "輟學";
            this.buttonItem5.Visible = false;
            this.buttonItem5.Click += new System.EventHandler(this.ChangeStatus_Click);
            // 
            // pxStatus
            // 
            this.pxStatus.CanvasColor = System.Drawing.SystemColors.Control;
            this.pxStatus.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.pxStatus.Location = new System.Drawing.Point(255, 4);
            this.pxStatus.Margin = new System.Windows.Forms.Padding(4);
            this.pxStatus.Name = "pxStatus";
            this.pxStatus.Size = new System.Drawing.Size(79, 21);
            this.pxStatus.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.pxStatus.Style.BackColor1.Color = System.Drawing.Color.LightBlue;
            this.pxStatus.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground2;
            this.pxStatus.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarDockedBorder;
            this.pxStatus.Style.BorderSide = DevComponents.DotNetBar.eBorderSide.None;
            this.pxStatus.Style.BorderWidth = 0;
            this.pxStatus.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.pxStatus.Style.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 136 ) ));
            this.pxStatus.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.pxStatus.Style.GradientAngle = 90;
            this.pxStatus.Style.TextTrimming = System.Drawing.StringTrimming.Word;
            this.pxStatus.TabIndex = 187;
            this.pxStatus.Text = "畢業或離校";
            this.pxStatus.Click += new System.EventHandler(this.pxStatus_Click);
            // 
            // lblStudent
            // 
            // 
            // 
            // 
            this.lblStudent.BackgroundStyle.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Etched;
            this.lblStudent.BackgroundStyle.BorderBottomColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.lblStudent.BackgroundStyle.BorderBottomWidth = 1;
            this.lblStudent.BackgroundStyle.BorderTopColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.lblStudent.BorderSide = DevComponents.DotNetBar.eBorderSide.None;
            this.lblStudent.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ( (byte)( 136 ) ));
            this.lblStudent.Location = new System.Drawing.Point(41, 1);
            this.lblStudent.Name = "lblStudent";
            this.lblStudent.Size = new System.Drawing.Size(293, 28);
            this.lblStudent.TabIndex = 186;
            //// 
            //// _marks
            //// 
            //this._marks.BackColor = System.Drawing.Color.Transparent;
            //this._marks.Dock = System.Windows.Forms.DockStyle.Bottom;
            //this._marks.Font = new System.Drawing.Font("微軟正黑體", 9.75F);
            //this._marks.Location = new System.Drawing.Point(0, 37);
            //this._marks.Margin = new System.Windows.Forms.Padding(4);
            //this._marks.MinimumSize = new System.Drawing.Size(550, 0);
            //this._marks.Name = "_marks";
            //this._marks.RunningID = null;
            //this._marks.SaveButtonVisible = false;
            //this._marks.Size = new System.Drawing.Size(550, 22);
            //this._marks.TabIndex = 189;
            //this._marks.Title = null;
            // 
            // StudentDescriptionPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.Controls.Add(this._marks);
            this.Controls.Add(this.ctxMenuBar);
            this.Controls.Add(this.pxStatus);
            this.Controls.Add(this.lblStudent);
            this.Name = "StudentDescriptionPane";
            this.Size = new System.Drawing.Size(375, 59);
            this.PrimaryKeyChanged += new System.EventHandler(this.StudentDescriptionPane_PrimaryKeyChanged);
            ( (System.ComponentModel.ISupportInitialize)( this.ctxMenuBar ) ).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.ContextMenuBar ctxMenuBar;
        private DevComponents.DotNetBar.ButtonItem ctxChangeStatus;
        private DevComponents.DotNetBar.ButtonItem buttonItem2;
        private DevComponents.DotNetBar.ButtonItem buttonItem3;
        private DevComponents.DotNetBar.ButtonItem buttonItem4;
        private DevComponents.DotNetBar.ButtonItem buttonItem1;
        private DevComponents.DotNetBar.ButtonItem buttonItem5;
        private DevComponents.DotNetBar.PanelEx pxStatus;
        private DevComponents.DotNetBar.LabelX lblStudent;
        //private TagBar _marks;



    }
}
