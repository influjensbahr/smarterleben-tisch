//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

using UnityEngine;
using OTBT.Framework.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
    [System.Serializable]
    public class UIHideAllScreensAction : Action
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "UI/Hide all UI Screens";
        public override string Description => "Hides all UI screens";

        [SerializeField] bool m_Instant = true;

    
        public override float Run()
        {
            UIScreenController.instance.HideAll(m_Instant);

            return defaultPauseTime;
        }

        public override void Skip() => Run();

#if UNITY_EDITOR
        public override void ShowGUI()
        {
           
            m_Instant = EditorGUILayout.Toggle("Instant?", m_Instant);

            AfterRunningOption();
        }
#endif
    }
}
#endif
