// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Localization;
#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
#endif
using Sparrow.Verification;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.GraphicsBuffer;

namespace OTBT.Framework.Gameplay
{
    public class PlayDialogue : MonoBehaviour, ILocalizedTextLink, IVerify
    {
        [SerializeField] bool m_IsPlayer = false;
        [SerializeField] SpeakingCharacterTrigger m_SpeakingCharacter = null;
        [SerializeField] SpeakingCharacter.Emotion m_CurrentEmotion = SpeakingCharacter.Emotion.NEUTRAL;
        [SerializeField] string m_LineText = "";
        [SerializeField, FormerlySerializedAs("m_GatheredTextObject")] LocalizedTextObject m_LocalizedTextObject = null;

        public SpeakingCharacterTrigger speakingCharacter => m_SpeakingCharacter;

        public LocalizedTextObject localizedText => m_LocalizedTextObject;
        public bool setAtRuntime => false;

        public bool hasTextGatherObject => m_LocalizedTextObject != null;
        public void SetAtRuntime(bool setAtRuntime) { }
        public void AttemptLocaSystemUpdate()
        {
            if (!hasTextGatherObject && localizedText != null)
            {
                m_LocalizedTextObject = LocalizationDatabase.instance.GetByID(localizedText.textID);
#if UNITY_EDITOR
                UpdateAllVoicedInformation();
#endif
            }
        }
        public string currentEditorText => m_LineText;
        public void SetEditorText(string text)
        {
            m_LineText = text;
        }
        public void SetLocalizedTextObject(LocalizedTextObject obj)
        {
            UpdateLocalizedTextObject(obj);
#if UNITY_EDITOR
            UpdateAllVoicedInformation();
#endif
        }
        public string creationNote => "speech " + m_CurrentEmotion + " " + gameObject.scene.name + " " + gameObject.name;

        public void Play()
        {
            SpeakingCharacterTrigger speaker = m_IsPlayer ? DialogueManager.instance.playerTrigger : m_SpeakingCharacter;
            DialogueManager.instance.PlayDialogue(speaker, m_LineText, m_IsPlayer, m_LocalizedTextObject, m_CurrentEmotion);
        }


#if UNITY_EDITOR
        public void OnDestroy()
        {
            if (m_LocalizedTextObject != null)
            {
                if (EditorUtility.DisplayDialog("Remove loca object",
                                        $"You are removing a dialogue action. Shall we also delete the linked loca object?", "Delete", "Do Not Delete"))
                {
                    var path = AssetDatabase.GetAssetPath(m_LocalizedTextObject); 
                    m_LocalizedTextObject.CheckBeforeDestruction();
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }


        public void ShowGUI()
        {
            m_IsPlayer = GUILayout.Toolbar(m_IsPlayer ? 0 : 1, new string[] { "Player", "NPC" }) == 0 ? true : false;

            if (!m_IsPlayer)
            {
                EditorGUILayout.LabelField("Speaking character");
                SpeakingCharacterTrigger chara = (SpeakingCharacterTrigger)EditorGUILayout.ObjectField(m_SpeakingCharacter, typeof(SpeakingCharacterTrigger), true);
                if(m_SpeakingCharacter != chara)
                {
                    m_SpeakingCharacter = chara;
                    if (m_LocalizedTextObject != null)
                    {
                        m_LocalizedTextObject.SetSpeakingCharacter(chara.speakingCharacter);

                    }
                }
            }

            EditorGUILayout.LabelField("Line text");
            m_LineText = EditorGUILayout.TextArea(m_LineText, GUILayout.Height(80));


            OTBT.Framework.Utils.Editor.EditorUtils.DrawLabelWithGlyphicon(m_LineText.Length > 120 ? "Long" : (m_LineText.Length < 60 ? "Short" : "Normal"), m_LineText.Length > 120 ? "chevron-up.png" : (m_LineText.Length < 60 ? "chevron-down.png" : "check.png"));

            m_CurrentEmotion = (SpeakingCharacter.Emotion)EditorGUILayout.EnumPopup("Line emotion", m_CurrentEmotion);


            OTBT.Framework.Utils.Editor.EditorUtils.GuiLine();



            LocalizationEditorHelpers.LocalizedTextLinkEditor(this);
        }

#endif

        public void UpdateAllVoicedInformation()
        {
#if UNITY_EDITOR
            if (m_LocalizedTextObject != null)
            {
                m_LocalizedTextObject.SetAutomaticNote(creationNote);
                m_LocalizedTextObject.SetSpeakingCharacter(m_SpeakingCharacter.speakingCharacter);
                m_LocalizedTextObject.SetManualNote(m_LocalizedTextObject.manualNote);
                m_LocalizedTextObject.SetPreviousObject(null);
                m_LocalizedTextObject.SetIsVoiced(true);
            }
#endif
        }

        public void UpdateLocalizedTextObject(LocalizedTextObject locaTextObject)
        {
            m_LocalizedTextObject = locaTextObject;
            if (locaTextObject != null)
            {

                (locaTextObject).SetOriginalString(m_LineText);
                if (m_IsPlayer)
                {
                    (locaTextObject).SetBaseIdentifier("PlayerLine");
                }
                else
                {
                    if (m_SpeakingCharacter != null)
                        (locaTextObject).SetBaseIdentifier(m_SpeakingCharacter.name);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(LocalizationDatabase.instance);
            AssetDatabase.SaveAssets();
#endif
        }

        public void Verify(CheckVerifyInterface checker)
        {
            if (!m_IsPlayer)
                checker.Check(m_SpeakingCharacter != null, "Dialogue component has no speaker assigned", this);
            checker.Check(!m_LineText.Equals(""), "No line to be said.", gameObject);
        }
    }
}
