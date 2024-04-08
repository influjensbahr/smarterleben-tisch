// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using OTBT.Framework.Core;
using Sparrow.Verification;
using UnityEngine;

namespace OTBT.Framework.UI
{
    [AddComponentMenu("OTBT/Atoms/Show Screen")]
    public class ShowScreen : MonoBehaviour, IVerify
    {
        [SerializeField, WideToggle] bool m_IgnoreStackReturnRules = false;
        [SerializeField] StringOrAtomReference<UIScreenAtom> m_ScreenTitle;
        public void Show()
        {
            UIScreenController.instance.ShowScreen(m_ScreenTitle, returnToFlowStrategyPosition: m_IgnoreStackReturnRules);
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_ScreenTitle != null, "Showscreen has no Screentitle", gameObject);
        }

    }

}
