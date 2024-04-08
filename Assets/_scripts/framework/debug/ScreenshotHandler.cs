// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Marc Freitag
//

using System;
using System.Collections;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using UnityEngine;
using UnityEngine.Rendering;

namespace OTBT.Framework.Debugging
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
                Array.Reverse(screenshotTex.GetPixels());
                m_ScreenshotSprite = Sprite.Create(screenshotTex, new Rect(0, 0, Screen.width, Screen.height), Vector2.zero);
            }
            
        }
        
        public Sprite GetScreenshot()
        {
            if (m_ScreenshotCoroutine != null) StopCoroutine(m_ScreenshotCoroutine);
            m_ScreenshotCoroutine = StartCoroutine(CoroutineTakeScreenshot());
            return m_ScreenshotSprite;
        }
        
        public byte[] GetScreenshotBytes()
        {
            GetScreenshot();
            while (m_ScreenshotCoroutine != null) { }
            return ConvertSpriteToBytes(m_ScreenshotSprite);
        }

        private byte[] ConvertSpriteToBytes(Sprite sprite)
        {
            Texture2D texture = sprite.texture;
            return texture.EncodeToPNG();
        }
        
        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}
