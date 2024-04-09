using System;
using System.Collections.Generic;
using UnityEngine;

public class Eventmanager : MonoBehaviour
{
    [SerializeField] Eventbase m_Eventbase;
    Dictionary<Eventbase.TravelEventType, int> m_EventValues = new ();
    public event Action<Eventbase.TravelEventType> OnEventTriggered;

    void AddEvent(Eventbase.TravelEventType eventType, int value)
    {
        if (!m_EventValues.TryAdd(eventType, value))
        {
            m_EventValues[eventType] += value;
        }

        CheckAndTriggerEvent(eventType);
    }

    private void CheckAndTriggerEvent(Eventbase.TravelEventType eventType)
    {
        foreach (var eventItem in m_Eventbase.Events)
        {
            if (eventItem.EventType == eventType && m_EventValues[eventType] >= eventItem.TriggerTreshold)
            {
                TriggerEvent(eventType);
            }
        }
    }

    void TriggerEvent(Eventbase.TravelEventType eventType)
    {
        Debug.Log($"ATTENTION: {eventType} Event triggered for next Phase.");
        OnEventTriggered?.Invoke(eventType);
    }

    public void TestEventManager()
    {
        AddEvent(Eventbase.TravelEventType.Rebellion, 5);
    }
}
