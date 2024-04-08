// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using DG.Tweening;
using OTBT.Framework.Localization;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Gameplay
{
    public class TutorialBubble : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] CanvasGroup m_CanvasGroup;
        [SerializeField] RectTransform m_RectTransform;
        [SerializeField] LocalizeTextUI m_LocalizedText = default;
        [SerializeField] Transform m_StartPosition;

        [Header("Behaviour")]
        [SerializeField] Vector3 m_StartAnimationDirection = Vector3.zero;
        [SerializeField] float m_FadeDuration = 0.75f;
        [SerializeField] bool m_SyncPositionContinuously = false;

        [Header("Events")]
        [SerializeField] UnityEvent m_OnAnimateIn = default;
        [SerializeField] UnityEvent m_OnAnimateOut = default;

        [SerializeField] PlaySoundButton m_PlaySoundButton = default;

        bool m_AutoPlayedAudio = false;

        Tweener tweener = null;

        private void OnValidate()
        {
            if (m_CanvasGroup == null) m_CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (m_RectTransform == null) m_RectTransform = gameObject.GetComponent<RectTransform>();
            if (m_LocalizedText == null) m_LocalizedText = gameObject.GetComponentInChildren<LocalizeTextUI>();
        }

        private void Start()
        {
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;
            m_CanvasGroup.alpha = 0f;
        }

        private void Update()
        {
            if (m_SyncPositionContinuously)
            {
                if (tweener != null)
                {
                    tweener.ChangeEndValue(m_StartPosition.position, true);
                }
                else
                {
                    tweener = m_RectTransform.DOMove(m_StartPosition.position, 0.5f);
                }
            }
        }

        public void AnimateIn(UnityAction callback = null, bool instant = false, Func<bool> autoPlayAudioFunc = null)
        {
            if (m_CanvasGroup.alpha >= 1f)
            {
                callback?.Invoke();
                return;
            }

            m_OnAnimateIn?.Invoke();
            m_RectTransform.position = m_StartPosition.position - m_StartAnimationDirection;

            m_CanvasGroup.blocksRaycasts = true;
            m_CanvasGroup.interactable = true;

            m_CanvasGroup.DOFade(1f, instant ? 0f : m_FadeDuration);
            if (tweener != null) tweener.Kill();
            tweener = m_RectTransform.DOMove(m_StartPosition.position, instant ? 0f : m_FadeDuration).OnComplete(() =>
            {
                tweener = null;
                callback?.Invoke();
                if (!m_AutoPlayedAudio && (autoPlayAudioFunc == null ? false : autoPlayAudioFunc()))
                {
                    m_PlaySoundButton.PlayCurrentSound();
                }
                m_AutoPlayedAudio = true;
            });
        }

        public void AnimateOut(UnityAction callback = null, bool instant = false)
        {
            if (m_CanvasGroup.alpha <= 0f)
            {
                callback?.Invoke();
                return;
            }
            m_PlaySoundButton.StopSound();
            m_OnAnimateOut?.Invoke();
            m_CanvasGroup.DOFade(0f, instant ? 0f : m_FadeDuration);
            if (tweener != null) DOTween.Kill(tweener);
            tweener = null;
            tweener = m_RectTransform.DOMove(m_RectTransform.position - m_StartAnimationDirection, instant ? 0f : m_FadeDuration).OnComplete(() =>
            {
                tweener = null;
                callback?.Invoke();
                m_CanvasGroup.interactable = false;
                m_CanvasGroup.blocksRaycasts = false;
            });
        }
    }
}
