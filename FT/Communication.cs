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
        public static Communication singleton = new Communication(); // 加一行

        public int[] ReadDataInt { get; private set; }
        public bool[] ReadDataBool { get; private set; }
        public string[] ReadDataString { get; private set; }
        public double[] ReadDataDouble { get; private set; }

        public int[] WriteData { get; set; }
        public bool[] WriteDataBool { get; set; }
        public string Error;
        public NJCompoletLibrary compolet;

        private Communication() 
        {
            ReadDataInt = new int[100];
            ReadDataBool = new bool[400];
            ReadDataString = new string[100];
            ReadDataDouble = new double[200];

            WriteData = new int[500];
            WriteDataBool = new bool[500];

            compolet = CompoletSingleton.GetCompolet();
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

            //读取PLC地址{td_PC_out}
            try
            {
                #region 整型变量
                ReadDataInt[0] = this.compolet.ReadVariableInt("PlcOutID[0]");
                #endregion
                #region 布尔变量
                //托盘扫码完成
                ReadDataBool[0] = compolet.ReadVariableBool("PLC标志位[0]");
                //探测器测试完成
                ReadDataBool[1] = compolet.ReadVariableBool("PLC标志位[1]");
                //20个托盘已摆好
                ReadDataBool[2] = compolet.ReadVariableBool("PLC标志位[2]");
                #endregion

                #region 测试信息，字符串变量
                //产品编码
                ReadDataString[0] = compolet.ReadVariableString("PLC测试信息[0]");
                //类型
                ReadDataString[1] = compolet.ReadVariableString("PLC测试信息[1]");
                //测试工位
                ReadDataString[2] = compolet.ReadVariableString("PLC测试信息[2]");
                //结果
                ReadDataString[3] = compolet.ReadVariableString("PLC测试信息[3]");
                //托盘编号
                ReadDataString[4] = compolet.ReadVariableString("PLC测试信息[4]");
                //托盘位置
                ReadDataString[5] = compolet.ReadVariableString("PLC测试信息[5]");
                //外观
                ReadDataString[6] = compolet.ReadVariableString("PLC测试信息[6]");
                //开始时间
                ReadDataString[7] = compolet.ReadVariableString("PLC测试信息[7]");
                //完成时间
                ReadDataString[8] = compolet.ReadVariableString("PLC测试信息[8]");
                //当前托盘索引
                ReadDataString[20] = compolet.ReadVariableString("PLC测试信息[20]");
                #endregion
            }
            catch (Exception ex)
            {
                this.Error = ex.ToString();
                throw ex;
            }

            //写入PLC地址{td_PC_in}
            try
            {

                compolet.WriteVariable("PlcInID[0]", WriteData[0]);

                #region 标志位写入PLC
                //托盘扫码记录完成
                compolet.WriteVariable("PC标志位[0]", WriteDataBool[0]);
                //测试信息记录完成
                compolet.WriteVariable("PC标志位[1]", WriteDataBool[1]);
                //托盘初始化完成
                compolet.WriteVariable("PC标志位[2]", WriteDataBool[2]);
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
    }

}
