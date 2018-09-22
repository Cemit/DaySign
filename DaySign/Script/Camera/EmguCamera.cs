using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaySign
{
    class EmguCamera
    {
        private VideoCapture capture;
        int i = 0;

        public EmguCamera()
        {
            capture = new VideoCapture();
        }

        public Bitmap GetPhoto()
        {
            if (i++%128 == 0)
            {
                //capture.Dispose(); //释放一下资源
                //capture = new VideoCapture();
                GC.Collect();
            }

            //Console.WriteLine(i);
            //return capture.QueryFrame().Bitmap;
            Bitmap bitmap = new Bitmap(capture.QueryFrame().Bitmap);
            return bitmap;
            //bitmap.SaveData(saveFileName); //写入硬盘，避免内存出错
            //bitmap.Dispose();

        }

        public void Disposable()
        {
            capture.Dispose();
        }
    }
}
