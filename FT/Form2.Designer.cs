﻿namespace FT
{
    partial class Form2
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
            this.LB_UserName = new System.Windows.Forms.Label();
            this.CB_UserName = new System.Windows.Forms.ComboBox();
            this.LB_Password = new System.Windows.Forms.Label();
            this.TB_Password = new System.Windows.Forms.TextBox();
            this.BTN_Login = new System.Windows.Forms.Button();
            this.BTN_Modify = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LB_UserName
            // 
            this.LB_UserName.AutoSize = true;
            this.LB_UserName.Location = new System.Drawing.Point(145, 82);
            this.LB_UserName.Name = "LB_UserName";
            this.LB_UserName.Size = new System.Drawing.Size(41, 12);
            this.LB_UserName.TabIndex = 0;
            this.LB_UserName.Text = "用户名";
            // 
            // CB_UserName
            // 
            this.CB_UserName.FormattingEnabled = true;
            this.CB_UserName.Location = new System.Drawing.Point(203, 79);
            this.CB_UserName.Name = "CB_UserName";
            this.CB_UserName.Size = new System.Drawing.Size(121, 20);
            this.CB_UserName.TabIndex = 1;
            // 
            // LB_Password
            // 
            this.LB_Password.AutoSize = true;
            this.LB_Password.Location = new System.Drawing.Point(145, 121);
            this.LB_Password.Name = "LB_Password";
            this.LB_Password.Size = new System.Drawing.Size(29, 12);
            this.LB_Password.TabIndex = 2;
            this.LB_Password.Text = "密码";
            // 
            // TB_Password
            // 
            this.TB_Password.Location = new System.Drawing.Point(203, 118);
            this.TB_Password.Name = "TB_Password";
            this.TB_Password.PasswordChar = '*';
            this.TB_Password.Size = new System.Drawing.Size(121, 21);
            this.TB_Password.TabIndex = 3;
            // 
            // BTN_Login
            // 
            this.BTN_Login.Location = new System.Drawing.Point(147, 169);
            this.BTN_Login.Name = "BTN_Login";
            this.BTN_Login.Size = new System.Drawing.Size(75, 23);
            this.BTN_Login.TabIndex = 4;
            this.BTN_Login.Text = "登录";
            this.BTN_Login.UseVisualStyleBackColor = true;
            this.BTN_Login.Click += new System.EventHandler(this.BTN_Login_Click);
            // 
            // BTN_Modify
            // 
            this.BTN_Modify.Location = new System.Drawing.Point(277, 169);
            this.BTN_Modify.Name = "BTN_Modify";
            this.BTN_Modify.Size = new System.Drawing.Size(75, 23);
            this.BTN_Modify.TabIndex = 5;
            this.BTN_Modify.Text = "修改";
            this.BTN_Modify.UseVisualStyleBackColor = true;
            this.BTN_Modify.Click += new System.EventHandler(this.BTN_Modify_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.BTN_Modify);
            this.Controls.Add(this.BTN_Login);
            this.Controls.Add(this.TB_Password);
            this.Controls.Add(this.LB_Password);
            this.Controls.Add(this.CB_UserName);
            this.Controls.Add(this.LB_UserName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "登录";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LB_UserName;
        private System.Windows.Forms.ComboBox CB_UserName;
        private System.Windows.Forms.Label LB_Password;
        private System.Windows.Forms.TextBox TB_Password;
        private System.Windows.Forms.Button BTN_Login;
        private System.Windows.Forms.Button BTN_Modify;
    }
}