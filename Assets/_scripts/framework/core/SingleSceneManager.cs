//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//


using UnityEngine;
using Sparrow.Verification;
using OTBT.Framework.Utils;
using OTBT.Framework.Utils.Editor;
using OTBT.Framework.Gameplay;
using System.Collections.Generic;
using UnityEditor;
using OTBT.Framework.Localization;

#if OTBT_AC
using AC;
#endif

namespace OTBT.Framework.Core
{
    /// <summary>
    /// A single scene can contain this to handle operation when scene is started and ended
    /// </summary>
    public class SingleSceneManager : MonoBehaviour, IVerify, IExtendDefaultEditor
    {
        [SerializeField] Camera m_MainCamera;

#if OTBT_AC
        [SerializeField] GameObject m_GameEngine;
        [SerializeField] ActionList m_OnStartACCutscene;
        public ActionList onStartAC => m_OnStartACCutscene;
        public GameObject gameEngine => m_GameEngine;
#endif

        void Start()
        {
            if (m_MainCamera != null) m_MainCamera.enabled = true;

            SceneLoadManager.instance.onSceneStarted += SceneStarted;
        }


        public void SceneStarted()
        {
            Dbg.Log(this, "Scene started!");
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
#if OTBT_AC
            var acMainCamera = m_MainCamera.GetComponent<AC.MainCamera>();
            acMainCamera.Enable();
            m_OnStartACCutscene?.Interact();
#endif
        }
        public void Verify(CheckVerifyInterface checker)
        {

        }

#if UNITY_EDITOR

        async void GenerateTTS(bool clear = false)
        {
            generating = true;
            EditorUtility.DisplayProgressBar("Generating TTS", "Gathering lines now", 0f);

            List<LocalizedTextObject> locaTexts = new List<LocalizedTextObject>();
            foreach (PlayDialogue d in GameObject.FindObjectsByType<PlayDialogue>(FindObjectsSortMode.None))
            {
                if (d.localizedText != null)
                {
                    d.localizedText.SetSpeakingCharacter(d.speakingCharacter.speakingCharacter);
                    EditorUtility.SetDirty(d);
                    locaTexts.Add(d.localizedText);
                }
            }
#if OTBT_AC

            foreach (ActionList d in GameObject.FindObjectsByType<ActionList>(FindObjectsSortMode.None))
            {
                foreach (Action dsa in d.actions)
                {
                    DialogueSpeechAction ds = dsa as DialogueSpeechAction;
                    if (ds != null)
                        if (ds.localizedText != null)
                        {
                            ds.localizedText.SetSpeakingCharacter(ds.speakingCharacter.speakingCharacter);
                            EditorUtility.SetDirty(ds);
                            locaTexts.Add(ds.localizedText);
                        }

                    DialogueChoiceAction dc = dsa as DialogueChoiceAction;
                    if (dc != null && dc.conversation != null)
                    {
                        foreach (ButtonDialog bd in dc.conversation.options)
                        {
                            if (bd.localizedText != null)
                            {
                                if (bd.localizedText.speakingCharacter == null)
                                    bd.localizedText.SetSpeakingCharacter(DialogueManager.instance.player);
                                locaTexts.Add(bd.localizedText);
                            }
                        }
                    }
                }
            }
#endif

            int numberOfOperations = locaTexts.Count;
            int numberDone = 0;

            foreach (LocalizedTextObject locaObj in locaTexts)
            {
                EditorUtility.DisplayProgressBar("Generating TTS", $"Downloading line {locaObj.textID} now", (float)numberDone / (float)numberOfOperations);
                await locaObj.GenerateTTS(clear);
                numberDone++;
            }
            EditorUtility.ClearProgressBar();
            generating = false;
        }

        bool generating = false;
        public void ExtendDefaultEditor()
        {
            GUI.enabled = !generating;
         /*   if(GUILayout.Button(OTBT.Framework.Utils.Editor.EditorUtils.LabelWithGlyphicon("Generate TTS for this scene: all new (expensive!)", Glyphicons.Cloud)))
            {
                GenerateTTS(true);
            }*/
            if (GUILayout.Button(OTBT.Framework.Utils.Editor.EditorUtils.LabelWithGlyphicon("Generate TTS for this scene: nonexisting only!", Glyphicons.Cloud)))
            {
                GenerateTTS(false);
            }
            GUI.enabled = true;
        }

#endif
    }
}
