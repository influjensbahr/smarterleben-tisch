//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Animation;
using OTBT.Framework.Core;
using Sparrow.Verification;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    /// <summary>
    /// Single UI screen with functionality to show and hide, as well as some options for their behavior when hidden and on startup
    /// </summary>
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster))]
    public class UIScreen : MonoBehaviour, IVerify, IGamePauseTrigger
    {
        public enum ScreenMode
        {
            Exclusive,
            Additional
        }

        [SerializeField, HideInInspector] StringOrAtomReference<UIScreenAtom> m_ScreenTitle = new();
        [SerializeField,] ScreenMode m_ScreenMode = ScreenMode.Exclusive;
        [Tooltip("If set, this screen will not be revisited when using the BACK button in the application")]
        [SerializeField, WideToggle] bool m_IgnoreInStack;
        [Tooltip("Should Screens below be hidden when showing this Screen?")]
        [SerializeField, WideToggle] bool m_HideScreensBelow = true;
        [Tooltip("Should the game pause when this sceen is shown?")]
        [SerializeField, WideToggle] bool m_ScreenPausesGameplay = false;

        [Header("Show and Hide Animation")]
        [SerializeField]
        [RequireInterface(typeof(IUIAnimationWithCallback<float>))]
        Object m_ScreenAnimation;
        public IUIAnimationWithCallback<float> screenAnimation => m_ScreenAnimation as IUIAnimationWithCallback<float>;

        [Tooltip("Time for the animation in seconds")]
        [SerializeField] float m_TransitionDuration = 1f;

        [SerializeField]
        [RequireInterface(typeof(IUIScreenPreparations))]
        Object m_ScreenPreparation;
        public IUIScreenPreparations screenPreparation => m_ScreenPreparation as IUIScreenPreparations;

        [SerializeField, WideToggle] bool m_DisableGameObjectWhenHidden;
        [SerializeField, WideToggle] bool m_DisableCanvasWhenHidden = true;
        [SerializeField, WideToggle] bool m_DisableRaycasterWhenHidden = true;

        [Tooltip("Activate this to stretch the canvas to screen space on start, e.g. if you want to place them next to each other in the scene for easier editing.")]
        [SerializeField, WideToggle] bool m_StretchToScreenSpace = true;
        [SerializeField, WideToggle] bool m_OnStartInstantHide = true;

        public UnityEvent onBeforeShow;
        public UnityEvent onBeforeHide;
        public UnityEvent onCompleteShow;
        public UnityEvent onCompleteHide;

        [SerializeField, HideInInspector] Canvas m_Canvas;
        [SerializeField, HideInInspector] CanvasGroup m_CanvasGroup;
        [SerializeField, HideInInspector] GraphicRaycaster m_GraphicRaycaster;
        [SerializeField, HideInInspector] RectTransform m_RectTransform;

        readonly List<GraphicRaycaster> m_RayCasters = new();
        public StringOrAtomReference<UIScreenAtom> title => m_ScreenTitle;
        public bool ignoreInStack => m_IgnoreInStack;
        public bool hideScreensBelow => m_HideScreensBelow;
        public bool pausesGameplay => m_ScreenPausesGameplay;
        public bool isShowing => m_Shown;
        public bool isExclusive => m_ScreenMode == ScreenMode.Exclusive;
        bool m_Shown = false;

        string m_BufferedName = "";
        void OnValidate()
        {
            if (m_Canvas == null) m_Canvas = GetComponent<Canvas>();
            if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_GraphicRaycaster == null) m_GraphicRaycaster = GetComponent<GraphicRaycaster>();
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
        }

        void Awake()
        {
            if (m_Canvas == null) m_Canvas = GetComponent<Canvas>();
            if (m_CanvasGroup == null) m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_GraphicRaycaster == null) m_GraphicRaycaster = GetComponent<GraphicRaycaster>();
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();

            foreach (GraphicRaycaster raycaster in gameObject.GetComponentsInChildren<GraphicRaycaster>())
                if (raycaster.enabled)
                    m_RayCasters.Add(raycaster);

            if (screenAnimation == null)
            {
                m_ScreenAnimation = gameObject.AddComponent<UICanvasGroupFade>();
                ((UICanvasGroupFade)m_ScreenAnimation).SetCanvasGroup(GetComponent<CanvasGroup>());
            }

            if (m_StretchToScreenSpace)
            {
                m_RectTransform.offsetMin = Vector2.zero;
                m_RectTransform.offsetMax = Vector2.zero;
            }

            if (m_OnStartInstantHide) Hide(true);
        }

        public void Register()
        {
            UIScreenController.instance.RegisterScreen(this, m_ScreenMode);
        }

        public async Task PrepareShow()
        {
            if (screenPreparation == null) return;

            bool done = false;
            screenPreparation.PrepareShow(() => { done = true; });
            while (!done)
            {
                await Task.Yield();
            }
        }

        public async Task PrepareHide()
        {
            if (screenPreparation == null) return;

            bool done = false;
            screenPreparation.PrepareHide(() => { done = true; });
            while (!done)
            {
                await Task.Yield();
            }
        }

        public void Show(bool instant = false, bool invertAnimation = false)
        {
            if (m_CanvasGroup == null) return;
            if (isShowing) return;
            OnBeforeShow();
            if(m_ScreenPausesGameplay)
                EventManager.instance.PauseGame(this);
            
            transform.SetAsLastSibling();
            screenAnimation?.SetInverted(invertAnimation);
            screenAnimation?.ResetState();
            screenAnimation?.AnimateToState(UIAnimationStateLerper.SHOWN, instant ? 0f : m_TransitionDuration, 0f, () =>
            {
                OnShowComplete();
            });
        }

        public void Hide(bool instant = false, bool invertAnimation = false)
        {
            if (m_CanvasGroup == null) return;

            OnBeforeHide();
            if (m_ScreenPausesGameplay)
                EventManager.instance.ResumeGame(this);

            screenAnimation?.SetInverted(invertAnimation);
            screenAnimation?.AnimateToState(UIAnimationStateLerper.OUT, instant ? 0f : m_TransitionDuration, 0f, () =>
            {
                OnHideComplete();
            });
        }

        public void Hide()
        {
            Hide(false, false);
        }

        void OnBeforeShow()
        {
            UpdateName(true);
            m_Shown = true;
            if (m_DisableGameObjectWhenHidden)
                gameObject.SetActive(true);
            if (m_DisableCanvasWhenHidden)
                m_Canvas.enabled = true;
            if (m_DisableRaycasterWhenHidden)
            {
                m_GraphicRaycaster.enabled = true;
                foreach (GraphicRaycaster r in m_RayCasters)
                    r.enabled = true;
            }

            onBeforeShow?.Invoke();
        }

        void OnBeforeHide()
        {
            onBeforeHide?.Invoke();
        }

        void OnShowComplete()
        {
            onCompleteShow?.Invoke();
        }

        void OnHideComplete()
        {

            m_Shown = false;
            UpdateName(false);
            if (m_DisableGameObjectWhenHidden)
                gameObject.SetActive(false);
            if (m_DisableCanvasWhenHidden)
                m_Canvas.enabled = false;
            if (m_DisableRaycasterWhenHidden)
            {
                m_GraphicRaycaster.enabled = false;
                foreach (GraphicRaycaster r in m_RayCasters)
                    r.enabled = false;
            }
            onCompleteHide?.Invoke();
        }

        void UpdateName(bool shown)
        {
            if (m_BufferedName == "") m_BufferedName = gameObject.name;
            const string active = "[SHOWN]";
            const string notActive = "[HIDDEN]";

            string tag = shown ? active : notActive;
            gameObject.name = $"{m_BufferedName} {tag}";
        }

        public void SetAnimation(Object animation)
        {
            m_ScreenAnimation = animation;
        }


        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(!title.isEmpty, "Screen without a Title or Identifier", gameObject);
        }
    }
}
