namespace ServiceTuner
{
    partial class Form_GetGroupName
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_GetGroupName));
            this.label_NewGroupName = new System.Windows.Forms.Label();
            this.textBox_NewGroupName = new System.Windows.Forms.TextBox();
            this.button_NewGroupName = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_NewGroupName
            // 
            this.label_NewGroupName.AutoSize = true;
            this.label_NewGroupName.Location = new System.Drawing.Point(89, 17);
            this.label_NewGroupName.Name = "label_NewGroupName";
            this.label_NewGroupName.Size = new System.Drawing.Size(113, 12);
            this.label_NewGroupName.TabIndex = 0;
            this.label_NewGroupName.Text = "输入一个新的组名：";
            // 
            // textBox_NewGroupName
            // 
            this.textBox_NewGroupName.Location = new System.Drawing.Point(70, 40);
            this.textBox_NewGroupName.Name = "textBox_NewGroupName";
            this.textBox_NewGroupName.Size = new System.Drawing.Size(146, 21);
            this.textBox_NewGroupName.TabIndex = 1;
            // 
            // button_NewGroupName
            // 
            this.button_NewGroupName.Location = new System.Drawing.Point(106, 76);
            this.button_NewGroupName.Name = "button_NewGroupName";
            this.button_NewGroupName.Size = new System.Drawing.Size(75, 23);
            this.button_NewGroupName.TabIndex = 2;
            this.button_NewGroupName.Text = "确认";
            this.button_NewGroupName.UseVisualStyleBackColor = true;
            this.button_NewGroupName.Click += new System.EventHandler(this.button_NewGroupName_Click);
            // 
            // Form_GetGroupName
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 111);
            this.Controls.Add(this.button_NewGroupName);
            this.Controls.Add(this.textBox_NewGroupName);
            this.Controls.Add(this.label_NewGroupName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_GetGroupName";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "新组别...";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form_GetGroupName_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_NewGroupName;
        private System.Windows.Forms.TextBox textBox_NewGroupName;
        private System.Windows.Forms.Button button_NewGroupName;
    }
}