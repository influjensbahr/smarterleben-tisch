// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 

using Sparrow.Verification;
using UnityEngine;

namespace OTBT.Framework.Audio
{
    [AddComponentMenu("OTBT/Atoms/Play Audio")]
    public class PlaySoundCue : MonoBehaviour, IVerify
    {
        [SerializeField] SoundCue m_SoundCue;
        [SerializeField] bool m_Loop;
        [SerializeField] bool m_PlayAtPosition;

        public void Play()
        {
            if (m_SoundCue == null) return;

            var tx = m_PlayAtPosition ? transform : null;
            if (m_Loop)
            {
                AudioPlayer.instance.PlayLoop(m_SoundCue, position:tx);
                return;
            }
            AudioPlayer.instance.PlayOneShot(m_SoundCue, position:tx);
        }

        public void Stop()
        {
            if (m_Loop)
            {
                AudioPlayer.instance.StopLoop(m_SoundCue);
                return;
            }
            AudioPlayer.instance.StopOneShot(m_SoundCue);
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_SoundCue != null, "Playsoundcue has no Soundcue", gameObject);
        }
    }
}
