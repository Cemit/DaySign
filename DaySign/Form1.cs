﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DaySign
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Manager.GetManager().ShowPicture(openFileDialog1, pictureBox1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Manager.GetManager().CheckFace(pictureBox1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Manager.GetManager().GetFaceData(pictureBox1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Manager.GetManager().CompareFace(pictureBox1);
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Manager.GetManager().SetShowPhotoTimerTick(videoTimer, 33, faceTimer, 100, pictureBox1);
        }
    }
}
