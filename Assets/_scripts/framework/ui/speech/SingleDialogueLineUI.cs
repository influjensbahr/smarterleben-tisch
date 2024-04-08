// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 


using OTBT.Framework.Animation;
using OTBT.Framework.Gameplay;
using Sparrow.Verification;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace OTBT.Framework.UI
{
    /// <summary>
    /// Script to be placed on UI elements that display dialogue lines
    /// </summary>
    public class SingleDialogueLineUI : MonoBehaviour, IVerify
    {
        [SerializeField] Image m_SpeakerPortrait = default;
        [SerializeField] TextMeshProUGUI m_NameDisplay = default;
        [SerializeField] TextMeshProUGUI m_LineDisplay = default;
        [SerializeField] float m_FadeOutTime = 0.5f;
        [SerializeField] float m_UpwardsMovementTime = 0f;
        [SerializeField] float m_FadeInTime = 0.5f;
        [SerializeField] TextMeshProUGUI m_LineIDDisplay = default;
        [SerializeField] Transform m_TopAnchor = default;

        [SerializeField]
        [RequireInterface(typeof(IUIAnimationWithCallback<float>))]
        private UnityEngine.Object m_FadingAnimation;

        [SerializeField]
        [RequireInterface(typeof(IUIAnimationWithCallback<Transform>))]
        private UnityEngine.Object m_MovementAnimation;

        private ActiveDialogue m_ActiveDialogue = null;

        public ActiveDialogue activeDialogue => m_ActiveDialogue;

        bool m_HasEnded = false;
        public Image speakerPortrait => m_SpeakerPortrait;
        public bool hasEnded => m_HasEnded;
        public IUIAnimationWithCallback<Transform> movement => m_MovementAnimation as IUIAnimationWithCallback<Transform>;
        public IUIAnimationWithCallback<float> fading => m_FadingAnimation as IUIAnimationWithCallback<float>;

        SpeakingCharacter m_Speaker = null;
        SingleDialogueLineUI m_PreviousLine = null;
        DialogueDisplayUI m_DialogueDisplay = null;
        string m_InternalLine = ""; // unlocalized for internal processess, not neccessarily the one displayed
        bool m_IsPlayer = false;

        public bool isPlayer => m_IsPlayer;
        public string internalLine => m_InternalLine;
        public SpeakingCharacter speaker => m_Speaker;
        public DialogueDisplayUI display => m_DialogueDisplay;

        public void ResetState()
        {
            m_PreviousLine = null;
            m_Speaker = null;
            m_HasEnded = false;
            fading?.ResetState();
            movement?.ResetState();
            if(m_LineIDDisplay != null) m_LineIDDisplay.text = "";
        }

        public void Setup(DialogueDisplayUI disp, SingleDialogueLineUI prevLine, bool isPlay, Transform positioningAnchor, int lineId, ActiveDialogue activeDialogue = null)
        {
            ResetState();
            m_ActiveDialogue = activeDialogue;
            m_DialogueDisplay = disp;
            m_PreviousLine = prevLine == this ? null : prevLine;
            m_HasEnded = false;
            m_IsPlayer = isPlay;
            if (m_LineIDDisplay != null) m_LineIDDisplay.text = lineId > 0 ? "id " + lineId : "";
            movement?.AnimateToState(positioningAnchor, 0f);
        }

        public void UpdateLine(SpeakingCharacter speaker, string line)
        {
            m_Speaker = speaker;
            m_InternalLine = line;
            if (m_NameDisplay != null)
            {
                m_NameDisplay.text = speaker == null ? "" : speaker.speakerName;
                m_NameDisplay.color = speaker == null ? Color.white : speaker.color;
                m_LineDisplay.text = DialogueManager.instance.TransformLine(line);
            }
            else
            {
                m_LineDisplay.text = (speaker == null ? "" : speaker.speakerName + ": ") + DialogueManager.instance.TransformLine(line);
            }

            UpdateSpeakerPortrait();

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            fading?.ResetState();
            fading?.AnimateToState(UIAnimationStateLerper.SHOWN, m_FadeInTime);
            if (m_PreviousLine != null && !m_PreviousLine.hasEnded && m_PreviousLine != this)
                m_PreviousLine.movement?.AnimateToState(m_TopAnchor, m_UpwardsMovementTime);
        }

        internal void TriggerEnding(UnityAction callback)
        {
            m_HasEnded = true;
            if (fading == null)
            {
                callback?.Invoke();
            }
            else
            {
                fading.AnimateToState(UIAnimationStateLerper.OUT, m_FadeOutTime, 1f, callback);
            }
        }

 
        void UpdateSpeakerPortrait()
        {
            if (m_SpeakerPortrait == null) return;
            if (speaker == null) return;
            if (speaker.portrait == null) return;
            if (m_PreviousLine != null && m_Speaker != null && m_PreviousLine.speaker == m_Speaker)
            {
                m_SpeakerPortrait.gameObject.SetActive(false);
                m_NameDisplay.gameObject.SetActive(false);
            }
            else
            {
                m_SpeakerPortrait.gameObject.SetActive(true);
                m_NameDisplay.gameObject.SetActive(true);
                m_SpeakerPortrait.sprite = speaker.portrait;
            }

        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_LineDisplay != null, "SingleDialogueLineUI: m_LineDisplay not setup properly", gameObject);
            checker.Check(m_TopAnchor != null, "SingleDialogueLineUI: m_TopAnchor not setup properly", gameObject);
        }
    }
}
