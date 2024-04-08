// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using System;
using OTBT.Framework.Core;
using OTBT.Framework.Utils;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using Sparrow.Verification;
using UnityEditor;
using System.Linq;

namespace OTBT.Framework.UI
{
    /// <summary>
    ///     Simple controller for which UI Screen is displayed currently. Can show and hide screens, and save them in a stack
    ///     mode
    /// </summary>
    public class UIScreenController : Singleton<UIScreenController>, IVerify
    {
        public enum ScreenflowMode
        {
            Stack,
            Single
        }

        [SerializeField] ScreenflowMode m_ScreenFlowModeMode = ScreenflowMode.Single;

        [Header("Pause game")]
        [SerializeField] StringOrAtomReference<EventAtom> m_PauseTrigger = default;
        [SerializeField] StringOrAtomReference<UIScreenAtom> m_PauseScreen = default;
        [SerializeField] List<UIScreen> m_PauseBlockers = new List<UIScreen>();

        [Tooltip("Only one of these UIScreens can be active at any given time.")]
        readonly List<UIScreen> m_MutuallyExclusiveScreens = new();
        readonly List<UIScreen> m_AdditionalScreens = new();

        public UnityAction<UIScreen> onShowAnyScreen = null;

        public ScreenflowMode screenFlowMode => m_ScreenFlowModeMode;
        public UIScreen currentScreen => m_Strategy.currentScreen;
        ScreenFlowStrategy m_Strategy;
        public ScreenFlowStrategy strategy => m_Strategy;

        public void AddScreen(UIScreen screen)
        {
            m_AdditionalScreens.Add(screen);
        }

        public void RemoveScreen(UIScreen screen)
        {
            m_AdditionalScreens.Remove(screen);
        }

        void Awake()
        {
            m_Strategy = m_ScreenFlowModeMode switch {
                ScreenflowMode.Stack => new ScreenFlowStrategyStack(),
                ScreenflowMode.Single => new ScreenFlowStrategySingle(),
                _ => throw new ArgumentOutOfRangeException()
            };

            var screens = FindObjectsOfType<UIScreen>();
            foreach (var screen in screens)
            {
                screen.Register();
            }
        }

        void Start()
        {

            if (!m_PauseTrigger.isEmpty && !m_PauseScreen.isEmpty)
            {
                EventManager.instance.StartListening(m_PauseTrigger, TriggerPauseDisplay);
            }
        }

        void TriggerPauseDisplay()
        {
            foreach (UIScreen blockerscreen in m_PauseBlockers)
                if (blockerscreen.isShowing) return;

            GetScreen(m_PauseScreen, out UIScreen screen);
            if (screen == null) return;

            if (screen.isShowing)
            {
                HideScreen(screen);
            } else
            {
                ShowScreen(screen);
            }
        }

        /// <summary>
        ///     Hides the current screen.
        /// </summary>
        public void HideCurrent()
        {
            m_Strategy.HideScreen(m_Strategy.currentScreen);
        }

        public bool IsShowing(StringOrAtomReference<UIScreenAtom> screenName)
        {
            bool screen_is_exclusive = GetScreen(screenName, out UIScreen screen);
            return screen.isShowing;
        }

        public void ShowScreen(UIScreenAtom screenName, bool instant = false, bool invertAnimation = false, bool returnToFlowStrategyPosition = false)
        {
            GetScreen(screenName, out UIScreen screen);
            ShowScreen(screen, instant, invertAnimation, returnToFlowStrategyPosition);
        }

        public void ShowScreen(StringOrAtomReference<UIScreenAtom> screenName, bool instant = false, bool invertAnimation = false, bool returnToFlowStrategyPosition = false)
        {
            GetScreen(screenName, out UIScreen screen);
            ShowScreen(screen, instant, invertAnimation, returnToFlowStrategyPosition);
        }

        public void ShowScreen(UIScreen screen, bool instant = false, bool invertAnimation = false, bool returnToFlowStrategyPosition = false)
        {
            if (screen == null) return;
            if (screen.isExclusive)
            {
                onShowAnyScreen?.Invoke(screen);
                m_Strategy.ShowScreen(screen, instant, invertAnimation, returnToFlowStrategyPosition);
            } else
            {
                screen.Show(instant);
            }

        }

        public void HideScreen(StringOrAtomReference<UIScreenAtom> screenName, bool instant = false, bool returnToFlowStrategyPosition = false)
        {
            GetScreen(screenName, out UIScreen screen);
            HideScreen(screen, instant, returnToFlowStrategyPosition);
        }

        public void HideScreen(UIScreen screen, bool instant = false, bool returnToFlowStrategyPosition = false)
        {
            if (screen == null) return;
            if (screen.isExclusive)
            {
                m_Strategy.HideScreen(screen, instant, returnToFlowStrategyPosition: returnToFlowStrategyPosition);
            }
            else
            {
                screen.Hide(instant); 
            }
        }

        public void HideAll(bool instant = false)
        {
            m_Strategy.ResetState();
            foreach (UIScreen screen in m_MutuallyExclusiveScreens)
            {
                if(screen.isShowing)
                    screen.Hide(instant);
            }
        }

        public bool GetScreen(StringOrAtomReference<UIScreenAtom> screenReference, out UIScreen retScreen)
        {
            if (screenReference.atom != null) return GetScreen(screenReference.atom, out retScreen);

            foreach (UIScreen screen in m_MutuallyExclusiveScreens)
            {
                if (screen.title.ToString().Equals(screenReference.ToString()))
                {
                    retScreen = screen;
                    return true;
                }
            }

            foreach (UIScreen screen in m_AdditionalScreens)
            {
                if (screen.title.ToString().Equals(screenReference.ToString()))
                {
                    retScreen = screen;
                    return false;
                }
            }

            retScreen = null;
            return false;
        }

        public bool GetScreen(UIScreenAtom screenReference, out UIScreen retScreen)
        {
            foreach (UIScreen screen in m_MutuallyExclusiveScreens)
            {
                if (screen.title.atom == screenReference)
                {
                    retScreen = screen;
                    return true;
                }
            }

            foreach (UIScreen screen in m_AdditionalScreens)
            {
                if (screen.title.atom == screenReference)
                {
                    retScreen = screen;
                    return false;
                }
            }

            retScreen = null;
            return false;
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.CheckElementsNotNull(m_MutuallyExclusiveScreens, nameof(m_MutuallyExclusiveScreens), this.gameObject);
            checker.CheckElementsNotNull(m_AdditionalScreens, nameof(m_AdditionalScreens), this.gameObject);
        }

        public void RegisterScreen(UIScreen uiScreen, UIScreen.ScreenMode screenMode)
        {
            if (screenMode == UIScreen.ScreenMode.Exclusive)
                m_MutuallyExclusiveScreens.Add(uiScreen);
            else if (screenMode == UIScreen.ScreenMode.Additional)
                m_AdditionalScreens.Add(uiScreen);
        }
    }
    public abstract class ScreenFlowStrategy
    {
        public virtual UIScreen currentScreen { get; protected set; }

        public abstract void ResetState();

#if UNITY_EDITOR
        public abstract void PrintDebugStateGUI();
#endif

        public abstract Task ShowScreen(UIScreen screen, bool instant = false, bool invertAnimation = false, bool returnToFlowStrategyPosition = false);
        public abstract Task HideScreen(UIScreen screen, bool instant = false, bool invertAnimation = false, bool returnToFlowStrategyPosition = false);

    }

    public class ScreenFlowStrategySingle : ScreenFlowStrategy
    {
        public override UIScreen currentScreen { get; protected set; }

        public override void ResetState() {
            currentScreen = null;
        }

#if UNITY_EDITOR
        public override void PrintDebugStateGUI()
        {
            if(currentScreen != null)
                EditorGUILayout.HelpBox($"Current screen: {currentScreen.name}", MessageType.Info);
        }
#endif

        public override async Task ShowScreen(UIScreen screen, bool instant = false, bool invertAnimation = false, bool ignoreReturnRules = false)
        {
            if (screen.hideScreensBelow && currentScreen != null) await currentScreen.PrepareHide();
            await screen.PrepareShow();

            if (screen.hideScreensBelow && currentScreen != null) currentScreen.Hide(instant, invertAnimation:invertAnimation);
            currentScreen = screen;
            currentScreen.Show(invertAnimation:invertAnimation);
        }
        public override async Task HideScreen(UIScreen screen, bool instant = false, bool invertAnimation = false, bool ignoreReturnRules = false)
        {
            if (currentScreen != null) await currentScreen.PrepareHide();
            if (currentScreen == screen) currentScreen = null;
            screen.Hide(instant, invertAnimation:invertAnimation);
        }
    }

    public class ScreenFlowStrategyStack : ScreenFlowStrategy
    {
        public override UIScreen currentScreen => m_Screens.Count > 0 ? m_Screens.Peek() : null;

        readonly Stack<UIScreen> m_Screens = new Stack<UIScreen>();

        public override void ResetState()
        {
            m_Screens.Clear();
        }

#if UNITY_EDITOR
        public override void PrintDebugStateGUI()
        {
            string stackMessage = "";
            for(int i = 0; i < m_Screens.Count; i++)
            {
                stackMessage += m_Screens.ElementAt<UIScreen>(i).name + "\n";
            }
            EditorGUILayout.HelpBox($"Current screen stack: \n\n{stackMessage}", MessageType.Info);
        }
#endif

        public override async Task ShowScreen(UIScreen screen, bool instant = false, bool invertAnimation = false, bool returnToPositionInFlowStrategy = false)
        {
            // wait until everything is ready
            if (screen.hideScreensBelow && currentScreen != null && currentScreen != screen) await currentScreen.PrepareHide();
            await screen.PrepareShow();
            if (screen.hideScreensBelow && currentScreen != null && currentScreen != screen) currentScreen.Hide(instant, invertAnimation:invertAnimation);
            screen.Show(instant, invertAnimation:invertAnimation);

            if (returnToPositionInFlowStrategy)
                ReturnTo(screen);
            m_Screens.Push(screen);
        }

        void ReturnTo(UIScreen screen)
        {
            Debug.Log("Return to: " + screen);
            if (m_Screens.Contains(screen))
            {
                while (currentScreen != screen && m_Screens.Count > 0)
                {
                    Debug.Log("Pop!");
                    m_Screens.Pop();
                }
                Debug.Log("Pop!");
                m_Screens.Pop();
            }
        }

        public override async Task HideScreen(UIScreen screen, bool instant = false, bool invertAnimation = false, bool ignoreReturnRules = true)
        {
            if (screen != currentScreen) return;

            if (currentScreen != null && currentScreen != screen) await currentScreen.PrepareHide();
            UIScreen toHide = currentScreen;

            m_Screens.Pop();
            if (screen.hideScreensBelow && currentScreen != null) await currentScreen.PrepareShow();

            toHide.Hide(instant, invertAnimation:invertAnimation);
            if (screen.hideScreensBelow && currentScreen != null) currentScreen.Show(invertAnimation:invertAnimation);
        }
    }

}
