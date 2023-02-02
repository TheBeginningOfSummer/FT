using FT.Data;
using MyToolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FT
{
    public partial class Form1 : Form
    {
        Communication communication = Communication.Singleton;
        //日志文件记录
        LogFile logfile = new LogFile();
        //加载的报警信息
        Dictionary<string, string> alarmInformation;
        //当前报警信息
        List<string> warning = new List<string>();
        //托盘数据
        TrayManager trayManager;
        //托盘类型
        string trayType;
        //登录界面
        Form2 loginForm;
        //是否更新数据
        bool isUpdateData = false;

        readonly Stopwatch stopwatch1 = new Stopwatch();
        readonly Stopwatch stopwatch2 = new Stopwatch();

        public Form1(Form2 form2)
        {
            InitializeComponent();

            loginForm = form2;
            //数据初始化
            try
            {
                #region 数据查询
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
                #endregion

                #region 托盘数据加载
                //托盘数据
                trayManager = new TrayManager();
                //托盘类型设置
                foreach (var trayType in trayManager.TrayType)
                {
                    CB_TypeOfTray.Items.Add(trayType.Key);
                }
                trayType = "";
                //加载上次的托盘状态
                trayManager.LoadTraysData(trayType);
                //更新托盘状态到界面
                trayManager.UpdateTrayLabels(PN_Trays);
                #endregion

                //报警信息读取
                alarmInformation = JsonManager.ReadJsonString<Dictionary<string, string>>(Environment.CurrentDirectory + "\\Configuration\\", "Alarm");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "程序初始化", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //打开通信端口
            try
            {
                communication.Compolet.Open();
                isUpdateData = true;
                DataUpdate();
                InterfaceUpdate();
                AlarmCheck();
            }
            catch (Exception ex)
            {
                isUpdateData = false;
                MessageBox.Show(ex.Message, "打开端口", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DataUpdate()
        {
            Task.Run(() =>
            {
                while (isUpdateData)
                {
                    try
                    {
                        stopwatch1.Restart();
                        Thread.Sleep(100);
                        communication.RefreshData();
                        #region 数据处理
                        if (communication.ReadTestInformation[20] != "" && communication.ReadTestInformation[20] != null)
                        {
                            //当前托盘索引更新
                            trayManager.TrayIndex = int.Parse(communication.ReadTestInformation[20]);
                        }
                        else
                        {
                            //托盘索引为0时，无法为托盘附码
                            trayManager.TrayIndex = 0;
                        }
                        //托盘数据初始化
                        if (communication.ReadFlagBits[2])
                        {
                            if (trayType != "" && trayType != " ")
                            {
                                //初始化
                                trayManager.InitializeTrays(trayType);
                                trayManager.UpdateTrayLabels(PN_Trays);
                                //托盘初始化完成,PLC检测到此值为true后，将PLC标志位[2]置为false
                                communication.WriteVariable(true, "PC标志位[2]");
                            }
                        }
                        //托盘扫码完成
                        if (communication.ReadFlagBits[0])
                        {
                            //将扫到的托盘码赋值给托盘管理类
                            trayManager.SetTrayNumber(communication.ReadTestInformation[4]);
                            //托盘扫码完成,PLC检测到此值为true后，将PLC标志位[0]置为false
                            communication.WriteVariable(true, "PC标志位[0]");
                        }
                        //产品测试完成
                        if (communication.ReadFlagBits[1])
                        {
                            Sensor sensor = new Sensor(
                                communication.ReadTestInformation[0],
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
                            //数据存储
                            trayManager.SaveTraysData();
                            //数据库数据存储
                            SensorDataManager.AddSensor(sensor);
                            //产品信息录入完成。将PLC标志位[1]置为false，PC标志位置true，表示准备好下一次的录入
                            communication.WriteVariable(false, "PLC标志位[1]");
                            communication.WriteVariable(true, "PC标志位[1]");
                        }
                        #endregion
                        stopwatch1.Stop();
                        logfile.WriteLog("更新数据结束", "更新数据");
                        label93.Invoke(new Action(() => label93.Text = $"{stopwatch1.ElapsedMilliseconds}ms"));
                    }
                    catch (Exception e)
                    {
                        logfile.WriteLog($"数据更新出错。{e.Message}", "更新数据");
                    }
                }
            });
        }

        public void InterfaceUpdate()
        {
            Task.Run(() =>
            {
                while (isUpdateData)
                {
                    try
                    {
                        stopwatch2.Restart();
                        Thread.Sleep(150);
                        #region 更新数据
                        //示教界面数据更新
                        SetTextBoxText(txtX示教吸1实盘第一列, communication.ReadLocation[24]);
                        //SetTextBoxText(txtX示教吸2实盘第一列, communication.ReadLocation[25]);
                        SetTextBoxText(txtX示教吸1倒实盘第一列, communication.ReadLocation[26]);
                        //SetTextBoxText(txtX示教吸2倒实盘第一列, communication.ReadLocation[27]);
                        //SetTextBoxText(txtX示教吸1NG盘第一列, communication.ReadLocation[28]);
                        //SetTextBoxText(txtX示教吸2NG盘第一列, communication.ReadLocation[29]);
                        SetTextBoxText(txtX示教实盘位置, communication.ReadLocation[30]);
                        SetTextBoxText(txtX示教倒实盘位置, communication.ReadLocation[31]);
                        SetTextBoxText(txtX示教NG盘位置, communication.ReadLocation[32]);
                        SetTextBoxText(txtX示教倒NG盘位置, communication.ReadLocation[33]);
                        SetTextBoxText(txtX示教夹爪位置, communication.ReadLocation[34]);
                        SetTextBoxText(txtX示教扫码位置, communication.ReadLocation[35]);
                        SetTextBoxText(txtX示教视觉实盘第一列, communication.ReadLocation[36]);
                        //SetTextBoxText(txtX示教视觉倒实盘第一列, communication.ReadLocation[37]);

                        SetTextBoxText(txtY示教吸1实盘第一行, communication.ReadLocation[41]);
                        SetTextBoxText(txtY示教吸2实盘第一行, communication.ReadLocation[42]);
                        SetTextBoxText(txtY示教吸1倒实盘第一行, communication.ReadLocation[43]);
                        SetTextBoxText(txtY示教吸2倒实盘第一行, communication.ReadLocation[44]);
                        SetTextBoxText(txtY示教吸1NG盘第一行, communication.ReadLocation[45]);
                        SetTextBoxText(txtY示教吸2NG盘第一行, communication.ReadLocation[46]);
                        SetTextBoxText(txtY示教实盘位置, communication.ReadLocation[47]);
                        SetTextBoxText(txtY示教倒实盘位置, communication.ReadLocation[48]);
                        SetTextBoxText(txtY示教NG盘位置, communication.ReadLocation[49]);
                        SetTextBoxText(txtY示教倒NG盘位置, communication.ReadLocation[50]);
                        SetTextBoxText(txtY示教夹爪位置, communication.ReadLocation[51]);
                        SetTextBoxText(txtY示教扫码位置, communication.ReadLocation[52]);
                        SetTextBoxText(txtY示教视觉实盘第一列, communication.ReadLocation[53]);
                        SetTextBoxText(txtY示教视觉倒实盘第一列, communication.ReadLocation[54]);

                        SetTextBoxText(txt实盘示教初始位置, communication.ReadLocation[58]);
                        //SetTextBoxText(txt实盘示教扫码位置, communication.ReadLocation[59]);

                        SetTextBoxText(txtNG盘示教初始位置, communication.ReadLocation[65]);

                        //SetTextBoxText(txt倒实盘示教初始位置, communication.ReadLocation[69]);

                        SetTextBoxText(txt倒NG盘示教初始位置, communication.ReadLocation[73]);

                        SetTextBoxText(txt平移示教上料位置, communication.ReadLocation[77]);
                        SetTextBoxText(txt平移示教下料位置, communication.ReadLocation[78]);
                        SetTextBoxText(txt平移示教中转位置, communication.ReadLocation[79]);
                        SetTextBoxText(txt平移示教中转位置2, communication.ReadLocation[80]);

                        SetTextBoxText(txtBYX示教上料位置, communication.ReadLocation[83]);
                        SetTextBoxText(txtBYX示教视觉1位置, communication.ReadLocation[84]);
                        SetTextBoxText(txtBYX示教第一列, communication.ReadLocation[85]);
                        SetTextBoxText(txtBYX示教第二列, communication.ReadLocation[86]);
                        SetTextBoxText(txtBYX示教第三列, communication.ReadLocation[87]);
                        SetTextBoxText(txtBYX示教第四列, communication.ReadLocation[88]);
                        SetTextBoxText(txtBYX示教第五列, communication.ReadLocation[89]);
                        SetTextBoxText(txtBYX示教第六列, communication.ReadLocation[90]);
                        SetTextBoxText(txtBYX示教第七列, communication.ReadLocation[91]);
                        SetTextBoxText(txtBYX示教第八列, communication.ReadLocation[92]);
                        SetTextBoxText(txtBYX示教视觉2位置, communication.ReadLocation[93]);
                        SetTextBoxText(txtBYX示教矫正位置, communication.ReadLocation[169]);
                        SetTextBoxText(txtBYX示教矫正位置2, communication.ReadLocation[170]);
                        SetTextBoxText(txtBYX示教矫正位置3, communication.ReadLocation[171]);
                        SetTextBoxText(txtBYX示教矫正位置4, communication.ReadLocation[172]);
                        SetTextBoxText(txtBYX示教矫正位置5, communication.ReadLocation[173]);
                        SetTextBoxText(txtBYX示教矫正位置6, communication.ReadLocation[174]);
                        SetTextBoxText(txtBYX示教矫正位置7, communication.ReadLocation[175]);
                        SetTextBoxText(txtBYX示教矫正位置8, communication.ReadLocation[176]);

                        SetTextBoxText(txtBYY示教Sokt夹爪3, communication.ReadLocation[95]);
                        SetTextBoxText(txtBYY示教Sokt夹爪4, communication.ReadLocation[96]);

                        SetTextBoxText(txtBYY示教Sokt夹爪1记忆, communication.ReadLocation[82]);
                        SetTextBoxText(txtBYY示教上料12位置, communication.ReadLocation[97]);
                        SetTextBoxText(txtBYY示教视觉1位置, communication.ReadLocation[98]);
                        SetTextBoxText(txtBYY示教视觉2位置, communication.ReadLocation[99]);
                        SetTextBoxText(txtBYY示教第一行, communication.ReadLocation[100]);
                        SetTextBoxText(txtBYY示教第二行, communication.ReadLocation[101]);
                        SetTextBoxText(txtBYY示教视觉3位置, communication.ReadLocation[102]);
                        SetTextBoxText(txtBYY示教视觉4位置, communication.ReadLocation[103]);
                        SetTextBoxText(txtBYY示教上料34位置, communication.ReadLocation[104]);
                        SetTextBoxText(txtBYY示教视觉5位置, communication.ReadLocation[105]);
                        SetTextBoxText(txtBYY示教视觉6位置, communication.ReadLocation[106]);

                        SetTextBoxText(txtBYZ示教上料位置, communication.ReadLocation[107]);
                        SetTextBoxText(txtBYZ示教上升位置, communication.ReadLocation[108]);
                        SetTextBoxText(txtBYZ示教视觉位置1, communication.ReadLocation[109]);
                        SetTextBoxText(txtBYZ示教视觉位置2, communication.ReadLocation[110]);
                        SetTextBoxText(txtBYZ下料视觉位置, communication.ReadLocation[111]);
                        SetTextBoxText(txtBYZ示教视觉位置3, communication.ReadLocation[112]);

                        SetTextBoxText(txtSk1示教黑体位置, communication.ReadLocation[117]);
                        SetTextBoxText(txtSk1示教翻转位置, communication.ReadLocation[118]);
                        //SetTextBoxText(txtSk1示教测试1位置, communication.ReadLocation[119]);
                        //SetTextBoxText(txtSk1示教测试2位置, communication.ReadLocation[120]);
                        //SetTextBoxText(txtSk1示教测试3位置, communication.ReadLocation[121]);

                        SetTextBoxText(txtSk2示教黑体位置, communication.ReadLocation[125]);
                        SetTextBoxText(txtSk2示教翻转位置, communication.ReadLocation[126]);
                        //SetTextBoxText(txtSk2示教测试1位置, communication.ReadLocation[127]);
                        //SetTextBoxText(txtSk2示教测试2位置, communication.ReadLocation[128]);
                        //SetTextBoxText(txtSk2示教测试3位置, communication.ReadLocation[129]);

                        SetTextBoxText(txtSk3示教黑体位置, communication.ReadLocation[133]);
                        SetTextBoxText(txtSk3示教翻转位置, communication.ReadLocation[134]);
                        //SetTextBoxText(txtSk3示教测试1位置, communication.ReadLocation[135]);
                        //SetTextBoxText(txtSk3示教测试2位置, communication.ReadLocation[136]);
                        //SetTextBoxText(txtSk3示教测试3位置, communication.ReadLocation[137]);

                        SetTextBoxText(txtSk4示教黑体位置, communication.ReadLocation[141]);
                        SetTextBoxText(txtSk4示教翻转位置, communication.ReadLocation[142]);
                        //SetTextBoxText(txtSk4示教测试1位置, communication.ReadLocation[143]);
                        //SetTextBoxText(txtSk4示教测试2位置, communication.ReadLocation[144]);
                        //SetTextBoxText(txtSk4示教测试3位置, communication.ReadLocation[145]);

                        SetTextBoxText(txtBk1示教20位置, communication.ReadLocation[149]);
                        SetTextBoxText(txtBk1示教35位置, communication.ReadLocation[150]);

                        SetTextBoxText(txtBk2示教20位置, communication.ReadLocation[154]);
                        SetTextBoxText(txtBk2示教35位置, communication.ReadLocation[155]);

                        SetTextBoxText(txtBk3示教20位置, communication.ReadLocation[159]);
                        SetTextBoxText(txtBk3示教35位置, communication.ReadLocation[160]);

                        SetTextBoxText(txtBk4示教20位置, communication.ReadLocation[164]);
                        SetTextBoxText(txtBk4示教35位置, communication.ReadLocation[165]);

                        SetTextBoxText(txt托盘上料个数, communication.ReadLocation[60]);
                        SetTextBoxText(txt托盘下料个数, communication.ReadLocation[61]);
                        SetTextBoxText(txtSokt上料个数, communication.ReadLocation[122]);
                        SetTextBoxText(txtSokt下料个数, communication.ReadLocation[123]);



                        //当前位置
                        SetTextBoxText(txtX示教当前位置, communication.ReadLocation[0]);
                        SetTextBoxText(txtY示教当前位置, communication.ReadLocation[1]);
                        SetTextBoxText(txt实盘示教当前位置, communication.ReadLocation[2]);
                        SetTextBoxText(txtNG盘示教当前位置, communication.ReadLocation[3]);
                        //SetTextBoxText(txt倒实盘示教当前位置, communication.ReadLocation[4]);
                        SetTextBoxText(txt倒NG盘示教当前位置, communication.ReadLocation[5]);
                        SetTextBoxText(txt平移示教当前位置, communication.ReadLocation[6]);
                        SetTextBoxText(txtBYX示教当前位置, communication.ReadLocation[7]);
                        SetTextBoxText(txtBYY示教当前位置, communication.ReadLocation[8]);
                        SetTextBoxText(txtBYZ示教当前位置, communication.ReadLocation[9]);
                        SetTextBoxText(txtSk1示教当前位置, communication.ReadLocation[10]);
                        SetTextBoxText(txtSk2示教当前位置, communication.ReadLocation[11]);
                        SetTextBoxText(txtSk3示教当前位置, communication.ReadLocation[12]);
                        SetTextBoxText(txtSk4示教当前位置, communication.ReadLocation[13]);
                        SetTextBoxText(txtBk1示教当前位置, communication.ReadLocation[14]);
                        SetTextBoxText(txtBk2示教当前位置, communication.ReadLocation[15]);
                        SetTextBoxText(txtBk3示教当前位置, communication.ReadLocation[16]);
                        SetTextBoxText(txtBk4示教当前位置, communication.ReadLocation[17]);


                        SetTextBoxText(txtX当前位置, communication.ReadLocation[0]);
                        SetTextBoxText(txtY当前位置, communication.ReadLocation[1]);
                        SetTextBoxText(txt实盘当前位置, communication.ReadLocation[2]);
                        SetTextBoxText(txtNG盘当前位置, communication.ReadLocation[3]);
                        SetTextBoxText(txt倒实盘当前位置, communication.ReadLocation[4]);
                        SetTextBoxText(txt倒NG盘当前位置, communication.ReadLocation[5]);
                        SetTextBoxText(txtBY平移当前位置, communication.ReadLocation[6]);
                        SetTextBoxText(txtBYX当前位置, communication.ReadLocation[7]);
                        SetTextBoxText(txtBYY当前位置, communication.ReadLocation[8]);
                        SetTextBoxText(txtBYZ当前位置, communication.ReadLocation[9]);
                        SetTextBoxText(txtSk1当前位置, communication.ReadLocation[10]);
                        SetTextBoxText(txtSk2当前位置, communication.ReadLocation[11]);
                        SetTextBoxText(txtSk3当前位置, communication.ReadLocation[12]);
                        SetTextBoxText(txtSk4当前位置, communication.ReadLocation[13]);
                        SetTextBoxText(txtBk1当前位置, communication.ReadLocation[14]);
                        SetTextBoxText(txtBk2当前位置, communication.ReadLocation[15]);
                        SetTextBoxText(txtBk3当前位置, communication.ReadLocation[16]);
                        SetTextBoxText(txtBk4当前位置, communication.ReadLocation[17]);
                        SetTextBoxText(txt中空旋转1当前位置, communication.ReadLocation[18]);
                        SetTextBoxText(txt中空旋转2当前位置, communication.ReadLocation[19]);
                        SetTextBoxText(txtRFB1当前位置, communication.ReadLocation[20]);
                        SetTextBoxText(txtRFB2当前位置, communication.ReadLocation[21]);
                        SetTextBoxText(txtRFB3当前位置, communication.ReadLocation[22]);
                        SetTextBoxText(txtRFB4当前位置, communication.ReadLocation[23]);

                        SetTextBoxText(txtX示教固定当前位置, communication.ReadLocation[0]);
                        SetTextBoxText(txtY示教固定当前位置, communication.ReadLocation[1]);
                        SetTextBoxText(txtBYX示教固定当前位置, communication.ReadLocation[7]);
                        SetTextBoxText(txtBYY示教固定当前位置, communication.ReadLocation[8]);
                        SetTextBoxText(txtBYZ示教固定当前位置, communication.ReadLocation[9]);

                        //IO信息界面
                        SetLabelColor(communication.ReadPLCIO[0], X00);
                        SetLabelColor(communication.ReadPLCIO[1], X01);
                        SetLabelColor(communication.ReadPLCIO[2], X02);
                        SetLabelColor(communication.ReadPLCIO[3], X03);
                        SetLabelColor(communication.ReadPLCIO[4], X04);
                        SetLabelColor(communication.ReadPLCIO[5], X05);
                        SetLabelColor(communication.ReadPLCIO[6], X06);
                        SetLabelColor(communication.ReadPLCIO[7], X07);
                        SetLabelColor(communication.ReadPLCIO[8], X08);
                        SetLabelColor(communication.ReadPLCIO[9], X09);
                        SetLabelColor(communication.ReadPLCIO[10], X10);
                        SetLabelColor(communication.ReadPLCIO[11], X11);
                        SetLabelColor(communication.ReadPLCIO[12], X12);
                        SetLabelColor(communication.ReadPLCIO[13], X13);
                        SetLabelColor(communication.ReadPLCIO[14], X14);
                        SetLabelColor(communication.ReadPLCIO[15], X15);
                        SetLabelColor(communication.ReadPLCIO[16], X16);
                        SetLabelColor(communication.ReadPLCIO[17], X17);
                        SetLabelColor(communication.ReadPLCIO[18], X18);
                        SetLabelColor(communication.ReadPLCIO[19], X19);
                        SetLabelColor(communication.ReadPLCIO[20], X20);
                        SetLabelColor(communication.ReadPLCIO[21], X21);
                        SetLabelColor(communication.ReadPLCIO[22], X22);
                        SetLabelColor(communication.ReadPLCIO[23], X23);
                        SetLabelColor(communication.ReadPLCIO[24], X24);
                        SetLabelColor(communication.ReadPLCIO[25], X25);
                        SetLabelColor(communication.ReadPLCIO[26], X26);
                        SetLabelColor(communication.ReadPLCIO[27], X27);
                        SetLabelColor(communication.ReadPLCIO[28], X28);
                        SetLabelColor(communication.ReadPLCIO[29], X29);
                        SetLabelColor(communication.ReadPLCIO[30], X30);
                        SetLabelColor(communication.ReadPLCIO[31], X31);
                        SetLabelColor(communication.ReadPLCIO[32], X32);
                        SetLabelColor(communication.ReadPLCIO[33], X33);
                        SetLabelColor(communication.ReadPLCIO[34], X34);
                        SetLabelColor(communication.ReadPLCIO[35], X35);
                        SetLabelColor(communication.ReadPLCIO[36], X36);
                        SetLabelColor(communication.ReadPLCIO[37], X37);
                        SetLabelColor(communication.ReadPLCIO[38], X38);
                        SetLabelColor(communication.ReadPLCIO[39], X39);
                        SetLabelColor(communication.ReadPLCIO[40], X40);
                        SetLabelColor(communication.ReadPLCIO[41], X41);
                        SetLabelColor(communication.ReadPLCIO[42], X42);
                        SetLabelColor(communication.ReadPLCIO[43], X43);
                        SetLabelColor(communication.ReadPLCIO[44], X44);
                        SetLabelColor(communication.ReadPLCIO[45], X45);
                        SetLabelColor(communication.ReadPLCIO[46], X46);
                        SetLabelColor(communication.ReadPLCIO[47], X47);
                        SetLabelColor(communication.ReadPLCIO[48], X48);
                        SetLabelColor(communication.ReadPLCIO[49], X49);
                        SetLabelColor(communication.ReadPLCIO[50], X50);
                        SetLabelColor(communication.ReadPLCIO[51], X51);
                        SetLabelColor(communication.ReadPLCIO[52], X52);
                        SetLabelColor(communication.ReadPLCIO[53], X53);
                        SetLabelColor(communication.ReadPLCIO[54], X54);
                        SetLabelColor(communication.ReadPLCIO[55], X55);
                        SetLabelColor(communication.ReadPLCIO[56], X56);
                        SetLabelColor(communication.ReadPLCIO[57], X57);
                        SetLabelColor(communication.ReadPLCIO[58], X58);
                        SetLabelColor(communication.ReadPLCIO[59], X59);
                        SetLabelColor(communication.ReadPLCIO[60], X60);
                        SetLabelColor(communication.ReadPLCIO[61], X61);
                        SetLabelColor(communication.ReadPLCIO[62], X62);
                        SetLabelColor(communication.ReadPLCIO[63], X63);
                        SetLabelColor(communication.ReadPLCIO[64], X64);
                        SetLabelColor(communication.ReadPLCIO[65], X65);
                        SetLabelColor(communication.ReadPLCIO[66], X66);
                        SetLabelColor(communication.ReadPLCIO[67], X67);
                        SetLabelColor(communication.ReadPLCIO[68], X68);
                        SetLabelColor(communication.ReadPLCIO[69], X69);
                        SetLabelColor(communication.ReadPLCIO[70], X70);
                        SetLabelColor(communication.ReadPLCIO[71], X71);
                        SetLabelColor(communication.ReadPLCIO[72], X72);
                        SetLabelColor(communication.ReadPLCIO[73], X73);
                        SetLabelColor(communication.ReadPLCIO[74], X74);
                        SetLabelColor(communication.ReadPLCIO[75], X75);
                        SetLabelColor(communication.ReadPLCIO[76], X76);
                        SetLabelColor(communication.ReadPLCIO[77], X77);
                        SetLabelColor(communication.ReadPLCIO[78], X78);
                        SetLabelColor(communication.ReadPLCIO[79], X79);
                        SetLabelColor(communication.ReadPLCIO[80], X80);
                        SetLabelColor(communication.ReadPLCIO[81], X81);
                        SetLabelColor(communication.ReadPLCIO[82], X82);
                        SetLabelColor(communication.ReadPLCIO[83], X83);
                        SetLabelColor(communication.ReadPLCIO[84], X84);
                        SetLabelColor(communication.ReadPLCIO[85], X85);
                        SetLabelColor(communication.ReadPLCIO[86], X86);
                        SetLabelColor(communication.ReadPLCIO[87], X87);
                        SetLabelColor(communication.ReadPLCIO[88], X88);
                        SetLabelColor(communication.ReadPLCIO[89], X89);
                        SetLabelColor(communication.ReadPLCIO[90], X90);
                        SetLabelColor(communication.ReadPLCIO[91], X91);
                        SetLabelColor(communication.ReadPLCIO[92], X92);
                        SetLabelColor(communication.ReadPLCIO[93], X93);
                        SetLabelColor(communication.ReadPLCIO[94], X94);
                        SetLabelColor(communication.ReadPLCIO[95], X95);
                        SetLabelColor(communication.ReadPLCIO[96], X96);
                        SetLabelColor(communication.ReadPLCIO[97], X97);
                        SetLabelColor(communication.ReadPLCIO[98], X98);
                        SetLabelColor(communication.ReadPLCIO[99], X99);
                        SetLabelColor(communication.ReadPLCIO[100], X100);
                        SetLabelColor(communication.ReadPLCIO[101], X101);
                        SetLabelColor(communication.ReadPLCIO[102], X102);
                        SetLabelColor(communication.ReadPLCIO[103], X103);
                        SetLabelColor(communication.ReadPLCIO[104], X104);
                        SetLabelColor(communication.ReadPLCIO[105], X105);
                        SetLabelColor(communication.ReadPLCIO[106], X106);
                        SetLabelColor(communication.ReadPLCIO[107], X107);
                        SetLabelColor(communication.ReadPLCIO[108], X108);
                        SetLabelColor(communication.ReadPLCIO[109], X109);
                        SetLabelColor(communication.ReadPLCIO[110], X110);
                        SetLabelColor(communication.ReadPLCIO[111], X111);
                        //SetLabelColor(communication.ReadPLCIO[112], X112);
                        //SetLabelColor(communication.ReadPLCIO[113], X113);
                        //SetLabelColor(communication.ReadPLCIO[114], X114);
                        //SetLabelColor(communication.ReadPLCIO[115], X115);
                        //SetLabelColor(communication.ReadPLCIO[116], X116);
                        //SetLabelColor(communication.ReadPLCIO[117], X117);
                        //SetLabelColor(communication.ReadPLCIO[118], X118);
                        //SetLabelColor(communication.ReadPLCIO[119], X119);
                        //SetLabelColor(communication.ReadPLCIO[120], X120);
                        //SetLabelColor(communication.ReadPLCIO[121], X121);
                        //SetLabelColor(communication.ReadPLCIO[122], X122);
                        //SetLabelColor(communication.ReadPLCIO[123], X123);
                        //SetLabelColor(communication.ReadPLCIO[124], X124);
                        //SetLabelColor(communication.ReadPLCIO[125], X125);
                        //SetLabelColor(communication.ReadPLCIO[126], X126);
                        //SetLabelColor(communication.ReadPLCIO[127], X127);
                        SetLabelColor(communication.ReadPLCIO[128], X128);
                        SetLabelColor(communication.ReadPLCIO[129], X129);
                        SetLabelColor(communication.ReadPLCIO[130], X130);
                        SetLabelColor(communication.ReadPLCIO[131], X131);
                        SetLabelColor(communication.ReadPLCIO[132], X132);
                        SetLabelColor(communication.ReadPLCIO[133], X133);
                        SetLabelColor(communication.ReadPLCIO[134], X134);
                        SetLabelColor(communication.ReadPLCIO[135], X135);
                        //SetLabelColor(communication.ReadPLCIO[136], X136);
                        //SetLabelColor(communication.ReadPLCIO[137], X137);
                        //SetLabelColor(communication.ReadPLCIO[138], X138);
                        //SetLabelColor(communication.ReadPLCIO[139], X139);
                        //SetLabelColor(communication.ReadPLCIO[140], X140);
                        //SetLabelColor(communication.ReadPLCIO[141], X141);
                        //SetLabelColor(communication.ReadPLCIO[142], X142);
                        //SetLabelColor(communication.ReadPLCIO[143], X143);

                        //SetLabelColor(communication.ReadPLCIO[150], Y00);
                        //SetLabelColor(communication.ReadPLCIO[151], Y01);
                        //SetLabelColor(communication.ReadPLCIO[152], Y02);
                        //SetLabelColor(communication.ReadPLCIO[153], Y03);
                        //SetLabelColor(communication.ReadPLCIO[154], Y04);
                        SetLabelColor(communication.ReadPLCIO[155], LB_Connection);
                        SetLabelColor(communication.ReadPLCIO[156], LB_初始化状态);
                        SetLabelColor(communication.ReadPLCIO[157], LB_手动状态);
                        SetLabelColor(communication.ReadPLCIO[158], LB_自动状态);
                        SetLabelColor(communication.ReadPLCIO[159], LB_自动运行状态);
                        //SetLabelColor(communication.ReadPLCIO[155], Y05);
                        //SetLabelColor(communication.ReadPLCIO[156], Y06);
                        //SetLabelColor(communication.ReadPLCIO[157], Y07);
                        //SetLabelColor(communication.ReadPLCIO[158], Y08);
                        //SetLabelColor(communication.ReadPLCIO[159], Y09);
                        SetLabelColor(communication.ReadPLCIO[160], 夹爪状态);
                        SetLabelColor(communication.ReadPLCIO[161], LB_自动本地状态);
                        SetLabelColor(communication.ReadPLCIO[162], LB_自动远程状态);
                        SetLabelColor(communication.ReadPLCIO[163], LB_自动远程状态测试);
                        //SetLabelColor(communication.ReadPLCIO[164], Y14);
                        //SetLabelColor(communication.ReadPLCIO[165], Y15);
                        SetLabelColor(communication.ReadPLCIO[166], LB_FW_上料X);
                        SetLabelColor(communication.ReadPLCIO[167], LB_FW_上料Y);
                        SetLabelColor(communication.ReadPLCIO[168], LB_FW_实盘);
                        SetLabelColor(communication.ReadPLCIO[169], LB_FW_倒实盘);
                        SetLabelColor(communication.ReadPLCIO[170], LB_FW_NG盘);
                        SetLabelColor(communication.ReadPLCIO[171], LB_FW_倒NG盘);
                        SetLabelColor(communication.ReadPLCIO[172], LB_FW_平移);
                        SetLabelColor(communication.ReadPLCIO[173], LB_FW_搬运X);
                        SetLabelColor(communication.ReadPLCIO[174], LB_FW_搬运Y);
                        SetLabelColor(communication.ReadPLCIO[175], LB_FW_搬运Z);
                        SetLabelColor(communication.ReadPLCIO[176], LB_FW_夹具1);
                        SetLabelColor(communication.ReadPLCIO[177], LB_FW_夹具2);
                        SetLabelColor(communication.ReadPLCIO[178], LB_FW_夹具3);
                        SetLabelColor(communication.ReadPLCIO[179], LB_FW_夹具4);
                        SetLabelColor(communication.ReadPLCIO[180], LB_FW_黑体1);
                        SetLabelColor(communication.ReadPLCIO[181], LB_FW_黑体2);
                        SetLabelColor(communication.ReadPLCIO[182], LB_FW_黑体3);
                        SetLabelColor(communication.ReadPLCIO[183], LB_FW_黑体4);
                        SetLabelColor(communication.ReadPLCIO[184], LB_FW_吸嘴1);
                        SetLabelColor(communication.ReadPLCIO[185], LB_FW_吸嘴2);
                        SetLabelColor(communication.ReadPLCIO[186], LB_FW_辐射板1);
                        SetLabelColor(communication.ReadPLCIO[187], LB_FW_辐射板2);
                        SetLabelColor(communication.ReadPLCIO[188], LB_FW_辐射板3);
                        SetLabelColor(communication.ReadPLCIO[189], LB_FW_辐射板4);
                        SetLabelColor(communication.ReadPLCIO[190], LB_FW_钧舵1激活);
                        SetLabelColor(communication.ReadPLCIO[191], LB_FW_钧舵2激活);
                        SetLabelColor(communication.ReadPLCIO[192], LB_FW_钧舵3激活);
                        SetLabelColor(communication.ReadPLCIO[193], LB_FW_钧舵4激活);
                        //SetLabelColor(communication.ReadPLCIO[194], Y44);
                        //SetLabelColor(communication.ReadPLCIO[195], Y45);
                        //SetLabelColor(communication.ReadPLCIO[196], Y46);
                        //SetLabelColor(communication.ReadPLCIO[197], Y47);
                        //SetLabelColor(communication.ReadPLCIO[198], Y48);
                        //SetLabelColor(communication.ReadPLCIO[199], Y49);
                        //SetLabelColor(communication.ReadPLCIO[200], Y50);
                        //SetLabelColor(communication.ReadPLCIO[201], Y51);
                        //SetLabelColor(communication.ReadPLCIO[202], Y52);
                        //SetLabelColor(communication.ReadPLCIO[203], Y53);
                        //SetLabelColor(communication.ReadPLCIO[204], Y54);
                        //SetLabelColor(communication.ReadPLCIO[205], Y55);
                        //SetLabelColor(communication.ReadPLCIO[206], Y56);
                        //SetLabelColor(communication.ReadPLCIO[207], Y57);
                        //SetLabelColor(communication.ReadPLCIO[208], Y58);
                        //SetLabelColor(communication.ReadPLCIO[209], Y59);
                        //SetLabelColor(communication.ReadPLCIO[210], Y60);
                        //SetLabelColor(communication.ReadPLCIO[211], Y61);
                        //SetLabelColor(communication.ReadPLCIO[212], Y62);
                        //SetLabelColor(communication.ReadPLCIO[213], Y63);
                        //SetLabelColor(communication.ReadPLCIO[214], Y64);
                        //SetLabelColor(communication.ReadPLCIO[215], Y65);
                        //SetLabelColor(communication.ReadPLCIO[216], Y66);
                        //SetLabelColor(communication.ReadPLCIO[217], Y67);
                        //SetLabelColor(communication.ReadPLCIO[218], Y68);
                        //SetLabelColor(communication.ReadPLCIO[219], Y69);
                        //SetLabelColor(communication.ReadPLCIO[220], Y70);
                        //SetLabelColor(communication.ReadPLCIO[221], Y71);
                        //SetLabelColor(communication.ReadPLCIO[222], Y72);
                        //SetLabelColor(communication.ReadPLCIO[223], Y73);
                        //SetLabelColor(communication.ReadPLCIO[224], Y74);
                        //SetLabelColor(communication.ReadPLCIO[225], Y75);
                        //SetLabelColor(communication.ReadPLCIO[226], Y76);
                        //SetLabelColor(communication.ReadPLCIO[227], Y77);
                        //SetLabelColor(communication.ReadPLCIO[228], Y78);
                        //SetLabelColor(communication.ReadPLCIO[229], Y79);
                        //SetLabelColor(communication.ReadPLCIO[230], Y80);
                        //SetLabelColor(communication.ReadPLCIO[231], Y81);
                        //SetLabelColor(communication.ReadPLCIO[232], Y82);
                        //SetLabelColor(communication.ReadPLCIO[233], Y83);
                        //SetLabelColor(communication.ReadPLCIO[234], Y84);
                        //SetLabelColor(communication.ReadPLCIO[235], Y85);
                        //SetLabelColor(communication.ReadPLCIO[236], Y86);
                        //SetLabelColor(communication.ReadPLCIO[237], Y87);
                        //SetLabelColor(communication.ReadPLCIO[238], Y88);
                        //SetLabelColor(communication.ReadPLCIO[239], Y89);
                        //SetLabelColor(communication.ReadPLCIO[240], Y90);
                        //SetLabelColor(communication.ReadPLCIO[241], Y91);
                        //SetLabelColor(communication.ReadPLCIO[242], Y92);
                        //SetLabelColor(communication.ReadPLCIO[243], Y93);
                        //SetLabelColor(communication.ReadPLCIO[244], Y94);
                        //SetLabelColor(communication.ReadPLCIO[245], Y95);
                        //SetLabelColor(communication.ReadPLCIO[246], Y96);
                        //SetLabelColor(communication.ReadPLCIO[247], Y97);
                        //SetLabelColor(communication.ReadPLCIO[248], Y98);
                        //SetLabelColor(communication.ReadPLCIO[249], Y99);
                        //SetLabelColor(communication.ReadPLCIO[250], Y100);
                        //SetLabelColor(communication.ReadPLCIO[251], Y101);
                        //SetLabelColor(communication.ReadPLCIO[252], Y102);
                        //SetLabelColor(communication.ReadPLCIO[253], Y103);
                        //SetLabelColor(communication.ReadPLCIO[254], Y104);
                        //SetLabelColor(communication.ReadPLCIO[255], Y105);
                        //SetLabelColor(communication.ReadPLCIO[256], Y103);
                        //SetLabelColor(communication.ReadPLCIO[257], Y107);
                        //SetLabelColor(communication.ReadPLCIO[258], Y108);
                        //SetLabelColor(communication.ReadPLCIO[259], Y109);
                        //SetLabelColor(communication.ReadPLCIO[260], Y110);
                        //SetLabelColor(communication.ReadPLCIO[261], Y111);
                        //SetLabelColor(communication.ReadPLCIO[262], Y112);
                        //SetLabelColor(communication.ReadPLCIO[263], Y113);
                        //SetLabelColor(communication.ReadPLCIO[264], Y114);
                        //SetLabelColor(communication.ReadPLCIO[265], Y115);
                        //SetLabelColor(communication.ReadPLCIO[266], Y116);
                        //SetLabelColor(communication.ReadPLCIO[267], Y117);
                        //SetLabelColor(communication.ReadPLCIO[268], Y118);
                        //SetLabelColor(communication.ReadPLCIO[269], Y119);
                        //SetLabelColor(communication.ReadPLCIO[270], Y120);
                        //SetLabelColor(communication.ReadPLCIO[271], Y121);
                        //SetLabelColor(communication.ReadPLCIO[272], Y122);
                        //SetLabelColor(communication.ReadPLCIO[273], Y123);
                        //SetLabelColor(communication.ReadPLCIO[274], Y124);
                        //SetLabelColor(communication.ReadPLCIO[275], Y125);
                        //SetLabelColor(communication.ReadPLCIO[276], Y126);
                        //SetLabelColor(communication.ReadPLCIO[277], Y127);

                        //参数设置
                        SetTextBoxText(txt上料X轴定位速度, communication.ReadPLCPmt[0]);
                        SetTextBoxText(txt上料Y轴定位速度, communication.ReadPLCPmt[1]);
                        SetTextBoxText(txt升降轴定位速度, communication.ReadPLCPmt[2]);
                        SetTextBoxText(txt平移轴定位速度, communication.ReadPLCPmt[3]);
                        SetTextBoxText(txt中空轴定位速度, communication.ReadPLCPmt[4]);
                        SetTextBoxText(txt搬运X轴定位速度, communication.ReadPLCPmt[5]);
                        SetTextBoxText(txt搬运Y轴定位速度, communication.ReadPLCPmt[6]);
                        SetTextBoxText(txt搬运Z轴定位速度, communication.ReadPLCPmt[7]);
                        SetTextBoxText(txtSocket轴定位速度, communication.ReadPLCPmt[8]);
                        SetTextBoxText(txt黑体轴定位速度, communication.ReadPLCPmt[9]);
                        SetTextBoxText(txt热辐射轴定位速度, communication.ReadPLCPmt[11]);

                        SetTextBoxText(txt上料X轴手动速度, communication.ReadPLCPmt[15]);
                        SetTextBoxText(txt上料Y轴手动速度, communication.ReadPLCPmt[16]);
                        SetTextBoxText(txt升降轴手动速度, communication.ReadPLCPmt[17]);
                        SetTextBoxText(txt平移轴手动速度, communication.ReadPLCPmt[18]);
                        SetTextBoxText(txt中空轴手动速度, communication.ReadPLCPmt[19]);
                        SetTextBoxText(txt搬运X轴手动速度, communication.ReadPLCPmt[20]);
                        SetTextBoxText(txt搬运Y轴手动速度, communication.ReadPLCPmt[21]);
                        SetTextBoxText(txt搬运Z轴手动速度, communication.ReadPLCPmt[22]);
                        SetTextBoxText(txtSocket轴手动速度, communication.ReadPLCPmt[23]);
                        SetTextBoxText(txt黑体轴手动速度, communication.ReadPLCPmt[24]);
                        SetTextBoxText(txt热辐射轴手动速度, communication.ReadPLCPmt[25]);

                        SetTextBoxText(txt夹具1次数, communication.ReadPLCPmt[31]);
                        SetTextBoxText(txt夹具2次数, communication.ReadPLCPmt[32]);
                        SetTextBoxText(txt夹具3次数, communication.ReadPLCPmt[33]);
                        SetTextBoxText(txt夹具4次数, communication.ReadPLCPmt[34]);

                        //信息读取
                        SetTextBoxText(txt控制器时间, communication.ReadTestInformation[29]);
                        SetTextBoxText(txt扫码信息, communication.ReadTestInformation[30]);
                        SetTextBoxText(txt下视觉偏移X, communication.ReadTestInformation[32]);
                        SetTextBoxText(txt下视觉偏移Y, communication.ReadTestInformation[33]);
                        SetTextBoxText(txt下视觉偏移θ, communication.ReadTestInformation[34]);
                        SetTextBoxText(txt上视觉偏移X, communication.ReadTestInformation[36]);
                        SetTextBoxText(txt上视觉偏移Y, communication.ReadTestInformation[37]);
                        SetTextBoxText(txt上视觉偏移θ, communication.ReadTestInformation[38]);
                        SetTextBoxText(txt计算偏移X, communication.ReadTestInformation[39]);
                        SetTextBoxText(txt计算偏移Y, communication.ReadTestInformation[40]);
                        SetTextBoxText(txt计算偏移θ, communication.ReadTestInformation[41]);
                        #endregion
                        stopwatch2.Stop();
                        logfile.WriteLog("界面数据更新结束", "界面数据更新");
                        label95.Invoke(new Action(() => label95.Text = $"{stopwatch2.ElapsedMilliseconds}ms"));
                    }
                    catch (Exception e)
                    {
                        logfile.WriteLog($"界面数据更新出错。{e.Message}", "界面数据更新");
                    }
                }
            });
        }

        #region 报警跳出与记录
        public void AlarmCheck()
        {
            Task.Run(() =>
            {
                while (isUpdateData)
                {
                    try
                    {
                        Thread.Sleep(300);
                        for (int i = 0; i < communication.ReadPLCAlarm.Length; i++)
                        {
                            if (communication.ReadPLCAlarm[i])
                            {
                                //如果当前报警列表不包含检测到的字符串
                                if (!warning.Contains(alarmInformation[i.ToString()]))
                                {
                                    TB_Warning.Invoke(new Action(() => TB_Warning.Clear()));
                                    warning.Add(alarmInformation[i.ToString()]);
                                    //显示到控件上
                                    foreach (var item in warning)
                                        TB_Warning.Invoke(new Action(() => TB_Warning.AppendText(item + Environment.NewLine)));
                                    logfile.WriteLog(alarmInformation[i.ToString()], "报警记录");
                                }
                            }
                            else
                            {
                                //如果当前报警列表包含已经清除的报警
                                if (warning.Contains(alarmInformation[i.ToString()]))
                                {
                                    TB_Warning.Invoke(new Action(() => TB_Warning.Clear()));
                                    warning.Remove(alarmInformation[i.ToString()]);
                                    //更新显示内容
                                    foreach (var item in warning)
                                        TB_Warning.Invoke(new Action(() => TB_Warning.AppendText(item + Environment.NewLine)));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logfile.WriteLog("报警监测循环：" + e.Message, "报警记录");
                    }
                }
            });
        }
        #endregion

        #region 方法
        //IO颜色方法
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

        //实时显示Text内容方法
        public void SetTextBoxText<T>(TextBox textBox, T variable)
        {
            if (variable != null)
                textBox.Invoke(new Action(() => textBox.Text = variable.ToString()));
        }

        //信息追溯导出EXCEL
        public void DataToExcel(DataGridView m_DataView)
        {
            SaveFileDialog kk = new SaveFileDialog
            {
                Title = "保存EXCEL文件",
                Filter = "EXCEL文件(*.xls) |*.xls |所有文件(*.*) |*.*",
                FilterIndex = 1,
                FileName = DateTime.Now.ToString("D")
            };
            if (kk.ShowDialog() == DialogResult.OK)
            {
                string FileName = kk.FileName;
                if (File.Exists(FileName))
                    File.Delete(FileName);
                FileStream objFileStream;
                StreamWriter objStreamWriter;
                string strLine = "";
                objFileStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write);
                objStreamWriter = new StreamWriter(objFileStream, System.Text.Encoding.Unicode);
                for (int i = 0; i < m_DataView.Columns.Count; i++)
                {
                    if (m_DataView.Columns[i].Visible == true)
                    {
                        strLine = strLine + m_DataView.Columns[i].HeaderText.ToString() + Convert.ToChar(9);
                    }
                }
                objStreamWriter.WriteLine(strLine);
                strLine = "";

                for (int i = 0; i < m_DataView.Rows.Count; i++)
                {
                    if (m_DataView.Columns[0].Visible == true)
                    {
                        if (m_DataView.Rows[i].Cells[0].Value == null)
                            strLine = strLine + " " + Convert.ToChar(9);
                        else
                            strLine = strLine + m_DataView.Rows[i].Cells[0].Value.ToString() + Convert.ToChar(9);
                    }
                    for (int j = 1; j < m_DataView.Columns.Count; j++)
                    {
                        if (m_DataView.Columns[j].Visible == true)
                        {
                            if (m_DataView.Rows[i].Cells[j].Value == null)
                                strLine = strLine + " " + Convert.ToChar(9);
                            else
                            {
                                string rowstr = "";
                                rowstr = m_DataView.Rows[i].Cells[j].Value.ToString();
                                if (rowstr.IndexOf("\r\n") > 0)
                                    rowstr = rowstr.Replace("\r\n", " ");
                                if (rowstr.IndexOf("\t") > 0)
                                    rowstr = rowstr.Replace("\t", " ");
                                strLine = strLine + rowstr + Convert.ToChar(9);
                            }
                        }
                    }
                    objStreamWriter.WriteLine(strLine);
                    strLine = "";
                }
                objStreamWriter.Close();
                objFileStream.Close();
                MessageBox.Show(this, "保存EXCEL成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

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

        private void BTN_Output_Click(object sender, EventArgs e)
        {
            DataToExcel(dataGridView);
        }
        #endregion

        #region 切换托盘类型
        private void CB_TypeOfTray_SelectedIndexChanged(object sender, EventArgs e)
        {
            trayType = CB_TypeOfTray.Text;
            communication.WriteVariable(trayManager.TrayType[CB_TypeOfTray.Text].Index, "PlcInID[0]");
        }
        #endregion

        #region 示教1操作
        private void btnX示教吸1实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[30]");
        }

        private void btnX示教吸1实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[30]");
        }

        private void btnX示教吸2实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[32]");
        }

        private void btnX示教吸2实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[32]");
        }

        private void btnX示教实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[34]");
        }

        private void btnX示教实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[34]");
        }

        private void btnX示教倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[35]");
        }

        private void btnX示教倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[35]");
        }

        private void btnX示教NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[36]");
        }

        private void btnX示教NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[36]");
        }

        private void btnX示教倒NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[37]");
        }

        private void btnX示教倒NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[37]");
        }

        private void btnX示教夹爪位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[38]");
        }

        private void btnX示教夹爪位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[38]");
        }

        private void btnX示教扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[39]");
        }

        private void btnX示教扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[39]");
        }

        private void btnX示教吸1NG盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[40]");
        }

        private void btnX示教吸1NG盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[40]");
        }

        private void btnX示教吸2NG盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[42]");
        }

        private void btnX示教吸2NG盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[42]");
        }

        private void btnX示教吸1倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[51]");
        }

        private void btnX示教吸1倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[51]");
        }

        private void btnX示教吸2倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[53]");
        }

        private void btnX示教吸2倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[53]");
        }

        private void btnX示教视觉实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[190]");
        }

        private void btnX示教视觉实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[190]");
        }

        private void btnX示教视觉倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[192]");
        }

        private void btnX示教视觉倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[192]");
        }

        private void btnY示教吸1实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[31]");
        }

        private void btnY示教吸1实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[31]");
        }

        private void btnY示教吸1NG盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[41]");
        }

        private void btnY示教吸1NG盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[41]");
        }

        private void btnY示教吸2实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[33]");
        }

        private void btnY示教吸2实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[33]");
        }

        private void btnY示教吸2NG盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[43]");
        }

        private void btnY示教吸2NG盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[43]");
        }

        private void btnY示教实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[45]");
        }

        private void btnY示教实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[45]");
        }

        private void btnY示教倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[46]");
        }

        private void btnY示教倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[46]");
        }

        private void btnY示教NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[47]");
        }

        private void btnY示教NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[47]");
        }

        private void btnY示教倒NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[48]");
        }

        private void btnY示教倒NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[48]");
        }

        private void btnY示教夹爪位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[49]");
        }

        private void btnY示教夹爪位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[49]");
        }

        private void btnY示教扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[50]");
        }

        private void btnY示教扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[50]");
        }

        private void btnY示教吸1倒实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[52]");
        }

        private void btnY示教吸1倒实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[52]");
        }

        private void btnY示教吸2倒实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[54]");
        }

        private void btnY示教吸2倒实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[54]");
        }

        private void btnY示教视觉实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[191]");
        }

        private void btnY示教视觉实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[191]");
        }

        private void btnY示教视觉倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[193]");
        }

        private void btnY示教视觉倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[193]");
        }

        private void btn实盘示教初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[56]");
        }

        private void btn实盘示教初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[56]");
        }

        private void btn实盘示教扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[57]");
        }

        private void btn实盘示教扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[57]");
        }

        private void btnNG盘示教初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[61]");
        }

        private void btnNG盘示教初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[61]");
        }

        private void btn倒实盘示教初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[62]");
        }

        private void btn倒实盘示教初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[62]");
        }

        private void btn倒NG盘示教初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[63]");
        }

        private void btn倒NG盘示教初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[63]");
        }

        private void btn平移示教上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[71]");
        }

        private void btn平移示教上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[71]");
        }

        private void btn平移示教下料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[72]");
        }

        private void btn平移示教下料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[72]");
        }

        private void btn平移示教中转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[73]");
        }

        private void btn平移示教中转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[73]");
        }
        #endregion

        #region 示教2操作
        private void btnBYX示教上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[76]");
        }

        private void btnBYX示教上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[76]");
        }

        private void btnBYX示教视觉1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[77]");
        }

        private void btnBYX示教视觉1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[77]");
        }

        private void btnBYX示教视觉2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[78]");
        }

        private void btnBYX示教视觉2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[78]");
        }

        private void btnBYX示教第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[79]");
        }

        private void btnBYX示教第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[79]");
        }

        private void btnBYX示教第二列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[80]");
        }

        private void btnBYX示教第二列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[80]");
        }

        private void btnBYX示教第三列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[81]");
        }

        private void btnBYX示教第三列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[81]");
        }

        private void btnBYX示教第四列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[82]");
        }

        private void btnBYX示教第四列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[82]");
        }

        private void btnBYX示教第五列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[83]");
        }

        private void btnBYX示教第五列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[83]");
        }

        private void btnBYX示教第六列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[84]");
        }

        private void btnBYX示教第六列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[84]");
        }

        private void btnBYX示教第七列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[85]");
        }

        private void btnBYX示教第七列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[85]");
        }

        private void btnBYX示教第八列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[86]");
        }

        private void btnBYX示教第八列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[86]");
        }

        private void btnBYY示教上料12位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[100]");
        }

        private void btnBYY示教上料12位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[100]");
        }

        private void btnBYY示教上料34位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[108]");
        }

        private void btnBYY示教上料34位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[108]");
        }

        private void btnBYY示教视觉1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[101]");
        }

        private void btnBYY示教视觉1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[101]");
        }

        private void btnBYY示教视觉2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[102]");
        }

        private void btnBYY示教视觉2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[102]");
        }

        private void btnBYY示教视觉3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[106]");
        }

        private void btnBYY示教视觉3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[106]");
        }

        private void btnBYY示教视觉4位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[107]");
        }

        private void btnBYY示教视觉4位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[107]");
        }

        private void btnBYY示教第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[103]");
        }

        private void btnBYY示教第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[103]");
        }

        private void btnBYY示教第二行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[104]");
        }

        private void btnBYY示教第二行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[104]");
        }

        private void btnBYZ示教上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[111]");
        }

        private void btnBYZ示教上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[111]");
        }

        private void btnBYZ示教上升位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[110]");
        }

        private void btnBYZ示教上升位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[110]");
        }

        private void btnBYZ示教视觉位置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[117]");
        }

        private void btnBYZ示教视觉位置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[117]");
        }

        private void btnBYZ示教视觉位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[113]");
        }

        private void btnBYZ示教视觉位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[113]");
        }

        private void btnBYZ示教视觉位置3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[116]");
        }

        private void btnBYZ示教视觉位置3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[116]");
        }

        private void btnBYZ下料视觉位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[114]");
        }

        private void btnBYZ下料视觉位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[114]");
        }

        private void btnSk1示教黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[121]");
        }

        private void btnSk1示教黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[121]");
        }

        private void btnSk1示教翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[122]");
        }

        private void btnSk1示教翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[122]");
        }

        private void btnSk1示教测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[123]");
        }

        private void btnSk1示教测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[123]");
        }

        private void btnSk1示教测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[124]");
        }

        private void btnSk1示教测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[124]");
        }

        private void btnSk1示教测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[125]");
        }

        private void btnSk1示教测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[125]");
        }

        private void btnSk2示教黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[131]");
        }

        private void btnSk2示教黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[131]");
        }

        private void btnSk2示教翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[132]");
        }

        private void btnSk2示教翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[132]");
        }

        private void btnSk2示教测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[133]");
        }

        private void btnSk2示教测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[133]");
        }

        private void btnSk2示教测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[134]");
        }

        private void btnSk2示教测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[134]");
        }

        private void btnSk2示教测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[135]");
        }

        private void btnSk2示教测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[135]");
        }

        private void btnSk3示教黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[141]");
        }

        private void btnSk3示教黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[141]");
        }

        private void btnSk3示教翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[142]");
        }

        private void btnSk3示教翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[142]");
        }

        private void btnSk3示教测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[143]");
        }

        private void btnSk3示教测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[143]");
        }

        private void btnSk3示教测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[144]");
        }

        private void btnSk3示教测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[144]");
        }

        private void btnSk3示教测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[145]");
        }

        private void btnSk3示教测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[145]");
        }

        private void btnSk4示教黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[151]");
        }

        private void btnSk4示教黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[151]");
        }

        private void btnSk4示教翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[152]");
        }

        private void btnSk4示教翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[152]");
        }

        private void btnSk4示教测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[153]");
        }

        private void btnSk4示教测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[153]");
        }

        private void btnSk4示教测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[154]");
        }

        private void btnSk4示教测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[154]");
        }

        private void btnSk4示教测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[155]");
        }

        private void btnSk4示教测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[155]");
        }

        private void btnBk1示教20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[161]");
        }

        private void btnBk1示教20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[161]");
        }

        private void btnBk1示教35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[162]");
        }

        private void btnBk1示教35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[162]");
        }

        private void btnBk2示教20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[166]");
        }

        private void btnBk2示教20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[166]");
        }

        private void btnBk2示教35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[167]");
        }

        private void btnBk2示教35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[167]");
        }

        private void btnBk3示教20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[171]");
        }

        private void btnBk3示教20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[171]");
        }

        private void btnBk3示教35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[172]");
        }

        private void btnBk3示教35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[172]");
        }

        private void btnBk4示教20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[176]");
        }

        private void btnBk4示教20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[176]");
        }

        private void btnBk4示教35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[177]");
        }

        private void btnBk4示教35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[177]");
        }
        private void btnBYY示教视觉5位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[98]");
        }

        private void btnBYY示教视觉5位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[98]");
        }

        private void btnBYY示教视觉6位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[99]");
        }

        private void btnBYY示教视觉6位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[99]");
        }
        #endregion

        #region 气缸操作
        private void btn上料机械手上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[490]");
        }

        private void btn上料机械手上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[490]");
        }

        private void btn上料机械手下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[491]");
        }

        private void btn上料机械手下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[491]");
        }

        private void btn上料机械手伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[492]");
        }

        private void btn上料机械手伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[492]");
        }

        private void btn上料机械手缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[493]");
        }

        private void btn上料机械手缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[493]");
        }

        private void btn实盘防卡盘伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[494]");
        }

        private void btn实盘防卡盘伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[494]");
        }

        private void btn实盘防卡盘缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[495]");
        }

        private void btn实盘防卡盘缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[495]");
        }

        private void btnNG盘防卡盘伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[496]");
        }

        private void btnNG盘防卡盘伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[496]");
        }

        private void btnNG盘防卡盘缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[497]");
        }

        private void btnNG盘防卡盘缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[497]");
        }

        private void btn上料吸嘴1上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[498]");
        }

        private void btn上料吸嘴1上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[498]");
        }

        private void btn上料吸嘴1下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[499]");
        }

        private void btn上料吸嘴1下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[499]");
        }

        private void btn上料吸嘴2上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[502]");
        }

        private void btn上料吸嘴2上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[502]");
        }

        private void btn上料吸嘴2下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[503]");
        }

        private void btn上料吸嘴2下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[503]");
        }

        private void btn平移吸嘴12上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[506]");
        }

        private void btn平移吸嘴12上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[506]");
        }

        private void btn平移吸嘴12下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[507]");
        }

        private void btn平移吸嘴12下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[507]");
        }

        private void btn平移吸嘴34上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[508]");
        }

        private void btn平移吸嘴34上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[508]");
        }

        private void btn平移吸嘴34下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[509]");
        }

        private void btn平移吸嘴34下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[509]");
        }

        private void btn翻转气缸0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[518]");
        }

        private void btn翻转气缸0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[518]");
        }

        private void btn翻转气缸180_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[519]");
        }

        private void btn翻转气缸180_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[519]");
        }

        private void btn夹爪1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[520]");
        }

        private void btn夹爪1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[520]");
        }

        private void btn夹爪1张开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[521]");
        }

        private void btn夹爪1张开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[521]");
        }

        private void btn夹爪1闭合1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[522]");
        }

        private void btn夹爪1闭合1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[522]");
        }

        private void btn夹爪1复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[525]");
        }

        private void btn夹爪1复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[525]");
        }

        private void btn夹爪2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[526]");
        }

        private void btn夹爪2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[526]");
        }

        private void btn夹爪2张开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[527]");
        }

        private void btn夹爪2张开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[527]");
        }

        private void btn夹爪2闭合1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[528]");
        }

        private void btn夹爪2闭合1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[528]");
        }

        private void btn夹爪2复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[531]");
        }

        private void btn夹爪2复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[531]");
        }

        private void btn除尘器1吹扫_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[532]");
        }

        private void btn除尘器1吹扫_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[532]");
        }

        private void btn除尘器1复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[533]");
        }

        private void btn除尘器1复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[533]");
        }

        private void btn除尘器2吹扫_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[534]");
        }

        private void btn除尘器2吹扫_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[534]");
        }

        private void btn除尘器2复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[525]");
        }

        private void btn除尘器2复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[525]");
        }

        private void btn上料吸嘴1真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[500]");
        }

        private void btn上料吸嘴1真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[500]");
        }

        private void btn上料吸嘴1破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[501]");
        }

        private void btn上料吸嘴1破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[501]");
        }

        private void btn上料吸嘴2真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[504]");
        }

        private void btn上料吸嘴2真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[504]");
        }

        private void btn上料吸嘴2破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[505]");
        }

        private void btn上料吸嘴2破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[505]");
        }

        private void btn平移吸嘴1真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[510]");
        }

        private void btn平移吸嘴1真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[510]");
        }

        private void btn平移吸嘴1破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[511]");
        }

        private void btn平移吸嘴1破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[511]");
        }

        private void btn平移吸嘴2真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[512]");
        }

        private void btn平移吸嘴2真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[512]");
        }

        private void btn平移吸嘴2破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[513]");
        }

        private void btn平移吸嘴2破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[513]");
        }

        private void btn平移吸嘴3真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[514]");
        }

        private void btn平移吸嘴3真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[514]");
        }

        private void btn平移吸嘴3破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[515]");
        }

        private void btn平移吸嘴3破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[515]");
        }

        private void btn平移吸嘴4真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[516]");
        }

        private void btn平移吸嘴4真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[516]");
        }

        private void btn平移吸嘴4破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[517]");
        }

        private void btn平移吸嘴4破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[517]");
        }

        private void btn旋转夹爪1上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[536]");
        }

        private void btn旋转夹爪1上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[536]");
        }

        private void btn旋转夹爪1下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[537]");
        }

        private void btn旋转夹爪1下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[537]");
        }

        private void btn旋转夹爪2上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[538]");
        }

        private void btn旋转夹爪2上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[538]");
        }

        private void btn旋转夹爪2下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[539]");
        }

        private void btn旋转夹爪2下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[539]");
        }

        private void btn旋转夹爪3上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[540]");
        }

        private void btn旋转夹爪3上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[540]");
        }

        private void btn旋转夹爪3下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[541]");
        }

        private void btn旋转夹爪3下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[541]");
        }

        private void btn旋转夹爪4上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[542]");
        }

        private void btn旋转夹爪4上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[542]");
        }

        private void btn旋转夹爪4下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[543]");
        }

        private void btn旋转夹爪4下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[543]");
        }

        private void btn工位1光阑伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[544]");
        }

        private void btn工位1光阑伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[544]");
        }

        private void btn工位1光阑缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[545]");
        }

        private void btn工位1光阑缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[545]");
        }

        private void btn工位1光阑上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[546]");
        }

        private void btn工位1光阑上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[546]");
        }

        private void btn工位1光阑下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[547]");
        }

        private void btn工位1光阑下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[547]");
        }

        private void btn工位1辐射板伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[548]");
        }

        private void btn工位1辐射板伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[548]");
        }

        private void btn工位1辐射板缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[549]");
        }

        private void btn工位1辐射板缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[549]");
        }

        private void btn工位1翻转0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[550]");
        }

        private void btn工位1翻转0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[550]");
        }

        private void btn工位1翻转90_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[551]");
        }

        private void btn工位1翻转90_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[551]");
        }

        private void btn工位2光阑伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[552]");
        }

        private void btn工位2光阑伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[552]");
        }

        private void btn工位2光阑缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[553]");
        }

        private void btn工位2光阑缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[553]");
        }

        private void btn工位2光阑上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[554]");
        }

        private void btn工位2光阑上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[554]");
        }

        private void btn工位2光阑下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[555]");
        }

        private void btn工位2光阑下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[555]");
        }

        private void btn工位2辐射板伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[556]");
        }

        private void btn工位2辐射板伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[556]");
        }

        private void btn工位2辐射板缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[557]");
        }

        private void btn工位2辐射板缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[557]");
        }

        private void btn工位2翻转0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[558]");
        }

        private void btn工位2翻转0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[558]");
        }

        private void brn工位2翻转90_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[559]");
        }

        private void brn工位2翻转90_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[559]");
        }

        private void btn工位3光阑伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[560]");
        }

        private void btn工位3光阑伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[560]");
        }

        private void btn工位3光阑缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[561]");
        }

        private void btn工位3光阑缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[561]");
        }

        private void btn工位3光阑上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[562]");
        }

        private void btn工位3光阑上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[562]");
        }

        private void btn工位3光阑下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[563]");
        }

        private void btn工位3光阑下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[563]");
        }

        private void btn工位3辐射板伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[564]");
        }

        private void btn工位3辐射板伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[564]");
        }

        private void btn工位3辐射板缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[565]");
        }

        private void btn工位3辐射板缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[565]");
        }

        private void btn工位3翻转0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[566]");
        }

        private void btn工位3翻转0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[566]");
        }

        private void btn工位3翻转90_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[567]");
        }

        private void btn工位3翻转90_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[567]");
        }

        private void btn工位4光阑伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[568]");
        }

        private void btn工位4光阑伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[568]");
        }

        private void btn工位4光阑缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[569]");
        }

        private void btn工位4光阑缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[569]");
        }

        private void btn工位4光阑上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[570]");
        }

        private void btn工位4光阑上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[570]");
        }

        private void btn工位4光阑下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[571]");
        }

        private void btn工位4光阑下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[571]");
        }

        private void btn工位4辐射板伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[572]");
        }

        private void btn工位4辐射板伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[572]");
        }

        private void btn工位4辐射板缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[573]");
        }

        private void btn工位4辐射板缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[573]");
        }

        private void btn工位4翻转0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[574]");
        }

        private void btn工位4翻转0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[574]");
        }

        private void btn工位4翻转90_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[575]");
        }

        private void btn工位4翻转90_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[575]");
        }

        private void btn工位1风扇上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[576]");
        }

        private void btn工位1风扇上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[576]");
        }

        private void btn工位2风扇上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[577]");
        }

        private void btn工位2风扇上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[577]");
        }

        private void btn工位3风扇上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[578]");
        }

        private void btn工位3风扇上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[578]");
        }

        private void btn工位4风扇上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[579]");
        }

        private void btn工位4风扇上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[579]");
        }

        private void btnEFU上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[580]");
        }

        private void btnEFU上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[580]");
        }

        private void btn工位1风扇断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[581]");
        }

        private void btn工位1风扇断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[581]");
        }

        private void btn工位2风扇断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[582]");
        }

        private void btn工位2风扇断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[582]");
        }

        private void btn工位3风扇断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[583]");
        }

        private void btn工位3风扇断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[583]");
        }

        private void btn工位4风扇断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[584]");
        }

        private void btn工位4风扇断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[584]");
        }

        private void btnEFU断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[585]");
        }

        private void btnEFU断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[585]");
        }

        private void btn_产品有无_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[619]");
        }

        private void btn_产品有无_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[619]");
        }

        private void btn条形码_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[620]");
        }

        private void btn条形码_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[620]");
        }

        private void btn二维码_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[621]");
        }

        private void btn二维码_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[621]");
        }

        private void btn_下视觉标定_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[622]");
        }

        private void btn_下视觉标定_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[622]");
        }

        private void btn_下视觉对位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[623]");
        }

        private void btn_下视觉对位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[623]");
        }

        private void btn_上视觉2标定_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[624]");
        }

        private void btn_上视觉2标定_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[624]");
        }

        private void btn_上视觉2对位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[625]");
        }

        private void btn_上视觉2对位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[625]");
        }

        private void btn_上视觉2外观_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[626]");
        }

        private void btn_上视觉2外观_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[626]");
        }

        private void btn下视觉标定有效_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[627]");
        }

        private void btn下视觉标定有效_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[627]");
        }

        private void btn下视觉标定复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[629]");
        }

        private void btn下视觉标定复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[629]");
        }

        private void btn上视觉标定有效_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[627]");
        }

        private void btn上视觉标定有效_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[627]");
        }

        private void btn上视觉标定复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[628]");
        }

        private void btn上视觉标定复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[628]");
        }

        #endregion

        #region 手动定位1
        private void btnX指定位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[212]");
        }

        private void btnX指定位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[212]");
        }

        private void btnX吸1实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[200]");
        }

        private void btnX吸1实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[200]");
        }

        private void btnX吸2实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[201]");
        }

        private void btnX吸2实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[201]");
        }

        private void btnX吸1倒实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[202]");
        }

        private void btnX吸1倒实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[202]");
        }

        private void btnX吸2倒实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[203]");
        }

        private void btnX吸2倒实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[203]");
        }

        private void btnX吸1NG盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[204]");
        }

        private void btnX吸1NG盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[204]");
        }

        private void btnX吸2NG盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[205]");
        }

        private void btnX吸2NG盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[205]");
        }

        private void btnX实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[206]");
        }

        private void btnX实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[206]");
        }

        private void btnX倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[207]");
        }

        private void btnX倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[207]");
        }

        private void btnXNG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[208]");
        }

        private void btnXNG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[208]");
        }

        private void btnX倒NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[209]");
        }

        private void btnX倒NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[209]");
        }

        private void btnX夹爪位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[210]");
        }

        private void btnX夹爪位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[210]");
        }

        private void btnX扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[211]");
        }

        private void btnX扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[211]");
        }

        private void btnX视觉实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[213]");
        }

        private void btnX视觉实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[213]");
        }

        private void btnX视觉倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[214]");
        }

        private void btnX视觉倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[214]");
        }

        private void btnY吸1实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[220]");
        }

        private void btnY吸1实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[220]");
        }

        private void btnY吸2实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[221]");
        }

        private void btnY吸2实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[221]");
        }

        private void btnY吸1倒实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[222]");
        }

        private void btnY吸1倒实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[222]");
        }

        private void btnY吸2倒实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[223]");
        }

        private void btnY吸2倒实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[223]");
        }

        private void btnY吸1NG盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[224]");
        }

        private void btnY吸1NG盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[224]");
        }

        private void btnY吸2NG盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[225]");
        }

        private void btnY吸2NG盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[225]");
        }

        private void btnY实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[226]");
        }

        private void btnY实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[226]");
        }

        private void btnY倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[227]");
        }

        private void btnY倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[227]");
        }

        private void btnYNG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[228]");
        }

        private void btnYNG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[228]");
        }

        private void btnY倒NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[229]");
        }

        private void btnY倒NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[229]");
        }

        private void btnY夹爪位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[230]");
        }

        private void btnY夹爪位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[230]");
        }

        private void btnY扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[231]");
        }

        private void btnY扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[231]");
        }

        private void btnY指定位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[232]");
        }

        private void btnY指定位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[232]");
        }

        private void btnY视觉实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[233]");
        }

        private void btnY视觉实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[233]");
        }

        private void btnY视觉倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[234]");
        }

        private void btnY视觉倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[234]");
        }

        private void btn实盘初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[240]");
        }

        private void btn实盘初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[240]");
        }

        private void btn实盘扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[241]");
        }

        private void btn实盘扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[241]");
        }

        private void btnNG盘初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[242]");
        }

        private void btnNG盘初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[242]");
        }

        private void btn倒实盘初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[243]");
        }

        private void btn倒实盘初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[243]");
        }

        private void btn倒NG盘初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[244]");
        }

        private void btn倒NG盘初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[244]");
        }

        private void btnBY平移上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[250]");
        }

        private void btnBY平移上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[250]");
        }

        private void btnBY平移下料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[251]");
        }

        private void btnBY平移下料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[251]");
        }

        private void btnBY平移中转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[252]");
        }

        private void btnBY平移中转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[252]");
        }

        private void btn旋转一90位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[351]");
        }

        private void btn旋转一90位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[351]");
        }

        private void btn旋转一180位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[352]");
        }

        private void btn旋转一180位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[352]");
        }

        private void btn旋转二90位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[355]");
        }

        private void btn旋转二90位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[355]");
        }

        private void btn旋转二180位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[356]");
        }

        private void btn旋转二180位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[356]");
        }

        private void btnBYX上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[260]");
        }

        private void btnBYX上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[260]");
        }

        private void btnBYX视觉位置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[261]");
        }

        private void btnBYX视觉位置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[261]");
        }

        private void btnBYX第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[262]");
        }

        private void btnBYX第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[262]");
        }

        private void btnBYX第二列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[263]");
        }

        private void btnBYX第二列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[263]");
        }

        private void btnBYX第三列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[264]");
        }

        private void btnBYX第三列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[264]");
        }

        private void btnBYX第四列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[265]");
        }

        private void btnBYX第四列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[265]");
        }

        private void btnBYX第五列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[266]");
        }

        private void btnBYX第五列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[266]");
        }

        private void btnBYX第六列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[267]");
        }

        private void btnBYX第六列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[267]");
        }

        private void btnBYX第七列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[268]");
        }

        private void btnBYX第七列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[268]");
        }

        private void btnBYX第八列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[269]");
        }

        private void btnBYX第八列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[269]");
        }

        private void btnBYX视觉位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[270]");
        }

        private void btnBYX视觉位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[270]");
        }

        private void btnBYY上料12位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[275]");
        }

        private void btnBYY上料12位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[275]");
        }

        private void btnBYY视觉位置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[276]");
        }

        private void btnBYY视觉位置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[276]");
        }

        private void btnBYY视觉位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[277]");
        }

        private void btnBYY视觉位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[277]");
        }

        private void btnBYY第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[278]");
        }

        private void btnBYY第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[278]");
        }

        private void btnBYY第二行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[279]");
        }

        private void btnBYY第二行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[279]");
        }

        private void btnBYY视觉位置3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[280]");
        }

        private void btnBYY视觉位置3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[280]");
        }

        private void btnBYY视觉位置4_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[281]");
        }

        private void btnBYY视觉位置4_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[281]");
        }

        private void btnBYY上料34位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[282]");
        }

        private void btnBYY上料34位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[282]");
        }

        private void btnBYZ上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[285]");
        }

        private void btnBYZ上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[285]");
        }

        private void btnBYZ上升位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[286]");
        }

        private void btnBYZ上升位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[286]");
        }

        private void btnBYZ视觉位置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[287]");
        }

        private void btnBYZ视觉位置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[287]");
        }

        private void btnBYZ视觉位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[288]");
        }

        private void btnBYZ视觉位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[288]");
        }

        private void btnBYZ下料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[289]");
        }

        private void btnBYZ下料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[289]");
        }

        private void btnBYZ视觉位置3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[290]");
        }

        private void btnBYZ视觉位置3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[290]");
        }

        private void btn搬运X移动_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[630]");
        }

        private void btn搬运X移动_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[630]");
        }

        private void btn搬运Y移动_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[631]");
        }

        private void btn搬运Y移动_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[631]");
        }

        private void btn搬运θ移动_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[291]");
        }

        private void btn搬运θ移动_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[291]");
        }

        private void btnBYY视觉位置5_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[273]");
        }

        private void btnBYY视觉位置5_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[273]");
        }

        private void btnBYY视觉位置6_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[274]");
        }

        private void btnBYY视觉位置6_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[274]");
        }

        private void btnBkyi35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[336]");
        }

        private void btnBkyi35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[336]");
        }

        private void btnSokt夹爪3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[256]");
        }

        private void btnSokt夹爪3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[256]");
        }

        private void btnSokt夹爪4_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[257]");
        }

        private void btnSokt夹爪4_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[257]");
        }

        #endregion

        #region 手动定位2
        private void btnSk1黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[295]");
        }

        private void btnSk1黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[295]");
        }

        private void btnSk1翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[296]");
        }

        private void btnSk1翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[296]");
        }

        private void btnSk1测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[297]");
        }

        private void btnSk1测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[297]");
        }

        private void btnSk1测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[298]");
        }

        private void btnSk1测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[298]");
        }

        private void btnSk1测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[299]");
        }

        private void btnSk1测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[299]");
        }

        private void btnSk2黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[305]");
        }

        private void btnSk2黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[305]");
        }

        private void btnSk2翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[306]");
        }

        private void btnSk2翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[306]");
        }

        private void btnSk2测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[307]");
        }

        private void btnSk2测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[307]");
        }

        private void btnSk2测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[308]");
        }

        private void btnSk2测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[308]");
        }

        private void btnSk2测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[309]");
        }

        private void btnSk2测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[309]");
        }

        private void btnSk3黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[315]");
        }

        private void btnSk3黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[315]");
        }

        private void btnSk3翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[316]");
        }

        private void btnSk3翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[316]");
        }

        private void btnSk3测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[317]");
        }

        private void btnSk3测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[317]");
        }

        private void btnSk3测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[318]");
        }

        private void btnSk3测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[318]");
        }

        private void btnSk3测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[319]");
        }

        private void btnSk3测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[319]");
        }

        private void btnSk4黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[325]");
        }

        private void btnSk4黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[325]");
        }

        private void btnSk4翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[326]");
        }

        private void btnSk4翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[326]");
        }

        private void btnSk4测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[327]");
        }

        private void btnSk4测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[327]");
        }

        private void btnSk4测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[328]");
        }

        private void btnSk4测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[328]");
        }

        private void btnSk4测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[329]");
        }

        private void btnSk4测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[329]");
        }

        private void btnBkyi20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[335]");
        }

        private void btnBkyi20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[335]");
        }

        private void btnBker20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[339]");
        }

        private void btnBker20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[339]");
        }

        private void btnBker35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[340]");
        }

        private void btnBker35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[340]");
        }

        private void btnBksan20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[343]");
        }

        private void btnBksan20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[343]");
        }

        private void btnBksan35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[344]");
        }

        private void btnBksan35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[344]");
        }

        private void btnBksi20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[347]");
        }

        private void btnBksi20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[347]");
        }

        private void btnBksi35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[348]");
        }

        private void btnBksi35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[348]");
        }

        private void btnJD1加紧_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[600]");
        }

        private void btnJD1加紧_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[600]");
        }

        private void btnJD1打开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[601]");
        }

        private void btnJD1打开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[601]");
        }

        private void btnJD2加紧_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[602]");
        }

        private void btnJD2加紧_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[602]");
        }

        private void btnJD2打开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[603]");
        }

        private void btnJD2打开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[603]");
        }

        private void btnJD3加紧_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[604]");
        }

        private void btnJD3加紧_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[604]");
        }

        private void btnJD3打开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[605]");
        }

        private void btnJD3打开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[605]");
        }

        private void btnJD4加紧_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[606]");
        }

        private void btnJD4加紧_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[606]");
        }

        private void btnJD4打开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[607]");
        }

        private void btnJD4打开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[607]");
        }
        #endregion

        #region 回原点
        private void btnX回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[7]");
        }

        private void btnX回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[7]");
        }

        private void btnY回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[8]");
        }

        private void btnY回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[8]");
        }

        private void btn实盘回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[9]");
        }

        private void btn实盘回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[9]");
        }

        private void btnNG盘回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[10]");
        }

        private void btnNG盘回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[10]");
        }

        private void btn倒实盘回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[11]");
        }

        private void btn倒实盘回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[11]");
        }

        private void btn倒NG盘回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[12]");
        }

        private void btn倒NG盘回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[12]");
        }

        private void btnBY平移回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[13]");
        }

        private void btnBY平移回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[13]");
        }

        private void btnBYX回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[14]");
        }

        private void btnBYX回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[14]");
        }

        private void btnBYY回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[15]");
        }

        private void btnBYY回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[15]");
        }

        private void btnBYZ回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[16]");
        }

        private void btnBYZ回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[16]");
        }

        private void btnSk1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[17]");
        }

        private void btnSk1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[17]");
        }

        private void btnSk2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[18]");
        }

        private void btnSk2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[18]");
        }

        private void btnSk3回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[19]");
        }

        private void btnSk3回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[19]");
        }

        private void btnSk4回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[20]");
        }

        private void btnSk4回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[20]");
        }

        private void btnBk1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[21]");
        }

        private void btnBk1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[21]");
        }

        private void btnBk2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[22]");
        }

        private void btnBk2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[22]");
        }

        private void btnBk3回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[23]");
        }

        private void btnBk3回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[23]");
        }

        private void btnBk4回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[24]");
        }

        private void btnBk4回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[24]");
        }

        private void 旋转一回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[25]");
        }

        private void 旋转一回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[25]");
        }

        private void 旋转二回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[26]");
        }

        private void 旋转二回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[26]");
        }

        private void btnJD1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[608]");
        }

        private void btnJD1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[608]");
        }

        private void btnJD2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[609]");
        }

        private void btnJD2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[609]");
        }

        private void btnJD3回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[610]");
        }

        private void btnJD3回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[610]");
        }

        private void btnJD4回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[611]");
        }

        private void btnJD4回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[611]");
        }
        #endregion

        #region 手动移动
        private void btnX停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[360]");
        }

        private void btnX停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[360]");
        }

        private void btnX左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[362]");
        }

        private void btnX左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[362]");
        }

        private void btnX右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[361]");
        }

        private void btnX右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[361]");
        }

        private void btnY停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[365]");
        }

        private void btnY停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[365]");
        }

        private void btnY前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[366]");
        }

        private void btnY前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[366]");
        }

        private void btnY后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[367]");
        }

        private void btnY后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[367]");
        }

        private void btn实盘停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[370]");
        }

        private void btn实盘停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[370]");
        }

        private void btn实盘上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[371]");
        }

        private void btn实盘上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[371]");
        }

        private void btn实盘下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[372]");
        }

        private void btn实盘下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[372]");
        }

        private void btnNG盘停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[380]");
        }

        private void btnNG盘停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[380]");
        }

        private void btnNG盘上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[381]");
        }

        private void btnNG盘上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[381]");
        }

        private void btnNG盘下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[382]");
        }

        private void btnNG盘下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[382]");
        }

        private void btn倒实盘停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[375]");
        }

        private void btn倒实盘停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[375]");
        }

        private void btn倒实盘上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[376]");
        }

        private void btn倒实盘上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[376]");
        }

        private void btn倒实盘下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[377]");
        }

        private void btn倒实盘下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[377]");
        }

        private void btn倒NG盘停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[385]");
        }

        private void btn倒NG盘停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[385]");
        }

        private void btn倒NG盘上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[386]");
        }

        private void btn倒NG盘上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[386]");
        }

        private void btn倒NG盘下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[387]");
        }

        private void btn倒NG盘下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[387]");
        }

        private void btnBY平移停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[390]");
        }

        private void btnBY平移停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[390]");
        }

        private void btnBY平移右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[391]");
        }

        private void btnBY平移右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[391]");
        }

        private void btnBY平移左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[392]");
        }

        private void btnBY平移左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[392]");
        }

        private void btnBYX停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[395]");
        }

        private void btnBYX停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[395]");
        }

        private void btnBYX右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[396]");
        }

        private void btnBYX右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[396]");
        }

        private void btnBYX左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[397]");
        }

        private void btnBYX左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[397]");
        }

        private void btnBYY停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[400]");
        }

        private void btnBYY停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[400]");
        }

        private void btnBYY前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[401]");
        }

        private void btnBYY前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[401]");
        }

        private void btnBYY后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[402]");
        }

        private void btnBYY后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[402]");
        }

        private void btnBYZ停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[405]");
        }

        private void btnBYZ停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[405]");
        }

        private void btnBYZ下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[406]");
        }

        private void btnBYZ下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[406]");
        }

        private void btnBYZ上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[407]");
        }

        private void btnBYZ上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[407]");
        }

        private void btn旋转1停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[450]");
        }

        private void btn旋转1停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[450]");
        }

        private void btn旋转1右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[451]");
        }

        private void btn旋转1右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[451]");
        }

        private void btn旋转1左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[452]");
        }

        private void btn旋转1左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[452]");
        }

        private void btn旋转2停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[455]");
        }

        private void btn旋转2停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[455]");
        }

        private void btn旋转2右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[456]");
        }

        private void btn旋转2右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[456]");
        }

        private void btn旋转2左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[457]");
        }

        private void btn旋转2左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[457]");
        }

        private void btnSk1停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[410]");
        }

        private void btnSk1停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[410]");
        }

        private void btnSk1前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[411]");
        }

        private void btnSk1前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[411]");
        }

        private void btnSk1后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[412]");
        }

        private void btnSk1后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[412]");
        }

        private void btnSk2停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[415]");
        }

        private void btnSk2停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[415]");
        }

        private void btnSk2前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[416]");
        }

        private void btnSk2前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[416]");
        }

        private void btnSk2后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[417]");
        }

        private void btnSk2后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[417]");
        }

        private void btnSk3停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[420]");
        }

        private void btnSk3停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[420]");
        }

        private void btnSk3前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[421]");
        }

        private void btnSk3前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[421]");
        }

        private void btnSk3后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[422]");
        }

        private void btnSk3后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[422]");
        }

        private void btnSk4停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[425]");
        }

        private void btnSk4停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[422]");
        }

        private void btnSk4前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[426]");
        }

        private void btnSk4前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[426]");
        }

        private void btnSk4后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[427]");
        }

        private void btnSk4后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[427]");
        }

        private void btnBk1停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[430]");
        }

        private void btnBk1停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[430]");
        }

        private void btnBk1下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[431]");
        }

        private void btnBk1下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[431]");
        }

        private void btnBk1上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[432]");
        }

        private void btnBk1上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[432]");
        }

        private void btnBk2停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[435]");
        }

        private void btnBk2停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[435]");
        }

        private void btnBk2下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[436]");
        }

        private void btnBk2下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[436]");
        }

        private void btnBk2上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[437]");
        }

        private void btnBk2上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[437]");
        }

        private void btnBk3停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[440]");
        }

        private void btnBk3停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[440]");
        }

        private void btnBk3下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[441]");
        }

        private void btnBk3下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[441]");
        }

        private void btnBk3上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[442]");
        }

        private void btnBk3上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[442]");
        }

        private void btnBk4停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[445]");
        }

        private void btnBk4停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[445]");
        }

        private void btnBk4下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[446]");
        }

        private void btnBk4下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[446]");
        }

        private void btnBk4上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[447]");
        }

        private void btnBk4上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[447]");
        }
        #endregion

        #region 写入速度
        private void btn写入速度_Click(object sender, EventArgs e)
        {
            //定位速度
            if (txt上料X轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt上料X轴定位速度设置.Text), "PLCInPmt[0]");
                //communication.WritePLCPmt[0] = Convert.ToDouble(txt上料X轴定位速度设置.Text);
                txt上料X轴定位速度.Text = txt上料X轴定位速度设置.Text;
                txt上料X轴定位速度设置.Text = null;
            }
            if (txt上料Y轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt上料Y轴定位速度设置.Text), "PLCInPmt[1]");
                txt上料Y轴定位速度.Text = txt上料Y轴定位速度设置.Text;
                txt上料Y轴定位速度设置.Text = null;
            }
            if (txt升降轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt升降轴定位速度设置.Text), "PLCInPmt[2]");
                txt升降轴定位速度.Text = txt升降轴定位速度设置.Text;
                txt升降轴定位速度设置.Text = null;
            }
            if (txt平移轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt平移轴定位速度设置.Text), "PLCInPmt[3]");
                txt平移轴定位速度.Text = txt平移轴定位速度设置.Text;
                txt平移轴定位速度设置.Text = null;
            }
            if (txt中空轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt中空轴定位速度设置.Text), "PLCInPmt[4]");
                txt中空轴定位速度.Text = txt中空轴定位速度设置.Text;
                txt中空轴定位速度设置.Text = null;
            }
            if (txt搬运X轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt搬运X轴定位速度设置.Text), "PLCInPmt[5]");
                txt搬运X轴定位速度.Text = txt搬运X轴定位速度设置.Text;
                txt搬运X轴定位速度设置.Text = null;
            }
            if (txt搬运Y轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt搬运Y轴定位速度设置.Text), "PLCInPmt[6]");
                txt搬运Y轴定位速度.Text = txt搬运Y轴定位速度设置.Text;
                txt搬运Y轴定位速度设置.Text = null;
            }
            if (txt搬运Z轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt搬运Z轴定位速度设置.Text), "PLCInPmt[7]");
                txt搬运Z轴定位速度.Text = txt搬运Z轴定位速度设置.Text;
                txt搬运Z轴定位速度设置.Text = null;
            }
            if (txtSocket轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txtSocket轴定位速度设置.Text), "PLCInPmt[8]");
                txtSocket轴定位速度.Text = txtSocket轴定位速度设置.Text;
                txtSocket轴定位速度设置.Text = null;
            }
            if (txt黑体轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt黑体轴定位速度设置.Text), "PLCInPmt[9]");
                txt黑体轴定位速度.Text = txt黑体轴定位速度设置.Text;
                txt黑体轴定位速度设置.Text = null;
            }
            if (txt热辐射轴定位速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt热辐射轴定位速度设置.Text), "PLCInPmt[11]");
                txt热辐射轴定位速度.Text = txt热辐射轴定位速度设置.Text;
                txt热辐射轴定位速度设置.Text = null;
            }


            //手动速度
            if (txt上料X轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt上料X轴手动速度设置.Text), "PLCInPmt[15]");
                txt上料X轴手动速度.Text = txt上料X轴手动速度设置.Text;
                txt上料X轴手动速度设置.Text = null;
            }
            if (txt上料Y轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt上料Y轴手动速度设置.Text), "PLCInPmt[16]");
                txt上料Y轴手动速度.Text = txt上料Y轴手动速度设置.Text;
                txt上料Y轴手动速度设置.Text = null;
            }
            if (txt升降轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt升降轴手动速度设置.Text), "PLCInPmt[17]");
                txt升降轴手动速度.Text = txt升降轴手动速度设置.Text;
                txt升降轴手动速度设置.Text = null;
            }
            if (txt平移轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt平移轴手动速度设置.Text), "PLCInPmt[18]");
                txt平移轴手动速度.Text = txt平移轴手动速度设置.Text;
                txt平移轴手动速度设置.Text = null;
            }
            if (txt中空轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt中空轴手动速度设置.Text), "PLCInPmt[19]");
                txt中空轴手动速度.Text = txt中空轴手动速度设置.Text;
                txt中空轴手动速度设置.Text = null;
            }
            if (txt搬运X轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt搬运X轴手动速度设置.Text), "PLCInPmt[20]");
                txt搬运X轴手动速度.Text = txt搬运X轴手动速度设置.Text;
                txt搬运X轴手动速度设置.Text = null;
            }
            if (txt搬运Y轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt搬运Y轴手动速度设置.Text), "PLCInPmt[21]");
                txt搬运Y轴手动速度.Text = txt搬运Y轴手动速度设置.Text;
                txt搬运Y轴手动速度设置.Text = null;
            }
            if (txt搬运Z轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt搬运Z轴手动速度设置.Text), "PLCInPmt[22]");
                txt搬运Z轴手动速度.Text = txt搬运Z轴手动速度设置.Text;
                txt搬运Z轴手动速度设置.Text = null;
            }
            if (txtSocket轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txtSocket轴手动速度设置.Text), "PLCInPmt[23]");
                txtSocket轴手动速度.Text = txtSocket轴手动速度设置.Text;
                txtSocket轴手动速度设置.Text = null;
            }
            if (txt黑体轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt黑体轴手动速度设置.Text), "PLCInPmt[24]");
                txt黑体轴手动速度.Text = txt黑体轴手动速度设置.Text;
                txt黑体轴手动速度设置.Text = null;
            }
            if (txt热辐射轴手动速度设置.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txt热辐射轴手动速度设置.Text), "PLCInPmt[25]");
                txt热辐射轴手动速度.Text = txt热辐射轴手动速度设置.Text;
                txt热辐射轴手动速度设置.Text = null;
            }

            //communication.WriteVariables(communication.WritePLCPmt, "PLCInPmt", 0, 24);
        }

        //只能输入数字
        private void txt上料X轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt上料Y轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt升降轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt平移轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt中空轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt搬运X轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt搬运Y轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt搬运Z轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txtSocket轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt黑体轴定位速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt上料X轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt上料Y轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt升降轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt平移轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt中空轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt搬运X轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt搬运Y轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt搬运Z轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txtSocket轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void txt黑体轴手动速度设置_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void SpeedSet_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text != "")
                if (double.Parse(textBox.Text) > 500)
                {
                    textBox.Text = "500";
                    MessageBox.Show("输入1-500的值", "提示");
                }
        }

        
        private void AngleSet_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text != "")
                if (textBox.Text != "-")
                    if (double.Parse(textBox.Text) > 90|| double.Parse(textBox.Text)<-90)
                {
                    textBox.Text = "90";
                    MessageBox.Show("输入0-90的值", "提示");
                }
        }

        private void txtSok矫正位置2_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text != "")
                if (textBox.Text != "-")
                    if (double.Parse(textBox.Text) > 2 || double.Parse(textBox.Text) < -2)
                    {
                        textBox.Text = "0";
                        MessageBox.Show("输入-2-2的值", "提示");
                    }
        }

        #endregion

        #region 功能设置
        private void btn门开关功能开关_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[460]");
        }

        private void btn门开关功能开关_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[460]");
        }

        private void btn门开关功能关闭_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[469]");
        }

        private void btn门开关功能关闭_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[469]");
        }

        private void btn真空发生功能开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[470]");
        }

        private void btn真空发生功能开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[470]");
        }

        private void btn真空发生功能关_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[471]");
        }

        private void btn真空发生功能关_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[471]");
        }

        #endregion

        #region 自动界面
        private void BTN_SwitchUser_Click(object sender, EventArgs e)
        {
            loginForm.Show();
            this.Hide();
        }

        private void BTN_Modify_Click(object sender, EventArgs e)
        {
            if (loginForm.CB_UserName.Text == "操作员")
            {
                MessageBox.Show("未授权用户组", "修改密码");
                return;
            }
            if (TB_Password.Text == "" || TB_NewPassword.Text == "")
            {
                MessageBox.Show("请输入密码", "修改密码");
                return;
            }
            if (TB_Password.Text != TB_NewPassword.Text)
            {
                MessageBox.Show("两次输入不一样", "修改密码");
                return;
            }
            JsonManager.SaveJsonString(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\FTData", "engineerData",
                new UserData() { UserType = 1, UserName = "工程师", Password = TB_Password.Text });
            MessageBox.Show("修改成功", "修改密码");
        }

        private void BTN_打开端口_Click(object sender, EventArgs e)
        {
            //打开通信端口
            try
            {
                communication.Compolet.Open();
                isUpdateData = true;
                DataUpdate();
                InterfaceUpdate();
                AlarmCheck();
                MessageBox.Show("打开了端口", "打开端口");
            }
            catch (Exception ex)
            {
                isUpdateData = false;
                MessageBox.Show(ex.Message, "打开端口");
            }
        }

        private void BTN_关闭端口_Click(object sender, EventArgs e)
        {
            try
            {
                //关闭通信端口
                communication.Compolet.Close();
                isUpdateData = false;
                MessageBox.Show("关闭了端口", "打开端口");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "关闭端口");
            }
        }

        private void btn报警复位_Click(object sender, EventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[0]");
            warning.Clear();
            TB_Warning.Clear();
        }

        private void btn蜂鸣停止_Click(object sender, EventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[1]");
        }

        private void btn手动模式_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否切换手动模式？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
               communication.WriteVariable(true, "PlcInIO[2]");
            }
        }

        private void btn自动模式_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否切换自动模式？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PlcInIO[28]");
            }
        }

        private void btn自动运行_Click(object sender, EventArgs e)
        {
            if (CB_TypeOfTray.Text == "" || CB_TypeOfProduction.Text == "" || CB_Socket类.Text == "" || CB_工位.Text == "" || CB_工作盘数.Text == "")
            {
                MessageBox.Show("类别未选择完成，不能启动", "提示");
                return;
            }

            DialogResult result = MessageBox.Show("是否开始自动运行？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PlcInIO[4]");
            }
        }

        private void btn自动停止_Click(object sender, EventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[5]");
        }

        private void btn初始化_Click(object sender, EventArgs e)
        {
            if (CB_TypeOfTray.Text == "" || CB_TypeOfProduction.Text == "" || CB_Socket类.Text == "" || CB_工位.Text == "" || CB_工作盘数.Text == "")
            {
                MessageBox.Show("类别未选择完成，不能启动", "提示");
                return;
            }

            DialogResult result = MessageBox.Show("是否初始化？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PlcInIO[6]");
            }
        }

        private void BTN_查看日志_Click(object sender, EventArgs e)
        {
            Form3 logForm = new Form3();
            logForm.ShowDialog();
        }


        private void CB_TypeOfProduction_SelectedIndexChanged(object sender, EventArgs e)//选择产品索引
        {
            if (this.CB_TypeOfProduction.SelectedItem.ToString() == "GST212W2")
            {
                communication.WriteVariable(1, "PlcInID[1]");
            }

            if (this.CB_TypeOfProduction.SelectedItem.ToString() == "GST612W2")
            {
                communication.WriteVariable(2, "PlcInID[1]");
            }

            if (this.CB_TypeOfProduction.SelectedItem.ToString() == "GST412C0")
            {
                communication.WriteVariable(3, "PlcInID[1]");
            }

            if (this.CB_TypeOfProduction.SelectedItem.ToString() == "GST612M2")
            {
                communication.WriteVariable(4, "PlcInID[1]");
            }

            if (this.CB_TypeOfProduction.SelectedItem.ToString() == "GST612W9")
            {
                communication.WriteVariable(5, "PlcInID[1]");
            }
        }

        private void CB_Socket类_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.CB_Socket类.SelectedItem.ToString() == "四目")
            {
                communication.WriteVariable(1, "PlcInID[2]");
            }

            if (this.CB_Socket类.SelectedItem.ToString() == "单目")
            {
                communication.WriteVariable(2, "PlcInID[2]");
            }
        }

        private void CB_工位_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.CB_工位.SelectedItem.ToString() == "1工位")
            {
                communication.WriteVariable(1, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "2工位")
            {
                communication.WriteVariable(2, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "3工位")
            {
                communication.WriteVariable(3, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "4工位")
            {
                communication.WriteVariable(4, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "12工位")
            {
                communication.WriteVariable(12, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "13工位")
            {
                communication.WriteVariable(13, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "14工位")
            {
                communication.WriteVariable(14, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "23工位")
            {
                communication.WriteVariable(23, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "24工位")
            {
                communication.WriteVariable(24, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "34工位")
            {
                communication.WriteVariable(34, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "123工位")
            {
                communication.WriteVariable(123, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "124工位")
            {
                communication.WriteVariable(124, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "234工位")
            {
                communication.WriteVariable(234, "PlcInID[3]");
            }

            if (this.CB_工位.SelectedItem.ToString() == "1234工位")
            {
                communication.WriteVariable(66, "PlcInID[3]");
            }

        }

        private void CB_工作盘数_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.CB_工作盘数.SelectedItem.ToString() == "1盘")
            {
                communication.WriteVariable(1, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "2盘")
            {
                communication.WriteVariable(2, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "3盘")
            {
                communication.WriteVariable(3, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "4盘")
            {
                communication.WriteVariable(4, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "5盘")
            {
                communication.WriteVariable(5, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "6盘")
            {
                communication.WriteVariable(6, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "7盘")
            {
                communication.WriteVariable(7, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "8盘")
            {
                communication.WriteVariable(8, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "9盘")
            {
                communication.WriteVariable(9, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "10盘")
            {
                communication.WriteVariable(10, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "11盘")
            {
                communication.WriteVariable(11, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "12盘")
            {
                communication.WriteVariable(12, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "13盘")
            {
                communication.WriteVariable(13, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "14盘")
            {
                communication.WriteVariable(14, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "15盘")
            {
                communication.WriteVariable(15, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "16盘")
            {
                communication.WriteVariable(16, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "17盘")
            {
                communication.WriteVariable(17, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "18盘")
            {
                communication.WriteVariable(18, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "19盘")
            {
                communication.WriteVariable(19, "PlcInID[5]");
            }

            if (this.CB_工作盘数.SelectedItem.ToString() == "20盘")
            {
                communication.WriteVariable(20, "PlcInID[5]");
            }

        }

        #endregion

        #region 窗口关闭
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                loginForm.Close();
                //关闭通信端口
                communication.Compolet.Close();
            }
            catch (Exception)
            {

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //让用户选择点击
            DialogResult result = MessageBox.Show("是否确认关闭？", "提示",MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //判断是否取消事件
            if (result == DialogResult.No)
            {
                //取消退出
                e.Cancel = true;
            }
        }
        #endregion

        #region 后来加
        private void btnBYY示教Sokt夹爪3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[96]");
        }

        private void btnBYY示教Sokt夹爪3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[96]");
        }

        private void btnBYY示教Sokt夹爪4_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[97]");
        }

        private void btnBYY示教Sokt夹爪4_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[97]");
        }

        private void btn光源开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[467]");
        }

        private void btn光源开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[467]");
        }

        private void btn光源关_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[468]");
        }

        private void btn光源关_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[468]");
        }

        private void btnJD清除状态_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[613]");
        }

        private void btnJD清除状态_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[613]");
        }

        private void btn一键真空破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[586]");
        }

        private void btn一键真空破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[586]");
        }

        private void btnJD1打开小位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[594]");
        }

        private void btnJD1打开小位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[594]");
        }

        private void btnJD1相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtR1旋转角度.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txtR1旋转角度.Text), "PLCInPmt[26]");
            }
            communication.WriteVariable(true, "PlcInIO[596]");
        }

        private void btnJD1相对旋转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[596]");
        }

        private void btnBYY示教Sokt夹爪1记忆_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[95]");
        }

        private void btnBYY示教Sokt夹爪1记忆_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[95]");
        }

        private void btnBYYSokt夹爪1记忆_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[272]");
        }

        private void btnBYYSokt夹爪1记忆_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[272]");
        }

        private void btnJD2打开小位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[595]");
        }

        private void btnJD2打开小位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[595]");
        }

        private void btnJD3打开小位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[592]");
        }

        private void btnJD3打开小位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[592]");
        }

        private void btnJD4打开小位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[593]");
        }

        private void btnJD4打开小位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[593]");
        }

        private void btnJD2相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtR2旋转角度.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txtR2旋转角度.Text), "PLCInPmt[27]");
            }
            communication.WriteVariable(true, "PlcInIO[597]");
        }

        private void btnJD2相对旋转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[597]");
        }

        private void btnJD3相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtR3旋转角度.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txtR3旋转角度.Text), "PLCInPmt[28]");
            }
            communication.WriteVariable(true, "PlcInIO[598]");
        }

        private void btnJD3相对旋转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[598]");
        }

        private void btnJD4相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtR4旋转角度.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(txtR4旋转角度.Text), "PLCInPmt[29]");
            }
            communication.WriteVariable(true, "PlcInIO[599]");
        }

        private void btnJD4相对旋转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[599]");
        }

        private void btnX上一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[216]");
        }

        private void btnX上一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[216]");
        }

        private void btnX下一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[215]");
        }

        private void btnX下一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[215]");
        }

        private void btnY上一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[236]");
        }

        private void btnY上一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[236]");
        }

        private void btnY下一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[235]");
        }

        private void btnY下一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[235]");
        }

        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[292]");
        }

        private void button2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[292]");
        }

        private void btnBYZSok位置34_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[293]");
        }

        private void btnBYZSok位置34_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[293]");
        }

        private void btnBYX示教矫正位置_MouseDown_1(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[635]");
        }

        private void btnBYX示教矫正位置_MouseUp_1(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[635]");
        }

        private void btnBYX示教矫正位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[636]");
        }

        private void btnBYX示教矫正位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[636]");
        }

        private void btnBYX示教矫正位置3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[637]");
        }

        private void btnBYX示教矫正位置3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[637]");
        }

        private void btnBYX示教矫正位置4_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[638]");
        }

        private void btnBYX示教矫正位置4_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[638]");
        }

        private void btnBYX示教矫正位置5_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[639]");
        }

        private void btnBYX示教矫正位置5_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[639]");
        }

        private void btnBYX示教矫正位置6_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[640]");
        }

        private void btnBYX示教矫正位置6_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[640]");
        }

        private void btnBYX示教矫正位置7_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[641]");
        }

        private void btnBYX示教矫正位置7_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[641]");
        }

        private void btnBYX示教矫正位置8_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[642]");
        }

        private void btnBYX示教矫正位置8_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[642]");
        }

        //private void btn矫正按钮_Click(object sender, EventArgs e)
        //{
        //    if (txtSok矫正位置2.Text != "")
        //    {
        //        communication.WriteVariable(Convert.ToDouble(txtSok矫正位置2.Text), "PLCInPmt[32]");
        //        txtSok矫正位置2.Text = null;
        //    }

        //    if (txtSok矫正位置3.Text != "")
        //    {
        //        communication.WriteVariable(Convert.ToDouble(txtSok矫正位置3.Text), "PLCInPmt[33]");
        //        txtSok矫正位置3.Text = null;
        //    }

        //    if (txtSok矫正位置4.Text != "")
        //    {
        //        communication.WriteVariable(Convert.ToDouble(txtSok矫正位置4.Text), "PLCInPmt[34]");
        //        txtSok矫正位置4.Text = null;
        //    }
        //}

        private void btn自动模式本地_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否选择自动模式本地？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PlcInIO[3]");
                communication.WriteVariable(false, "PlcInIO[27]");
            }
        }

        private void btn自动模式远程_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否选择自动模式远程？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PlcInIO[27]");
                communication.WriteVariable(false, "PlcInIO[3]");
            }
        }

        private void btn上料命令_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否选择上料？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PCwrite[0]");
            }
        }

        private void btn下料命令_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否选择下料？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PCwrite[1]");
            }
        }

        private void btn工装移入命令_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否工装移入？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PCwrite[4]");
            }
        }

        private void btn工装移出命令_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否工装移出？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PCwrite[5]");
            }
        }

        private void btn工装移入低温命令_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否移入低温位置？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PCwrite[2]");
            }
        }

        private void btn工装移入高温命令_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否移入高温位置？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PCwrite[3]");
            }
        }

       
        private void btnRFB1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[721]");
        }

        private void btnRFB1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[721]");
        }

        private void btnRFB2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[722]");
        }

        private void btnRFB2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[722]");
        }

        private void btnRFB3回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[723]");
        }

        private void btnRFB3回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[723]");
        }

        private void btnRFB4回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[724]");
        }

        private void btnRFB4回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[724]");
        }

        private void btnRFByi60位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[700]");
        }

        private void btnRFByi60位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[700]");
        }

        private void btnRFBer60位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[705]");
        }

        private void btnRFBer60位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[705]");
        }

        private void btnRFBsan60位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[710]");
        }

        private void btnRFBsan60位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[710]");
        }

        private void btnRFBsi60位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[715]");
        }

        private void btnRFBsi60位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[715]");
        }

        private void btnRFByi120位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[702]");
        }

        private void btnRFByi120位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[702]");
        }

        private void btnRFBer120位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[707]");
        }

        private void btnRFBer120位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[707]");
        }

        private void btnRFBsan120位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[712]");
        }

        private void btnRFBsan120位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[712]");
        }

        private void btnRFBsi120位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[717]");
        }

        private void btnRFBsi120位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[717]");
        }

        private void btnRFByi90位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[701]");
        }

        private void btnRFByi90位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[701]");
        }

        private void btnRFBer90位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[706]");
        }

        private void btnRFBer90位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[706]");
        }

        private void btnRFBsan90位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[711]");
        }

        private void btnRFBsan90位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[711]");
        }

        private void btnRFBsi90位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[716]");
        }

        private void btnRFBsi90位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[716]");
        }

        private void btnRFB1左转_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[651]");
        }

        private void btnRFB1左转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[651]");
        }

        private void btnRFB1停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[650]");
        }

        private void btnRFB1停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[650]");
        }

        private void btnRFB1右转_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[652]");
        }

        private void btnRFB1右转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[652]");
        }

        private void btnRFB2左转_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[656]");
        }

        private void btnRFB2左转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[656]");
        }

        private void btnRFB2停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[655]");
        }

        private void btnRFB2停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[655]");
        }

        private void btnRFB2右转_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[657]");
        }

        private void btnRFB2右转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[657]");
        }

        private void btnRFB3左转_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[661]");
        }

        private void btnRFB3左转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[661]");
        }

        private void btnRFB3停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[660]");
        }

        private void btnRFB3停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[660]");
        }

        private void btnRFB3右转_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[662]");
        }

        private void btnRFB3右转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[662]");
        }

        private void btnRFB4左转_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[666]");
        }

        private void btnRFB4左转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[666]");
        }

        private void btnRFB4停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[665]");
        }

        private void btnRFB4停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[665]");
        }

        private void btnRFB4右转_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[667]");
        }

        private void btnRFB4右转_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[667]");
        }

        private void btn倒实一级伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[730]");
        }

        private void btn倒实一级伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[730]");
        }

        private void btn倒实一级缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[731]");
        }

        private void btn倒实一级缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[731]");
        }

        private void btn倒实二级伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[732]");
        }

        private void btn倒实二级伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[732]");
        }

        private void btn倒实二级缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[733]");
        }

        private void btn倒实二级缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[733]");
        }

        private void btnNG一级伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[734]");
        }

        private void btnNG一级伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[734]");
        }

        private void btnNG一级缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[735]");
        }

        private void btnNG一级缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[735]");
        }

        private void btnNG二级伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[736]");
        }

        private void btnNG二级伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[736]");
        }

        private void btnNG二级缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[737]");
        }

        private void btnNG二级缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[737]");
        }

        private void btn热板上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[472]");
        }

        private void btn热板上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[472]");
        }

        private void btn热板断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[473]");
        }

        private void btn热板断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[473]");
        }

        private void btn黑体一键上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[334]");
        }

        private void btn黑体一键上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[334]");
        }

        private void CB_选择夹爪_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.CB_选择夹爪.SelectedItem.ToString() == "夹爪1")
            {
                communication.WriteVariable(1, "PlcInID[4]");
            }

            if (this.CB_选择夹爪.SelectedItem.ToString() == "夹爪2")
            {
                communication.WriteVariable(2, "PlcInID[4]");
            }
        }

        private void btn平移示教中转位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[74]");
        }

        private void btn平移示教中转位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[74]");
        }

        private void btn计数功能清零_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[474]");
        }

        private void btn计数功能清零_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[474]");
        }

        private void btn人工上下料_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[210]");
        }

        private void btn人工上下料_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[210]");
        }

        private void btn一键置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[475]");
        }

        private void btn一键置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[475]");
        }

        private void btn自动模式远程测试_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[29]");
        }

        private void btn自动模式远程测试_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(false, "PlcInIO[29]");
        }

        #endregion

        private void BTN_Test0_Click(object sender, EventArgs e)
        {
            //托盘附码
            communication.WriteVariable(true, "PLC标志位[0]");
        }

        private void BTN_Test1_Click(object sender, EventArgs e)
        {
            //测试完成
            communication.WriteVariable(true, "PLC标志位[1]");
        }

        private void BTN_Test2_Click(object sender, EventArgs e)
        {
            //初始化托盘数据
            communication.WriteVariable(true, "PLC标志位[2]");
        }

        
    }
}
