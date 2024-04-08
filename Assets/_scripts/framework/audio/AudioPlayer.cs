//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Core;
using OTBT.Framework.Networking;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace OTBT.Framework.Audio
{
    /// <summary>
    /// The main class we use to play any audio in the game!
    /// </summary>
    public class AudioPlayer : Singleton<AudioPlayer>, IVerify
    {
        [SerializeField] int m_PoolSize = 15;

        List<SoundCue> m_CurrentSounds = new List<SoundCue>();
        List<SoundCue> m_DelayCue = new List<SoundCue>(); // contains sounds that are waiting for their delay, used if they are to be stopped
        ObjectPool<AudioSource> m_AudioSourcePool;

        ThemePlayer m_MusicPlayer = new ThemePlayer();
        ThemePlayer m_AmbiancePlayer = new ThemePlayer();

        List<SingleActiveSound> m_DialogueSources = new List<SingleActiveSound>();

        public ThemePlayer music => m_MusicPlayer;
        public ThemePlayer ambiance => m_AmbiancePlayer;

        public void OnPause() {
            foreach (SingleActiveSound sound in m_DialogueSources) sound.SetPaused(true);
        }
        public void OnResume()
        {
            foreach (SingleActiveSound sound in m_DialogueSources) sound.SetPaused(false);
        }

        bool internalInit = false;
        void InitInternal()
        {
            EventManager.instance.StartListening(EventManager.pauseGameEvent.identifier, OnPause);
            EventManager.instance.StartListening(EventManager.unpauseGameEvent.identifier, OnResume);
        }

        public void ReleaseRunningSound(SingleActiveSound rel)
        {
            if (m_DialogueSources.Contains(rel))
            {
                m_DialogueSources.Remove(rel);
                m_AudioSourcePool.Release(rel.audioSource);
            }
        }

        void Awake()
        {
            m_AudioSourcePool = new ObjectPool<AudioSource>(CreatePooledItem, defaultCapacity:m_PoolSize);
        }

        AudioSource CreatePooledItem()
        {
            var go = new GameObject("Pooled Audio Source");
            go.transform.parent = transform;
            var ps = go.AddComponent<AudioSource>();
            return ps;
        }

        void CleanupAndReturn(SoundCue cue)
        {
            if (cue == null) return;
            if (cue.source == null) return;
            m_AudioSourcePool.Release(cue.source);
            m_CurrentSounds.Remove(cue);
            cue.source = null;
        }

        #region PlayOneShot

        public void PlayOneShotURL(string clipURL, UnityAction<SingleActiveSound> onStart = null, UnityAction<SingleActiveSound> onComplete = null, string typeOverride = ".mp3")
        {
            Debug.Log("Start web cache");
            WebAssetCache.instance.LoadAudioClip(clipURL, (clip) =>
            {
                Debug.Log("Web cache sucess: " + clip);
                SingleActiveSound s = PlayOneShotSpeech(clip, onComplete: onComplete);
                onStart?.Invoke(s);
            }, (e) =>
            {
                Debug.Log("Web cache fail: " + e);
                Debug.LogException(e);
            }, typeOverride: typeOverride);
        }

        public void PlayOneShot(SoundCue cue, float volume = 1f, float delay = 0f, Transform position = null)
        {
            if (cue == null) return;
            _ = PlayOneShotAsync(cue, volume, delay, position);
        }

        public void StopOneShot(SoundCue cue)
        {
            if (cue == null) return;
            if (m_DelayCue.Contains(cue))
                m_DelayCue.Remove(cue);

            if (cue.source == null) return;
            if (cue.isPlaying) cue.source.Stop();
            CleanupAndReturn(cue);
        }

        async Task PlayOneShotAsync(SoundCue cue, float volumeScale = 1f, float delay = 0f, Transform position = null)
        {
            if (delay > 0f || cue.delay > 0f)
            {
                m_DelayCue.Add(cue);
                await Task.Delay(TimeSpan.FromSeconds(Mathf.Max(delay, cue.delay)));
                if (!m_DelayCue.Contains(cue)) return;
                m_DelayCue.Remove(cue);
            }

            if (cue.source != null && cue.source.isPlaying) return;

            AudioSource playSource = m_AudioSourcePool.Get();
            m_CurrentSounds.Add(cue);
            cue.SetupAudioSource(playSource, volumeScale, position, loop:false);
            cue.source.Play();

            _ = PlayOneShotFinishTask(cue);
        }

        public AudioSource GetAudioSource() => m_AudioSourcePool.Get();
        public void ReturnAudioSource(AudioSource source) => m_AudioSourcePool.Release(source);

        public SingleActiveSound PlayOneShotSpeech(AudioClip clip, float volume = 1f, Transform position = null, AudioVolumes.SoundCategory category = AudioVolumes.SoundCategory.DIALOGUE, UnityAction<SingleActiveSound> onComplete = null, UnityAction<SingleActiveSound> onStart = null)
        {
            if (!internalInit) InitInternal();
            if (clip == null) return null;
            AudioSource playSource = m_AudioSourcePool.Get();
            playSource.clip = clip;
            playSource.volume = volume;
            playSource.outputAudioMixerGroup = AudioVolumes.instance.GetAudioMixerGroup(category);
            playSource.spatialBlend = (position != null) ? 1f : 0f;
            playSource.loop = false;
            if (position != null) playSource.gameObject.transform.position = position.position;

            SingleActiveSound sound = new SingleActiveSound();
            onStart?.Invoke(sound);
            sound.StartPlaying(playSource, clip, onComplete);
            m_DialogueSources.Add(sound);
            return sound;
        }

        public void StopSpeech(AudioClip clip)
        {
            if (clip == null) return;
            for(int i = m_DialogueSources.Count - 1; i >= 0; i--)
                if (m_DialogueSources[i].audioSource.clip.Equals(clip))
                    m_DialogueSources[i].StopAudio();
        }

        async Task PlayOneShotFinishTask(AudioSource src)
        {
            await Task.Delay(TimeSpan.FromSeconds(src.clip.length));
            m_AudioSourcePool.Release(src);
        }

        // returns the audio source to the free stack after one shot has played
        async Task PlayOneShotFinishTask(SoundCue cue)
        {
            await Task.Delay(TimeSpan.FromSeconds(cue.clip.length));
            CleanupAndReturn(cue);
        }

        #endregion

        #region PlayLoop

        public void PlayLoop(SoundCue cue, float delay = 0f, float fadeTime = 0f, float volume = 1f, Transform position = null, bool fadeIn = true)
        {
            if (cue == null) return;

            cue.isStopping = false;
            _ = PlayLoopingSoundAsync(
                cue,
                Mathf.Max(delay, cue.delay),
                fadeIn ? Mathf.Max(cue.fadeInTime, fadeTime) : 0f,
                volume,
                position);
        }

        public void StopLoop(SoundCue cue, float delay = 0f, float fadeTime = 0f)
        {
            if (cue == null) return;

            cue.isStopping = true;
            _ = StopLoopAsync(
                cue,
                delay,
                Mathf.Max(cue.fadeOutTime, fadeTime));
        }

        public void FadeLoopToVolume(SoundCue cue, float delay = 0f, float fadeTime = 0f, float targetVolume = 1f)
        {
            if (cue == null) return;

            _ = FadeLoopToVolumeAsync(
                cue,
                delay,
                fadeTime,
                targetVolume);
        }

        async Task PlayLoopingSoundAsync(SoundCue cue, float delay = 0f, float fadeDuration = 0f, float volume = 1f, Transform position = null)
        {
            if (delay > 0f)
            {
                m_DelayCue.Add(cue);
                await Task.Delay(TimeSpan.FromSeconds(delay));
                if (!m_DelayCue.Contains(cue)) return;
                m_DelayCue.Remove(cue);
                if (cue.isStopping) return;
            }

            if (cue.source != null && cue.source.isPlaying) return;

            // get audio source to start playing
            AudioSource playSource = m_AudioSourcePool.Get();
            m_CurrentSounds.Add(cue);
            cue.SetupAudioSource(playSource, volume, position, loop:true);

            if (fadeDuration > 0)
                await FadeLoopToVolumeAsync(cue, 0f, fadeDuration, volume);
            else
                cue.source.Play();
        }

        async Task StopLoopAsync(SoundCue cue, float delay = 0f, float fadeTime = 0f)
        {
            if (delay > 0f)
                await Task.Delay(TimeSpan.FromSeconds(delay));

            if (m_DelayCue.Contains(cue))
                m_DelayCue.Remove(cue);

            if (cue.source == null || !cue.isStopping || !cue.isPlaying) return;

            if (fadeTime > 0)
                await FadeLoopToVolumeAsync(cue, 0f, fadeTime, 0f);
            else
                cue.source.Stop();

            CleanupAndReturn(cue);
        }

        #endregion

        #region MusicThemePlayer

        public class ThemePlayer
        {
            MusicTheme m_CurrentMusicTheme = null;

            /// <summary>
            /// Plays a specific theme
            /// </summary>
            /// <param name="theme"></param>
            /// <param name="stateId"></param>
            public void Play(MusicTheme theme, int stateId)
            {
                if (m_CurrentMusicTheme != null && m_CurrentMusicTheme != theme)
                    m_CurrentMusicTheme.StopPlaying();

                if (m_CurrentMusicTheme == theme)
                {
                    m_CurrentMusicTheme.SwitchToState(stateId);
                } else
                {
                    m_CurrentMusicTheme = theme;
                    m_CurrentMusicTheme.StartPlaying(stateId);
                }
            }

            /// <summary>
            /// Stops the current theme
            /// </summary>
            public void Stop()
            {
                if (m_CurrentMusicTheme != null)
                    m_CurrentMusicTheme.StopPlaying();
            }

        }

        #endregion

        #region Fading

        async Task FadeLoopToVolumeAsync(SoundCue cue, float delay = 0f, float fadeTime = 0f, float targetVolume = 1f)
        {
            cue.isStopping = false;
            if (delay > 0f)
            {
                m_DelayCue.Add(cue);
                await Task.Delay(TimeSpan.FromSeconds(delay));
                if (!m_DelayCue.Contains(cue)) return;
                m_DelayCue.Remove(cue);
            }

            //@JENS: Why are we returning if the source is playing? we just want to fade its volume
            if (cue.source == null) return; //&& cue.source.isPlaying
            if (cue.isStopping) return;

            if (cue.source != null)
                await Fade(cue.source, fadeTime, targetVolume);
        }

        async Task Fade(AudioSource source, float duration, float targetVolume, int volumeChangesPerSecond = 10)
        {
            var fadeSteps = (int)(volumeChangesPerSecond * duration);
            var fadeStepDelay = (int)duration * 1000 / fadeSteps;
            var fadeStepSize = (targetVolume - source.volume) / fadeSteps;

            if (!source.isPlaying) source.Play();

            for (var i = 1; i < fadeSteps; i++)
            {
                if (source == null) break;
                source.volume += fadeStepSize;
                await Task.Delay(fadeStepDelay);
            }

            if (source != null)
                source.volume = targetVolume;
        }

        #endregion

        public void Verify(CheckVerifyInterface checker)
        { 
        }
    }
}
