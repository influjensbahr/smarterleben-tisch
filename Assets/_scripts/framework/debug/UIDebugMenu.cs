using Sparrow.Verification;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OTBT.Framework.Debugging
{
    public class UIDebugMenu : MonoBehaviour, IVerify
    {
        [SerializeField, VRequired] TMP_InputField m_TextInputField = default;
        [SerializeField, VRequired] Button m_SendButton = default;
        [SerializeField, VRequired] Button m_ScreenshotButton = default;
        [SerializeField, VRequired] Image m_ScreenshotPreview = default;
        [SerializeField, VRequired] TextMeshProUGUI m_ButtonLabel = default;
        [SerializeField, VRequired] ScreenshotHandler m_ScreenshotHandler = default;
        
        [SerializeField] UnityEvent m_OnSendEvent = null;
        Sprite m_Screenshot;

        private void Start()
        {
            m_SendButton?.onClick.AddListener(LogSendenButtonPressed);
            m_ScreenshotButton?.onClick.AddListener(TakeScreenshot);
        }

        public async void LogSendenButtonPressed()
        {
            m_ButtonLabel.text = "Sende...";
            await ErrorLogManager.instance.SendLog(m_TextInputField.text, m_Screenshot);
            m_TextInputField.text = "";
            m_ButtonLabel.text = "Erfolg!";
            m_OnSendEvent?.Invoke();
            await Task.Delay(5 * 1000);
            m_ButtonLabel.text = "An Entwickler senden";
        }
        
        public async void TakeScreenshot()
        {
            print("Waiting for screenshot");
            while (m_ScreenshotHandler.GetScreenshot() == null)
            {
                await Task.Yield();
            }
            var screenshot = m_ScreenshotHandler.GetScreenshot();
            UpdateScreenshotPreview(screenshot);
            print("Screenshot taken");
        }

        void UpdateScreenshotPreview(Sprite screenshot)
        {
            m_Screenshot = screenshot;
            m_ScreenshotPreview.sprite = screenshot;
        }

        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}
