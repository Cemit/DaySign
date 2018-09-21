using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DaySign
{
    class Debug
    {
        static public void CreateFace()
        {
            IntPtr detectEngine = IntPtr.Zero;
            int detectSize = 40 * 1024 * 1024;
            int nScale = 50;
            int nMaxFaceNum = 10;
            string appId = "En1GxY7XKxnLFZ469NJBAk2ZJuay9ECAvFWswJC6enHM";
            string sdkFDKey = "45djczjVnWqAjhWMbcLZhid2MF4srtWEzKRk3JqcNDDL";
            IntPtr pMem = Marshal.AllocHGlobal(detectSize);
            //int retCode = FaceAPI.AFD_FSDK_InitialFaceEngine(appId, sdkFDKey, pMem, detectSize, ref detectEngine, (int)AFD_FSDK_OrientPriority.AFD_FSDK_OPF_0_HIGHER_EXT, nScale, nMaxFaceNum);
        }

        static public void AddData(byte[] faceByte)
        {
            AccessData accessData = new AccessData(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DaySign.mdb", "Data");
            /*
            public int _uid;
            public string _class;
            public string _name;
            public byte[] _face;
             * */
            string[] data = { "999", "15软件1", "LanQ", faceByte.GetString() };
            string[] head = { "_uid", "_class", "_name", "_face" };
            accessData.AddData(head, data);
        }

        static public void ErrorLog()
        {
            Error.Log("debug");
        }
    }
}
