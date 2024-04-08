// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Gameplay;
using Sparrow.Verification;
using TMPro;
using UnityEngine;

namespace OTBT.Framework.UI
{
    public class SingleDialogueChoiceUI : MonoBehaviour, IVerify
    {
        [SerializeField] TextMeshProUGUI m_TextField = default;
        [SerializeField] UIAudio m_Audio;
        SingleDialogueChoice m_Choice = null;
        int m_ChoiceIndex;
        bool m_Active = true;

        public void SetMute(bool mute)
        {
            if (m_Audio) m_Audio.mute = mute;
        }

        public void Setup(SingleDialogueChoice choice, int index = -1)
        {
            m_Active = true;
            m_Choice = choice;
            m_ChoiceIndex = index;
            m_TextField.text = DialogueManager.instance.TransformLine(choice.lineText);
        }

        public void ButtonHit()
        {
            if (!m_Active) return;
            m_Active = false;
            m_Choice.onSelectionCallback?.Invoke(m_ChoiceIndex);
        }


        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_TextField != null, "SingleDialogueChoiceUI misses Textfield", gameObject);
        }
    }
}
