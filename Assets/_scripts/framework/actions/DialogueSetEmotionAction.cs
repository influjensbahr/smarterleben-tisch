//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC
using UnityEngine;
using System.Collections.Generic;
using OTBT.Framework.Gameplay;
using OTBT.Framework.Localization;
using UnityEngine.Serialization;
using OTBT.Framework.Utils;
using Sparrow.Verification;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC
{

    [System.Serializable]
    public class DialogueSetEmotionAction : Action, IVerify
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Dialogue/Set Emotion";
        public override string Description => "Triggers a character to enter specified emotion";

        [SerializeField] bool m_IsPlayer = false;
        [SerializeField] SpeakingCharacterTrigger m_SpeakingCharacter = null;
        [SerializeField] SpeakingCharacter.Emotion m_CurrentEmotion = SpeakingCharacter.Emotion.NEUTRAL;

        public override float Run()
        {
            SpeakingCharacterTrigger speaker = m_IsPlayer ? DialogueManager.instance.playerTrigger : m_SpeakingCharacter;
            speaker.SetEmotion(m_CurrentEmotion);
            return 0f;
        }


#if UNITY_EDITOR
        public override void ShowGUI(List<ActionParameter> parameters)
        {
            if (m_SpeakingCharacter != null)
                overrideColor = Color.Lerp(OTBT.Framework.Utils.Editor.EditorUtils.ActionColor, m_SpeakingCharacter.speakingCharacter.color, 0.95f);
            m_IsPlayer = GUILayout.Toolbar(m_IsPlayer ? 0 : 1, new string[] {
                "Player",
                "NPC"
            }) == 0;

            if (!m_IsPlayer)
            {
                EditorGUILayout.LabelField("Speaking character");
                SpeakingCharacterTrigger chara = (SpeakingCharacterTrigger)EditorGUILayout.ObjectField(m_SpeakingCharacter, typeof(SpeakingCharacterTrigger), true);
                if (m_SpeakingCharacter != chara)
                {
                    m_SpeakingCharacter = chara;
                }
            }

            EditorGUILayout.LabelField("Target emotion");
            m_CurrentEmotion = (SpeakingCharacter.Emotion)EditorGUILayout.EnumPopup(m_CurrentEmotion);
        }

        public override string SetLabel()
        {
            return "Set Emotion";
        }

#endif
        public void Verify(CheckVerifyInterface checker)
        {
            if (!m_IsPlayer)
                checker.Check(m_SpeakingCharacter != null, "Dialogue set emotion action has no speaker assigned", this);
        }
    }
}
#endif
