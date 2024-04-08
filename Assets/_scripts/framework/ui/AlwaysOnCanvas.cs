//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Animation;
using OTBT.Framework.Core;
using OTBT.Framework.Debugging;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    /// <summary>
    /// A convenient singleton manager that allows for fullscreen fades to black at any time. You can also provide a callback that is to be executed once the screen is black. Note that fading back to visible has to be triggered by hand.
    /// </summary>
    public class AlwaysOnCanvas : Singleton<AlwaysOnCanvas>, IVerify, IPrepareOnBuild
    {
        [Header("Fade to black")]
        [SerializeField]
        [RequireInterface(typeof(IUIAnimationWithCallback<float>))]
        private UnityEngine.Object m_ScreenAnimation;
        public IUIAnimationWithCallback<float> screenAnim => m_ScreenAnimation as IUIAnimationWithCallback<float>;
        [SerializeField] float m_FadeTime = 1f;
        [SerializeField, VRequired] CanvasGroup m_FadeToBlackCanvasGroup;

        [Header("Loading and saving")]
        [SerializeField, VRequired] Slider m_LoadingSlider;
        [SerializeField, VRequired] CanvasGroup m_LoadingSliderCanvasGroup;
        [SerializeField]
        [RequireInterface(typeof(IUIAnimationWithCallback<float>))]
        private UnityEngine.Object m_FadeForSaveIcon;
        public IUIAnimationWithCallback<float> saveIconFade => m_FadeForSaveIcon as IUIAnimationWithCallback<float>;
        [SerializeField] CanvasGroup m_SavingIconCanvasGroup;

        [Header("Debug")]
        [SerializeField, VRequired] UICanvasGroupFade m_DebugMenu = default;
        [SerializeField, VRequired] TextMeshProUGUI m_BuildNumberText = default;
        [SerializeField] string m_BuildNumber = "";
        [SerializeField, VRequired] Button m_DebugMenuButton = default;

        public float remainingTime => screenAnim.RemainingTime();
        float m_SliderInitialTime = 0f;

        private void Start()
        {
            if(!Debug.isDebugBuild && !Application.isEditor)
            {
                if (m_DebugMenuButton) m_DebugMenuButton.gameObject.SetActive(false);
                if (m_BuildNumberText) m_BuildNumberText.gameObject.SetActive(false);
                if (m_DebugMenu) m_DebugMenu.gameObject.SetActive(false);
            } else
            {
                if (m_DebugMenu) m_DebugMenu.gameObject.SetActive(false);
                m_DebugMenu.AnimateToState(UIAnimationStateLerper.START, 0f);
            }
            m_FadeToBlackCanvasGroup.blocksRaycasts = false;
            m_LoadingSlider.value = 0f;
            m_BuildNumberText.text = $"Build date: {m_BuildNumber}";
            ErrorLogManager.instance.AddLog($"Build date: {m_BuildNumber}");
        }

        public void ShowDebugMenu()
        {
            Debug.Log("Showing");
            m_DebugMenu.gameObject.SetActive(true);
            m_DebugMenu.AnimateToState(UIAnimationStateLerper.SHOWN, 1f);
        }

        public void HideDebugMenu()
        {
            m_DebugMenu.AnimateToState(UIAnimationStateLerper.OUT, 1f);
            EventManager.instance.TriggerInTime(1f, () => m_DebugMenu.gameObject.SetActive(false));
        }

        public void SetSliderValue(float value)
        {
            if (value == 0) m_LoadingSliderCanvasGroup.alpha = 0f;
            if (m_LoadingSlider.value < 0.001f && value > 0f)
                m_SliderInitialTime = Time.time;
            m_LoadingSlider.value = value;
        }

        void Update()
        {
            if (m_LoadingSlider.value < 0.001f) return;
            m_LoadingSliderCanvasGroup.alpha = Mathf.Lerp(m_LoadingSliderCanvasGroup.alpha, 1f, Time.deltaTime);
        }

        public float StartFadeToBlack(Action whenBlackCallback = null, bool blocksRaycasts = true ,bool instant = false, float duration = -1f)
        {
            if(m_FadeToBlackCanvasGroup != null)
                m_FadeToBlackCanvasGroup.blocksRaycasts = blocksRaycasts;
            screenAnim.AnimateToState(UIAnimationStateLerper.SHOWN, instant ? 0f : (duration < 0 ? m_FadeTime : duration), 0f, () => whenBlackCallback?.Invoke());
            return (duration < 0 ? m_FadeTime : duration);
        }

        public float StartFadeToVisible(Action whenBlackCallback = null, bool instant = false, float duration = -1f)
        {
            screenAnim.AnimateToState(UIAnimationStateLerper.START, instant ? 0f : (duration < 0 ? m_FadeTime : duration), 0f, () =>
            {
                if (m_FadeToBlackCanvasGroup != null)
                    m_FadeToBlackCanvasGroup.blocksRaycasts = false;
                whenBlackCallback?.Invoke();
            });
            return (duration < 0 ? m_FadeTime : duration);
        }

        public float ShowSaveIcon()
        {
            saveIconFade.AnimateToState(UIAnimationStateLerper.SHOWN, m_FadeTime, 0f, null);
            return m_FadeTime;
        }

        public void HideSaveIcon(float delay = 2f)
        {
            EventManager.instance.TriggerInTime(delay, () => saveIconFade.AnimateToState(UIAnimationStateLerper.START, m_FadeTime));
        }

        public void Verify(CheckVerifyInterface checker) {}

        public void PrepareOnBuildOrAwake()
        {
            m_BuildNumber = DateTime.Now.ToString("d");
        }
    }
}