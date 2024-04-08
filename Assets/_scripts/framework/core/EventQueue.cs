//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System.Collections.Generic;
using UnityEngine.Events;

namespace OTBT.Framework.Core
{
    /// <summary>
    /// Simple generic event dictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventQueue<T>
    {
        private Dictionary<T, UnityEvent> m_EventDictionary = new Dictionary<T, UnityEvent>();

        public EventQueue()
        {
            if (m_EventDictionary == null)
                m_EventDictionary = new Dictionary<T, UnityEvent>();
        }

        public void TriggerEvent(T eventName)
        {
            if (m_EventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
                thisEvent.Invoke();
        }

        public void StartListening(T eventName, UnityAction listener)
        {
            if (m_EventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
            {
                thisEvent.RemoveListener(listener);
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.RemoveListener(listener);
                thisEvent.AddListener(listener);
                m_EventDictionary.Add(eventName, thisEvent);
            }
        }

        public void StopListening(T eventName, UnityAction listener)
        {
            if (m_EventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
                thisEvent.RemoveListener(listener);
        }

        internal void ClearEventListeners(T eventName)
        {
            if (m_EventDictionary.TryGetValue(eventName, out UnityEvent thisEvent))
                thisEvent.RemoveAllListeners();
        }
    }
}