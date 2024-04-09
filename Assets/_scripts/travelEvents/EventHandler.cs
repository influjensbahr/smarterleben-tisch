using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventHandler", menuName = "TravelEvents/EventHandler", order = 0)]
public class EventHandler : ScriptableObject
{
    [Serializable]
    struct Answer
    {
        string m_AnswerText;
        EventType m_EventType;
    }

    [Serializable]
    public enum EventType
    {
        Rebellion,
        Shortage,
        Bandits,
        SocialUnrest,
        NaturalDisaster,
        Disease,
        TechnologicalProgress,
        Celebration,
        Harmony,
        None
    }

    [SerializeField] Dictionary<string, Answer> m_DialogueOptions = new Dictionary<string, Answer>();
    [SerializeField] Dictionary<EventType, int> m_EventWeights = new Dictionary<EventType, int>();
}