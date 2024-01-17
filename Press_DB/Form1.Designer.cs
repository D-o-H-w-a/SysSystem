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
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            listBox = new GroupBox();
            groupBox = new GroupBox();
            OPCstateTxt = new TextBox();
            inBtn = new Button();
            outBtn = new Button();
            SQLstateTxt = new TextBox();
            dataGrid = new DataGridView();
            cellNum = new DataGridViewTextBoxColumn();
            cellState = new DataGridViewTextBoxColumn();
            scState = new DataGridViewTextBoxColumn();
            workDate = new DataGridViewTextBoxColumn();
            workTime = new DataGridViewTextBoxColumn();
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
            groupBox.Controls.Add(OPCstateTxt);
            groupBox.Controls.Add(inBtn);
            groupBox.Controls.Add(outBtn);
            groupBox.Controls.Add(SQLstateTxt);
            groupBox.FlatStyle = FlatStyle.Popup;
            groupBox.Font = new Font("맑은 고딕", 1F);
            groupBox.Location = new Point(3, 317);
            groupBox.Name = "groupBox";
            groupBox.Size = new Size(755, 66);
            groupBox.TabIndex = 3;
            groupBox.TabStop = false;
            // 
            // OPCstateTxt
            // 
            OPCstateTxt.BackColor = SystemColors.ActiveCaption;
            OPCstateTxt.BorderStyle = BorderStyle.None;
            OPCstateTxt.Font = new Font("맑은 고딕", 10.8F);
            OPCstateTxt.Location = new Point(281, 36);
            OPCstateTxt.Name = "OPCstateTxt";
            OPCstateTxt.ReadOnly = true;
            OPCstateTxt.ShortcutsEnabled = false;
            OPCstateTxt.Size = new Size(164, 24);
            OPCstateTxt.TabIndex = 1;
            OPCstateTxt.TabStop = false;
            OPCstateTxt.Text = "OPC Comm Status";
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
            inBtn.Text = "IN";
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
            outBtn.Text = "OUT";
            outBtn.UseVisualStyleBackColor = false;
            outBtn.Click += outBtn_Click;
            // 
            // SQLstateTxt
            // 
            SQLstateTxt.BackColor = SystemColors.ActiveCaption;
            SQLstateTxt.BorderStyle = BorderStyle.None;
            SQLstateTxt.Font = new Font("맑은 고딕", 10.8F);
            SQLstateTxt.Location = new Point(281, 6);
            SQLstateTxt.Name = "SQLstateTxt";
            SQLstateTxt.ReadOnly = true;
            SQLstateTxt.ShortcutsEnabled = false;
            SQLstateTxt.Size = new Size(164, 24);
            SQLstateTxt.TabIndex = 1;
            SQLstateTxt.TabStop = false;
            SQLstateTxt.Text = "SQL Comm Status";
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
            dataGrid.Columns.AddRange(new DataGridViewColumn[] { cellNum, cellState, scState, workDate, workTime });
            dataGrid.Enabled = false;
            dataGrid.EnableHeadersVisualStyles = false;
            dataGrid.Location = new Point(12, 12);
            dataGrid.MultiSelect = false;
            dataGrid.Name = "dataGrid";
            dataGrid.ReadOnly = true;
            dataGrid.RightToLeft = RightToLeft.No;
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = SystemColors.Control;
            dataGridViewCellStyle7.Font = new Font("맑은 고딕", 9F);
            dataGridViewCellStyle7.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.True;
            dataGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            dataGrid.RowHeadersVisible = false;
            dataGrid.RowHeadersWidth = 51;
            dataGrid.Size = new Size(755, 457);
            dataGrid.TabIndex = 0;
            dataGrid.TabStop = false;
            // 
            // cellNum
            // 
            cellNum.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cellNum.DefaultCellStyle = dataGridViewCellStyle2;
            cellNum.HeaderText = "Cell Num";
            cellNum.MinimumWidth = 6;
            cellNum.Name = "cellNum";
            cellNum.ReadOnly = true;
            cellNum.Width = 122;
            // 
            // cellState
            // 
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cellState.DefaultCellStyle = dataGridViewCellStyle3;
            cellState.HeaderText = "Result";
            cellState.MinimumWidth = 6;
            cellState.Name = "cellState";
            cellState.ReadOnly = true;
            cellState.Width = 158;
            // 
            // scState
            // 
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            scState.DefaultCellStyle = dataGridViewCellStyle4;
            scState.HeaderText = "Crane State";
            scState.MinimumWidth = 6;
            scState.Name = "scState";
            scState.ReadOnly = true;
            scState.Width = 157;
            // 
            // workDate
            // 
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
            workDate.DefaultCellStyle = dataGridViewCellStyle5;
            workDate.HeaderText = "Work Date";
            workDate.MinimumWidth = 6;
            workDate.Name = "workDate";
            workDate.ReadOnly = true;
            workDate.Width = 158;
            // 
            // workTime
            // 
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter;
            workTime.DefaultCellStyle = dataGridViewCellStyle6;
            workTime.HeaderText = "Work Time";
            workTime.MinimumWidth = 6;
            workTime.Name = "workTime";
            workTime.ReadOnly = true;
            workTime.Width = 157;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 553);
            Controls.Add(dataGrid);
            Controls.Add(listBox);
            Name = "Form1";
            Text = "OPCClinet";
            listBox.ResumeLayout(false);
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGrid).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox listBox;
        private DataGridView dataGrid;
        private DataGridViewTextBoxColumn cellNum;
        private DataGridViewTextBoxColumn cellState;
        private DataGridViewTextBoxColumn scState;
        private DataGridViewTextBoxColumn workDate;
        private DataGridViewTextBoxColumn workTime;
        private GroupBox groupBox;
        private TextBox OPCstateTxt;
        private Button inBtn;
        private Button outBtn;
        private TextBox SQLstateTxt;
    }
}
