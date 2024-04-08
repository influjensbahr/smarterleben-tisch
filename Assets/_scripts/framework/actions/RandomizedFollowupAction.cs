
//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

using UnityEngine;
using OTBT.Framework.Utils;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC { 

	[System.Serializable]
	public class RandomizedFollowupAction : Action {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Utils/Randomized Followup";
        public override string Description => "Plays a random followup action, according to the settings provided";
		public override int NumSockets => m_NumberOfOptions;


		[SerializeField] int m_NumberOfOptions = 1;

		RandomizedSequence m_Sequence = null;


		public override int GetNextOutputIndex()
        {
            if (m_Sequence == null)
                m_Sequence = new RandomizedSequence(NumSockets);
            return m_Sequence.GetNextIndex();
        }

        override public void Skip () {
			 Run ();
		}
		
		#if UNITY_EDITOR

		override public void ShowGUI () {

            if (m_Sequence == null)
                m_Sequence = new RandomizedSequence(NumSockets);
            m_Sequence.UpdateCount(NumSockets);
            m_Sequence.ShowGUI(this);

			EditorGUILayout.Space();


            m_NumberOfOptions = EditorGUILayout.IntSlider("Number of options", m_NumberOfOptions, 0, 16);

			EditorUtils.GuiLine();
        }

		public override string SetLabel () {
			return "Randomized Followup";
		}

		#endif
		
	}

}
#endif