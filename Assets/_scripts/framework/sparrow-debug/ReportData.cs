// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using UnityEngine;

namespace Sparrow.BugTracking
{
    public struct ReportData
    {
        public string log;
        public string headline;
        public string userComment;
        public Sprite screenshot;

        public string fullLog => userComment.Equals("") ? log : ("User comment: " + userComment + "\n\n" + log);

        public byte[] GetScreenshotByteArray(int maxSize = 10 * 1024 * 1024)
        {
            if (screenshot == null) return null;
            Texture2D texture = screenshot.texture;

            byte[] screenshotBytes = ImageConversion.EncodeToPNG(texture);

            // Compress the image if it's too large
            if (screenshotBytes.Length > maxSize) // 10 MB
            {
                int quality = 100;
                while (screenshotBytes.Length > maxSize && quality >= 0) // 10 MB
                {
                    quality = Mathf.Max(0, quality-10);
                    screenshotBytes = texture.EncodeToJPG(quality);
                }
            }
            return screenshotBytes;
        }
    }
}
