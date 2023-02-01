using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FT
{
    public partial class Form3 : Form
    {
        readonly LogFile logFile = new LogFile();
        readonly List<string> errorList = new List<string>();
        string filePath;

        public Form3()
        {
            InitializeComponent();
        }

        private void BTN_错误日志加载_Click(object sender, EventArgs e)
        {
            LB_ErrorLog.DataSource = null;
            errorList.Clear();
            filePath = $"{DTP_CheckDate.Value:yyyyMMdd}报警记录";
            errorList.AddRange(logFile.ReadLog(filePath));
            LB_ErrorLog.DataSource = errorList;
        }

        private void BTN_错误日志删除_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (object item in LB_ErrorLog.SelectedItems)
                {
                    errorList.RemoveAll(s => s.Contains(item.ToString()));
                }
                File.WriteAllLines($"{AppDomain.CurrentDomain.BaseDirectory}log\\{filePath}.log", errorList.ToArray());
                LB_ErrorLog.DataSource = null;
                errorList.Clear();
                errorList.AddRange(logFile.ReadLog(filePath));
                LB_ErrorLog.DataSource = errorList;
                MessageBox.Show("删除成功。", "日志删除");
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败。" + ex.Message, "日志删除");
            }
        }
    }
}
