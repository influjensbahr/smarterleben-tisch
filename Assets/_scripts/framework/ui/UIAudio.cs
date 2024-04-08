// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 

using OTBT.Framework.Audio;
using Sparrow.Verification;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OTBT.Framework.UI
{
    [AddComponentMenu("OTBT/Atoms/UI Audio")]
    public class UIAudio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IVerify
    {
        [SerializeField] SoundCue m_OnHoverStart, m_OnHoverEnd, m_OnClickStart, m_OnClickEnd;

        public bool mute = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_OnHoverStart == null || mute) return;
            AudioPlayer.instance.PlayOneShot(m_OnHoverStart);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_OnHoverEnd == null || mute) return;
            AudioPlayer.instance.PlayOneShot(m_OnHoverEnd);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_OnClickStart == null || mute) return;
            AudioPlayer.instance.PlayOneShot(m_OnClickStart);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (m_OnClickEnd == null || mute) return;
            AudioPlayer.instance.PlayOneShot(m_OnClickEnd);
        }

        public void Verify(CheckVerifyInterface checker)
        {
            
        }
    }
}
