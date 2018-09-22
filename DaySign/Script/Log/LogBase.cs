using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    struct ErrorClass
    {
        public string className; //类名
        public string frameName; //方法名
    }
    abstract class LogBase
    {
        protected ErrorClass GetErrorClass(int depth)
        {
            StackTrace trace = new StackTrace();
            StackFrame frame = trace.GetFrame(depth);
            MethodBase method = frame.GetMethod();
            string className = method.ReflectedType.Name;
            ErrorClass ret = new ErrorClass()
            {
                className = className,
                frameName = method.Name
            };
            return ret;
        }

        public abstract string FilePath { get; }

        public abstract void AddLog(ErrorClass errorClass, string log);

    }
}
