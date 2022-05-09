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
        private string sensorCode;
        public string SensorCode
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
        //探测器类型
        public string SensorType { get; set; }
        //测试工位
        public string TestStation { get; set; }
        //测试结果
        private int sensorQuality;
        public int SensorQuality
        {
            get { return sensorQuality; }
            set
            {
                sensorQuality = value;
                if (SensorCode != "null" && SensorCode != "noData")
                {
                    if (sensorQuality == 0)
                    {
                        WinformTool.InvokeOnThread(SensorStatusLabel, new Action(() => { SensorStatusLabel.BackColor = Color.LightGreen; }));
                    }
                    else if (sensorQuality == 1)
                    {
                        WinformTool.InvokeOnThread(SensorStatusLabel, new Action(() => { SensorStatusLabel.BackColor = Color.OrangeRed; }));
                    }
                }
            }
        }
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
        //状态显示控件
        public Label SensorStatusLabel;

        public SensorData(string sensorCode, string sensorType = "", string testStation = "", int sensorQuality = -1, string trayNumber = "", int posInTray = -1, string appearance = "", string startTime = "", string endTime = "")
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

        public SensorData()
        {

        }

        public void SetTrayData(string trayNumber, int posInTray)
        {
            TrayNumber = trayNumber;
            PosInTray = posInTray;
            SensorStatusLabel.Text = PosInTray.ToString();
        }

        public void SetTestData(string testStation, int quality)
        {
            TestStation = testStation;
            SensorQuality = quality;
            //EndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string DateConverter(string date)
        {
            if (date.Length == 14)
                return (date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + date.Substring(8, 2) + ":" + date.Substring(10, 2) + ":" + date.Substring(12, 2));
            else
                return ("error");
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
            }
        }
        //托盘长方向孔位
        public int TrayLength { get; set; }
        //托盘宽方向孔位
        public int TrayWidth { get; set; }
        //托盘中探测器的数据
        public Dictionary<int, SensorData> Sensors { get; set; }
        //显示托盘号的控件
        public Label TrayLabel;
        //控件位置
        public Position PosOnPanel;

        public Tray(int length, int width, Position posOnPanel, string trayNumber = "")
        {
            TrayLabel = new Label();
            TrayLabel.Name = trayNumber.ToString();
            TrayLabel.ForeColor = Color.Black;
            //TrayLabel.BackColor = Color.LightSkyBlue;
            TrayLabel.Text = trayNumber;

            TrayLength = length;
            TrayWidth = width;
            PosOnPanel = posOnPanel;
            TrayNumber = trayNumber;


            Sensors = new Dictionary<int, SensorData>();
            for (int i = 0; i < length * width; i++)
            {
                SensorData sensor = new SensorData("noData");
                sensor.SetTrayData(trayNumber, i + 1);
                Sensors.Add(sensor.PosInTray, sensor);
            }
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
                WinformTool.InvokeOnThread(Sensors[i + 1].SensorStatusLabel, new Action(() => { Sensors[i + 1].SensorStatusLabel.Location = location[i]; }));
                //显示控件
                WinformTool.InvokeOnThread(canvasControl, new Action(() => { canvasControl.Controls.Add(Sensors[i + 1].SensorStatusLabel); }));
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
                WinformTool.InvokeOnThread(Sensors[i + 1].SensorStatusLabel, new Action(() => { Sensors[i + 1].SensorStatusLabel.Location = location[i]; }));
                WinformTool.InvokeOnThread(canvasControl, new Action(() => { canvasControl.Controls.Add(Sensors[i + 1].SensorStatusLabel); }));
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
