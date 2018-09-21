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
        const string appID = "En1GxY7XKxnLFZ469NJBAk2gUJrCsPEUdmKDgS6uzqRf";
        const string keyFD = "GkEJpaSAqMVEiggg6VBJVBTXCx5zf3V6Ro4QWCKt4xLQ";
        const string keyFR = "GkEJpaSAqMVEiggg6VBJVBU1rZ8hcTudSnb4gJqjkH4h";

        const string SqlString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DaySign.mdb";

        const string video = "Video.jpg";
        const string path = "Data";
        string Path
        {
            get
            {
                if (true)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    return path + "\\";
                }
            }
        }

        static Manager managerObj;
        static EmguCamera emguCamera;
        static Face_FD faceFD;
        static Face_FR faceFR;
        static bool isCreate = false;

        Manager() { }

        public static Manager GetManager()
        {
            if (!isCreate) //各类的初始化
            {
                managerObj = new Manager();
                faceFD = new Face_FD(); //负责人脸检测的类
                faceFD.InitialFaceEngine(appID, keyFD);
                faceFR = new Face_FR(); //负责人脸识别的类
                faceFR.InitialFaceEngine(appID, keyFR);
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
            if (byteData == null) return;
            Console.WriteLine("Start Compare");
            foreach (var item in Directory.GetFiles(path, "*.data"))
            {
                byte[] fileData = File.ReadAllBytes(item);
                float f = faceFR.CompareFace(byteData, fileData);
                Console.WriteLine("{0}:{1} {2}**{3}", item, f, byteData.Length, fileData.Length);
            }
        }

        public void SetVideoPhoto(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            File.Delete(video);
            pictureBox.Image = emguCamera.GetPhoto(video);
        }


    }
}
