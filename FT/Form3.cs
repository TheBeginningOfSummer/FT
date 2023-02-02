using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FT
{
    public partial class Form3 : Form
    {
        readonly List<string> errorList = new List<string>();
        readonly LogFile logFile = new LogFile();
        string filePath;

        public Form3()
        {
            InitializeComponent();
        }

        private void UpdateLog(ListBox listBox)
        {
            listBox.DataSource = null;
            errorList.Clear();
            errorList.AddRange(logFile.ReadLog(filePath));
            listBox.DataSource = errorList;
        }

        private void BTN_错误日志加载_Click(object sender, EventArgs e)
        {
            filePath = $"{DTP_CheckDate.Value:yyyyMMdd}报警记录";
            UpdateLog(LB_ErrorLog);
        }

        private void BTN_错误日志删除_Click(object sender, EventArgs e)
        {
            try
            {
                if (LB_ErrorLog.SelectedItems.Count == 0) return;
                foreach (object item in LB_ErrorLog.SelectedItems)
                    errorList.RemoveAll(s => s.Contains(item.ToString()));
                //File.WriteAllLines($"{AppDomain.CurrentDomain.BaseDirectory}log\\{filePath}.log", errorList.ToArray());
                //UpdateLog(LB_ErrorLog);
                LB_ErrorLog.DataSource = null;
                LB_ErrorLog.DataSource = errorList;
                //MessageBox.Show("删除成功。", "日志删除");
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败。" + ex.Message, "日志删除");
            }
        }
    }
}
