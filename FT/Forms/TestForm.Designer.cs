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
            this.TC测试 = new System.Windows.Forms.TabControl();
            this.TPPLC数据访问 = new System.Windows.Forms.TabPage();
            this.TP文件生成 = new System.Windows.Forms.TabPage();
            this.TB信息 = new System.Windows.Forms.TextBox();
            this.GB添加值 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BTN保存 = new System.Windows.Forms.Button();
            this.TB_Key = new System.Windows.Forms.TextBox();
            this.BTN清除 = new System.Windows.Forms.Button();
            this.TB_Value = new System.Windows.Forms.TextBox();
            this.TB_Modify = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.BTN添加 = new System.Windows.Forms.Button();
            this.BTN查找 = new System.Windows.Forms.Button();
            this.GB_ReadData.SuspendLayout();
            this.GB_WriteData.SuspendLayout();
            this.TC测试.SuspendLayout();
            this.TPPLC数据访问.SuspendLayout();
            this.TP文件生成.SuspendLayout();
            this.GB添加值.SuspendLayout();
            this.SuspendLayout();
            // 
            // PN_TestFormData
            // 
            this.PN_TestFormData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PN_TestFormData.AutoScroll = true;
            this.PN_TestFormData.Location = new System.Drawing.Point(6, 6);
            this.PN_TestFormData.Name = "PN_TestFormData";
            this.PN_TestFormData.Size = new System.Drawing.Size(607, 412);
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
            this.GB_ReadData.Location = new System.Drawing.Point(619, 9);
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
            this.GB_WriteData.Location = new System.Drawing.Point(619, 115);
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
            // TC测试
            // 
            this.TC测试.Controls.Add(this.TPPLC数据访问);
            this.TC测试.Controls.Add(this.TP文件生成);
            this.TC测试.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TC测试.Location = new System.Drawing.Point(0, 0);
            this.TC测试.Name = "TC测试";
            this.TC测试.SelectedIndex = 0;
            this.TC测试.Size = new System.Drawing.Size(800, 450);
            this.TC测试.TabIndex = 4;
            // 
            // TPPLC数据访问
            // 
            this.TPPLC数据访问.Controls.Add(this.PN_TestFormData);
            this.TPPLC数据访问.Controls.Add(this.GB_ReadData);
            this.TPPLC数据访问.Controls.Add(this.GB_WriteData);
            this.TPPLC数据访问.Location = new System.Drawing.Point(4, 22);
            this.TPPLC数据访问.Name = "TPPLC数据访问";
            this.TPPLC数据访问.Padding = new System.Windows.Forms.Padding(3);
            this.TPPLC数据访问.Size = new System.Drawing.Size(792, 424);
            this.TPPLC数据访问.TabIndex = 0;
            this.TPPLC数据访问.Text = "数据访问";
            this.TPPLC数据访问.UseVisualStyleBackColor = true;
            // 
            // TP文件生成
            // 
            this.TP文件生成.Controls.Add(this.BTN查找);
            this.TP文件生成.Controls.Add(this.TB信息);
            this.TP文件生成.Controls.Add(this.GB添加值);
            this.TP文件生成.Location = new System.Drawing.Point(4, 22);
            this.TP文件生成.Name = "TP文件生成";
            this.TP文件生成.Padding = new System.Windows.Forms.Padding(3);
            this.TP文件生成.Size = new System.Drawing.Size(792, 424);
            this.TP文件生成.TabIndex = 1;
            this.TP文件生成.Text = "文件生成";
            this.TP文件生成.UseVisualStyleBackColor = true;
            // 
            // TB信息
            // 
            this.TB信息.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TB信息.Location = new System.Drawing.Point(8, 6);
            this.TB信息.Multiline = true;
            this.TB信息.Name = "TB信息";
            this.TB信息.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TB信息.Size = new System.Drawing.Size(522, 410);
            this.TB信息.TabIndex = 9;
            // 
            // GB添加值
            // 
            this.GB添加值.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GB添加值.Controls.Add(this.label1);
            this.GB添加值.Controls.Add(this.BTN保存);
            this.GB添加值.Controls.Add(this.TB_Key);
            this.GB添加值.Controls.Add(this.BTN清除);
            this.GB添加值.Controls.Add(this.TB_Value);
            this.GB添加值.Controls.Add(this.TB_Modify);
            this.GB添加值.Controls.Add(this.label2);
            this.GB添加值.Controls.Add(this.BTN添加);
            this.GB添加值.Location = new System.Drawing.Point(536, 6);
            this.GB添加值.Name = "GB添加值";
            this.GB添加值.Size = new System.Drawing.Size(250, 150);
            this.GB添加值.TabIndex = 8;
            this.GB添加值.TabStop = false;
            this.GB添加值.Text = "添加值";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "key";
            // 
            // BTN保存
            // 
            this.BTN保存.Location = new System.Drawing.Point(168, 114);
            this.BTN保存.Name = "BTN保存";
            this.BTN保存.Size = new System.Drawing.Size(75, 23);
            this.BTN保存.TabIndex = 7;
            this.BTN保存.Text = "保存";
            this.BTN保存.UseVisualStyleBackColor = true;
            this.BTN保存.Click += new System.EventHandler(this.BTN保存_Click);
            // 
            // TB_Key
            // 
            this.TB_Key.Location = new System.Drawing.Point(56, 24);
            this.TB_Key.Name = "TB_Key";
            this.TB_Key.Size = new System.Drawing.Size(166, 21);
            this.TB_Key.TabIndex = 0;
            // 
            // BTN清除
            // 
            this.BTN清除.Location = new System.Drawing.Point(87, 114);
            this.BTN清除.Name = "BTN清除";
            this.BTN清除.Size = new System.Drawing.Size(75, 23);
            this.BTN清除.TabIndex = 6;
            this.BTN清除.Text = "清除";
            this.BTN清除.UseVisualStyleBackColor = true;
            this.BTN清除.Click += new System.EventHandler(this.BTN清除_Click);
            // 
            // TB_Value
            // 
            this.TB_Value.Location = new System.Drawing.Point(56, 60);
            this.TB_Value.Name = "TB_Value";
            this.TB_Value.Size = new System.Drawing.Size(166, 21);
            this.TB_Value.TabIndex = 1;
            // 
            // TB_Modify
            // 
            this.TB_Modify.Location = new System.Drawing.Point(56, 87);
            this.TB_Modify.Name = "TB_Modify";
            this.TB_Modify.Size = new System.Drawing.Size(166, 21);
            this.TB_Modify.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "value";
            // 
            // BTN添加
            // 
            this.BTN添加.Location = new System.Drawing.Point(6, 114);
            this.BTN添加.Name = "BTN添加";
            this.BTN添加.Size = new System.Drawing.Size(75, 23);
            this.BTN添加.TabIndex = 4;
            this.BTN添加.Text = "添加";
            this.BTN添加.UseVisualStyleBackColor = true;
            this.BTN添加.Click += new System.EventHandler(this.BTN添加_Click);
            // 
            // BTN查找
            // 
            this.BTN查找.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BTN查找.Location = new System.Drawing.Point(542, 162);
            this.BTN查找.Name = "BTN查找";
            this.BTN查找.Size = new System.Drawing.Size(75, 23);
            this.BTN查找.TabIndex = 10;
            this.BTN查找.Text = "查找";
            this.BTN查找.UseVisualStyleBackColor = true;
            this.BTN查找.Click += new System.EventHandler(this.BTN查找_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TC测试);
            this.Controls.Add(this.BTN_Test);
            this.Name = "TestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "测试窗口";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestForm_FormClosing);
            this.GB_ReadData.ResumeLayout(false);
            this.GB_ReadData.PerformLayout();
            this.GB_WriteData.ResumeLayout(false);
            this.GB_WriteData.PerformLayout();
            this.TC测试.ResumeLayout(false);
            this.TPPLC数据访问.ResumeLayout(false);
            this.TP文件生成.ResumeLayout(false);
            this.TP文件生成.PerformLayout();
            this.GB添加值.ResumeLayout(false);
            this.GB添加值.PerformLayout();
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
        private System.Windows.Forms.TabControl TC测试;
        private System.Windows.Forms.TabPage TPPLC数据访问;
        private System.Windows.Forms.TabPage TP文件生成;
        private System.Windows.Forms.TextBox TB_Value;
        private System.Windows.Forms.TextBox TB_Key;
        private System.Windows.Forms.Button BTN添加;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TB_Modify;
        private System.Windows.Forms.Button BTN清除;
        private System.Windows.Forms.Button BTN保存;
        private System.Windows.Forms.GroupBox GB添加值;
        private System.Windows.Forms.TextBox TB信息;
        private System.Windows.Forms.Button BTN查找;
    }
}