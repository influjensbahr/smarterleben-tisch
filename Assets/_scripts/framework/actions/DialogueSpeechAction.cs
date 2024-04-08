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
using System;
#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC
{

    [System.Serializable]
    public class DialogueSpeechAction : Action, ILocalizedTextLink, IVerify, ITranscriptAction
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Dialogue/Speech";
        public override string Description => "Triggers a character to say something";


        [SerializeField] SpeakingCharacterTrigger m_SpeakingCharacter = null;
        [SerializeField] string m_MessageText = "";
        [SerializeField] bool m_IsPlayer = false;
        [SerializeField] bool m_PlayInBackground = false;
        [SerializeField] SpeakingCharacter.Emotion m_CurrentEmotion = SpeakingCharacter.Emotion.NEUTRAL;
        [SerializeField, FormerlySerializedAs("m_GatheredObject")] LocalizedTextObject m_LocalizedTextObject = null;
        public LocalizedTextObject localizedText => m_LocalizedTextObject;

        public SpeakingCharacterTrigger speakingCharacter => m_IsPlayer ? DialogueManager.instance.playerTrigger : m_SpeakingCharacter;

        public bool isPlayer => m_IsPlayer;

        ActiveDialogue m_PlayingDialogue = null;

        public void SetEditorText(string text)
        {
            m_MessageText = text;
        }
        public bool setAtRuntime => false;
        public void SetAtRuntime(bool setAtRuntime) { }
        public bool hasTextGatherObject => m_LocalizedTextObject != null;
        public string currentEditorText => m_MessageText;
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
        public void SetLocalizedTextObject(LocalizedTextObject obj)
        {

            m_LocalizedTextObject = obj;
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Apply current displayed data to the loca object?",
                                        $"Do you want to set the loca object to whatever is currently written in this dialogue node?", "Yes, set it!", "No, leave as-is"))
            {

                UpdateLocalizedTextObject(obj);
            }
            UpdateAllVoicedInformation();
#endif
        }

#if UNITY_EDITOR
        bool m_LocalizationFoldout = false;
        public GameObject gameObject => parentActionListInEditor == null ? null : parentActionListInEditor.gameObject;
        public string creationNote => "speech " + m_CurrentEmotion + " " + (gameObject != null ? ((gameObject.scene != null ? gameObject.scene.name : "") + " " + gameObject.name) : "");
#endif



        public override float Run()
        {
            if (isRunning)
            {
                isRunning = m_PlayingDialogue.isActive;
                return 0.1f;
            }
            if (!isRunning)
            {
                isRunning = true;
                SpeakingCharacterTrigger speaker = m_IsPlayer ? DialogueManager.instance.playerTrigger : m_SpeakingCharacter;
                m_PlayingDialogue = DialogueManager.instance.PlayDialogue(speaker, m_MessageText, m_IsPlayer, localizedText, m_CurrentEmotion);
                return m_PlayInBackground ? 0f : 0.1f;
            }
            return 0f;
        }

        public void SetIsPlayer(bool v)
        {
            m_IsPlayer = v;
        }


      

#if UNITY_EDITOR
        public void OnDeleteAction()
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

        public override void ShowGUI(List<ActionParameter> parameters)
        {
            if (m_SpeakingCharacter != null)
                overrideColor = Color.Lerp(OTBT.Framework.Utils.Editor.EditorUtils.ActionColor, m_SpeakingCharacter.speakingCharacter.color, 0.95f);
            if(m_IsPlayer)
                overrideColor = OTBT.Framework.Utils.Editor.EditorUtils.ActionColor;
            
            m_IsPlayer = GUILayout.Toolbar(m_IsPlayer ? 0 : 1, new string[] {
                "Player",
                "NPC"
            }) == 0;

            if(parentActionListInEditor == null)
            {
                (this as Action).AttemptToFindActionList();
            }

            if (!m_IsPlayer)
            {
                EditorGUILayout.LabelField("Speaking character");
                SpeakingCharacterTrigger chara = (SpeakingCharacterTrigger)EditorGUILayout.ObjectField(m_SpeakingCharacter, typeof(SpeakingCharacterTrigger), true);
                if (chara != null)
                {
                    if (m_SpeakingCharacter != chara)
                    {
                        m_SpeakingCharacter = chara;
                        EditorUtility.SetDirty(this);
                        if (m_LocalizedTextObject != null)
                        {
                            m_LocalizedTextObject.SetSpeakingCharacter(chara.speakingCharacter);
                        }
                    }
                }
                if(m_SpeakingCharacter == null && parentActionListInEditor != null)
                {
                    DialogueSpeakerHelper helper = parentActionListInEditor.gameObject.GetComponent<DialogueSpeakerHelper>();
                    if(helper != null)
                    {
                        Color oldColor = GUI.color;
                        if (helper.characterList.Count == 1 && helper.characterList[0] != null && helper.autoSet)
                        {
                            m_SpeakingCharacter = helper.characterList[0];
                            EditorUtility.SetDirty(this);
                            if (m_LocalizedTextObject != null)
                            {
                                m_LocalizedTextObject.SetSpeakingCharacter(helper.characterList[0].speakingCharacter);
                            }
                        }
                        else
                        {
                            GUILayout.Label(OTBT.Framework.Utils.Editor.EditorUtils.LabelWithGlyphicon("Set to known speaker", Glyphicons.DisplayPerson));
                            foreach (SpeakingCharacterTrigger charac in helper.characterList)
                            {
                                if (charac == null) continue;
                                if (charac.speakingCharacter == null) continue;
                                GUI.color = Color.Lerp(oldColor, charac.speakingCharacter.color, 0.7f);
                                if (GUILayout.Button(charac.speakingCharacter.name))
                                {
                                    m_SpeakingCharacter = charac;
                                    EditorUtility.SetDirty(this);
                                    if (m_LocalizedTextObject != null)
                                    {
                                        m_LocalizedTextObject.SetSpeakingCharacter(charac.speakingCharacter);
                                    }
                                }
                            }
                        }
                        GUI.color = oldColor;
                    }
                    
                }
            }

            EditorGUILayout.LabelField("Line Text");
            GUIStyle wrapped = new GUIStyle(EditorStyles.textArea);
            wrapped.wordWrap = true;
            wrapped.stretchHeight = true;
            m_MessageText = EditorGUILayout.TextArea(m_MessageText, wrapped, GUILayout.MinHeight(80), GUILayout.ExpandHeight(true));


            OTBT.Framework.Utils.Editor.EditorUtils.DrawLabelWithGlyphicon(m_MessageText.Length > 120 ? "Long" : (m_MessageText.Length < 60 ? "Short" : "Normal"), m_MessageText.Length > 120 ? Glyphicons.ChevronUp : (m_MessageText.Length < 60 ? Glyphicons.ChevronDown : Glyphicons.Check));

            m_CurrentEmotion = (SpeakingCharacter.Emotion)EditorGUILayout.EnumPopup("Line emotion", m_CurrentEmotion);


            m_PlayInBackground = EditorGUILayout.Toggle("Play in Background?", m_PlayInBackground);

            OTBT.Framework.Utils.Editor.EditorUtils.Separator(1);

            m_LocalizationFoldout = LocalizationEditorHelpers.LocalizedTextLinkEditor(this, compact: true, foldout: m_LocalizationFoldout);

            OTBT.Framework.Utils.Editor.EditorUtils.DrawVerify(this);
        }

        public void UpdateAllVoicedInformation()
        {
            if (m_LocalizedTextObject != null)
            {
                m_LocalizedTextObject.SetAutomaticNote(creationNote);
                m_LocalizedTextObject.SetManualNote(m_LocalizedTextObject.manualNote);
                if(m_SpeakingCharacter != null)
                    m_LocalizedTextObject.SetSpeakingCharacter(m_SpeakingCharacter.speakingCharacter);
                DialogueSpeechAction prev = ActionUtils.GetPreviousActionOfType<DialogueSpeechAction>(this);
                m_LocalizedTextObject.SetPreviousObject(prev == null ? null : (prev.localizedText));
                m_LocalizedTextObject.SetIsVoiced(true);
            }
        }

        public override string SetLabel()
        {
            return "Dialogue Speech";
        }

        public void UpdateLocalizedTextObject(LocalizedTextObject gather)
        {
            if (gather != null)
            {
                (gather).SetOriginalString(m_MessageText);
                if (m_IsPlayer)
                {
                    (gather).SetBaseIdentifier("PlayerLine");
                }
                else
                {
                    if (m_SpeakingCharacter != null)
                        (gather).SetBaseIdentifier(m_SpeakingCharacter.name);
                }
            }
            EditorUtility.SetDirty(LocalizationDatabase.instance);
            AssetDatabase.SaveAssets();
        }


        internal void SetSpeakingCharacter(SpeakingCharacterTrigger speakingCharacterTrigger)
        {
            m_SpeakingCharacter = speakingCharacterTrigger;
            if (m_LocalizedTextObject != null)
            {
                m_LocalizedTextObject.SetSpeakingCharacter(m_SpeakingCharacter.speakingCharacter);
            }
        }

        public string ToTranscript(int i = 0)
        {
            return (m_IsPlayer ? "Player" : (m_SpeakingCharacter != null ? (m_SpeakingCharacter.speakingCharacter != null ? m_SpeakingCharacter.speakingCharacter.name : m_SpeakingCharacter.name) : "NPC")) + ": " + (m_CurrentEmotion == SpeakingCharacter.Emotion.NEUTRAL ? "" : "(" + m_CurrentEmotion.ToString().ToLower() + ")") +
                m_MessageText + (m_LocalizedTextObject == null ? "" : " (" + m_LocalizedTextObject.textID + ")");
        }

        public Action FollowAction(int i = 0)
        {
            if (endings.Count == 0) return null;
            Action next = endings[0].skipActionActual;
            List<Action> visitedNodes = new();
            while (!(next is ITranscriptAction) && next != null)
            {
                if (visitedNodes.Contains(next)) 
                    return null;
                if (next == null || next.endings == null || next.endings.Count == 0)
                    return null;
                visitedNodes.Add(next);
                next = next.endings[0].skipActionActual;
            }
            return next;
        }
#endif
        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            if (!m_IsPlayer)
                checker.Check(m_SpeakingCharacter != null, "Dialogue action has no speaker assigned", gameObject);
            checker.Check(!m_MessageText.Equals(""), "No line to be said.", gameObject);
#endif
        }
    }
}
#endif
