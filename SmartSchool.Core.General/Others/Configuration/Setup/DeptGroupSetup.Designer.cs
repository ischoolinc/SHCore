namespace SmartSchool.Others.Configuration.Setup
{
    partial class DeptGroupSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeptGroupSetup));
            this.btnSave = new DevComponents.DotNetBar.ButtonX();
            this.btnExit = new DevComponents.DotNetBar.ButtonX();
            this.dgDeptGroup = new DevComponents.DotNetBar.Controls.DataGridViewX();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptGroupName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeptGroupCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewComboBoxExColumn1 = new SmartSchool.Others.Configuration.Setup.DataGridViewComboBoxExColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgDeptGroup)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackColor = System.Drawing.Color.Transparent;
            this.btnSave.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnSave.Location = new System.Drawing.Point(147, 160);
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
            this.btnExit.Location = new System.Drawing.Point(240, 160);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(87, 23);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "離開";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dgDeptGroup
            // 
            this.dgDeptGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgDeptGroup.BackgroundColor = System.Drawing.Color.White;
            this.dgDeptGroup.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgDeptGroup.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgDeptGroup.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDeptGroupName,
            this.colDeptGroupCode});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("微軟正黑體", 9.75F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgDeptGroup.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgDeptGroup.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(229)))));
            this.dgDeptGroup.Location = new System.Drawing.Point(2, 12);
            this.dgDeptGroup.Name = "dgDeptGroup";
            this.dgDeptGroup.RowHeadersWidth = 25;
            this.dgDeptGroup.RowTemplate.Height = 24;
            this.dgDeptGroup.Size = new System.Drawing.Size(325, 141);
            this.dgDeptGroup.TabIndex = 2;
            this.dgDeptGroup.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgDeptGroup_CellEnter);
            this.dgDeptGroup.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgDeptGroup_CellValidated);
            this.dgDeptGroup.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgDeptGroup_DataError);
            this.dgDeptGroup.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgDeptGroup_RowHeaderMouseClick);
            this.dgDeptGroup.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dgDeptGroup_UserDeletingRow);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn1.FillWeight = 6F;
            this.dataGridViewTextBoxColumn1.HeaderText = "部別名稱";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ToolTipText = "請輸入部別名稱，如:日間部、夜間部、綜合高中等";
            this.dataGridViewTextBoxColumn1.Width = 200;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dataGridViewTextBoxColumn2.FillWeight = 3F;
            this.dataGridViewTextBoxColumn2.HeaderText = "部別代碼";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ToolTipText = "請輸入部別名稱對應的代碼，如:D、N、C等";
            this.dataGridViewTextBoxColumn2.Width = 98;
            // 
            // colDeptGroupName
            // 
            this.colDeptGroupName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDeptGroupName.FillWeight = 6F;
            this.colDeptGroupName.HeaderText = "部別名稱";
            this.colDeptGroupName.Name = "colDeptGroupName";
            this.colDeptGroupName.ToolTipText = "請輸入部別名稱，如:日間部、夜間部、綜合高中等";
            this.colDeptGroupName.Width = 200;
            // 
            // colDeptGroupCode
            // 
            this.colDeptGroupCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDeptGroupCode.FillWeight = 3F;
            this.colDeptGroupCode.HeaderText = "部別代碼";
            this.colDeptGroupCode.Name = "colDeptGroupCode";
            this.colDeptGroupCode.ToolTipText = "請輸入部別名稱對應的代碼，如:D、N、C等";
            // 
            // dataGridViewComboBoxExColumn1
            // 
            this.dataGridViewComboBoxExColumn1.Items = ((System.Collections.Generic.List<string>)(resources.GetObject("dataGridViewComboBoxExColumn1.Items")));
            this.dataGridViewComboBoxExColumn1.Name = "dataGridViewComboBoxExColumn1";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // DeptGroupSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 195);
            this.Controls.Add(this.dgDeptGroup);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnSave);
            this.DoubleBuffered = true;
            this.Name = "DeptGroupSetup";
            this.Text = "部別管理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DeptGroupSetup_FormClosing);
            this.Load += new System.EventHandler(this.DeptGroupSetup_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgDeptGroup)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevComponents.DotNetBar.ButtonX btnSave;
        private DevComponents.DotNetBar.ButtonX btnExit;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewComboBoxExColumn dataGridViewComboBoxExColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DevComponents.DotNetBar.Controls.DataGridViewX dgDeptGroup;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptGroupName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeptGroupCode;
    }
}