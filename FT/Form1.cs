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

        Form2 form;

        public Form1(Form2 form2)
        {
            InitializeComponent();
            //TC_Main.Selecting += new TabControlCancelEventHandler(TC_Main_Selecting);

            form = form2;

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

                DataUpdate();
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
                        Thread.Sleep(100);
                        //communication.RefreshData();

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

                        SetTextBoxText(txt平移示教上料位置, communication.ReadLocation[77]);
                        SetTextBoxText(txt平移示教下料位置, communication.ReadLocation[78]);
                        SetTextBoxText(txt平移示教中转位置, communication.ReadLocation[79]);

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

                        SetTextBoxText(txtBYY示教上料12位置, communication.ReadLocation[97]);
                        SetTextBoxText(txtBYY示教视觉1位置, communication.ReadLocation[98]);
                        SetTextBoxText(txtBYY示教视觉2位置, communication.ReadLocation[99]);
                        SetTextBoxText(txtBYY示教第一行, communication.ReadLocation[100]);
                        SetTextBoxText(txtBYY示教第二行, communication.ReadLocation[101]);
                        SetTextBoxText(txtBYY示教视觉3位置, communication.ReadLocation[102]);
                        SetTextBoxText(txtBYY示教视觉4位置, communication.ReadLocation[103]);
                        SetTextBoxText(txtBYY示教上料34位置, communication.ReadLocation[104]);

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
                        SetTextBoxText(txt平移示教当前位置, communication.ReadLocation[5]);
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
                        SetTextBoxText(txt上料Y轴手动速度, communication.ReadPLCPmt[16]);
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

        #region 报警跳出与记录
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
        #endregion

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

        #region 示教1操作
        private void btnX示教吸1实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[30] = true;
        }

        private void btnX示教吸1实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[30] = false;
        }
               
        private void btnX示教吸2实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[32] = true;
        }

        private void btnX示教吸2实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[32] = false;
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

        private void btnX示教吸2NG盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[42] = true;
        }

        private void btnX示教吸2NG盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[42] = false;
        }

        private void btnX示教吸1倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[51] = true;
        }

        private void btnX示教吸1倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[51] = false;
        }

        private void btnX示教吸2倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[53] = true;
        }

        private void btnX示教吸2倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[53] = false;
        }

        private void btnX示教视觉实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[190] = true;
        }

        private void btnX示教视觉实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[190] = false;
        }

        private void btnX示教视觉倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[192] = true;
        }

        private void btnX示教视觉倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[192] = false;
        }

        private void btnY示教吸1实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[31] = true;
        }

        private void btnY示教吸1实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[31] = false;
        }

        private void btnY示教吸1NG盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[41] = true;
        }

        private void btnY示教吸1NG盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[41] = false;
        }

        private void btnY示教吸2实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[33] = true;
        }

        private void btnY示教吸2实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[33] = false;
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

        
        private void btnY示教吸1倒实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[52] = true;
        }

        private void btnY示教吸1倒实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[52] = false;
        }

        
        private void btnY示教吸2倒实盘第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[54] = true;
        }

        private void btnY示教吸2倒实盘第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[54] = false;
        }

        private void btnY示教视觉实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[191] = true;
        }

        private void btnY示教视觉实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[191] = false;
        }

        private void btnY示教视觉倒实盘第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[193] = true;
        }

        private void btnY示教视觉倒实盘第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[193] = false;
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
        #endregion

        #region 示教2操作
        private void btnBYX示教上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[76] = true;
        }

        private void btnBYX示教上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[76] = false;
        }

        private void btnBYX示教视觉1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[77] = true;
        }

        private void btnBYX示教视觉1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[77] = false;
        }

        private void btnBYX示教视觉2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[78] = true;
        }

        private void btnBYX示教视觉2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[78] = false;
        }

        private void btnBYX示教第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[79] = true;
        }

        private void btnBYX示教第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[79] = false;
        }

        private void btnBYX示教第二列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[80] = true;
        }

        private void btnBYX示教第二列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[80] = false;
        }

        private void btnBYX示教第三列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[81] = true;
        }

        private void btnBYX示教第三列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[81] = false;
        }

        private void btnBYX示教第四列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[82] = true;
        }

        private void btnBYX示教第四列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[82] = false;
        }

        private void btnBYX示教第五列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[83] = true;
        }

        private void btnBYX示教第五列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[83] = false;
        }

        private void btnBYX示教第六列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[84] = true;
        }

        private void btnBYX示教第六列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[84] = false;
        }

        private void btnBYX示教第七列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[85] = true;
        }

        private void btnBYX示教第七列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[85] = false;
        }

        private void btnBYX示教第八列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[86] = true;
        }

        private void btnBYX示教第八列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[86] = false;
        }

        private void btnBYY示教上料12位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[100] = true;
        }

        private void btnBYY示教上料12位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[100] = false;
        }

        private void btnBYY示教上料34位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[108] = true;
        }

        private void btnBYY示教上料34位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[108] = false;
        }

        private void btnBYY示教视觉1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[101] = true;
        }

        private void btnBYY示教视觉1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[101] = false;
        }

        private void btnBYY示教视觉2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[102] = true;
        }

        private void btnBYY示教视觉2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[102] = false;
        }

        private void btnBYY示教视觉3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[106] = true;
        }

        private void btnBYY示教视觉3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[106] = false;
        }

        private void btnBYY示教视觉4位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[107] = true;
        }

        private void btnBYY示教视觉4位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[107] = false;
        }

        private void btnBYY示教第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[103] = true;
        }

        private void btnBYY示教第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[103] = false;
        }

        private void btnBYY示教第二行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[104] = true;
        }

        private void btnBYY示教第二行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[104] = false;
        }

        private void btnBYZ示教上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[111] = true;
        }

        private void btnBYZ示教上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[111] = false;
        }

        private void btnBYZ示教上升位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[110] = true;
        }

        private void btnBYZ示教上升位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[110] = false;
        }

        private void btnBYZ示教视觉位置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[112] = true;
        }

        private void btnBYZ示教视觉位置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[112] = false;
        }

        private void btnBYZ示教视觉位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[113] = true;
        }

        private void btnBYZ示教视觉位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[113] = false;
        }

        private void btnBYZ示教视觉位置3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[115] = true;
        }

        private void btnBYZ示教视觉位置3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[115] = false;
        }

        private void btnBYZ下料视觉位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[114] = true;
        }

        private void btnBYZ下料视觉位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[114] = false;
        }

        private void btnSk1示教黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[121] = true;
        }

        private void btnSk1示教黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[121] = false;
        }

        private void btnSk1示教翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[122] = true;
        }

        private void btnSk1示教翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[122] = false;
        }

        private void btnSk1示教测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[123] = true;
        }

        private void btnSk1示教测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[123] = false;
        }

        private void btnSk1示教测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[124] = true;
        }

        private void btnSk1示教测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[124] = false;
        }

        private void btnSk1示教测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[125] = true;
        }

        private void btnSk1示教测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[125] = false;
        }

        private void btnSk2示教黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[131] = true;
        }

        private void btnSk2示教黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[131] = false;
        }

        private void btnSk2示教翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[132] = true;
        }

        private void btnSk2示教翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[132] = false;
        }

        private void btnSk2示教测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[133] = true;
        }

        private void btnSk2示教测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[133] = false;
        }

        private void btnSk2示教测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[134] = true;
        }

        private void btnSk2示教测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[134] = false;
        }

        private void btnSk2示教测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[135] = true;
        }

        private void btnSk2示教测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[135] = false;
        }

        private void btnSk3示教黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[141] = true;
        }

        private void btnSk3示教黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[141] = false;
        }

        private void btnSk3示教翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[142] = true;
        }

        private void btnSk3示教翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[142] = false;
        }

        private void btnSk3示教测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[143] = true;
        }

        private void btnSk3示教测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[143] = false;
        }

        private void btnSk3示教测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[144] = true;
        }

        private void btnSk3示教测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[144] = false;
        }

        private void btnSk3示教测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[145] = true;
        }

        private void btnSk3示教测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[145] = false;
        }

        private void btnSk4示教黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[151] = true;
        }

        private void btnSk4示教黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[151] = false;
        }

        private void btnSk4示教翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[152] = true;
        }

        private void btnSk4示教翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[152] = false;
        }

        private void btnSk4示教测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[153] = true;
        }

        private void btnSk4示教测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[153] = false;
        }

        private void btnSk4示教测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[154] = true;
        }

        private void btnSk4示教测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[154] = false;
        }

        private void btnSk4示教测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[155] = true;
        }

        private void btnSk4示教测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[155] = false;
        }

        private void btnBk1示教20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[161] = true;
        }

        private void btnBk1示教20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[161] = false;
        }

        private void btnBk1示教35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[162] = true;
        }

        private void btnBk1示教35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[162] = false;
        }

        private void btnBk2示教20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[166] = true;
        }

        private void btnBk2示教20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[166] = false;
        }

        private void btnBk2示教35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[167] = true;
        }

        private void btnBk2示教35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[167] = true;
        }

        private void btnBk3示教20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[171] = true;
        }

        private void btnBk3示教20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[171] = false;
        }

        private void btnBk3示教35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[172] = true;
        }

        private void btnBk3示教35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[172] = false;
        }

        private void btnBk4示教20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[176] = true;
        }

        private void btnBk4示教20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[176] = false;
        }

        private void btnBk4示教35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[177] = true;
        }

        private void btnBk4示教35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[177] = false;
        }
        #endregion

        #region 气缸操作
        private void btn上料机械手上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[490] = true;
        }

        private void btn上料机械手上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[490] = false;
        }

        private void btn上料机械手下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[491] = true;
        }

        private void btn上料机械手下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[491] = false;
        }

        private void btn上料机械手伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[492] = true;
        }

        private void btn上料机械手伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[492] = false;
        }

        private void btn上料机械手缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[493] = true;
        }

        private void btn上料机械手缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[493] = false;
        }

        private void btn实盘防卡盘伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[494] = true;
        }

        private void btn实盘防卡盘伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[494] = false;
        }

        private void btn实盘防卡盘缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[495] = true;
        }

        private void btn实盘防卡盘缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[495] = false;
        }

        private void btnNG盘防卡盘伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[496] = true;
        }

        private void btnNG盘防卡盘伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[496] = false;
        }

        private void btnNG盘防卡盘缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[497] = true;
        }

        private void btnNG盘防卡盘缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[497] = false;
        }

        private void btn上料吸嘴1上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[498] = true;
        }

        private void btn上料吸嘴1上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[498] = false;
        }

        private void btn上料吸嘴1下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[499] = true;
        }

        private void btn上料吸嘴1下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[499] = false;
        }

        private void btn上料吸嘴2上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[502] = true;
        }

        private void btn上料吸嘴2上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[502] = false;
        }

        private void btn上料吸嘴2下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[503] = true;
        }

        private void btn上料吸嘴2下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[503] = false;
        }

        private void btn平移吸嘴12上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[506] = true;
        }

        private void btn平移吸嘴12上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[506] = false;
        }

        private void btn平移吸嘴12下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[507] = true;
        }

        private void btn平移吸嘴12下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[507] = false;
        }

        private void btn平移吸嘴34上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[508] = true;
        }

        private void btn平移吸嘴34上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[508] = false;
        }

        private void btn平移吸嘴34下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[509] = true;
        }

        private void btn平移吸嘴34下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[509] = false;
        }

        private void btn翻转气缸0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[518] = true;
        }

        private void btn翻转气缸0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[518] = false;
        }

        private void btn翻转气缸180_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[519] = true;
        }

        private void btn翻转气缸180_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[519] = false;
        }

        private void btn夹爪1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[520] = true;
        }

        private void btn夹爪1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[520] = false;
        }

        private void btn夹爪1张开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[521] = true;
        }

        private void btn夹爪1张开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[521] = false;
        }

        private void btn夹爪1闭合1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[522] = true;
        }

        private void btn夹爪1闭合1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[522] = false;
        }

        private void btn夹爪1复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[525] = true;
        }

        private void btn夹爪1复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[525] = false;
        }

        private void btn夹爪2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[526] = true;
        }

        private void btn夹爪2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[526] = false;
        }

        private void btn夹爪2张开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[527] = true;
        }

        private void btn夹爪2张开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[527] = false;
        }

        private void btn夹爪2闭合1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[528] = true;
        }

        private void btn夹爪2闭合1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[528] = false;
        }

        private void btn夹爪2复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[531] = true;
        }

        private void btn夹爪2复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[531] = false;
        }

        private void btn除尘器1吹扫_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[532] = true;
        }

        private void btn除尘器1吹扫_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[532] = false;
        }

        private void btn除尘器1复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[533] = true;
        }

        private void btn除尘器1复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[533] = false;
        }

        private void btn除尘器2吹扫_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[534] = true;
        }

        private void btn除尘器2吹扫_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[534] = false;
        }

        private void btn除尘器2复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[525] = true;
        }

        private void btn除尘器2复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[525] = false;
        }

        private void btn上料吸嘴1真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[500] = true;
        }

        private void btn上料吸嘴1真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[500] = false;
        }

        private void btn上料吸嘴1破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[501] = true;
        }

        private void btn上料吸嘴1破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[501] = false;
        }

        private void btn上料吸嘴2真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[504] = true;
        }

        private void btn上料吸嘴2真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[504] = false;
        }

        private void btn上料吸嘴2破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[505] = true;
        }

        private void btn上料吸嘴2破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[505] = false;
        }

        private void btn平移吸嘴1真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[510] = true;
        }

        private void btn平移吸嘴1真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[510] = false;
        }

        private void btn平移吸嘴1破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[511] = true;
        }

        private void btn平移吸嘴1破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[511] = false;
        }

        private void btn平移吸嘴2真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[512] = true;
        }

        private void btn平移吸嘴2真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[512] = false;
        }

        private void btn平移吸嘴2破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[513] = true;
        }

        private void btn平移吸嘴2破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[513] = false;
        }

        private void btn平移吸嘴3真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[514] = true;
        }

        private void btn平移吸嘴3真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[514] = false;
        }

        private void btn平移吸嘴3破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[515] = true;
        }

        private void btn平移吸嘴3破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[515] = false;
        }

        private void btn平移吸嘴4真空_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[516] = true;
        }

        private void btn平移吸嘴4真空_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[516] = false;
        }

        private void btn平移吸嘴4破坏_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[517] = true;
        }

        private void btn平移吸嘴4破坏_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[518] = false;
        }

        private void btn旋转夹爪1上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[536] = true;
        }

        private void btn旋转夹爪1上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[536] = false;
        }

        private void btn旋转夹爪1下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[537] = true;
        }

        private void btn旋转夹爪1下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[537] = false;
        }

        private void btn旋转夹爪2上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[538] = true;
        }

        private void btn旋转夹爪2上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[538] = false;
        }

        private void btn旋转夹爪2下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[539] = true;
        }

        private void btn旋转夹爪2下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[539] = false;
        }

        private void btn旋转夹爪3上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[540] = true;
        }

        private void btn旋转夹爪3上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[540] = false;
        }

        private void btn旋转夹爪3下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[541] = true;
        }

        private void btn旋转夹爪3下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[541] = false;
        }

        private void btn旋转夹爪4上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[542] = true;
        }

        private void btn旋转夹爪4上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[542] = false;
        }

        private void btn旋转夹爪4下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[543] = true;
        }

        private void btn旋转夹爪4下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[543] = false;
        }

        private void btn工位1光阑伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[544] = true;
        }

        private void btn工位1光阑伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[544] = false;
        }

        private void btn工位1光阑缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[545] = true;
        }

        private void btn工位1光阑缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[545] = false;
        }

        private void btn工位1光阑上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[546] = true;
        }

        private void btn工位1光阑上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[546] = false;
        }

        private void btn工位1光阑下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[547] = true;
        }

        private void btn工位1光阑下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[547] = false;
        }

        private void btn工位1辐射板伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[548] = true;
        }

        private void btn工位1辐射板伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[548] = false;
        }

        private void btn工位1辐射板缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[549] = true;
        }

        private void btn工位1辐射板缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[549] = false;
        }

        private void btn工位1翻转0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[550] = true;
        }

        private void btn工位1翻转0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[550] = false;
        }

        private void btn工位1翻转90_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[551] = true;
        }

        private void btn工位1翻转90_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[551] = false;
        }

        private void btn工位2光阑伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[552] = true;
        }

        private void btn工位2光阑伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[552] = false;
        }

        private void btn工位2光阑缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[553] = true;
        }

        private void btn工位2光阑缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[553] = false;
        }

        private void btn工位2光阑上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[554] = true;
        }

        private void btn工位2光阑上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[554] = false;
        }

        private void btn工位2光阑下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[555] = true;
        }

        private void btn工位2光阑下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[555] = false;
        }

        private void btn工位2辐射板伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[556] = true;
        }

        private void btn工位2辐射板伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[556] = false;
        }

        private void btn工位2辐射板缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[557] = true;
        }

        private void btn工位2辐射板缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[557] = false;
        }

        private void btn工位2翻转0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[558] = true;
        }

        private void btn工位2翻转0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[558] = false;
        }

        private void brn工位2翻转90_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[559] = true;
        }

        private void brn工位2翻转90_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[559] = false;
        }
                
        private void btn工位3光阑伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[560] = true;
        }

        private void btn工位3光阑伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[560] = false;
        }

        private void btn工位3光阑缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[561] = true;
        }

        private void btn工位3光阑缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[561] = false;
        }

        private void btn工位3光阑上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[562] = true;
        }

        private void btn工位3光阑上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[562] = false;
        }

        private void btn工位3光阑下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[563] = true;
        }

        private void btn工位3光阑下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[563] = false;
        }

        private void btn工位3辐射板伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[564] = true;
        }

        private void btn工位3辐射板伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[564] = false;
        }

        private void btn工位3辐射板缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[565] = true;
        }

        private void btn工位3辐射板缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[565] = false;
        }

        private void btn工位3翻转0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[566] = true;
        }

        private void btn工位3翻转0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[566] = false;
        }

        private void btn工位3翻转90_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[567] = true;
        }

        private void btn工位3翻转90_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[567] = false;
        }

        private void btn工位4光阑伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[568] = true;
        }

        private void btn工位4光阑伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[568] = false;
        }

        private void btn工位4光阑缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[569] = true;
        }

        private void btn工位4光阑缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[569] = false;
        }

        private void btn工位4光阑上升_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[570] = true;
        }

        private void btn工位4光阑上升_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[570] = false;
        }

        private void btn工位4光阑下降_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[571] = true;
        }

        private void btn工位4光阑下降_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[571] = false;
        }

        private void btn工位4辐射板伸出_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[572] = true;
        }

        private void btn工位4辐射板伸出_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[572] = false;
        }

        private void btn工位4辐射板缩回_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[573] = true;
        }

        private void btn工位4辐射板缩回_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[573] = false;
        }

        private void btn工位4翻转0_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[574] = true;
        }

        private void btn工位4翻转0_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[575] = false;
        }

        private void btn工位4翻转90_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[575] = true;
        }

        private void btn工位4翻转90_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[575] = false;
        }

        private void btn工位1风扇上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[576] = true;
        }

        private void btn工位1风扇上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[576] = false;
        }

        private void btn工位2风扇上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[577] = true;
        }

        private void btn工位2风扇上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[577] = false;
        }

        private void btn工位3风扇上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[578] = true;
        }

        private void btn工位3风扇上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[578] = false;
        }

        private void btn工位4风扇上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[579] = true;
        }

        private void btn工位4风扇上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[579] = false;
        }

        private void btnEFU上电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[580] = true;
        }

        private void btnEFU上电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[580] = false;
        }

        private void btn工位1风扇断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[581] = true;
        }

        private void btn工位1风扇断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[581] = false;
        }

        private void btn工位2风扇断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[582] = true;
        }

        private void btn工位2风扇断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[582] = false;
        }

        private void btn工位3风扇断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[583] = true;
        }

        private void btn工位3风扇断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[583] = false;
        }

        private void btn工位4风扇断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[584] = true;
        }

        private void btn工位4风扇断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[584] = false;
        }

        private void btnEFU断电_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[585] = true;
        }

        private void btnEFU断电_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[585] = false;
        }
        #endregion

        #region 手动定位1
        private void btnX指定位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[212] = true;
        }
        
        private void btnX指定位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[212] = false;
        }

        private void btnX吸1实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[200] = true;
        }

        private void btnX吸1实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[200] = false;
        }

        private void btnX吸2实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[201] = true;
        }

        private void btnX吸2实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[201] = false;
        }

        private void btnX吸1倒实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[202] = true;
        }

        private void btnX吸1倒实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[202] = false;
        }

        private void btnX吸2倒实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[203] = true;
        }

        private void btnX吸2倒实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[203] = false;
        }

        private void btnX吸1NG盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[204] = true;
        }

        private void btnX吸1NG盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[204] = false;
        }

        private void btnX吸2NG盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[205] = true;
        }

        private void btnX吸2NG盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[205] = false;
        }

        private void btnX实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[206] = true;
        }

        private void btnX实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[206] = false;
        }

        private void btnX倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[207] = true;
        }

        private void btnX倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[207] = false;
        }

        private void btnXNG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[208] = true;
        }

        private void btnXNG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[208] = false;
        }

        private void btnX倒NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[209] = true;
        }

        private void btnX倒NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[209] = false;
        }

        private void btnX夹爪位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[210] = true;
        }

        private void btnX夹爪位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[210] = false;
        }

        private void btnX扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[211] = true;
        }

        private void btnX扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[211] = false;
        }

        private void btnX视觉实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[213] = true;
        }

        private void btnX视觉实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[213] = false;
        }

        private void btnX视觉倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[214] = true;
        }

        private void btnX视觉倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[214] = false;
        }

        private void btnY吸1实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[220] = true;
        }

        private void btnY吸1实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[220] = false;
        }

        private void btnY吸2实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[221] = true;
        }

        private void btnY吸2实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[221] = false;
        }

        private void btnY吸1倒实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[222] = true;
        }

        private void btnY吸1倒实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[222] = false;
        }

        private void btnY吸2倒实盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[223] = true;
        }

        private void btnY吸2倒实盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[223] = false;
        }

        private void btnY吸1NG盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[224] = true;
        }

        private void btnY吸1NG盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[224] = false;
        }

        private void btnY吸2NG盘_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[225] = true;
        }

        private void btnY吸2NG盘_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[225] = false;
        }

        private void btnY实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[226] = true;
        }

        private void btnY实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[226] = false;
        }

        private void btnY倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[227] = true;
        }

        private void btnY倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[227] = false;
        }

        private void btnYNG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[228] = true;
        }

        private void btnYNG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[228] = false;
        }

        private void btnY倒NG盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[229] = true;
        }

        private void btnY倒NG盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[229] = false;
        }
                
        private void btnY夹爪位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[230] = true;
        }

        private void btnY夹爪位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[230] = false;
        }

        private void btnY扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[231] = true;
        }

        private void btnY扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[231] = false;
        }

        private void btnY指定位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[232] = true;
        }

        private void btnY指定位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[232] = false;
        }

        private void btnY视觉实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[233] = true;
        }

        private void btnY视觉实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[233] = false;
        }

        private void btnY视觉倒实盘位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[234] = true;
        }

        private void btnY视觉倒实盘位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[234] = false;
        }

        private void btn实盘初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[240] = true;
        }

        private void btn实盘初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[240] = false;
        }

        private void btn实盘扫码位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[241] = true;
        }

        private void btn实盘扫码位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[241] = false;
        }

        private void btnNG盘初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[242] = true;
        }

        private void btnNG盘初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[242] = false;
        }

        private void btn倒实盘初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[243] = true;
        }

        private void btn倒实盘初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[243] = false;
        }

        private void btn倒NG盘初始位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[244] = true;
        }

        private void btn倒NG盘初始位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[244] = false;
        }

        private void btnBY平移上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[250] = true;
        }

        private void btnBY平移上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[250] = false;
        }

        private void btnBY平移下料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[251] = true;
        }

        private void btnBY平移下料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[251] = false;
        }

        private void btnBY平移中转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[252] = true;
        }

        private void btnBY平移中转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[252] = false;
        }

        private void btn旋转一90位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[351] = true;
        }

        private void btn旋转一90位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[351] = false;
        }

        private void btn旋转一180位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[352] = true;
        }

        private void btn旋转一180位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[352] = false;
        }

        private void btn旋转二90位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[355] = true;
        }

        private void btn旋转二90位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[355] = false;
        }

        private void btn旋转二180位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[356] = true;
        }

        private void btn旋转二180位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[356] = false;
        }

        private void btnBYX上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[260] = true;
        }

        private void btnBYX上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[260] = false;
        }

        private void btnBYX视觉位置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[261] = true;
        }

        private void btnBYX视觉位置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[261] = false;
        }

        private void btnBYX第一列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[262] = true;
        }

        private void btnBYX第一列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[262] = false;
        }

        private void btnBYX第二列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[263] = true;
        }

        private void btnBYX第二列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[263] = false;
        }

        private void btnBYX第三列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[264] = true;
        }

        private void btnBYX第三列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[264] = false;
        }

        private void btnBYX第四列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[265] = true;
        }

        private void btnBYX第四列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[265] = false;
        }

        private void btnBYX第五列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[266] = true;
        }

        private void btnBYX第五列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[266] = false;
        }

        private void btnBYX第六列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[267] = true;
        }

        private void btnBYX第六列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[267] = false;
        }

        private void btnBYX第七列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[268] = true;
        }

        private void btnBYX第七列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[268] = false;
        }

        private void btnBYX第八列_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[269] = true;
        }

        private void btnBYX第八列_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[269] = false;
        }

        private void btnBYX视觉位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[270] = true;
        }

        private void btnBYX视觉位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[270] = false;
        }

        private void btnBYY上料12位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[275] = true;
        }

        private void btnBYY上料12位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[275] = true;
        }

        private void btnBYY视觉位置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[276] = true;
        }

        private void btnBYY视觉位置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[276] = false;
        }

        private void btnBYY视觉位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[277] = true;
        }

        private void btnBYY视觉位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[277] = false;
        }

        private void btnBYY第一行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[278] = true;
        }

        private void btnBYY第一行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[278] = false;
        }

        private void btnBYY第二行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[279] = true;
        }

        private void btnBYY第二行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[279] = false;
        }

        private void btnBYY视觉位置3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[280] = true;
        }

        private void btnBYY视觉位置3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[280] = false;
        }

        private void btnBYY视觉位置4_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[281] = true;
        }

        private void btnBYY视觉位置4_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[281] = false;
        }

        private void btnBYY上料34位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[282] = true;
        }

        private void btnBYY上料34位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[282] = false;
        }

        private void btnBYZ上料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[285] = true;
        }

        private void btnBYZ上料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[285] = false;
        }

        private void btnBYZ上升位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[286] = true;
        }

        private void btnBYZ上升位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[286] = false;
        }

        private void btnBYZ视觉位置1_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[287] = true;
        }

        private void btnBYZ视觉位置1_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[287] = false;
        }

        private void btnBYZ视觉位置2_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[288] = true;
        }

        private void btnBYZ视觉位置2_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[288] = false;
        }

        private void btnBYZ下料位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[289] = true;
        }

        private void btnBYZ下料位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[289] = false;
        }

        private void btnBYZ视觉位置3_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[290] = true;
        }

        private void btnBYZ视觉位置3_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[290] = false;
        }
        #endregion

        #region 手动定位2
        private void btnSk1黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[295] = true;
        }

        private void btnSk1黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[295] = false;
        }

        private void btnSk1翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[296] = true;
        }

        private void btnSk1翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[296] = false;
        }

        private void btnSk1测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[297] = true;
        }

        private void btnSk1测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[297] = false;
        }

        private void btnSk1测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[298] = true;
        }

        private void btnSk1测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[298] = false;
        }

        private void btnSk1测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[299] = true;
        }

        private void btnSk1测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[298] = false;
        }

        private void btnSk2黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[305] = true;
        }

        private void btnSk2黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[305] = false;
        }

        private void btnSk2翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[306] = true;
        }

        private void btnSk2翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[306] = false;
        }

        private void btnSk2测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[307] = true;
        }

        private void btnSk2测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[307] = false;
        }

        private void btnSk2测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[308] = true;
        }

        private void btnSk2测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[308] = false;
        }

        private void btnSk2测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[309] = true;
        }

        private void btnSk2测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[309] = false;
        }

        private void btnSk3黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[315] = true;
        }

        private void btnSk3黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[315] = false;
        }

        private void btnSk3翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[316] = true;
        }

        private void btnSk3翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[316] = false;
        }

        private void btnSk3测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[317] = true;
        }

        private void btnSk3测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[317] = false;
        }

        private void btnSk3测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[318] = true;
        }

        private void btnSk3测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[318] = false;
        }

        private void btnSk3测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[319] = true;
        }

        private void btnSk3测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[319] = false;
        }

        private void btnSk4黑体位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[325] = true;
        }

        private void btnSk4黑体位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[325] = false;
        }

        private void btnSk4翻转位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[326] = true;
        }

        private void btnSk4翻转位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[326] = false;
        }

        private void btnSk4测试1位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[327] = true;
        }

        private void btnSk4测试1位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[327] = false;
        }

        private void btnSk4测试2位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[328] = true;
        }

        private void btnSk4测试2位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[328] = false;
        }

        private void btnSk4测试3位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[329] = true;
        }

        private void btnSk4测试3位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[329] = false;
        }

        private void btnBkyi20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[335] = true;
        }

        private void btnBkyi20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[335] = false;
        }

        private void btnBker20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[339] = true;
        }

        private void btnBker20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[339] = false;
        }

        private void btnBker35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[340] = true;
        }

        private void btnBker35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[340] = false;
        }

        private void btnBksan20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[343] = true;
        }

        private void btnBksan20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[343] = false;
        }

        private void btnBksan35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[344] = true;
        }

        private void btnBksan35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[344] = false;
        }

        private void btnBksi20位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[347] = true;
        }

        private void btnBksi20位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[347] = false;
        }

        private void btnBksi35位置_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[348] = true;
        }

        private void btnBksi35位置_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[348] = false;
        }

        private void btnJD1加紧_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[600] = true;
        }

        private void btnJD1加紧_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[600] = false;
        }

        private void btnJD1打开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[601] = true;
        }

        private void btnJD1打开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[601] = false;
        }

        private void btnJD2加紧_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[602] = true;
        }

        private void btnJD2加紧_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[602] = false;
        }

        private void btnJD2打开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[603] = true;
        }

        private void btnJD2打开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[603] = false;
        }

        private void btnJD3加紧_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[604] = true;
        }

        private void btnJD3加紧_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[604] = false;
        }

        private void btnJD3打开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[605] = true;
        }

        private void btnJD3打开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[605] = false;
        }

        private void btnJD4加紧_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[606] = true;
        }

        private void btnJD4加紧_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[606] = false;
        }

        private void btnJD4打开_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[607] = true;
        }

        private void btnJD4打开_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[607] = false;
        }
        #endregion

        #region 回原点
        private void btnX回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[7] = true;
        }

        private void btnX回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[7] = false;
        }

        private void btnY回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[8] = true;
        }

        private void btnY回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[8] = false;
        }

        private void btn实盘回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[9] = true;
        }

        private void btn实盘回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[9] = false;
        }

        private void btnNG盘回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[10] = true;
        }

        private void btnNG盘回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[10] = false;
        }

        private void btn倒实盘回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[11] = true;
        }

        private void btn倒实盘回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[11] = false;
        }

        private void btn倒NG盘回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[12] = true;
        }

        private void btn倒NG盘回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[12] = false;
        }

        private void btnBY平移回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[13] = true;
        }

        private void btnBY平移回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[13] = false;
        }

        private void btnBYX回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[14] = true;

        }

        private void btnBYX回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[14] = false;
        }

        private void btnBYY回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[15] = true;
        }

        private void btnBYY回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[15] = false;
        }

        private void btnBYZ回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[16] = true;
        }

        private void btnBYZ回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[16] = false;
        }

        private void btnSk1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[17] = true;
        }

        private void btnSk1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[17] = false;
        }

        private void btnSk2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[18] = true;
        }

        private void btnSk2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[18] = false;
        }

        private void btnSk3回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[19] = true;
        }

        private void btnSk3回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[19] = false;
        }

        private void btnSk4回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[20] = true;
        }

        private void btnSk4回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[20] = false;
        }

        private void btnBk1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[21] = true;
        }

        private void btnBk1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[21] = false;
        }

        private void btnBk2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[22] = true;
        }

        private void btnBk2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[22] = false;
        }

        private void btnBk3回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[23] = true;
        }

        private void btnBk3回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[23] = false;
        }

        private void btnBk4回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[24] = true;
        }

        private void btnBk4回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[24] = false;
        }

        private void 旋转一回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[25] = true;
        }

        private void 旋转一回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[25] = false;
        }

        private void 旋转二回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[26] = true;
        }

        private void 旋转二回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[26] = false;
        }

        private void btnJD1回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[608] = true;
        }

        private void btnJD1回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[608] = false;
        }

        private void btnJD2回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[609] = true;
        }

        private void btnJD2回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[609] = false;
        }

        private void btnJD3回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[610] = true;
        }

        private void btnJD3回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[610] = false;
        }

        private void btnJD4回原点_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[611] = true;
        }

        private void btnJD4回原点_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[611] = false;
        }
        #endregion

        #region 手动移动
        private void btnX停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[360] = true;
        }

        private void btnX停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[360] = false;
        }

        private void btnX左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[362] = true;
        }

        private void btnX左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[362] = false;
        }

        private void btnX右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[361] = true;
        }

        private void btnX右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[361] = false;
        }

        private void btnY停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[365] = true;
        }

        private void btnY停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[365] = false;
        }

        private void btnY前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[366] = true;
        }

        private void btnY前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[366] = false;
        }

        private void btnY后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[367] = true;
        }

        private void btnY后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[367] = false;
        }

        private void btn实盘停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[370] = true;
        }

        private void btn实盘停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[370] = false;
        }

        private void btn实盘上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[371] = true;
        }

        private void btn实盘上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[371] = false;
        }

        private void btn实盘下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[372] = true;
        }

        private void btn实盘下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[372] = false;
        }

        private void btnNG盘停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[375] = true;
        }

        private void btnNG盘停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[375] = false;
        }

        private void btnNG盘上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[376] = true;
        }

        private void btnNG盘上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[376] = false;
        }

        private void btnNG盘下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[377] = true;
        }

        private void btnNG盘下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[377] = false;
        }

        private void btn倒实盘停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[380] = true;
        }

        private void btn倒实盘停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[380] = false;
        }

        private void btn倒实盘上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[381] = true;
        }

        private void btn倒实盘上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[381] = false;
        }

        private void btn倒实盘下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[382] = true;
        }

        private void btn倒实盘下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[382] = false;
        }

        private void btn倒NG盘停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[385] = true;
        }

        private void btn倒NG盘停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[385] = false;
        }

        private void btn倒NG盘上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[386] = true;
        }

        private void btn倒NG盘上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[386] = false;
        }

        private void btn倒NG盘下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[387] = true;
        }

        private void btn倒NG盘下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[387] = false;
        }

        private void btnBY平移停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[390] = true;
        }

        private void btnBY平移停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[390] = false;
        }

        private void btnBY平移右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[391] = true;
        }

        private void btnBY平移右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[391] = false;
        }

        private void btnBY平移左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[392] = true;
        }

        private void btnBY平移左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[392] = false;
        }

        private void btnBYX停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[395] = true;
        }

        private void btnBYX停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[395] = false;
        }

        private void btnBYX右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[396] = true;
        }

        private void btnBYX右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[396] = false;
        }

        private void btnBYX左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[397] = true;
        }

        private void btnBYX左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[397] = false;
        }

        private void btnBYY停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[400] = true;
        }

        private void btnBYY停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[400] = false;
        }

        private void btnBYY前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[401] = true;
        }

        private void btnBYY前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[401] = false;
        }

        private void btnBYY后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[402] = true;
        }

        private void btnBYY后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[402] = false;
        }

        private void btnBYZ停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[405] = true;
        }

        private void btnBYZ停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[405] = false;
        }

        private void btnBYZ下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[406] = true;
        }

        private void btnBYZ下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[406] = false;
        }

        private void btnBYZ上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[407] = true;
        }

        private void btnBYZ上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[407] = false;
        }

        private void btn旋转1停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[450] = true;
        }

        private void btn旋转1停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[450] = false;
        }

        private void btn旋转1右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[451] = true;
        }

        private void btn旋转1右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[451] = false;
        }

        private void btn旋转1左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[452] = true;
        }

        private void btn旋转1左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[452] = false;
        }

        private void btn旋转2停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[455] = true;
        }

        private void btn旋转2停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[455] = false;
        }

        private void btn旋转2右行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[456] = true;
        }

        private void btn旋转2右行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[456] = false;
        }

        private void btn旋转2左行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[457] = true;
        }

        private void btn旋转2左行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[457] = false;
        }

        private void btnSk1停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[410] = true;
        }

        private void btnSk1停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[410] = false;
        }

        private void btnSk1前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[411] = true;
        }

        private void btnSk1前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[411] = false;
        }

        private void btnSk1后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[412] = true;
        }

        private void btnSk1后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[412] = false;
        }

        private void btnSk2停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[415] = true;
        }

        private void btnSk2停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[415] = false;
        }

        private void btnSk2前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[416] = true;
        }

        private void btnSk2前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[416] = false;
        }

        private void btnSk2后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[417] = true;
        }

        private void btnSk2后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[417] = false;
        }

        private void btnSk3停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[420] = true;
        }

        private void btnSk3停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[420] = false;
        }

        private void btnSk3前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[421] = true;
        }

        private void btnSk3前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[421] = false;
        }

        private void btnSk3后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[422] = true;
        }

        private void btnSk3后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[422] = false;
        }

        private void btnSk4停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[425] = true;
        }

        private void btnSk4停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[425] = false;
        }

        private void btnSk4前行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[426] = true;
        }

        private void btnSk4前行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[426] = false;
        }

        private void btnSk4后行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[427] = true;
        }

        private void btnSk4后行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[427] = false;
        }

        private void btnBk1停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[430] = true;
        }

        private void btnBk1停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[430] = false;
        }

        private void btnBk1下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[431] = true;
        }

        private void btnBk1下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[431] = false;
        }

        private void btnBk1上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[432] = true;
        }

        private void btnBk1上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[432] = false;
        }

        private void btnBk2停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[435] = true;
        }

        private void btnBk2停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[435] = false;
        }

        private void btnBk2下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[436] = true;
        }

        private void btnBk2下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[436] = false;
        }

        private void btnBk2上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[437] = true;
        }

        private void btnBk2上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[437] = false;
        }

        private void btnBk3停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[440] = true;
        }

        private void btnBk3停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[440] = false;
        }

        private void btnBk3下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[441] = true;
        }

        private void btnBk3下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[441] = false;
        }

        private void btnBk3上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[442] = true;
        }

        private void btnBk3上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[442] = false;
        }

        private void btnBk4停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[445] = true;
        }

        private void btnBk4停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[445] = false;
        }

        private void btnBk4下行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[446] = true;
        }

        private void btnBk4下行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[446] = false;
        }

        private void btnBk4上行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[447] = true;
        }

        private void btnBk4上行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[447] = false;
        }
        #endregion

        #region 写入速度
        private void btn写入速度_Click(object sender, EventArgs e)
        {
            //定位速度
            if (txt上料X轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[0] = Convert.ToDouble(txt上料X轴定位速度设置.Text);
                txt上料X轴定位速度.Text = txt上料X轴定位速度设置.Text;
                txt上料X轴定位速度设置.Text = null;
            }
            if (txt上料Y轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[1] = Convert.ToDouble(txt上料Y轴定位速度设置.Text);
                txt上料Y轴定位速度.Text = txt上料Y轴定位速度设置.Text;
                txt上料Y轴定位速度设置.Text = null;
            }
            if (txt升降轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[2] = Convert.ToDouble(txt升降轴定位速度设置.Text);
                txt升降轴定位速度.Text = txt升降轴定位速度设置.Text;
                txt升降轴定位速度设置.Text = null;
            }
            if (txt平移轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[3] = Convert.ToDouble(txt平移轴定位速度设置.Text);
                txt平移轴定位速度.Text = txt平移轴定位速度设置.Text;
                txt平移轴定位速度设置.Text = null;
            }
            if (txt中空轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[4] = Convert.ToDouble(txt中空轴定位速度设置.Text);
                txt中空轴定位速度.Text = txt中空轴定位速度设置.Text;
                txt中空轴定位速度设置.Text = null;
            }
            if (txt搬运X轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[5] = Convert.ToDouble(txt搬运X轴定位速度设置.Text);
                txt搬运X轴定位速度.Text = txt搬运X轴定位速度设置.Text;
                txt搬运X轴定位速度设置.Text = null;
            }
            if (txt搬运Y轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[6] = Convert.ToDouble(txt搬运Y轴定位速度设置.Text);
                txt搬运Y轴定位速度.Text = txt搬运Y轴定位速度设置.Text;
                txt搬运Y轴定位速度设置.Text = null;
            }
            if (txt搬运Z轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[7] = Convert.ToDouble(txt搬运Z轴定位速度设置.Text);
                txt搬运Z轴定位速度.Text = txt搬运Z轴定位速度设置.Text;
                txt搬运Z轴定位速度设置.Text = null;
            }
            if (txtSocket轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[8] = Convert.ToDouble(txtSocket轴定位速度设置.Text);
                txtSocket轴定位速度.Text = txtSocket轴定位速度设置.Text;
                txtSocket轴定位速度设置.Text = null;
            }
            if (txt黑体轴定位速度设置.Text != "")
            {
                communication.WritePLCPmt[9] = Convert.ToDouble(txt黑体轴定位速度设置.Text);
                txt黑体轴定位速度.Text = txt黑体轴定位速度设置.Text;
                txt黑体轴定位速度设置.Text = null;
            }

            //手动速度
            if (txt上料X轴手动速度设置.Text != "")
            {
                txt上料X轴手动速度.Text = txt上料X轴手动速度设置.Text;
                txt上料X轴手动速度设置.Text = null;
            }
            if (txt上料Y轴手动速度设置.Text != "")
            {
                txt上料Y轴手动速度.Text = txt上料Y轴手动速度设置.Text;
                txt上料Y轴手动速度设置.Text = null;
            }
            if (txt升降轴手动速度设置.Text != "")
            {
                txt升降轴手动速度.Text = txt升降轴手动速度设置.Text;
                txt升降轴手动速度设置.Text = null;
            }
            if (txt平移轴手动速度设置.Text != "")
            {
                txt平移轴手动速度.Text = txt平移轴手动速度设置.Text;
                txt平移轴手动速度设置.Text = null;
            }
            if (txt中空轴手动速度设置.Text != "")
            {
                txt中空轴手动速度.Text = txt中空轴手动速度设置.Text;
                txt中空轴手动速度设置.Text = null;
            }
            if (txt搬运X轴手动速度设置.Text != "")
            {
                txt搬运X轴手动速度.Text = txt搬运X轴手动速度设置.Text;
                txt搬运X轴手动速度设置.Text = null;
            }
            if (txt搬运Y轴手动速度设置.Text != "")
            {
                txt搬运Y轴手动速度.Text = txt搬运Y轴手动速度设置.Text;
                txt搬运Y轴手动速度设置.Text = null;
            }
            if (txt搬运Z轴手动速度设置.Text != "")
            {
                txt搬运Z轴手动速度.Text = txt搬运Z轴手动速度设置.Text;
                txt搬运Z轴手动速度设置.Text = null;
            }
            if (txtSocket轴手动速度设置.Text != "")
            {
                txtSocket轴手动速度.Text = txtSocket轴手动速度设置.Text;
                txtSocket轴手动速度设置.Text = null;
            }
            if (txt黑体轴手动速度设置.Text != "")
            {
                txt黑体轴手动速度.Text = txt黑体轴手动速度设置.Text;
                txt黑体轴手动速度设置.Text = null;
            }
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
        #endregion

        #region 功能设置
        private void btn门开关功能开关_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[460] = true;
        }

        private void btn门开关功能开关_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[460] = false;
        }


        #endregion

        #region 自动界面
        private void btn报警复位_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[0] = true;
        }

        private void btn报警复位_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[0] = false;
        }

        private void btn蜂鸣停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[1] = true;
        }

        private void btn蜂鸣停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[1] = false;
        }

        private void btn手动模式_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[2] = true;
        }

        private void btn手动模式_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[2] = false;
        }

        private void btn自动模式_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[3] = true;
        }

        private void btn自动模式_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[3] = false;
        }

        private void btn自动运行_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[4] = true;
        }

        private void btn自动运行_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[4] = false;
        }

        private void btn自动停止_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[5] = true;
        }

        private void btn自动停止_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[5] = false;
        }

        private void btn初始化_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[6] = true;
        }

        private void btn初始化_MouseUp(object sender, MouseEventArgs e)
        {
            communication.WritePLCIO[6] = false;
        }

        #endregion

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            form.Close();
        }
    }
}
