namespace FT
{
    partial class Form3
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
            this.LB_ErrorLog = new System.Windows.Forms.ListBox();
            this.DTP_CheckDate = new System.Windows.Forms.DateTimePicker();
            this.BTN_错误日志加载 = new System.Windows.Forms.Button();
            this.BTN_错误日志删除 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LB_ErrorLog
            // 
            this.LB_ErrorLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LB_ErrorLog.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LB_ErrorLog.FormattingEnabled = true;
            this.LB_ErrorLog.ItemHeight = 21;
            this.LB_ErrorLog.Location = new System.Drawing.Point(12, 74);
            this.LB_ErrorLog.Name = "LB_ErrorLog";
            this.LB_ErrorLog.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LB_ErrorLog.Size = new System.Drawing.Size(776, 361);
            this.LB_ErrorLog.TabIndex = 0;
            // 
            // DTP_CheckDate
            // 
            this.DTP_CheckDate.Location = new System.Drawing.Point(202, 26);
            this.DTP_CheckDate.Name = "DTP_CheckDate";
            this.DTP_CheckDate.Size = new System.Drawing.Size(200, 21);
            this.DTP_CheckDate.TabIndex = 1;
            // 
            // BTN_错误日志加载
            // 
            this.BTN_错误日志加载.Location = new System.Drawing.Point(408, 26);
            this.BTN_错误日志加载.Name = "BTN_错误日志加载";
            this.BTN_错误日志加载.Size = new System.Drawing.Size(75, 23);
            this.BTN_错误日志加载.TabIndex = 2;
            this.BTN_错误日志加载.Text = "加载";
            this.BTN_错误日志加载.UseVisualStyleBackColor = true;
            this.BTN_错误日志加载.Click += new System.EventHandler(this.BTN_错误日志加载_Click);
            // 
            // BTN_错误日志删除
            // 
            this.BTN_错误日志删除.Location = new System.Drawing.Point(489, 26);
            this.BTN_错误日志删除.Name = "BTN_错误日志删除";
            this.BTN_错误日志删除.Size = new System.Drawing.Size(75, 23);
            this.BTN_错误日志删除.TabIndex = 3;
            this.BTN_错误日志删除.Text = "删除";
            this.BTN_错误日志删除.UseVisualStyleBackColor = true;
            this.BTN_错误日志删除.Click += new System.EventHandler(this.BTN_错误日志删除_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BTN_错误日志删除);
            this.Controls.Add(this.BTN_错误日志加载);
            this.Controls.Add(this.DTP_CheckDate);
            this.Controls.Add(this.LB_ErrorLog);
            this.Name = "Form3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "错误日志";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox LB_ErrorLog;
        private System.Windows.Forms.DateTimePicker DTP_CheckDate;
        private System.Windows.Forms.Button BTN_错误日志加载;
        private System.Windows.Forms.Button BTN_错误日志删除;
    }
}