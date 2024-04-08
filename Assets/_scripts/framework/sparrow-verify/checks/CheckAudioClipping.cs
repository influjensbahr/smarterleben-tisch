//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckAudioClipping : VerifyCheckBase
    {
        public override string description => "Audio Clipping";
        public override string longDescription => "Checks for audio files above a certain clipping threshold which might cause clipping issues when played.";
        [SerializeField] float m_ClippingThreshold = 0.994f; // You can adjust this value
        [SerializeField] int m_ClippingTolerance = 1;

        public override bool DrawSpecificProfileEditor()
        {
            float clipping = m_ClippingThreshold;
            m_ClippingThreshold = EditorGUILayout.FloatField("Clipping Threshold", m_ClippingThreshold);
            int clippingTolerance = m_ClippingTolerance;
            m_ClippingTolerance = EditorGUILayout.IntField("Clipping Tolerance", m_ClippingTolerance);
            return !Mathf.Approximately(clipping, m_ClippingThreshold) || clippingTolerance != m_ClippingTolerance;
        }

        public override void PerformCheck(GameObject gameObject)
        {
            var audioSources = gameObject.GetComponents<AudioSource>();

            foreach (var audioSource in audioSources)
            {
                if (audioSource.clip != null)
                {
                    CheckForClippingInAudioClip(audioSource.clip, gameObject);
                }
            }
        }

        public override void PerformCheck(AudioClip audioClip)
        {
            CheckForClippingInAudioClip(audioClip, null);
        }

    
        private void CheckForClippingInAudioClip(AudioClip audioClip, GameObject gameObject)
        {
            if (audioClip == null)
                return;

            float[] samples = new float[audioClip.samples * audioClip.channels];
                   
            if (audioClip.loadType == AudioClipLoadType.DecompressOnLoad) { 
                audioClip.GetData(samples, 0);
            } else {
                return;
            }

            int clippingsFound = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                if (Math.Abs(samples[i]) >= m_ClippingThreshold)
                {
                    clippingsFound++;
                    if (clippingsFound > m_ClippingTolerance)
                    {
                        string message = $"Audio Clip '{audioClip.name}' contains samples that may cause clipping.";
                        if (gameObject != null)
                        {
                            AddFailedCheck(message, gameObject).WithSeverity(VerifyResult.Severity.Warning);
                            return;
                        }
                        else
                        {
                            AddFailedCheck(message, audioClip).WithSeverity(VerifyResult.Severity.Warning);
                            return;
                        }
                    }
                }
            }
        }
    }
}
#endif