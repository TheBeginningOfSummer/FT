using FT.Data;
using MyToolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FT
{
    public class TrayManager
    {
        public int TrayIndex;
        //托盘种类
        public Dictionary<string, TypeOfTray> TrayType;
        //Mapping图布局
        public List<Position> MappingLayout;
        //上料盘数据
        public List<Tray> Trays = new List<Tray>();

        LogFile logfile = new LogFile();

        public TrayManager()
        {
            try
            {
                //读取托盘种类配置文件
                TrayType = JsonManager.ReadJsonString<Dictionary<string, TypeOfTray>>(Environment.CurrentDirectory + "\\Configuration\\", "TypeOfTray");
                //读取Mapping图布局配置文件
                MappingLayout = JsonManager.ReadJsonString<List<Position>>(Environment.CurrentDirectory + "\\Configuration\\", "MappingLayout");
            }
            catch (Exception)
            {
                //logfile.Writelog("读取托盘配置数据：" + e.Message);
            }
        }

        /// <summary>
        /// 按托盘种类初始化托盘
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
        /// 设置托盘指定位置内探测器的数据
        /// </summary>
        /// <param name="sensorData">收到的探测器数据</param>
        public void SetSensorDataInTray(SensorData sensorData)
        {
            if (TrayIndex <= 0) return;
            SensorData sensor = Trays[TrayIndex - 1].Sensors[sensorData.PosInTray];
            //SensorData sensor1 = Trays.Where((tray) => tray.TrayNumber == sensorData.TrayNumber).FirstOrDefault().Sensors[sensorData.PosInTray];
            sensor.SensorCode = sensorData.SensorCode;
            sensor.SensorType = sensorData.SensorType;
            sensor.TestStation = sensorData.TestStation;
            sensor.SensorQuality = sensorData.SensorQuality;
            
            sensor.Appearance = sensorData.Appearance;
            sensor.StartTime = sensorData.StartTime;
            sensor.EndTime = sensorData.EndTime;
        }
    }
}
