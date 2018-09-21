using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    class EmguCamera
    {
        private VideoCapture capture;

        public EmguCamera()
        {
            capture = new VideoCapture();
        }

        public Image GetPhoto(string saveFileName)
        {
            Bitmap bitmap = capture.QueryFrame().Bitmap;
            bitmap.SaveData(saveFileName); //写入硬盘，避免内存出错
            return Image.FromFile(saveFileName);
        }

        public void Disposable()
        {
            capture.Dispose();
        }
    }
}
