using FT.Data;
using MyToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FT
{
    public class TrayManager
    {
        //托盘索引，区分托盘是第几盘，之后可以按托盘编号来寻找（Linq）
        public int TrayIndex;
        //托盘种类
        public Dictionary<string, TypeOfTray> TrayType;
        //Mapping图布局
        public List<Position> MappingLayout;
        //当前所有的上料盘数据
        public List<Tray> Trays = new List<Tray>();

        readonly LogFile logfile = new LogFile();

        public TrayManager()
        {
            try
            {
                if (Trays == null) Trays = new List<Tray>();
                InitialzeMappingLayout();
                InitializeTrayType();
            }
            catch (Exception e)
            {
                logfile.WriteLog("读取托盘配置数据发生错误。" + e.Message, "托盘配置读取");
            }
        }

        /// <summary>
        /// 初始化Mapping图的位置配置文件，如果没有则创建默认配置
        /// </summary>
        public void InitialzeMappingLayout()
        {
            //读取Mapping图布局配置文件
            MappingLayout = JsonManager.ReadJsonString<List<Position>>(Environment.CurrentDirectory + "\\Configuration\\", "MappingLayout");
            if (MappingLayout == null)
            {
                MappingLayout = new List<Position>();
                MappingLayout.Add(new Position(20, 30));
                MappingLayout.Add(new Position(220, 30));
                MappingLayout.Add(new Position(420, 30));
                JsonManager.SaveJsonString($"{Environment.CurrentDirectory}\\Configuration\\", "MappingLayout", MappingLayout);
            }
        }

        /// <summary>
        /// 初始化托盘种类，当没有配置文件时创建默认配置
        /// </summary>
        public void InitializeTrayType()
        {
            //读取托盘种类配置文件
            TrayType = JsonManager.ReadJsonString<Dictionary<string, TypeOfTray>>(Environment.CurrentDirectory + "\\Configuration\\", "TypeOfTray");
            if (TrayType == null)
            {
                TrayType = new Dictionary<string, TypeOfTray>();
                TrayType.Add("Default", new TypeOfTray() { Index = 9999, TrayType = "Default", Length = 5, Width = 5 });
                JsonManager.SaveJsonString($"{Environment.CurrentDirectory}\\Configuration\\", "TypeOfTray", TrayType);
            }
        }

        /// <summary>
        /// 按托盘种类初始化托盘，每个盘在界面上的位置是固定的
        /// 按照每个盘在界面的位置来初始化
        /// </summary>
        /// <param name="trayType">托盘种类</param>
        public void InitializeTrays(string trayType)
        {
            if (trayType == "") return;
            Trays.Clear();
            for (int i = 0; i < MappingLayout.Count; i++)
            {
                Trays.Add(new Tray(TrayType[trayType].Length, TrayType[trayType].Width, MappingLayout[i], i.ToString()));
            }
        }

        /// <summary>
        /// 设置托盘的编号
        /// </summary>
        /// <param name="index">列表索引</param>
        /// <param name="trayNumber">托盘编号</param>
        public void SetTrayNumber(string trayNumber)
        {
            Trays[TrayIndex - 1].TrayNumber = trayNumber;
        }

        /// <summary>
        /// 设置指定编号的托盘内探测器的数据,参数sensorData包含了托盘探测器在盘中的位置
        /// </summary>
        /// <param name="sensorData">收到的探测器数据</param>
        public void SetSensorDataInTray(Sensor sensorData)
        {
            if (TrayIndex <= 0) return;
            Sensor sensor = Trays[TrayIndex - 1].Sensors[sensorData.PosInTray.ToString()];
            //SensorData sensor1 = Trays.Where((tray) => tray.TrayNumber == sensorData.TrayNumber).FirstOrDefault().Sensors[sensorData.PosInTray];
            sensor.SensorCode = sensorData.SensorCode;
            sensor.SensorType = sensorData.SensorType;
            sensor.TestStation = sensorData.TestStation;
            sensor.SensorQuality = sensorData.SensorQuality;
            
            sensor.Appearance = sensorData.Appearance;
            sensor.StartTime = sensorData.StartTime;
            sensor.EndTime = sensorData.EndTime;
        }

        /// <summary>
        /// 保存托盘数据
        /// </summary>
        public void SaveTraysData()
        {
            List<TrayData> traysData = new List<TrayData>();
            for (int i = 0; i < Trays.Count; i++)
            {
                traysData.Add(new TrayData(Trays[i]));
            }
            JsonManager.SaveJsonString(Environment.CurrentDirectory + "\\Cache", "TraysData", traysData);
        }

        /// <summary>
        /// 加载托盘数据，加载上次保存的托盘数据
        /// </summary>
        /// <param name="trayType">托盘类型</param>
        public void LoadTraysData(string trayType)
        {
            List<TrayData> trays = JsonManager.ReadJsonString<List<TrayData>>(Environment.CurrentDirectory + "\\Cache", "TraysData");
            if (trays != null)
            {
                Trays.Clear();
                foreach (TrayData trayData in trays)
                {
                    Trays.Add(new Tray(trayData));
                }
            }
            else
            {
                InitializeTrays(trayType);
            }
        }

        /// <summary>
        /// 更新托盘显示
        /// </summary>
        /// <param name="canvasControl">显示托盘的控件</param>
        public void UpdateTrayLabels(Control canvasControl)
        {
            WinformTool.InvokeOnThread(canvasControl, new Action(() => canvasControl.Controls.Clear()));
            foreach (var tray in Trays)
            {
                tray.UpdateTrayLabel(canvasControl);
            }
        }
    }
}
