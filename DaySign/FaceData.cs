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

    struct FaceDataString : IStringStruct //字段名需要和数据的头信息相同
    {
        public string _uid;
        public string _class;
        public string _name;
        public string _face;
    }

    class FaceData : Data
    {
        public FaceData(DataSave save) : base(save) { }

        public FaceDataStruct[] GetDatas()
        {
            object[] faceObjs = GetDatas(new FaceDataString());
            FaceDataString[] faceDatas = new FaceDataString[faceObjs.Length];
            //转换object数组为FaceDataString数组
            int i = 0;
            foreach (var item in faceObjs)
            {
                faceDatas[i++] = (FaceDataString)item;
            }
            FaceDataStruct[] ret = new FaceDataStruct[faceObjs.Length];
            //转换FaceDataString数组为FaceDataStruct数组
            i = 0;
            foreach (var item in faceDatas)
            {
                ret[i++] = new FaceDataStruct()
                {
                    _uid = Convert.ToInt32(item._uid),
                    _class = item._class,
                    _name = item._name,
                    _face = item._face.GetByte()
                };
            }
            return ret;
        }
    }

}
