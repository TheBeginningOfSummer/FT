using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NJCompolet;

namespace FT
{
   public class Communication
    {
        public static Communication singleton = new Communication();
        public NJCompoletLibrary compolet;
        public string Error;
        LogFile logfile = new LogFile();

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

        #region 写入PLC的数据
        ///// <summary>
        ///// 写PLCIO
        ///// </summary>
        //public bool[] WritePLCIO { get; set; }
        ///// <summary>
        ///// 写托盘与产品信息
        ///// </summary>
        //public int[] WriteProductionData { get; set; }
        ///// <summary>
        ///// 写标志位数组
        ///// </summary>
        //public bool[] WriteFlagBits { get; set; }
        /// <summary>
        /// 写运动参数
        /// </summary>
        public double[] WritePLCPmt { get; set; }
        #endregion

        private Communication()
        {
            compolet = CompoletSingleton.GetCompolet();

            ReadPLCIO = new bool[400];
            ReadLocation = new double[200];
            ReadFlagBits = new bool[100];
            ReadTestInformation = new string[100];
            ReadPLCAlarm = new bool[200];
            ReadPLCPmt = new double[50];

            //WritePLCIO = new bool[800];
            //WriteProductionData = new int[50];
            //WriteFlagBits = new bool[50];
            WritePLCPmt = new double[50];
        }

        public void RefreshData()
        {
            //打开通信端口
            //try
            //{
            //    this.compolet.Open();
            //}
            //catch (Exception ex)
            //{
            //    this.Error = ex.ToString();
            //    throw ex;
            //}

            //读取PLC地址
            try
            {
                #region 读IO信息
                ReadBoolVariables(ReadPLCIO, "PlcOutIO", 0, 111);
                ReadBoolVariables(ReadPLCIO, "PlcOutIO", 128, 135);
                ReadBoolVariables(ReadPLCIO, "PlcOutIO", 155, 162);
                #endregion

                #region 读位置信息
                for (int i = 0; i <= 180; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                #endregion

                #region 读报警信息
                ReadBoolVariables(ReadPLCAlarm, "PlcOutAlarm", 0, 88);
                ReadBoolVariables(ReadPLCAlarm, "PlcOutAlarm", 100, 158);
                ReadBoolVariables(ReadPLCAlarm, "PlcOutAlarm", 170, 173);

                #endregion

                #region 读参数信息
                for (int i = 0; i < 35; i++)
                {
                    ReadPLCPmt[i] = compolet.ReadVariableReal("PlcInPmt[" + i.ToString() + "]");
                }
                #endregion

                #region 读取测试信息（字符串变量）
                //当前控制器时间
                ReadTestInformation[29] = compolet.ReadVariableString("PLC测试信息[29]");
                //上视觉1扫码信息
                ReadTestInformation[30] = compolet.ReadVariableString("PLC测试信息[30]");
                //下视觉对位X
                ReadTestInformation[32] = compolet.ReadVariableString("PLC测试信息[32]");
                //下视觉对位Y
                ReadTestInformation[33] = compolet.ReadVariableString("PLC测试信息[33]");
                //下视觉对位θ
                ReadTestInformation[34] = compolet.ReadVariableString("PLC测试信息[34]");
                //上视觉2对位X
                ReadTestInformation[36] = compolet.ReadVariableString("PLC测试信息[36]");
                //上视觉2对位Y
                ReadTestInformation[37] = compolet.ReadVariableString("PLC测试信息[37]");
                //上视觉2对位θ
                ReadTestInformation[38] = compolet.ReadVariableString("PLC测试信息[38]");
                //计算对位X
                ReadTestInformation[39] = compolet.ReadVariableString("PLC测试信息[39]");
                //计算对位Y
                ReadTestInformation[40] = compolet.ReadVariableString("PLC测试信息[40]");
                //计算对位θ
                ReadTestInformation[41] = compolet.ReadVariableString("PLC测试信息[41]");
                #endregion
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }

            //关闭通信端口
            //compolet.Close();
        }

        public void RefreshCriticalData()
        {
            try
            {
                #region 从PLC读取标志位
                //托盘扫码完成
                ReadFlagBits[0] = compolet.ReadVariableBool("PLC标志位[0]");
                //探测器测试完成
                ReadFlagBits[1] = compolet.ReadVariableBool("PLC标志位[1]");
                //logfile.Writelog("PLC标志位[1]读取完成", "PLC标志位[1]记录");
                //20个托盘已摆好
                ReadFlagBits[2] = compolet.ReadVariableBool("PLC标志位[2]");
                #endregion

                #region 读取测试信息（字符串变量）
                //产品编码
                ReadTestInformation[0] = compolet.ReadVariableString("PLC测试信息[0]");
                //类型
                ReadTestInformation[1] = compolet.ReadVariableString("PLC测试信息[1]");
                //测试工位
                ReadTestInformation[2] = compolet.ReadVariableString("PLC测试信息[2]");
                //结果
                ReadTestInformation[3] = compolet.ReadVariableString("PLC测试信息[3]");
                //托盘编号
                ReadTestInformation[4] = compolet.ReadVariableString("PLC测试信息[4]");
                //托盘位置
                ReadTestInformation[5] = compolet.ReadVariableString("PLC测试信息[5]");
                //外观
                ReadTestInformation[6] = compolet.ReadVariableString("PLC测试信息[6]");
                //开始时间
                ReadTestInformation[7] = compolet.ReadVariableString("PLC测试信息[7]");
                //完成时间
                ReadTestInformation[8] = compolet.ReadVariableString("PLC测试信息[8]");
                //当前托盘索引
                ReadTestInformation[20] = compolet.ReadVariableString("PLC测试信息[20]");
                //良率
                ReadTestInformation[21] = compolet.ReadVariableString("PLC测试信息[21]");
                //探针次数
                ReadTestInformation[22] = compolet.ReadVariableString("PLC测试信息[22]");

                #endregion
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw;
            }
        }

        public void ReadBoolVariables(bool[] boolArray, string variableName, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                boolArray[i] = compolet.ReadVariableBool(variableName + "[" + i.ToString() + "]");
            }
        }

        public void WriteVariables<T>(T[] variableArray, string variableName, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                compolet.WriteVariable(variableName + "[" + i.ToString() + "]", variableArray[i]);
            }
        }

        public void WriteVariable<T>(T variable, string variableName)
        {
            try
            {
                compolet.WriteVariable(variableName, variable);
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }
        }

    }

}
