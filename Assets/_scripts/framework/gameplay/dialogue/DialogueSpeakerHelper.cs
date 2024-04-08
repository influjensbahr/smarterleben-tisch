//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC
using AC;
using OTBT.Framework.Localization;
using OTBT.Framework.Utils;
using OTBT.Framework.Utils.Editor;
using Sparrow.Verification;
using RenderHeads.Media.AVProVideo;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Gameplay
{
    public class DialogueSpeakerHelper : MonoBehaviour, IExtendDefaultEditor, IVerify
    {
#pragma warning disable CS0414
        [SerializeField] List<SpeakingCharacterTrigger> m_CharacterList = new();
        [SerializeField] ActionList m_ActionList = null;
        [SerializeField] bool m_AutoSetToCharacter = true;
#pragma warning restore CS0414

#if UNITY_EDITOR
        public bool autoSet => m_AutoSetToCharacter;

        void OnValidate()
        {
            if (m_ActionList == null) m_ActionList = GetComponent<ActionList>();
        }
        public List<SpeakingCharacterTrigger> characterList => m_CharacterList;
        public string GetTranscript()
        {
            return DialogueToTranscript(m_ActionList.actions);
        }
  
        public static string DialogueToTranscript(List<Action> actionList)
        {
            string transcript = ""; // Start with an empty transcript
            HashSet<Action> visited = new HashSet<Action>(); // To avoid loops

            DFS(actionList[0], ref transcript, visited); // Start DFS from the first action

            return transcript;
        }

        static void DFS(Action currentAction, ref string currentTranscript, HashSet<Action> visited)
        {
            if (visited.Contains(currentAction))
            {
                return;
            }
            visited.Add(currentAction);

            if (currentAction is DialogueSpeechAction)
            {
                DialogueSpeechAction speechAction = currentAction as DialogueSpeechAction;
                currentTranscript += speechAction.ToTranscript() + "\n";
                foreach (ActionEnd actionEnd in speechAction.endings)
                {
                    DFS(actionEnd.skipActionActual, ref currentTranscript, visited);
                }
            }
            else if (currentAction is DialogueChoiceAction)
            {
                DialogueChoiceAction choiceAction = currentAction as DialogueChoiceAction;
                for (int i = 0; i < choiceAction.endings.Count; i++)
                {
                    string optionTranscript = currentTranscript + "Choice " + (i + 1).ToString() + ": " + choiceAction.ToTranscript(i) + "\n";
                    DFS(choiceAction.endings[i].skipActionActual, ref optionTranscript, visited);
                }
            } else if(currentAction != null)
            {
                foreach (ActionEnd actionEnd in currentAction.endings)
                {
                    DFS(actionEnd.skipActionActual, ref currentTranscript, visited);
                }
            }
        }

        public class DialogueTranscript
        {
            private HashSet<Action> visitedNodes = new HashSet<Action>();
            private StringBuilder transcript = new StringBuilder();

            private Dictionary<Action, int> m_LineNumbers = new Dictionary<Action, int>();

            public static string PrintTranscript(Action node, ActionList actionList, string indent = "", int choiceNumber = 0)
            {
                return new DialogueTranscript().PrintTranscriptInternal(node, actionList, indent, choiceNumber);
            }
           
            private int GetLineNumber(Action a)
            {
                if (m_LineNumbers.ContainsKey(a)) return m_LineNumbers[a];
                int newLine = m_LineNumbers.Count + 1;
                m_LineNumbers.Add(a, newLine);
                return newLine;
            }

            public string PrintTranscriptInternal(Action node, ActionList actionList, string indent = "", int choiceNumber = 0)
            {
                // If we've already visited this node, don't print it again
                if (visitedNodes.Contains(node) || node == null)
                {
                    return transcript.ToString();
                }

                // First print all choices and lines
                if (node is ITranscriptAction && !(node is DialogueChoiceAction))
                {
                    ITranscriptAction speechAction = node as ITranscriptAction;
                    Action nextAction = speechAction.FollowAction();
                    transcript.AppendLine("#" + GetLineNumber(node) + ": " + speechAction.ToTranscript() + (nextAction == null ? " (END)" : (nextAction is DialogueChoiceAction ? (" (-> choice #" + GetLineNumber(nextAction) + ")") : "(-> line #" + GetLineNumber(nextAction) + ")")));
                } else if (node is DialogueChoiceAction)
                {
                    for (int i = 0; i < node.endings.Count; i++)
                    {
                        DialogueChoiceAction choiceAction = node as DialogueChoiceAction;
                        Action nextAction = choiceAction.FollowAction(i);
                        transcript.AppendLine("    Choice #" + GetLineNumber(choiceAction) + ", " + "Option " + (i + 1).ToString() + ": " + choiceAction.ToTranscript(i) + (nextAction == null ? " (END)" : (nextAction is DialogueChoiceAction ? (" (-> choice #" + GetLineNumber(nextAction) + ")") : "(-> line #" + GetLineNumber(nextAction) + ")")));
                    }
                }

                // Mark this node as visited
                visitedNodes.Add(node);

                // Go through each connection
                for (int i = 0; i < node.endings.Count; i++)
                {
                    var connection = node.endings[i];

                    // Calculate the choice number for the next level (if there is more than one choice)
                    int nextChoiceNumber = node.endings.Count > 1 ? i + 1 : 0;

                    Action followupAction = null;
                    switch(connection.resultAction)
                    {
                        case ResultAction.Continue:
                            if (actionList.actions.Count > (actionList.actions.IndexOf(node) + 1)) {
                                followupAction = actionList.actions[actionList.actions.IndexOf(node) + 1];
                            }
                            break;
                        case ResultAction.Skip:
                            followupAction = connection.skipActionActual;
                            if(followupAction == null)
                            {
                                followupAction = actionList.actions[connection.skipAction];
                            }
                            break;
                    }

                    // Recursive call for the connected node
                    if (followupAction != null && !visitedNodes.Contains(followupAction))
                        PrintTranscriptInternal(followupAction, actionList, indent + "  ", nextChoiceNumber);
                }

                return transcript.ToString();
            }
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgef�hrt.
        public async void ExtendDefaultEditor()
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgef�hrt.
        {
            if(GUILayout.Button("Show Transcript"))
            {
                m_ActionList = m_ActionList == null ? GetComponentInChildren<ActionList>() : m_ActionList;
                if (m_ActionList == null)
                {
                    Dbg.Error(this, "Please assign an action list to this editor first!");
                }
                else
                {
                    Dbg.Log(this, DialogueTranscript.PrintTranscript(m_ActionList.actions[0], m_ActionList));
                }
            }

            /*
            if(GUILayout.Button("TEST DO NOT PUSH ME"))
            {
                Action lastAction = m_ActionList.actions.Last();
                // Create 10 ActionWait actions
                for (int i = 0; i < 10; i++)
                {
                    DialogueSpeechAction actionWait = (DialogueSpeechAction)Action.CreateNew<DialogueSpeechAction>();
                    lastAction.endings.Add(new ActionEnd()
                    {
                        skipActionActual = actionWait
                    }); ;
                    m_ActionList.actions.Add(actionWait);
                    lastAction = actionWait;
                }
            }
            */
            foreach (ActionList actionList in GetComponentsInChildren<ActionList>())
            {
                if (m_CharacterList.Count == 0)
                {
                    if (GUILayout.Button(OTBT.Framework.Utils.Editor.EditorUtils.LabelWithGlyphicon("Find speakers in scene", Glyphicons.Search)))
                    {
                        foreach (SpeakingCharacterTrigger list in GameObject.FindObjectsByType<SpeakingCharacterTrigger>(FindObjectsSortMode.None))
                        {
                            if (list.gameObject.scene == this.gameObject.scene && list.speakingCharacter != null && list.speakingCharacter.name != "Player")
                                m_CharacterList.Add(list);
                        }
                    }
                }

                int countOptionsWithoutLoca = 0;
                int countSpeechWithoutLoca = 0;
                int countSpeechWithoutSpeaker = 0;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    int countTotal = 0;
                    int countPlayer = 0;
                    foreach (SpeakingCharacterTrigger t in m_CharacterList)
                    {
                        int count = 0;
                        foreach (Action a in actionList.actions)
                        {
                            DialogueSpeechAction dsa = a as DialogueSpeechAction;
                            if (dsa != null && dsa.speakingCharacter == t)
                            {
                                count++;
                                countTotal++;
                            }
                        }
                        if(t != null && t.speakingCharacter != null)
                            EditorGUILayout.LabelField(t.speakingCharacter.name + ": " + count + " lines");
                    }
                    foreach (Action a in actionList.actions)
                    {
                        DialogueSpeechAction dsa = a as DialogueSpeechAction;
                        if (dsa != null && dsa.isPlayer)
                        {
                            countPlayer++;
                        }
                        if(dsa != null && dsa.speakingCharacter == null)
                        {
                            countSpeechWithoutSpeaker++;
                        }
                        if (dsa != null)
                            if (dsa.localizedText == null) countSpeechWithoutLoca++;
                        DialogueChoiceAction dca = a as DialogueChoiceAction;
                        if (dca != null && dca.conversation != null)
                        {
                            countPlayer += dca.conversation.GetCount();
                            countTotal += dca.conversation.GetCount();
                            foreach (ButtonDialog dia in dca.conversation.options)
                                if (dia.localizedText == null) countOptionsWithoutLoca++;
                        }
                    }
                    EditorGUILayout.LabelField("Player: " + countPlayer + " lines");
                    EditorGUILayout.LabelField("Total: " + countTotal + " lines");
                    if (countOptionsWithoutLoca > 0 || countSpeechWithoutLoca > 0) EditorGUILayout.Space();
                    if (countOptionsWithoutLoca > 0) EditorGUILayout.LabelField("Choices w/o loca: " + countOptionsWithoutLoca + " lines");
                    if (countSpeechWithoutLoca > 0) EditorGUILayout.LabelField("Total w/o loca: " + countSpeechWithoutLoca + " lines");

                    if (countSpeechWithoutSpeaker > 0)
                    {
                        GUILayout.Space(15);
                        EditorGUILayout.LabelField("Total w/o speaker assigned at all: " + countSpeechWithoutSpeaker + " lines");
                        Color oldColor = GUI.color;
                        GUILayout.Label(OTBT.Framework.Utils.Editor.EditorUtils.LabelWithGlyphicon("Set all unassigned to known speaker", Glyphicons.DisplayPerson));
                        foreach (SpeakingCharacterTrigger charac in characterList)
                        {
                            if (charac == null) continue;
                            if (charac.speakingCharacter == null) continue;
                            GUI.color = Color.Lerp(oldColor, charac.speakingCharacter.color, 0.7f);
                            if (GUILayout.Button(charac.speakingCharacter.name))
                            {
                                foreach (Action a in actionList.actions)
                                {
                                    DialogueSpeechAction dsa = a as DialogueSpeechAction;
                                    if (dsa != null && dsa.speakingCharacter == null)
                                    {
                                        dsa.SetSpeakingCharacter(charac);
                                    }
                                }
                                EditorUtility.SetDirty(this);
                            }
                        }
                        GUI.color = oldColor;
                    }
                }
                if (countOptionsWithoutLoca > 0 || countSpeechWithoutLoca > 0)
                {
                    if (GUILayout.Button(OTBT.Framework.Utils.Editor.EditorUtils.LabelWithGlyphicon("Auto-create missing loca items", Glyphicons.Plant)))
                    {
                        foreach (Action a in actionList.actions)
                        {
                            DialogueSpeechAction dsa = a as DialogueSpeechAction;
                            if (dsa != null && dsa.localizedText == null)
                            {
                                LocalizedTextObject newObject = LocalizedTextObject.Create(dsa.currentEditorText, dsa.creationNote);
                                dsa.SetLocalizedTextObject(newObject);
                                newObject.Process(() => dsa.UpdateAllVoicedInformation());
                                EditorUtility.SetDirty(dsa);
                                await Task.Delay(500);
                            }
                            DialogueChoiceAction dca = a as DialogueChoiceAction;
                            if (dca != null && dca.conversation != null)
                            {
                                foreach (ButtonDialog dia in dca.conversation.options)
                                {
                                    if (dia.localizedText == null)
                                    {
                                        LocalizedTextObject newObject = LocalizedTextObject.Create(dia.currentEditorText, dia.creationNote);
                                        dia.SetLocalizedTextObject(newObject);
                                        newObject.Process(() => dia.UpdateAllVoicedInformation());
                                        await Task.Delay(500);
                                    }
                                }
                                EditorUtility.SetDirty(dca);
                                EditorUtility.SetDirty(dca.conversation);
                            }
                        }
                        EditorUtility.SetDirty(actionList);
                        EditorUtility.SetDirty(gameObject);
                    }
                }
            }
        }
#endif

        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}

#endif