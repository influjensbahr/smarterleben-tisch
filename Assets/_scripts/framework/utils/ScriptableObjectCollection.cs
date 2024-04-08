// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using Sparrow.Verification;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OTBT.Framework.Utils
{
    public abstract class ScriptableObjectCollection<TObject> : ScriptableObject, IExtendDefaultEditor, IVerify, IPrepareOnBuild, IScriptableObjectCollection where TObject : ScriptableObject
    {
        [SerializeField] protected List<TObject> m_Objects = new List<TObject>();


#if UNITY_EDITOR
        void OnValidate()
        {
            Refresh<TObject>();
        }

        public void RefreshList()
        {
            Refresh<TObject>();
        }

        public void Refresh<T>() where T : TObject
        {
            if(m_Objects == null) m_Objects = new List<TObject>();
            m_Objects.Clear();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) m_Objects.Add(asset);
            }
            EditorUtility.SetDirty(this);
        }

        public string GetTypeName()
        {
            return typeof(TObject).Name;
        }


        public void ExtendDefaultEditor()
        {
            if (GUILayout.Button("Refresh list"))
            {
                RefreshList();
            }
        }

        
#endif
        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            checker.Check(m_Objects.Count > 0, "Scriptable Object collection without elements", this, () => RefreshList());
#endif
        }

        public void PrepareOnBuildOrAwake()
        {
#if UNITY_EDITOR
            RefreshList();
#endif
        }
    }
}
