//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

using UnityEngine;
using OTBT.Framework.Core;
using OTBT.Framework.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
    [System.Serializable]
    public class UIScreenAction : Action
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "UI/Show or Hide UI Screen";
        public override string Description => "Shows or hides a specific UI screen";

        // Declare variables here
        [SerializeField] StringOrAtomReference<UIScreenAtom> m_TargetScreen = new();
        [SerializeField] bool m_Instant = true;
        [SerializeField] bool m_Hide = true;

        UIScreen m_Screen;
        bool m_AnimationComplete = false;
        bool m_HasStarted = false;
    
        public override float Run()
        {
            if(isRunning)
            {
                if (m_AnimationComplete)
                {
                    m_AnimationComplete = false;
                    isRunning = false;
                    return 0f;
                }
                return 0.1f;
            } else
            {
                if (!m_HasStarted)
                {
                    if (!UIScreenController.instance.GetScreen(m_TargetScreen, out m_Screen)) return defaultPauseTime;

                    if (m_Hide) m_Screen.onCompleteHide.AddListener(WaitForHide);
                    if (!m_Hide) m_Screen.onCompleteShow.AddListener(WaitForShow);
                    if (!m_Hide)
                        UIScreenController.instance.ShowScreen(m_TargetScreen, m_Instant);
                    if (m_Hide)
                        UIScreenController.instance.HideScreen(m_TargetScreen, m_Instant);

                    if (m_Instant) return defaultPauseTime;

                    m_AnimationComplete = false;

                    isRunning = true;
                    willWait = true;
                    m_HasStarted = true;
                }
                return 0.1f;
            }
        }

        void WaitForShow()
        {
            m_Screen.onCompleteShow.RemoveListener(WaitForShow);
            m_AnimationComplete = true;
        }
        void WaitForHide()
        {
            m_Screen.onCompleteHide.RemoveListener(WaitForHide);
            m_AnimationComplete = true;
        }

        public override void Skip() => Run();

#if UNITY_EDITOR
        public override void ShowGUI()
        {
            m_Hide = GUILayout.Toolbar(m_Hide ? 1 : 0, new[] {
                "Show",
                "Hide"
            }) == 1;
            m_TargetScreen.ShowGUI(this, "Screen Name");
            m_Instant = EditorGUILayout.Toggle("Instant?", m_Instant);

            AfterRunningOption();
        }

        public override string SetLabel() => $"UI Screen: {m_TargetScreen}";
#endif
    }
}
#endif
