using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sparrow.Verification;
using UnityEngine;

public class Eventmanager : MonoBehaviour
{
    [SerializeField] Eventbase m_Eventbase;
    Dictionary<Eventbase.TravelEventType, int> m_EventValues = new Dictionary<Eventbase.TravelEventType, int>();

    public void AddEvent(Eventbase.TravelEventType eventType, int value)
    {
        if (!m_EventValues.ContainsKey(eventType))
        {
            m_EventValues[eventType] = 0;
        }

        m_EventValues[eventType] += value;
        Debug.Log($"Event {eventType} added with value {value}");

        CheckAndTriggerEvent(eventType);
    }

    private void CheckAndTriggerEvent(Eventbase.TravelEventType eventType)
    {
            foreach (var eventItem in m_Eventbase.Events)
            {
                Debug.Log($"Checking event {eventItem.EventType}: {m_EventValues[eventType]} :: {eventItem.TriggerTreshold}");
                if (eventItem.EventType == eventType && m_EventValues[eventType] >= eventItem.TriggerTreshold)
                {
                    TriggerEvent(eventType);
                    return;
                }
            }
    }

    public void TriggerEvent(Eventbase.TravelEventType eventType)
    {
        Debug.Log($"Event {eventType} triggered");
    }

    public void TestEventManager()
    {
        AddEvent(Eventbase.TravelEventType.Rebellion, 5);
    }
}
