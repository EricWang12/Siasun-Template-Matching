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

 

using System;
using Accord.Extensions;
using Accord.Extensions.Imaging;
using Accord.Extensions.Imaging.Filters;
using DotImaging;



namespace Accord.Extensions.Imaging.Algorithms.LINE2D
{
    /// <summary>
    /// Image template pyramid. See <see cref="ImageTemplate"/> for details.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ImageTemplatePyramid<T>: ITemplatePyramid, ITemplatePyramid<T>
        where T: ImageTemplate, new()
    {
        /// <summary>
        ///number of features per pyramid level.
        /// </summary>
        public const int PYRLEVEL = 0;
        /*
        /// <summary>
        /// Minimum number of features per template.
        /// </summary>
        static int DEFAULT_MIN_FEATURES = 30; //not used - suppress the compiler warning
         */

        /// <summary>
        /// Maximum number of features per pyramid level.
        /// </summary>
        static int[] DEFAULT_MAX_FEATURES_PER_LEVEL = new int[] { 400 };//, 100 / 2 }; //bigger image towards smaller one   

        /// <summary>
        /// Gets the image templates. One for each pyramid level.
        /// </summary>
        public T[] Templates { get;  set; }

        /// <summary>
        /// Creates an empty image template pyramid. Requires initialization.
        /// </summary>
        public ImageTemplatePyramid() { }

        /// <summary>
        /// Initialized image template pyramid with the provided templates.
        /// </summary>
        /// <param name="templates"></param>
        public void Initialize(T[] templates)
        { 
            this.Templates = templates;
        }

        /// <summary>
        /// Creates image templates pyramid from the input image by using the provided parameters.
        /// </summary>
        /// <param name="sourceImage">Input image.</param>
        /// <param name="classLabel">Templates class label.</param>
        /// <param name="angle"></param>
        /// <param name="minFeatureStrength">Minimum feature's gradient strength.</param>
        /// <param name="minNumberOfFeatures">Minimum number of features. If the number of features is less that minimum null will be returned.</param>
        /// <param name="maxNumberOfFeaturesPerLevel">Maximum number of features per template. The features will be scattered semi-uniformly.</param>
        /// <returns>Image template pyramid.</returns>
        public static ImageTemplatePyramid<T> CreatePyramid(Bgr<byte>[,] sourceImage, string classLabel, float angle,int minFeatureStrength = 40, int minNumberOfFeatures = 30, int[] maxNumberOfFeaturesPerLevel = null)
        { 
            return CreatePyramid<Bgr<byte>>(sourceImage, classLabel, angle,
                                            (image, minFeatures, maxFeatures, label, Angle) => 
                                            {
                                                var t = new T();
                                                t.Initialize(sourceImage, minFeatureStrength, maxFeatures, label, angle);
                                                return t;
                                            },
                                            minNumberOfFeatures, maxNumberOfFeaturesPerLevel);
        }

        /// <summary>
        /// Creates image templates pyramid from the black-white image by using the provided parameters.
        /// </summary>
        /// <param name="sourceImage">Input image.</param>
        /// <param name="classLabel">Templates class label.</param>
        /// <param name="angle"></param>
        /// <param name="minFeatureStrength">Minimum feature's gradient strength.</param>
        /// <param name="minNumberOfFeatures">Minimum number of features. If the number of features is less that minimum null will be returned.</param>
        /// <param name="maxNumberOfFeaturesPerLevel">Maximum number of features per template. The features will be scattered semi-uniformly.</param>
        /// <returns>Image template pyramid.</returns>
        public static ImageTemplatePyramid<T> CreatePyramidFromPreparedBWImage(Gray<byte>[,] sourceImage, string classLabel, float angle, int minFeatureStrength = 40, int minNumberOfFeatures = 30, int[] maxNumberOfFeaturesPerLevel = null)
        {
            return CreatePyramid<Gray<byte>>(sourceImage, classLabel, angle,
                                            (image, minFeatures, maxFeatures, label, Angle) =>
                                            {
                                                var t = new T();
                                                t.Initialize(sourceImage, minFeatureStrength, maxFeatures, label, Angle);
                                                return t;
                                            },
                                            minNumberOfFeatures, maxNumberOfFeaturesPerLevel);
        }

        private static ImageTemplatePyramid<T> CreatePyramid<TColor>(TColor[,] sourceImage, string classLabel, float angle,
                                                                     Func<TColor[,], int, int, string, float, T> templateCreationFunc,
                                                                     int minNumberOfFeatures, int[] maxNumberOfFeaturesPerLevel = null)
            where TColor: struct, IColor<byte>
        {
            maxNumberOfFeaturesPerLevel = maxNumberOfFeaturesPerLevel ?? DEFAULT_MAX_FEATURES_PER_LEVEL;

            int nPyramids = maxNumberOfFeaturesPerLevel.Length;
            T[] templates = new T[nPyramids];
            var image = sourceImage;

            bool isValid = true;
            for (int pyrLevel = PYRLEVEL; pyrLevel < nPyramids; pyrLevel++)
            {
                if (pyrLevel > 0)
                    image = image.PyrDown();

                var newTemplate = templateCreationFunc(image, minNumberOfFeatures, maxNumberOfFeaturesPerLevel[pyrLevel], classLabel ,angle);
                templates[pyrLevel] = newTemplate;

                if (templates[pyrLevel].Features.Length < minNumberOfFeatures) //if there is no enough features mark Pyramid as invalid
                {
                    Console.WriteLine("there is no enough features mark Pyramid as invalid");
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                var pyr = new ImageTemplatePyramid<T>(); pyr.Initialize(templates);
                return pyr;
            }
            else
                return null;
        }

        #region ITemplatePyramid Interface

        ITemplate[] ITemplatePyramid.Templates
        {
            get
            {
                return this.Templates;
            }
        }

        #endregion
    }
}
