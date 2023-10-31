namespace FT
{
    partial class TestForm
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
            this.PN_TestFormData = new System.Windows.Forms.Panel();
            this.GB_ReadData = new System.Windows.Forms.GroupBox();
            this.LB_Interval = new System.Windows.Forms.Label();
            this.BTN_Stop = new System.Windows.Forms.Button();
            this.BTN_Start = new System.Windows.Forms.Button();
            this.LB_数据变量名 = new System.Windows.Forms.Label();
            this.CB_VariableName = new System.Windows.Forms.ComboBox();
            this.GB_WriteData = new System.Windows.Forms.GroupBox();
            this.BNT_WriteData = new System.Windows.Forms.Button();
            this.TB_VariableIndex = new System.Windows.Forms.TextBox();
            this.TB_Variable = new System.Windows.Forms.TextBox();
            this.LB_写入的数据 = new System.Windows.Forms.Label();
            this.BTN_Test = new System.Windows.Forms.Button();
            this.GB_ReadData.SuspendLayout();
            this.GB_WriteData.SuspendLayout();
            this.SuspendLayout();
            // 
            // PN_TestFormData
            // 
            this.PN_TestFormData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PN_TestFormData.AutoScroll = true;
            this.PN_TestFormData.Location = new System.Drawing.Point(2, 2);
            this.PN_TestFormData.Name = "PN_TestFormData";
            this.PN_TestFormData.Size = new System.Drawing.Size(622, 446);
            this.PN_TestFormData.TabIndex = 0;
            // 
            // GB_ReadData
            // 
            this.GB_ReadData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GB_ReadData.Controls.Add(this.LB_Interval);
            this.GB_ReadData.Controls.Add(this.BTN_Stop);
            this.GB_ReadData.Controls.Add(this.BTN_Start);
            this.GB_ReadData.Controls.Add(this.LB_数据变量名);
            this.GB_ReadData.Controls.Add(this.CB_VariableName);
            this.GB_ReadData.Location = new System.Drawing.Point(630, 2);
            this.GB_ReadData.Name = "GB_ReadData";
            this.GB_ReadData.Size = new System.Drawing.Size(167, 100);
            this.GB_ReadData.TabIndex = 1;
            this.GB_ReadData.TabStop = false;
            this.GB_ReadData.Text = "数据读取";
            // 
            // LB_Interval
            // 
            this.LB_Interval.AutoSize = true;
            this.LB_Interval.Location = new System.Drawing.Point(6, 76);
            this.LB_Interval.Name = "LB_Interval";
            this.LB_Interval.Size = new System.Drawing.Size(53, 12);
            this.LB_Interval.TabIndex = 8;
            this.LB_Interval.Text = "刷新间隔";
            // 
            // BTN_Stop
            // 
            this.BTN_Stop.Location = new System.Drawing.Point(86, 40);
            this.BTN_Stop.Name = "BTN_Stop";
            this.BTN_Stop.Size = new System.Drawing.Size(75, 23);
            this.BTN_Stop.TabIndex = 7;
            this.BTN_Stop.Text = "停止";
            this.BTN_Stop.UseVisualStyleBackColor = true;
            this.BTN_Stop.Click += new System.EventHandler(this.BTN_Stop_Click);
            // 
            // BTN_Start
            // 
            this.BTN_Start.Location = new System.Drawing.Point(5, 40);
            this.BTN_Start.Name = "BTN_Start";
            this.BTN_Start.Size = new System.Drawing.Size(75, 23);
            this.BTN_Start.TabIndex = 6;
            this.BTN_Start.Text = "开始";
            this.BTN_Start.UseVisualStyleBackColor = true;
            this.BTN_Start.Click += new System.EventHandler(this.BTN_Start_Click);
            // 
            // LB_数据变量名
            // 
            this.LB_数据变量名.Location = new System.Drawing.Point(6, 17);
            this.LB_数据变量名.Name = "LB_数据变量名";
            this.LB_数据变量名.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.LB_数据变量名.Size = new System.Drawing.Size(65, 12);
            this.LB_数据变量名.TabIndex = 1;
            this.LB_数据变量名.Text = "数据变量名";
            this.LB_数据变量名.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CB_VariableName
            // 
            this.CB_VariableName.FormattingEnabled = true;
            this.CB_VariableName.Location = new System.Drawing.Point(72, 14);
            this.CB_VariableName.Name = "CB_VariableName";
            this.CB_VariableName.Size = new System.Drawing.Size(95, 20);
            this.CB_VariableName.TabIndex = 5;
            this.CB_VariableName.SelectedIndexChanged += new System.EventHandler(this.CB_VariableName_SelectedIndexChanged);
            // 
            // GB_WriteData
            // 
            this.GB_WriteData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GB_WriteData.Controls.Add(this.BNT_WriteData);
            this.GB_WriteData.Controls.Add(this.TB_VariableIndex);
            this.GB_WriteData.Controls.Add(this.TB_Variable);
            this.GB_WriteData.Controls.Add(this.LB_写入的数据);
            this.GB_WriteData.Location = new System.Drawing.Point(630, 108);
            this.GB_WriteData.Name = "GB_WriteData";
            this.GB_WriteData.Size = new System.Drawing.Size(167, 100);
            this.GB_WriteData.TabIndex = 2;
            this.GB_WriteData.TabStop = false;
            this.GB_WriteData.Text = "数据写入";
            // 
            // BNT_WriteData
            // 
            this.BNT_WriteData.Location = new System.Drawing.Point(86, 41);
            this.BNT_WriteData.Name = "BNT_WriteData";
            this.BNT_WriteData.Size = new System.Drawing.Size(75, 23);
            this.BNT_WriteData.TabIndex = 3;
            this.BNT_WriteData.Text = "数据写入";
            this.BNT_WriteData.UseVisualStyleBackColor = true;
            this.BNT_WriteData.Click += new System.EventHandler(this.BNT_WriteData_Click);
            // 
            // TB_VariableIndex
            // 
            this.TB_VariableIndex.Location = new System.Drawing.Point(5, 41);
            this.TB_VariableIndex.Name = "TB_VariableIndex";
            this.TB_VariableIndex.Size = new System.Drawing.Size(75, 21);
            this.TB_VariableIndex.TabIndex = 2;
            // 
            // TB_Variable
            // 
            this.TB_Variable.Location = new System.Drawing.Point(72, 14);
            this.TB_Variable.Name = "TB_Variable";
            this.TB_Variable.Size = new System.Drawing.Size(95, 21);
            this.TB_Variable.TabIndex = 1;
            // 
            // LB_写入的数据
            // 
            this.LB_写入的数据.AutoSize = true;
            this.LB_写入的数据.Location = new System.Drawing.Point(6, 17);
            this.LB_写入的数据.Name = "LB_写入的数据";
            this.LB_写入的数据.Size = new System.Drawing.Size(65, 12);
            this.LB_写入的数据.TabIndex = 0;
            this.LB_写入的数据.Text = "写入的数据";
            // 
            // BTN_Test
            // 
            this.BTN_Test.Location = new System.Drawing.Point(722, 425);
            this.BTN_Test.Name = "BTN_Test";
            this.BTN_Test.Size = new System.Drawing.Size(75, 23);
            this.BTN_Test.TabIndex = 3;
            this.BTN_Test.Text = "测试";
            this.BTN_Test.UseVisualStyleBackColor = true;
            this.BTN_Test.Click += new System.EventHandler(this.BTN_Test_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BTN_Test);
            this.Controls.Add(this.GB_WriteData);
            this.Controls.Add(this.GB_ReadData);
            this.Controls.Add(this.PN_TestFormData);
            this.Name = "TestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "测试窗口";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestForm_FormClosing);
            this.GB_ReadData.ResumeLayout(false);
            this.GB_ReadData.PerformLayout();
            this.GB_WriteData.ResumeLayout(false);
            this.GB_WriteData.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PN_TestFormData;
        private System.Windows.Forms.GroupBox GB_ReadData;
        private System.Windows.Forms.GroupBox GB_WriteData;
        private System.Windows.Forms.Label LB_数据变量名;
        private System.Windows.Forms.ComboBox CB_VariableName;
        private System.Windows.Forms.TextBox TB_Variable;
        private System.Windows.Forms.Label LB_写入的数据;
        private System.Windows.Forms.Button BTN_Stop;
        private System.Windows.Forms.Button BTN_Start;
        private System.Windows.Forms.Label LB_Interval;
        private System.Windows.Forms.Button BNT_WriteData;
        private System.Windows.Forms.TextBox TB_VariableIndex;
        private System.Windows.Forms.Button BTN_Test;
    }
}