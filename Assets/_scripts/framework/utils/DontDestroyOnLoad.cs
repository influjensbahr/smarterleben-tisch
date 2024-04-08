//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Sets this game object up as DontDestroyOnload
    /// </summary>
    public class DontDestroyOnLoad : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
