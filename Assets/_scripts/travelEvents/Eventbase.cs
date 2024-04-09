using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Eventbase", menuName = "SpaceJam/TravelEvents/Eventbase", order = 0)]
public class Eventbase : ScriptableObject
{
    [Serializable]
    public enum TravelEventType
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

    [SerializeField] List<DialogueOption> m_DialogueOptions = new ();
    [SerializeField] List<Events> m_Events = new ();
    public List<Events> Events => m_Events;
    public List<DialogueOption> DialogueOptions => m_DialogueOptions;
}

[Serializable]
public class DialogueOption
{
    [Serializable]
    struct Answer
    {
        public string m_AnswerText;
        public Eventbase.TravelEventType m_EventType;
        public int m_Weight;
    }
    [SerializeField] string m_DialogueText;
    [SerializeField] Answer m_AnswerTextA;
    [SerializeField] Answer m_AnswerTextB;
}

[Serializable]
public class Events
{
    public Events(Eventbase.TravelEventType eventType, int value, int triggerTreshold)
    {
        m_EventType = eventType;
        m_Value = value;
        m_TriggerThreshold = triggerTreshold;
    }
    
    [SerializeField] Eventbase.TravelEventType m_EventType;
    [SerializeField] int m_Value;
    [SerializeField] int m_TriggerThreshold;
    public Eventbase.TravelEventType EventType => m_EventType;
    public int TriggerTreshold => m_TriggerThreshold;
    public int Value => m_Value;
}
