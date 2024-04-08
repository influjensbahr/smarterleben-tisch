// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 


using OTBT.Framework.Animation;
using OTBT.Framework.Core;
using OTBT.Framework.Localization;
using Sparrow.Verification;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace OTBT.Framework.UI
{
    /// <summary>
    /// Script to be placed on UI elements that display dialogue lines
    /// </summary>
    public class SingleNotificationUI : MonoBehaviour, IVerify
    {
        [SerializeField, VRequired] Image m_NotificationIcon = default;
        [SerializeField, VRequired] TextMeshProUGUI m_LineDisplay = default;
        [SerializeField, VRequired] LocalizeTextUI m_LineDisplayLocalized = default;
        [SerializeField, VRequired] GameObject m_CloseButton = default;
        [SerializeField] static float s_FadeOutTime = 0.5f;
        [SerializeField] static float s_FadeInTime = 0.5f;
        [SerializeField] static float s_DisplayTime = 5f;

        [SerializeField]
        [RequireInterface(typeof(IUIAnimationWithCallback<float>))]
        private UnityEngine.Object m_FadingAnimation;

        public enum NotificationType
        {
            TIMED, CLICKED
        }

        NotificationType m_NotificationType = NotificationType.TIMED;

        UnityAction m_OnHideCallback = null;
        bool m_HasEnded = false;
        public Image notificationIcon => m_NotificationIcon;
        public bool hasEnded => m_HasEnded;
        public IUIAnimationWithCallback<float> fading => m_FadingAnimation as IUIAnimationWithCallback<float>;

        NotificationDisplayUI m_NotificationDisplay = null;

        public NotificationDisplayUI display => m_NotificationDisplay;

        public void ResetState()
        {
            m_HasEnded = false;
            fading?.ResetState();
        }

        public void Setup(NotificationDisplayUI disp, string lineText = "",  LocalizedTextObject locaObj = null, Sprite notificationIcon = null, NotificationType type = NotificationType.TIMED)
        {
            ResetState();
            m_NotificationDisplay = disp;

            // set text
            if (locaObj == null)
            {
                m_LineDisplayLocalized.enabled = false;
                m_LineDisplay.text = lineText;
            } else
            {
                m_LineDisplayLocalized.SetLocalizedTextObject(locaObj);
                m_LineDisplayLocalized.enabled = true;
            }


            // set icon
            if (m_NotificationIcon != null)
            {
                if (notificationIcon != null)
                {
                    m_NotificationIcon.gameObject.SetActive(true);
                    m_NotificationIcon.sprite = notificationIcon;
                } else
                {
                    m_NotificationIcon.gameObject.SetActive(false);
                }
            }

            m_NotificationType = type;
            m_CloseButton.gameObject.SetActive(type == NotificationType.CLICKED);
            

            m_HasEnded = false;
        }

        public void TriggerStart(UnityAction callback = null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            m_OnHideCallback = callback;

            fading?.ResetState();
            fading?.AnimateToState(UIAnimationStateLerper.SHOWN, s_FadeInTime);

            if(m_NotificationType == NotificationType.TIMED)
            {
                EventManager.instance.TriggerInTime(s_FadeInTime + s_DisplayTime, () => TriggerEnding());
            }
        }

        public bool IsDisplaying(string line = "", LocalizedTextObject loca = null)
        {
            if(m_LineDisplayLocalized != null && m_LineDisplayLocalized.enabled)
            {
                return m_LineDisplayLocalized.localizedText == loca;
            }
            return m_LineDisplay.text.Equals(line);
        }

        public void CloseClicked()
        {
            TriggerEnding();
        }

        internal void TriggerEnding()
        {
            m_HasEnded = true;
            if (fading == null)
            {
                m_OnHideCallback?.Invoke();
            }
            else
            {
                fading.AnimateToState(UIAnimationStateLerper.OUT, s_FadeOutTime, 1f, m_OnHideCallback);
            }
        }


        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}
