using FT.Data;
using MyToolkit;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FT
{
    public partial class Form2 : Form
    {
        public Dictionary<string, UserData> Users = new Dictionary<string, UserData>();

        public Form1 form1;

        public Form2()
        {
            InitializeComponent();
            CB_UserName.Text = "操作员";
            CB_UserName.Items.Add("操作员");
            CB_UserName.Items.Add("工程师");
            CB_UserName.Items.Add("管理员");
            Users.Add("操作员", new UserData() { UserType = 2, UserName = "操作员", Password = "" });
            Users.Add("管理员", new UserData() { UserType = 0, UserName = "管理员", Password = "666666" });
            UserData engineer = JsonManager.ReadJsonString<UserData>(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\FTData", "engineerData");
            if (engineer == null)
            {
                Users.Add("工程师", new UserData() { UserType = 1, UserName = "工程师", Password = "" });
            }
            else
            {
                Users.Add(engineer.UserName, engineer);
            }
        }

        private void BTN_Login_Click(object sender, EventArgs e)
        {
            if (!Users.ContainsKey(CB_UserName.Text)) return;
            if (CB_UserName.Text == "操作员")
            {
                if (form1 == null)
                {
                    form1 = new Form1(this); form1.LB_实时权限显示.Text = "操作员"; form1.Show();
                    form1.TC_Main.Selecting += new TabControlCancelEventHandler(TC_Main_Selecting);
                    this.Hide();
                }
                else
                {
                    form1.LB_实时权限显示.Text = "操作员";
                    form1.Show();
                    this.Hide();
                }
            }
            else if (Users[CB_UserName.Text].Password == TB_Password.Text)
            {
                if (form1 == null)
                {
                    form1 = new Form1(this); form1.LB_实时权限显示.Text = CB_UserName.Text; form1.Show();
                    form1.TC_Main.Selecting += new TabControlCancelEventHandler(TC_Main_Selecting);
                    this.Hide();
                }
                else
                {
                    form1.LB_实时权限显示.Text = CB_UserName.Text;
                    form1.Show();
                    this.Hide();
                }
            }
            else
            {
                MessageBox.Show("密码错误", "登录", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
