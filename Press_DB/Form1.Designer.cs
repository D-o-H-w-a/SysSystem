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
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle9 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            uiBox = new GroupBox();
            stateTxt = new TextBox();
            inBtn = new Button();
            outBtn = new Button();
            listBox = new GroupBox();
            dataGrid = new DataGridView();
            cellNum = new DataGridViewTextBoxColumn();
            cellState = new DataGridViewTextBoxColumn();
            workDate = new DataGridViewTextBoxColumn();
            workTime = new DataGridViewTextBoxColumn();
            uiBox.SuspendLayout();
            listBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGrid).BeginInit();
            SuspendLayout();
            // 
            // uiBox
            // 
            uiBox.Controls.Add(stateTxt);
            uiBox.Controls.Add(inBtn);
            uiBox.Controls.Add(outBtn);
            uiBox.Location = new Point(9, 12);
            uiBox.Name = "uiBox";
            uiBox.Size = new Size(1241, 140);
            uiBox.TabIndex = 2;
            uiBox.TabStop = false;
            // 
            // stateTxt
            // 
            stateTxt.BorderStyle = BorderStyle.None;
            stateTxt.Location = new Point(533, 104);
            stateTxt.Name = "stateTxt";
            stateTxt.ReadOnly = true;
            stateTxt.Size = new Size(154, 20);
            stateTxt.TabIndex = 1;
            stateTxt.Text = "State:";
            // 
            // inBtn
            // 
            inBtn.BackColor = SystemColors.HotTrack;
            inBtn.ForeColor = Color.White;
            inBtn.Image = (Image)resources.GetObject("inBtn.Image");
            inBtn.Location = new Point(1120, 13);
            inBtn.Name = "inBtn";
            inBtn.Size = new Size(93, 52);
            inBtn.TabIndex = 0;
            inBtn.Text = "입고";
            inBtn.UseVisualStyleBackColor = false;
            inBtn.Click += inBtn_Click;
            // 
            // outBtn
            // 
            outBtn.BackColor = SystemColors.HotTrack;
            outBtn.ForeColor = Color.White;
            outBtn.Image = (Image)resources.GetObject("outBtn.Image");
            outBtn.Location = new Point(1120, 82);
            outBtn.Name = "outBtn";
            outBtn.Size = new Size(93, 52);
            outBtn.TabIndex = 0;
            outBtn.Text = "출고";
            outBtn.UseVisualStyleBackColor = false;
            outBtn.Click += outBtn_Click;
            // 
            // listBox
            // 
            listBox.Controls.Add(dataGrid);
            listBox.Location = new Point(4, 158);
            listBox.Name = "listBox";
            listBox.Size = new Size(1253, 807);
            listBox.TabIndex = 1;
            listBox.TabStop = false;
            // 
            // dataGrid
            // 
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = SystemColors.Control;
            dataGridViewCellStyle6.Font = new Font("맑은 고딕", 9F);
            dataGridViewCellStyle6.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGrid.Columns.AddRange(new DataGridViewColumn[] { cellNum, cellState, workDate, workTime });
            dataGrid.Location = new Point(0, 17);
            dataGrid.Name = "dataGrid";
            dataGrid.RightToLeft = RightToLeft.No;
            dataGrid.RowHeadersVisible = false;
            dataGrid.RowHeadersWidth = 51;
            dataGrid.Size = new Size(1249, 784);
            dataGrid.TabIndex = 0;
            // 
            // cellNum
            // 
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cellNum.DefaultCellStyle = dataGridViewCellStyle7;
            cellNum.HeaderText = "셀 번호";
            cellNum.MinimumWidth = 6;
            cellNum.Name = "cellNum";
            cellNum.ReadOnly = true;
            cellNum.Width = 125;
            // 
            // cellState
            // 
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cellState.DefaultCellStyle = dataGridViewCellStyle8;
            cellState.HeaderText = "셀 상태";
            cellState.MinimumWidth = 6;
            cellState.Name = "cellState";
            cellState.ReadOnly = true;
            cellState.Width = 875;
            // 
            // workDate
            // 
            dataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.MiddleCenter;
            workDate.DefaultCellStyle = dataGridViewCellStyle9;
            workDate.HeaderText = "작업 날짜";
            workDate.MinimumWidth = 6;
            workDate.Name = "workDate";
            workDate.ReadOnly = true;
            workDate.Width = 125;
            // 
            // workTime
            // 
            dataGridViewCellStyle10.Alignment = DataGridViewContentAlignment.MiddleCenter;
            workTime.DefaultCellStyle = dataGridViewCellStyle10;
            workTime.HeaderText = "작업 시간";
            workTime.MinimumWidth = 6;
            workTime.Name = "workTime";
            workTime.ReadOnly = true;
            workTime.Width = 125;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 977);
            Controls.Add(uiBox);
            Controls.Add(listBox);
            Name = "Form1";
            Text = "OPCClinet";
            uiBox.ResumeLayout(false);
            uiBox.PerformLayout();
            listBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGrid).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox uiBox;
        private Button inBtn;
        private Button outBtn;
        private TextBox stateTxt;
        private GroupBox listBox;
        private DataGridView dataGrid;
        private DataGridViewTextBoxColumn cellNum;
        private DataGridViewTextBoxColumn cellState;
        private DataGridViewTextBoxColumn workDate;
        private DataGridViewTextBoxColumn workTime;
    }
}
