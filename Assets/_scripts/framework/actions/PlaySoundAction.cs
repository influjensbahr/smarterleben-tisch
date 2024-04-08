//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

using UnityEngine;
using OTBT.Framework.Audio;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class PlaySoundAction : Action
	{
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Audio/Play Sound";
        public override string Description => "Plays a single sound";

        
		[SerializeField] SoundCue m_SoundCue;
		[SerializeField] float m_Volume = 1f;
		[SerializeField] float m_Delay = 0f;
		[SerializeField] Transform m_Position = null;
		[SerializeField] bool m_Loop = false;
        [SerializeField] bool m_StartLoop = true;
        [SerializeField] float m_FadeTime = 0f;


		override public float Run(){
			if(!m_Loop)
			{
                AudioPlayer.instance.PlayOneShot(m_SoundCue, m_Volume, m_Delay, m_Position);
            } else
			{
				if(m_StartLoop)
					AudioPlayer.instance.PlayLoop(m_SoundCue, m_Delay, m_FadeTime, m_Volume, m_Position);
				if(!m_StartLoop)
                    AudioPlayer.instance.StopLoop(m_SoundCue, m_Delay, m_FadeTime);
            }
			
			return 0f;
		}

		override public void Skip(){
			Run();
		}


#if UNITY_EDITOR

		override public void ShowGUI () {
            m_Loop = GUILayout.Toolbar(m_Loop ? 1 : 0, new string[] { "One-shot", "Looping" }) == 1;
            m_SoundCue = (SoundCue)EditorGUILayout.ObjectField("Sound cue: ", m_SoundCue, typeof(SoundCue), true);
            m_Volume = EditorGUILayout.Slider("Volume:", m_Volume, 0f, 1f);
            m_Delay = EditorGUILayout.Slider("Delay:", m_Delay, 0f, 5f);
            m_Position = (Transform)EditorGUILayout.ObjectField("Position: ", m_Position, typeof(Transform), true);
			if (m_Loop)
			{
				m_StartLoop = EditorGUILayout.Toggle(m_StartLoop ? "Starting loop" : "Stopping loop", m_StartLoop);
                m_FadeTime = EditorGUILayout.Slider("FadeTime:", m_FadeTime, 0f, 5f);
            }
            AfterRunningOption ();
		}

		public override string SetLabel () {
			return "Play Sound";
		}
#endif
	}
}
#endif