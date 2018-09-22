using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    class Log : LogBase
    {
        static Log logObj = new Log();
        public override string FilePath => "Log.txt";

        static public void AddLog(string log)
        {
            logObj.AddLog(logObj.GetErrorClass(2), log);
        }

        public override void AddLog(ErrorClass errorClass, string log)
        {
            log = string.Format("{0} {1}.{2}:{3}", DateTime.Now.ToString(), errorClass.className, errorClass.frameName, log);
            log.WriteLineToFile(FilePath);
        }
    }
}
