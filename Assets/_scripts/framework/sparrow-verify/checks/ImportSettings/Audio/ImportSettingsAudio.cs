//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Marc Freitag
//

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [CreateAssetMenu(fileName = "importSettingsAudio", menuName = "Sparrow/Verification/Import-Settings/Import Settings Audio", order = 0)]
    [Serializable]    
    public class ImportSettingsAudio : ImportSettingsBase
        {
            [SerializeField] public CustomPropertyField<bool> forceToMono = new CustomPropertyField<bool>(false);
            [SerializeField] public CustomPropertyField<bool> normalize = new CustomPropertyField<bool>(false);
            [SerializeField] public CustomPropertyField<bool> loadInBackground = new CustomPropertyField<bool>(true);
            [SerializeField] public CustomPropertyField<bool> ambisonic = new CustomPropertyField<bool>(false);
            [SerializeField] public CustomPropertyField<AudioCompressionFormat> compressionFormat = new CustomPropertyField<AudioCompressionFormat>(AudioCompressionFormat.Vorbis);
            [SerializeField] public CustomPropertyField<AudioClipLoadType> loadType = new CustomPropertyField<AudioClipLoadType>(AudioClipLoadType.DecompressOnLoad);
            [SerializeField] public CustomPropertyField<bool> preloadAudio = new CustomPropertyField<bool>(false);
            [SerializeField][Range(0f,100f)] public CustomPropertyField<float> quality = new CustomPropertyField<float>(100f);
            [SerializeField] public CustomPropertyField<uint> sampleRateOverride = new CustomPropertyField<uint>(0);
            [SerializeField] public CustomPropertyField<AudioSampleRateSetting> sampleRateSettings = new CustomPropertyField<AudioSampleRateSetting>(AudioSampleRateSetting.PreserveSampleRate);
        }
}
#endif