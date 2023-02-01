using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public Form3()
        {
            InitializeComponent();
        }

        private void BTN_错误日志加载_Click(object sender, EventArgs e)
        {
            errorList.Clear();
            errorList.AddRange(logFile.ReadLog("20220616报警记录"));
            LB_ErrorLog.DataSource = errorList;
        }

        private void BTN_错误日志删除_Click(object sender, EventArgs e)
        {
            List<string> deleteList = new List<string>();
            foreach (object item in LB_ErrorLog.SelectedItems)
            {
                deleteList.Add(item.ToString());
            }
        }
    }
}
