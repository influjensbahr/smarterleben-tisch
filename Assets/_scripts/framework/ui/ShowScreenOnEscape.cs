// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 

using OTBT.Framework.Core;
using Sparrow.Verification;
using UnityEngine;

namespace OTBT.Framework.UI
{
    public class ShowScreenOnEscape : MonoBehaviour, IVerify
    {
        [SerializeField] StringOrAtomReference<UIScreenAtom> m_Screen;

        void Start()
        {
            EventManager.instance.StartListening("escape_pressed", OpenScreen);
        }

        void OpenScreen()
        {
            UIScreenController.instance.GetScreen(m_Screen, out UIScreen screen);
            if (screen == null) return;

            UIScreenController.instance.ShowScreen(m_Screen);
        }


        public void Verify(CheckVerifyInterface verify)
        {
            verify.Check(!m_Screen.isEmpty, "No Screen reference set.", this);
        }
    }
}
