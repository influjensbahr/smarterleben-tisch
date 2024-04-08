//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Animation;
using OTBT.Framework.Gameplay;
using Sparrow.Verification;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    public class DialogueChoiceDisplayUI : MonoBehaviour, IVerify
    {
        [SerializeField] GameObject m_ChoicePrefab = null;
        [SerializeField] Transform m_ChoicesParent = null;
        [SerializeField] List<SingleDialogueChoiceUI> m_ChoiceDisplays = new List<SingleDialogueChoiceUI>();

        [Header("Animation Parameters")]
        [SerializeField]
        [RequireInterface(typeof(IUIAnimationWithCallback<float>))]
        private Object m_FadingAnimation;
        [SerializeField] float m_AlphaFadeTime = 1f;

        [SerializeField]
        [RequireInterface(typeof(IUIAnimationWithCallback<Transform>))]
        private Object m_MovementAnimation;
        [SerializeField] Transform m_AnchorWhenShowing = null;
        [SerializeField] Transform m_AnchorWhenHiding = null;
        [SerializeField] float m_MovementTime = 1f;

        public IUIAnimationWithCallback<Transform> movement => m_MovementAnimation as IUIAnimationWithCallback<Transform>;
        public IUIAnimationWithCallback<float> fading => m_FadingAnimation as IUIAnimationWithCallback<float>;

        public bool showing => m_Showing;

        bool m_Showing = false;

        private void Start()
        {
            fading?.ResetState();
            movement?.ResetState();
        }

        public void Show()
        {
            m_Showing = true;
            gameObject.SetActive(true);
            fading?.AnimateToState(UIAnimationStateLerper.SHOWN, m_AlphaFadeTime);
            if(m_AnchorWhenShowing != null)
                movement?.AnimateToState(m_AnchorWhenShowing, m_MovementTime);
            for (int i = 0; i < m_ChoiceDisplays.Count; i++)
                m_ChoiceDisplays[i].SetMute(false);
        }

        public void Hide()
        {
            m_Showing = false;
            fading?.AnimateToState(UIAnimationStateLerper.OUT, m_AlphaFadeTime, callback: () => gameObject.SetActive(false));
            if(m_AnchorWhenHiding != null)
                movement?.AnimateToState(m_AnchorWhenHiding, m_MovementTime);
            for (int i = 0; i < m_ChoiceDisplays.Count; i++)
                m_ChoiceDisplays[i].SetMute(true);
        }

        public void SetupChoices(List<SingleDialogueChoice> choices)
        {
            while (m_ChoiceDisplays.Count < choices.Count)
            {
                GameObject newChoice = Instantiate(m_ChoicePrefab, m_ChoicesParent);
                SingleDialogueChoiceUI ui = newChoice.GetComponent<SingleDialogueChoiceUI>();
                m_ChoiceDisplays.Add(ui);
            }

            for (int i = 0; i < m_ChoiceDisplays.Count; i++)
            {
                if (i < choices.Count)
                {
                    // when choices are removed, SingleDialogueChoice.OptionID and the actual index of the option that needs to be called mismatch.
                    // we submit our own indices for this case, and ignore the OptionID
                    m_ChoiceDisplays[i].Setup(choices[i], choices[i].optionID);
                    m_ChoiceDisplays[i].gameObject.SetActive(true);
                } else
                {
                    m_ChoiceDisplays[i].gameObject.SetActive(false);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_ChoicePrefab != null, "DialogChoiceDisplay:m_ChoicePrefab  not setup properly.", gameObject);
            checker.Check(m_ChoicesParent != null, "DialogChoiceDisplay: m_ChoicesParent not setup properly.", gameObject);
        }
    }
}
