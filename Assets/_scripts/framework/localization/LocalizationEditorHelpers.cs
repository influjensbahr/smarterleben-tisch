//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using OTBT.Framework.Networking;
using StringUtility = OTBT.Framework.Utils.StringUtility;
using OTBT.Framework.Utils;

namespace OTBT.Framework.Localization
{
    public class LocalizationEditorHelpers
    {
        static string[] toolbarOptions = {
            "Set with object",
            "Set at runtime"
        };

        public static bool LocalizedTextLinkEditor(ILocalizedTextLink smTarget, bool offerRuntime = false, bool compact = false, bool foldout = false, bool hideDialogue = false)
        {
            if (offerRuntime)
            {
                bool toolbar = EditorUtils.BoolToolbar(smTarget.setAtRuntime, toolbarOptions);
                if (toolbar != smTarget.setAtRuntime)
                {
                    smTarget.SetAtRuntime(toolbar);
                }

                EditorUtils.Space(compact ? 5 : 10);
            }
            if (smTarget.setAtRuntime && offerRuntime)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUIStyle textStyle = EditorStyles.label;
                    textStyle.wordWrap = true;
                    GUILayout.Label("Translations will need to be assigned at runtime, for example via a database load.", textStyle);
                }
            }
            else
            {
                if (!smTarget.hasTextGatherObject)
                {
                    if (smTarget.localizedText != null && smTarget.localizedText.textID > 0)
                    {
                        EditorGUILayout.HelpBox("Seems like this was set up before the big loca system revamp - it will continue to work, but consider updating it to new functionalities!", MessageType.Warning);
                        if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Attempt update", Glyphicons.Clean)))
                        {
                            smTarget.AttemptLocaSystemUpdate();
                            EditorUtility.SetDirty(smTarget.gameObject);
                            return foldout;
                        }
                        EditorUtils.Space();
                    } 
                    else
                    {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            GUILayout.Label(EditorUtils.LabelWithGlyphicon(" Text is not linked to Localization DB yet.", "cloud-off.png"));
                            GUILayout.Space(5);
                            if(compact) EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(compact ? " Create" : " Create object now", Glyphicons.Clean)))
                            {
                                LocalizedTextObject newObject = LocalizedTextObject.Create(smTarget.currentEditorText, smTarget.creationNote);
                                smTarget.SetLocalizedTextObject(newObject);
                                newObject.Process(() => smTarget.UpdateAllVoicedInformation());
                                if(smTarget.gameObject != null)
                                    EditorUtility.SetDirty(smTarget.gameObject);

                                return foldout;
                            }
                            GUILayout.Space(5);
                            if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(compact ? " Find" : " Try to find object", Glyphicons.Search)))
                            {
                                int bestValue = int.MaxValue;
                                LocalizedTextObject bestFit = null;
                                foreach(LocalizedTextObject to in LocalizationDatabase.instance.textCollection.textObjects)
                                {
                                    if (string.IsNullOrEmpty(to.originalString)) continue;

                                    int score = StringUtility.StringCloseness(to.originalString, smTarget.currentEditorText);
                                    if (score < bestValue)
                                    {
                                        bestValue = score;
                                        bestFit = to;
                                    }
                                }
                                
                                if(EditorUtility.DisplayDialog("Apply found object?",
                                    $"The best match I have found is this (score {bestValue}): \n\nMy string: {smTarget.currentEditorText}\nFound obj: {bestFit.originalString}", "Assign", "Do Not Assign"))
                                {
                                    smTarget.SetLocalizedTextObject(bestFit);
                                    smTarget.localizedText.Process(() => smTarget.UpdateAllVoicedInformation());
                                    EditorUtility.SetDirty(smTarget.gameObject);
                                    return foldout;
                                }
                                    EditorGUIUtility.PingObject(bestFit);
                            }
                            if(compact) GUILayout.EndHorizontal();
                        }
                    }
                }

                if (smTarget.localizedText != null)
                {
                    if (!smTarget.localizedText.originalString.Equals(smTarget.currentEditorText))
                    {
                        EditorUtils.Space();
                        EditorUtils.GuiLine();
                        EditorUtils.Space();

                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            GUILayout.Label(EditorUtils.LabelWithGlyphicon(" Displayed text is not equal to text in DB!", "square-edit.png"));

                            GUILayout.Space(5);
                            if (GUILayout.Button("Update DB to displayed text"))
                            {
                                smTarget.localizedText.SetOriginalString(smTarget.currentEditorText);
                            }
                            GUILayout.Space(5);
                            if (GUILayout.Button("Update display to what's in the DB"))
                            {
                                smTarget.SetEditorText(smTarget.localizedText.originalString);
                            }
                        }
                    }
                }

                if (!compact) EditorUtils.Space();
                if(!compact) EditorUtils.Header("Localized Text Object ID: " + (smTarget.localizedText == null ? "-" : smTarget.localizedText.textID));
                if (compact) foldout = EditorGUILayout.Foldout(foldout, "Localized Text Object ID: " + (smTarget.localizedText == null ? "-" : smTarget.localizedText.textID), true);
                if (!compact || foldout)
                {
                    LocalizedTextObject selection = EditorGUILayout.ObjectField(smTarget.localizedText, typeof(LocalizedTextObject), false) as LocalizedTextObject;
                    if (selection != smTarget.localizedText)
                    {
                        smTarget.SetLocalizedTextObject(selection);
                        EditorUtility.SetDirty(smTarget.gameObject);
                        return foldout;
                    }

                    if (smTarget.localizedText != null)
                    {
                        DrawLocalizedTextEditor(smTarget.localizedText, new SerializedObject(smTarget.localizedText), compact, smTarget, hideDialogue: hideDialogue);
                    }
                }
            }

            if (GUI.changed)
            {
                if(smTarget != null && smTarget.gameObject != null)
                    EditorUtility.SetDirty(smTarget.gameObject);
                if (smTarget != null && smTarget.gameObject != null && smTarget.gameObject.scene != null)
                    EditorSceneManager.MarkSceneDirty(smTarget.gameObject.scene);
            }

            return foldout;
        }



        public static async void DrawLocalizedTextEditor(LocalizedTextObject smTarget, SerializedObject serializedObject, bool compact = false, ILocalizedTextLink textLink = null, bool hideDialogue =false)
        {
            if (smTarget == null || smTarget.textID <= 0)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label(EditorUtils.LabelWithGlyphicon(" Text is not linked to Localization DB yet.", "cloud-off.png"));
                    GUILayout.Space(5);
                    if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Setup now", Glyphicons.Clean)))
                    {
                        if (smTarget != null)
                        {
                            smTarget.Process();
                            if (textLink != null) textLink.UpdateAllVoicedInformation();
                        }
                    }
                    if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Set TextID manually", "construction-cone.png")))
                    {
                        var name = EditorInputDialog.Show("Set TextID manually", "Be careful with this. This will set the internal ID of a localize text object to something that's already existant in the database. Please only insert a number.", "");
                        if (!string.IsNullOrEmpty(name))
                        {
                            if(int.TryParse(name, out int id)) {
                                smTarget.SetTextID(id);
                            }
                        }
                    }
                }
            }
            else
            {
                if (!compact || smTarget.needsProcessing) { 
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        if (smTarget.needsProcessing)
                        {
                            GUILayout.Label(EditorUtils.LabelWithGlyphicon(" Status: updated, should push!", "refresh.png"));
                        }
                        else if (!compact)
                        {
                            GUILayout.Label(EditorUtils.LabelWithGlyphicon(" Status: ok.", "check.png"));
                        }

                        if (!compact)
                            GUILayout.BeginHorizontal();

                        if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Push to Server", Glyphicons.SquareTriangleUp)))
                        {
                            _= smTarget.PushToServer();
                            if (textLink != null) textLink.UpdateAllVoicedInformation();
                            EditorUtils.Save(smTarget);
                        }
                        if (!compact)
                        {
                            if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Pull from Server", Glyphicons.SquareTriangleDown)))
                            {
                                _ = smTarget.LoadFromServer();
                                EditorUtils.Save(smTarget);
                            }
                            GUILayout.EndHorizontal();

                            if (await smTarget.hasMissingLanguages)
                            {
                                EditorUtils.Space(5);
                                if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Add missing languages", Glyphicons.StatsCircle)))
                                {
                                    await smTarget.AddMissingLanguages();
                                    EditorUtils.Save(smTarget);
                                }
                            }
                            GUILayout.Space(5);
                            if (!smTarget.temporaryID)
                            {
                                if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Unlink this object from DB", Glyphicons.LinkRemove)))
                                {
                                    if (EditorUtility.DisplayDialog("Unlink from Database?", "This will unset the textID of this object, removing the connection to the database and allowing you to create a new one.", "Yes", "no"))
                                    {
                                        smTarget.SetDatabaseID(-1);
                                        EditorUtils.Save(smTarget);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if(!compact) GUILayout.Space(10);

            if (!hideDialogue && smTarget != null && smTarget.automaticNote != null && !smTarget.automaticNote.Equals(""))
                EditorGUILayout.LabelField("Source Note: " + smTarget.automaticNote);

            EditorGUI.BeginChangeCheck();

            if (!compact)
            {
                var display = serializedObject.FindProperty("m_OriginalString");
                if(display != null)
                    EditorGUILayout.PropertyField(display);
            }

            if (!hideDialogue)
            {
                EditorGUILayout.LabelField("Manual note");
                string manualNote = EditorGUILayout.TextArea(smTarget.manualNote);
                if (manualNote != smTarget.manualNote)
                    smTarget.SetManualNote(manualNote);

            }
            if (compact && EditorGUI.EndChangeCheck())
            {
                smTarget.UpdatedValue();
            }
            if (!compact)
            {
                GUILayout.Space(5);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Translations"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AllowRuntimeOverride"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ForceSingleRuntimeUpdate"));

                GUILayout.Space(10);

                if (!hideDialogue)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {   
                        if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Try to Play", "play.png")))
                        {
                            EditorUtils.PlayClip(smTarget.GetBestAudio());
                        }
                        GUILayout.Space(5);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SpeakingCharacter"));

                        if (!string.IsNullOrEmpty(smTarget.manualNote))
                            EditorUtils.DrawLabelWithGlyphicon(smTarget.manualNote, "quotation-left.png");
                        if (!string.IsNullOrEmpty(smTarget.automaticNote))
                            EditorUtils.DrawLabelWithGlyphicon(smTarget.automaticNote, "terminal.png");
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Generate TTS", "volume-down.png")))
                        {
                            _ = smTarget.GenerateTTS();
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            } else
            {
                if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Try to Play", "play.png")))
                {
                    EditorUtils.PlayClip(smTarget.GetBestAudio());
                }
                GUILayout.Space(5);
            }

            if (!compact && EditorGUI.EndChangeCheck())
            {
                smTarget.UpdatedValue();
            }

            serializedObject.ApplyModifiedProperties();
        }

        
    }
}

#endif
