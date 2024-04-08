// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Marc Freitag
//

using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Sparrow.BugTracking
{
    public class UIDebugMenu : MonoBehaviour
    {
        [SerializeField] TMP_InputField m_TextInputField = default;
        [SerializeField] TMP_InputField m_TextInputFieldHeadline = default;
        [SerializeField] Button m_SendButton = default;
        [SerializeField] Button m_ScreenshotButton = default;
        [SerializeField] Image m_ScreenshotPreview = default;
        [SerializeField] TextMeshProUGUI m_ButtonLabel = default;
        [SerializeField] ScreenshotHandler m_ScreenshotHandler = default;
        
        [SerializeField] UnityEvent m_OnSendEvent = null;
        Sprite m_Screenshot;
        Canvas m_DebugWindowCanvas;

        void Start()
        {
            m_SendButton?.onClick.AddListener(LogSendenButtonPressed);
            m_ScreenshotButton?.onClick.AddListener(TakeScreenshot);
            m_DebugWindowCanvas = GetComponent<Canvas>();
        }

        public async void LogSendenButtonPressed()
        {
            m_ButtonLabel.text = "Sending...";
            await BugTrackingManager.instance.SendReport(m_TextInputFieldHeadline.text, m_TextInputField.text, m_Screenshot);
            m_TextInputField.text = "";
            m_ButtonLabel.text = "Success!";
            m_OnSendEvent?.Invoke();
            await Task.Delay(5 * 1000);
            m_ButtonLabel.text = "Send to developers";
        }
        
        public async void TakeScreenshot()
        {
            m_DebugWindowCanvas.enabled = false;
            while (m_ScreenshotHandler.GetScreenshot() == null)
            {
                await Task.Yield();
            }
            var screenshot = m_ScreenshotHandler.GetScreenshot();
            m_DebugWindowCanvas.enabled = true;
            UpdateScreenshotPreview(screenshot);
        }

        void UpdateScreenshotPreview(Sprite screenshot)
        {
            m_Screenshot = screenshot;
            m_ScreenshotPreview.sprite = screenshot;
        }
    }
}
