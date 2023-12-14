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
            groupBox3 = new GroupBox();
            listView = new ListView();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(listView);
            groupBox3.Location = new Point(4, 12);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(1253, 953);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "groupBox3";
            // 
            // listView
            // 
            listView.Location = new Point(6, 26);
            listView.Name = "listView";
            listView.Size = new Size(1241, 927);
            listView.TabIndex = 0;
            listView.UseCompatibleStateImageBehavior = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 977);
            Controls.Add(groupBox3);
            Name = "Form1";
            Text = "OPCClinet";
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private GroupBox groupBox3;
        private ListView listView;
    }
}
