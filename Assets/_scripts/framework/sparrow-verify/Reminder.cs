// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using UnityEngine;

namespace Sparrow.Verification
{
    /// <summary>
    /// This component can be used to add a simple reminder message to a game object. The
    /// verify system will track a reference to this, making sure you don't forget to tackle
    /// this in due time!
    /// </summary>
    public class Reminder : MonoBehaviour, IVerify
    {
        [SerializeField, TextArea] string m_Message = "";

        public void Verify(CheckVerifyInterface checker)
        {
            checker.AddFailedCheck($"Reminder: {m_Message}", this, () => DestroyImmediate(this), "Reminders")
                .WithSeverity(VerifyResult.Severity.Info);
        }
    }
}
