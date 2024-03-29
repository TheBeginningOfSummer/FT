﻿using System;
using System.Collections;
using System.Collections.Generic;
using CIPCommunication;

namespace FT
{
    public class Communication
    {
        public static Communication Singleton = new Communication();
        public NJCompoletLibrary Compolet;
        readonly IComparer stringValueComparer = new StringValueComparer();

        #region 从PLC读取的数据
        /// <summary>
        /// 读到的PLCIO
        /// </summary>
        public bool[] ReadPLCIO { get; private set; }
        /// <summary>
        /// 读的标志位数组
        /// </summary>
        public bool[] ReadFlagBits { get; private set; }
        /// <summary>
        /// 读的测试信息
        /// </summary>
        public string[] ReadTestInformation { get; private set; }
        /// <summary>
        /// 读取运动参数
        /// </summary>
        public double[] ReadPLCPmt { get; private set; }
        #endregion

        #region 要读取的变量名
        readonly string[] plcOutIOName;
        readonly string[] plcOutLocationName;
        readonly string[] plcOutAlarmName;
        readonly string[] plcInPmtName;
        readonly string[] plcOutFlagName;
        readonly string[] plcTestInfoName;
        readonly string[] plcOutFWName;
        #endregion

        #region 读取到的哈希表
        public Hashtable PLCOutput;
        public Hashtable Location;
        public Hashtable Alarm;
        public Hashtable PLCPmt;
        public Hashtable FlagBits;
        public Hashtable TestInformation;
        public Hashtable PLCFW;
        #endregion

        private Communication()
        {
            Compolet = new NJCompoletLibrary();
            #region 初始化读取值存储数组
            ReadPLCIO = new bool[400];
            ReadFlagBits = new bool[100];
            ReadTestInformation = new string[100];
            ReadPLCPmt = new double[150];
            #endregion

            #region 初始化变量
            //PLC Out IO
            plcOutIOName = InitializeStringArray("PlcOutIO", 0, 299);
            PLCOutput = InitializeHashtable<bool>(plcOutIOName, false);
            //位置信息
            plcOutLocationName = InitializeStringArray("PlcOutLocation", 0, 199);
            Location = InitializeHashtable<double>(plcOutLocationName, 0);
            //报警信息
            plcOutAlarmName = InitializeStringArray("PlcOutAlarm", 0, 249);
            Alarm = InitializeHashtable<bool>(plcOutAlarmName, false);
            //参数信息
            plcInPmtName = InitializeStringArray("PlcInPmt", 0, 149);
            PLCPmt = InitializeHashtable<double>(plcInPmtName, 0);
            //标志位
            plcOutFlagName = InitializeStringArray("PLC标志位", 0, 2);
            FlagBits = InitializeHashtable<bool>(plcOutFlagName, false);
            //字符串信息
            plcTestInfoName = InitializeStringArray("PLC测试信息", 0, 59);
            TestInformation = InitializeHashtable<string>(plcTestInfoName, "noData");
            //复位信号
            plcOutFWName = InitializeStringArray("FW", 100, 150);
            PLCFW = InitializeHashtable<bool>(plcOutFWName, false);
            #endregion
        }

        /// <summary>
        /// 初始化一组连续的字符串数组
        /// </summary>
        /// <param name="mainValue">字符串值</param>
        /// <param name="start">开始的索引</param>
        /// <param name="end">结束的索引</param>
        /// <returns></returns>
        public static string[] InitializeStringArray(string mainValue, int start, int end)
        {
            List<string> arrayList = new List<string>();
            for (int i = start; i <= end; i++)
                arrayList.Add($"{mainValue}[{i}]");
            return arrayList.ToArray();
        }
        /// <summary>
        /// 以一个字符串数组为键，创建哈希表
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">作为键的字符串数组</param>
        /// <param name="value">初始值</param>
        /// <returns></returns>
        public static Hashtable InitializeHashtable<T>(string[] key, T value)
        {
            if (key == null) return null;
            Hashtable hashtable = new Hashtable();
            for (int i = 0; i < key.Length; i++)
            {
                hashtable.Add(key[i], value);
            }
            return hashtable;
        }

        /// <summary>
        /// 复制哈希表
        /// </summary>
        /// <param name="sourceTable">源表</param>
        /// <param name="targetTable">目标的表</param>
        public void GetValue<T>(Hashtable sourceTable, Hashtable targetTable)
        {
            foreach (DictionaryEntry keyValue in sourceTable)
            {
                if (targetTable.ContainsKey(keyValue.Key))
                    targetTable[keyValue.Key] = Convert.ChangeType(keyValue.Value, typeof(T));
            }
        }
        /// <summary>
        /// 将哈希表转化为按键排序的数组
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="sourceTable">源表</param>
        /// <param name="array">要更新的数组</param>
        public void HashtableToArray<T>(Hashtable sourceTable, T[] array)
        {
            if (sourceTable == null) return;
            string[] keys = new string[sourceTable.Count];
            sourceTable.Keys.CopyTo(keys, 0);
            if (array == null || array.Length < sourceTable.Count)
            {
                array = new T[sourceTable.Count];
            }
            sourceTable.Values.CopyTo(array, 0);
            Array.Sort(keys, array, stringValueComparer);
        }

        public void RefreshData()
        {
            try
            {
                #region 方式一更新数据:直接读
                GetValue<bool>(Compolet.GetHashtable(plcOutIOName), PLCOutput);
                GetValue<double>(Compolet.GetHashtable(plcOutLocationName), Location);
                GetValue<bool>(Compolet.GetHashtable(plcOutAlarmName), Alarm);
                GetValue<bool>(Compolet.GetHashtable(plcOutFWName), PLCFW);
                //GetValue(compolet.GetHashtable(plcInPmtName), PLCPmt);
                //GetValue(compolet.GetHashtable(plcOutFlagName), FlagBits);
                //GetValue(compolet.GetHashtable(plcTestInfoName), TestInformation);
                #endregion

                #region 方式二更新数据：排序
                HashtableToArray(Compolet.GetHashtable(plcOutIOName), ReadPLCIO);
                //HashtableToArray(Compolet.GetHashtable(plcOutLocationName), ReadLocation);
                //HashtableToArray(Compolet.GetHashtable(plcOutAlarmName), ReadPLCAlarm);
                HashtableToArray(Compolet.GetHashtable(plcInPmtName), ReadPLCPmt);
                #endregion

                #region 从PLC读取标志位
                //托盘扫码完成
                ReadFlagBits[0] = Compolet.ReadVariable<bool>("PLC标志位[0]");
                //探测器测试完成
                ReadFlagBits[1] = Compolet.ReadVariable<bool>("PLC标志位[1]");
                //20个托盘已摆好
                ReadFlagBits[2] = Compolet.ReadVariable<bool>("PLC标志位[2]");
                #endregion

                #region 读取测试信息（字符串变量）
                //产品编码
                ReadTestInformation[0] = Compolet.ReadVariable<string>("PLC测试信息[0]");
                //类型
                ReadTestInformation[1] = Compolet.ReadVariable<string>("PLC测试信息[1]");
                //测试工位
                ReadTestInformation[2] = Compolet.ReadVariable<string>("PLC测试信息[2]");
                //结果
                ReadTestInformation[3] = Compolet.ReadVariable<string>("PLC测试信息[3]");
                //托盘编号
                ReadTestInformation[4] = Compolet.ReadVariable<string>("PLC测试信息[4]");
                //托盘位置
                ReadTestInformation[5] = Compolet.ReadVariable<string>("PLC测试信息[5]");
                //外观
                ReadTestInformation[6] = Compolet.ReadVariable<string>("PLC测试信息[6]");
                //开始时间
                ReadTestInformation[7] = Compolet.ReadVariable<string>("PLC测试信息[7]");
                //完成时间
                ReadTestInformation[8] = Compolet.ReadVariable<string>("PLC测试信息[8]");
                //当前托盘索引
                ReadTestInformation[20] = Compolet.ReadVariable<string>("PLC测试信息[20]");
                //良率
                ReadTestInformation[21] = Compolet.ReadVariable<string>("PLC测试信息[21]");
                //探针次数
                ReadTestInformation[22] = Compolet.ReadVariable<string>("PLC测试信息[22]");
                //当前控制器时间
                ReadTestInformation[29] = Compolet.ReadVariable<string>("PLC测试信息[29]");
                //上视觉1扫码信息
                ReadTestInformation[30] = Compolet.ReadVariable<string>("PLC测试信息[30]");
                //下视觉对位X
                ReadTestInformation[32] = Compolet.ReadVariable<string>("PLC测试信息[32]");
                //下视觉对位Y
                ReadTestInformation[33] = Compolet.ReadVariable<string>("PLC测试信息[33]");
                //下视觉对位θ
                ReadTestInformation[34] = Compolet.ReadVariable<string>("PLC测试信息[34]");
                //上视觉2对位X
                ReadTestInformation[36] = Compolet.ReadVariable<string>("PLC测试信息[36]");
                //上视觉2对位Y
                ReadTestInformation[37] = Compolet.ReadVariable<string>("PLC测试信息[37]");
                //上视觉2对位θ
                ReadTestInformation[38] = Compolet.ReadVariable<string>("PLC测试信息[38]");
                //计算对位X
                ReadTestInformation[39] = Compolet.ReadVariable<string>("PLC测试信息[39]");
                //计算对位Y
                ReadTestInformation[40] = Compolet.ReadVariable<string>("PLC测试信息[40]");
                //计算对位θ
                ReadTestInformation[41] = Compolet.ReadVariable<string>("PLC测试信息[41]");
                //下视觉2对位X
                ReadTestInformation[42] = Compolet.ReadVariable<string>("PLC测试信息[42]");
                //下视觉2对位Y
                ReadTestInformation[43] = Compolet.ReadVariable<string>("PLC测试信息[43]");
                //下视觉2对位θ
                ReadTestInformation[44] = Compolet.ReadVariable<string>("PLC测试信息[44]");
                //钧舵1反馈字节
                ReadTestInformation[45] = Compolet.ReadVariable<string>("PLC测试信息[45]");
                //钧舵2反馈字节
                ReadTestInformation[46] = Compolet.ReadVariable<string>("PLC测试信息[46]");
                //钧舵3反馈字节
                ReadTestInformation[47] = Compolet.ReadVariable<string>("PLC测试信息[47]");
                //钧舵4反馈字节
                ReadTestInformation[48] = Compolet.ReadVariable<string>("PLC测试信息[48]");
                //上视觉1偏移X
                ReadTestInformation[49] = Compolet.ReadVariable<string>("PLC测试信息[49]");
                //上视觉1偏移Y
                ReadTestInformation[50] = Compolet.ReadVariable<string>("PLC测试信息[50]");
                //上视觉1偏移θ
                ReadTestInformation[51] = Compolet.ReadVariable<string>("PLC测试信息[51]");

                #endregion
            }
            catch (Exception e)
            {
                LogManager.WriteLog($"读取PLC数据。{e.Message}", LogType.Error);
            }
        }

        public bool WriteVariable<T>(T variable, string variableName)
        {
            try
            {
                bool result = Compolet.WriteVariable(variableName, variable);
                if (!result) LogManager.WriteLog($"参数{variableName}写入失败，检查连接状态。", LogType.Error);
                return result;
            }
            catch (Exception e)
            {
                LogManager.WriteLog($"参数{variableName}写入失败。{e.Message}", LogType.Error);
                return false;
            }
        }

        public bool WriteVariable<T>(string variable, string variableName)
        {
            try
            {
                T value = (T)Convert.ChangeType(variable, typeof(T));
                bool result = Compolet.WriteVariable(variableName, value);
                if (!result) LogManager.WriteLog($"参数{variableName}写入失败，检查连接状态。", LogType.Error);
                return result;
            }
            catch (Exception e)
            {
                LogManager.WriteLog($"参数{variableName}写入失败。{e.Message}", LogType.Error);
                return false;
            }
        }

    }

}
