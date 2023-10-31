using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FT
{
    public partial class TestForm : Form
    {
        public AutoResetEvent AutoSwitch = new AutoResetEvent(false);
        readonly Communication updater = Communication.Singleton;
        readonly Stopwatch stopwatch = new Stopwatch();
        readonly List<Label> labels1 = new List<Label>();
        readonly List<Label> labels2 = new List<Label>();

        string[] variableNames;
        bool isUpdate = false;
        //CancellationTokenSource cancelUpdate = new CancellationTokenSource();

        public TestForm()
        {
            InitializeComponent();

            CB_VariableName.Items.Add(nameof(updater.PlcOutIO));
            CB_VariableName.Items.Add(nameof(updater.PlcOutLocation));
            CB_VariableName.Items.Add(nameof(updater.PlcOutAlarm));
            CB_VariableName.Items.Add(nameof(updater.PlcInPmt));
            CB_VariableName.Items.Add(nameof(updater.PLC标志位));
            CB_VariableName.Items.Add(nameof(updater.PLC测试信息));
        }

        #region 界面更新方法
        public void InitializeDisplayLabel(Control control, int labelCount, List<Label> labels1, List<Label> labels2)
        {
            control.Controls.Clear();
            labels1.Clear();
            labels2.Clear();
            List<Point> points = Data.WinformTool.SetLocation(10, 10, labelCount, 9, 120, 50);
            for (int i = 0; i < labelCount; i++)
            {
                labels1.Add(new Label() { Width = 120, Location = points[i] });
            }
            foreach (var item in labels1)
            {
                control.Controls.Add(item);
            }
            for (int i = 0; i < labelCount; i++)
            {
                labels2.Add(new Label() { Width = 120, Location = new Point(points[i].X, points[i].Y + 25) });
            }
            foreach (var item in labels2)
            {
                control.Controls.Add(item);
            }
        }

        public string VariableNameConvert(string name,out Type type)
        {
            switch (name)
            {
                case "PlcOutIO":
                    type = typeof(bool);
                    return "ReadPLCIO";
                case "PlcOutLocation":
                    type = typeof(double);
                    return "ReadLocation";
                case "PlcOutAlarm":
                    type = typeof(bool);
                    return "ReadPLCAlarm";
                case "PlcInPmt":
                    type = typeof(double);
                    return "ReadPLCPmt";
                case "PLC标志位":
                    type = typeof(bool);
                    return "ReadFlagBits";
                case "PLC测试信息":
                    type = typeof(string);
                    return "ReadTestInformation";
                default:
                    type = typeof(string);
                    return null;
            }

        }

        public void InterfaceUpdate<T>(string variable)
        {
            while (isUpdate)
            {
                try
                {
                    stopwatch.Restart();
                    Thread.Sleep(100);
                    if (!ShowData(variableNames, labels1)) break;
                    if (!ShowData((T[])updater.GetType().GetField(variable).GetValue(updater), labels2)) break;
                    stopwatch.Stop();
                    Data.WinformTool.InvokeOnThread(LB_Interval, new Action(() => LB_Interval.Text = $"{stopwatch.ElapsedMilliseconds}ms"));
                }
                catch (Exception e)
                {
                    Data.WinformTool.InvokeOnThread(LB_Interval, new Action(() => LB_Interval.Text = e.Message));
                }
            }
        }

        public void InterfaceUpdate(string[] variableNames)
        {
            while (isUpdate)
            {
                try
                {
                    stopwatch.Restart();
                    Thread.Sleep(100);
                    ShowData(updater.Compolet.ReadVariablesKeyArray(variableNames), labels1);
                    ShowData(updater.Compolet.ReadVariablesValueArray(variableNames), labels2);
                    stopwatch.Stop();
                    Data.WinformTool.InvokeOnThread(LB_Interval, new Action(() => LB_Interval.Text = $"{stopwatch.ElapsedMilliseconds}ms"));
                }
                catch (Exception e)
                {
                    Data.WinformTool.InvokeOnThread(LB_Interval, new Action(() => LB_Interval.Text = e.Message));
                }
            }
        }

        public bool ShowData<T>(T[] array, List<Label> labels)
        {
            if (array == null) return false;
            if (labels == null) return false;
            if (labels.Count == 0) return false;
            //由于下拉列表改变时，标签列表没有更改，标签与所显示的数组数量不匹配
            //且因为是用标签数量来遍历显示，所以当标签列表还未更新时，显示的数组小于标签数量时，
            //数组访问会超数组界限
            if (labels.Count > array.Length) return false;
            for (int i = 0; i < labels.Count; i++)
            {
                if (array[i] == null) continue;
                labels[i].Invoke(new Action(() => labels[i].Text = array[i].ToString()));
            }
            return true;
        }

        public void ShowData(object[] array, List<Label> labels)
        {
            if (array == null) return;
            for (int i = 0; i < labels.Count; i++)
                labels[i].Invoke(new Action(() => labels[i].Text = array[i].ToString()));
        }
        #endregion

        #region 测试用代码
        private void Test(int trayAmount, int trayCapacity)
        {
            Task.Run(() =>
            {
                for (int i = 1; i <= trayAmount; i++)
                {
                    updater.WriteVariable(i.ToString(), "PLC测试信息[20]");
                    for (int j = 1; j <= trayCapacity; j++)
                    {
                        updater.WriteVariable(j.ToString(), "PLC测试信息[5]");
                        updater.WriteVariable(true, "PLC标志位[1]");
                        AutoSwitch.WaitOne();
                    }
                }
            });
        }
        #endregion

        private void BTN_Start_Click(object sender, EventArgs e)
        {
            try
            {
                if (variableNames == null) return;
                string variable = VariableNameConvert(CB_VariableName.Text, out Type type);
                if (variable == null) return;
                InitializeDisplayLabel(PN_TestFormData, variableNames.Length, labels1, labels2);
                isUpdate = true;
                if (type == typeof(bool))
                {
                    Task.Run(() => InterfaceUpdate<bool>(variable));
                }
                else if (type == typeof(string))
                {
                    Task.Run(() => InterfaceUpdate<string>(variable));
                }
                else if (type == typeof(double))
                {
                    Task.Run(() => InterfaceUpdate<double>(variable));
                }
                BTN_Start.Enabled = false;
                BTN_Stop.Enabled = true;
            }
            catch (Exception)
            {

            }
        }

        private void BTN_Stop_Click(object sender, EventArgs e)
        {
            isUpdate = false;
            PN_TestFormData.Controls.Clear();
            BTN_Start.Enabled = true;
            BTN_Stop.Enabled = false;
        }

        private void BNT_WriteData_Click(object sender, EventArgs e)
        {
            try
            {
                if (TB_VariableIndex.Text == "") return;
                if (TB_Variable.Text == "") return;
                //string variableName = CB_VariableName.Text.Replace("Name", "");
                string variableName = CB_VariableName.Text;
                switch (CB_VariableName.Text)
                {
                    case nameof(updater.PlcOutIO):
                        if (TB_Variable.Text == "0")
                            updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", false);
                        if (TB_Variable.Text == "1")
                            updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", true);
                        break;
                    case nameof(updater.PlcOutLocation):
                        updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", double.Parse(TB_Variable.Text));
                        break;
                    case nameof(updater.PlcOutAlarm):
                        if (TB_Variable.Text == "0")
                            updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", false);
                        if (TB_Variable.Text == "1")
                            updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", true);
                        break;
                    case nameof(updater.PlcInPmt):
                        updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", double.Parse(TB_Variable.Text));
                        break;
                    case nameof(updater.PLC标志位):
                        if (TB_Variable.Text == "0")
                            updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", false);
                        if (TB_Variable.Text == "1")
                            updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", true);
                        break;
                    case nameof(updater.PLC测试信息):
                        updater.Compolet.WriteVariable($"{variableName}[{TB_VariableIndex.Text}]", TB_Variable.Text);
                        break;
                    default:
                        variableNames = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("写入失败。" + ex.Message);
            }

        }

        private void CB_VariableName_SelectedIndexChanged(object sender, EventArgs e)
        {
            variableNames = (string[])updater.GetType().GetField(CB_VariableName.Text).GetValue(updater);
            isUpdate = false;
            PN_TestFormData.Controls.Clear();
            BTN_Start.Enabled = true;
            BTN_Stop.Enabled = false;
        }

        private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            isUpdate = false;
        }

        private void BTN_Test_Click(object sender, EventArgs e)
        {
            //BTN_Test.Enabled = false;
            Test(20, 40);
        }
    }
}
