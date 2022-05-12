using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FT.Data
{
    public class SensorData
    {
        //探测器编码
        public virtual string SensorCode { get; set; }
        //探测器类型
        public string SensorType { get; set; }
        //测试工位
        public string TestStation { get; set; }
        //测试结果
        public virtual int SensorQuality { get; set; }
        //所在托盘编号
        public string TrayNumber { get; set; }
        //所在托盘中的位置
        public int PosInTray { get; set; }
        //外观
        public string Appearance { get; set; }
        //开始测试时间
        public string StartTime { get; set; }
        //测试完成时间
        public string EndTime { get; set; }

        /// <summary>
        /// 此处初始化Sensor的属性值
        /// </summary>
        /// <param name="sensorCode">传感器编码</param>
        /// <param name="sensorType">传感器类型</param>
        /// <param name="testStation">测试工位</param>
        /// <param name="sensorQuality">测试结果</param>
        /// <param name="trayNumber">托盘编号</param>
        /// <param name="posInTray">在托盘中的位置</param>
        /// <param name="appearance">外观</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        public SensorData(string sensorCode, string sensorType = "", string testStation = "", int sensorQuality = -1, string trayNumber = "", int posInTray = -1, string appearance = "", string startTime = "", string endTime = "")
        {
            SensorCode = sensorCode;
            SensorType = sensorType;
            TestStation = testStation;
            SensorQuality = sensorQuality;
            TrayNumber = trayNumber;
            PosInTray = posInTray;
            Appearance = appearance;
            StartTime = DateConverter(startTime);
            EndTime = DateConverter(endTime);
        }
        /// <summary>
        /// 此处初始化用来存储的Sensor数据
        /// </summary>
        /// <param name="sensor">要存储的Sensor数据</param>
        public SensorData(Sensor sensor)
        {
            SensorCode = sensor.SensorCode;
            SensorType = sensor.SensorType;
            TestStation = sensor.TestStation;
            SensorQuality = sensor.SensorQuality;
            TrayNumber = sensor.TrayNumber;
            PosInTray = sensor.PosInTray;
            Appearance = sensor.Appearance;
            StartTime = sensor.StartTime;
            EndTime = sensor.EndTime;
        }

        public SensorData()
        {

        }

        public void SetTestData(string testStation, int quality)
        {
            TestStation = testStation;
            SensorQuality = quality;
            //EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string DateConverter(string date)
        {
            if (date.Length == 14)
                return (date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + date.Substring(8, 2) + ":" + date.Substring(10, 2) + ":" + date.Substring(12, 2));
            else
                return (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    public class Sensor : SensorData
    {
        //探测器编码
        private string sensorCode;
        public override string SensorCode
        {
            get { return sensorCode; }
            set
            {
                sensorCode = value;
                if (sensorCode == "null")
                    WinformTool.InvokeOnThread(SensorStatusLabel, new Action(() => { SensorStatusLabel.BackColor = Color.White; }));
                if (sensorCode == "noData")
                    WinformTool.InvokeOnThread(SensorStatusLabel, new Action(() => { SensorStatusLabel.BackColor = Color.Gray; }));
            }
        }
        //测试结果
        private int sensorQuality;
        public override int SensorQuality
        {
            get { return sensorQuality; }
            set
            {
                sensorQuality = value;
                if (SensorCode != "null" && SensorCode != "noData")
                {
                    if (sensorQuality == 0)
                    {
                        WinformTool.InvokeOnThread(SensorStatusLabel, new Action(() => { SensorStatusLabel.BackColor = Color.LightGray; }));
                    }
                    else if (sensorQuality == 1)
                    {
                        WinformTool.InvokeOnThread(SensorStatusLabel, new Action(() => { SensorStatusLabel.BackColor = Color.Lime; }));
                    }
                    else if (sensorQuality == 2)
                    {
                        WinformTool.InvokeOnThread(SensorStatusLabel, new Action(() => { SensorStatusLabel.BackColor = Color.OrangeRed; }));
                    }
                }
            }
        }
        //状态显示控件
        public Label SensorStatusLabel;

        /// <summary>
        /// 此处初始化Sensor的属性值
        /// </summary>
        /// <param name="sensorCode">传感器编码</param>
        /// <param name="sensorType">传感器类型</param>
        /// <param name="testStation">测试工位</param>
        /// <param name="sensorQuality">测试结果</param>
        /// <param name="trayNumber">托盘编号</param>
        /// <param name="posInTray">在托盘中的位置</param>
        /// <param name="appearance">外观</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        public Sensor(string sensorCode, string sensorType = "", string testStation = "", int sensorQuality = -1, string trayNumber = "", int posInTray = -1, string appearance = "", string startTime = "", string endTime = "")
        {
            SensorStatusLabel = new Label
            {
                Name = sensorCode,
                Width = 24,
                Height = 24,
                Text = PosInTray.ToString()
            };

            SensorCode = sensorCode;
            SensorType = sensorType;
            TestStation = testStation;
            SensorQuality = sensorQuality;
            TrayNumber = trayNumber;
            PosInTray = posInTray;
            Appearance = appearance;
            StartTime = DateConverter(startTime);
            EndTime = DateConverter(endTime);
            //Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>
        /// 此处初始化从文件加载的Sensor数据
        /// </summary>
        /// <param name="sensorData">从文件中读取到的数据</param>
        public Sensor(SensorData sensorData)
        {
            SensorStatusLabel = new Label
            {
                Name = sensorCode,
                Width = 24,
                Height = 24,
                Text = PosInTray.ToString()
            };

            SensorCode = sensorData.SensorCode;
            SensorType = sensorData.SensorType;
            TestStation = sensorData.TestStation;
            SensorQuality = sensorData.SensorQuality;
            TrayNumber = sensorData.TrayNumber;
            PosInTray = sensorData.PosInTray;
            Appearance = sensorData.Appearance;
            StartTime = sensorData.StartTime;
            EndTime = sensorData.EndTime;
            SensorStatusLabel.Text = PosInTray.ToString();
        }

        public void SetTrayData(string trayNumber, int posInTray)
        {
            TrayNumber = trayNumber;
            PosInTray = posInTray;
            SensorStatusLabel.Text = PosInTray.ToString();
        }
    }

    public class TrayData
    {
        //托盘编号
        public virtual string TrayNumber { get; set; }
        //托盘长方向孔位
        public int TrayLength { get; set; }
        //托盘宽方向孔位
        public int TrayWidth { get; set; }
        //托盘中探测器的数据
        public virtual Dictionary<string, SensorData> Sensors { get; set; }

        public int X;

        public int Y;

        public TrayData(Tray tray)
        {
            Sensors = new Dictionary<string, SensorData>();
            foreach (var item in tray.Sensors)
            {
                SensorData sensorData = new SensorData(item.Value);
                Sensors.Add(sensorData.PosInTray.ToString(), sensorData);
            }

            TrayNumber = tray.TrayNumber;
            TrayLength = tray.TrayLength;
            TrayWidth = tray.TrayWidth;

            X = tray.PosOnPanel.X;
            Y = tray.PosOnPanel.Y;
        }

        public TrayData()
        {

        }
    }

    public class Tray
    {
        //托盘编号
        private string trayNumber;
        public string TrayNumber
        {
            get { return trayNumber; }
            set
            {
                trayNumber = value;
                if (TrayLabel.IsHandleCreated)
                {
                    TrayLabel.Invoke(new Action(() => TrayLabel.Text = trayNumber));
                }
                else
                {
                    TrayLabel.Text = trayNumber;
                }
                if (Sensors != null) UpdateSensorsTrayNumber(trayNumber);
            }
        }
        //托盘长方向孔位
        public int TrayLength { get; set; }
        //托盘宽方向孔位
        public int TrayWidth { get; set; }
        //托盘中探测器的数据
        public Dictionary<string, Sensor> Sensors { get; set; }
        //控件位置
        public Position PosOnPanel;
        //显示托盘号的控件
        public Label TrayLabel;

        public Tray(int length, int width, Position posOnPanel, string trayNumber = "")
        {
            TrayLabel = new Label();
            TrayLabel.Name = trayNumber.ToString();
            TrayLabel.ForeColor = Color.Black;
            //TrayLabel.BackColor = Color.LightSkyBlue;
            TrayLabel.Text = trayNumber;

            Sensors = new Dictionary<string, Sensor>();
            for (int i = 0; i < length * width; i++)
            {
                Sensor sensor = new Sensor("noData");
                sensor.SetTrayData(trayNumber, i + 1);
                Sensors.Add(sensor.PosInTray.ToString(), sensor);
            }

            TrayNumber = trayNumber;
            TrayLength = length;
            TrayWidth = width;
            PosOnPanel = posOnPanel;
        }

        public Tray(TrayData trayData)
        {
            TrayLabel = new Label();
            TrayLabel.Name = trayData.TrayNumber.ToString();
            TrayLabel.ForeColor = Color.Black;
            //TrayLabel.BackColor = Color.LightSkyBlue;
            TrayLabel.Text = trayData.TrayNumber;

            Sensors = new Dictionary<string, Sensor>();
            foreach (var item in trayData.Sensors)
            {
                Sensor sensor = new Sensor(item.Value);
                Sensors.Add(sensor.PosInTray.ToString(), sensor);
            }

            TrayNumber = trayData.TrayNumber;
            TrayLength = trayData.TrayLength;
            TrayWidth = trayData.TrayWidth;
            PosOnPanel = new Position(trayData.X, trayData.Y);
        }

        /// <summary>
        /// 标签阵列
        /// </summary>
        /// <param name="canvasControl">要显示标签的控件</param>
        /// <param name="x">相对于控件的左上角横坐标</param>
        /// <param name="y">相对于控件的左上角纵坐标</param>
        public void UpdateTrayLabel(Control canvasControl, int x, int y)
        {
            //之前的显示控件清除
            //WinformTool.InvokeOnThread(canvasControl, new Action(() => { canvasControl.Controls.Clear(); }));
            //设置托盘编号显示控件的位置
            WinformTool.InvokeOnThread(TrayLabel, new Action(() => { TrayLabel.Location = new Point(x, y - 25); }));
            //将托盘编号显示控件在canvasControl中显示出来
            WinformTool.InvokeOnThread(canvasControl, new Action(() => { canvasControl.Controls.Add(TrayLabel); }));
            //计算控件位置
            List<Point> location = WinformTool.SetLocation(x, y, Sensors.Count, TrayLength, 25, 25);
            //探测器控件设置
            for (int i = 0; i < Sensors.Count; i++)
            {
                //Sensors[i + 1].SensorStatusLabel.Text = Sensors[i + 1].PosInTray.ToString();
                //设置位置
                WinformTool.InvokeOnThread(Sensors[(i + 1).ToString()].SensorStatusLabel, new Action(() => { Sensors[(i + 1).ToString()].SensorStatusLabel.Location = location[i]; }));
                //显示控件
                WinformTool.InvokeOnThread(canvasControl, new Action(() => { canvasControl.Controls.Add(Sensors[(i + 1).ToString()].SensorStatusLabel); }));
            }
        }

        public void UpdateTrayLabel(Control canvasControl)
        {
            //之前的显示控件清除
            //WinformTool.InvokeOnThread(canvasControl, new Action(() => { canvasControl.Controls.Clear(); }));
            //设置托盘编号显示控件的位置
            WinformTool.InvokeOnThread(TrayLabel, new Action(() => { TrayLabel.Location = new Point(PosOnPanel.X, PosOnPanel.Y - 25); }));
            //将托盘编号显示控件在canvasControl中显示出来
            WinformTool.InvokeOnThread(canvasControl, new Action(() => { canvasControl.Controls.Add(TrayLabel); }));
            //计算控件位置
            List<Point> location = WinformTool.SetLocation(PosOnPanel.X, PosOnPanel.Y, Sensors.Count, TrayLength, 25, 25);

            for (int i = 0; i < Sensors.Count; i++)
            {
                //Sensors[i + 1].SensorStatusLabel.Text = Sensors[i + 1].PosInTray.ToString();
                WinformTool.InvokeOnThread(Sensors[(i + 1).ToString()].SensorStatusLabel, new Action(() => { Sensors[(i + 1).ToString()].SensorStatusLabel.Location = location[i]; }));
                WinformTool.InvokeOnThread(canvasControl, new Action(() => { canvasControl.Controls.Add(Sensors[(i + 1).ToString()].SensorStatusLabel); }));
            }
        }

        public void UpdateSensorsTrayNumber(string trayNumber)
        {
            foreach (var sensor in Sensors)
            {
                sensor.Value.TrayNumber = trayNumber;
            }
        }
    }

    public static class WinformTool
    {
        public static void DrawToBitmap(Control control, string path, string name)
        {
            Bitmap bitmap = new Bitmap(control.Width, control.Height);
            control.DrawToBitmap(bitmap, new Rectangle(0, 0, control.Width, control.Height));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            bitmap.Save(path + "\\" + name + ".bmp");
        }

        public static void InvokeOnThread(Control control, Action method)
        {
            if (control.IsHandleCreated)
            {
                control.Invoke(method);
            }
            else
            {
                method();
            }
        }
        /// <summary>
        /// 得到一个矩形阵列的坐标
        /// </summary>
        /// <param name="x">阵列起始X坐标</param>
        /// <param name="y">阵列起始Y坐标</param>
        /// <param name="count">阵列元素个数</param>
        /// <param name="length">每行的元素个数</param>
        /// <param name="xInterval">阵列坐标x方向间距</param>
        /// <param name="yInterval">阵列坐标y方向间距</param>
        /// <returns>阵列坐标列表</returns>
        public static List<Point> SetLocation(int x, int y, int count, int length, int xInterval, int yInterval)
        {
            int o = x;
            List<Point> locationList = new List<Point>();
            for (int i = 0; i < count; i++)
            {
                locationList.Add(new Point(x, y));
                x = x + xInterval;
                if ((i + 1) % length == 0)
                {
                    x = o;
                    y = y + yInterval;
                }
            }
            return locationList;
        }
        /// <summary>
        /// 设置一个Label组成的矩形阵列
        /// </summary>
        /// <param name="labelsLocation">阵列坐标列表</param>
        /// <param name="labelSize">Label大小（方形）</param>
        /// <param name="code">每个阵列的标记</param>
        /// <param name="offset">标记相对于起始坐标的偏移</param>
        /// <returns>包含标记的Label阵列列表</returns>
        public static List<Label> SetLabel(List<Point> labelsLocation, int labelSize, string code, Point offset)
        {
            List<Label> labelList = new List<Label>();
            Label title = new Label();
            title.Name = code;
            title.Width = 150;
            title.ForeColor = Color.OrangeRed;
            title.Text = code;
            title.Location = new Point(labelsLocation[0].X, labelsLocation[0].Y - offset.Y);
            labelList.Add(title);
            for (int i = 0; i < labelsLocation.Count; i++)
            {
                Label slot = new Label();
                slot.Name = code + i.ToString();
                slot.Width = labelSize;
                slot.Height = labelSize;
                slot.ForeColor = Color.Blue;
                slot.BackColor = Color.LightSkyBlue;
                slot.Text = (i + 1).ToString();
                slot.Location = labelsLocation[i];
                labelList.Add(slot);
            }
            return labelList;
        }
        /// <summary>
        /// 在控件上绘制Label列表
        /// </summary>
        /// <param name="canvasControl">需要绘制的控件</param>
        /// <param name="labels">Label列表</param>
        public static void DrawLabel(Control canvasControl, List<Label> labels)
        {
            for (int i = 0; i < labels.Count; i++)
            {
                canvasControl.Controls.Add(labels[i]);
            }
        }

        public static void ClearLabel(Control canvasControl, List<Label> labels)
        {
            foreach (var item in labels)
            {
                canvasControl.Controls.Remove(item);
            }
        }
    }

}
