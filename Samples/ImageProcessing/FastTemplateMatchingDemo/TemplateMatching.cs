//#define FILE_CAPTURE //comment it to enable camera capture
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
using Accord.Math;

namespace FastTemplateMatching
{
    public class TemplateMatching 
    {
 

        //int minDetectionsPerGroup = 0; //for match grouping (postprocessing)

        public List<TemplatePyramid> LocaltemplPyrs = null;

        private LinearizedMapPyramid linPyr = null;

        public TemplateMatching() { }

        #region initialize with local file

        ///// <summary>
        ///// Initialize with a threshold setting and Angle Number setting
        ///// </summary
        //public void initialize(String fileName, int inputThreshold = 80 , float angleGap = 1 , int sizes = 1) 
        //{

        //    this.threshold = inputThreshold;
        //    this.totalAngles = (int)(360 / angleGap);
        //    this.totalSizes = sizes;
//#if AUTOINIT
//            try
//            {
//                Console.WriteLine("Reading from existing Template Data");
//                templPyrs = fromXML(fileName);
//            }
//            catch (Exception)
//            {
//                Console.WriteLine("\nTemplate NOT found! ");
//                templPyrs = fromFiles(fileName);
//            }
//            Console.WriteLine("COMPLETE!");
//#else
//#if READFILE
//            templPyrs = fromFiles(fileName, true);
//#else
//            templPyrs = fromXML(fileName);
//#endif

//#endif
            
//        }
        /// <summary>
        /// Manually update/build Template
        /// </summary
        public static  void buildTemplate(string fileName, ref List<TemplatePyramid> templPyrs, bool saveToXml = false)
        {
            templPyrs = fromFiles(fileName, saveToXml);
        }
        #endregion

        #region build template with Camera
        static int TPindex = 0;

      

        //Load the images into the specific retList, put null to use the default templpyr
        private static void rotateLoad(List<TemplatePyramid> retList, Gray<byte>[,] image, int angles, int Width, int Height, bool buildXMLTemplateFile = false)
        {
            
            string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
            retList = (retList == null ? new List<TemplatePyramid>() : retList);
            
            for (int i = 0; i < angles; i++)
            {
                float ImageAngle = (float)i * 360 / angles;
                Bitmap bitmap = image.ToBitmap();

                Bitmap returnBitmap = new Bitmap(Width, Height);
                Graphics graphics = Graphics.FromImage(returnBitmap);
                graphics.TranslateTransform((float)Width / 2, (float)Height / 2);
                graphics.RotateTransform(ImageAngle);
                graphics.TranslateTransform(-(float)Width / 2, -(float)Height / 2);
                graphics.DrawImage(bitmap, new System.Drawing.Point(-(image.Width() - Width) / 2, -(image.Height() - Height) / 2));
                bitmap.Dispose();
                returnBitmap.Save("TP.bmp");
                Console.WriteLine(" angle " + ImageAngle);
                Gray<byte>[,] preparedBWImage = ImageIO.LoadGray("TP.bmp").Clone();

                // Accord.Extensions.Imaging.

                try
                {
                    TemplatePyramid newTemp = TemplatePyramid.CreatePyramidFromPreparedBWImage(
                        preparedBWImage, " Template #" + TPindex++, ImageAngle);
                    validateFeatures(ref newTemp, image);
                    retList.Add(newTemp);
                }
                catch (Exception)
                { }
            }
            if (buildXMLTemplateFile)
                XMLTemplateSerializer<ImageTemplatePyramid<ImageTemplate>, ImageTemplate>.ToFile(retList, Path.Combine(resourceDir, "template" + ".xml"));

        }
       
        /// <summary>
        /// Given a specific image, eliminate the redunant features (i.e. remove any feature that is outside the green circle
        /// </summary>
        /// <param name="reps">current template pyrimid to be checked</param>
        /// <param name="image">the video image, served as a size comparator</param>
        /// /// <returns>nothing.</returns>
        public static void validateFeatures(ref TemplatePyramid reps, Gray<byte>[,] image)
        {
            List<Feature> temp;
            int shortSide = (image.Width() < image.Height()) ? image.Width() : image.Height();
            foreach(Template t in reps.Templates) {
                temp = t.Features.ToList();
                for (int i = 0; i < t.Features.Count(); i++) {


                    int p = (t.Features[i].X + t.BoundingRect.X - image.Width() / 2) * (t.Features[i].X + t.BoundingRect.Y - image.Width() / 2) + (t.Features[i].Y - image.Height() / 2) * (t.Features[i].Y - image.Height() / 2);
                    //Console.WriteLine(p + "    " + t.Features[i].X + "  " + t.Features[i].Y);
                    if (p > ((shortSide / 2) * (shortSide / 2)))
                    {
                        //Console.WriteLine(p + "asdf gsfdgafhsdgsdfhsdhsdhsd    " + t.Features[i].X + "  " + t.Features[i].Y);
                        temp.Remove(t.Features[i]);
                    }
                }
               
                t.Features = temp.ToArray();
                
                //Console.WriteLine("NUM of features: " + t.Features.Count());
                
                //GC.Collect();
            }
            
        }

        /// <summary>
        /// Draw the framework ( a small red circle, blue square and a big green circle)
        /// </summary>
        /// <param name="frame">the video frame that about to draw on</param>
        /// /// <returns>nothing.</returns>
        private static void drawFrameWork(Bgr<byte>[,] frame)
        {
            int RectWidth, RectHeight;
            int SqrSide;
            float RectRatio;
            int maxside = (int)(Math.Sqrt(frame.Width() * frame.Width() + frame.Height() * frame.Height()));
            RectRatio = (float)(frame.Height() > frame.Width() ? frame.Width() : frame.Height()) / maxside;
            //Console.WriteLine(frame.Height() + "  "+  frame.Width() +"   "+maxside + "  " + RectRatio);
            RectWidth = (int)(frame.Width() * RectRatio);
            RectHeight = (int)(frame.Height() * RectRatio);

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

        #region template capture variables
        Bgr<byte>[,] frame = null;
        float CabRatio = 1;

        Gray<byte>[,] ResiizedtemplatePic, templatePic;
        private State Cap = State.Init;

        enum State
        {
            Init,
            BuildingTemplate,
            Calibrate,
            Confirm,
            Rotate,
            ConfirmDone,
            Done
        }
        #endregion

        /// <summary>
        /// Build template with the cemara
        /// 
        /// State Machine---->
        /// int -> build template -> resize to find the best -> draw the template then comfirm by user ->
        /// rotate for angles ->done and return
        /// 
        /// make sure the object is in green circle
        /// </summary>
        /// <param name="name">template name, default to "Template" if the name is null</param>
        /// <param name="templPyrs">the template list that to be used</param
        /// <param name="videoCapture">the video stream</param>
        /// <param name="pictureBox">the picture box of window form</param>
        /// <param name="minRatio">The ratio of smallest size to original, default to 0.4</param>
        /// /// <returns>nothing.</returns>
        public void TemplateCapture(string name, ref List<TemplatePyramid> templPyrs, ImageStreamReader videoCapture, PictureBox pictureBox, float minRatio = 0.4f)
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


                    
                    drawFrameWork(frame);

                    pictureBox.Image = frame.ToBitmap(); //it will be just casted (data is shared) 24bpp color
                    break;
                case State.BuildingTemplate:
                    Console.WriteLine("building template");
                    videoCapture.ReadTo(ref frame);

                    if (frame == null)
                        return;

                    templatePic = frame.ToGray();

                    //var list = new List<TemplatePyramid>();
                    try
                    {
                        if (templPyrs == null)
                            templPyrs = new List<TemplatePyramid>();
                        rotateLoad(templPyrs, templatePic, 1, frame.Width(), frame.Height());
                        //templPyrs.Add(TemplatePyramid.CreatePyramidFromPreparedBWImage(
                        //templatePic, " Template #" + TPindex++ , 0));
                        //templPyrs = BTlist;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("ERROR IN CREATING TEMPLATE!");

                        return;
                    }
           
                    Cap = State.Calibrate;
                    break;

                case State.Calibrate:
                    Console.WriteLine("calibrating template   " + CabRatio + "    " + templatePic.Width() * CabRatio);
                    var bestRepresentatives = findObjects(frame, templPyrs);
                    if (bestRepresentatives.Count == 0)
                    {
                        if (templPyrs.Count != 0) templPyrs.RemoveAt(templPyrs.Count - 1);
                        CabRatio -= (float)0.01;
                        int width = (int)(templatePic.Width() * CabRatio);
                        int height = (int)(templatePic.Height() * CabRatio);
                        if(CabRatio < minRatio)
                        {
                            Console.WriteLine("Calibration failed");
                            CabRatio = 1;
                            Cap = State.Init;
                        }
                        DotImaging.Primitives2D.Size Nsize = new DotImaging.Primitives2D.Size(width, height);
                        ResiizedtemplatePic = ResizeExtensions_Gray.Resize(templatePic, Nsize, Accord.Extensions.Imaging.InterpolationMode.NearestNeighbor);

                        try
                        {
                            templPyrs.Add(TemplatePyramid.CreatePyramidFromPreparedBWImage(
                            ResiizedtemplatePic, new FileInfo(name + " #" + TPindex++).Name, 0));
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("ERROR IN CALIBRATING TEMPLATE!");
                        }
                    }
                    else
                    {
                        ResiizedtemplatePic = (ResiizedtemplatePic == null ? templatePic : ResiizedtemplatePic);

                        CaptureFrame(templPyrs,videoCapture, pictureBox);
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
                            templPyrs.RemoveAt(templPyrs.Count - 1);
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
                    int SqrSide = (int)(frame.Height() / Math.Sqrt(2));
                    rotateLoad(templPyrs, ResiizedtemplatePic, 10 ,SqrSide,SqrSide,false);

                    Cap = State.ConfirmDone;
                    break;
                case State.ConfirmDone:
                    Console.WriteLine("Do you want to build a new template? press y to build another template, other keys to abort");
                    string OtherTemplate = Console.ReadLine();
                    if(OtherTemplate == "y" || OtherTemplate == "Y")
                    {
                        Cap = State.Init;
                    }
                    else
                    {
                        string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
                        XMLTemplateSerializer<ImageTemplatePyramid<ImageTemplate>, ImageTemplate>.ToFile(templPyrs, Path.Combine(resourceDir, name + ".xml"));
                        Cap = State.Done;
                    }
                    break;
                //break;
                case State.Done:
                    CaptureFrame(templPyrs,videoCapture, pictureBox);
                    GC.Collect();
                    break;
                    //break;

            }
            pictureBox.Image = frame.ToBitmap(); //it will be just casted (data is shared) 24bpp color



        }
       
        /// <summary>
        /// Build template with the cemara
        /// 
        /// State Machine---->
        /// int -> build template -> resize to find the best -> draw the template then comfirm by user ->
        /// rotate for angles ->done and return
        /// 
        /// make sure the object is in green circle
        /// </summary>
        /// <param name="templPyrs">the template list that to be used</param
        /// <param name="videoCapture">the video stream</param>
        /// <param name="pictureBox">the picture box of window form</param>
        /// /// <returns>nothing.</returns>
        public void TemplateCapture(ref List<TemplatePyramid> templPyrs, ImageStreamReader videoCapture, PictureBox pictureBox)
        {
            TemplateCapture(null,ref templPyrs, videoCapture, pictureBox);
        }

        public void CaptureFrame(List<TemplatePyramid> templPyrs, ImageStreamReader videoCapture, PictureBox pictureBox)
        {
            DotImaging.Font font = new DotImaging.Font(FontTypes.HERSHEY_DUPLEX, 1, 0.1f);

            videoCapture.ReadTo(ref frame);

            if (frame == null)
                return;

            long preprocessTime, matchTime;
            var bestRepresentatives = findObjects(frame, templPyrs, out preprocessTime, out matchTime);

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

        #region default methods


        private static void rotateImage(string inputFile, float angle, string outFile)
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

        /// <summary>
        /// build template List from a Gray image
        /// </summary>
        /// <param name="image">input image</param>
        /// <param name="buildXMLTemplateFile">true to build a xml file with current templates</param>
        /// <param name="angles">the number of angles, default to 360</param>
        /// <param name="sized">number of sizes, default to 1</param>
        /// <param name="minRatio">The ratio of smallest size to original, default to 0.6</param>
        /// <returns>List of templates.</returns>
        public static List<TemplatePyramid> buildTemplate(Gray<byte>[,] image, bool buildXMLTemplateFile, int angles, int sizes = 1, float minRatio = 0.6f)
        {
            List<TemplatePyramid> retList = new List<TemplatePyramid>();
            float Ratio = 1;
            Gray<byte>[,] tempIMG;
            rotateLoad(retList, image, angles,image.Width(), image.Height(), false);
            for (int i = 0; i < sizes -1; i++)
            {
                Ratio -= (float)(1 - minRatio) / sizes;
                int width = (int)(image.Width() * Ratio);
                int height = (int)(image.Height() * Ratio);

                DotImaging.Primitives2D.Size Nsize = new DotImaging.Primitives2D.Size(width, height);
                tempIMG = ResizeExtensions_Gray.Resize(image, Nsize, Accord.Extensions.Imaging.InterpolationMode.NearestNeighbor);

                rotateLoad(retList, tempIMG, angles, image.Width(), image.Height(), buildXMLTemplateFile);
            }
            return retList;

        }

        /// <summary>
        /// build template List from a XML file
        /// </summary>
        /// <param name="filename">input XML file name, the file has to be in the current directory </param>
        /// <param name="buildXMLTemplateFile">true to build a xml file with current templates</param>
        /// <param name="angles">the number of angles, default to 360</param>
        /// <param name="sized">number of sizes, default to 1</param>
        /// <returns>List of templates.</returns>
        public static List<TemplatePyramid> fromFiles(String fileName, bool buildXMLTemplateFile , int angles = 360, int sizes = 1)
        {
            Console.WriteLine("Building templates from files...");
            Gray<byte>[,] ResizedtemplatePic;
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

                string tempFile = "TP.bmp";
                for (int i = 0; i < angles; i++)
                {
                    float ImageAngle = (float)i * 360 / angles;
                    float FileRatio = 1;
                    rotateImage(file, ImageAngle, tempFile);
                    for (int j = 0; j < sizes; j++)
                    {
                        Console.WriteLine("Size # " + j + " angle " + ImageAngle);
                        Gray<byte>[,] preparedBWImage = ImageIO.LoadGray(tempFile).Clone();
                        FileRatio -= (float)0.01;
                        int width = (int)(preparedBWImage.Width() * FileRatio);
                        int height = (int)(preparedBWImage.Height() * FileRatio);
                        if (FileRatio < 0)
                        {
                            Console.WriteLine("Too many sizes to build, Max 100");
                            FileRatio = 1;
                            break;
                        }
                        DotImaging.Primitives2D.Size Nsize = new DotImaging.Primitives2D.Size(width, height);
                        ResizedtemplatePic = ResizeExtensions_Gray.Resize(preparedBWImage, Nsize, Accord.Extensions.Imaging.InterpolationMode.NearestNeighbor);

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
            if (buildXMLTemplateFile)
                XMLTemplateSerializer<ImageTemplatePyramid<ImageTemplate>, ImageTemplate>.ToFile(list, Path.Combine(resourceDir, fileName.Substring(0, fileName.IndexOf(".")) + ".xml"));
            return list;
        }

        /// <summary>
        /// build template List from a XML file
        /// </summary>
        /// <param name="filename">input XML file name, the file has to be in the current directory</param>
        /// <returns>List of templates.</returns>
        public static  List<TemplatePyramid> fromXML(String fileName)
        {
            List<TemplatePyramid> list = new List<TemplatePyramid>();

            string resourceDir = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Resources");
            //list = XMLTemplateSerializer<TemplatePyramid, Template>.FromFile(Path.Combine(resourceDir, "OpenHand_Right.xml")).ToList();
            list = XMLTemplateSerializer<TemplatePyramid, Template>.FromFile(Path.Combine(resourceDir, fileName.Substring(0, fileName.IndexOf(".")) + ".xml")).ToList();

            return list;
        }


        /// <summary>
        /// find the matches in the given image.  
        /// </summary>
        /// <param name="image">input image.</param>
        /// <param name="Threshold">the threshold for the matching algor.</param>
        /// <param name="labels">the specific label(s) that included in the returned List
        ///                      set to null to find all matches</param>
        /// <returns>List of found matches.</returns>
        public static List<Match> findObjects(Bgr<byte>[,] image, List<TemplatePyramid> templPyrs, int Threshold = 80, String[] labels = null, int minDetectionsPerGroup = 0)
        {
            var grayIm = image.ToGray();

            var bestRepresentatives = new List<Match>();

            LinearizedMapPyramid linPyr  = LinearizedMapPyramid.CreatePyramid(grayIm); //prepare linear-pyramid maps
            List<Match> matches = linPyr.MatchTemplates(templPyrs, Threshold);

            var matchGroups = new MatchClustering(minDetectionsPerGroup).Group(matches.ToArray());
            foreach (var group in matchGroups)
            {
                if (labels == null ? true : labels.Contains(group.Representative.Template.ClassLabel))
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
        public static List<Match> findObjects(Bgr<byte>[,] image, List<TemplatePyramid> templPyrs,  out long preprocessTime, out long matchTime, int Threshold = 80, String[] labels = null, int minDetectionsPerGroup = 0)
        {
            var grayIm = image.ToGray();

            var bestRepresentatives = new List<Match>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            LinearizedMapPyramid linPyr = LinearizedMapPyramid.CreatePyramid(grayIm); //prepare linear-pyramid maps
            preprocessTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            List<Match> matches = linPyr.MatchTemplates(templPyrs, Threshold);
            stopwatch.Stop(); matchTime = stopwatch.ElapsedMilliseconds;

            var matchGroups = new MatchClustering(minDetectionsPerGroup).Group(matches.ToArray());
            foreach (var group in matchGroups)
            {
                if (labels == null ? true : labels.Contains(group.Representative.Template.ClassLabel) )
                    bestRepresentatives.Add(group.Representative);
            }

            return bestRepresentatives;
        }

        #endregion
    }
}
