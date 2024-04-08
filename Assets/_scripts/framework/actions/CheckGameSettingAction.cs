//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

using UnityEngine;
using OTBT.Framework.Core;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC
{
    [System.Serializable]
    public class CheckGameSettingAction : Action
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Core/Check Game Setting";
        public override string Description => "Branches Depending on a Game Setting";
        public override int NumSockets => m_GameSetting == null ? 1 : m_GameSetting.enumType.GetEnumValues().Length;

        [SerializeField] GameSetting m_GameSetting = null;


        public override int GetNextOutputIndex()
        {
            if (m_GameSetting == null) return 0;
            return m_GameSetting.GetIntValue();
        }

		#if UNITY_EDITOR
        public override void ShowGUI()
        {
            m_GameSetting = (GameSetting)EditorGUILayout.ObjectField("Game Setting", m_GameSetting, typeof(GameSetting), false);
        }

        protected override string GetSocketLabel(int i)
        {
            if (m_GameSetting == null || m_GameSetting.enumType == null) return "";
            var names = m_GameSetting.enumType.GetEnumNames();
            return names[i];
        }
		#endif
    }
}
#endif
