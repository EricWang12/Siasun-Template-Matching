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

// Accord Imaging Library
// The Accord.Extensions.NET Framework
// http://accord.googlecode.com
//
// Copyright © César Souza, 2009-2013
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotImaging;
using DotImaging.Primitives2D;

namespace Accord.Extensions.Imaging.Moments
{
    /// <summary>
    ///   Raw image moments.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    ///   In image processing, computer vision and related fields, an image moment is
    ///   a certain particular weighted average (moment) of the image pixels' intensities,
    ///   or a function of such moments, usually chosen to have some attractive property 
    ///   or interpretation.</para>
    ///
    /// <para>
    ///   Image moments are useful to describe objects after segmentation. Simple properties 
    ///   of the image which are found via image moments include area (or total intensity), 
    ///   its centroid, and information about its orientation.</para>
    ///   
    /// <para>
    ///   The raw moments are the most basic moments which can be computed from an image,
    ///   and can then be further processed to achieve <see cref="CentralMoments"/> or even
    ///   <see cref="HuMoments"/>.</para>
    ///   
    /// <para>
    ///   References:
    ///   <list type="bullet">
    ///     <item><description>
    ///       Wikipedia contributors. "Image moment." Wikipedia, The Free Encyclopedia. Wikipedia,
    ///       The Free Encyclopedia. Available at http://en.wikipedia.org/wiki/Image_moment </description></item>
    ///   </list>
    /// </para>
    /// </remarks>
    /// 
    /// <example>
    /// <code>
    /// Bitmap image = ...;
    ///
    /// // Compute the raw moments of up to third order
    /// RawMoments m = new RawMoments(image, order: 3);
    /// </code>
    /// </example>
    /// 
    /// <seealso cref="HuMoments"/>
    /// <seealso cref="CentralMoments"/>
    /// 
    public class RawMoments : IMoments
    {

        /// <summary>
        ///   Gets the default maximum moment order.
        /// </summary>
        /// 
        public const int DefaultOrder = 3;


        /// <summary>
        ///   Raw moment of order (0,0).
        /// </summary>
        /// 
        public float M00 { get; private set; }

        /// <summary>
        ///   Raw moment of order (1,0).
        /// </summary>
        /// 
        public float M10 { get; private set; }

        /// <summary>
        ///   Raw moment of order (0,1).
        /// </summary>
        /// 
        public float M01 { get; private set; }

        /// <summary>
        ///   Raw moment of order (1,1).
        /// </summary>
        /// 
        public float M11 { get; private set; }

        /// <summary>
        ///   Raw moment of order (2,0).
        /// </summary>
        /// 
        public float M20 { get; private set; }

        /// <summary>
        ///   Raw moment of order (0,2).
        /// </summary>
        /// 
        public float M02 { get; private set; }

        /// <summary>
        ///   Raw moment of order (2,1).
        /// </summary>
        /// 
        public float M21 { get; private set; }

        /// <summary>
        ///   Raw moment of order (1,2).
        /// </summary>
        /// 
        public float M12 { get; private set; }

        /// <summary>
        ///   Raw moment of order (3,0).
        /// </summary>
        /// 
        public float M30 { get; private set; }

        /// <summary>
        ///   Raw moment of order (0,3).
        /// </summary>
        /// 
        public float M03 { get; private set; }


        /// <summary>
        ///   Inverse raw moment of order (0,0).
        /// </summary>
        /// 
        public float InvM00 { get; private set; }



        /// <summary>
        ///   Gets the X centroid of the image.
        /// </summary>
        /// 
        public float CenterX { get; private set; }

        /// <summary>
        ///   Gets the Y centroid of the image.
        /// </summary>
        /// 
        public float CenterY { get; private set; }

        /// <summary>
        ///   Gets the area (for binary images) or sum of
        ///   gray level (for grayscale images).
        /// </summary>
        /// 
        public float Area { get { return M00; } }

        /// <summary>
        ///   Gets or sets the maximum order of the moments.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="Moments"/> class.
        /// </summary>
        public RawMoments(int order = DefaultOrder)
        {
            this.Order = order;
        }

        /// <summary>
        /// Computes moments for the provided image.
        /// </summary>
        /// <param name="image">Image.</param>
        /// <param name="area">Area</param>
        public void Compute(Gray<byte>[,] image, Rectangle area)
        {
            Reset();
            
            float m00, m01, m10, m11, m02, m20, m12, m21, m30, m03;

            using (var uImg = image.Lock(area))
            {
                computeByte(uImg, Point.Empty, Order,
                            out m00, out m01, out m10,
                            out m11, out m02, out m20,
                            out m12, out m21, out m30, out m03);
            }
            
            this.M00 += m00; this.M01 += m01; this.M10 += m10;
            this.M11 += m11; this.M02 += m02; this.M20 += m20;
            this.M12 += m12; this.M21 += m21; this.M30 += m30; this.M03 += m03;

            InvM00 = 1f / M00;
            CenterX = M10 * InvM00;
            CenterY = M01 * InvM00;
        }

        /// <summary>
        /// Computes moments for the provided image.
        /// </summary>
        /// <param name="image">Image.</param>
        /// <param name="area">Area</param>
        public void Compute(Gray<float>[,] image, Rectangle area)
        {
            Reset();

            float m00, m01, m10, m11, m02, m20, m12, m21, m30, m03;

            using (var uImg = image.Lock(area))
            {
                computeFloat(uImg, Point.Empty, Order,
                             out m00, out m01, out m10,
                             out m11, out m02, out m20,
                             out m12, out m21, out m30, out m03);
            }

            this.M00 += m00; this.M01 += m01; this.M10 += m10;
            this.M11 += m11; this.M02 += m02; this.M20 += m20;
            this.M12 += m12; this.M21 += m21; this.M30 += m30; this.M03 += m03;

            InvM00 = 1f / M00;
            CenterX = M10 * InvM00;
            CenterY = M01 * InvM00;
        }

        private unsafe static void computeByte(IImage image, Point offset, int order, 
                                               out float m00, out float m01, out float m10,
                                               out float m11, out float m02, out float m20,
                                               out float m12, out float m21, out float m30, out float m03)
        {
            m00 = m01 = m10 = m11 = m02 = m20 = m12 = m21 = m30 = m03 = 0;

            int width = image.Width;
            int height = image.Height;

            byte* imagePtr = (byte*)image.ImageData;

            for (int row = 0; row < height; row++)
            {
                float y = row + offset.Y;

                for (int col = 0; col < width; col++)
                {
                    float x = col + offset.X;
                    float v = imagePtr[col];

                    computeRawMoments(x, y, v, order,
                                       ref m00, ref m01, ref m10,
                                       ref m11, ref m02, ref m20,
                                       ref m12, ref m21, ref m30, ref m03);
                }

                imagePtr += image.Stride;
            }       
        }

        private unsafe static void computeFloat(IImage image, Point offset, int order,
                                                out float m00, out float m01, out float m10,
                                                out float m11, out float m02, out float m20,
                                                out float m12, out float m21, out float m30, out float m03)
        {
            m00 = m01 = m10 = m11 = m02 = m20 = m12 = m21 = m30 = m03 = 0;

            int width = image.Width;
            int height = image.Height;

            float* imagePtr = (float*)image.ImageData;

            for (int row = 0; row < height; row++)
            {
                float y = row + offset.Y;

                for (int col = 0; col < width; col++)
                {
                    float x = col + offset.X;
                    float v = imagePtr[col];

                    computeRawMoments(x, y, v, order,
                                       ref m00, ref m01, ref m10,
                                       ref m11, ref m02, ref m20,
                                       ref m12, ref m21, ref m30, ref m03);
                }

                imagePtr += image.Stride;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void computeRawMoments(float x, float y, float v, int order,
                                                     ref float m00, ref float m01, ref float m10,
                                                     ref float m11, ref float m02, ref float m20,
                                                     ref float m12, ref float m21, ref float m30, ref float m03)
        {
            m00 += v;
            m01 += y * v;
            m10 += x * v;

            if (order >= 2)
            {
                m11 += x * y * v;
                m02 += y * y * v;
                m20 += x * x * v;
            }

            if (order >= 3)
            {
                m12 += x * y * y * v;
                m21 += x * x * y * v;

                m30 += x * x * x * v;
                m03 += y * y * y * v;
            }
        }

        /// <summary>
        ///   Resets all moments to zero.
        /// </summary>
        /// 
        protected void Reset()
        {
            M00 = M10 = M01 = 0;
            M11 = M20 = M02 = 0;

            M21 = M12 = 0;
            M30 = M03 = 0;

            InvM00 = 0;

            CenterX = CenterY = 0;
        }
    }
}
