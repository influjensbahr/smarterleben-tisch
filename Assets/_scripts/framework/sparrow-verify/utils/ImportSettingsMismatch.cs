//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Sparrow.Verification
{
    [Serializable]
    public class ImportSettingsMismatch<TImportSettings> where TImportSettings : ImportSettingsBase
    {
        public TImportSettings PresetToUse { get; }
        public ModelImporter ModelImporter { get; }
        public TextureImporter TextureImporter { get; }
        public AudioImporter AudioImporter { get; }
        public List<string> MismatchedSettings { get; }

        public ImportSettingsMismatch(TImportSettings presetToUse, AudioImporter audioImporter)
        {
            PresetToUse = presetToUse;
            AudioImporter = audioImporter;
            MismatchedSettings = new List<string>();
        }
        
        public ImportSettingsMismatch(TImportSettings presetToUse, TextureImporter textureImporter)
        {
            PresetToUse = presetToUse;
            TextureImporter = textureImporter;
            MismatchedSettings = new List<string>();
        }
        public ImportSettingsMismatch(TImportSettings presetToUse, ModelImporter modelImporter)
        {
            PresetToUse = presetToUse;
            ModelImporter = modelImporter;
            MismatchedSettings = new List<string>();
        }

        public void AddMismatchedSetting(string settingName)
        {
            MismatchedSettings.Add(settingName);
        }
    }
}
#endif