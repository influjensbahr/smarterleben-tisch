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
using UnityEditor;
using OTBT.Framework.Utils.Editor;
#endif

namespace AC {

	[System.Serializable]
	public class SwitchSceneAction : Action {
		public override ActionCategory Category => ActionCategory.OTBT;
		public override string Title => "Core/Switch Scene";
		public override string Description => "Switches to another scene using our OTBT Manager";

		// Declare variables here
		[SerializeField] StringOrAtomReference<SceneAtom> m_TargetScene = new StringOrAtomReference<SceneAtom>();
        [SerializeField] bool m_FadeToBlack = true;

		override public float Run() {
			SceneLoadManager.instance.SwitchToScene(m_TargetScene, m_FadeToBlack);
			return 0f;
		}

		override public void Skip() {
			Run();
		}


#if UNITY_EDITOR

		override public void ShowGUI() {
			m_TargetScene.ShowGUI(this, "Scene-name");
			m_FadeToBlack = EditorGUILayout.Toggle("Use fade to black?", m_FadeToBlack);

            AfterRunningOption();
		}


		public override string SetLabel() {
			return "Switch scene: " + m_TargetScene.ToString();
		}

#endif
	}
}
#endif