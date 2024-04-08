// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

#if OTBT_AC
using AC;

#endif
using OTBT.Framework.Utils.Editor;
using OTBT.Framework.ColorPalettes.Editor;
#if OTBT_LipSync
using RogoDigital.Lipsync;
#endif
using UnityEditor;
using UnityEngine;
using OTBT.Framework.Utils;
using static OTBT.Framework.Networking.TranslationServer;
using OTBT.Framework.Networking;
using System.Collections;
using UnityEngine.Networking;
using OTBT.Framework.Core;
using OTBT.Framework.Localization;
using static OTBT.Framework.Gameplay.DialogueManager;

namespace OTBT.Framework.Gameplay
{
    [CustomEditor(typeof(SpeakingCharacter))]
    public class SpeakingCharacterEditor : Editor
    {
        SpeakingCharacter m_SpeakingCharacter = null;

        public override void OnInspectorGUI()
        {
            if (m_SpeakingCharacter == null) m_SpeakingCharacter = target as SpeakingCharacter;
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("Character: " + m_SpeakingCharacter.speakerName, "https://wiki.beatentrack.games/doc/gameplay-lJh4GxbIz5#h-speaker-setup");

            int speakerID = EditorGUILayout.IntField("Speaker ID", m_SpeakingCharacter.speakerID);
            if (!speakerID.Equals(m_SpeakingCharacter.speakerID))
            {
                m_SpeakingCharacter.SetID(speakerID);
                EditorUtility.SetDirty(m_SpeakingCharacter);
            }

            EditorGUILayout.LabelField("Portrait");
            Sprite speakerPortrait = (Sprite)EditorGUILayout.ObjectField(m_SpeakingCharacter.portrait, typeof(Sprite), false); ;
            if (speakerPortrait != m_SpeakingCharacter.portrait)
            {
                m_SpeakingCharacter.SetPortrait(speakerPortrait);
            }

            if (GUILayout.Button("Update filenames"))
            {
                m_SpeakingCharacter.UpdateFileName();
            }

            EditorUtils.GuiLine();

            string speakerName = EditorGUILayout.TextField("Speaker name", m_SpeakingCharacter.speakerName);
            if (!speakerName.Equals(m_SpeakingCharacter.speakerName))
            {
                m_SpeakingCharacter.SetName(speakerName);
                EditorUtility.SetDirty(m_SpeakingCharacter);
            }

            m_SpeakingCharacter.SetUsePalette(EditorGUILayout.Toggle("Use color palette", m_SpeakingCharacter.usesPalette));
            if (m_SpeakingCharacter.usesPalette)
            {
                m_SpeakingCharacter.SetColorPaletteSlot(ColorByPaletteEditor.DrawColorSelector(m_SpeakingCharacter.selectedPaletteIndex));
                EditorUtility.SetDirty(m_SpeakingCharacter);
            }
            else
            {
                Color speakerColor = EditorGUILayout.ColorField("Speaker color", m_SpeakingCharacter.color);
                if (!speakerColor.Equals(m_SpeakingCharacter.color))
                {
                    m_SpeakingCharacter.SetColor(speakerColor);
                    EditorUtility.SetDirty(m_SpeakingCharacter);
                }
            }

            GUILayout.Label("TTS Voice configs: " + m_SpeakingCharacter.ttsConfigs.Count);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("+"))
            {
                m_SpeakingCharacter.AddTtsConfig();
            }
            if(GUILayout.Button("-"))
            {
                m_SpeakingCharacter.RemoveTtsConfig();
            }
            EditorGUILayout.EndHorizontal();
            
            for(int i = 0; i < m_SpeakingCharacter.ttsConfigs.Count; i++)
            {
                m_SpeakingCharacter.SetTTSConfig(i, TTSConfigGUI(m_SpeakingCharacter.ttsConfigs[i], m_SpeakingCharacter));

                if (m_SpeakingCharacter != null && m_SpeakingCharacter.ttsConfigs[i].voiceID != null && !m_SpeakingCharacter.ttsConfigs[i].voiceID.Equals(""))
                {
                    if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Play test audio", Glyphicons.SpeechBubbleAlert)))
                    {
                        // get random string to say
                        string rand = "";
                        if (LocalizationDatabase.instance.textCollection.textObjects.Count > 0)
                            rand = LocalizationDatabase.instance.textCollection.textObjects[UnityEngine.Random.Range(0, LocalizationDatabase.instance.textCollection.textObjects.Count)].originalString;

                        if (rand.Length == 0) rand = "Dies ist ein Test.";

                        ReferenceManager.instance.StartCoroutine(StreamAudioFromUrl(TranslationServer.GetTTSAddress(rand, m_SpeakingCharacter.ttsConfigs[i])));
                        //TranslationServer.PlayTTS("Dies ist ein Test", m_SpeakingCharacter.ttsConfig);
                    }
                }
            }

            EditorUtils.DrawVerify(m_SpeakingCharacter);
            EditorUtils.EndColoredEditor();
        }

        public IEnumerator StreamAudioFromUrl(string url)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error + " // URL: " + url);
                }
                else
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                    EditorUtils.PlayClip(audioClip);
                    Debug.Log("Playing audio");
                }
            }
        }

        public TTSConfigData TTSConfigGUI(TTSConfigData target, SpeakingCharacter smTarget)
        {
            TTSConfigData config = (TTSConfigData)target;

            EditorUtils.Space();
            EditorUtils.GuiLine();

            EditorGUILayout.LabelField("Selected voice: " + target.voiceID + " (" + target.gender + ")");

            config.optionNumber = (VoiceOption)EditorGUILayout.EnumPopup("OptionNum", config.optionNumber);
            EditorUtils.Space();
            config.service = (TTSService)EditorGUILayout.EnumPopup("Service", config.service);
            config.language = (TTSLocale)EditorGUILayout.EnumPopup("Language", config.language);

            if (GUILayout.Button("Load Voice IDs and Genders"))
            {
                // Assume that there's a function to load available voice IDs and genders from the server.
                target.LoadVoiceIDsAndGenders();
            }

            target.DrawVoiceIDEditor(smTarget);

            return config;
        }
    }
}
   