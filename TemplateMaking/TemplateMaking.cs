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
using Template = Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplate;
using TemplatePyramid = Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplatePyramid<Accord.Extensions.Imaging.Algorithms.LINE2D.ImageTemplate>;
using DotImaging;
using DotImaging.Primitives2D;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace TemplateMaking
{
    public partial class TemplateMaking : Form
    {

        public TemplateMaking()
        {
            InitializeComponent();                                                                                              
        }
    }
}
