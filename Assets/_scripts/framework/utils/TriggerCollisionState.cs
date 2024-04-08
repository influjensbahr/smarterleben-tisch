//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Keeps track of objects in contact with the Adventure Creator player.
    /// </summary>
    public class TriggerCollisionState : MonoBehaviour
    {
        bool m_IsColliding = false;
        public bool isColliding => m_IsColliding;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == AC.KickStarter.player.gameObject)
            {
                m_IsColliding = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject == AC.KickStarter.player.gameObject)
            {
                m_IsColliding = false;
            }
        }
    }
}
#endif
