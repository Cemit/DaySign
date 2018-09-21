using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{

    struct FaceDataStruct //字段名需要和数据的头信息相同
    {
        public int _uid;
        public string _class;
        public string _name;
        public byte[] _face;
    }

    abstract class Data
    {
        abstract public object[] GetDatas();
    }

    class FaceData : Data
    {
        DataSave dataCtrl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">sql链接字符串</param>
        public FaceData(DataSave save)
        {
            dataCtrl = save;
        }

        public override object[] GetDatas()
        {
            string[][] obj = dataCtrl.GetAllData();
            if (obj.Length == 0) return null;
            string[] head = obj[0]; //第一行记录着表的头信息
            FieldInfo[] fieldInfos = typeof(FaceDataStruct).GetFields();
            int[] index = new int[fieldInfos.Length]; //记录枚举各字段是在obj数据中的几个字段
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                for (int j = 0; j < head.Length; j++)
                {
                    if (fieldInfos[i].Name == head[j])
                    {
                        index[i] = j;
                    }
                }
            }
            object[] retArray = new object[obj.Length - 1]; //返回结构体数组

            for (int i = 1; i < obj.Length; i++)
            {
                string[] item = obj[i]; //当前行的数据
                FaceDataStruct faceData = new FaceDataStruct();
                for (int j = 0; j < fieldInfos.Length; j++)
                {
                    fieldInfos[j].SetValue(faceData, item[index[j]]); //将数据传入结构体
                }
                retArray[i - 1] = faceData;
            }

            return retArray;
        }
    }
}
