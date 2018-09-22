using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    enum IndexType
    {
        head, end
    }

    static class FileExpand
    {
        static public void SaveData(this Image image, string fileName)
        {
            image.Save(fileName, ImageFormat.Jpeg);
        }

        static public void SaveData(this byte[] byteArray, string fileName)
        {
            File.WriteAllBytes(fileName, byteArray);
        }

        static public void WriteLineToFile(this string value, string fileName)
        {
            WriteLineToFile(value, fileName, IndexType.head);
        }

        static public void WriteLineToFile(this string value, string fileName, IndexType indexType)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate);

            StreamReader streamReader = new StreamReader(fileStream);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            fileStream.Close();

            fileStream = new FileStream(fileName, FileMode.OpenOrCreate);

            StreamWriter streamWriter = new StreamWriter(fileStream);

            switch (indexType)
            {
                case IndexType.head:
                    streamWriter.WriteLine(value);
                    streamWriter.Write(data);
                    break;
                case IndexType.end:
                    streamWriter.Write(data);
                    streamWriter.WriteLine(value);
                    break;
                default:
                    throw new Exception("error type");
            }

            streamWriter.Close();

            fileStream.Close();
        }
    }
}
