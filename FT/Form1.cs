using FT.Data;
using MyToolkit;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FT
{
    public partial class Form1 : Form
    {
        LogFile logfile;

        Communication communication = Communication.singleton;
        //托盘数据
        TrayManager trayManager;

        public Form1()
        {
            InitializeComponent();
            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_Selecting);
            try
            {
                //数据库初始化
                SensorDataManager.InitializeDatabase();
                //设置查询探测器类型
                CB_SensorType.Items.Add("");
                CB_SensorType.Items.Add("金属");
                CB_SensorType.Items.Add("陶瓷");
                CB_SensorType.Items.Add("晶圆");
                //设置查询时间上下限
                DTP_MinTime.Value = Convert.ToDateTime(DateTime.Now.AddDays(-7));
                DTP_MaxTime.Value = Convert.ToDateTime(DateTime.Now.AddDays(1));

                //托盘数据
                trayManager = new TrayManager();
                //托盘类型设置
                foreach (var trayType in trayManager.TrayType)
                {
                    CB_TypeOfTray.Items.Add(trayType.Key);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "程序初始化");
            }
        }

        public void DataUpdate()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        communication.RefreshData();

                        //当前托盘索引更新
                        trayManager.TrayIndex = int.Parse(communication.ReadDataString[20]);
                        //托盘数据初始化
                        if (communication.ReadDataBool[2])
                        {
                            trayManager.InitializeTrays(CB_TypeOfTray.Text);
                            foreach (var tray in trayManager.Trays)
                            {
                                tray.UpdateTrayLabel(PN_Trays);
                            }
                            //托盘初始化完成,PLC检测到此值为true后，将PLC标志位[2]置为false
                            communication.WriteDataBool[2] = true;
                        }
                        //托盘扫码完成
                        if (communication.ReadDataBool[0])
                        {
                            trayManager.SetTrayNumber(communication.ReadDataString[4]);
                            //托盘扫码完成,PLC检测到此值为true后，将PLC标志位[0]置为false
                            communication.WriteDataBool[0] = true;
                        }
                        //产品测试完成
                        if (communication.ReadDataBool[1])
                        {
                            SensorData sensor = new SensorData(communication.ReadDataString[0],
                                communication.ReadDataString[1],
                                communication.ReadDataString[2],
                                int.Parse(communication.ReadDataString[3]),
                                communication.ReadDataString[4],
                                int.Parse(communication.ReadDataString[5]),
                                communication.ReadDataString[6],
                                communication.ReadDataString[7],
                                communication.ReadDataString[8]);
                            //Mapping图更新
                            trayManager.SetSensorDataInTray(sensor);
                            //数据库数据存储
                            SensorDataManager.AddSensor(sensor);
                            //产品信息录入完成。PLC检测到此值为true后，将PLC标志位[1]置为false
                            communication.WriteDataBool[1] = true;
                        }

                        communication.RefreshData();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            });
        }

        void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (textBox1.Text == "" || textBox1.Text == null)//禁用某个Tab
            {
                if (e.TabPageIndex == 20) e.Cancel = true;
            }

        }

        public void logStart_MouseUp(object sender, MouseEventArgs e)
        {
            //textBox1.Text = "0";
            X00.BackColor = Color.White;
        }

        public void logStart_MouseDown(object sender, MouseEventArgs e)
        {
            textBox1.Text = "1";
            logfile = new LogFile();
            
            logfile.Writelog("1");
            logfile.Writelog("2");
            logfile.Writelog("3");
            logfile.Writelog("4");

            X00.BackColor = Color.Lime;
            
        }

        private void ManualBtn_Click(object sender, EventArgs e)
        {

        }

        #region 数据库操作
        private void BTN_AddItem_Click(object sender, EventArgs e)
        {
            //SensorData sensor = new SensorData("1008", "类型2", "1-2", 0, "11111", 2, "合格", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "default");
            //SensorDataManager.AddSensor(sensor);
        }

        private void BTN_SensorInquire_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView.DataSource = SensorDataManager.InquireSensor(TB_SensorCode.Text, CB_SensorType.Text, DTP_MinTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), DTP_MaxTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "查询数据");
            }
        }
        #endregion

        #region Mapping图操作
        private void BTN_Mapping_Click(object sender, EventArgs e)
        {
            //按照类型初始化托盘
            trayManager.InitializeTrays(CB_TypeOfTray.Text);
            foreach (var tray in trayManager.Trays)
            {
                tray.UpdateTrayLabel(PN_Trays);
            }
        }
        
        private void BTN_MappingTest_Click(object sender, EventArgs e)
        {
            PN_Trays.Controls.Clear();
        }

        private void BTN_MappingTest2_Click(object sender, EventArgs e)
        {
            JsonManager.SaveJsonString(Environment.CurrentDirectory + "\\Configuration.json", "123", "321");
        }
        #endregion
        



    }
}
