//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Audio
{
    public class SoundImporterDefaults
    {
        public enum AudioImportPreset { MUSIC, AMBIANCE, SFX_FREQUENT_SHORT, SFX_FREQUENT_MEDIUM, SFX_RARE_SHORT, SFX_RARE_MEDIUM, VOICE_OVER, NOT_YET_SET }

        public struct AudioImportSettings
        {
            public AudioImportPreset preset;
            public AudioClipLoadType loadType;
            public AudioCompressionFormat compressionFormat;
            public AudioSampleRateSetting sampleRate;
            public float quality;
        }

        // from https://blog.theknightsofunity.com/wrong-import-settings-killing-unity-game-part-2/
        public static Dictionary<AudioImportPreset, AudioImportSettings> defaults = new Dictionary<AudioImportPreset, AudioImportSettings>
        {
            {
                AudioImportPreset.VOICE_OVER,
                new AudioImportSettings()
                {
                    preset = AudioImportPreset.VOICE_OVER,
                    loadType = AudioClipLoadType.Streaming,
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    quality = 0.8f, // slightly higher quality
                    sampleRate = AudioSampleRateSetting.PreserveSampleRate
                }
            },
            {
                AudioImportPreset.MUSIC,
                new AudioImportSettings()
                {
                    preset = AudioImportPreset.MUSIC,
                    loadType = AudioClipLoadType.Streaming,
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    quality = 0.75f,
                    sampleRate = AudioSampleRateSetting.PreserveSampleRate
                }
            },
            {
                AudioImportPreset.AMBIANCE,
                new AudioImportSettings()
                {
                    preset = AudioImportPreset.AMBIANCE,
                    loadType = AudioClipLoadType.Streaming,
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    quality = 0.75f,
                    sampleRate = AudioSampleRateSetting.OptimizeSampleRate
                }
            },
            {
                AudioImportPreset.SFX_FREQUENT_SHORT,
                new AudioImportSettings()
                {
                    preset = AudioImportPreset.SFX_FREQUENT_SHORT,
                    loadType = AudioClipLoadType.DecompressOnLoad,
                    compressionFormat = AudioCompressionFormat.PCM,
                    quality = 0.85f,
                    sampleRate = AudioSampleRateSetting.OptimizeSampleRate
                }
            },
            {
                AudioImportPreset.SFX_FREQUENT_MEDIUM,
                new AudioImportSettings()
                {
                    preset = AudioImportPreset.SFX_FREQUENT_MEDIUM,
                    loadType = AudioClipLoadType.CompressedInMemory,
                    compressionFormat = AudioCompressionFormat.ADPCM,
                    quality = 0.85f,
                    sampleRate = AudioSampleRateSetting.OptimizeSampleRate
                }
            },
            {
                AudioImportPreset.SFX_RARE_SHORT,
                new AudioImportSettings()
                {
                    preset = AudioImportPreset.SFX_RARE_SHORT,
                    loadType = AudioClipLoadType.CompressedInMemory,
                    compressionFormat = AudioCompressionFormat.ADPCM,
                    quality = 0.85f,
                    sampleRate = AudioSampleRateSetting.OptimizeSampleRate
                }
            },
            {
                AudioImportPreset.SFX_RARE_MEDIUM,
                new AudioImportSettings()
                {
                    preset = AudioImportPreset.SFX_RARE_MEDIUM,
                    loadType = AudioClipLoadType.CompressedInMemory,
                    compressionFormat = AudioCompressionFormat.Vorbis,
                    quality = 0.85f,
                    sampleRate = AudioSampleRateSetting.OptimizeSampleRate
                }
            }
        };
    }
}
#endif