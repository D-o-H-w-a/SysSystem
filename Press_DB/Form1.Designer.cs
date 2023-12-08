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
            groupBox1 = new GroupBox();
            hostIp_Txt = new TextBox();
            host_Txt = new TextBox();
            disConnect_Btn = new Button();
            connect_Btn = new Button();
            groupBox3 = new GroupBox();
            status_Txt = new TextBox();
            groupBox2 = new GroupBox();
            button1 = new Button();
            read_Btn = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(status_Txt);
            groupBox1.Controls.Add(hostIp_Txt);
            groupBox1.Controls.Add(host_Txt);
            groupBox1.Controls.Add(disConnect_Btn);
            groupBox1.Controls.Add(connect_Btn);
            groupBox1.Location = new Point(4, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(622, 148);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // hostIp_Txt
            // 
            hostIp_Txt.Location = new Point(80, 91);
            hostIp_Txt.Name = "hostIp_Txt";
            hostIp_Txt.Size = new Size(345, 27);
            hostIp_Txt.TabIndex = 1;
            hostIp_Txt.Text = "Host:";
            // 
            // host_Txt
            // 
            host_Txt.Location = new Point(11, 91);
            host_Txt.Name = "host_Txt";
            host_Txt.ReadOnly = true;
            host_Txt.Size = new Size(51, 27);
            host_Txt.TabIndex = 1;
            host_Txt.Text = "Host:";
            // 
            // disConnect_Btn
            // 
            disConnect_Btn.Location = new Point(441, 86);
            disConnect_Btn.Name = "disConnect_Btn";
            disConnect_Btn.Size = new Size(162, 32);
            disConnect_Btn.TabIndex = 0;
            disConnect_Btn.Text = "Disconnect";
            disConnect_Btn.UseVisualStyleBackColor = true;
            // 
            // connect_Btn
            // 
            connect_Btn.Location = new Point(441, 26);
            connect_Btn.Name = "connect_Btn";
            connect_Btn.Size = new Size(162, 32);
            connect_Btn.TabIndex = 0;
            connect_Btn.Text = "Connect";
            connect_Btn.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Location = new Point(4, 168);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(1253, 797);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "groupBox3";
            // 
            // status_Txt
            // 
            status_Txt.Location = new Point(11, 39);
            status_Txt.Name = "status_Txt";
            status_Txt.Size = new Size(226, 27);
            status_Txt.TabIndex = 2;
            status_Txt.Text = "Status: Not Connected";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(button1);
            groupBox2.Controls.Add(read_Btn);
            groupBox2.Location = new Point(635, 14);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(622, 148);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "groupBox2";
            // 
            // button1
            // 
            button1.Location = new Point(441, 86);
            button1.Name = "button1";
            button1.Size = new Size(162, 32);
            button1.TabIndex = 0;
            button1.Text = "Disconnect";
            button1.UseVisualStyleBackColor = true;
            // 
            // read_Btn
            // 
            read_Btn.Location = new Point(441, 26);
            read_Btn.Name = "read_Btn";
            read_Btn.Size = new Size(162, 32);
            read_Btn.TabIndex = 0;
            read_Btn.Text = "Read";
            read_Btn.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 977);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private GroupBox groupBox3;
        private Button disConnect_Btn;
        private Button connect_Btn;
        private TextBox host_Txt;
        private TextBox hostIp_Txt;
        private TextBox status_Txt;
        private GroupBox groupBox2;
        private Button button1;
        private Button read_Btn;
    }
}
