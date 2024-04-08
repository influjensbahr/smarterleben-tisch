// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 

using Sparrow.Verification;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    [CreateAssetMenu(menuName = "OTBT/Utils/Credits", fileName = "Credits")]
    public class Credits : ScriptableObject, IVerify
    {
        [Serializable]
        public class CreditsGroup
        {
            [SerializeField] string m_Title;
            [SerializeField] List<string> m_Names;
            public string title => m_Title;
            public List<string> names => m_Names;
        }
        [SerializeField] List<CreditsGroup> m_Credits;

        public List<CreditsGroup> credits => m_Credits;


        public void Verify(CheckVerifyInterface checker)
        {
        }
    }

}
