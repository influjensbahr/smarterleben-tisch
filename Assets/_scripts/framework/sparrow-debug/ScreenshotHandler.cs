// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Marc Freitag
//

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sparrow.BugTracking
{
    public class ScreenshotHandler : Singleton<ScreenshotHandler>
    {
        RenderTexture renderTexture = null;
        Sprite m_ScreenshotSprite = null;
        Coroutine m_ScreenshotCoroutine = null;

        IEnumerator CoroutineTakeScreenshot(Action<Sprite> callBackOnFinish = null)
        {
            yield return new WaitForEndOfFrame();
            
            renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(renderTexture);
            AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32, ReadbackCompleted);
            callBackOnFinish?.Invoke(m_ScreenshotSprite);
        }
        
        void ReadbackCompleted(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.LogError("GPU readback error detected.");
                return;
            }

            DestroyImmediate(renderTexture);

            using (var imageBytes = request.GetData<byte>())
            {
                var screenshotTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
                screenshotTex.LoadRawTextureData(imageBytes.ToArray());
                screenshotTex.Apply();

                // Create a new texture and set its pixels to the reverse of the original texture's pixels in each column
                var flippedTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
                var pixels = screenshotTex.GetPixels32();

                for (int x = 0; x < screenshotTex.width; x++)
                {
                    for (int y = 0; y < screenshotTex.height / 2; y++)
                    {
                        int mirrorY = screenshotTex.height - y - 1;
                        Color temp = pixels[y * screenshotTex.width + x];
                        pixels[y * screenshotTex.width + x] = pixels[mirrorY * screenshotTex.width + x];
                        pixels[mirrorY * screenshotTex.width + x] = temp;
                    }
                }

                flippedTex.SetPixels32(pixels);
                flippedTex.Apply();

                m_ScreenshotSprite = Sprite.Create(flippedTex, new Rect(0, 0, Screen.width, Screen.height), Vector2.zero);
            }
        }
        
        public Sprite GetScreenshot()
        {
            if (m_ScreenshotCoroutine != null) StopCoroutine(m_ScreenshotCoroutine);
            m_ScreenshotCoroutine = StartCoroutine(CoroutineTakeScreenshot());
            return m_ScreenshotSprite;
        }
    }
}
