//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using Sparrow.Verification;
using OTBT.Framework.Localization;
using OTBT.Framework.Utils;
using OTBT.Framework.Networking;
using OTBT.Framework.Gameplay;

namespace AC {
	[System.Serializable]
	public class DialogueChoiceAction : ActionConversation, IVerify, ITranscriptAction
	{
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Dialogue/Choice";
        public override string Description => "Simple display and editor for conversation options.";

        public DialogueChoiceAction() {
			numSockets = 0;
			overrideOptions = true;
		}


#if UNITY_EDITOR

        bool m_StatsLoaded = false;

        public void OnDestroy() {
			if (conversation != null && Application.isEditor && EditorUtility.DisplayDialog("Auch das verlinkte Conversation-Objekt löschen?",
					"Auch das verlinkte Conversation-Objekt löschen? " + conversation.gameObject.name, "Löschen", "Behalten")) {
				DestroyImmediate(conversation.gameObject);
			}
		}

        public override void ShowGUI(List<ActionParameter> parameters) {
			if (conversation == null && GUILayout.Button("Create conversation object")) {
				GameObject go = new GameObject(parentActionList.gameObject.name + " - Conversation Options " + parentActionList.gameObject.transform.childCount);
				go.transform.parent = parentActionList.gameObject.transform;
				conversation = go.AddComponent<Conversation>();
				conversation.autoPlay = true;
            }
            overrideOptions = true;

            parameterID = Action.ChooseParameterGUI("Conversation:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0) {
				constantID = 0;
				conversation = null;
			} else {
				conversation = (Conversation)EditorGUILayout.ObjectField("Conversation:", conversation, typeof(Conversation), true);

				constantID = FieldToID<Conversation>(conversation, constantID);
				conversation = IDToField<Conversation>(conversation, constantID, false);
			}

			if (conversation) {
				GUILayout.Space(10f);

				// render every option
				for (int i = 0; i < conversation.options.Count; i++) {
					if (conversation.options[i].myGameObject == null)
						conversation.options[i].myGameObject = parentActionListInEditor == null ? null : parentActionListInEditor.gameObject;

                    EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Option #" + i);
					string tmp = conversation.options[i].label;

                    OTBT.Framework.Utils.Editor.EditorUtils.DrawLabelWithGlyphicon(tmp.Length > 120 ? "Long" : (tmp.Length < 60 ? "Short" : "Normal"), tmp.Length > 120 ? Glyphicons.ChevronUp : (tmp.Length < 60 ? Glyphicons.ChevronDown : Glyphicons.Check));

                    if (GUILayout.Button("-")) {
						conversation.options.RemoveAt(i);
						EditorUtility.SetDirty(conversation);
						break;
					}
					
					EditorGUILayout.EndHorizontal();
                   
                    GUIStyle wrapped = new GUIStyle(EditorStyles.textArea);
                    wrapped.wordWrap = true;
                    wrapped.stretchHeight = true;

                    tmp = GUILayout.TextArea(conversation.options[i].label, wrapped, GUILayout.MinHeight(60), GUILayout.ExpandHeight(true));
                    if (tmp != conversation.options[i].label) {
						conversation.options[i].label = tmp;
						EditorUtility.SetDirty(conversation);
					}
                    if (conversation.options[i].selectCount >= 0 && m_StatsLoaded)
                    {
                        OTBT.Framework.Utils.Editor.EditorUtils.DrawLabelWithGlyphicon("Option was picked " + conversation.options[i].selectCount + " times", Glyphicons.StatsBars);
                    }
                    bool playerSpeech = EditorGUILayout.Toggle("Make player say this", conversation.options[i].makePlayerSayThis);
					if (playerSpeech != conversation.options[i].makePlayerSayThis)
                    {
                        conversation.options[i].makePlayerSayThis = playerSpeech;
                        EditorUtility.SetDirty(conversation);
                    }
					if(playerSpeech == true)
					{
						conversation.options[i].foldout = LocalizationEditorHelpers.LocalizedTextLinkEditor(conversation.options[i], compact: true, foldout: conversation.options[i].foldout);
                    }
                    EditorGUILayout.Space(10f);
				}
				if (GUILayout.Button("+ Add option")) {
					conversation.options.Add(new ButtonDialog(conversation.options.Count, "New option", true));
					EditorUtility.SetDirty(conversation);
				}
				GUILayout.Space(5f);

                if (GUILayout.Button("Load stats from server"))
                {
                    _ = TranslationServer.LoadDialogueStats(conversation.guid, (stats) =>
					{
						if (conversation.options.Count > 0) conversation.options[0].selectCount = stats.choice0;
                        if (conversation.options.Count > 1) conversation.options[1].selectCount = stats.choice1;
                        if (conversation.options.Count > 2) conversation.options[2].selectCount = stats.choice2;
                        if (conversation.options.Count > 3) conversation.options[3].selectCount = stats.choice3;
                        if (conversation.options.Count > 4) conversation.options[4].selectCount = stats.choice4;
                        if (conversation.options.Count > 5) conversation.options[5].selectCount = stats.choice5;
                        if (conversation.options.Count > 6) conversation.options[6].selectCount = stats.choice6;
                        if (conversation.options.Count > 7) conversation.options[7].selectCount = stats.choice7;
                        if (conversation.options.Count > 8) conversation.options[8].selectCount = stats.choice8;
                        if (conversation.options.Count > 9) conversation.options[9].selectCount = stats.choice9;
						m_StatsLoaded = true;
                    });
                }

                GUILayout.Space(10f);
				numSockets = conversation.options.Count;

                OTBT.Framework.Utils.Editor.EditorUtils.DrawVerify(this);
            }
		}


        public string ToTranscript(int i = 0)
        {
			return conversation.options[i].currentEditorText + (conversation.options[i].localizedText == null ? "" : " (" + conversation.options[i].localizedText.textID + ")");
        }

        public Action FollowAction(int i = 0)
        {
            Action next = endings[i].skipActionActual;
            while (!(next is ITranscriptAction) && next != null)
            {
                if (next == null ||next.endings == null || next.endings.Count == 0)
                {
                    next = null;
                    break;
                }
                next = next.endings[0].skipActionActual;
            }
            return next;
        }
#endif

        public void Verify(CheckVerifyInterface checker)
        {
            checker.CheckNotNull(conversation, "Conversation object", this);
            if (conversation != null)
                conversation.Verify(checker);
        }
    }
}
#endif
