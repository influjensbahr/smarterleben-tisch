//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using UnityEngine;

namespace OTBT.Framework.Utils
{
    public class DisableGameObjectOnStart : MonoBehaviour
    {
        void Start() => gameObject.SetActive(false);
    }
}
