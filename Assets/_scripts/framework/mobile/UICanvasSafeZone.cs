//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.ColorPalettes;
using OTBT.Framework.Utils.Editor;
using Sparrow.Verification;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    /// <summary>
    /// Sizes down a UI canvas to match the safe zone of the current device. 
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class UICanvasSafeZone : MonoBehaviour, IVerify
    {
        [SerializeField]
        bool m_ControlWidth = true, m_ControlHeight = true;

        Canvas m_Canvas;
        RectTransform m_SafeAreaTransform;
        Rect m_LastSafeArea = Rect.zero;
        Vector2 m_InitialAnchorsMin, m_InitialAnchorsMax;

        void Awake()
        {
            m_Canvas = GetComponent<Canvas>();
            m_SafeAreaTransform = GetComponent<RectTransform>();
            m_LastSafeArea = Screen.safeArea;
            m_InitialAnchorsMin = m_SafeAreaTransform.anchorMin;
            m_InitialAnchorsMax = m_SafeAreaTransform.anchorMax;

            ApplySafeArea();
        }

        void Update()
        {
            if (Screen.safeArea == m_LastSafeArea) return;

            m_LastSafeArea = Screen.safeArea;
            ApplySafeArea();
        }

        void ApplySafeArea()
        {
            if (m_SafeAreaTransform == null)
                return;

            var safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            var pixelRect = m_Canvas.pixelRect;
            anchorMin.x /= pixelRect.width;
            anchorMax.x /= pixelRect.width;
            anchorMin.y /= pixelRect.height;
            anchorMax.y /= pixelRect.height;

            if (!m_ControlWidth)
            {
                anchorMin.x = m_InitialAnchorsMin.x;
                anchorMax.x = m_InitialAnchorsMax.x;
            }
            if (!m_ControlHeight)
            {
                anchorMin.y = m_InitialAnchorsMin.y;
                anchorMax.y = m_InitialAnchorsMax.y;
            }

            m_SafeAreaTransform.anchorMin = anchorMin;
            m_SafeAreaTransform.anchorMax = anchorMax;
        }

        public void Verify(CheckVerifyInterface checker)
        {
            foreach (UICanvasSafeZone child in GetComponentsInChildren<UICanvasSafeZone>()) {
                checker.Check(child == this, "UICanvasSafeZone inside another UICanvasSafeZone - this should never happen!", child.gameObject);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UICanvasSafeZone), true)]
    public class UICanvasSafeZoneEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            OTBT.Framework.Utils.Editor.EditorUtils.BeginColoredEditor();
            OTBT.Framework.Utils.Editor.EditorUtils.DrawLogoHeader("Canvas Safe Zone", "https://wiki.beatentrack.games/doc/ui-58hkPeqDvy#h-canvas-safe-zones");

            var safeZone = target as UICanvasSafeZone;
            if (safeZone.gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                EditorGUILayout.HelpBox("You probably want to add a Graphic Raycaster to this object to make things interactive!", MessageType.Info);
                if (GUILayout.Button("You're Right, Please Do!")) safeZone.gameObject.AddComponent<GraphicRaycaster>();
            }

            base.OnInspectorGUI();
            OTBT.Framework.Utils.Editor.EditorUtils.DrawVerify(safeZone);
            OTBT.Framework.Utils.Editor.EditorUtils.EndColoredEditor();
        }
    }
#endif
}
