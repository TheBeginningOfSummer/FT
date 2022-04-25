using FT.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT
{
    public class SensorDataManager
    {
        public static string ConnectionString = "Data Source=SensorData.db;Pooling=true;FailIfMissing=false";
        public static List<string> TableList = new List<string>();

        public static void InitializeDatabase()
        {
            //得到数据库表信息
            DataTable schemaTable = SQLiteTool.GetTableList(ConnectionString);
            for (int i = 0; i < schemaTable.Rows.Count; i++)
            {
                TableList.Add(schemaTable.Rows[i]["TABLE_NAME"].ToString());
            }
            //如果表不存在则创建
            if (!TableList.Contains("Sensors"))
            {
                string sql = "create table Sensors (ID INTEGER primary key autoincrement,编码 TEXT,类型 TEXT,测试工位 TEXT,测试结果 INTEGER,托盘编号 TEXT,位置 INTEGER,外观 TEXT,开始时间 TEXT,完成时间 TEXT)";
                SQLiteTool.ExecuteSQL(ConnectionString, sql);
            }
        }

        public static string AddCondition(string tableHeader, string codition)
        {
            return " and " + tableHeader + " = '" + codition + "'";
        }
        //向数据库添加数据
        public static bool AddSensor(SensorData sensor)
        {
            string sql = "insert into Sensors(编码,类型,测试工位,测试结果,托盘编号,位置,外观,开始时间,完成时间) " +
                "values(@编码,@类型,@测试工位,@测试结果,@托盘编号,@位置,@外观,@开始时间,@完成时间)";
            SQLiteParameter[] paras = new SQLiteParameter[]
            {
                new SQLiteParameter("@编码",sensor.SensorCode),
                new SQLiteParameter("@类型",sensor.SensorType),
                new SQLiteParameter("@测试工位",sensor.TestStation),
                new SQLiteParameter("@测试结果",sensor.SensorQuality),
                new SQLiteParameter("@托盘编号",sensor.TrayNumber),
                new SQLiteParameter("@位置",sensor.PosInTray),
                new SQLiteParameter("@外观",sensor.Appearance),
                new SQLiteParameter("@开始时间",sensor.StartTime),
                new SQLiteParameter("@完成时间",sensor.EndTime)
            };
            if (SQLiteTool.ExecuteSQL(ConnectionString, sql, paras))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //查询数据
        public static DataTable InquireSensor(string sensorCode, string sensorType, string minTime, string maxTime)
        {
            string sql;
            if (sensorCode == "" && sensorType == "")
            {
                //sql = "select * from Sensors where 开始时间 between datetime('now','start of day','-1 day') and datetime('now','start of day','1 day')";
                sql = "select * from Sensors where 开始时间 >='" + minTime + "' and 开始时间<='" + maxTime + "'";
            }
            else if (sensorCode != "" && sensorType == "")
            {
                sql = "select * from Sensors where 开始时间 >= '" + minTime + "' and 开始时间<= '" + maxTime + "'" + AddCondition("编码", sensorCode);
            }
            else if (sensorCode == "" && sensorType != "")
            {
                sql = "select * from Sensors where 开始时间 >= '" + minTime + "' and 开始时间<= '" + maxTime + "'" + AddCondition("类型", sensorType);
            }
            else
            {
                sql = "select * from Sensors where 开始时间 >= '" + minTime + "' and 开始时间<= '" + maxTime + "'" + AddCondition("编码", sensorCode) + AddCondition("类型", sensorType);
            }
            return SQLiteTool.ExecuteQuery(SensorDataManager.ConnectionString, sql).Tables[0];
        }
    }

    public class SQLiteTool
    {
        private static void SetCommand(SQLiteCommand command, SQLiteConnection connection, string sqlString, params SQLiteParameter[] parameters)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                command.Parameters.Clear();
                command.Connection = connection;
                command.CommandText = sqlString;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 30;
                if (parameters != null)
                {
                    foreach (SQLiteParameter parameter in parameters)
                    {
                        command.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        /// <summary>
        /// 查询数据库
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="sqlString">SQL命令</param>
        /// <param name="parameters">可选参数</param>
        /// <returns>所查询的数据</returns>
        public static DataSet ExecuteQuery(string connectionString, string sqlString, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    DataSet dataSet = new DataSet();
                    try
                    {
                        SetCommand(command, connection, sqlString, parameters);
                        SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(command);
                        dataAdapter.Fill(dataSet);
                        return dataSet;
                    }
                    catch (Exception)
                    {
                        return dataSet;
                    }
                }
            }
        }
        /// <summary>
        /// 数据库执行指令
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="sqlString">SQL命令</param>
        /// <param name="parameters">可选参数</param>
        /// <returns>是否成功</returns>
        public static bool ExecuteSQL(string connectionString, string sqlString, params SQLiteParameter[] parameters)
        {
            bool result = true;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    SetCommand(command, connection, sqlString, parameters);
                    SQLiteTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        result = false;
                    }
                }
            }
            return result;
        }

        public static DataTable GetTableList(string connectionString)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                DataTable schemaTable = connection.GetSchema("TABLES");
                schemaTable.Columns.Remove("TABLE_CATALOG");
                schemaTable.Columns.Remove("TABLE_SCHEMA");
                schemaTable.Columns["TABLE_NAME"].SetOrdinal(0);
                return schemaTable;
            }
        }
    }
}
