//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using OTBT.Framework.Utils;
using OTBT.Framework.Utils.Editor;
using Sparrow.Verification;
using System.CodeDom;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Verify
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class VerifyCustomEditors : Editor
    {
        public override void OnInspectorGUI()
        {
            var verify = target as IVerify;
            var type = target.GetType();
            bool singleton = false;
            if (target.GetType().BaseType.IsGenericType)
                singleton = target.GetType().BaseType.GetGenericTypeDefinition() == typeof(Singleton<>);
            if (verify != null) OTBT.Framework.Utils.Editor.EditorUtils.BeginColoredEditor(singleton);
            if (verify != null) OTBT.Framework.Utils.Editor.EditorUtils.DrawTinyLogoHeader(type.ToString() + (singleton ? " (Singleton)" : ""));
            base.OnInspectorGUI();
            if (verify != null) OTBT.Framework.Utils.Editor.EditorUtils.DrawVerify(verify);
            var verifyExtend = target as IExtendDefaultEditor;
            if (verifyExtend != null)
            {
                OTBT.Framework.Utils.Editor.EditorUtils.Space();
                verifyExtend.ExtendDefaultEditor();
            }
            if (verify != null) OTBT.Framework.Utils.Editor.EditorUtils.EndColoredEditor();
        }
    }
}
#endif