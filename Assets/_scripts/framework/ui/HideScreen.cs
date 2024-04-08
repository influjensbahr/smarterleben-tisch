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
    [AddComponentMenu("OTBT/Atoms/Hide Screen")]
    public class HideScreen : MonoBehaviour, IVerify
    {
        [SerializeField] StringOrAtomReference<UIScreenAtom> m_ScreenTitle;
        public void Hide()
        {
            Debug.Log("HI");
            UIScreenController.instance.ShowScreen(m_ScreenTitle);
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_ScreenTitle != null, "HideScreen has no Screentitle", gameObject);
        }
    }
}
