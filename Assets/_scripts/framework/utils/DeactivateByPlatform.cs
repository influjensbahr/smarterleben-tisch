//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using Sparrow.Verification;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Deactivates GameObjects on specific platforms to mitigate incompatibilities.
    /// </summary>
    public class DeactivateByPlatform : MonoBehaviour,IVerify, IPrepareOnBuild
    {
        [SerializeField] List<RuntimePlatform> m_PlatformsToDeactivateOn = new List<RuntimePlatform>();
        [SerializeField] bool m_AlsoDestroyObject = false;

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_PlatformsToDeactivateOn.Count != 0, "DeactivateByPlatform has no platforms", gameObject);
        }


        public void PrepareOnBuildOrAwake()
        {
            foreach (RuntimePlatform rp in m_PlatformsToDeactivateOn)
            {
                if (Application.platform != rp)
                    continue;

                gameObject.SetActive(false);

                if (m_AlsoDestroyObject)
                    DestroyImmediate(gameObject);

                return;
            }
        }
    }
}
