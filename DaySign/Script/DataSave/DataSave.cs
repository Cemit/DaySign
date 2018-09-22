using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    abstract class DataSave
    {
        abstract public string[][] GetAllData();
        abstract public bool AddData(string[] head, string[] data);
    }
}
