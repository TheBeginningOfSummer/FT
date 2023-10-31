using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FT
{
    public partial class Form3 : Form
    {
        readonly List<string> messageList = new List<string>();
        public string FileName { get; set; } = "报警记录";
        public string FilePath { get; set; } = "Warning";

        public Form3()
        {
            InitializeComponent();
        }

        private void UpdateLog(ListBox listBox)
        {
            listBox.DataSource = null;
            messageList.Clear();
            messageList.AddRange(LogFile.ReadLog($"{DTP_CheckDate.Value:yyyyMMdd}{FileName}", FilePath));
            listBox.DataSource = messageList;
        }

        private void BTN_错误日志加载_Click(object sender, EventArgs e)
        {
            UpdateLog(LB_Log);
        }

        private void BTN_错误日志删除_Click(object sender, EventArgs e)
        {
            try
            {
                if (LB_Log.SelectedItems.Count == 0) return;
                foreach (object item in LB_Log.SelectedItems)
                    messageList.RemoveAll(s => s.Contains(item.ToString()));
                LB_Log.DataSource = null;
                LB_Log.DataSource = messageList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败。" + ex.Message, "日志删除");
            }
        }
    }
}
