//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//


using OTBT.Framework.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.UI
{
    [CustomEditor(typeof(UIScreenController))]
    public class UIScreenControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("UI Screen Controller", "https://wiki.beatentrack.games/doc/ui-58hkPeqDvy#h-defining-and-switching-between-ui-screens");

            UIScreenController screenController = (UIScreenController)target;

            switch(screenController.screenFlowMode)
            {
                case UIScreenController.ScreenflowMode.Stack:
                    EditorGUILayout.HelpBox("In a stack, closing a screen will trigger the previous screen to be opened again. This is only true for the mutually exclusive screens", MessageType.Info);
                    break;
                case UIScreenController.ScreenflowMode.Single:
                    EditorGUILayout.HelpBox("In single mode screens are simply hidden and shown as prompted, with no additional logic", MessageType.Info);
                    break;
            }

            base.OnInspectorGUI();

            EditorUtils.Space();

            if(Application.isPlaying)
                screenController.strategy.PrintDebugStateGUI();

            EditorUtils.DrawVerify(screenController);
            EditorUtils.EndColoredEditor();
        }
    }
}
