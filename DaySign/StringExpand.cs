using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    enum SplitTpye
    {
        [Description(null)]
        none,
        [Description(" ")]
        space,
        [Description(",")]
        comma, //逗号
        [Description(", ")]
        spaceAndComma //"a, b"
    }

    enum LeftCharType
    {
        [Description(null)]
        none,
        [Description("\"")]
        quote, //引号
        [Description("\'")]
        singleQuote, //单引号
        [Description("(")]
        bracket, //小括号
        [Description("{")]
        braces, //大括号
    }

    enum RightCharType
    {
        [Description(null)]
        none,
        [Description("\"")]
        quote, //引号
        [Description("\'")]
        singleQuote, //单引号
        [Description(")")]
        bracket, //小括号
        [Description("}")]
        braces, //大括号
    }

    static class StringExpand
    {
        public static string GetFormatString(this int number)
        {
            return GetFormatString(number, null, null, null);
        }

        public static string GetFormatString(this int number, SplitTpye splitTpye)
        {
            return GetFormatString(number, splitTpye.GetText(), null, null);
        }

        public static string GetFormatString(this int number, LeftCharType leftCharType, RightCharType rightCharType)
        {
            return GetFormatString(number, null, leftCharType.GetText(), rightCharType.GetText());
        }

        public static string GetFormatString(this int number, SplitTpye splitTpye, LeftCharType leftCharType, RightCharType rightCharType)
        {
            return GetFormatString(number, splitTpye.GetText(), leftCharType.GetText(), rightCharType.GetText());
        }

        public static string GetFormatString(this int number, string splitChar, string leftChar, string rightChar)
        {
            if (number < 1)
            {
                Error.Log(ErrorType.inputError);
                return null;
            }
            string ret = leftChar + "{0}" + rightChar;
            for (int i = 1; i < number; i++)
            {
                ret += splitChar + leftChar + "{" + i.ToString() + "}" + rightChar;
            }
            return ret;
        }

        public static string GetString(this byte[] b)
        {
            if (b == null)
            {
                Error.Log(ErrorType.inputError);
                return null;
            }
            string s = null;
            foreach (var item in b)
            {
                string hexItem = Convert.ToString(item, 16);
                hexItem = hexItem.Length == 1 ? "0" + hexItem : hexItem;
                s += hexItem;
                /*
                Console.WriteLine(item);
                string hexStr = hexItem.ToString();
                int decInt = Convert.ToInt32(hexStr, 16);
                Console.WriteLine(Convert.ToByte(decInt));
                */
            }
            return s;
        }

        //将十六进制文本转换为比特数组
        public static byte[] GetByte(this string str)
        {
            if (str.Length % 2 != 0)
            {
                Error.Log(ErrorType.inputError);
                return null;
            }
            byte[] ret = new byte[str.Length / 2];
            for (int i = 0, j = 0; i < str.Length; i += 2, j++)
            {
                string hexStr = str[i].ToString() + str[i + 1].ToString();
                int decInt = Convert.ToInt32(hexStr, 16);
                ret[j] = Convert.ToByte(decInt);
            }
            return ret;
        }
    } 
}
