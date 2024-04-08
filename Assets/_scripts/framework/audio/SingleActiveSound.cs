//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Core;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Audio
{
    public class SingleActiveSound
    {
        public AudioSource audioSource;
        //float initialDuration = 0f;
        //float duration = 0f;
        bool paused = false;
        UnityAction<SingleActiveSound> onComplete = null;

        void StartAudio()
        {
            if (!audioSource.isPlaying) audioSource.Play();
            EventManager.instance.StartListening(EventManager.eventUpdate.identifier, UpdateFrame);
        }

        public void StopAudio()
        {
            if (audioSource.isPlaying) audioSource.Stop();
            AudioPlayer.instance.ReleaseRunningSound(this);
            EventManager.instance.StopListening(EventManager.eventUpdate.identifier, UpdateFrame);
            onComplete?.Invoke(this);
        }

        void PauseAudio()
        {
            if (audioSource.isPlaying) audioSource.Pause();
            EventManager.instance.StopListening(EventManager.eventUpdate.identifier, UpdateFrame);
        }

        public void StartPlaying(AudioSource src, AudioClip clip, UnityAction<SingleActiveSound> _onComplete = null)
        {
            audioSource = src;
            paused = false;
            //duration = clip.length;
            //initialDuration = clip.length;
            onComplete = _onComplete;
            StartAudio();
        }

        public void SetPaused(bool p)
        {
            paused = p;
            if (p)
            {
                PauseAudio();
            }
            if (!p)
            {
                StartAudio();
            }
        }

        public void UpdateFrame()
        {
            if (paused) return;

            if (!audioSource.isPlaying)
                StopAudio();

            /*
             * OLD implementation but audioclip.length is not reliable
             * duration -= Time.deltaTime;
            if (duration <= 0f)
            {
                StopAudio();
            }*/
        }
    }
}
