//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Audio
{
    [CustomEditor(typeof(SoundCue))]
    public class SoundCueEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtils.BeginColoredEditor();
            EditorUtils.DrawLogoHeader();
            EditorUtils.DrawWikiLinkButton("https://wiki.beatentrack.games/doc/audio-Ffch4FrHDN#h-soundcue-used-the-setup-how-a-sound-is-played");

            SoundCue cue = (SoundCue)target;

            if(cue.count > 1)
                cue.ShowSequenceGUI();

            base.OnInspectorGUI();
            EditorUtils.DrawVerify(cue);
            EditorUtils.EndColoredEditor();
        }
    }
}
