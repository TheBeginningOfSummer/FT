using FT.Data;
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
    public partial class Form2 : Form
    {
        Dictionary<string, UserData> userData = new Dictionary<string, UserData>();

        public Form2()
        {
            InitializeComponent();
            CB_UserName.Text = "操作员";
            CB_UserName.Items.Add("操作员");
            CB_UserName.Items.Add("工程师");
            CB_UserName.Items.Add("管理员");
            userData.Add("管理员", new UserData() { UserType = 0, UserName = "管理员", Password = "Admin2345", IsLogin = false });
        }

        private void BTN_Login_Click(object sender, EventArgs e)
        {
            if (!userData.ContainsKey(CB_UserName.Text)) return;
            if (userData[CB_UserName.Text].Password == TB_Password.Text)
            {
                Form1 form1 = new Form1(this); form1.Show();
                form1.TC_Main.Selecting += new TabControlCancelEventHandler(TC_Main_Selecting);
                this.Hide();
            }
            else
            {
                MessageBox.Show("密码错误", "登录");
            }
        }

        private void BTN_Modify_Click(object sender, EventArgs e)
        {
            if (CB_UserName.Text == "操作员")
            {
                MessageBox.Show("未授权用户组", "修改密码");
                return;
            }
            if (!userData.ContainsKey(CB_UserName.Text)) return;
            if (userData[CB_UserName.Text].Password == TB_Password.Text)
            {
                
            }
        }

        void TC_Main_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (CB_UserName.Text == "操作员")//禁用某个Tab
            {
                if (e.TabPageIndex == 6) e.Cancel = true;
                if (e.TabPageIndex == 7) e.Cancel = true;
                if (e.TabPageIndex == 9) e.Cancel = true;
            }
        }
    }
}
