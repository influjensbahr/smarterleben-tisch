//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC

using OTBT.Framework.UI;
#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace AC { 
    public class FadeToBlackAction : Action
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "UI/Fade to black";
        public override string Description => "Fades to black using OTBT systems";

        // Declare variables here
        [SerializeField]
        bool m_FadeToBlack = true;

        [SerializeField]
        bool m_Instant = false;


        override public float Run()
        {
            if (!isRunning)
            {
                isRunning = true;
                if (m_FadeToBlack)
                {
                    return AlwaysOnCanvas.instance.StartFadeToBlack(() =>
                    {
                        isRunning = false;
                    }, instant: m_Instant);
                }
                else
                {
                    return AlwaysOnCanvas.instance.StartFadeToVisible(() =>
                    {
                        isRunning = false;
                    }, instant: m_Instant);
                }
            }

            if (isRunning)
                return AlwaysOnCanvas.instance.remainingTime;
            return 0f;
        }


#if UNITY_EDITOR
        override public void ShowGUI()
        {
            m_FadeToBlack = GUILayout.Toolbar(m_FadeToBlack ? 1 : 0, new string[] { "To Visible", "To Black" }) == 1;
            m_Instant = EditorGUILayout.Toggle("Instant", m_Instant);
            AfterRunningOption();
        }

        public override string SetLabel()
        {
            return "OTBT Fade to black";
        }
#endif
    }
}

#endif