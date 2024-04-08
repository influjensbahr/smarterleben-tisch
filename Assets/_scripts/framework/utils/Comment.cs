//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using Sparrow.Verification;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Class to use for comments on game objects. This component can be added to anything, but it will be removed / empty on builds.
    /// </summary>
    public class Comment : MonoBehaviour, IVerify
    {
        [TextArea(15, 20)]
        [SerializeField] string m_Note;

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(!string.IsNullOrWhiteSpace(m_Note), "Comment is placed but empty", gameObject);
        }
    }
}
