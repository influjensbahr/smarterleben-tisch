//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckVerifyInterface : VerifyCheckBase
    {
#if UNITY_EDITOR
        public override string description => "IVerify interface";
        public override string longDescription => "This check calls and verifies all implementations of the IVerify interface. Use that interface to quickly add checks to your own classes.";


        public override void PerformCheckForProject() { }

        public override void PerformCheck(GameObject obj)
        {
            foreach (IVerify component in obj.GetComponents<IVerify>())
                component.Verify(this);
        }

        public override void PerformCheck(MonoBehaviour m)
        {
            if(m is IVerify veri) veri.Verify(this);
        }

        public override void PerformCheck(ScriptableObject sobj)
        {
            if (sobj is IVerify veri) veri.Verify(this);
        }
#endif
    }
}

