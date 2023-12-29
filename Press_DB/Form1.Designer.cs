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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            OPCstateTxt = new TextBox();
            inBtn = new Button();
            outBtn = new Button();
            SQLstateTxt = new TextBox();
            listBox = new GroupBox();
            dataGrid = new DataGridView();
            groupBox = new GroupBox();
            workTime = new DataGridViewTextBoxColumn();
            workDate = new DataGridViewTextBoxColumn();
            scState = new DataGridViewTextBoxColumn();
            cellState = new DataGridViewTextBoxColumn();
            cellNum = new DataGridViewTextBoxColumn();
            listBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGrid).BeginInit();
            groupBox.SuspendLayout();
            SuspendLayout();
            // 
            // OPCstateTxt
            // 
            OPCstateTxt.BackColor = SystemColors.ActiveCaption;
            OPCstateTxt.BorderStyle = BorderStyle.None;
            OPCstateTxt.Font = new Font("맑은 고딕", 10.8F);
            OPCstateTxt.Location = new Point(130, 100);
            OPCstateTxt.Name = "OPCstateTxt";
            OPCstateTxt.ReadOnly = true;
            OPCstateTxt.Size = new Size(164, 24);
            OPCstateTxt.TabIndex = 1;
            OPCstateTxt.Text = "OPC Comm Status";
            // 
            // inBtn
            // 
            inBtn.BackColor = SystemColors.HotTrack;
            inBtn.Font = new Font("맑은 고딕", 9F);
            inBtn.ForeColor = Color.White;
            inBtn.Location = new Point(210, 28);
            inBtn.Name = "inBtn";
            inBtn.Size = new Size(84, 52);
            inBtn.TabIndex = 0;
            inBtn.Text = "입고";
            inBtn.UseVisualStyleBackColor = false;
            inBtn.Click += inBtn_Click;
            // 
            // outBtn
            // 
            outBtn.BackColor = SystemColors.HotTrack;
            outBtn.Font = new Font("맑은 고딕", 9F);
            outBtn.ForeColor = Color.White;
            outBtn.Location = new Point(422, 28);
            outBtn.Name = "outBtn";
            outBtn.Size = new Size(84, 52);
            outBtn.TabIndex = 0;
            outBtn.Text = "출고";
            outBtn.UseVisualStyleBackColor = false;
            outBtn.Click += outBtn_Click;
            // 
            // SQLstateTxt
            // 
            SQLstateTxt.BackColor = SystemColors.ActiveCaption;
            SQLstateTxt.BorderStyle = BorderStyle.None;
            SQLstateTxt.Font = new Font("맑은 고딕", 10.8F);
            SQLstateTxt.Location = new Point(422, 100);
            SQLstateTxt.Name = "SQLstateTxt";
            SQLstateTxt.ReadOnly = true;
            SQLstateTxt.Size = new Size(164, 24);
            SQLstateTxt.TabIndex = 1;
            SQLstateTxt.Text = "SQL Comm Status";
            // 
            // listBox
            // 
            listBox.Controls.Add(dataGrid);
            listBox.Location = new Point(9, 158);
            listBox.Name = "listBox";
            listBox.Size = new Size(761, 383);
            listBox.TabIndex = 1;
            listBox.TabStop = false;
            // 
            // dataGrid
            // 
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
            dataGrid.EnableHeadersVisualStyles = false;
            dataGrid.Location = new Point(6, 26);
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
            dataGrid.Size = new Size(755, 351);
            dataGrid.TabIndex = 0;
            // 
            // groupBox
            // 
            groupBox.BackColor = SystemColors.ActiveCaption;
            groupBox.Controls.Add(OPCstateTxt);
            groupBox.Controls.Add(inBtn);
            groupBox.Controls.Add(outBtn);
            groupBox.Controls.Add(SQLstateTxt);
            groupBox.Font = new Font("맑은 고딕", 1F);
            groupBox.Location = new Point(15, 8);
            groupBox.Name = "groupBox";
            groupBox.Size = new Size(755, 172);
            groupBox.TabIndex = 2;
            groupBox.TabStop = false;
            // 
            // workTime
            // 
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter;
            workTime.DefaultCellStyle = dataGridViewCellStyle6;
            workTime.HeaderText = "작업 시간";
            workTime.MinimumWidth = 6;
            workTime.Name = "workTime";
            workTime.ReadOnly = true;
            workTime.Width = 157;
            // 
            // workDate
            // 
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
            workDate.DefaultCellStyle = dataGridViewCellStyle5;
            workDate.HeaderText = "작업 날짜";
            workDate.MinimumWidth = 6;
            workDate.Name = "workDate";
            workDate.ReadOnly = true;
            workDate.Width = 158;
            // 
            // scState
            // 
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            scState.DefaultCellStyle = dataGridViewCellStyle4;
            scState.HeaderText = "크레인 상태";
            scState.MinimumWidth = 6;
            scState.Name = "scState";
            scState.ReadOnly = true;
            scState.Width = 157;
            // 
            // cellState
            // 
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cellState.DefaultCellStyle = dataGridViewCellStyle3;
            cellState.HeaderText = "처리 결과";
            cellState.MinimumWidth = 6;
            cellState.Name = "cellState";
            cellState.ReadOnly = true;
            cellState.Width = 158;
            // 
            // cellNum
            // 
            cellNum.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cellNum.DefaultCellStyle = dataGridViewCellStyle2;
            cellNum.HeaderText = "셀 번호";
            cellNum.MinimumWidth = 6;
            cellNum.Name = "cellNum";
            cellNum.ReadOnly = true;
            cellNum.Width = 122;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 553);
            Controls.Add(groupBox);
            Controls.Add(listBox);
            Name = "Form1";
            Text = "OPCClinet";
            listBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGrid).EndInit();
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button inBtn;
        private Button outBtn;
        private TextBox OPCstateTxt;
        private GroupBox listBox;
        private DataGridView dataGrid;
        private TextBox SQLstateTxt;
        private GroupBox groupBox;
        private DataGridViewTextBoxColumn cellNum;
        private DataGridViewTextBoxColumn cellState;
        private DataGridViewTextBoxColumn scState;
        private DataGridViewTextBoxColumn workDate;
        private DataGridViewTextBoxColumn workTime;
    }
}
