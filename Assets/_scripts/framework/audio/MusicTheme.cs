//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr 
//


using Sparrow.Verification;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Audio
{
    /// <summary>
    /// ScriptableObject to hold information about a music-Theme. This is used in the MusicManager to make music-changes at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "OTBT/Audio/Music Theme", fileName = "New Music Theme")]
    [Serializable]
    public class MusicTheme : ScriptableObject, IVerify
    {
        // Layers and states of the created theme. This is empty and can be populated through the inspector.
        [SerializeField] public List<MusicLayer> layers = new List<MusicLayer>();
        [SerializeField] public List<MusicState> states = new List<MusicState>();

        int m_CurrentState = 0;
        bool m_IsPlaying = false;

        public int currentState => m_CurrentState;
        public bool isPlaying => m_IsPlaying;

        void OnValidate()
        {
            if (states.Count == 0) states.Add(new MusicState("Default", layers.Count));
        }

        public void StartPlaying(int stateId = 0)
        {
            if (states.Count == 0 || layers.Count == 0) return;
            foreach (MusicLayer layer in layers)
                AudioPlayer.instance.PlayLoop(layer.cue, volume:0f, fadeIn: false);
            SwitchToState(stateId);
            m_IsPlaying = true;
        }

        public string[] GetStateLabels()
        {
            string[] ret = new string[states.Count];
            for (int i = 0; i < states.Count; i++)
                ret[i] = states[i].name;
            return ret;
        }

        public void SwitchToState(int stateId = 0)
        {
            var state = states[stateId];
            if (state.layerVolumes.Count != layers.Count) return;
            //@JENS: currently this is initialized with currentState = 0, but we rely on SwitchToState to fade in even when starting state 0, should be initialized as -1 maybe?
            //if (currentState == stateId) return; 
            for (int i = 0; i < state.layerVolumes.Count; i++)
                AudioPlayer.instance.FadeLoopToVolume(layers[i].cue, 0f, state.blendTime, state.layerVolumes[i]);
            m_CurrentState = stateId;
        }

        public void StopPlaying()
        {
            for (int i = 0; i < states[m_CurrentState].layerVolumes.Count; i++)
                AudioPlayer.instance.StopLoop(layers[i].cue, 0f, states[m_CurrentState].blendTime);
            m_IsPlaying = false;
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(layers.Count != 0 && states.Count != 0, "Musictheme has no layers or states", this);
        }

        /**
         * A layer contains references to an audioclip and can be equiped with an audiosource to play music in the scene.
         */
        [Serializable]
        public class MusicLayer
        {
            [SerializeField] public string name = "Layer";
            [SerializeField] public SoundCue cue;
        }

        /**
         * saves all volume information for a given state on every layer in the theme.
         */
        [Serializable]
        public class MusicState
        {
            [SerializeField] public string name;
            [SerializeField] public List<float> layerVolumes;
            [SerializeField] public float blendTime;

            public MusicState(string name, int layers)
            {
                this.name = name;
                blendTime = 0.5f;
                layerVolumes = new List<float>(new float[layers]);
                if (layerVolumes.Count > 0) layerVolumes[0] = 1f;
            }
        }
    }
}
