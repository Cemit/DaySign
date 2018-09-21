using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    class AccessData : DataSave
    {
        OleDbConnection oleDb;
        string formName;

        /// <summary>
        /// 使用的数据库的第一行必须和数据库的字段以及欲返回结构体同名
        /// </summary>
        /// <param name="sqlString"></param>
        /// <param name="formName"></param>
        public AccessData(string sqlString, string formName) //构造函数
        {
            oleDb = new OleDbConnection(sqlString);
            oleDb.Open();
            this.formName = formName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="head">
        /// 传入结构体数据的字段名称，需要和结构体解析成object时的顺序一致。
        /// example: FaceDataStructHead[] = { _uid, _class, _name, _face };
        /// </param>
        /// <param name="data">example: FaceDataStructData[][] = { { 1, 15软件1, 杨东雄, **** } }</param>
        /// <returns></returns>
        public override bool AddData(string[] head, string[] data)
        {
            return AddData(head, new string[][] { data }, out int[] error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="head">
        /// 传入结构体数据的字段名称，需要和结构体解析成object时的顺序一致。
        /// example: FaceDataStructHead[] = { _uid, _class, _name, _face };
        /// </param>
        /// <param name="data">example: FaceDataStructData[][] = { { 1, 15软件1, 杨东雄, **** } }</param>
        /// <param name="error">返回输入数据中发生错误的下标</param>>
        /// <returns></returns>
        public bool AddData(string[] head, string[][] data, out int[] error)
        {
            bool ret = true;
            List<int> errorList = new List<int>();
            string formHead = string.Format(
                "(" + head.Length.GetFormatString(SplitTpye.comma) + ")",
                head);
            for (int i = 0; i < data.Length; i++)
            {
                string formatString = data[i].Length.
                        GetFormatString(SplitTpye.comma, LeftCharType.singleQuote, RightCharType.singleQuote);
                string lineData = string.Format(
                    "(" + formatString + ")",
                    data[i]
                    );
                Console.WriteLine(lineData);
                string sql = string.Format(
                    "insert into {0} {1} values {2}",
                    formName, formHead, lineData);
                OleDbCommand oleDbCommand = new OleDbCommand(sql, oleDb);
                int change = oleDbCommand.ExecuteNonQuery(); //返回被修改的数目
                Console.WriteLine(change);
                if (change == 0)
                {
                    ret = false;
                    errorList.Add(i);
                };
            }
            error = errorList.ToArray();
            return ret;
            //string sql = "insert into 表1 (昵称,账号) values ('LanQ','2545493686')";
        }

        public override string[][] GetAllData()
        {
            string sql = "select * from " + formName;
            OleDbDataAdapter dbDataAdapter = new OleDbDataAdapter(sql, oleDb); //创建适配对象
            DataTable dt = new DataTable(); //新建表对象
            dbDataAdapter.Fill(dt); //用适配对象填充表对象
            string[][] obj = new string[dt.Rows.Count][];
            int i = 0;
            foreach (DataRow item in dt.Rows)
            {
                int length = item.ItemArray.Length;
                string[] lineData = new string[length];
                for (int j = 0; j < length; j++)
                {
                    lineData[j] = item.ItemArray[j].ToString();
                }
                obj[i++] = lineData;
            }
            return obj;
        }
    }
}
