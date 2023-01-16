using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMRON.Compolet.CIP;

namespace CIPCommunication
{
    public class NJCompoletLibrary
    {
        private CIPPortCompolet portCompolet;
        private NJCompolet compolet;

        public DateTime CurrentTime;
        public string PeerAddress;
        public int LocalPort;

        public NJCompoletLibrary(string peerAddress = "192.168.250.6", int localPort = 2)
        {
            PeerAddress = peerAddress;
            LocalPort = localPort;
        }

        /// <summary>
        /// 开启
        /// </summary>
        public void Open()
        {
            this.portCompolet = new CIPPortCompolet();
            this.compolet = new NJCompolet();

            if (!portCompolet.IsOpened(LocalPort))
            {
                try
                {
                    portCompolet.Open(LocalPort);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            if (portCompolet.IsOpened(LocalPort))
            {
                this.compolet.ConnectionType = ConnectionType.UCMM;
                this.compolet.DontFragment = false;
                this.compolet.HeartBeatTimer = 0;
                this.compolet.LocalPort = LocalPort;
                this.compolet.PeerAddress = PeerAddress;
                this.compolet.ReceiveTimeLimit = ((long)(750));
                this.compolet.OnHeartBeatTimer += new System.EventHandler(this.njCompolet1_OnHeartBeatTimer);

                this.compolet.Active = true;

                if (!this.compolet.IsConnected)
                {
                    throw new Exception("Connection failed !" + System.Environment.NewLine + "Please check PeerAddress." + this.compolet.PeerAddress);
                }
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            this.compolet.Active = false;

            if (!portCompolet.IsOpened(LocalPort))
                return;

            try
            {
                portCompolet.Close(LocalPort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void njCompolet1_OnHeartBeatTimer(object sender, System.EventArgs e)
        {
            if (!portCompolet.IsOpened(LocalPort)) return;
            try
            {
                DateTime date = (DateTime)this.compolet.ReadVariable("_CurrentTime");
                this.CurrentTime = date;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 读数据
        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="variableName">变量名</param>
        /// <returns></returns>
        public int ReadVariableInt(string variableName)//int
        {
            object out1;
            int out2;
            if (!portCompolet.IsOpened(LocalPort))
                return 0;
            else
            {
                out1 = this.compolet.ReadVariable(variableName);
                out2 = Convert.ToInt32(out1);
                return out2;
            }
        }

        public bool ReadVariableBool(string variableName)//Bool
        {
            object out1;
            bool out2;
            if (!portCompolet.IsOpened(LocalPort))
                return false;
            else
            {
                out1 = this.compolet.ReadVariable(variableName);
                out2 = Convert.ToBoolean(out1);
                return out2;
            }
        }

        public double ReadVariableReal(string variableName)//Real
        {
            object out1;
            double out2;
            if (!portCompolet.IsOpened(LocalPort))
                return 0;
            else
            {
                out1 = this.compolet.ReadVariable(variableName);
                out2 = Convert.ToDouble(out1);
                return out2;
            }
        }

        public string ReadVariableString(string variableName)//string
        {
            object out1;
            string out2;
            if (!portCompolet.IsOpened(LocalPort))
                return "0";
            else
            {
                out1 = this.compolet.ReadVariable(variableName);
                out2 = Convert.ToString(out1);
                return out2;
            }
        }
        /// <summary>
        /// 读多个数据
        /// </summary>
        /// <param name="variableNames">数据变量名</param>
        /// <returns></returns>
        public bool[] ReadVariablesBool(string[] variableNames)
        {
            if (!portCompolet.IsOpened(LocalPort)) return new bool[] { false };
            Hashtable hashtable = this.compolet.ReadVariableMultiple(variableNames);
            string[] keys = new string[hashtable.Count];
            bool[] values = new bool[hashtable.Count];
            hashtable.Keys.CopyTo(keys, 0);
            hashtable.Values.CopyTo(values, 0);
            Array.Sort(keys, values);
            return values;
        }

        public double[] ReadVariablesReal(string[] variableNames)
        {
            if (!portCompolet.IsOpened(LocalPort)) return new double[] { 0 };
            Hashtable hashtable = this.compolet.ReadVariableMultiple(variableNames);
            string[] keys = new string[hashtable.Count];
            double[] values = new double[hashtable.Count];
            hashtable.Keys.CopyTo(keys, 0);
            hashtable.Values.CopyTo(values, 0);
            Array.Sort(keys, values);
            return values;
        }

        public string[] ReadVariablesString(string[] variableNames)
        {
            if (!portCompolet.IsOpened(LocalPort)) return new string[] { "null" };
            Hashtable hashtable = this.compolet.ReadVariableMultiple(variableNames);
            string[] keys = new string[hashtable.Count];
            string[] values = new string[hashtable.Count];
            hashtable.Keys.CopyTo(keys, 0);
            hashtable.Values.CopyTo(values, 0);
            Array.Sort(keys, values);
            return values;
        }

        public Hashtable GetHashtable(string[] variableNames)
        {
            if (!portCompolet.IsOpened(LocalPort))
                return null;
            else
                return compolet.ReadVariableMultiple(variableNames);
        }

        public string[] GetVariablesKey(string[] variableNames)
        {
            if (!portCompolet.IsOpened(LocalPort)) return new string[] { "null" };
            Hashtable hashtable = this.compolet.ReadVariableMultiple(variableNames);
            string[] keys = new string[hashtable.Count];
            //string[] values = new string[hashtable.Count];
            hashtable.Keys.CopyTo(keys, 0);
            //hashtable.Values.CopyTo(values, 0);
            Array.Sort(keys);
            return keys;
        }

        public string[] GetVariablesValue(string[] variableNames)
        {
            if (!portCompolet.IsOpened(LocalPort)) return new string[] { "null" };
            Hashtable hashtable = this.compolet.ReadVariableMultiple(variableNames);
            string[] keys = new string[hashtable.Count];
            string[] values = new string[hashtable.Count];
            hashtable.Keys.CopyTo(keys, 0);
            hashtable.Values.CopyTo(values, 0);
            Array.Sort(keys, values);
            return values;
        }
        #endregion

        #region 写数据
        public void WriteVariable(string variableName, object writeData)
        {
            if (!portCompolet.IsOpened(LocalPort)) return;
            this.compolet.WriteVariable(variableName, writeData);
        }

        public void WriteVariable<T>(string variableName, T variable)
        {
            if (!portCompolet.IsOpened(LocalPort)) return;
            this.compolet.WriteVariable(variableName, variable);
        }
        #endregion

    }
}

