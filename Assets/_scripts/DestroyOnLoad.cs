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
    /// Removes the GameObject once the scene starts running.
    /// Uses Destroy in player builds and DestroyImmediate in editor for immediate cleanup.
    /// </summary>
    public class DestroyOnLoad : MonoBehaviour
    {
        public void Start()
        {
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#else
            Destroy(gameObject);
#endif
        }

    }
}
