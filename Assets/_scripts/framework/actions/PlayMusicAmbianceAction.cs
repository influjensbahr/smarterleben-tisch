//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

using UnityEngine;
using OTBT.Framework.Audio;
using Sparrow.Verification;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC
{

    [System.Serializable]
    public class PlayMusicAmbianceAction : Action, IVerify
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Audio/Control Music or Ambiance";
        public override string Description => "Control Music or Ambiance";
        
        [SerializeField] int m_MusicOrAmbiance = 0;
        [SerializeField] bool m_StopCurrentMusic = false;
        [SerializeField] MusicTheme m_MusicTheme = null;
        [SerializeField] int m_MusicState = 0;


        public override float Run()
        {
            var player = m_MusicOrAmbiance == 0 ? AudioPlayer.instance.music : AudioPlayer.instance.ambiance;
            if (m_StopCurrentMusic)
            {
                player.Stop();
                return 0f;
            }
            if(m_MusicTheme != null)
                player.Play(m_MusicTheme, m_MusicState);

            return 0f;
        }

        public override void Skip()
        {
            Run();
        }


#if UNITY_EDITOR

        public override void ShowGUI()
        {
            m_MusicOrAmbiance = GUILayout.Toolbar(m_MusicOrAmbiance, new string[] {
                "Music",
                "Ambiance"
            });

            if (m_MusicTheme == null)
                m_StopCurrentMusic = EditorGUILayout.Toggle("Stop Current Music", m_StopCurrentMusic);

            if (!m_StopCurrentMusic)
            {
                EditorGUILayout.HelpBox("You can also use this action just to switch music states, if the theme is already playing", MessageType.Info);
                m_MusicTheme = (MusicTheme)EditorGUILayout.ObjectField("Music Theme", m_MusicTheme, typeof(MusicTheme), false);
                if (m_MusicTheme != null)
                {
                    m_MusicState = EditorGUILayout.Popup("Music State", m_MusicState, m_MusicTheme.GetStateLabels());
                }
            }

            AfterRunningOption();
        }

        public override string SetLabel()
        {
            return "Control Music/Ambiance";
        }

        
#endif
        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR

            checker.Check(!(!m_StopCurrentMusic && m_MusicTheme == null), "Control music action that does nothing", parentActionListInEditor? parentActionListInEditor.gameObject : null);
#endif
        }
    }
}
#endif
