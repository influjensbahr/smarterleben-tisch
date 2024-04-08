//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils;
using Sparrow.Verification;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OTBT.Framework.Audio
{
    /// <summary>
    /// Provides settings for how certain sounds are played and triggered in the game
    /// </summary>
    [Serializable]
    public class SoundCue : MonoBehaviour, IVerify
    {
        [SerializeField] List<SoundImporter> m_Sounds = new List<SoundImporter>();
        [Tooltip("If this is false, the sounds plays in full 2D. If true, spatial blend value is applied")]
        [SerializeField] bool m_PlayIn3D = false;
        [Tooltip("Determines the '3D-ness' of the sound. 1 is fully played in the 3D world, 0 is played in mono.")]
        [SerializeField] float m_SpatialBlend = 1f;
        [Tooltip("Delay in seconds before the sound starts")]
        [SerializeField] float m_Delay = 0f;
        [Tooltip("Fade in time in seconds")]
        [SerializeField] float m_FadeInTime = 0f;
        [Tooltip("Fade out time in seconds")]
        [SerializeField] float m_FadeOutTime = 0f;
        [Tooltip("Scaling of the volume. 0 = silent, 1 = normal loudness")]
        [SerializeField] float m_VolumeScaling = 1f;

        RandomizedSequence m_Sequence = null;

        SoundImporter m_CurrentClip = null;

        public AudioSource source { get; set; } = null;
        public bool isStopping { get; set; } = false;
        public bool isPlaying => source == null ? false : source.isPlaying;
        public float delay => m_Delay;
        public AudioClip clip => m_CurrentClip == null ? null : m_CurrentClip.clip;
        public float fadeInTime => m_FadeInTime;
        public float fadeOutTime => m_FadeOutTime;
        public int count => m_Sounds.Count;

        public void Play()
        {
            AudioPlayer.instance.PlayOneShot(this);
        }

        public void Stop()
        {
            AudioPlayer.instance.StopOneShot(this);
        }

        public void SetupAudioSource(AudioSource src, float volumeScale = 1f, Transform position = null, bool loop = false)
        {
            if (m_Sequence == null)
                m_Sequence = new RandomizedSequence(m_Sounds.Count);

            int next = m_Sequence.GetNextIndex();
            if (next < 0) return;

            m_CurrentClip = m_Sounds[next];
            source = src;
            source.clip = m_CurrentClip.clip;
            source.spatialBlend = (m_PlayIn3D || position != null) ? m_SpatialBlend : 0f;
            source.volume = m_VolumeScaling * volumeScale;
            source.pitch = 1f;
            source.outputAudioMixerGroup = AudioVolumes.instance.GetAudioMixerGroup(m_CurrentClip.category);
            source.loop = loop;
            if (position != null) source.gameObject.transform.position = position.position;
        }

        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            checker.Check(m_Sounds.Count != 0, "Empty sound cue found", gameObject, () => { });
#endif
        }

#if UNITY_EDITOR
        public void AddSound(SoundImporter imp)
        {
            m_Sounds.Add(imp);
        }

        public void ShowSequenceGUI()
        {
            if (m_Sequence == null)
                m_Sequence = new RandomizedSequence(m_Sounds.Count);
            m_Sequence.UpdateCount(m_Sounds.Count);
            m_Sequence.ShowGUI(this);
        }

      
#endif
    }
}