
//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

using UnityEngine;
using OTBT.Framework.AI;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC { 

	[System.Serializable]
	public class CheckKnowledgeFactAction : Action {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "AI/Check single fact";
        public override string Description => "Checks the value of a single fact";
        public override int NumSockets => 2;

        [SerializeField] FuzzyOption m_FuzzyOption = new FuzzyOption();

		public override int GetNextOutputIndex()
        {
			return KnowledgeManager.instance.MatchSingleOption(m_FuzzyOption) ? 1 : 0;
        }
		
		#if UNITY_EDITOR

		override public void ShowGUI () {
			if(m_FuzzyOption == null) m_FuzzyOption = new FuzzyOption();
            EditorGUILayout.HelpBox("Exit 0 means the condition is FALSE, 1 means the condition is TRUE", MessageType.Info);
			m_FuzzyOption.ShowGUI(this);
			EditorUtils.GuiLine();
        }

		public override string SetLabel () {
			return "Check single fact";
		}

		#endif
		
	}

}
#endif