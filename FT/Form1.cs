using FT.Data;
using MyToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FT
{
    public partial class Form1 : Form
    {
        #region 组件
        //登录界面
        Form2 loginForm;
        //通信
        Communication communication = Communication.Singleton;
        //托盘数据
        TrayManager trayManager;
        //计时组件
        readonly Stopwatch stopwatch1 = new Stopwatch();
        readonly Stopwatch stopwatch2 = new Stopwatch();
        #endregion

        #region 加载的信息
        //加载的报警信息
        Dictionary<string, string> alarmInformation;
        //当前报警信息
        List<string> warning = new List<string>();

        //加载的TextBox地址信息
        Dictionary<string, string> textBoxInformation;
        //所有TabPage
        Dictionary<string, TabPage> mainPages;
        //主界面TextBox
        Dictionary<string, TextBox> firstPageTextBoxes;
        //示教界面TextBox
        Dictionary<string, GroupBox> calibrationGroups;
        Dictionary<string, TextBox> calibrationTextBoxes;
        //手动电机TextBox
        Dictionary<string, GroupBox> manualPageGroups;
        Dictionary<string, TextBox> manualPageTextBoxes;

        //加载的帮助文档信息
        Dictionary<string, string> helpInformation;
        //示教界面按下的按钮
        List<Button> calibrationButtons = new List<Button>();
        #endregion

        #region 其他变量
        //托盘类型
        string currentTrayType = "";
        //临时变量存储，切换型号时临时存储上一型号
        string tempTrayType = "";
        //是否更新
        bool isUpdate = false;
        //IsWrite用于写入PLC值时判断当前连接是否断开，调用写入方法时的返回布尔值赋给此变量
        //当值为false时可以弹出提示框，每次调用前需要先赋值为true
        private bool isWrite = true;
        public bool IsWrite
        {
            get { return isWrite; }
            set
            {
                if (isWrite != value)
                {
                    isWrite = value;
                    if (!isWrite)
                    {
                        MessageBox.Show($"参数写入失败。请检查连接状态。{workState}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LogManager.WriteLog($"参数写入失败。请检查连接状态。{workState}", LogType.Error);
                    }
                }
            }
        }
        //写入参数时的工作状态
        private string workState = "";
        //写入参数时设置工作状态
        private void SetMessage(string info = "")
        {
            IsWrite = true;
            workState = info;
        }
        #endregion

        public Form1(Form2 form2)
        {
            InitializeComponent();

            loginForm = form2;

            //数据初始化、打开通信端口
            try
            {
                #region 配置数据加载
                //获取界面控件
                mainPages = GetControls<TabPage>(TC_Main); 
                firstPageTextBoxes = GetControls<TextBox>(GB_Connection);
                calibrationGroups = GetControls<GroupBox>(mainPages["TP示教界面1"], mainPages["TP示教界面2"]);
                calibrationTextBoxes = GetControls<TextBox>(calibrationGroups.Values.ToArray());
                manualPageGroups = GetControls<GroupBox>(mainPages["TP手动电机1"], mainPages["TP手动电机2"]);
                manualPageTextBoxes = GetControls<TextBox>(manualPageGroups.Values.ToArray());

                //示教手动电机上下料个数统计TextBox信息读取地址加载
                textBoxInformation = JsonManager.ReadJsonString<Dictionary<string, string>>(Environment.CurrentDirectory + "\\Configuration", "TextBoxInfo");
                //报警信息读取
                alarmInformation = JsonManager.ReadJsonString<Dictionary<string, string>>(Environment.CurrentDirectory + "\\Configuration", "Alarm");
                //帮助文档信息加载
                helpInformation = JsonManager.ReadJsonString<Dictionary<string, string>>(Environment.CurrentDirectory + "\\Configuration", "HelpInfo");
                InitializeHelpMenu();
                #endregion

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
                //下拉列表托盘类型加载
                SetCB_TypeOfTray(trayManager);
                //加载上次的托盘状态
                trayManager.LoadTraysData(currentTrayType);
                //更新托盘状态到界面
                trayManager.UpdateTrayLabels(PN_Trays);
                #endregion

                #region 打开数据通信端口
                communication.Compolet.Open();
                isUpdate = true;
                UpdateData();
                UpdateInterface();
                UpdateAlarm();
                #endregion
            }
            catch (Exception e)
            {
                isUpdate = false;
                MessageBox.Show(e.Message, "程序初始化", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogManager.WriteLog("程序初始化：" + e.Message, LogType.Error);
            }
        }

        #region 方法
        //界面IO颜色更新方法
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

        public Dictionary<string, T> GetControls<T>(params Control[] mainControl)
        {
            Dictionary<string, T> controlDic = new Dictionary<string, T>();
            if (mainControl.Length == 0)
            {
                foreach (Control item in Controls)
                {
                    if (item is T control)
                        controlDic.Add(item.Name, control);
                }
            }
            else
            {
                foreach (Control item in mainControl)
                {
                    foreach (Control child in item.Controls)
                    {
                        if (child is T control)
                            controlDic.Add(child.Name, control);
                    }
                }
            }
            return controlDic;
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
                                string rowstr = m_DataView.Rows[i].Cells[j].Value.ToString();
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
        //更新界面传感器类型下拉列表
        public void SetCB_TypeOfTray(TrayManager trayManager)
        {
            if (trayManager == null) return;
            CB_TypeOfTray.Items.Clear();
            foreach (var trayType in trayManager.TrayType)
                CB_TypeOfTray.Items.Add(trayType.Key);
        }
        //使用次数检测
        public void CheckCount(double count, double times, ref bool isShow, string message = "上料吸嘴1使用次数已达上限，请及时更换")
        {
            double Vac1Count = count - times;
            double Vac1Mod = Vac1Count % 100;
            if (Vac1Count < 0)
            {
                isShow = true;
            }
            else
            {
                if (Vac1Count != 0 && Vac1Mod != 0)
                {
                    isShow = true;
                }
            }

            if (isShow)
            {
                if (Vac1Count >= 0)
                {
                    if (Vac1Count == 0 || Vac1Mod == 0)
                    {
                        DialogResult result = MessageBox.Show(message, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.Yes) isShow = false;
                    }
                }
            }
        }
        //在TextBox中显示警报信息并记录
        public void RecordAndShow(string message, LogType logType, TextBox textBox = null, bool isShowUser = true)
        {
            if (isShowUser)
                message = $"{loginForm.CurrentUser}  {message}";
            LogManager.WriteLog(message, logType);
            textBox?.Invoke(new Action(() =>
            textBox.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}")));
        }
        //检测报警列表
        public void CheckWarning(Hashtable waringTable)
        {
            for (int i = 0; i < waringTable.Count; i++)
            {
                string key = $"PlcOutAlarm[{i}]";
                if (!alarmInformation.ContainsKey(i.ToString())) continue;
                if ((bool)waringTable[key])
                {
                    //如果当前报警列表不包含检测到的字符串
                    if (!warning.Contains(alarmInformation[i.ToString()]))
                    {
                        warning.Add(alarmInformation[i.ToString()]);
                        //显示到控件上
                        RecordAndShow(alarmInformation[i.ToString()], LogType.Warning, TB_Warning, false);
                    }
                }
                else
                {
                    if (warning.Contains(alarmInformation[i.ToString()]))
                    {
                        warning.Remove(alarmInformation[i.ToString()]);
                        TB_Warning.Invoke(new Action(() => TB_Warning.Clear()));
                        //更新显示内容
                        foreach (var item in warning)
                            TB_Warning.Invoke(new Action(() => TB_Warning.AppendText(item + Environment.NewLine)));
                    }
                }
            }
        }

        private void InitializeHelpMenu()
        {
            if (helpInformation == null) return;
            foreach (var item in helpInformation)
            {
                if (item.Key == "BrowserPath") continue;
                ToolStripMenuItem menu = new ToolStripMenuItem()
                {
                    Name = $"TSM{item.Key}",
                    Tag = item.Key,
                    Text = item.Key
                };
                menu.Click += TSM打开文档_Click;
                TSM帮助.DropDownItems.Add(menu);
            }
        }

        #endregion

        #region 更新
        public void UpdateData()
        {
            Task.Run(() =>
            {
                while (isUpdate)
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
                            if (currentTrayType != "" && currentTrayType != " ")
                            {
                                //初始化
                                trayManager.InitializeTrays(currentTrayType);
                                trayManager.UpdateTrayLabels(PN_Trays);
                                communication.WriteVariable(false, "PLC标志位[2]");
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
                            communication.WriteVariable(false, "PLC标志位[0]");
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
                        label93.Invoke(new Action(() => label93.Text = $"{stopwatch1.ElapsedMilliseconds}ms"));
                    }
                    catch (Exception e)
                    {
                        LogManager.WriteLog($"数据更新出错。{e.Message}", LogType.Error);
                    }
                }
            });
        }

        public void UpdateInterface()
        {
            Task.Run(() =>
            {
                #region 是否提示更换次数
                bool isShow = true;
                bool isShow1 = true;
                bool isShow2 = true;
                bool isShow3 = true;
                bool isShow4 = true;
                bool isShow5 = true;
                bool isShow6 = true;
                bool isShow7 = true;
                bool isShow8 = true;
                bool isShow9 = true;
                bool isShow10 = true;
                bool isShow11 = true;
                bool isShow12 = true;
                bool isShow13 = true;
                #endregion
                while (isUpdate)
                {
                    try
                    {
                        stopwatch2.Restart();
                        Thread.Sleep(150);
                        #region 界面显示更新数据

                        #region 示教界面和主界面手动界面部分TextBox更新
                        foreach (var item in textBoxInformation)
                        {
                            if (firstPageTextBoxes.ContainsKey(item.Key))
                                SetTextBoxText(firstPageTextBoxes[item.Key], communication.Location[item.Value]);
                            if (manualPageTextBoxes.ContainsKey(item.Key))
                                SetTextBoxText(manualPageTextBoxes[item.Key], communication.Location[item.Value]);
                            if (calibrationTextBoxes.ContainsKey(item.Key))
                                SetTextBoxText(calibrationTextBoxes[item.Key], communication.Location[item.Value]);
                        }
                        #endregion

                        #region Lable显示本机当前状态
                        if (communication.ReadPLCIO[157])
                        {
                            LB_手动状态.Invoke(new Action(() => LB_手动状态.Text = "手动模式"));
                        }

                        if (communication.ReadPLCIO[158])
                        {
                            LB_手动状态.Invoke(new Action(() => LB_手动状态.Text = "自动模式"));
                        }

                        if (communication.ReadPLCIO[159])
                        {
                            LB_自动运行状态.Invoke(new Action(() => LB_自动运行状态.Text = "运行中"));
                        }
                        else
                        {
                            LB_自动运行状态.Invoke(new Action(() => LB_自动运行状态.Text = "停止中"));
                        }

                        if (communication.ReadPLCIO[156])
                        {
                            LB_初始化状态.Invoke(new Action(() => LB_初始化状态.Text = "初始化完成"));
                        }
                        else
                        {
                            LB_初始化状态.Invoke(new Action(() => LB_初始化状态.Text = "初始化未完成"));
                        }

                        if (communication.ReadPLCIO[155])
                        {
                            LB_Connection.Invoke(new Action(() => LB_Connection.Text = "本机已连接"));
                        }
                        else
                        {
                            LB_Connection.Invoke(new Action(() => LB_Connection.Text = "本机未连接"));
                        }

                        SetLabelColor(communication.ReadPLCIO[155], LB_Connection);
                        SetLabelColor(communication.ReadPLCIO[156], LB_初始化状态);
                        SetLabelColor(communication.ReadPLCIO[158], LB_手动状态);
                        SetLabelColor(communication.ReadPLCIO[159], LB_自动运行状态);
                        #endregion

                        #region IO信息界面
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
                        SetLabelColor(communication.ReadPLCIO[112], X112);
                        SetLabelColor(communication.ReadPLCIO[113], X113);
                        SetLabelColor(communication.ReadPLCIO[114], X114);
                        SetLabelColor(communication.ReadPLCIO[115], X115);
                        SetLabelColor(communication.ReadPLCIO[116], X116);
                        SetLabelColor(communication.ReadPLCIO[117], X117);
                        SetLabelColor(communication.ReadPLCIO[118], X118);
                        SetLabelColor(communication.ReadPLCIO[119], X119);
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
                        SetLabelColor(communication.ReadPLCIO[195], 夹爪2状态);
                        SetLabelColor(communication.ReadPLCIO[196], 夹爪3状态);
                        SetLabelColor(communication.ReadPLCIO[197], 夹爪4状态);
                        #endregion

                        #region 气缸信号显示
                        SetLabelColor(communication.ReadPLCIO[16], LB上料机械手上升);
                        SetLabelColor(communication.ReadPLCIO[17], LB上料机械手下降);
                        SetLabelColor(communication.ReadPLCIO[18], LB上料机械手夹紧);
                        SetLabelColor(communication.ReadPLCIO[19], LB上料机械手张开);
                        SetLabelColor(communication.ReadPLCIO[32], LB上料吸嘴1上升);
                        SetLabelColor(communication.ReadPLCIO[33], LB上料吸嘴1下降);
                        SetLabelColor(communication.ReadPLCIO[35], LB上料吸嘴2上升);
                        SetLabelColor(communication.ReadPLCIO[36], LB上料吸嘴2下降);
                        SetLabelColor(communication.ReadPLCIO[40], LB平移吸嘴12上升);
                        SetLabelColor(communication.ReadPLCIO[41], LB平移吸嘴12下降);
                        SetLabelColor(communication.ReadPLCIO[42], LB平移吸嘴34上升);
                        SetLabelColor(communication.ReadPLCIO[43], LB平移吸嘴34下降);
                        SetLabelColor(communication.ReadPLCIO[48], LB翻转气缸0);
                        SetLabelColor(communication.ReadPLCIO[49], LB翻转气缸180);
                        SetLabelColor(communication.ReadPLCIO[128], LB实盘卡盘伸出);
                        SetLabelColor(communication.ReadPLCIO[130], LB实盘卡盘缩回);
                        SetLabelColor(communication.ReadPLCIO[134], LBNG卡盘伸出);
                        SetLabelColor(communication.ReadPLCIO[132], LBNG卡盘缩回);
                        SetLabelColor(communication.ReadPLCIO[34], LB上料吸嘴1真空);
                        SetLabelColor(communication.ReadPLCIO[37], LB上料吸嘴2真空);
                        SetLabelColor(communication.ReadPLCIO[44], LB平移吸嘴1真空);
                        SetLabelColor(communication.ReadPLCIO[45], LB平移吸嘴2真空);
                        SetLabelColor(communication.ReadPLCIO[46], LB平移吸嘴3真空);
                        SetLabelColor(communication.ReadPLCIO[47], LB平移吸嘴4真空);
                        SetLabelColor(communication.ReadPLCIO[238], LBEFU上电);
                        SetLabelColor(communication.ReadPLCIO[50], LB夹爪1回原点);
                        SetLabelColor(communication.ReadPLCIO[57], LB夹爪2回原点);
                        SetLabelColor(communication.ReadPLCIO[52], LB夹爪1闭合1);
                        SetLabelColor(communication.ReadPLCIO[59], LB夹爪2闭合1);
                        SetLabelColor(communication.ReadPLCIO[194], LB除尘器1吹扫);
                        SetLabelColor(communication.ReadPLCIO[64], LB旋转夹爪1上升);
                        SetLabelColor(communication.ReadPLCIO[66], LB旋转夹爪2上升);
                        SetLabelColor(communication.ReadPLCIO[68], LB旋转夹爪3上升);
                        SetLabelColor(communication.ReadPLCIO[70], LB旋转夹爪4上升);
                        SetLabelColor(communication.ReadPLCIO[65], LB旋转夹爪1下降);
                        SetLabelColor(communication.ReadPLCIO[67], LB旋转夹爪2下降);
                        SetLabelColor(communication.ReadPLCIO[69], LB旋转夹爪3下降);
                        SetLabelColor(communication.ReadPLCIO[71], LB旋转夹爪4下降);
                        SetLabelColor(communication.ReadPLCIO[72], LB工位1光阑伸出);
                        SetLabelColor(communication.ReadPLCIO[80], LB工位2光阑伸出);
                        SetLabelColor(communication.ReadPLCIO[88], LB工位3光阑伸出);
                        SetLabelColor(communication.ReadPLCIO[96], LB工位4光阑伸出);
                        SetLabelColor(communication.ReadPLCIO[73], LB工位1光阑缩回);
                        SetLabelColor(communication.ReadPLCIO[81], LB工位2光阑缩回);
                        SetLabelColor(communication.ReadPLCIO[89], LB工位3光阑缩回);
                        SetLabelColor(communication.ReadPLCIO[97], LB工位4光阑缩回);
                        SetLabelColor(communication.ReadPLCIO[74], LB工位1光阑上升);
                        SetLabelColor(communication.ReadPLCIO[82], LB工位2光阑上升);
                        SetLabelColor(communication.ReadPLCIO[90], LB工位3光阑上升);
                        SetLabelColor(communication.ReadPLCIO[98], LB工位4光阑上升);
                        SetLabelColor(communication.ReadPLCIO[104], LB工位1光阑右上升);
                        SetLabelColor(communication.ReadPLCIO[106], LB工位2光阑右上升);
                        SetLabelColor(communication.ReadPLCIO[108], LB工位3光阑右上升);
                        SetLabelColor(communication.ReadPLCIO[110], LB工位4光阑右上升);
                        SetLabelColor(communication.ReadPLCIO[75], LB工位1光阑下降);
                        SetLabelColor(communication.ReadPLCIO[83], LB工位2光阑下降);
                        SetLabelColor(communication.ReadPLCIO[91], LB工位3光阑下降);
                        SetLabelColor(communication.ReadPLCIO[99], LB工位4光阑下降);
                        SetLabelColor(communication.ReadPLCIO[105], LB工位1光阑右下降);
                        SetLabelColor(communication.ReadPLCIO[107], LB工位2光阑右下降);
                        SetLabelColor(communication.ReadPLCIO[109], LB工位3光阑右下降);
                        SetLabelColor(communication.ReadPLCIO[111], LB工位4光阑右下降);
                        SetLabelColor(communication.ReadPLCIO[76], LB工位1辐射板上升);
                        SetLabelColor(communication.ReadPLCIO[84], LB工位2辐射板上升);
                        SetLabelColor(communication.ReadPLCIO[92], LB工位3辐射板上升);
                        SetLabelColor(communication.ReadPLCIO[100], LB工位4辐射板上升);
                        SetLabelColor(communication.ReadPLCIO[77], LB工位1辐射板下降);
                        SetLabelColor(communication.ReadPLCIO[85], LB工位2辐射板下降);
                        SetLabelColor(communication.ReadPLCIO[93], LB工位3辐射板下降);
                        SetLabelColor(communication.ReadPLCIO[101], LB工位4辐射板下降);
                        SetLabelColor(communication.ReadPLCIO[78], LB工位1翻转0);
                        SetLabelColor(communication.ReadPLCIO[86], LB工位2翻转0);
                        SetLabelColor(communication.ReadPLCIO[94], LB工位3翻转0);
                        SetLabelColor(communication.ReadPLCIO[102], LB工位4翻转0);
                        SetLabelColor(communication.ReadPLCIO[79], LB工位1翻转90);
                        SetLabelColor(communication.ReadPLCIO[87], LB工位2翻转90);
                        SetLabelColor(communication.ReadPLCIO[95], LB工位3翻转90);
                        SetLabelColor(communication.ReadPLCIO[103], LB工位4翻转90);
                        SetLabelColor(communication.ReadPLCIO[112], LB工位1光阑伸出右);
                        SetLabelColor(communication.ReadPLCIO[114], LB工位2光阑伸出右);
                        SetLabelColor(communication.ReadPLCIO[116], LB工位3光阑伸出右);
                        SetLabelColor(communication.ReadPLCIO[118], LB工位4光阑伸出右);
                        SetLabelColor(communication.ReadPLCIO[113], LB工位1光阑缩回右);
                        SetLabelColor(communication.ReadPLCIO[115], LB工位2光阑缩回右);
                        SetLabelColor(communication.ReadPLCIO[117], LB工位3光阑缩回右);
                        SetLabelColor(communication.ReadPLCIO[119], LB工位4光阑缩回右);
                        #endregion

                        #region 参数设置界面
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
                        #endregion

                        #region 气缸界面使用次数检测
                        SetTextBoxText(txt夹具1次数, communication.ReadPLCPmt[31]);
                        CheckCount(communication.ReadPLCPmt[31], 10000, ref isShow1, "工装1使用次数已达上限，请及时更换");
                        SetTextBoxText(txt夹具2次数, communication.ReadPLCPmt[32]);
                        CheckCount(communication.ReadPLCPmt[32], 10000, ref isShow2, "工装2使用次数已达上限，请及时更换");
                        SetTextBoxText(txt夹具3次数, communication.ReadPLCPmt[33]);
                        CheckCount(communication.ReadPLCPmt[33], 10000, ref isShow3, "工装3使用次数已达上限，请及时更换");
                        SetTextBoxText(txt夹具4次数, communication.ReadPLCPmt[34]);
                        CheckCount(communication.ReadPLCPmt[34], 10000, ref isShow4, "工装4使用次数已达上限，请及时更换");
                        SetTextBoxText(txt上料吸嘴1次数, communication.ReadPLCPmt[111]);
                        CheckCount(communication.ReadPLCPmt[111], 10000, ref isShow,"上料吸嘴1使用次数已达上限，请及时更换");
                        SetTextBoxText(txt上料吸嘴2次数, communication.ReadPLCPmt[112]);
                        CheckCount(communication.ReadPLCPmt[112], 10000, ref isShow5, "上料吸嘴2使用次数已达上限，请及时更换");
                        SetTextBoxText(txt平移吸嘴1次数, communication.ReadPLCPmt[113]);
                        CheckCount(communication.ReadPLCPmt[113], 10000, ref isShow6, "平移吸嘴1使用次数已达上限，请及时更换");
                        SetTextBoxText(txt平移吸嘴2次数, communication.ReadPLCPmt[114]);
                        CheckCount(communication.ReadPLCPmt[114], 10000, ref isShow7, "平移吸嘴2使用次数已达上限，请及时更换");
                        SetTextBoxText(txt平移吸嘴3次数, communication.ReadPLCPmt[115]);
                        CheckCount(communication.ReadPLCPmt[115], 10000, ref isShow8, "平移吸嘴3使用次数已达上限，请及时更换");
                        SetTextBoxText(txt平移吸嘴4次数, communication.ReadPLCPmt[116]);
                        CheckCount(communication.ReadPLCPmt[116], 10000, ref isShow9, "平移吸嘴4使用次数已达上限，请及时更换");
                        SetTextBoxText(txt测试夹爪1次数, communication.ReadPLCPmt[117]);
                        CheckCount(communication.ReadPLCPmt[117], 10000, ref isShow10, "测试夹爪1使用次数已达上限，请及时更换胶带");
                        SetTextBoxText(txt测试夹爪2次数, communication.ReadPLCPmt[118]);
                        CheckCount(communication.ReadPLCPmt[118], 10000, ref isShow11, "测试夹爪2使用次数已达上限，请及时更换胶带");
                        SetTextBoxText(txt测试夹爪3次数, communication.ReadPLCPmt[119]);
                        CheckCount(communication.ReadPLCPmt[119], 10000, ref isShow12, "测试夹爪3使用次数已达上限，请及时更换胶带");
                        SetTextBoxText(txt测试夹爪4次数, communication.ReadPLCPmt[120]);
                        CheckCount(communication.ReadPLCPmt[120], 10000, ref isShow13, "测试夹爪4使用次数已达上限，请及时更换胶带");
                        #endregion

                        #region 手动电机界面信息
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
                        SetTextBoxText(txt下视觉2偏移X, communication.ReadTestInformation[42]);
                        SetTextBoxText(txt下视觉2偏移Y, communication.ReadTestInformation[43]);
                        SetTextBoxText(txt下视觉2偏移θ, communication.ReadTestInformation[44]);
                        SetTextBoxText(txtR1当前字节, communication.ReadTestInformation[45]);
                        SetTextBoxText(txtR2当前字节, communication.ReadTestInformation[46]);
                        SetTextBoxText(txtR3当前字节, communication.ReadTestInformation[47]);
                        SetTextBoxText(txtR4当前字节, communication.ReadTestInformation[48]);
                        SetTextBoxText(txt上视觉1偏移X, communication.ReadTestInformation[49]);
                        SetTextBoxText(txt上视觉1偏移Y, communication.ReadTestInformation[50]);
                        SetTextBoxText(txt上视觉1偏移θ, communication.ReadTestInformation[51]);
                        #endregion

                        #region IO信息界面复位信息读取
                        SetLabelColor(communication.ReadPLCIO[50], LB_FW_增广1);
                        SetLabelColor(communication.ReadPLCIO[57], LB_FW_增广2);
                        SetLabelColor(communication.ReadPLCIO[16], LB_FW_夹爪气缸上);
                        SetLabelColor(communication.ReadPLCIO[19], LB_FW_夹爪气缸张);
                        SetLabelColor(communication.ReadPLCIO[32], LB_FW_吸嘴1上);
                        SetLabelColor(communication.ReadPLCIO[35], LB_FW_吸嘴2上);
                        SetLabelColor(communication.ReadPLCIO[41], LB_FW_吸嘴12下);
                        SetLabelColor(communication.ReadPLCIO[43], LB_FW_吸嘴34下);
                        SetLabelColor(communication.ReadPLCIO[48], LB_FW_翻转0度);
                        SetLabelColor(communication.ReadPLCIO[64], LB_FW_钧舵1上);
                        SetLabelColor(communication.ReadPLCIO[66], LB_FW_钧舵2上);
                        SetLabelColor(communication.ReadPLCIO[68], LB_FW_钧舵3上);
                        SetLabelColor(communication.ReadPLCIO[70], LB_FW_钧舵4上);
                        SetLabelColor(communication.ReadPLCIO[104], LB_FW_工位1光阑右上);
                        SetLabelColor(communication.ReadPLCIO[73], LB_FW_工位1光阑缩回);
                        SetLabelColor(communication.ReadPLCIO[74], LB_FW_工位1光阑左上);
                        SetLabelColor(communication.ReadPLCIO[77], LB_FW_工位1辐射板下);
                        SetLabelColor(communication.ReadPLCIO[78], LB_FW_工位1零度);
                        SetLabelColor(communication.ReadPLCIO[106], LB_FW_工位2光阑右上);
                        SetLabelColor(communication.ReadPLCIO[81], LB_FW_工位2光阑缩回);
                        SetLabelColor(communication.ReadPLCIO[82], LB_FW_工位2光阑左上);
                        SetLabelColor(communication.ReadPLCIO[85], LB_FW_工位2辐射板下);
                        SetLabelColor(communication.ReadPLCIO[86], LB_FW_工位2零度);
                        SetLabelColor(communication.ReadPLCIO[108], LB_FW_工位3光阑右上);
                        SetLabelColor(communication.ReadPLCIO[89], LB_FW_工位3光阑缩回);
                        SetLabelColor(communication.ReadPLCIO[90], LB_FW_工位3光阑左上);
                        SetLabelColor(communication.ReadPLCIO[93], LB_FW_工位3辐射板下);
                        SetLabelColor(communication.ReadPLCIO[94], LB_FW_工位3零度);
                        SetLabelColor(communication.ReadPLCIO[110], LB_FW_工位4光阑右上);
                        SetLabelColor(communication.ReadPLCIO[97], LB_FW_工位4光阑缩回);
                        SetLabelColor(communication.ReadPLCIO[98], LB_FW_工位4光阑左上);
                        SetLabelColor(communication.ReadPLCIO[101], LB_FW_工位4辐射板下);
                        SetLabelColor(communication.ReadPLCIO[102], LB_FW_工位4零度);
                        #endregion

                        #endregion
                        stopwatch2.Stop();
                        label95.Invoke(new Action(() => label95.Text = $"{stopwatch2.ElapsedMilliseconds}ms"));                                  
                    }
                    catch (Exception e)
                    {
                        LogManager.WriteLog($"界面数据更新：{e.Message}", LogType.Error);
                    }
                }
            });
        }

        public void UpdateAlarm()
        {
            Task.Run(() =>
            {
                while (isUpdate)
                {
                    try
                    {
                        Thread.Sleep(300);
                        //方法二 读哈希表
                        for (int i = 0; i < communication.Alarm.Count; i++)
                        {
                            string key = $"PlcOutAlarm[{i}]";
                            if (!alarmInformation.ContainsKey(i.ToString())) continue;
                            if ((bool)communication.Alarm[key])
                            {
                                //如果当前报警列表不包含检测到的字符串
                                if (!warning.Contains(alarmInformation[i.ToString()]))
                                {
                                    TB_Warning.Invoke(new Action(() => TB_Warning.Clear()));
                                    warning.Add(alarmInformation[i.ToString()]);
                                    //显示到控件上
                                    foreach (var item in warning)
                                        TB_Warning.Invoke(new Action(() => TB_Warning.AppendText(item + Environment.NewLine)));
                                    LogManager.WriteLog(alarmInformation[i.ToString()], LogType.Warning);
                                }
                            }
                            else
                            {
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

                        //方法三
                        //CheckWarning(communication.Alarm);
                    }
                    catch (Exception e)
                    {
                        LogManager.WriteLog("报警监测循环：" + e.Message, LogType.Warning);
                    }
                }
            });
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

        #region 写入速度
        private void WriteSpeed(TextBox speed, TextBox currentSpeed, string address = "PLCInPmt[0]", string message = "")
        {
            if (speed.Text != "")
            {
                communication.WriteVariable(Convert.ToDouble(speed.Text), address);
                RecordAndShow($"{message}：{currentSpeed.Text}更改为{speed.Text}mm/s", LogType.Modification, TB_Modification);
                currentSpeed.Text = speed.Text;
                speed.Text = null;
            }
        }

        private void BTN写入速度_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否写入对应速度？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                //定位速度
                WriteSpeed(txt上料X轴定位速度设置, txt上料X轴定位速度, "PLCInPmt[0]", "上料X轴定位速度设置");
                WriteSpeed(txt上料Y轴定位速度设置, txt上料Y轴定位速度, "PLCInPmt[1]", "上料Y轴定位速度设置");
                WriteSpeed(txt升降轴定位速度设置, txt升降轴定位速度, "PLCInPmt[2]", "升降轴定位速度设置");
                WriteSpeed(txt平移轴定位速度设置, txt平移轴定位速度, "PLCInPmt[3]", "平移轴定位速度设置");
                WriteSpeed(txt中空轴定位速度设置, txt中空轴定位速度, "PLCInPmt[4]", "中空轴定位速度设置");
                WriteSpeed(txt搬运X轴定位速度设置, txt搬运X轴定位速度, "PLCInPmt[5]", "搬运X轴定位速度设置");
                WriteSpeed(txt搬运Y轴定位速度设置, txt搬运Y轴定位速度, "PLCInPmt[6]", "搬运Y轴定位速度设置");
                WriteSpeed(txt搬运Z轴定位速度设置, txt搬运Z轴定位速度, "PLCInPmt[7]", "搬运Z轴定位速度设置");
                WriteSpeed(txtSocket轴定位速度设置, txtSocket轴定位速度, "PLCInPmt[8]", "Socket轴定位速度设置");
                WriteSpeed(txt黑体轴定位速度设置, txt黑体轴定位速度, "PLCInPmt[9]", "黑体轴定位速度设置");
                WriteSpeed(txt热辐射轴定位速度设置, txt热辐射轴定位速度, "PLCInPmt[11]", "热辐射轴定位速度设置");

                //手动速度
                WriteSpeed(txt上料X轴手动速度设置, txt上料X轴手动速度, "PLCInPmt[15]", "上料X轴手动速度设置");
                WriteSpeed(txt上料Y轴手动速度设置, txt上料Y轴手动速度, "PLCInPmt[16]", "上料Y轴手动速度设置");
                WriteSpeed(txt升降轴手动速度设置, txt升降轴手动速度, "PLCInPmt[17]", "升降轴手动速度设置");
                WriteSpeed(txt平移轴手动速度设置, txt平移轴手动速度, "PLCInPmt[18]", "平移轴手动速度设置");
                WriteSpeed(txt中空轴手动速度设置, txt中空轴手动速度, "PLCInPmt[19]", "中空轴手动速度设置");
                WriteSpeed(txt搬运X轴手动速度设置, txt搬运X轴手动速度, "PLCInPmt[20]", "搬运X轴手动速度设置");
                WriteSpeed(txt搬运Y轴手动速度设置, txt搬运Y轴手动速度, "PLCInPmt[21]", "搬运Y轴手动速度设置");
                WriteSpeed(txt搬运Z轴手动速度设置, txt搬运Z轴手动速度, "PLCInPmt[22]", "搬运Z轴手动速度设置");
                WriteSpeed(txtSocket轴手动速度设置, txtSocket轴手动速度, "PLCInPmt[23]", "Socket轴手动速度设置");
                WriteSpeed(txt黑体轴手动速度设置, txt黑体轴手动速度, "PLCInPmt[24]", "黑体轴手动速度设置");
                WriteSpeed(txt热辐射轴手动速度设置, txt热辐射轴手动速度, "PLCInPmt[25]", "热辐射轴手动速度设置");
            }
        }
        //只能输入数字
        private void TB速度设置_KeyPress(object sender, KeyPressEventArgs e)
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
                    if (double.Parse(textBox.Text) > 90 || double.Parse(textBox.Text) < -90)
                    {
                        textBox.Text = "90";
                        MessageBox.Show("输入0-90的值", "提示");
                    }
        }
        #endregion

        #region 功能设置
        bool Model_Change = false;//用于切换打开和关闭引脚检测功能的变量，默认关闭引脚检测功能
        private void btn引脚检测功能_Click(object sender, EventArgs e)
        {
            //打开与关闭引脚检测功能的按钮
            if(Model_Change == true) //也可以简化为if(Model_Change)
            {
                communication.WriteVariable(false, "PlcInIO[469]");
                btn引脚检测功能.Text = "关闭";
                btn引脚检测功能.BackColor = Color.Transparent;
                Model_Change = false;
                RecordAndShow($"引脚检测打开", LogType.Modification, TB_Modification);
            }
            else
            {
                communication.WriteVariable(true, "PlcInIO[469]");
                btn引脚检测功能.Text = "打开";
                btn引脚检测功能.BackColor = Color.LightGreen;
                Model_Change = true;
                RecordAndShow($"引脚检测关闭", LogType.Modification, TB_Modification);
            }
        }

        private void BTN打开_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            communication.WriteVariable(true, (string)button.Tag);
        }
        private void BTN打开_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            RecordAndShow($"{button.Name.Substring(3)}", LogType.Modification, TB_Modification);
            communication.WriteVariable(false, (string)button.Tag);
        }

        private void BTN关闭_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            communication.WriteVariable(true, (string)button.Tag);
        }
        private void BTN关闭_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            RecordAndShow($"{button.Name.Substring(3)}", LogType.Modification, TB_Modification);
            communication.WriteVariable(false, (string)button.Tag);
        }

        private void BTN提示报警跳过_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            DialogResult result = MessageBox.Show($"{button.Text}？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"{button.Text}", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, (string)button.Tag);
            }
        }
        #endregion

        #region 主界面按钮
        private void btn手动模式_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否切换手动模式？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"切换为手动模式", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[2]");
            }
        }

        private void btn自动模式_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否切换自动模式？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"切换为自动模式", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[28]");
            }
        }

        private void btn自动运行_Click(object sender, EventArgs e)
        {
            if (CB_TypeOfTray.Text == "" || CB_Socket类.Text == "" || CB_工位.Text == "" || CB_工作盘数.Text == "")
            {
                MessageBox.Show("类别未选择完成，不能启动", "提示");
                return;
            }

            DialogResult result = MessageBox.Show("是否开始自动运行？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"自动运行开始", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[4]");
            }
        }

        private void btn自动停止_Click(object sender, EventArgs e)
        {
            RecordAndShow($"自动运行停止", LogType.Modification, TB_Modification);
            communication.WriteVariable(true, "PlcInIO[5]");
        }

        private void btn初始化_Click(object sender, EventArgs e)
        {
            if (CB_TypeOfTray.Text == "" || CB_Socket类.Text == "" || CB_工位.Text == "" || CB_工作盘数.Text == "")
            {
                MessageBox.Show("类别未选择完成，不能启动", "提示");
                return;
            }

            DialogResult result = MessageBox.Show("是否初始化？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"初始化", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[6]");
                InitialForm initialForm = new InitialForm();
                initialForm.ShowDialog();
            }
        }

        #region 权限及密码
        private void BTN_SwitchUser_Click(object sender, EventArgs e)
        {
            loginForm.Show();
            this.Hide();
        }

        private void BTN_Modify_Click(object sender, EventArgs e)
        {
            if (loginForm.CB_UserName.Text == "操作员" || loginForm.CB_UserName.Text == "管理员")
            {
                MessageBox.Show("未授权用户组", "修改密码");
                TB_Password.Text = "";
                TB_NewPassword.Text = "";
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
                TB_Password.Text = "";
                TB_NewPassword.Text = "";
                return;
            }
            JsonManager.SaveJsonString(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\FTData", "engineerData",
                new UserData() { UserType = 1, UserName = "工程师", Password = TB_Password.Text });
            MessageBox.Show("修改成功", "修改密码");
            TB_Password.Text = "";
            TB_NewPassword.Text = "";
        }
        #endregion

        #region 生产配方选择
        private void CB_TypeOfTray_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (CB_TypeOfTray.SelectedIndex >= 0)
                {
                    DialogResult result = MessageBox.Show($"您是否选择 “{CB_TypeOfTray.Text}” 型号？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        foreach (var button in calibrationButtons)
                        {
                            button.BackColor = Color.Transparent;
                        }
                        calibrationButtons.Clear();
                        SetMessage("切换型号。");
                        currentTrayType = CB_TypeOfTray.Text; 
                        IsWrite = communication.WriteVariable(currentTrayType.Substring(2), "PLC测试信息[55]");
                        IsWrite = communication.WriteVariable(currentTrayType.Substring(2).Length, "PlcInPmt[72]");
                        if (IsWrite)
                        {
                            RecordAndShow($"{tempTrayType}切换为{currentTrayType}", LogType.Modification, TB_Modification);
                            tempTrayType = currentTrayType;
                        }

                        IsWrite = communication.WriteVariable(trayManager.TrayType[currentTrayType].Length, "PLCInPmt[45]");
                        IsWrite = communication.WriteVariable(Convert.ToDouble(trayManager.TrayType[currentTrayType].Width), "PLCInPmt[46]");
                        IsWrite = communication.WriteVariable(trayManager.TrayType[currentTrayType].LineSpacing, "PLCInPmt[47]");
                        IsWrite = communication.WriteVariable(trayManager.TrayType[currentTrayType].ColumnSpacing, "PLCInPmt[48]");
                        IsWrite = communication.WriteVariable(trayManager.TrayType[currentTrayType].TrayHeight, "PLCInPmt[49]");
                        IsWrite = communication.WriteVariable(Convert.ToDouble(trayManager.TrayType[currentTrayType].Length * trayManager.TrayType[currentTrayType].Width), "PLCInPmt[50]");
                        IsWrite = communication.WriteVariable(trayManager.TrayType[currentTrayType].Index, "PlcInID[1]");

                        if (trayManager.TrayType[currentTrayType].VacAngle == -90)
                        {
                            IsWrite = communication.WriteVariable(1, "PlcInID[7]");
                        }
                        else if (trayManager.TrayType[currentTrayType].VacAngle == 0)
                        {
                            IsWrite = communication.WriteVariable(2, "PlcInID[7]");
                        }
                        else if (trayManager.TrayType[currentTrayType].VacAngle == 90)
                        {
                            IsWrite = communication.WriteVariable(3, "PlcInID[7]");
                        }
                        else
                        {
                            IsWrite = communication.WriteVariable(-1, "PlcInID[7]");
                        }

                        if (trayManager.TrayType[currentTrayType].ClawsAngle == -90)
                        {
                            IsWrite = communication.WriteVariable(1, "PlcInID[8]");
                        }
                        else if (trayManager.TrayType[currentTrayType].ClawsAngle == 0)
                        {
                            IsWrite = communication.WriteVariable(2, "PlcInID[8]");
                        }
                        else if (trayManager.TrayType[currentTrayType].ClawsAngle == 90)
                        {
                            IsWrite = communication.WriteVariable(3, "PlcInID[8]");
                        }
                        else
                        {
                            IsWrite = communication.WriteVariable(-1, "PlcInID[8]");
                        }

                        string currentType = trayManager.TrayType[CB_TypeOfTray.Text].TrayType.Substring(0, 2);
                        if (currentType == "晶圆")
                        {
                            IsWrite = communication.WriteVariable(1, "PlcInID[6]");
                        }
                        else if (currentType == "金属")
                        {
                            IsWrite = communication.WriteVariable(2, "PlcInID[6]");
                        }
                        else if (currentType == "陶瓷")
                        {
                            IsWrite = communication.WriteVariable(3, "PlcInID[6]");
                        }
                        else
                        {
                            IsWrite = communication.WriteVariable(-1, "PlcInID[6]");
                        }

                        if (!IsWrite)
                            MessageBox.Show("参数未完全写入。", "参数写入", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SetMessage();
                    }
                    else
                    {
                        CB_TypeOfTray.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CB_Socket类_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CB_Socket类.SelectedIndex >= 0)
            {
                DialogResult result = MessageBox.Show($"您是否选择 “{CB_Socket类.Text}” ？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    RecordAndShow($"当前型号为：{CB_TypeOfTray.Text}", LogType.Modification, TB_Modification);
                    if (this.CB_Socket类.SelectedItem.ToString() == "四目")
                    {
                        communication.WriteVariable(1, "PlcInID[2]");
                        RecordAndShow($"当前产品为{currentTrayType}，更改为四目", LogType.Modification, TB_Modification);
                    }

                    if (this.CB_Socket类.SelectedItem.ToString() == "单目")
                    {
                        communication.WriteVariable(2, "PlcInID[2]");
                        RecordAndShow($"当前产品为{currentTrayType}，更改为单目", LogType.Modification, TB_Modification);
                    }
                }
                else
                {
                    CB_Socket类.SelectedIndex = -1;
                }
            }
        }

        private void CB_工位_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CB_工位.SelectedIndex >= 0)
            {
                DialogResult result = MessageBox.Show($"您是否选择 “{CB_工位.Text}” ？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string number = System.Text.RegularExpressions.Regex.Replace(CB_工位.Text, @"[^0-9]+", "");
                    if (int.TryParse(number, out int pos))
                    {
                        communication.WriteVariable(pos, "PlcInID[3]");
                        RecordAndShow($"切换为第{pos}个工位", LogType.Modification, TB_Modification);
                    }
                }
                else
                {
                    CB_工位.SelectedIndex = -1;
                }
            }
        }

        private void CB_工作盘数_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CB_工作盘数.SelectedIndex >= 0)
            {
                DialogResult result = MessageBox.Show($"您是否选择 “{CB_工作盘数.Text}” ？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string number = System.Text.RegularExpressions.Regex.Replace(CB_工作盘数.Text, @"[^0-9]+", "");
                    if (int.TryParse(number, out int pos))
                    {
                        communication.WriteVariable(pos, "PlcInID[5]");
                        RecordAndShow($"工作盘数{pos}盘", LogType.Modification, TB_Modification);
                    }
                }
                else
                {
                    CB_工作盘数.SelectedIndex = -1;
                }
            }
        }
        #endregion

        #region 自动模式选择
        private void btn自动模式本地_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否选择自动模式本地？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"选择自动模式本地", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[3]");
                communication.WriteVariable(false, "PlcInIO[27]");
                communication.WriteVariable(false, "PlcInIO[29]");
            }
        }

        private void btn自动模式远程测试_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否选择自动模式远程-测试？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"选择自动模式远程-测试", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[29]");
                communication.WriteVariable(false, "PlcInIO[3]");
                communication.WriteVariable(false, "PlcInIO[27]");
            }
        }
        #endregion

        #region 常用功能
        private void btn手动给测试机触发信号_MouseDown(object sender, MouseEventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[149]");
        }

        private void btn手动给测试机触发信号_MouseUp(object sender, MouseEventArgs e)
        {
            RecordAndShow($"手动给测试机触发信号", LogType.Modification, TB_Modification);
            communication.WriteVariable(false, "PlcInIO[149]");
        }

        private void btn人工上下料_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("上料X轴是否移动到取托盘避让位置？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"上料X轴移动到取托盘避让位置", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[210]");
            }
        }

        private void btn黑体一键上升_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("互锁条件：1黑体手动平移轴在指定位置，且黑体到位信号X13有信号；请确认设备满足以上条件，再开启黑体一键上升功能！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"黑体一键上升", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[334]");
            }
        }
        private void btn旋转夹爪1下降1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("人工确认：钧舵夹爪1已打开小位置且夹爪下方无干涉可正常下降！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"钧舵夹爪1气缸下降", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[537]");
            }
        }
        private void btn旋转夹爪2下降1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("人工确认：钧舵夹爪2已打开小位置且夹爪下方无干涉可正常下降！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"钧舵夹爪2气缸下降", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[539]");
            }
        }
        private void btn旋转夹爪3下降1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("人工确认：钧舵夹爪3已打开小位置且夹爪下方无干涉可正常下降！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"钧舵夹爪3气缸下降", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[541]");
            }
        }
        private void btn旋转夹爪4下降1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("人工确认：钧舵夹爪4已打开小位置且夹爪下方无干涉可正常下降！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"钧舵夹爪4气缸下降", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[543]");
            }
        }

        private void btn夹爪一键下料_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("互锁条件：1搬运Z轴在上升位置；2钧舵夹爪1、2、3、4气缸都在上升位置；3工位1、2、3、4翻转气缸都在翻0°位置；请确认设备满足以上条件，再开启钧舵夹爪一键下料功能！", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RecordAndShow($"钧舵夹爪一键下料", LogType.Modification, TB_Modification);
                communication.WriteVariable(true, "PlcInIO[150]");
            }
        }
        #endregion

        #region 报警历史及信息修改历史查看
        private void BTN查看报警历史_Click(object sender, EventArgs e)
        {
            Form3 logForm = new Form3();
            logForm.ShowDialog();
        }

        private void BTN报警复位_Click(object sender, EventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[0]");
        }

        private void BTN蜂鸣停止_Click(object sender, EventArgs e)
        {
            communication.WriteVariable(true, "PlcInIO[1]");
        }

        private void BTN查看修改历史_Click(object sender, EventArgs e)
        {
            Form3 logForm = new Form3
            {
                Text = "查看记录",
                FileName = "更改记录",
                FilePath = "Modification"
            };
            logForm.ShowDialog();
        }

        private void BTN清除修改信息_Click(object sender, EventArgs e)
        {
            TB_Modification.Text = "";
        }
        #endregion

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

        private bool WriteValue(TextBox valueBox, string message)
        {
            if (double.TryParse(valueBox.Text, out double value))
            {
                if (communication.WriteVariable(value, (string)valueBox.Tag))
                {
                    valueBox.Text = "";
                    return true;
                }
                else
                {
                    MessageBox.Show("参数写入失败，请检查链接");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool WriteValue(TextBox valueBox, string message, int min = 0)
        {
            if (double.TryParse(valueBox.Text, out double value))
            {
                if (value >= min)
                {
                    if (communication.WriteVariable(value, (string)valueBox.Tag))
                    {
                        valueBox.Text = "";
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("参数写入失败，请检查链接");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show($"输入错误请检查,值应大于0");
                    return false;
                }
            }
            else
            {
                MessageBox.Show(message);
                return false;
            }
        }

        private bool WriteValue(TextBox valueBox, string message, int min = 0, int max = 26)
        {
            if (double.TryParse(valueBox.Text, out double value))
            {
                if (value >= min && value <= max)
                {
                    if (communication.WriteVariable(value, (string)valueBox.Tag))
                    {
                        valueBox.Text = "";
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("参数写入失败，请检查链接");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show($"输入错误请检查,请输入{min}-{max}之间的整数");
                    return false;
                }
            }
            else
            {
                MessageBox.Show(message);
                return false;
            }
        }

        #region 手动气缸、电机操作
        private void BTN手动操作_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender; 
            communication.WriteVariable(true, (string)button.Tag);
            RecordAndShow($"手动操作 [{button.Name.Substring(3)}] 按下", LogType.Modification, TB_Modification);
        }

        private void BTN手动操作_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender; 
            communication.WriteVariable(false, (string)button.Tag);
        }
        //吸嘴夹爪工装次数统计清零
        private void BTN计数清零_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("使用次数是否清零？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Button button = (Button)sender;
                communication.WriteVariable(true, (string)button.Tag);
                RecordAndShow($"计数清零 [{button.Name.Substring(3)}]", LogType.Modification, TB_Modification);
            }
        }
        //手动电机2操作
        private void BTN钧舵相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;

        }

        private void BTN钧舵相对旋转_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            communication.WriteVariable(false, (string)button.Tag);
        }

        private void btnJD1相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtR1旋转角度.Text != "")
            {
                Button button = (Button)sender;
                communication.WriteVariable(Convert.ToDouble(txtR1旋转角度.Text), "PLCInPmt[26]");
                RecordAndShow($"[{button.Name.Substring(3)}] 旋转角度：{txtR1旋转角度.Text}", LogType.Modification, TB_Modification);
            }
            communication.WriteVariable(true, "PlcInIO[596]");
        }
        
        private void btnJD2相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtR2旋转角度.Text != "")
            {
                Button button = (Button)sender;
                communication.WriteVariable(Convert.ToDouble(txtR2旋转角度.Text), "PLCInPmt[27]");
                RecordAndShow($"[{button.Name.Substring(3)}] 旋转角度：{txtR2旋转角度.Text}", LogType.Modification, TB_Modification);
            }
            communication.WriteVariable(true, "PlcInIO[597]");
        }
        
        private void btnJD3相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtR3旋转角度.Text != "")
            {
                Button button = (Button)sender;
                communication.WriteVariable(Convert.ToDouble(txtR3旋转角度.Text), "PLCInPmt[28]");
                RecordAndShow($"[{button.Name.Substring(3)}] 旋转角度：{txtR3旋转角度.Text}", LogType.Modification, TB_Modification);
            }
            communication.WriteVariable(true, "PlcInIO[598]");
        }
        
        private void btnJD4相对旋转_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtR4旋转角度.Text != "")
            {
                Button button = (Button)sender; 
                communication.WriteVariable(Convert.ToDouble(txtR4旋转角度.Text), "PLCInPmt[29]");
                RecordAndShow($"[{button.Name.Substring(3)}] 旋转角度：{txtR4旋转角度.Text}", LogType.Modification, TB_Modification);
            }
            communication.WriteVariable(true, "PlcInIO[599]");
        }
        
        //手动电机1夹爪选择
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
        #endregion

        #region 示教操作
        //将PLC上的bool值改为true一定时间后变回false
        private async Task BoolSwitchAsync(string variableName, string message = "", string tip = "您是否确定此操作？", int delay = 1000)
        {
            DialogResult result = MessageBox.Show(tip, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                if (communication.WriteVariable(true, variableName))
                {
                    await Task.Delay(delay);
                    SetMessage("示教中。");
                    RecordAndShow($"{message}", LogType.Modification, TB_Modification);
                    IsWrite = communication.WriteVariable(false, variableName);
                    SetMessage();
                }
                else
                {
                    MessageBox.Show("参数写入失败，连接断开。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
        }
        //将PLC上的bool值改为true一定时间后变回false
        private async Task BoolSwitchAsync(Button button, string message = "", string tip = "您是否确定此操作？", int delay = 1000)
        {
            DialogResult result = MessageBox.Show(tip, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                button.BackColor = Color.LightGreen;
                calibrationButtons.Add(button);
                if (communication.WriteVariable(true, (string)button.Tag))
                {
                    await Task.Delay(delay);
                    SetMessage("示教中。");
                    RecordAndShow($"{message}", LogType.Modification, TB_Modification);
                    IsWrite = communication.WriteVariable(false, (string)button.Tag);
                    SetMessage();
                    button.BackColor = Color.LightGreen;
                    calibrationButtons.Add(button);
                }
                else
                {
                    MessageBox.Show("参数写入失败，连接断开。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            } 
        }
        //示教界面1示教
        private async void BTN示教1_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (calibrationTextBoxes.ContainsKey($"txt{button.Name.Substring(3)}"))
                await BoolSwitchAsync(button, $"示教1 [{button.Name.Substring(3)}] 按下，当前值：{calibrationTextBoxes[$"txt{button.Name.Substring(3)}"].Text}");
            else
                await BoolSwitchAsync(button, $"示教1 [{button.Name.Substring(3)}] 按下");
        }
        //示教界面2示教
        private async void BTN示教2_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (calibrationTextBoxes.ContainsKey($"txt{button.Name.Substring(3)}"))
                await BoolSwitchAsync(button, $"示教2 [{button.Name.Substring(3)}] 按下，当前值：{calibrationTextBoxes[$"txt{button.Name.Substring(3)}"].Text}");
            else
                await BoolSwitchAsync(button, $"示教2 [{button.Name.Substring(3)}] 按下");
        }

        private void BTN值写入_MouseDown(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            string[] info = ((string)button.Tag).Split(';');
            if (info.Length == 2)
            {
                if (button.Name == "btn钧舵打开小位置设置")
                {
                    if (WriteValue(calibrationTextBoxes[info[1]],
                        "输入错误请检查,请输入钧舵夹爪打开小位置值[40-（夹爪夹持方向产品的尺寸+4）。4指的是左右各留2mm的夹持余量，可根据实际情况进行调整]。 如：W9产品-夹爪夹持方向产品的尺寸为24mm，则输入钧舵夹爪打开小位置值为12mm；W7产品-夹爪夹持方向产品的尺寸为18mm，则输入钧舵夹爪打开小位置值为18mm。", 
                        0, 26))
                        communication.WriteVariable(true, info[0]);
                }
                else if (button.Name == "btn判断范围写入")
                {
                    if (WriteValue(calibrationTextBoxes[info[1]], "输入错误请检查", 0))
                        communication.WriteVariable(true, info[0]);
                }
                else
                {
                    if (WriteValue(calibrationTextBoxes[info[1]], "输入错误请检查"))
                        communication.WriteVariable(true, info[0]);
                }
            }
            else if (info.Length == 3)
            {
                WriteValue(calibrationTextBoxes[info[1]], "输入错误请检查");
                WriteValue(calibrationTextBoxes[info[2]], "输入错误请检查");
                communication.WriteVariable(true, info[0]);
            }
        }

        private void BTN值写入_MouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            string[] info = ((string)button.Tag).Split(';');
            communication.WriteVariable(false, info[0]);
            RecordAndShow($"示教1值写入 [{button.Name.Substring(3)}]", LogType.Modification, TB_Modification);
        }
        //示教2
        private void btn自动模式远程_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否选择自动模式远程-控料？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                communication.WriteVariable(true, "PlcInIO[27]");
                communication.WriteVariable(false, "PlcInIO[3]");
                communication.WriteVariable(false, "PlcInIO[29]");
                RecordAndShow($"选择自动模式远程-控料", LogType.Modification, TB_Modification);
            }
        }
        #endregion

        private void BTN确认更改_Click(object sender, EventArgs e)
        {
            try
            {
                #region 输入参数校验
                if (string.IsNullOrEmpty(txt输入产品型号.Text))
                {
                    MessageBox.Show("请输入产品型号!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (txt输入产品型号.Text.Length <= 2)
                {
                    MessageBox.Show("请输入正确的产品型号，如晶圆612W9");
                    return;
                }

                if (int.TryParse(txt托盘行数.Text, out int length))
                {
                    if (length <= 0 || length > 10)
                    {
                        MessageBox.Show("托盘行数输入错误请检查,请输入1-10之间的整数");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("托盘行数输入错误请检查,请输入1-10之间的整数");
                    return;
                }

                if (int.TryParse(txt托盘列数.Text, out int width))
                {
                    if (width <= 0 || width > 10)
                    {
                        MessageBox.Show("托盘列数输入错误请检查,请输入1-10之间的整数");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("托盘列数输入错误请检查,请输入1-10之间的整数");
                    return;
                }

                if (double.TryParse(txt托盘行间距.Text, out double 行间距))
                {
                    if (行间距 <= 0)
                    {
                        MessageBox.Show("输入错误请检查,托盘行间距应大于0");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("输入错误请检查,请输入托盘行间距");
                    return;
                }

                if (double.TryParse(txt托盘列间距.Text, out double 列间距))
                {
                    if (列间距 <= 0)
                    {
                        MessageBox.Show("输入错误请检查,托盘列间距应大于0");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("输入错误请检查,请输入托盘列间距");
                    return;
                }

                if (double.TryParse(txt托盘高度.Text, out double 托盘高度))
                {
                    if (托盘高度 <= 0)
                    {
                        MessageBox.Show("输入错误请检查,托盘间距应大于0");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("输入错误请检查,请输入托盘间距");
                    return;
                }

                if (int.TryParse(txt上料吸嘴旋转角度.Text, out int 上料吸嘴旋转角度))
                {
                    if (上料吸嘴旋转角度 != -90 && 上料吸嘴旋转角度 != 0 && 上料吸嘴旋转角度 != 90)
                    {
                        MessageBox.Show("输入错误请检查,请输入上料吸嘴旋转角度，如-90、0、90。 提示：吸嘴顺时针旋转时，输入90；吸嘴逆时针旋转时，输入-90。");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("输入错误请检查,请输入上料吸嘴旋转角度，如-90、0、90。 提示：吸嘴顺时针旋转时，输入90；吸嘴逆时针旋转时，输入-90。");
                    return;
                }

                if (int.TryParse(txt搬运夹爪旋转角度.Text, out int 搬运夹爪旋转角度))
                {
                    if (搬运夹爪旋转角度 != -90 && 搬运夹爪旋转角度 != 0 && 搬运夹爪旋转角度 != 90)
                    {
                        MessageBox.Show("输入错误请检查,请输入搬运夹爪旋转角度，如-90、0、90。 提示：夹爪顺时针旋转时，输入90；夹爪逆时针旋转时，输入-90。");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("输入错误请检查,请输入搬运夹爪旋转角度，如-90、0、90。 提示：夹爪顺时针旋转时，输入90；夹爪逆时针旋转时，输入-90。");
                    return;
                }
                #endregion

                if (loginForm != null)
                {
                    if (loginForm.CurrentUser == "管理员")
                    {
                        DialogResult result = MessageBox.Show($"请确认产品型号无误 “{txt输入产品型号.Text}” ", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            string 产品型号 = txt输入产品型号.Text;
                            if (trayManager.TrayType.ContainsKey(产品型号))
                            {
                                trayManager.TrayType[产品型号].Length = length;
                                trayManager.TrayType[产品型号].Width = width;
                                trayManager.TrayType[产品型号].LineSpacing = 行间距;
                                trayManager.TrayType[产品型号].ColumnSpacing = 列间距;
                                trayManager.TrayType[产品型号].TrayHeight = 托盘高度;
                                trayManager.TrayType[产品型号].VacAngle = 上料吸嘴旋转角度;
                                trayManager.TrayType[产品型号].ClawsAngle = 搬运夹爪旋转角度;
                                trayManager.SaveTrayType(产品型号, trayManager.TrayType[产品型号]);
                                RecordAndShow($"{产品型号}参数修改，每行的产品数：{length} 每列的产品数：{width} 行间距：{行间距} 列间距：{列间距} 托盘高度：{托盘高度} " +
                                    $"上料吸嘴旋转角度：{上料吸嘴旋转角度} 搬运夹爪旋转角度：{搬运夹爪旋转角度}", LogType.Modification, TB_Modification);
                            }
                            else
                            {
                                TypeOfTray typeOfTray = new TypeOfTray()
                                {
                                    Index = trayManager.TrayType.Count + 1,
                                    TrayType = 产品型号,
                                    Length = length,
                                    Width = width,
                                    LineSpacing = 行间距,
                                    ColumnSpacing = 列间距,
                                    TrayHeight = 托盘高度,
                                    VacAngle = 上料吸嘴旋转角度,
                                    ClawsAngle = 搬运夹爪旋转角度
                                };
                                trayManager.SaveTrayType(产品型号, typeOfTray);
                                RecordAndShow($"{产品型号}新增，每行的产品数：{length} 每列的产品数：{width} 行间距：{行间距} 列间距：{列间距} 托盘高度：{托盘高度} " +
                                    $"上料吸嘴旋转角度：{上料吸嘴旋转角度} 搬运夹爪旋转角度：{搬运夹爪旋转角度}", LogType.Modification, TB_Modification);
                            }
                            SetCB_TypeOfTray(trayManager);

                            MessageBox.Show("输入成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("请切换管理员权限，再进行修改!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception )
            {
                MessageBox.Show("输入错误请检查!");
            }
        }

        private void TSM打开文档_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem Menu = (ToolStripMenuItem)sender;
                string path = $"{Environment.CurrentDirectory}\\Documents\\{helpInformation[(string)Menu.Tag]}";
                path = path.Replace(" ", "\" \"");
                Process.Start(helpInformation["BrowserPath"], path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载帮助文件失败。{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BTN配方_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string address = (string)button.Tag;
            switch (address)
            {
                case "PlcInIO[738]":
                    await BoolSwitchAsync(address, "配方导出", "确认是否将当前产品示教位置导出。");
                    break;
                case "PlcInIO[739]":
                    await BoolSwitchAsync(address, "配方导入", "确认是否将其他产品示教位置导入到当前产品。");
                    break;
                default:
                    break;
            }
        }
    }
}
