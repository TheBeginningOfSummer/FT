using FT.Data;
using MyToolkit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FT
{
    public partial class Form1 : Form
    {
        Communication communication = Communication.singleton;
        //日志文件记录
        LogFile logfile = new LogFile();
        //报警信息
        Dictionary<string, string> alarmInformation;
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
                //报警信息读取
                alarmInformation = JsonManager.ReadJsonString<Dictionary<string, string>>(Environment.CurrentDirectory + "\\Configuration\\Alarm.json");
                
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
                        Thread.Sleep(300);
                        communication.ReadData();

                        //当前托盘索引更新
                        trayManager.TrayIndex = int.Parse(communication.ReadTestInformation[20]);
                        //托盘数据初始化
                        if (communication.ReadFlagBits[2])
                        {
                            trayManager.InitializeTrays(CB_TypeOfTray.Text);
                            foreach (var tray in trayManager.Trays)
                            {
                                tray.UpdateTrayLabel(PN_Trays);
                            }
                            //托盘初始化完成,PLC检测到此值为true后，将PLC标志位[2]置为false
                            communication.WriteFlagBits[2] = true;
                        }
                        //托盘扫码完成
                        if (communication.ReadFlagBits[0])
                        {
                            trayManager.SetTrayNumber(communication.ReadTestInformation[4]);
                            //托盘扫码完成,PLC检测到此值为true后，将PLC标志位[0]置为false
                            communication.WriteFlagBits[0] = true;
                        }
                        //产品测试完成
                        if (communication.ReadFlagBits[1])
                        {
                            SensorData sensor = new SensorData(communication.ReadTestInformation[0],
                                communication.ReadTestInformation[1],
                                communication.ReadTestInformation[2],
                                int.Parse(communication.ReadTestInformation[3]),
                                communication.ReadTestInformation[4],
                                int.Parse(communication.ReadTestInformation[5]),
                                communication.ReadTestInformation[6],
                                communication.ReadTestInformation[7],
                                communication.ReadTestInformation[8]);
                            //Mapping图更新
                            trayManager.SetSensorDataInTray(sensor);
                            //数据库数据存储
                            SensorDataManager.AddSensor(sensor);
                            //产品信息录入完成。PLC检测到此值为true后，将PLC标志位[1]置为false
                            communication.WriteFlagBits[1] = true;
                        }
                        //示教界面数据更新
                        SetTextBoxText(txtX示教吸1实盘第一列, communication.ReadLocation[24]);
                        SetTextBoxText(txtX示教吸2实盘第一列, communication.ReadLocation[25]);

                        //IO信息界面
                        SetLabelColor(communication.ReadPLCIO[0], X00);

                        communication.WriteData();
                    }
                    catch (Exception e)
                    {
                        logfile.Writelog("数据更新循环：" + e.Message);
                    }
                }
            });
        }

        public void AlarmCheck()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(300);
                        for (int i = 0; i < communication.ReadPLCAlarm.Length; i++)
                        {
                            if (communication.ReadPLCAlarm[i])
                            {
                                MessageBox.Show(alarmInformation[i.ToString()], "报警信息");
                                logfile.Writelog(alarmInformation[i.ToString()]);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logfile.Writelog("报警监测循环：" + e.Message);
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

        public void SetLabelColor(bool plcIO, Label indicator)
        {
            if (plcIO)
            {
                indicator.Invoke(new Action(() => indicator.BackColor = Color.Lime));
            }
            else
            {
                indicator.Invoke(new Action(() => indicator.BackColor = Color.White));
            }
        }

        public void SetTextBoxText<T>(TextBox textBox, T variable)
        {
            textBox.Invoke(new Action(() => textBox.Text = variable.ToString()));
        }

        #region 数据库操作
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
            PN_Trays.Controls.Clear();
        }
        
        private void CB_TypeOfTray_SelectedIndexChanged(object sender, EventArgs e)
        {
            communication.WriteProductionData[0] = trayManager.TrayType[CB_TypeOfTray.Text].Index;
        }
        #endregion

        #region 示教操作
        private void btnX示教吸1实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[30] = true;
        }

        private void btnX示教吸1实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[30] = false;
        }





        #endregion


       
    }
}
