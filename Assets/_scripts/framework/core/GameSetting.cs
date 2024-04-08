//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using Sparrow.Verification;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Core
{
    // these are all the enums a GameSetting can take form of
    public enum ZeroToTen { ZERO, ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, TEN }
    public enum Languages { ENGLISH, GERMAN }
    public enum ControlPreset { REGULAR, ONE_HANDED }
    public enum AudioChannels { STEREO, MONO, NONE, QUAD, SURROUND, FIVEPOINTONE, SEVENPOINTONE, PROLOGIC }
    public enum OnOff { OFF, ON }
    public enum SubtitleFont { REGULAR, DYSLEXIC, CLEAR }
    public enum SubtitleBackground { NONE, TINT, BLACK }
    public enum ControlsDisplay { REGULAR, ALL, NONE }
    public enum SubtitleSpeakerColor { NONE, NAME_ONLY, ALL }


    /// <summary>
    /// A single game setting for our games. Note that all settings are an integer at their core, but use
    /// an enum for outer representation.
    /// </summary>
    [CreateAssetMenu(menuName = "OTBT/Core/Game Setting", fileName = "New Game Setting")]
    public class GameSetting : ScriptableObject, ISaveData, IVerify
    {
        public static Type[] EnumTypes = {
            typeof(ZeroToTen),
            typeof(Languages),
            typeof(ControlPreset),
            typeof(AudioChannels),
            typeof(OnOff),
            typeof(SubtitleFont),
            typeof(SubtitleBackground),
            typeof(ControlsDisplay),
            typeof(SubtitleSpeakerColor)
        };

        [SerializeField] string m_Identifier = Guid.NewGuid().ToString();
        [SerializeField, HideInInspector] int m_EnumTypeIndex = 0;
        [SerializeField, HideInInspector] int m_DefaultValue = 0;
        [Tooltip("If true, this option will start again at 0 when top is reached")]
        [SerializeField] bool m_DoesModulo = true;
        [SerializeField] UnityAction<int> onValueChange;

        public Type enumType => EnumTypes[m_EnumTypeIndex];

        int m_Value;
        bool m_Loaded = false;
        DateTime m_LastChangeTimeStamp;

        public void ClearEvents()
        {
            onValueChange = null;
        }

        public void AddValueChangeAndExecute(UnityAction<int> action)
        {
            if (!m_Loaded) Load();
            onValueChange += action;
            action?.Invoke(m_Value);
        }

        public void SetIntValue(int v)
        {
            m_Value = v;
            m_LastChangeTimeStamp = DateTime.Now;
            Save();
            onValueChange?.Invoke(m_Value);
        }
        public int GetIntValue() { return m_Value; }

        public void Load()
        {
            m_Value = SaveGame.instance.metaSaveData.GetInt(m_Identifier, m_DefaultValue);
            m_Loaded = true;
            onValueChange?.Invoke(m_Value);
        }
        public void Save()
        {
            SaveGame.instance.metaSaveData.SetInt(m_Identifier, m_Value);
        }
        public void ResetToDefault()
        {
            m_Value = m_DefaultValue;
        }

        public void Increase()
        {
            TimeSpan time = DateTime.Now - m_LastChangeTimeStamp;
            if (time.Milliseconds > 300)
            {
                if (m_DoesModulo)
                {
                    SetIntValue((m_Value + 1) % Enum.GetValues(enumType).Length);
                } else
                {
                    SetIntValue(Mathf.Clamp(m_Value + 1, 0, Enum.GetValues(enumType).Length - 1));
                }
            }
        }

        public void Decrease()
        {
            TimeSpan time = DateTime.Now - m_LastChangeTimeStamp;
            if (time.Milliseconds > 300)
            {
                if (m_DoesModulo)
                {
                    SetIntValue((m_Value - 1 + Enum.GetValues(enumType).Length) % Enum.GetValues(enumType).Length);
                } else
                {
                    SetIntValue(Mathf.Clamp(m_Value - 1, 0, Enum.GetValues(enumType).Length - 1));
                }
            }
        }

        public void PreSaveAction() => Save();
        public void WhenLoadReady() => Load();


        public void Verify(CheckVerifyInterface checker)
        {
            
        }
    }
}
