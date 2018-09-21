using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DaySign
{
    public class Manager
    {
        const string APPID = "En1GxY7XKxnLFZ469NJBAk2gUJrCsPEUdmKDgS6uzqRf";
        const string KEYFD = "GkEJpaSAqMVEiggg6VBJVBTXCx5zf3V6Ro4QWCKt4xLQ";
        const string KEYFR = "GkEJpaSAqMVEiggg6VBJVBU1rZ8hcTudSnb4gJqjkH4h";
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

        static PictureBox showPhotoBox;
        static EmguCamera emguCamera;
        static Manager managerObj;
        static Face_FD faceFD;
        static Face_FR faceFR;

        static string sqlFormName = "Data";
        static bool isCreate = false;
        static bool timerStop = false;
        static int timerInterval = 100; 

        Manager() { }

        public static Manager GetManager()
        {
            if (!isCreate) //各类的初始化
            {
                managerObj = new Manager();
                faceFD = new Face_FD(); //负责人脸检测的类
                faceFD.InitialFaceEngine(APPID, KEYFD);
                faceFR = new Face_FR(); //负责人脸识别的类
                faceFR.InitialFaceEngine(APPID, KEYFR);
                emguCamera = new EmguCamera(); //摄像机类
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

        public bool CheckFace(PictureBox pictureBox)
        {

            if (pictureBox.Image == null)
            {
                MessageBox.Show("还没有打开文件！");
                return false;
            }
            Bitmap bitmap = new Bitmap(pictureBox.Image);
            bool ret = faceFD.CheckFace(ref bitmap);
            pictureBox.Image.Dispose();
            pictureBox.Image = bitmap;
            return ret;
        }

        public byte[] GetFaceData(PictureBox pictureBox)
        {
            if (pictureBox.Image == null)
            {
                MessageBox.Show("还没有打开文件！");
                return null;
            }
            Bitmap bitmap = new Bitmap(pictureBox.Image);
            byte[] byteData = faceFR.GetFaceData(bitmap, faceFD);
            if (byteData == null)
            {
                return null;
            }
            string fileName = Guid.NewGuid().ToString();
            fileName = Path + fileName;
            bitmap.Save(fileName + ".jpg");
            byteData.SaveData(fileName + ".data");
            return byteData;
        }

        public void CompareFace(PictureBox pictureBox)
        {
            Bitmap bitmap = new Bitmap(pictureBox.Image);
            byte[] byteData = faceFR.GetFaceData(bitmap, faceFD);
            if (byteData == null)
            {
                Log.AddLog("null face data");
                return;
            }
            //Console.WriteLine("Start Compare");

            FaceDataStruct[] sqlFaces = GetAllDataFormSQL();
            FaceDataStruct suitFace = new FaceDataStruct();
            bool hasSuit = false;
            foreach (var item in sqlFaces)
            {
                Console.WriteLine(faceFR.CompareFace(byteData, item._face));
                if (faceFR.CompareFace(byteData, item._face) >= 0.55f)
                {
                    hasSuit = true;
                    suitFace = item;
                }
            }

            if (hasSuit)
            {
                Console.WriteLine("{0} {1} {2}", suitFace._name, suitFace._uid, suitFace._class);
            }
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

        public void SetVideoPhoto(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            File.Delete(VIDEO_CACHE);
            pictureBox.Image = emguCamera.GetPhoto(VIDEO_CACHE);
        }

        public void SetShowPhotoTimerTick(Timer timer, PictureBox showPhotoBox)
        {
            Manager.showPhotoBox = showPhotoBox;
            timer.Interval = timerInterval;
            timer.Tick += Timer_Tick;
            timer.Enabled = true;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timerStop) return;

            GetManager().SetVideoPhoto(showPhotoBox);

            bool hasFace = GetManager().CheckFace(showPhotoBox);
            if (hasFace)
            {
                timerStop = true;
                byte[] data = GetManager().GetFaceData(showPhotoBox);
                if (data != null)
                {
                    //Debug.AddData(data);
                    CompareFace(showPhotoBox);
                }
                else
                {
                    timerStop = false;
                }
            }
        }
    }
}
