#region Licence and Terms
// Accord.NET Extensions Framework
// https://github.com/dajuric/accord-net-extensions
//
// Copyright © Darko Jurić, 2014-2015 
// darko.juric2@gmail.com
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU Lesser General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU Lesser General Public License for more details.
// 
//   You should have received a copy of the GNU Lesser General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/lgpl.txt>.
//
#endregion

//#define FILE_CAPTURE //comment it to enable camera capture
//#define READFILE
#define AUTOINIT        // auto init detects if there is a existing xml file and read it, (build it if not)


using Accord.Extensions;
using Accord.Extensions.Math.Geometry;
using Accord.Extensions.Imaging;
using Accord.Extensions.Imaging.Algorithms.LINE2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Template = Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplate;
using TemplatePyramid = Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplatePyramid<Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplate>;

using DotImaging;
using DotImaging.Primitives2D;


namespace FastTemplateMatching
{
    public partial class FastTP : Form
    { 
        
        ImageStreamReader videoCapture;
        #region User interface...

        TemplateMatching TMP;
        List<TemplatePyramid> TemplPyrs = null;
        string fileName;

        public FastTP()
        {
            InitializeComponent();

            TemplPyrs = new List<TemplatePyramid>();

            TMP = new TemplateMatching();
            
            fileName = "you-win-for-now-.bmp";

            //TMP.initialize(fileName,80,20,3);
           
            try
            {
#if FILE_CAPTURE
                //string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
                videoCapture = new ImageDirectoryCapture(@"./", "TP.bmp");
#else
                videoCapture = new CameraCapture(1);
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            if (videoCapture is CameraCapture)
                (videoCapture as CameraCapture).FrameSize = new DotImaging.Primitives2D.Size(1600 / 2, 1200/ 2); //set new Size(0,0) for the lowest one

                      
            this.FormClosing += FastTP_FormClosing;
            Application.Idle += videoCapture_NewFrame;
            videoCapture.Open();
        }
        

        void videoCapture_NewFrame(object sender, EventArgs e)
        {

            TMP.TemplateCapture(ref TemplPyrs, videoCapture, this.pictureBox);

        }

        void FastTP_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoCapture != null)
                videoCapture.Dispose();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            TMP.TPCapture();

        }
        #endregion

    }
}
