using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    enum ErrorType
    {
        [Description("输入数据格式错误")]
        inputError
    }
    class Error : LogBase
    {
        static Error error = new Error(); 
        public override string FilePath => "ErrorLog.txt";

        static public void Log(ErrorType type)
        {
            error.AddLog(error.GetErrorClass(2), type.GetText());
        }

        static public void Log(string log)
        {
            error.AddLog(error.GetErrorClass(2), log);
        }

        public override void AddLog(ErrorClass errorClass, string log)
        {
            log = string.Format("{0} {1}.{2}:{3}", DateTime.Now.ToString(), errorClass.className, errorClass.frameName, log);
            log.WriteLineToFile(FilePath);
            Console.WriteLine(log);
        }
    }
}
