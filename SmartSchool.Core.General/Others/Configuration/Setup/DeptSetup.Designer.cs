namespace SmartSchool.Others.Configuration.Setup
{
    partial class DeptSetup
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeptSetup));
            this.dgDept = new DevComponents.DotNetBar.Controls.DataGridViewX();
            this.btnSave = new DevComponents.DotNetBar.ButtonX();
            this.btnExit = new DevComponents.DotNetBar.ButtonX();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxExColumn1 = new SmartSchool.Others.Configuration.Setup.DataGridViewComboBoxExColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colChiName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptTeacher = new SmartSchool.Others.Configuration.Setup.DataGridViewComboBoxExColumn();
            this.colEngName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptGroup = new DevComponents.DotNetBar.Controls.DataGridViewComboBoxExColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgDept)).BeginInit();
            this.SuspendLayout();
            // 
            // dgDept
            // 
            this.dgDept.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgDept.BackgroundColor = System.Drawing.Color.White;
            this.dgDept.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgDept.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgDept.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCode,
            this.colChiName,
            this.colDeptTeacher,
            this.colEngName,
            this.colDeptGroup});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft JhengHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgDept.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgDept.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(229)))));
            this.dgDept.Location = new System.Drawing.Point(7, 7);
            this.dgDept.Name = "dgDept";
            this.dgDept.RowHeadersWidth = 25;
            this.dgDept.RowTemplate.Height = 24;
            this.dgDept.Size = new System.Drawing.Size(584, 393);
            this.dgDept.TabIndex = 0;
            this.dgDept.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgDept_CurrentCellDirtyStateChanged);
            this.dgDept.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dgDept_UserDeletingRow);
            // 
            // btnSave
            // 
            this.btnSave.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.Transparent;
            this.btnSave.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnSave.Location = new System.Drawing.Point(411, 406);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(87, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "儲存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnExit
            // 
            this.btnExit.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            this.btnExit.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnExit.Location = new System.Drawing.Point(504, 406);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(87, 23);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "離開";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.FillWeight = 3F;
            this.dataGridViewTextBoxColumn1.HeaderText = "科別代碼";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.FillWeight = 6F;
            this.dataGridViewTextBoxColumn2.HeaderText = "科別中文名稱";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewComboBoxExColumn1
            // 
            this.dataGridViewComboBoxExColumn1.HeaderText = "科主任";
            this.dataGridViewComboBoxExColumn1.Items = ((System.Collections.Generic.List<string>)(resources.GetObject("dataGridViewComboBoxExColumn1.Items")));
            this.dataGridViewComboBoxExColumn1.Name = "dataGridViewComboBoxExColumn1";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "科別英文名稱";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            this.dataGridViewTextBoxColumn3.Visible = false;
            this.dataGridViewTextBoxColumn3.Width = 120;
            // 
            // colCode
            // 
            this.colCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colCode.FillWeight = 3F;
            this.colCode.HeaderText = "科別代碼";
            this.colCode.Name = "colCode";
            // 
            // colChiName
            // 
            this.colChiName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colChiName.FillWeight = 6F;
            this.colChiName.HeaderText = "科別中文名稱";
            this.colChiName.Name = "colChiName";
            // 
            // colDeptTeacher
            // 
            this.colDeptTeacher.HeaderText = "科主任";
            this.colDeptTeacher.Items = ((System.Collections.Generic.List<string>)(resources.GetObject("colDeptTeacher.Items")));
            this.colDeptTeacher.Name = "colDeptTeacher";
            this.colDeptTeacher.Width = 120;
            // 
            // colEngName
            // 
            this.colEngName.HeaderText = "科別英文名稱";
            this.colEngName.Name = "colEngName";
            this.colEngName.ReadOnly = true;
            this.colEngName.Visible = false;
            this.colEngName.Width = 120;
            // 
            // colDeptGroup
            // 
            this.colDeptGroup.DisplayMember = "Text";
            this.colDeptGroup.DropDownHeight = 106;
            this.colDeptGroup.DropDownWidth = 121;
            this.colDeptGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colDeptGroup.HeaderText = "部別";
            this.colDeptGroup.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.colDeptGroup.ItemHeight = 17;
            this.colDeptGroup.Name = "colDeptGroup";
            this.colDeptGroup.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDeptGroup.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // DeptSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 437);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.dgDept);
            this.DoubleBuffered = true;
            this.Name = "DeptSetup";
            this.Text = "科別對照管理";
            this.Load += new System.EventHandler(this.DeptSetup_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgDept)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Controls.DataGridViewX dgDept;
        private DevComponents.DotNetBar.ButtonX btnSave;
        private DevComponents.DotNetBar.ButtonX btnExit;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewComboBoxExColumn dataGridViewComboBoxExColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colChiName;
        private DataGridViewComboBoxExColumn colDeptTeacher;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEngName;
        private DevComponents.DotNetBar.Controls.DataGridViewComboBoxExColumn colDeptGroup;
    }
}