using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DaySign.Script.Expand
{
    static class ConvertExpand
    {
        public static AFD_Face DeIntPtr(this AFD_FSDK_FACERES face)
        {
            AFD_Face ret = new AFD_Face()
            {
                faceNumber = face.nFace,
                faceOrient = (int)Marshal.PtrToStructure(face.lfaceOrient, typeof(int)),
                rect = (MRECT)Marshal.PtrToStructure(face.rcFace, typeof(MRECT))
            };
            return ret;
        }
    }
}
