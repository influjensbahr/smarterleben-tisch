
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

namespace AC { 

	[System.Serializable]
	public class EventAction : Action {
		public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "Core/Trigger Event";
        public override string Description => "Calls an OTBT event";


		[SerializeField]
		StringOrAtomReference<VanillaAtom> m_EventObject = new StringOrAtomReference<VanillaAtom>();
						
		override public float Run () {
			OTBT.Framework.Core.EventManager.instance.TriggerEvent(m_EventObject.ToString());
			
			return 0f;
		}

		override public void Skip () {
			 Run ();
		}
		
		#if UNITY_EDITOR
		override public void ShowGUI () {
			m_EventObject.ShowGUI(this);
            AfterRunningOption ();
		}
		

		public override string SetLabel () {
			return "OTBT Event";
		}
		#endif
	}
}
#endif