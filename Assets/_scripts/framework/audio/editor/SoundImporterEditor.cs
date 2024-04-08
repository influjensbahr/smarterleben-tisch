
//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using OTBT.Framework.Utils.Editor;
using OTBT.Framework.Utils;

namespace OTBT.Framework.Audio
{
    [CustomEditor(typeof(SoundImporter))]
    public class SoundImporterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader();
            EditorUtils.DrawWikiLinkButton("https://wiki.beatentrack.games/doc/audio-Ffch4FrHDN#h-soundimporter-import-settings-for-sound-files-all-sound-files-need-this");

            SoundImporter importer = (SoundImporter)target;

            GUI.enabled = importer.clip != null;
            if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Play", "play.png")))
			{
				EditorUtils.PlayClip(importer.clip);
			}
			GUI.enabled = true;

            base.OnInspectorGUI();

            if (importer.clip != null)
            {
                EditorUtils.Space();
                if (importer.CheckIfShouldAdjustImportSettings())
                {
                    EditorGUILayout.HelpBox("Import settings have been changed or have not been applied yet. Please apply settings.", MessageType.Warning);
                }
            }
            GUI.enabled = importer.clip != null && importer.HasBeenSet();
            if (GUILayout.Button(EditorUtils.LabelWithGlyphicon("Set import settings", Glyphicons.DropDown)))
            {
                importer.AdjustImportSettings();
            }
            GUI.enabled = true;
            EditorUtils.DrawVerify(importer);
            EditorUtils.EndColoredEditor();
        }
    }
}
#endif