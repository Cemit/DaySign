using DaySign.Script.Expand;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DaySign
{
    public class Manager
    {
        static Manager managerObj; //单例
        static bool isCreate = false;

        const string APPID = "";
        const string KEYFD = "";
        const string KEYFR = "";
        const string SQLSTR = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DaySign.mdb";
        const string VIDEO_CACHE = "Video.jpg";
        const string PATH = "Data";
        string Path
        {
            get
            {
                if (true)
                {
                    if (!Directory.Exists(PATH))
                    {
                        Directory.CreateDirectory(PATH);
                    }
                    return PATH + "\\";
                }
            }
        }

        MRECT lastRect = new MRECT(); //上一个识别出来的矩形
        MRECT nextRect = new MRECT(); //下一个识别出来的矩形 
        int nullNextFace = 0;

        FaceDataStruct nowFace;

        PictureBox showPhotoBox;
        EmguCamera emguCamera; //emgu摄像机实例
        Face_FD faceFD; //寻找是否存在人脸
        Face_FR faceFR; //获取人脸信息，匹配人脸

        string sqlFormName = "Data";
        bool timerLock = false;
        bool faceTimerLock = true;
        bool compareLock = false;

        float compareEdge = 0.55f;
        int videoInterval;
        int faceInterval;


        Manager() { }

        public static Manager GetManager()
        {
            if (!isCreate) //各类的初始化
            {
                managerObj = new Manager();
                managerObj.faceFD = new Face_FD(); //负责人脸检测的类
                managerObj.faceFD.InitialFaceEngine(APPID, KEYFD);
                managerObj.faceFR = new Face_FR(); //负责人脸识别的类
                managerObj.faceFR.InitialFaceEngine(APPID, KEYFR);
                managerObj.emguCamera = new EmguCamera(); //摄像机类
                isCreate = true;
            }
            return managerObj;
        }

        public void ShowPicture(OpenFileDialog fileDialog, PictureBox pictureBox)
        {
            fileDialog.FileName = string.Empty;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                Image image = Image.FromFile(fileDialog.FileName);
                pictureBox.Image = image;
            }
            fileDialog.Dispose();
        }

        [Obsolete]
        public bool CheckFace(Bitmap photo, out AFD_Face face)
        {
            bool ret = faceFD.CheckFace(photo, out face);
            return ret;
        }

        [Obsolete]
        public bool CheckFace(PictureBox pictureBox)
        {
            return CheckFace(pictureBox, out AFD_Face face);
        }

        [Obsolete]
        public bool CheckFace(PictureBox pictureBox, out AFD_Face face)
        {
            face = new AFD_Face();
            if (!CheckImage(pictureBox)) return false;

            Bitmap bitmap = new Bitmap(pictureBox.Image);
            bool ret = faceFD.CheckFace(bitmap, out face);
            pictureBox.Image.Dispose();
            pictureBox.Image = bitmap;
            return ret;
        }

        public byte[] GetFaceData(AFD_FSDK_FACERES faceRes, IntPtr offInputPtr)
        {
            byte[] byteData = faceFR.GetFaceData(faceRes, offInputPtr);
            return byteData;
        }

        [Obsolete]
        public byte[] GetFaceData(PictureBox pictureBox)
        {
            if (!CheckImage(pictureBox)) return null;

            Bitmap bitmap = new Bitmap(pictureBox.Image);
            byte[] byteData = faceFR.GetFaceData(bitmap, faceFD);

            if (byteData == null) return null;

            return byteData;
        }

        [Obsolete]
        void GetFaceDataToFile(PictureBox pictureBox)
        {
            string fileName = Guid.NewGuid().ToString();
            fileName = Path + fileName;
            new Bitmap(pictureBox.Image).Save(fileName + ".jpg");
            GetFaceData(pictureBox).SaveData(fileName + ".data");
        }

        bool CheckImage(PictureBox pictureBox)
        {
            if (pictureBox.Image == null)
            {
                MessageBox.Show("摄像机错误！无法获取图像");
                Error.Log("摄像机错误！无法获取图像");
                timerLock = true; //一旦错误将高频发生，固将时钟停掉
                return false;
            }
            return true;
        }



        public bool CompareFace(PictureBox pictureBox, out FaceDataStruct suitFace)
        {
            return CompareFace(faceFR.GetFaceData(new Bitmap(pictureBox.Image), faceFD), out suitFace);
        }

        //匹配人脸
        public bool CompareFace(byte[] byteData, out FaceDataStruct suitFace)
        {
            suitFace = new FaceDataStruct();
            if (byteData == null)
            {
                Error.Log(ErrorType.inputError);
                return false;
            }
            //Console.WriteLine("Start Compare");

            FaceDataStruct[] sqlFaces = GetAllDataFormSQL();
            bool hasSuit = false;
            foreach (var item in sqlFaces)
            {
                Console.WriteLine(faceFR.CompareFace(byteData, item._face));
                if (faceFR.CompareFace(byteData, item._face) >= compareEdge)
                {
                    hasSuit = true;
                    suitFace = item;
                }
            }
            return hasSuit;
        }

        FaceDataStruct[] GetAllDataFormSQL()
        {
            AccessData data = new AccessData(SQLSTR, sqlFormName);
            FaceData faceData = new FaceData(data);
            FaceDataStruct[] faces = faceData.GetDatas();

            //foreach (var item in faces)
            //{
            //    //Console.WriteLine("{0}, {1}, {2}, {3}", item._uid, item._name, item._class, item._face);
            //}

            return faces;
        }

        [Obsolete]
        byte[][] GetAllDataFormFile()
        {
            List<byte[]> ret = new List<byte[]>();
            foreach (var item in Directory.GetFiles(PATH, "*.data"))
            {
                byte[] fileData = File.ReadAllBytes(item);
                ret.Add(fileData);
                //Console.WriteLine("{0}:{1} {2}**{3}", item, f, byteData.Length, fileData.Length);
            }
            return ret.ToArray();
        }

        public Bitmap GetVideoPhoto()
        {
            //File.Delete(VIDEO_CACHE);
            return emguCamera.GetPhoto();
        }

        [Obsolete]
        public void SetVideoPhoto(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            File.Delete(VIDEO_CACHE);
            Bitmap bitmap = new Bitmap(pictureBox.Image);
            emguCamera.GetPhoto();
        }

        //设置时钟事件
        public void SetShowPhotoTimerTick(Timer videoTimer, int videoInterval, Timer faceTimer, int faceInterval, PictureBox showPhotoBox)
        {
            this.showPhotoBox = showPhotoBox;

            this.videoInterval = videoInterval;
            videoTimer.Interval = videoInterval;
            videoTimer.Tick += VideoTimer_Tick;
            videoTimer.Enabled = true;

            this.faceInterval = faceInterval;
            faceTimer.Interval = faceInterval;
            faceTimer.Tick += FaceTimer_Tick;
            faceTimer.Enabled = true;

        }
            
        //刷新视频的事件
        private void VideoTimer_Tick(object sender, EventArgs e) 
        {
            if (timerLock) return;
            timerLock = true;
            //解除图片框的占用
            if (showPhotoBox.Image != null) showPhotoBox.Image.Dispose();
            Bitmap video = GetVideoPhoto(); //读取当前相机的照片

            //对矩形进行插值
            lastRect = LerpRect(lastRect, nextRect, videoInterval / faceInterval);

            if (isZeroRect(nextRect)) //为了显示更流畅，设置一个缓存时间
            {
                if (nullNextFace++%((faceInterval / videoInterval + 1) * 2)  == 0)
                {
                    lastRect = new MRECT();
                }
            }
            if (!isZeroRect(lastRect))
            {
                video = video.DrawRect(lastRect);
                string information = nowFace._class + " " + nowFace._name;
                video = video.DrawString(lastRect, information);
            }


            showPhotoBox.Image = video;
            timerLock = false;
            faceTimerLock = false;
        }

        //刷新脸的核心事件
        private void FaceTimer_Tick(object sender, EventArgs e)
        {
            if (faceTimerLock) return;
            if (timerLock) return;
            timerLock = true;
            Bitmap video = new Bitmap(showPhotoBox.Image);
            bool hasFace = faceFD.CheckFace(video, out AFD_FSDK_FACERES faceRes, out IntPtr offIntPtr, out IntPtr imageDataPtr); 

            lastRect = hasFace ? nextRect : lastRect; //刷新矩形位置
            nextRect = hasFace ? faceRes.DeIntPtr().rect : new MRECT();

            if (hasFace) //找到了脸
            {
                if (!compareLock)
                {
                    compareLock = true; //在下一张脸解锁

                    byte[] data = faceFR.GetFaceData(faceRes, offIntPtr);

                    if (CompareFace(data, out FaceDataStruct faceData))
                    {
                        nowFace = faceData;
                    }
                    else
                    {
                        compareLock = false;
                        nowFace = new FaceDataStruct();
                    }
                }
            }
            else
            {
                compareLock = false;
                nowFace = new FaceDataStruct();
            }
            timerLock = false;
            Marshal.FreeHGlobal(imageDataPtr); //释放内存
        }

        MRECT LerpRect(MRECT lastRect, MRECT nextRect, float value)
        {
            MRECT ret = new MRECT()
            {
                top = (int)(lastRect.top + (nextRect.top - lastRect.top) * value),
                bottom = (int)(lastRect.bottom + (nextRect.bottom- lastRect.bottom) * value),
                left = (int)(lastRect.left + (nextRect.left - lastRect.left) * value),
                right = (int)(lastRect.right + (nextRect.right - lastRect.right) * value)
            };
            return ret;
        }

        bool isZeroRect(MRECT rect)
        {
            return rect.top == 0 && rect.bottom == 0 && rect.left == 0 && rect.right == 0;
        }
    }
}
