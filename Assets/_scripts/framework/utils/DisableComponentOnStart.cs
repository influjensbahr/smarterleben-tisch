//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine;

namespace OTBT.Framework.Utils
{
    [RequireComponent(typeof(MonoBehaviour))]
    public class DisableComponentOnStart : MonoBehaviour
    {
        [SerializeField] MonoBehaviour m_Component = null;

        void Start()
        {
            if (m_Component != null) m_Component.enabled = false;
        }
    }

}
