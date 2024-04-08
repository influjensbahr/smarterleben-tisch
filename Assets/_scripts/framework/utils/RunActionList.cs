// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 

#if OTBT_AC
using AC;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    public class RunActionList : MonoBehaviour
    {
        [SerializeField] ActionList m_ActionList;
        [SerializeField] bool m_RunMoreThanOnce;

        public event System.Action onRun;
        public event System.Action onEnd;

        bool m_Running;
        int m_RunCount;

        void OnValidate()
        {
            if (m_ActionList == null) m_ActionList = GetComponent<ActionList>();
        }

        public void Run()
        {
            if (m_Running) return;
            if (!m_RunMoreThanOnce && m_RunCount > 0) return;

            m_ActionList.Interact();
            m_Running = true;
            m_RunCount++;
            onRun?.Invoke();
        }

        void Update()
        {
            if (!m_Running) return;
            if (m_ActionList.AreActionsRunning()) return;

            // also check for open conversations
            if (KickStarter.playerInput.activeConversation != null)
            {
                for (int i = 0; i < m_ActionList.actions.Count; i++)
                {
                    ActionConversation conv = m_ActionList.actions[i] as ActionConversation;
                    if (conv != null && conv.conversation == KickStarter.playerInput.activeConversation)       
                       return;
                }
            }
            
            m_Running = false;
            onEnd?.Invoke();
        }
    }
}
#endif
