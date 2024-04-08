//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Helper functions for manipulating colors quickly
    /// </summary>
    public class ColorUtils
    {
        [Obsolete("Clone() is obsolete, copy Color structs normally instead.")]
        public static Color Clone(Color color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }

        public static Color TowardsWhite(Color color, float factor)
        {
            return Color.Lerp(color, Color.white, factor);
        }

        public static Color TowardsBlack(Color color, float factor)
        {
            return Color.Lerp(color, Color.black, factor);
        }

        [Obsolete("TowardsColor() is obsolete, use Color.Lerp() instead.")]
        public static Color TowardsColor(Color from, Color to, float factor)
        {
            return Color.Lerp(from, to, factor);
        }

        public static Color Scale(Color color, float factor)
        {
            return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
        }

        public static Color ToGreyscaleMean(Color color)
        {
            float average = (color.r + color.g + color.b) / 3f;
            return new Color(average, average, average, color.a);
        }

        /// <summary>
        /// Transforms a color to greyscale but uses channel-dependent luminance perception for more pleasing result
        /// See https://e2eml.school/convert_rgb_to_grayscale.html
        /// </summary>
        public static Color ToGreyscaleAdjusted(Color color)
        {
            float avg = (color.r * 0.3f + color.g * 0.59f + color.b * 0.11f) / 3f;
            return new Color(avg, avg, avg, color.a);
        }

        public static Color SetRed(Color color, float red)
        {
            color.r = red;
            return color;
        }

        public static Color SetGreen(Color color, float green)
        {
            color.g = green;
            return color;
        }

        public static Color SetBlue(Color color, float blue)
        {
            color.b = blue;
            return color;
        }

        public static Color SetAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color ScaleAlpha(Color color, float factor)
        {
            color.a *= factor;
            return color;
        }
    }
}
