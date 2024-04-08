//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC
using OTBT.Framework.Core;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC { 

	[System.Serializable]
	public class SaveGameAction : Action {
		public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Core/Save game";
        public override string Description => "Triggers a save of the game state";
				
		
		override public float Run () {
			_ = SaveGame.instance.Save();
			return 0f;
		}

		override public void Skip () {
			 Run ();
		}
		
		#if UNITY_EDITOR
		override public void ShowGUI () {
			EditorGUILayout.HelpBox("This action simply triggers a save of the current state to the disk.", MessageType.Info);
            AfterRunningOption ();
		}
		

		public override string SetLabel () {
			return "Save game";
		}
		#endif
	}
}
#endif