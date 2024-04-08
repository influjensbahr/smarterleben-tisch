//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class SingleFolder<TImportSettings> where TImportSettings : ImportSettingsBase
    {
        [SerializeField] public string folderPath;
        [SerializeField] public TImportSettings importSettings; 
        
        public SingleFolder(string folderPath, TImportSettings importSettings)
        {
            this.folderPath = folderPath;
            this.importSettings = importSettings;
        }
    }
}
#endif