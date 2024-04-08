// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using System;
using OTBT.Framework.UI;
using UnityEngine;
using DG.Tweening;
using OTBT.Framework.Audio;
using TMPro;

namespace OTBT.Framework.Utils
{
    [RequireComponent(typeof(UIScreen))]
    public class CreditsScreen : MonoBehaviour
    {
        [SerializeField, HideInInspector] UIScreen m_Screen;
        [SerializeField] MusicTheme m_OptionalMusic;
        [SerializeField] Credits m_Credits;
        [SerializeField] RectTransform m_ScrollContainer;
        [SerializeField] RectTransform m_Content;
        [SerializeField] TMP_Text m_HeaderPrefab;
        [SerializeField] TMP_Text m_LinePrefab;

        Tween m_Tween;
        void OnValidate()
        {
            if (m_Screen == null) m_Screen = GetComponent<UIScreen>();
        }
        public void Awake()
        {
            m_Screen.onCompleteShow.AddListener(StartAnimation);
            m_Screen.onCompleteHide.AddListener(EndAnimation);
        }

        void EndAnimation()
        {
            if (m_OptionalMusic) m_OptionalMusic.StopPlaying();

            if (m_Tween != null) m_Tween.Complete();
        }

        void StartAnimation()
        {
            if (m_OptionalMusic) m_OptionalMusic.StartPlaying();

            if (m_Tween != null) m_Tween.Complete();

            m_ScrollContainer.anchoredPosition = Vector2.zero;

            var height = m_ScrollContainer.rect.height;
            m_Tween = m_ScrollContainer.DOAnchorPosY(height, height / 150f).SetEase(Ease.Linear);
        }
    }
}
