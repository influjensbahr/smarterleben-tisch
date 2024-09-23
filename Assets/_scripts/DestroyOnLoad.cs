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
    /// Destroys the GameObject on Build.
    /// </summary>
    public class DestroyOnLoad : MonoBehaviour
    {
        public void Start()
        {
            DestroyImmediate(gameObject);
        }

    }
}
