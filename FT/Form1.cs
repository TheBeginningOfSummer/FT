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
            TC_Main.Selecting += new TabControlCancelEventHandler(TC_Main_Selecting);
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

                #region 托盘数据
                //托盘数据
                trayManager = new TrayManager();
                //托盘类型设置
                foreach (var trayType in trayManager.TrayType)
                {
                    CB_TypeOfTray.Items.Add(trayType.Key);
                }
                #endregion

                //报警信息读取
                alarmInformation = JsonManager.ReadJsonString<Dictionary<string, string>>(Environment.CurrentDirectory + "\\Configuration\\", "Alarm");
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "程序初始化");
            }
        }

        void TC_Main_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (true)//禁用某个Tab
            {
                if (e.TabPageIndex == 20) e.Cancel = true;
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
                        communication.RefreshData();

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

                        #region 更新数据
                        //示教界面数据更新
                        SetTextBoxText(txtX示教吸1实盘第一列, communication.ReadLocation[24]);
                        SetTextBoxText(txtX示教吸2实盘第一列, communication.ReadLocation[25]);
                        SetTextBoxText(txtX示教吸1倒实盘第一列, communication.ReadLocation[26]);
                        SetTextBoxText(txtX示教吸2倒实盘第一列, communication.ReadLocation[27]);
                        SetTextBoxText(txtX示教吸1NG盘第一列, communication.ReadLocation[28]);
                        SetTextBoxText(txtX示教吸2NG盘第一列, communication.ReadLocation[29]);
                        SetTextBoxText(txtX示教实盘位置, communication.ReadLocation[30]);
                        SetTextBoxText(txtX示教倒实盘位置, communication.ReadLocation[31]);
                        SetTextBoxText(txtX示教NG盘位置, communication.ReadLocation[32]);
                        SetTextBoxText(txtX示教倒NG盘位置, communication.ReadLocation[33]);
                        SetTextBoxText(txtX示教夹爪位置, communication.ReadLocation[34]);
                        SetTextBoxText(txtX示教扫码位置, communication.ReadLocation[35]);
                        SetTextBoxText(txtX示教视觉实盘第一列, communication.ReadLocation[36]);
                        SetTextBoxText(txtX示教视觉倒实盘第一列, communication.ReadLocation[37]);

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
                        SetTextBoxText(txt实盘示教扫码位置, communication.ReadLocation[59]);

                        SetTextBoxText(txtNG盘示教初始位置, communication.ReadLocation[65]);

                        SetTextBoxText(txt倒实盘示教初始位置, communication.ReadLocation[69]);

                        SetTextBoxText(txt倒NG盘示教初始位置, communication.ReadLocation[73]);

                        //SetTextBoxText(txt平移上料位置, communication.ReadLocation[77]);
                        //SetTextBoxText(txtX平移下料位置, communication.ReadLocation[78]);
                        //SetTextBoxText(txtX平移翻转位置, communication.ReadLocation[79]);

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

                        SetTextBoxText(txtBYY示教上料位置, communication.ReadLocation[97]);
                        SetTextBoxText(txtBYY示教视觉1位置, communication.ReadLocation[98]);
                        SetTextBoxText(txtBYY示教视觉2位置, communication.ReadLocation[99]);
                        SetTextBoxText(txtBYY示教第一行, communication.ReadLocation[100]);
                        SetTextBoxText(txtBYY示教第二行, communication.ReadLocation[101]);
                        SetTextBoxText(txtBYY示教视觉3位置, communication.ReadLocation[102]);
                        SetTextBoxText(txtBYY示教视觉4位置, communication.ReadLocation[103]);

                        SetTextBoxText(txtBYZ示教上料位置, communication.ReadLocation[107]);
                        SetTextBoxText(txtBYZ示教上升位置, communication.ReadLocation[108]);
                        SetTextBoxText(txtBYZ示教视觉位置1, communication.ReadLocation[109]);
                        SetTextBoxText(txtBYZ示教视觉位置2, communication.ReadLocation[110]);
                        SetTextBoxText(txtBYZ下料视觉位置, communication.ReadLocation[111]);
                        SetTextBoxText(txtBYZ示教视觉位置3, communication.ReadLocation[112]);

                        SetTextBoxText(txtSk1示教黑体位置, communication.ReadLocation[117]);
                        SetTextBoxText(txtSk1示教翻转位置, communication.ReadLocation[118]);
                        SetTextBoxText(txtSk1示教测试1位置, communication.ReadLocation[119]);
                        SetTextBoxText(txtSk1示教测试2位置, communication.ReadLocation[120]);
                        SetTextBoxText(txtSk1示教测试3位置, communication.ReadLocation[121]);

                        SetTextBoxText(txtSk2示教黑体位置, communication.ReadLocation[125]);
                        SetTextBoxText(txtSk2示教翻转位置, communication.ReadLocation[126]);
                        SetTextBoxText(txtSk2示教测试1位置, communication.ReadLocation[127]);
                        SetTextBoxText(txtSk2示教测试2位置, communication.ReadLocation[128]);
                        SetTextBoxText(txtSk2示教测试3位置, communication.ReadLocation[129]);

                        SetTextBoxText(txtSk3示教黑体位置, communication.ReadLocation[133]);
                        SetTextBoxText(txtSk3示教翻转位置, communication.ReadLocation[134]);
                        SetTextBoxText(txtSk3示教测试1位置, communication.ReadLocation[135]);
                        SetTextBoxText(txtSk3示教测试2位置, communication.ReadLocation[136]);
                        SetTextBoxText(txtSk3示教测试3位置, communication.ReadLocation[137]);

                        SetTextBoxText(txtSk4示教黑体位置, communication.ReadLocation[141]);
                        SetTextBoxText(txtSk4示教翻转位置, communication.ReadLocation[142]);
                        SetTextBoxText(txtSk4示教测试1位置, communication.ReadLocation[143]);
                        SetTextBoxText(txtSk4示教测试2位置, communication.ReadLocation[144]);
                        SetTextBoxText(txtSk4示教测试3位置, communication.ReadLocation[145]);

                        SetTextBoxText(txtBk1示教20位置, communication.ReadLocation[149]);
                        SetTextBoxText(txtBk1示教35位置, communication.ReadLocation[150]);

                        SetTextBoxText(txtBk2示教20位置, communication.ReadLocation[154]);
                        SetTextBoxText(txtBk2示教35位置, communication.ReadLocation[155]);

                        SetTextBoxText(txtBk3示教20位置, communication.ReadLocation[159]);
                        SetTextBoxText(txtBk3示教35位置, communication.ReadLocation[160]);

                        SetTextBoxText(txtBk4示教20位置, communication.ReadLocation[164]);
                        SetTextBoxText(txtBk4示教35位置, communication.ReadLocation[165]);

                        //当前位置
                        SetTextBoxText(txtX示教当前位置, communication.ReadLocation[0]);
                        SetTextBoxText(txtY示教当前位置, communication.ReadLocation[1]);
                        SetTextBoxText(txt实盘示教当前位置, communication.ReadLocation[2]);
                        SetTextBoxText(txtNG盘示教当前位置, communication.ReadLocation[3]);
                        SetTextBoxText(txt倒实盘示教当前位置, communication.ReadLocation[4]);
                        //SetTextBoxText(txt平移示教当前位置, communication.ReadLocation[5]);
                        SetTextBoxText(txtX示教当前位置, communication.ReadLocation[6]);
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
                        //SetTextBoxText(txtX中空1当前位置, communication.ReadLocation[18]);
                        //SetTextBoxText(txtX中空2当前位置, communication.ReadLocation[19]);

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
                        SetTextBoxText(txtJD1当前位置, communication.ReadLocation[20]);
                        SetTextBoxText(txtJD2当前位置, communication.ReadLocation[21]);
                        SetTextBoxText(txtJD3当前位置, communication.ReadLocation[22]);
                        SetTextBoxText(txtJD4当前位置, communication.ReadLocation[23]);

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
                        //SetLabelColor(communication.ReadPLCIO[128], X128);
                        //SetLabelColor(communication.ReadPLCIO[129], X129);
                        //SetLabelColor(communication.ReadPLCIO[130], X130);
                        //SetLabelColor(communication.ReadPLCIO[131], X131);
                        //SetLabelColor(communication.ReadPLCIO[132], X132);
                        //SetLabelColor(communication.ReadPLCIO[133], X133);
                        //SetLabelColor(communication.ReadPLCIO[134], X134);
                        //SetLabelColor(communication.ReadPLCIO[135], X135);
                        //SetLabelColor(communication.ReadPLCIO[136], X136);
                        //SetLabelColor(communication.ReadPLCIO[137], X137);
                        //SetLabelColor(communication.ReadPLCIO[138], X138);
                        //SetLabelColor(communication.ReadPLCIO[139], X139);
                        //SetLabelColor(communication.ReadPLCIO[140], X140);
                        //SetLabelColor(communication.ReadPLCIO[141], X141);
                        //SetLabelColor(communication.ReadPLCIO[142], X142);
                        //SetLabelColor(communication.ReadPLCIO[143], X143);

                        SetLabelColor(communication.ReadPLCIO[150], Y00);
                        SetLabelColor(communication.ReadPLCIO[151], Y01);
                        SetLabelColor(communication.ReadPLCIO[152], Y02);
                        SetLabelColor(communication.ReadPLCIO[153], Y03);
                        SetLabelColor(communication.ReadPLCIO[154], Y04);
                        SetLabelColor(communication.ReadPLCIO[155], Y05);
                        SetLabelColor(communication.ReadPLCIO[156], Y06);
                        SetLabelColor(communication.ReadPLCIO[157], Y07);
                        SetLabelColor(communication.ReadPLCIO[158], Y08);
                        SetLabelColor(communication.ReadPLCIO[159], Y09);
                        SetLabelColor(communication.ReadPLCIO[160], Y10);
                        SetLabelColor(communication.ReadPLCIO[161], Y11);
                        SetLabelColor(communication.ReadPLCIO[162], Y12);
                        SetLabelColor(communication.ReadPLCIO[163], Y13);
                        SetLabelColor(communication.ReadPLCIO[164], Y14);
                        SetLabelColor(communication.ReadPLCIO[165], Y15);
                        SetLabelColor(communication.ReadPLCIO[166], Y16);
                        SetLabelColor(communication.ReadPLCIO[167], Y17);
                        SetLabelColor(communication.ReadPLCIO[168], Y18);
                        SetLabelColor(communication.ReadPLCIO[169], Y19);
                        SetLabelColor(communication.ReadPLCIO[170], Y20);
                        SetLabelColor(communication.ReadPLCIO[171], Y21);
                        SetLabelColor(communication.ReadPLCIO[172], Y22);
                        SetLabelColor(communication.ReadPLCIO[173], Y23);
                        SetLabelColor(communication.ReadPLCIO[174], Y24);
                        SetLabelColor(communication.ReadPLCIO[175], Y25);
                        SetLabelColor(communication.ReadPLCIO[176], Y26);
                        SetLabelColor(communication.ReadPLCIO[177], Y27);
                        SetLabelColor(communication.ReadPLCIO[178], Y28);
                        SetLabelColor(communication.ReadPLCIO[179], Y29);
                        SetLabelColor(communication.ReadPLCIO[180], Y30);
                        SetLabelColor(communication.ReadPLCIO[181], Y31);
                        SetLabelColor(communication.ReadPLCIO[182], Y32);
                        SetLabelColor(communication.ReadPLCIO[183], Y33);
                        SetLabelColor(communication.ReadPLCIO[184], Y34);
                        SetLabelColor(communication.ReadPLCIO[185], Y35);
                        SetLabelColor(communication.ReadPLCIO[186], Y36);
                        SetLabelColor(communication.ReadPLCIO[187], Y37);
                        SetLabelColor(communication.ReadPLCIO[188], Y38);
                        SetLabelColor(communication.ReadPLCIO[189], Y39);
                        SetLabelColor(communication.ReadPLCIO[190], Y40);
                        SetLabelColor(communication.ReadPLCIO[191], Y41);
                        SetLabelColor(communication.ReadPLCIO[192], Y42);
                        SetLabelColor(communication.ReadPLCIO[193], Y43);
                        SetLabelColor(communication.ReadPLCIO[194], Y44);
                        SetLabelColor(communication.ReadPLCIO[195], Y45);
                        SetLabelColor(communication.ReadPLCIO[196], Y46);
                        SetLabelColor(communication.ReadPLCIO[197], Y47);
                        SetLabelColor(communication.ReadPLCIO[198], Y48);
                        SetLabelColor(communication.ReadPLCIO[199], Y49);
                        SetLabelColor(communication.ReadPLCIO[200], Y50);
                        SetLabelColor(communication.ReadPLCIO[201], Y51);
                        SetLabelColor(communication.ReadPLCIO[202], Y52);
                        SetLabelColor(communication.ReadPLCIO[203], Y53);
                        SetLabelColor(communication.ReadPLCIO[204], Y54);
                        SetLabelColor(communication.ReadPLCIO[205], Y55);
                        SetLabelColor(communication.ReadPLCIO[206], Y56);
                        SetLabelColor(communication.ReadPLCIO[207], Y57);
                        SetLabelColor(communication.ReadPLCIO[208], Y58);
                        SetLabelColor(communication.ReadPLCIO[209], Y59);
                        SetLabelColor(communication.ReadPLCIO[210], Y60);
                        SetLabelColor(communication.ReadPLCIO[211], Y61);
                        SetLabelColor(communication.ReadPLCIO[212], Y62);
                        SetLabelColor(communication.ReadPLCIO[213], Y63);
                        SetLabelColor(communication.ReadPLCIO[214], Y64);
                        SetLabelColor(communication.ReadPLCIO[215], Y65);
                        SetLabelColor(communication.ReadPLCIO[216], Y66);
                        SetLabelColor(communication.ReadPLCIO[217], Y67);
                        SetLabelColor(communication.ReadPLCIO[218], Y68);
                        SetLabelColor(communication.ReadPLCIO[219], Y69);
                        SetLabelColor(communication.ReadPLCIO[220], Y70);
                        SetLabelColor(communication.ReadPLCIO[221], Y71);
                        SetLabelColor(communication.ReadPLCIO[222], Y72);
                        SetLabelColor(communication.ReadPLCIO[223], Y73);
                        SetLabelColor(communication.ReadPLCIO[224], Y74);
                        SetLabelColor(communication.ReadPLCIO[225], Y75);
                        SetLabelColor(communication.ReadPLCIO[226], Y76);
                        SetLabelColor(communication.ReadPLCIO[227], Y77);
                        SetLabelColor(communication.ReadPLCIO[228], Y78);
                        SetLabelColor(communication.ReadPLCIO[229], Y79);
                        SetLabelColor(communication.ReadPLCIO[230], Y80);
                        SetLabelColor(communication.ReadPLCIO[231], Y81);
                        SetLabelColor(communication.ReadPLCIO[232], Y82);
                        SetLabelColor(communication.ReadPLCIO[233], Y83);
                        SetLabelColor(communication.ReadPLCIO[234], Y84);
                        SetLabelColor(communication.ReadPLCIO[235], Y85);
                        SetLabelColor(communication.ReadPLCIO[236], Y86);
                        SetLabelColor(communication.ReadPLCIO[237], Y87);
                        SetLabelColor(communication.ReadPLCIO[238], Y88);
                        SetLabelColor(communication.ReadPLCIO[239], Y89);
                        SetLabelColor(communication.ReadPLCIO[240], Y90);
                        SetLabelColor(communication.ReadPLCIO[241], Y91);
                        SetLabelColor(communication.ReadPLCIO[242], Y92);
                        SetLabelColor(communication.ReadPLCIO[243], Y93);
                        SetLabelColor(communication.ReadPLCIO[244], Y94);
                        SetLabelColor(communication.ReadPLCIO[245], Y95);
                        SetLabelColor(communication.ReadPLCIO[246], Y96);
                        SetLabelColor(communication.ReadPLCIO[247], Y97);
                        SetLabelColor(communication.ReadPLCIO[248], Y98);
                        SetLabelColor(communication.ReadPLCIO[249], Y99);
                        SetLabelColor(communication.ReadPLCIO[250], Y100);
                        SetLabelColor(communication.ReadPLCIO[251], Y101);
                        SetLabelColor(communication.ReadPLCIO[252], Y102);
                        SetLabelColor(communication.ReadPLCIO[253], Y103);
                        SetLabelColor(communication.ReadPLCIO[254], Y104);
                        SetLabelColor(communication.ReadPLCIO[255], Y105);
                        SetLabelColor(communication.ReadPLCIO[256], Y103);
                        SetLabelColor(communication.ReadPLCIO[257], Y107);
                        SetLabelColor(communication.ReadPLCIO[258], Y108);
                        SetLabelColor(communication.ReadPLCIO[259], Y109);
                        SetLabelColor(communication.ReadPLCIO[260], Y110);
                        SetLabelColor(communication.ReadPLCIO[261], Y111);
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

                        SetTextBoxText(txt上料X轴手动速度, communication.ReadPLCPmt[15]);
                        //SetTextBoxText(txt上料Y轴手动速度, communication.ReadPLCPmt[16]);
                        SetTextBoxText(txt升降轴手动速度, communication.ReadPLCPmt[17]);
                        SetTextBoxText(txt平移轴手动速度, communication.ReadPLCPmt[18]);
                        SetTextBoxText(txt中空轴手动速度, communication.ReadPLCPmt[19]);
                        SetTextBoxText(txt搬运X轴手动速度, communication.ReadPLCPmt[20]);
                        SetTextBoxText(txt搬运Y轴手动速度, communication.ReadPLCPmt[21]);
                        SetTextBoxText(txt搬运Z轴手动速度, communication.ReadPLCPmt[22]);
                        SetTextBoxText(txtSocket轴手动速度, communication.ReadPLCPmt[23]);
                        SetTextBoxText(txt黑体轴手动速度, communication.ReadPLCPmt[24]);
                        #endregion
                    }
                    catch (Exception)
                    {
                        //logfile.Writelog("数据更新循环：" + e.Message);
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
                                logfile.Writelog(alarmInformation[i.ToString()], "报警记录");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logfile.Writelog("报警监测循环：" + e.Message, "报警记录");
                    }
                }
            });
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
            trayManager.InitializeTrays(CB_TypeOfTray.Text);
            foreach (var tray in trayManager.Trays)
            {
                tray.UpdateTrayLabel(PN_Trays);
            }
            //PN_Trays.Controls.Clear();
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

        private void btnY示教吸1实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[31] = true;
        }

        private void btnY示教吸1实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[31] = false;
        }

        private void btnX示教吸2实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[32] = true;
        }

        private void btnX示教吸2实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[32] = false;
        }

        private void btnY示教吸2实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[33] = true;
        }

        private void btnY示教吸2实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[33] = false;
        }

        private void btnX示教实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[34] = true;
        }

        private void btnX示教实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[34] = false;
        }

        private void btnX示教倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[35] = true;
        }

        private void btnX示教倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[35] = false;
        }

        private void btnX示教NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[36] = true;
        }

        private void btnX示教NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[36] = false;
        }

        private void btnX示教倒NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[37] = true;
        }

        private void btnX示教倒NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[37] = false;
        }

        private void btnX示教夹爪位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[38] = true;
        }

        private void btnX示教夹爪位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[38] = false;
        }

        private void btnX示教扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[39] = true;
        }

        private void btnX示教扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[39] = false;
        }

        private void btnX示教吸1NG盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[40] = true;
        }

        private void btnX示教吸1NG盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[40] = false;
        }

        private void btnY示教吸1NG盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[41] = true;
        }

        private void btnY示教吸1NG盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[41] = false;
        }

        private void btnX示教吸2NG盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[42] = true;
        }

        private void btnX示教吸2NG盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[42] = false;
        }

        private void btnY示教吸2NG盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[43] = true;
        }

        private void btnY示教吸2NG盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[43] = false;
        }

        private void btnY示教实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[45] = true;
        }

        private void btnY示教实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[45] = false;
        }

        private void btnY示教倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[46] = true;
        }

        private void btnY示教倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[46] = false;
        }

        private void btnY示教NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[47] = true;
        }

        private void btnY示教NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[47] = false;
        }

        private void btnY示教倒NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[48] = true;
        }

        private void btnY示教倒NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[48] = false;
        }

        private void btnY示教夹爪位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[49] = true;
        }

        private void btnY示教夹爪位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[49] = false;
        }

        private void btnY示教扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[50] = true;
        }

        private void btnY示教扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[50] = false;
        }

        private void btnX示教吸1倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[51] = true;
        }

        private void btnX示教吸1倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[51] = false;
        }

        private void btnY示教吸1倒实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[52] = true;
        }

        private void btnY示教吸1倒实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[52] = false;
        }

        private void btnX示教吸2倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[53] = true;
        }

        private void btnX示教吸2倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[53] = false;
        }

        private void btnY示教吸2倒实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[54] = true;
        }

        private void btnY示教吸2倒实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[54] = false;
        }

        private void btn实盘示教初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[56] = true;
        }

        private void btn实盘示教初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[56] = false;
        }

        private void btn实盘示教扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[57] = true;
        }

        private void btn实盘示教扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[57] = false;
        }

        private void btnNG盘示教初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[61] = true;
        }

        private void btnNG盘示教初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[61] = false;
        }

        private void btn倒实盘示教初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[62] = true;
        }

        private void btn倒实盘示教初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[62] = false;
        }

        private void btn倒NG盘示教初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[63] = true;
        }

        private void btn倒NG盘示教初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[63] = false;
        }

        private void btn平移示教上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[71] = true;
        }

        private void btn平移示教上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[71] = false;
        }

        private void btn平移示教下料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[72] = true;
        }

        private void btn平移示教下料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[72] = false;
        }

        private void btn平移示教中转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[73] = true;
        }

        private void btn平移示教中转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[73] = false;
        }
    }
}
