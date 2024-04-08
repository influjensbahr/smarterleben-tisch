//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC
using AC;
using FBT.Documents;
#endif

using OTBT.Framework.Networking;
using OTBT.Framework.Utils;
using OTBT.Framework.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Localization
{
    [CustomEditor(typeof(LocalizationDatabase))]
    public class LocalizationDatabaseEditor : Editor
    {
        private LocalizationDatabase smTarget;


        public override void OnInspectorGUI()
        {
            if (smTarget == null) smTarget = target as LocalizationDatabase;
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader("Loca Database");


            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Push ALL to Server", Glyphicons.SquareTriangleUp)))
                {
                    smTarget.textCollection.RefreshList();
                    foreach (LocalizedTextObject o in smTarget.textCollection.textObjects)
                        _ = o.PushToServer();
                }
                if (GUILayout.Button(EditorUtils.LabelWithGlyphicon(" Pull ALL from Server", Glyphicons.SquareTriangleDown)))
                {
                    smTarget.textCollection.RefreshList();
                    foreach (LocalizedTextObject o in smTarget.textCollection.textObjects)
                        _ = o.LoadFromServer();
                }
                GUILayout.EndHorizontal();
            }

            EditorUtils.Space();

            DrawDefaultInspector();


            if(smTarget.useWebServer)
            {
               if(GUILayout.Button("Test DB connection")) {
                    _ = NodeJsonDownloader.GetArrayResponse<Language>(smTarget.serverAddress + "/getLanguage?passwd=" + smTarget.serverPassword + "&projectID=" + LocalizationDatabase.instance.projectID + "&",
                    (a) =>
                    {
                        Dbg.Log(this, "Success!");
                    }, (error) =>
                    {
                        Dbg.Log(this, "Error when connecting to DB: " + error);
                    }, augmentArrayNotation: true);
                }

                if (GUILayout.Button("Synchronize Languages"))
                {
                    _ = smTarget.UpdateLanguages();
                }

                if (smTarget.serverAddress.EndsWith("/"))
                {
                    EditorGUILayout.HelpBox("The Server Address probably should not end with a /", MessageType.Warning);
                }
            }


            EditorUtils.DrawVerify(smTarget);
            EditorUtils.EndColoredEditor();
        }
    }
}
