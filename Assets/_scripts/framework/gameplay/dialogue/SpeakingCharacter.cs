// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.ColorPalettes;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Sparrow.Verification;
using static OTBT.Framework.Networking.TranslationServer;
using System.Collections.Generic;

namespace OTBT.Framework.Gameplay
{

    [CreateAssetMenu(menuName = "OTBT/Dialogue/Speaking Character", fileName = "SpeakingCharacter", order = 0)]
    public class SpeakingCharacter : ScriptableObject, IVerify
    {
        public enum Emotion 
        { 
            NEUTRAL, SAD, HAPPY, FEAR, ANGER, SURPRISE, DISGUST
        }

        [SerializeField] int m_SpeakerID = -1;
        [SerializeField] string m_SpeakerName = "Character";
        [SerializeField] Sprite m_SpeakerPortrait = null;

        [SerializeField] bool m_UseColorPalette = true;
        [SerializeField] Color m_SpeakerColor = Color.black;
        [SerializeField] int m_SelectedPaletteIndex = 0;

        [SerializeField] List<TTSConfigData> m_VoiceConfigs = new List<TTSConfigData>();

        public List<TTSConfigData> ttsConfigs => CreateTTSConfigs();
        public int speakerID => m_SpeakerID;

        private List<TTSConfigData> CreateTTSConfigs()
        {
            if (m_VoiceConfigs == null) m_VoiceConfigs = new List<TTSConfigData>();
            return m_VoiceConfigs;
        }

        public void SetTTSConfig(int i, TTSConfigData config)
        {
            if(m_VoiceConfigs.Count > i)
                m_VoiceConfigs[i] = config;
        }

        public void AddTtsConfig()
        {
            m_VoiceConfigs.Add(new TTSConfigData());
        }

        public void RemoveTtsConfig()
        {
            if (m_VoiceConfigs.Count > 0)
                m_VoiceConfigs.RemoveAt(m_VoiceConfigs.Count - 1);
        }

        public string m_OverrideName = "";
        public Sprite m_OverridePortrait = null;

        public int selectedPaletteIndex => m_SelectedPaletteIndex;
        public Sprite portrait => m_OverridePortrait == null ? m_SpeakerPortrait : m_OverridePortrait;
        public string speakerName => m_OverrideName.Length == 0 ? m_SpeakerName : m_OverrideName;
        public Color color => m_SpeakerColor;
        public bool usesPalette => m_UseColorPalette;

        public void SetUsePalette(bool v)
        {
            m_UseColorPalette = v;
        }
        public void SetColorPaletteSlot(int v)
        {
            if (v == selectedPaletteIndex) return;
            m_SelectedPaletteIndex = v;
            UpdateColorPaletteColor();
        }
        void UpdateColorPaletteColor()
        {
            var manager = ColorPaletteManager.instance;
            if (manager == null) return;
            if (manager.palette == null) return;

            SetColor(m_UseColorPalette ? manager.palette.GetColor(selectedPaletteIndex) : m_SpeakerColor);
        }

        public void SetOverrideName(string nam)
        {
            m_OverrideName = nam;
        }
        public void SetOverridePortrait(Sprite spri)
        {
            m_OverridePortrait = spri;
        }

        public void UpdateFileName()
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(path, speakerName);
#endif
        }
        public void SetID(int n)
        {
            m_SpeakerID = n;
            SetDirty();
        }
        public void SetName(string n)
        {
            m_SpeakerName = n;
            SetDirty();
        }

        public void SetColor(Color n)
        {
            m_SpeakerColor = n;
            SetDirty();
        }

        public void SetPortrait(Sprite n)
        {
            m_SpeakerPortrait = n;
            SetDirty();
        }

        private new void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }


        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            checker.Check(!m_SpeakerName.Equals("Character") && !m_SpeakerName.Equals("") && m_SpeakerPortrait != null,"Charactername or Sprite not given.", this);

            // Find all assets of a specific type
            string[] guids = AssetDatabase.FindAssets("t:SpeakingCharacter"); 

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                SpeakingCharacter obj = AssetDatabase.LoadAssetAtPath<SpeakingCharacter>(assetPath);
                if (obj != null)
                {
                    if(obj != this)
                    {
                        checker.Check(obj.speakerID != this.speakerID, $"Duplicate speaker ID found for speakers {obj.speakerName} and {this.speakerName}", this);
                    }
                }
            }
#endif
        }
    }
}
