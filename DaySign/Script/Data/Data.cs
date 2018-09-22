using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    abstract class Data
    {
        DataSave dataCtrl;

        public Data(DataSave save)
        {
            dataCtrl = save;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="structType">
        /// 为方便反射，需要提交一个需求结构体的姊妹结构体，继承IStringStruct。
        /// 要求字段名相同，但所有的字段类型都为string
        /// </param>
        /// <returns></returns>
        public object[] GetDatas(IStringStruct stringStruct)
        {
            Type structType = stringStruct.GetType();
            string[][] obj = dataCtrl.GetAllData();
            if (obj.Length == 0)
            {
                Error.Log(ErrorType.readNull);
                return null;
            }
            string[] head = obj[0]; //第一行记录着表的头信息
            FieldInfo[] fieldInfos = structType.GetFields();
            int[] index = new int[fieldInfos.Length]; //记录枚举各字段是在obj数据中的几个字段
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                for (int j = 0; j < head.Length; j++)
                {
                    if (fieldInfos[i].Name == head[j])
                    {
                        index[i] = j;
                    }
                    if (fieldInfos[i].FieldType != typeof(string))
                    {
                        Error.Log(ErrorType.inputError,
                            "为方便反射，需要提交一个需求结构体的姊妹结构体。" +
                            "要求字段名相同，但所有的字段类型都为string。");
                        return null;
                    }
                }
            }
            object[] retArray = new object[obj.Length - 1]; //返回结构体数组

            for (int i = 1; i < obj.Length; i++)
            {
                string[] item = obj[i]; //当前行的数据
                object faceData = Activator.CreateInstance(structType);
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
