// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Gameplay;
using OTBT.Framework.Localization;
using Sparrow.Verification;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace OTBT.Framework.UI
{
    public class NotificationDisplayUI : MonoBehaviour, IVerify
    {
        [Header("Notification prefabs")]
        [SerializeField] GameObject m_PrefabNotification = null;
        [SerializeField] Transform m_DialougeLinesParent = null;

        [Header("Display options")]
        [SerializeField] int m_DefaultObjectPoolCapacity = 3;

        List<SingleNotificationUI> m_Notifications = new List<SingleNotificationUI>();
        ObjectPool<SingleNotificationUI> m_NotificationPool = null;

        public void ClearDisplay()
        {
            for(int i = m_Notifications.Count -1; i >= 0; i--)
                RemoveFromDisplay(m_Notifications[i]);
        }

        public void SetAsCurrentNotificationDisplay()
        {
            DialogueManager.instance.SetCurrentNotificationDisplay(this);
        }
        public void RemoveCurrentNotificationDisplay()
        {
            DialogueManager.instance.SetCurrentNotificationDisplay(null);
        }

        public void RemoveFromDisplay(SingleNotificationUI lin)
        {
            if (m_NotificationPool == null) InitObjectPools();
            m_NotificationPool.Release(lin);
        }

        public void AddLine(string line = "", LocalizedTextObject loca = null, Sprite icon = null, SingleNotificationUI.NotificationType type = SingleNotificationUI.NotificationType.TIMED)
        {
            if (m_NotificationPool == null) InitObjectPools();
            SingleNotificationUI ui = m_NotificationPool.Get();
            ui.transform.SetAsFirstSibling();
            ui.Setup(this, line, loca, icon, type);
            ui.TriggerStart(() => RemoveFromDisplay(ui));
        }

        public void EndLine(string line, LocalizedTextObject loca = null, bool removeImmediately = false)
        {
            foreach (SingleNotificationUI lin in m_Notifications)
            {
                if (lin.IsDisplaying(line, loca))
                {
                    if (removeImmediately)
                    {
                        RemoveFromDisplay(lin);
                    }
                    else
                    {
                        lin.TriggerEnding();
                    }
                    return;
                }
            }
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_DialougeLinesParent != null, "Dialogue system has no parent to add lines to", gameObject);
        }

        ObjectPool<SingleNotificationUI> CreateObjectPool(GameObject prefab)
        {
            return new ObjectPool<SingleNotificationUI>(() =>
            {
                GameObject newInstance = Instantiate(prefab, m_DialougeLinesParent);
                SingleNotificationUI ui = newInstance.GetComponent<SingleNotificationUI>();
                return ui;
            },
            (ui) =>
            {
                m_Notifications.Add(ui);
                ui.gameObject.SetActive(true);
            },
            (ui) =>
            {
                m_Notifications.Remove(ui);
                ui.ResetState();
                ui.gameObject.SetActive(false);
            }, defaultCapacity: m_DefaultObjectPoolCapacity);
        }

        internal void InitObjectPools()
        {
            m_NotificationPool = CreateObjectPool(m_PrefabNotification);
        }
    }
}
