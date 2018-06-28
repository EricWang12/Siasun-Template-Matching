﻿//#define FILE_CAPTURE //comment it to enable camera capture
#define READFILE
//#define AUTOINIT        // auto init detects if there is a existing xml file and read it, (build it if not)
//#define runXML

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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Template = Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplate;
using TemplatePyramid = Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplatePyramid<Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplate>;
using DotImaging;
using DotImaging.Primitives2D;


namespace FastTemplateMatching
{
    public class TemplateMatching 
    {
        public int threshold = 80;

        int minDetectionsPerGroup = 0; //for match grouping (postprocessing)

        public List<TemplatePyramid> templPyrs;

        private int totalAngles = 360;
        private int totalSizes = 1;

        private LinearizedMapPyramid linPyr = null;
        // Find Object in a preprocessed img

        public TemplateMatching() { }

        #region initialize with local file

        /// <summary>
        /// Switch between two loading methods. 
        /// Creating a template from files or by deserializing.
        /// </summary
        public void initialize(String fileName)
        {
#if AUTOINIT
            try
            {
                Console.WriteLine("Reading from existing Template Data");
                templPyrs = fromXML(fileName);
            }
            catch (Exception)
            {
                Console.WriteLine("\nTemplate NOT found! ");
                templPyrs = fromFiles(fileName);
            }
            Console.WriteLine("COMPLETE!");
#else
#if READFILE
            templPyrs = fromFiles(fileName,true);
#else
            templPyrs = fromXML(fileName);
#endif

#endif

        }
        /// <summary>
        /// Initialize with a threshold setting
        /// </summary
        public void initialize(String fileName, int inputThreshold)
        {
            initialize(fileName);
            threshold = inputThreshold;
        }
        /// <summary>
        /// Initialize with a threshold setting and Angle Number setting
        /// </summary
        public void initialize(String fileName, float angleGap)
        {
            initialize(fileName);
            totalAngles = (int)(360 / angleGap);
        }
        /// <summary>
        /// Initialize with a threshold setting and Angle Number setting
        /// </summary
        public void initialize(String fileName, int inputThreshold, float angleGap)
        {
            initialize(fileName, inputThreshold);
            totalAngles = (int)(360 / angleGap);
        }
        /// <summary>
        /// Manually update/build Template
        /// </summary
        public void buildTemplate(string fileName, bool saveToXml)
        {
            templPyrs = fromFiles(fileName, saveToXml);
        }
        #endregion

        #region build template with Camera


        Bgr<byte>[,] frame = null;
        //Bgr<byte>[,] temp;
        int i;
        int TPindex = 0;
        float CabRatio = 1;
        Gray<byte>[,] ResiizedtemplatePic, templatePic;
        State Cap = State.Init;

        int RectWidth, RectHeight;
        int SqrSide;
        float RectRatio;

        enum State
        {
            Init,
            BuildingTemplate,
            Calibrate,
            Confirm,
            Rotate,
            Done
        }

        private void rotateLoad(Gray<byte>[,] image, int angles, bool buildXMLTemplateFile)
        {
            string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
            for (int i = 0; i < angles; i++)
            {
                float ImageAngle = (float)i * 360 / angles;
                Bitmap bitmap = image.ToBitmap();

                Bitmap returnBitmap = new Bitmap(SqrSide, SqrSide);
                Graphics graphics = Graphics.FromImage(returnBitmap);
                graphics.TranslateTransform((float)SqrSide / 2, (float)SqrSide / 2);
                graphics.RotateTransform(ImageAngle);
                graphics.TranslateTransform(-(float)SqrSide / 2, -(float)SqrSide / 2);
                graphics.DrawImage(bitmap, new System.Drawing.Point(-(frame.Width() - SqrSide) / 2, -(frame.Height() - SqrSide) / 2));
                bitmap.Dispose();
                returnBitmap.Save("TP.bmp");
                Console.WriteLine(" angle " + ImageAngle);
                Gray<byte>[,] preparedBWImage = ImageIO.LoadGray("TP.bmp").Clone();

                // Accord.Extensions.Imaging.

                try
                {
                    templPyrs.Add(TemplatePyramid.CreatePyramidFromPreparedBWImage(
                        preparedBWImage, " Template #" + TPindex++, ImageAngle));
                }
                catch (Exception)
                { }
            }
            if (buildXMLTemplateFile)
                XMLTemplateSerializer<ImageTemplatePyramid<ImageTemplate>, ImageTemplate>.ToFile(templPyrs, Path.Combine(resourceDir, "template" + ".xml"));

        }

        private void rotateLoad(List<TemplatePyramid> retList,Gray<byte>[,] image, int angles, bool buildXMLTemplateFile)
        {
            string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
            for (int i = 0; i < angles; i++)
            {
                float ImageAngle = (float)i * 360 / angles;
                Bitmap bitmap = image.ToBitmap();

                Bitmap returnBitmap = new Bitmap(SqrSide, SqrSide);
                Graphics graphics = Graphics.FromImage(returnBitmap);
                graphics.TranslateTransform((float)SqrSide / 2, (float)SqrSide / 2);
                graphics.RotateTransform(ImageAngle);
                graphics.TranslateTransform(-(float)SqrSide / 2, -(float)SqrSide / 2);
                graphics.DrawImage(bitmap, new System.Drawing.Point(-(frame.Width() - SqrSide) / 2, -(frame.Height() - SqrSide) / 2));
                bitmap.Dispose();
                returnBitmap.Save("TP.bmp");
                Console.WriteLine(" angle " + ImageAngle);
                Gray<byte>[,] preparedBWImage = ImageIO.LoadGray("TP.bmp").Clone();

                // Accord.Extensions.Imaging.

                try
                {
                    retList.Add(TemplatePyramid.CreatePyramidFromPreparedBWImage(
                        preparedBWImage, " Template #" + TPindex++, ImageAngle));
                }
                catch (Exception)
                { }
            }
            if (buildXMLTemplateFile)
                XMLTemplateSerializer<ImageTemplatePyramid<ImageTemplate>, ImageTemplate>.ToFile(templPyrs, Path.Combine(resourceDir, "template" + ".xml"));

        }


        private void drawFrameWork(Bgr<byte>[,] inputFrame)
        {
            DotImaging.Primitives2D.Rectangle rec = new DotImaging.Primitives2D.Rectangle((frame.Width() - RectWidth) / 2, (frame.Height() - RectHeight) / 2, (int)(frame.Width() * RectRatio), (int)(frame.Height() * RectRatio));
            //frame.Draw(rec, Bgr<byte>.Blue, 2);
            rec.Width = rec.Height;
            rec.X = (frame.Width() - RectHeight) / 2;
            //frame.Draw(rec, Bgr<byte>.Red, 2);
            Circle cir = new Circle((frame.Width()) / 2, (frame.Height()) / 2, (int)(frame.Height() * RectRatio) / 2);
            //frame.Draw(cir, Bgr<byte>.Green, 2);
            cir.Radius = frame.Height() / 2;
            frame.Draw(cir, Bgr<byte>.Green, 2);
            SqrSide = (int)(frame.Height() / Math.Sqrt(2));
            DotImaging.Primitives2D.Rectangle sqr = new DotImaging.Primitives2D.Rectangle((frame.Width() - SqrSide) / 2, (frame.Height() - SqrSide) / 2, SqrSide, SqrSide);
            frame.Draw(sqr, Bgr<byte>.Blue, 2);
            cir.Radius = SqrSide / 2;
            frame.Draw(cir, Bgr<byte>.Red, 2);

        }

        //ImageStreamReader videoCapture;

        /// <summary>
        /// Build template with the cemara
        /// 
        /// State Machine---->
        /// int -> build template -> resize to find the best -> draw the template then comfirm by user ->
        /// rotate for angles ->done and return
        /// 
        /// make sure the object is in green circle
        /// </summary
        public void TemplateCapture(string name, ImageStreamReader videoCapture, PictureBox pictureBox)
        {

            if (name == null) name = " Template";
#if runXML
            try
            {
                Console.WriteLine("Reading from existing Template Data");
                templPyrs = fromXML(name);
                Cap = State.Done;
            }
            catch (Exception)
            {
                Console.WriteLine("\nTemplate NOT found! \n initiating camera...");

            }
#endif
            switch (Cap)
            {

                case State.Init:
                    videoCapture.ReadTo(ref frame);

                    if (frame == null)
                        return;
                    int maxside = (int)(Math.Sqrt(frame.Width() * frame.Width() + frame.Height() * frame.Height()));
                    RectRatio = (float)(frame.Height() > frame.Width() ? frame.Width() : frame.Height()) / maxside;
                    //Console.WriteLine(frame.Height() + "  "+  frame.Width() +"   "+maxside + "  " + RectRatio);
                    RectWidth = (int)(frame.Width() * RectRatio);
                    RectHeight = (int)(frame.Height() * RectRatio);

                    drawFrameWork(frame);

                    pictureBox.Image = frame.ToBitmap(); //it will be just casted (data is shared) 24bpp color
                    break;
                case State.BuildingTemplate:
                    Console.WriteLine("building template");
                    videoCapture.ReadTo(ref frame);

                    if (frame == null)
                        return;

                    templatePic = frame.ToGray();

                    var list = new List<TemplatePyramid>();
                    try
                    {
                        templPyrs = new List<TemplatePyramid>();
                        rotateLoad(templatePic, 1,false);
                        //templPyrs.Add(TemplatePyramid.CreatePyramidFromPreparedBWImage(
                        //templatePic, " Template #" + TPindex++ , 0));
                        //templPyrs = BTlist;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("ERROR IN CREATING TEMPLATE!");

                        return;
                    }
                    if (templPyrs == null)
                    {
                        break;
                    }
                    Cap = State.Calibrate;
                    break;

                case State.Calibrate:
                    Console.WriteLine("calibrating template   " + CabRatio + "    " + templatePic.Width() * CabRatio);
                    var bestRepresentatives = findObjects(frame);
                    if (bestRepresentatives.Count == 0)
                    {
                        if (templPyrs.Count != 0) templPyrs.RemoveAt(templPyrs.Count - 1);
                        CabRatio -= (float)0.01;
                        int width = (int)(templatePic.Width() * CabRatio);
                        int height = (int)(templatePic.Height() * CabRatio);

                        DotImaging.Primitives2D.Size Nsize = new DotImaging.Primitives2D.Size(width, height);
                        ResiizedtemplatePic = ResizeExtensions_Gray.Resize(templatePic, Nsize, Accord.Extensions.Imaging.InterpolationMode.NearestNeighbor);

                        try
                        {
                            templPyrs.Add(TemplatePyramid.CreatePyramidFromPreparedBWImage(
                            ResiizedtemplatePic, new FileInfo(name + " #" + TPindex++).Name /*"OpenHand"*/, 0));
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("ERROR IN CALIBRATING TEMPLATE!");


                        }
                    }
                    else
                    {
                        ResiizedtemplatePic = (ResiizedtemplatePic == null ? templatePic : ResiizedtemplatePic);

                        CaptureFrame(videoCapture, pictureBox);
                        drawFrameWork(frame);
                        pictureBox.Image = frame.ToBitmap(); //it will be just casted (data is shared) 24bpp color

                        Cap = State.Confirm;
                    }
                    break;
                case State.Confirm:
                    Console.WriteLine("comfirm Template, press Y to continue, press R to retry, and other keys to abort");
                    string a = Console.ReadLine();

                    switch (a)
                    {
                        case "y":
                            Cap = State.Rotate;
                            break;
                        case "r":
                            Cap = State.Init;
                            break;
                        default:
                            Cap = State.Done;
                            break;
                    }
                    if (a == "y") Cap = State.Rotate;
                    else
                    {
                        Cap = State.Init;
                    }
                    break;
                case State.Rotate:
                    rotateLoad(ResiizedtemplatePic, totalAngles,false);
                    string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
                    XMLTemplateSerializer<ImageTemplatePyramid<ImageTemplate>, ImageTemplate>.ToFile(templPyrs, Path.Combine(resourceDir, name + ".xml"));

                    Cap = State.Done;
                    break;
                case State.Done:
                    CaptureFrame(videoCapture, pictureBox);
                    GC.Collect();
                    break;
                    //break;

            }
            pictureBox.Image = frame.ToBitmap(); //it will be just casted (data is shared) 24bpp color



        }

        public void TemplateCapture( ImageStreamReader videoCapture, PictureBox pictureBox)
        {
            TemplateCapture(null, videoCapture, pictureBox);
        }

        public void CaptureFrame(ImageStreamReader videoCapture, PictureBox pictureBox)
        {
            DotImaging.Font font = new DotImaging.Font(FontTypes.HERSHEY_DUPLEX, 1, 0.1f);

            videoCapture.ReadTo(ref frame);

            if (frame == null)
                return;

            long preprocessTime, matchTime;
            var bestRepresentatives = findObjects(frame, threshold, null, out preprocessTime, out matchTime);

            /************************************ drawing ****************************************/
            foreach (var m in bestRepresentatives)
            {
                frame.Draw(m.BoundingRect, Bgr<byte>.Blue, 1);
                frame.Draw(m.Points.Select(x => new Circle(x.X, x.Y, 5)).ToArray(), Bgr<byte>.Blue, -1);

                Console.WriteLine("Best template: " + m.Template.ClassLabel + "  AT  " + m.BoundingRect.X + "  " + m.BoundingRect.Y + " ANGLE: " + m.Template.Angle + " score: " + m.Score);
            }

            frame.Draw(String.Format("Matching {0} templates in: {1} ms", templPyrs.Count, matchTime),
                       font, new DotImaging.Primitives2D.Point(10, 25), Bgr<byte>.Green);
            /************************************ drawing ****************************************/

            pictureBox.Image = frame.ToBitmap(); //it will be just casted (data is shared) 24bpp color

            //frame.Save(String.Format("C:/probaImages/imgMarked_{0}.jpg", i)); b.Save(String.Format("C:/probaImages/img_{0}.jpg", i)); i++;
            GC.Collect();
        }

        public void TPCapture()
        {
            if (Cap != State.Init) return;
            Cap = State.BuildingTemplate;
        }
#endregion





        /// <summary>
        /// set Number of Sizes, default to 1
        /// </summary
        public void setNumOfAngles(int angle)
        {
            if (angle <= 0) angle = 1;
            totalAngles = angle;
        }
        /// <summary>
        /// set Number of Sizes, default to 1
        /// </summary
        public void setNumOfSizes(int Num)
        {
            if (Num <= 0) Num = 1;
            totalSizes = Num;
        }

        private void ResizeImage(float Ratio, string inputFile, string outFile)
        {
            Bitmap image = new Bitmap(inputFile, true);

            int width = (int)(image.Width * Ratio);
            int height = (int)(image.Height * Ratio);

            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            destImage.Save(outFile);
        }
        private void rotateImage(string inputFile, float angle, string outFile)
        {
            Bitmap bitmap = new Bitmap(inputFile, true);
            int maxside = (int)(Math.Sqrt(bitmap.Width * bitmap.Width + bitmap.Height * bitmap.Height));
            Bitmap returnBitmap = new Bitmap(maxside, maxside);
            Graphics graphics = Graphics.FromImage(returnBitmap);
            graphics.TranslateTransform((float)bitmap.Width / 2, (float)bitmap.Height / 2);
            graphics.RotateTransform(angle);
            graphics.TranslateTransform(-(float)bitmap.Width / 2, -(float)bitmap.Height / 2);
            graphics.DrawImage(bitmap, new System.Drawing.Point(0, 0));
            bitmap.Dispose();
            returnBitmap.Save(outFile);
        }


        public List<TemplatePyramid> buildTemplate(Gray<byte>[,] image, bool buildXMLTemplateFile, int angles)
        {
           
            return buildTemplate(image, buildXMLTemplateFile, angles,1);

        }

        public List<TemplatePyramid> buildTemplate(Gray<byte>[,] image, bool buildXMLTemplateFile, int angles, int sizes)
        {
           
            return buildTemplate(image, buildXMLTemplateFile, angles,sizes, 0.6f);

        }

        public List<TemplatePyramid> buildTemplate(Gray<byte>[,] image, bool buildXMLTemplateFile, int angles, int sizes, float minRatio)
        {
            List<TemplatePyramid> retList = new List<TemplatePyramid>();
            float Ratio = 1;
            rotateLoad(retList, image, angles, false);
            for (int i = 0; i < sizes -1; i++)
            {
                Ratio -= (float)(1 - minRatio) / sizes;
                int width = (int)(templatePic.Width() * Ratio);
                int height = (int)(templatePic.Height() * Ratio);

                DotImaging.Primitives2D.Size Nsize = new DotImaging.Primitives2D.Size(width, height);
                image = ResizeExtensions_Gray.Resize(image, Nsize, Accord.Extensions.Imaging.InterpolationMode.NearestNeighbor);

                rotateLoad(retList, image, angles, buildXMLTemplateFile);
            }
            return retList;

        }

        public List<TemplatePyramid> fromFiles(String fileName, bool buildXMLTemplateFile)
        {
            Console.WriteLine("Building templates from files...");

            var list = new List<TemplatePyramid>();

            string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
            string[] files = Directory.GetFiles(resourceDir, fileName);
            if (files.Length == 0)
            {
                throw new Exception("NO FILE FOUND");
            }
            object syncObj = new object();
            //int fileNum = 0;
            //Parallel.ForEach(files, delegate (string file)
            foreach (var file in files)
            {


                for (int j = 0; j < totalSizes; j++)
                {

                    string tempFile = file;

                    tempFile = "TP.bmp";
                    string tempRotatedFile = "TPR.bmp";
                    ResizeImage(0.6F - (float)j / 10, file, tempFile);


                    for (int i = 0; i < totalAngles; i++)
                    {
                        float ImageAngle = (float)i * 360 / totalAngles;
                        rotateImage(tempFile, ImageAngle, tempRotatedFile);
                        Console.WriteLine("Size # " + j + " angle " + ImageAngle);
                        Gray<byte>[,] preparedBWImage = ImageIO.LoadGray(tempRotatedFile).Clone();
                        try
                        {

                            var tp = TemplatePyramid.CreatePyramidFromPreparedBWImage(preparedBWImage, new FileInfo(file + "  " + i * 10).Name /*"OpenHand"*/, ImageAngle);
                            list.Add(tp);
                        }
                        catch (Exception)
                        { }
                    }
                }
            }
            if(buildXMLTemplateFile)
                XMLTemplateSerializer<ImageTemplatePyramid<ImageTemplate>, ImageTemplate>.ToFile(list, Path.Combine(resourceDir, fileName.Substring(0, fileName.IndexOf(".")) + ".xml"));
            return list;
        }

        public List<TemplatePyramid> fromXML(String fileName)
        {
            List<TemplatePyramid> list = new List<TemplatePyramid>();

            string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
            //list = XMLTemplateSerializer<TemplatePyramid, Template>.FromFile(Path.Combine(resourceDir, "OpenHand_Right.xml")).ToList();
            list = XMLTemplateSerializer<TemplatePyramid, Template>.FromFile(Path.Combine(resourceDir, fileName.Substring(0, fileName.IndexOf(".")) + ".xml")).ToList();

            return list;
        }

        public List<Match> findObjects(Bgr<byte>[,] image)
        {
            var grayIm = image.ToGray();

            var bestRepresentatives = new List<Match>();

            linPyr = LinearizedMapPyramid.CreatePyramid(grayIm); //prepare linear-pyramid maps
            List<Match> matches = linPyr.MatchTemplates(templPyrs, threshold);

            var matchGroups = new MatchClustering(minDetectionsPerGroup).Group(matches.ToArray());
            foreach (var group in matchGroups)
            {
                bestRepresentatives.Add(group.Representative);
            }

            return bestRepresentatives;
        }


        public List<Match> findObjects(Bgr<byte>[,] image, String[] labels)
        { 
            return findObjects(image, threshold, labels);
        }

        /// <summary>
        /// find the matches in the given image.  
        /// </summary>
        /// <param name="image">input image.</param>
        /// <param name="Threshold">the threshold for the matching algor.</param>
        /// <param name="labels">the specific label(s) that included in the returned List
        ///                      set to null to find all matches</param>
        /// <returns>List of found matches.</returns>
        public List<Match> findObjects(Bgr<byte>[,] image, int Threshold, String[] labels)
        {
            var grayIm = image.ToGray();

            var bestRepresentatives = new List<Match>();

            linPyr = LinearizedMapPyramid.CreatePyramid(grayIm); //prepare linear-pyramid maps
            List<Match> matches = linPyr.MatchTemplates(templPyrs, Threshold);

            var matchGroups = new MatchClustering(minDetectionsPerGroup).Group(matches.ToArray());
            foreach (var group in matchGroups)
            {
                if (labels == null ? labels.Contains(group.Representative.Template.ClassLabel):true)
                    bestRepresentatives.Add(group.Representative);
            }

            return bestRepresentatives;
        }

        /// <summary>
        ///  find the matches in the given image with stopwatch included.  
        /// </summary>
        /// <param name="image">input image.</param>
        /// <param name="Threshold">the threshold for the matching algor.</param>
        /// <param name="labels">the specific label(s) that included in the returned List
        ///                      set to null to find all matches</param>      
        /// <returns>List of found matches.</returns>
        public List<Match> findObjects(Bgr<byte>[,] image, int Threshold, String[] labels, out long preprocessTime, out long matchTime)
        {
            var grayIm = image.ToGray();

            var bestRepresentatives = new List<Match>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            linPyr = LinearizedMapPyramid.CreatePyramid(grayIm); //prepare linear-pyramid maps
            preprocessTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            List<Match> matches = linPyr.MatchTemplates(templPyrs, Threshold);
            stopwatch.Stop(); matchTime = stopwatch.ElapsedMilliseconds;

            var matchGroups = new MatchClustering(minDetectionsPerGroup).Group(matches.ToArray());
            foreach (var group in matchGroups)
            {
                if (labels == null ? labels.Contains(group.Representative.Template.ClassLabel) : true)
                    bestRepresentatives.Add(group.Representative);
            }

            return bestRepresentatives;
        }

    }
}