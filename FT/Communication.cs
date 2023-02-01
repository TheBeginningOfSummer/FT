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
        public string Error;
        readonly IComparer stringValueComparer = new StringValueComparer();
        readonly LogFile logfile = new LogFile();

        #region 从PLC读取的数据
        /// <summary>
        /// 读到的PLCIO
        /// </summary>
        public bool[] ReadPLCIO { get; private set; }
        /// <summary>
        /// 读的位置信息
        /// </summary>
        public double[] ReadLocation { get; private set; }
        /// <summary>
        /// 读的标志位数组
        /// </summary>
        public bool[] ReadFlagBits { get; private set; }
        /// <summary>
        /// 读的测试信息
        /// </summary>
        public string[] ReadTestInformation { get; private set; }
        /// <summary>
        /// 读报警信息
        /// </summary>
        public bool[] ReadPLCAlarm { get; private set; }
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
        #endregion

        #region 读取到的哈希表
        public Hashtable PLCIO;
        public Hashtable Location;
        public Hashtable Alarm;
        public Hashtable PLCPmt;
        public Hashtable FlagBits;
        public Hashtable TestInformation;
        #endregion

        private Communication()
        {
            Compolet = CompoletSingleton.GetCompolet();
            #region 初始化读取值存储数组
            ReadPLCIO = new bool[400];
            ReadLocation = new double[200];
            ReadFlagBits = new bool[100];
            ReadTestInformation = new string[100];
            ReadPLCAlarm = new bool[200];
            ReadPLCPmt = new double[50];
            #endregion
            #region 初始化变量
            //PLC Out IO
            plcOutIOName = InitializeStringArray("PlcOutIO", 0, 193);
            PLCIO = InitializeHashtable<bool>(plcOutIOName, false);
            //位置信息
            plcOutLocationName = InitializeStringArray("PlcOutLocation", 0, 180);
            Location = InitializeHashtable<double>(plcOutLocationName, 0);
            //报警信息
            plcOutAlarmName = InitializeStringArray("PlcOutAlarm", 0, 173);
            Alarm = InitializeHashtable<bool>(plcOutAlarmName, false);
            //参数信息
            plcInPmtName = InitializeStringArray("PlcInPmt", 0, 34);
            PLCPmt = InitializeHashtable<double>(plcInPmtName, 0);

            plcOutFlagName = InitializeStringArray("PLC标志位", 0, 2);
            FlagBits = InitializeHashtable<bool>(plcOutFlagName, false);

            plcTestInfoName = InitializeStringArray("PLC测试信息", 0, 41);
            TestInformation = InitializeHashtable<string>(plcTestInfoName, "noData");
            #endregion
        }

        public static string[] InitializeStringArray(string mainValue, int start, int end)
        {
            List<string> arrayList = new List<string>();
            for (int i = start; i <= end; i++)
                arrayList.Add($"{mainValue}[{i}]");
            return arrayList.ToArray();
        }

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

        public void GetValue(Hashtable sourceTable, Hashtable targetTable)
        {
            foreach (DictionaryEntry keyValue in sourceTable)
            {
                if (targetTable.ContainsKey(keyValue.Key))
                    targetTable[keyValue.Key] = keyValue.Value;
            }
        }

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
                #region 方式一更新数据
                //GetValue(compolet.GetHashtable(plcOutIOName), PLCIO);
                //GetValue(compolet.GetHashtable(plcOutLocationName), Location);
                //GetValue(compolet.GetHashtable(plcOutAlarmName), Alarm);
                //GetValue(compolet.GetHashtable(plcInPmtName), PLCPmt);
                //GetValue(compolet.GetHashtable(plcOutFlagName), FlagBits);
                //GetValue(compolet.GetHashtable(plcTestInfoName), TestInformation);
                #endregion

                #region 方式二更新数据
                HashtableToArray(Compolet.GetHashtable(plcOutIOName), ReadPLCIO);
                HashtableToArray(Compolet.GetHashtable(plcOutLocationName), ReadLocation);
                HashtableToArray(Compolet.GetHashtable(plcOutAlarmName), ReadPLCAlarm);
                HashtableToArray(Compolet.GetHashtable(plcInPmtName), ReadPLCPmt);
                #endregion

                #region 从PLC读取标志位
                //托盘扫码完成
                ReadFlagBits[0] = Compolet.ReadVariableBool("PLC标志位[0]");
                //探测器测试完成
                ReadFlagBits[1] = Compolet.ReadVariableBool("PLC标志位[1]");
                //20个托盘已摆好
                ReadFlagBits[2] = Compolet.ReadVariableBool("PLC标志位[2]");
                #endregion

                #region 读取测试信息（字符串变量）
                //产品编码
                ReadTestInformation[0] = Compolet.ReadVariableString("PLC测试信息[0]");
                //类型
                ReadTestInformation[1] = Compolet.ReadVariableString("PLC测试信息[1]");
                //测试工位
                ReadTestInformation[2] = Compolet.ReadVariableString("PLC测试信息[2]");
                //结果
                ReadTestInformation[3] = Compolet.ReadVariableString("PLC测试信息[3]");
                //托盘编号
                ReadTestInformation[4] = Compolet.ReadVariableString("PLC测试信息[4]");
                //托盘位置
                ReadTestInformation[5] = Compolet.ReadVariableString("PLC测试信息[5]");
                //外观
                ReadTestInformation[6] = Compolet.ReadVariableString("PLC测试信息[6]");
                //开始时间
                ReadTestInformation[7] = Compolet.ReadVariableString("PLC测试信息[7]");
                //完成时间
                ReadTestInformation[8] = Compolet.ReadVariableString("PLC测试信息[8]");
                //当前托盘索引
                ReadTestInformation[20] = Compolet.ReadVariableString("PLC测试信息[20]");
                //良率
                ReadTestInformation[21] = Compolet.ReadVariableString("PLC测试信息[21]");
                //探针次数
                ReadTestInformation[22] = Compolet.ReadVariableString("PLC测试信息[22]");
                //当前控制器时间
                ReadTestInformation[29] = Compolet.ReadVariableString("PLC测试信息[29]");
                //上视觉1扫码信息
                ReadTestInformation[30] = Compolet.ReadVariableString("PLC测试信息[30]");
                //下视觉对位X
                ReadTestInformation[32] = Compolet.ReadVariableString("PLC测试信息[32]");
                //下视觉对位Y
                ReadTestInformation[33] = Compolet.ReadVariableString("PLC测试信息[33]");
                //下视觉对位θ
                ReadTestInformation[34] = Compolet.ReadVariableString("PLC测试信息[34]");
                //上视觉2对位X
                ReadTestInformation[36] = Compolet.ReadVariableString("PLC测试信息[36]");
                //上视觉2对位Y
                ReadTestInformation[37] = Compolet.ReadVariableString("PLC测试信息[37]");
                //上视觉2对位θ
                ReadTestInformation[38] = Compolet.ReadVariableString("PLC测试信息[38]");
                //计算对位X
                ReadTestInformation[39] = Compolet.ReadVariableString("PLC测试信息[39]");
                //计算对位Y
                ReadTestInformation[40] = Compolet.ReadVariableString("PLC测试信息[40]");
                //计算对位θ
                ReadTestInformation[41] = Compolet.ReadVariableString("PLC测试信息[41]");



                #endregion

            }
            catch (Exception e)
            {
                logfile.WriteLog($"PLC数据读取。{e.Message}", "更新数据");
                this.Error = e.ToString();
            }
        }

        public void WriteVariables<T>(T[] variableArray, string variableName, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                Compolet.WriteVariable(variableName + "[" + i.ToString() + "]", variableArray[i]);
            }
        }

        public void WriteVariable<T>(T variable, string variableName)
        {
            try
            {
                Compolet.WriteVariable(variableName, variable);
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }
        }

    }

}
