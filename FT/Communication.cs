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
        public double[] ReadPLCPmt { get; set; }
        #endregion

        #region 写入PLC的数据
        /// <summary>
        /// 写PLCIO
        /// </summary>
        public bool[] WritePLCIO { get; set; }
        /// <summary>
        /// 写托盘与产品信息
        /// </summary>
        public int[] WriteProductionData { get; set; }
        /// <summary>
        /// 写标志位数组
        /// </summary>
        public bool[] WriteFlagBits { get; set; }
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

            WritePLCIO = new bool[700];
            WriteProductionData = new int[50];
            WriteFlagBits = new bool[50];
            WritePLCPmt = new double[50];
        }

        public void RefreshData()
        {
            //打开通信端口
            try
            {
                this.compolet.Open();
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }

            //读取PLC地址
            try
            {
                #region 读IO信息
                ReadBoolVariables(ReadPLCIO, "PlcOutIO", 0, 258);
                #endregion

                #region 读位置信息
                //1-35
                for (int i = 0; i < 36; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                //41-52
                for (int i = 41; i < 53; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                ReadLocation[58] = compolet.ReadVariableReal("PlcOutLocation[58]");
                ReadLocation[59] = compolet.ReadVariableReal("PlcOutLocation[59]");
                ReadLocation[65] = compolet.ReadVariableReal("PlcOutLocation[65]");
                ReadLocation[69] = compolet.ReadVariableReal("PlcOutLocation[69]");
                ReadLocation[73] = compolet.ReadVariableReal("PlcOutLocation[73]");
                ReadLocation[77] = compolet.ReadVariableReal("PlcOutLocation[77]");
                ReadLocation[78] = compolet.ReadVariableReal("PlcOutLocation[78]");
                ReadLocation[79] = compolet.ReadVariableReal("PlcOutLocation[79]");
                //83-92
                for (int i = 83; i < 93; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                //97-101
                for (int i = 97; i < 102; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                //107-111
                for (int i = 107; i < 112; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                //117-121
                for (int i = 117; i < 122; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                //125-129
                for (int i = 125; i < 130; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                //133-137
                for (int i = 133; i < 138; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                //141-145
                for (int i = 141; i < 146; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                ReadLocation[149] = compolet.ReadVariableReal("PlcOutLocation[149]");
                ReadLocation[150] = compolet.ReadVariableReal("PlcOutLocation[150]");
                ReadLocation[159] = compolet.ReadVariableReal("PlcOutLocation[159]");
                ReadLocation[160] = compolet.ReadVariableReal("PlcOutLocation[160]");
                ReadLocation[164] = compolet.ReadVariableReal("PlcOutLocation[164]");
                ReadLocation[165] = compolet.ReadVariableReal("PlcOutLocation[165]");
                #endregion

                #region 从PLC读取标志位
                //托盘扫码完成
                ReadFlagBits[0] = compolet.ReadVariableBool("PLC标志位[0]");
                //探测器测试完成
                ReadFlagBits[1] = compolet.ReadVariableBool("PLC标志位[1]");
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
                #endregion

                #region 读报警信息
                ReadBoolVariables(ReadPLCAlarm, "PlcOutAlarm", 0, 2);
                ReadBoolVariables(ReadPLCAlarm, "PlcOutAlarm", 16, 87);
                ReadBoolVariables(ReadPLCAlarm, "PlcOutAlarm", 100, 117);
                ReadBoolVariables(ReadPLCAlarm, "PlcOutAlarm", 120, 155);
                #endregion
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }
            
            //写入PLC地址
            try
            {
                #region 写入IO信息
                WriteVariables(WritePLCIO, "PlcInIO", 0, 26);
                WriteVariables(WritePLCIO, "PlcInIO", 30, 43);
                WriteVariables(WritePLCIO, "PlcInIO", 45, 54);
                WriteVariables(WritePLCIO, "PlcInIO", 56, 57);
                WriteVariables(WritePLCIO, "PlcInIO", 61, 63);
                WriteVariables(WritePLCIO, "PlcInIO", 71, 73);
                WriteVariables(WritePLCIO, "PlcInIO", 76, 90);
                WriteVariables(WritePLCIO, "PlcInIO", 100, 107);
                WriteVariables(WritePLCIO, "PlcInIO", 110, 115);
                WriteVariables(WritePLCIO, "PlcInIO", 121, 125);
                WriteVariables(WritePLCIO, "PlcInIO", 131, 135);
                WriteVariables(WritePLCIO, "PlcInIO", 141, 145);
                WriteVariables(WritePLCIO, "PlcInIO", 151, 155);
                WriteVariables(WritePLCIO, "PlcInIO", 161, 162);
                WriteVariables(WritePLCIO, "PlcInIO", 166, 167);
                WriteVariables(WritePLCIO, "PlcInIO", 171, 172);
                WriteVariables(WritePLCIO, "PlcInIO", 176, 177);
                WriteVariables(WritePLCIO, "PlcInIO", 181, 182);
                WriteVariables(WritePLCIO, "PlcInIO", 186, 187);
                WriteVariables(WritePLCIO, "PlcInIO", 200, 212);
                WriteVariables(WritePLCIO, "PlcInIO", 220, 232);
                WriteVariables(WritePLCIO, "PlcInIO", 240, 244);
                WriteVariables(WritePLCIO, "PlcInIO", 250, 252);
                WriteVariables(WritePLCIO, "PlcInIO", 260, 269);
                WriteVariables(WritePLCIO, "PlcInIO", 275, 279);
                WriteVariables(WritePLCIO, "PlcInIO", 285, 289);
                WriteVariables(WritePLCIO, "PlcInIO", 295, 299);
                WriteVariables(WritePLCIO, "PlcInIO", 305, 309);
                WriteVariables(WritePLCIO, "PlcInIO", 315, 319);
                WriteVariables(WritePLCIO, "PlcInIO", 325, 329);
                WriteVariables(WritePLCIO, "PlcInIO", 335, 336);
                WriteVariables(WritePLCIO, "PlcInIO", 339, 340);
                WriteVariables(WritePLCIO, "PlcInIO", 343, 344);
                WriteVariables(WritePLCIO, "PlcInIO", 347, 348);
                WriteVariables(WritePLCIO, "PlcInIO", 351, 352);
                WriteVariables(WritePLCIO, "PlcInIO", 355, 356);
                WriteVariables(WritePLCIO, "PlcInIO", 360, 362);
                WriteVariables(WritePLCIO, "PlcInIO", 365, 367);
                WriteVariables(WritePLCIO, "PlcInIO", 370, 372);
                WriteVariables(WritePLCIO, "PlcInIO", 375, 377);
                WriteVariables(WritePLCIO, "PlcInIO", 380, 382);
                WriteVariables(WritePLCIO, "PlcInIO", 385, 387);
                WriteVariables(WritePLCIO, "PlcInIO", 390, 392);
                WriteVariables(WritePLCIO, "PlcInIO", 395, 397);
                WriteVariables(WritePLCIO, "PlcInIO", 400, 402);
                WriteVariables(WritePLCIO, "PlcInIO", 405, 407);
                WriteVariables(WritePLCIO, "PlcInIO", 410, 412);
                WriteVariables(WritePLCIO, "PlcInIO", 415, 417);
                WriteVariables(WritePLCIO, "PlcInIO", 420, 422);
                WriteVariables(WritePLCIO, "PlcInIO", 425, 427);
                WriteVariables(WritePLCIO, "PlcInIO", 430, 432);
                WriteVariables(WritePLCIO, "PlcInIO", 435, 437);
                WriteVariables(WritePLCIO, "PlcInIO", 440, 442);
                WriteVariables(WritePLCIO, "PlcInIO", 445, 447);
                WriteVariables(WritePLCIO, "PlcInIO", 450, 452);
                WriteVariables(WritePLCIO, "PlcInIO", 455, 457);
                WriteVariables(WritePLCIO, "PlcInIO", 460, 480);
                WriteVariables(WritePLCIO, "PlcInIO", 490, 574);
                #endregion

                #region 写入托盘产品信息
                WriteVariables(WriteProductionData, "PlcInID", 0, 1);
                #endregion

                #region 标志位写入PLC
                //托盘扫码记录完成
                compolet.WriteVariable("PC标志位[0]", WriteFlagBits[0]);
                //测试信息记录完成
                compolet.WriteVariable("PC标志位[1]", WriteFlagBits[1]);
                //托盘初始化完成
                compolet.WriteVariable("PC标志位[2]", WriteFlagBits[2]);
                #endregion

                #region 参数写入
                WriteVariables(WritePLCPmt, "PlcInPmt", 0, 9);
                WriteVariables(WritePLCPmt, "PlcInPmt", 15, 24);
                #endregion
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }

            //关闭通信端口
            compolet.Close();

            
        }

        public void ReadData()
        {
            //打开通信端口
            try
            {
                this.compolet.Open();
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }

            //读取PLC地址
            try
            {
                #region 读IO信息
                ReadBoolVariables(ReadPLCIO, "PlcOutIO", 0, 258);
                #endregion

                #region 读位置信息
                for (int i = 0; i < 170; i++)
                {
                    ReadLocation[i] = compolet.ReadVariableReal("PlcOutLocation[" + i.ToString() + "]");
                }
                #endregion

                #region 从PLC读取标志位
                //托盘扫码完成
                ReadFlagBits[0] = compolet.ReadVariableBool("PLC标志位[0]");
                //探测器测试完成
                ReadFlagBits[1] = compolet.ReadVariableBool("PLC标志位[1]");
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
                #endregion

                #region 读报警信息
                ReadBoolVariables(ReadPLCAlarm, "PlcOutAlarm", 0, 158);
                #endregion

                #region 读参数信息
                for (int i = 0; i < 25; i++)
                {
                    ReadPLCPmt[i] = compolet.ReadVariableReal("PlcInPmt[" + i.ToString() + "]");
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }
        }

        public void WriteData()
        {
            //写入PLC地址
            try
            {
                #region 写入IO信息
                WriteVariables(WritePLCIO, "PlcInIO", 0, 26);
                WriteVariables(WritePLCIO, "PlcInIO", 30, 43);
                WriteVariables(WritePLCIO, "PlcInIO", 45, 54);
                WriteVariables(WritePLCIO, "PlcInIO", 56, 57);
                WriteVariables(WritePLCIO, "PlcInIO", 61, 63);
                WriteVariables(WritePLCIO, "PlcInIO", 71, 73);
                WriteVariables(WritePLCIO, "PlcInIO", 76, 90);
                WriteVariables(WritePLCIO, "PlcInIO", 100, 107);
                WriteVariables(WritePLCIO, "PlcInIO", 110, 115);
                WriteVariables(WritePLCIO, "PlcInIO", 121, 125);
                WriteVariables(WritePLCIO, "PlcInIO", 131, 135);
                WriteVariables(WritePLCIO, "PlcInIO", 141, 145);
                WriteVariables(WritePLCIO, "PlcInIO", 151, 155);
                WriteVariables(WritePLCIO, "PlcInIO", 161, 162);
                WriteVariables(WritePLCIO, "PlcInIO", 166, 167);
                WriteVariables(WritePLCIO, "PlcInIO", 171, 172);
                WriteVariables(WritePLCIO, "PlcInIO", 176, 177);
                WriteVariables(WritePLCIO, "PlcInIO", 181, 182);
                WriteVariables(WritePLCIO, "PlcInIO", 186, 187);
                WriteVariables(WritePLCIO, "PlcInIO", 200, 212);
                WriteVariables(WritePLCIO, "PlcInIO", 220, 232);
                WriteVariables(WritePLCIO, "PlcInIO", 240, 244);
                WriteVariables(WritePLCIO, "PlcInIO", 250, 252);
                WriteVariables(WritePLCIO, "PlcInIO", 260, 269);
                WriteVariables(WritePLCIO, "PlcInIO", 275, 279);
                WriteVariables(WritePLCIO, "PlcInIO", 285, 289);
                WriteVariables(WritePLCIO, "PlcInIO", 295, 299);
                WriteVariables(WritePLCIO, "PlcInIO", 305, 309);
                WriteVariables(WritePLCIO, "PlcInIO", 315, 319);
                WriteVariables(WritePLCIO, "PlcInIO", 325, 329);
                WriteVariables(WritePLCIO, "PlcInIO", 335, 336);
                WriteVariables(WritePLCIO, "PlcInIO", 339, 340);
                WriteVariables(WritePLCIO, "PlcInIO", 343, 344);
                WriteVariables(WritePLCIO, "PlcInIO", 347, 348);
                WriteVariables(WritePLCIO, "PlcInIO", 351, 352);
                WriteVariables(WritePLCIO, "PlcInIO", 355, 356);
                WriteVariables(WritePLCIO, "PlcInIO", 360, 362);
                WriteVariables(WritePLCIO, "PlcInIO", 365, 367);
                WriteVariables(WritePLCIO, "PlcInIO", 370, 372);
                WriteVariables(WritePLCIO, "PlcInIO", 375, 377);
                WriteVariables(WritePLCIO, "PlcInIO", 380, 382);
                WriteVariables(WritePLCIO, "PlcInIO", 385, 387);
                WriteVariables(WritePLCIO, "PlcInIO", 390, 392);
                WriteVariables(WritePLCIO, "PlcInIO", 395, 397);
                WriteVariables(WritePLCIO, "PlcInIO", 400, 402);
                WriteVariables(WritePLCIO, "PlcInIO", 405, 407);
                WriteVariables(WritePLCIO, "PlcInIO", 410, 412);
                WriteVariables(WritePLCIO, "PlcInIO", 415, 417);
                WriteVariables(WritePLCIO, "PlcInIO", 420, 422);
                WriteVariables(WritePLCIO, "PlcInIO", 425, 427);
                WriteVariables(WritePLCIO, "PlcInIO", 430, 432);
                WriteVariables(WritePLCIO, "PlcInIO", 435, 437);
                WriteVariables(WritePLCIO, "PlcInIO", 440, 442);
                WriteVariables(WritePLCIO, "PlcInIO", 445, 447);
                WriteVariables(WritePLCIO, "PlcInIO", 450, 452);
                WriteVariables(WritePLCIO, "PlcInIO", 455, 457);
                WriteVariables(WritePLCIO, "PlcInIO", 460, 480);
                WriteVariables(WritePLCIO, "PlcInIO", 490, 574);
                #endregion

                #region 写入托盘产品信息
                WriteVariables(WriteProductionData, "PlcInID", 0, 1);
                #endregion

                #region 标志位写入PLC
                //托盘扫码记录完成
                compolet.WriteVariable("PC标志位[0]", WriteFlagBits[0]);
                //测试信息记录完成
                compolet.WriteVariable("PC标志位[1]", WriteFlagBits[1]);
                //托盘初始化完成
                compolet.WriteVariable("PC标志位[2]", WriteFlagBits[2]);
                #endregion

                #region 参数写入
                WriteVariables(WritePLCPmt, "PlcInPmt", 0, 9);
                WriteVariables(WritePLCPmt, "PlcInPmt", 15, 24);
                #endregion
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }

            //关闭通信端口
            compolet.Close();
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
    }

}
