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
            listBox = new GroupBox();
            listView = new ListView();
            cellState = new ColumnHeader();
            dateHeader = new ColumnHeader();
            timeHeader = new ColumnHeader();
            uiBox = new GroupBox();
            stateTxt = new TextBox();
            readBtn = new Button();
            setResetBtn = new Button();
            cellNum = new ColumnHeader();
            listBox.SuspendLayout();
            uiBox.SuspendLayout();
            SuspendLayout();
            // 
            // listBox
            // 
            listBox.Controls.Add(listView);
            listBox.Location = new Point(4, 158);
            listBox.Name = "listBox";
            listBox.Size = new Size(1253, 807);
            listBox.TabIndex = 1;
            listBox.TabStop = false;
            // 
            // listView
            // 
            listView.BackColor = SystemColors.Window;
            listView.BorderStyle = BorderStyle.FixedSingle;
            listView.Columns.AddRange(new ColumnHeader[] { cellNum, cellState, dateHeader, timeHeader });
            listView.ForeColor = SystemColors.WindowFrame;
            listView.Location = new Point(5, 26);
            listView.Name = "listView";
            listView.RightToLeft = RightToLeft.No;
            listView.Size = new Size(1241, 781);
            listView.TabIndex = 0;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            // 
            // cellState
            // 
            cellState.Text = "셀 상태";
            cellState.TextAlign = HorizontalAlignment.Center;
            cellState.Width = 826;
            // 
            // dateHeader
            // 
            dateHeader.Text = "작업 날짜";
            dateHeader.TextAlign = HorizontalAlignment.Center;
            dateHeader.Width = 170;
            // 
            // timeHeader
            // 
            timeHeader.Text = "작업 시간";
            timeHeader.TextAlign = HorizontalAlignment.Center;
            timeHeader.Width = 170;
            // 
            // uiBox
            // 
            uiBox.Controls.Add(stateTxt);
            uiBox.Controls.Add(readBtn);
            uiBox.Controls.Add(setResetBtn);
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
            // readBtn
            // 
            readBtn.BackColor = SystemColors.HotTrack;
            readBtn.ForeColor = Color.White;
            readBtn.Location = new Point(1096, 20);
            readBtn.Name = "readBtn";
            readBtn.Size = new Size(139, 52);
            readBtn.TabIndex = 0;
            readBtn.Text = "Read";
            readBtn.UseVisualStyleBackColor = false;
            // 
            // setResetBtn
            // 
            setResetBtn.BackColor = SystemColors.HotTrack;
            setResetBtn.ForeColor = Color.White;
            setResetBtn.Location = new Point(1096, 82);
            setResetBtn.Name = "setResetBtn";
            setResetBtn.Size = new Size(139, 52);
            setResetBtn.TabIndex = 0;
            setResetBtn.Text = "SetReset";
            setResetBtn.UseVisualStyleBackColor = false;
            // 
            // cellNum
            // 
            cellNum.Text = "셀 번호";
            cellNum.Width = 70;
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
            listBox.ResumeLayout(false);
            uiBox.ResumeLayout(false);
            uiBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox listBox;
        private GroupBox uiBox;
        private Button readBtn;
        private Button setResetBtn;
        private TextBox stateTxt;
        private ListView listView;
        private ColumnHeader cellState;
        private ColumnHeader dateHeader;
        private ColumnHeader timeHeader;
        private ColumnHeader cellNum;
    }
}
