// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using System;
using System.Collections;
using OTBT.Framework.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace OTBT.Framework.Networking
{
    /// <summary>
    /// Streams audio from an URL, and delivers the audio clip via events.
    /// </summary>
    public class AudioStream
    {
        public enum State
        {
            Waiting,
            Downloading,
            Finished
        }

        readonly string m_Url;
        readonly AudioType m_AudioType;

        DownloadHandlerAudioClip m_DownloadHandler;
        UnityWebRequest m_Request;
        AudioClip m_AudioClip;

        public AudioClip audioClip => m_AudioClip;

        ulong m_BytesReceived;
        float m_TimeElapsed;
        State m_State = State.Waiting;

        float downloadSpeed => m_BytesReceived / m_TimeElapsed;
        public float progress => state switch {
            State.Waiting => 0,
            State.Downloading => m_Request.downloadProgress,
            State.Finished => 1,
            _ => -1
        };
        public State state => m_State;

        public event Action<AudioClip> onClipReady;
        public event Action<AudioClip> onDownloadFinished;
        public event Action onError;

        public AudioStream(MonoBehaviour host, string url, AudioType audioType)
        {
            m_Url = url;
            m_AudioType = audioType;
            host.StartCoroutine(LoadStream());
        }

        void LogError()
        {
            Dbg.Error(audioClip, "Encountered an error loading the AudioClip.");
        }

        IEnumerator LoadStream()
        {
            var pause = new WaitForEndOfFrame();

            float startTime = Time.time;
            using (m_Request = UnityWebRequestMultimedia.GetAudioClip(m_Url, m_AudioType))
            {
                m_Request.SendWebRequest();
                while (m_Request.downloadProgress <= 0.01f) yield return pause;

                if (m_Request.result != UnityWebRequest.Result.Success && m_Request.result != UnityWebRequest.Result.InProgress)
                {
                    onError?.Invoke();
                }

                m_State = State.Downloading;

                m_DownloadHandler = m_Request.downloadHandler as DownloadHandlerAudioClip;
                m_DownloadHandler.streamAudio = true;

                m_AudioClip = m_DownloadHandler.audioClip;
                m_AudioClip.name = "audioStream";
                onClipReady?.Invoke(audioClip);

                while (m_Request.downloadProgress < 1f)
                {
                    if (m_Request.result != UnityWebRequest.Result.Success && m_Request.result != UnityWebRequest.Result.InProgress)
                    {
                        onError?.Invoke();
                    }
                    m_TimeElapsed = Time.time - startTime;
                    m_BytesReceived = m_Request.downloadedBytes;
                    m_AudioClip = m_DownloadHandler.audioClip;
                    yield return pause;
                }

                //TODO: replace streamed Audio with a non-streamed version so you can seek in it
                m_State = State.Finished;
                onDownloadFinished?.Invoke(audioClip);
            }
        }

        internal static AudioType TryGuessAudioType(string url)
        {
            if (url.Contains(".wav")) return AudioType.WAV;
            if (url.Contains(".mp3")) return AudioType.MPEG;
            return AudioType.UNKNOWN;
        }
    }
}
