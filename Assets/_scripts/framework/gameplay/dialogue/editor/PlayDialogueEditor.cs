// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 


using OTBT.Framework.Utils.Editor;
using UnityEditor;

namespace OTBT.Framework.Gameplay
{
    [CustomEditor(typeof(PlayDialogue))]
    public class PlayDialogueEditor : Editor
    {
        PlayDialogue m_PlayDialogue = null;

        public override void OnInspectorGUI()
        {
            if (m_PlayDialogue == null) m_PlayDialogue = target as PlayDialogue;
            EditorUtils.BeginColoredEditor();
            m_PlayDialogue.ShowGUI();

            EditorUtils.DrawVerify(m_PlayDialogue);
            EditorUtils.EndColoredEditor();
        }
    }
}
