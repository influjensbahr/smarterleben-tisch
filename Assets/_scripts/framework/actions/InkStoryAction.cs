//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer:Alex Brühl
//

#if OTBT_INK && OTBT_AC

using OTBT.Framework.Utils.Editor;
using UnityEditor;
using UnityEngine;
using OTBT.Framework.Ink;

namespace AC
{
    [System.Serializable]
    public class InkStoryAction : Action
    {

        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Dialogue/InkStory";
        public override string Description => "Triggers an ink Story to start";

        [SerializeField] TextAsset m_InkStory = null;

        public InkStoryAction()
        {
            this.isDisplayed = true;
#if UNITY_EDITOR
            overrideColor = EditorUtils.ActionColor;
#endif
        }
        public override float Run()
        {
            if (!isRunning)
            {
                isRunning = true;
                InkpieceController.instance.StartStory(m_InkStory);
                return InkpieceController.instance.currentDialogueTime + 0.1f; //Adding 0.1f to avoid Race-Conditions
            }
            if(isRunning && InkpieceController.instance.currentDialogueTime != 0.0f)
            {
                return InkpieceController.instance.currentDialogueTime + 0.1f; //Adding 0.1f to avoid Race-Conditions
            } 
            else
            {
                isRunning = false;
                return 0f;
            }
        }

#if UNITY_EDITOR
        public override void ShowGUI()
        {
            base.ShowGUI();
            m_InkStory = (TextAsset)EditorGUILayout.ObjectField("Ink Story Json File:", m_InkStory, typeof(TextAsset), false);
        }
#endif
    }
}

#endif