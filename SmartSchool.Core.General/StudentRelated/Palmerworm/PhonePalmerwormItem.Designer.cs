﻿namespace SmartSchool.StudentRelated.Palmerworm
{
    partial class PhonePalmerwormItem
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

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.txtContactPhone = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtOtherPhone = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.txtEverPhone = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOthers = new DevComponents.DotNetBar.ButtonX();
            this.btnOther1 = new DevComponents.DotNetBar.ButtonItem();
            this.btnOther2 = new DevComponents.DotNetBar.ButtonItem();
            this.btnOther3 = new DevComponents.DotNetBar.ButtonItem();
            this.controlContainerItem1 = new DevComponents.DotNetBar.ControlContainerItem();
            this.txtSMS = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.lblSMS = new System.Windows.Forms.Label();
            this.buttonX4 = new DevComponents.DotNetBar.ButtonX();
            this.buttonItem4 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem1 = new DevComponents.DotNetBar.ButtonItem();
            this.btnCSMS = new DevComponents.DotNetBar.ButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this.picWaiting)).BeginInit();
            this.SuspendLayout();
            // 
            // txtContactPhone
            // 
            // 
            // 
            // 
            this.txtContactPhone.Border.Class = "TextBoxBorder";
            this.txtContactPhone.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtContactPhone.Location = new System.Drawing.Point(365, 17);
            this.txtContactPhone.Margin = new System.Windows.Forms.Padding(4);
            this.txtContactPhone.Name = "txtContactPhone";
            this.txtContactPhone.Size = new System.Drawing.Size(118, 25);
            this.txtContactPhone.TabIndex = 3;
            this.txtContactPhone.WordWrap = false;
            this.txtContactPhone.TextChanged += new System.EventHandler(this.txtContactPhone_TextChanged);
            // 
            // txtOtherPhone
            // 
            // 
            // 
            // 
            this.txtOtherPhone.Border.Class = "TextBoxBorder";
            this.txtOtherPhone.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtOtherPhone.Location = new System.Drawing.Point(131, 47);
            this.txtOtherPhone.Margin = new System.Windows.Forms.Padding(4);
            this.txtOtherPhone.Name = "txtOtherPhone";
            this.txtOtherPhone.Size = new System.Drawing.Size(118, 25);
            this.txtOtherPhone.TabIndex = 6;
            this.txtOtherPhone.WordWrap = false;
            this.txtOtherPhone.TextChanged += new System.EventHandler(this.txtOtherPhone_TextChanged);
            // 
            // txtEverPhone
            // 
            // 
            // 
            // 
            this.txtEverPhone.Border.Class = "TextBoxBorder";
            this.txtEverPhone.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtEverPhone.Location = new System.Drawing.Point(131, 17);
            this.txtEverPhone.Margin = new System.Windows.Forms.Padding(4);
            this.txtEverPhone.Name = "txtEverPhone";
            this.txtEverPhone.Size = new System.Drawing.Size(118, 25);
            this.txtEverPhone.TabIndex = 1;
            this.txtEverPhone.WordWrap = false;
            this.txtEverPhone.TextChanged += new System.EventHandler(this.txtEverPhone_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(63, 19);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 17);
            this.label3.TabIndex = 199;
            this.label3.Text = "戶籍電話";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(297, 19);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 17);
            this.label2.TabIndex = 198;
            this.label2.Text = "聯絡電話";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnOthers
            // 
            this.btnOthers.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnOthers.AutoExpandOnClick = true;
            this.btnOthers.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnOthers.Location = new System.Drawing.Point(33, 47);
            this.btnOthers.Margin = new System.Windows.Forms.Padding(4);
            this.btnOthers.Name = "btnOthers";
            this.btnOthers.Size = new System.Drawing.Size(90, 22);
            this.btnOthers.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.btnOther1,
            this.btnOther2,
            this.btnOther3});
            this.btnOthers.TabIndex = 5;
            // 
            // btnOther1
            // 
            this.btnOther1.GlobalItem = false;
            this.btnOther1.Name = "btnOther1";
            this.btnOther1.Text = "其它電話1";
            this.btnOther1.Click += new System.EventHandler(this.btnOther1_Click);
            // 
            // btnOther2
            // 
            this.btnOther2.GlobalItem = false;
            this.btnOther2.Name = "btnOther2";
            this.btnOther2.Text = "其它電話2";
            this.btnOther2.Click += new System.EventHandler(this.btnOther2_Click);
            // 
            // btnOther3
            // 
            this.btnOther3.GlobalItem = false;
            this.btnOther3.Name = "btnOther3";
            this.btnOther3.Text = "其它電話3";
            this.btnOther3.Click += new System.EventHandler(this.btnOther3_Click);
            // 
            // controlContainerItem1
            // 
            this.controlContainerItem1.AllowItemResize = true;
            this.controlContainerItem1.MenuVisibility = DevComponents.DotNetBar.eMenuVisibility.VisibleAlways;
            this.controlContainerItem1.Name = "controlContainerItem1";
            this.controlContainerItem1.Text = "controlContainerItem1";
            // 
            // txtSMS
            // 
            // 
            // 
            // 
            this.txtSMS.Border.Class = "TextBoxBorder";
            this.txtSMS.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.txtSMS.Location = new System.Drawing.Point(365, 47);
            this.txtSMS.Margin = new System.Windows.Forms.Padding(4);
            this.txtSMS.Name = "txtSMS";
            this.txtSMS.Size = new System.Drawing.Size(118, 25);
            this.txtSMS.TabIndex = 8;
            this.txtSMS.WordWrap = false;
            this.txtSMS.TextChanged += new System.EventHandler(this.txtSMS_TextChanged);
            // 
            // lblSMS
            // 
            this.lblSMS.AutoSize = true;
            this.lblSMS.BackColor = System.Drawing.Color.Transparent;
            this.lblSMS.ForeColor = System.Drawing.Color.Black;
            this.lblSMS.Location = new System.Drawing.Point(297, 49);
            this.lblSMS.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSMS.Name = "lblSMS";
            this.lblSMS.Size = new System.Drawing.Size(60, 17);
            this.lblSMS.TabIndex = 222;
            this.lblSMS.Text = "行動電話";
            this.lblSMS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonX4
            // 
            this.buttonX4.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX4.AutoExpandOnClick = true;
            this.buttonX4.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX4.Enabled = false;
            this.buttonX4.Location = new System.Drawing.Point(491, 49);
            this.buttonX4.Margin = new System.Windows.Forms.Padding(4);
            this.buttonX4.Name = "buttonX4";
            this.buttonX4.Size = new System.Drawing.Size(17, 22);
            this.buttonX4.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem4});
            this.buttonX4.SubItemsExpandWidth = 15;
            this.buttonX4.TabIndex = 9;
            this.buttonX4.Visible = false;
            // 
            // buttonItem4
            // 
            this.buttonItem4.GlobalItem = false;
            this.buttonItem4.Name = "buttonItem4";
            this.buttonItem4.Text = "SMS簡訊";
            this.buttonItem4.Click += new System.EventHandler(this.buttonItem4_Click);
            // 
            // buttonItem1
            // 
            this.buttonItem1.Name = "buttonItem1";
            // 
            // btnCSMS
            // 
            this.btnCSMS.Enabled = false;
            this.btnCSMS.GlobalItem = false;
            this.btnCSMS.Name = "btnCSMS";
            this.btnCSMS.Text = "SMS簡訊";
            this.btnCSMS.Visible = false;
            this.btnCSMS.Click += new System.EventHandler(this.btnCSMS_Click);
            // 
            // PhonePalmerwormItem
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.buttonX4);
            this.Controls.Add(this.txtSMS);
            this.Controls.Add(this.lblSMS);
            this.Controls.Add(this.btnOthers);
            this.Controls.Add(this.txtContactPhone);
            this.Controls.Add(this.txtOtherPhone);
            this.Controls.Add(this.txtEverPhone);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "PhonePalmerwormItem";
            this.Size = new System.Drawing.Size(550, 83);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.txtEverPhone, 0);
            this.Controls.SetChildIndex(this.txtOtherPhone, 0);
            this.Controls.SetChildIndex(this.txtContactPhone, 0);
            this.Controls.SetChildIndex(this.btnOthers, 0);
            this.Controls.SetChildIndex(this.lblSMS, 0);
            this.Controls.SetChildIndex(this.txtSMS, 0);
            this.Controls.SetChildIndex(this.buttonX4, 0);
            this.Controls.SetChildIndex(this.picWaiting, 0);
            ((System.ComponentModel.ISupportInitialize)(this.picWaiting)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal DevComponents.DotNetBar.Controls.TextBoxX txtContactPhone;
        internal DevComponents.DotNetBar.Controls.TextBoxX txtOtherPhone;
        internal DevComponents.DotNetBar.Controls.TextBoxX txtEverPhone;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private DevComponents.DotNetBar.ButtonX btnOthers;
        private DevComponents.DotNetBar.ButtonItem btnOther1;
        private DevComponents.DotNetBar.ButtonItem btnOther2;
        private DevComponents.DotNetBar.ButtonItem btnOther3;
        private DevComponents.DotNetBar.ControlContainerItem controlContainerItem1;
        internal DevComponents.DotNetBar.Controls.TextBoxX txtSMS;
        private System.Windows.Forms.Label lblSMS;
        private DevComponents.DotNetBar.ButtonX buttonX4;
        private DevComponents.DotNetBar.ButtonItem buttonItem4;
        private DevComponents.DotNetBar.ButtonItem buttonItem1;
        private DevComponents.DotNetBar.ButtonItem btnCSMS;
    }
}
