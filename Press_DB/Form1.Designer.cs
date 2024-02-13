namespace Press_DB
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            listBox = new GroupBox();
            groupBox = new GroupBox();
            PLCstateTxt = new TextBox();
            inBtn = new Button();
            outBtn = new Button();
            DBstateTxt = new TextBox();
            dataGrid = new DataGridView();
            error_Msg = new DataGridViewTextBoxColumn();
            error_Date = new DataGridViewTextBoxColumn();
            error_Time = new DataGridViewTextBoxColumn();
            listBox.SuspendLayout();
            groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGrid).BeginInit();
            SuspendLayout();
            // 
            // listBox
            // 
            listBox.Controls.Add(groupBox);
            listBox.Location = new Point(9, 158);
            listBox.Name = "listBox";
            listBox.Size = new Size(761, 383);
            listBox.TabIndex = 1;
            listBox.TabStop = false;
            // 
            // groupBox
            // 
            groupBox.BackColor = SystemColors.ActiveCaption;
            groupBox.Controls.Add(PLCstateTxt);
            groupBox.Controls.Add(inBtn);
            groupBox.Controls.Add(outBtn);
            groupBox.Controls.Add(DBstateTxt);
            groupBox.FlatStyle = FlatStyle.Popup;
            groupBox.Font = new Font("맑은 고딕", 1F);
            groupBox.Location = new Point(3, 317);
            groupBox.Name = "groupBox";
            groupBox.Size = new Size(755, 66);
            groupBox.TabIndex = 3;
            groupBox.TabStop = false;
            // 
            // PLCstateTxt
            // 
            PLCstateTxt.BackColor = SystemColors.ActiveCaption;
            PLCstateTxt.BorderStyle = BorderStyle.None;
            PLCstateTxt.Font = new Font("맑은 고딕", 10.8F);
            PLCstateTxt.Location = new Point(254, 36);
            PLCstateTxt.Name = "PLCstateTxt";
            PLCstateTxt.ReadOnly = true;
            PLCstateTxt.ShortcutsEnabled = false;
            PLCstateTxt.Size = new Size(216, 24);
            PLCstateTxt.TabIndex = 1;
            PLCstateTxt.TabStop = false;
            PLCstateTxt.Text = "PLC Disconnect";
            PLCstateTxt.TextAlign = HorizontalAlignment.Center;
            // 
            // inBtn
            // 
            inBtn.BackgroundImage = (Image)resources.GetObject("inBtn.BackgroundImage");
            inBtn.FlatStyle = FlatStyle.Popup;
            inBtn.Font = new Font("맑은 고딕", 9F);
            inBtn.ForeColor = Color.White;
            inBtn.Location = new Point(115, 21);
            inBtn.Name = "inBtn";
            inBtn.Size = new Size(113, 26);
            inBtn.TabIndex = 0;
            inBtn.Text = "TEST";
            inBtn.UseVisualStyleBackColor = false;
            inBtn.Click += inBtn_Click;
            // 
            // outBtn
            // 
            outBtn.BackgroundImage = (Image)resources.GetObject("outBtn.BackgroundImage");
            outBtn.FlatStyle = FlatStyle.Popup;
            outBtn.Font = new Font("맑은 고딕", 9F);
            outBtn.ForeColor = Color.White;
            outBtn.Location = new Point(492, 21);
            outBtn.Name = "outBtn";
            outBtn.Size = new Size(113, 26);
            outBtn.TabIndex = 0;
            outBtn.Text = "RESET";
            outBtn.UseVisualStyleBackColor = false;
            outBtn.Click += outBtn_Click;
            // 
            // DBstateTxt
            // 
            DBstateTxt.BackColor = SystemColors.ActiveCaption;
            DBstateTxt.BorderStyle = BorderStyle.None;
            DBstateTxt.Font = new Font("맑은 고딕", 10.8F);
            DBstateTxt.Location = new Point(254, 9);
            DBstateTxt.Name = "DBstateTxt";
            DBstateTxt.ReadOnly = true;
            DBstateTxt.ShortcutsEnabled = false;
            DBstateTxt.Size = new Size(216, 24);
            DBstateTxt.TabIndex = 1;
            DBstateTxt.TabStop = false;
            DBstateTxt.Text = "DB Disconnect";
            DBstateTxt.TextAlign = HorizontalAlignment.Center;
            // 
            // dataGrid
            // 
            dataGrid.AllowUserToResizeColumns = false;
            dataGrid.AllowUserToResizeRows = false;
            dataGrid.BackgroundColor = Color.FromArgb(191, 205, 218);
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("맑은 고딕", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGrid.Columns.AddRange(new DataGridViewColumn[] { error_Msg, error_Date, error_Time });
            dataGrid.Enabled = false;
            dataGrid.EnableHeadersVisualStyles = false;
            dataGrid.Location = new Point(12, 12);
            dataGrid.MultiSelect = false;
            dataGrid.Name = "dataGrid";
            dataGrid.ReadOnly = true;
            dataGrid.RightToLeft = RightToLeft.No;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = SystemColors.Control;
            dataGridViewCellStyle5.Font = new Font("맑은 고딕", 9F);
            dataGridViewCellStyle5.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.True;
            dataGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            dataGrid.RowHeadersVisible = false;
            dataGrid.RowHeadersWidth = 51;
            dataGrid.Size = new Size(755, 457);
            dataGrid.TabIndex = 0;
            dataGrid.TabStop = false;
            // 
            // error_Msg
            // 
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            error_Msg.DefaultCellStyle = dataGridViewCellStyle2;
            error_Msg.HeaderText = "Message";
            error_Msg.MinimumWidth = 6;
            error_Msg.Name = "error_Msg";
            error_Msg.ReadOnly = true;
            error_Msg.Width = 500;
            // 
            // error_Date
            // 
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            error_Date.DefaultCellStyle = dataGridViewCellStyle3;
            error_Date.HeaderText = "Error Date";
            error_Date.MinimumWidth = 6;
            error_Date.Name = "error_Date";
            error_Date.ReadOnly = true;
            error_Date.Width = 126;
            // 
            // error_Time
            // 
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            error_Time.DefaultCellStyle = dataGridViewCellStyle4;
            error_Time.HeaderText = "Error Time";
            error_Time.MinimumWidth = 6;
            error_Time.Name = "error_Time";
            error_Time.ReadOnly = true;
            error_Time.Width = 126;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 553);
            Controls.Add(dataGrid);
            Controls.Add(listBox);
            Name = "Form1";
            Text = "PLCClinet";
            listBox.ResumeLayout(false);
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGrid).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox listBox;
        private DataGridView dataGrid;
        private GroupBox groupBox;
        private TextBox PLCstateTxt;
        private Button inBtn;
        private Button outBtn;
        private TextBox DBstateTxt;
        private DataGridViewTextBoxColumn error_Msg;
        private DataGridViewTextBoxColumn error_Date;
        private DataGridViewTextBoxColumn error_Time;
    }
}
