using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FT
{
    class LogFile
    {

        public void Writelog(string strlog)
        {
            string strFilePath = AppDomain.CurrentDomain.BaseDirectory + "log";
            string strFileName = DateTime.Now.ToString("yyyyMMdd") + ".log";
            strFileName = strFilePath + "\\" + strFileName;
            if (!Directory.Exists(strFilePath))
            {
                Directory.CreateDirectory(strFilePath);
            }
            FileStream fs;
            StreamWriter sw;

            if (File.Exists(strFileName))
            {
                fs = new FileStream(strFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
            }
            sw = new StreamWriter(fs);
            sw.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "  " + strlog);
            sw.Close();
            fs.Close();
        }

    }
}