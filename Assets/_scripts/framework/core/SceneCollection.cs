//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using Sparrow.Verification;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Core
{
    /// <summary>
    /// A very simple class that contains a string title and a list of scene references. These represent a situation in the game that is loaded at once.
    /// </summary>
    [CreateAssetMenu(menuName = "OTBT/Core/Scene Collection", fileName = "New Scene Collection")]
    [Serializable]
    public class SceneCollection : ScriptableObject, IVerify
    {
        [SerializeField, HideInInspector] StringOrAtomReference<SceneAtom> m_SceneTitle = new StringOrAtomReference<SceneAtom>();
        [SerializeField] List<SceneReference> m_ScenesToLoad = new List<SceneReference>();

        public StringOrAtomReference<SceneAtom> objectRef => m_SceneTitle;

        public string sceneTitle => m_SceneTitle.ToString();
        public List<SceneReference> scenesToLoad => m_ScenesToLoad;


        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            checker.Check(!m_SceneTitle.isEmpty && m_ScenesToLoad.Count != 0, "Scene Collection has no Title or Scenes", this);
            foreach (SceneReference sr in m_ScenesToLoad)
                checker.Check(sr.IsInBuildSettings(), "Scene has not been added to build yet", this, () =>
                {
                    sr.AddToBuild();
                });
#endif
        }
    }
}