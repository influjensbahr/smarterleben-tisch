using System;
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

    [Tooltip("Enter a dialogue text and two answer text. Select also an EventType which you want to support. Once the trigger threshold is reached, the Event happens.")][SerializeField] List<DialogueOption> m_DialogueOptions = new ();
    [Tooltip("Select a TravelEventType and set its properties to use it ingame")][SerializeField] List<Events> m_Events = new ();
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
    public Events(Eventbase.TravelEventType eventType, int value, int triggerThreshold)
    {
        m_EventType = eventType;
        m_Value = value;
        m_TriggerThreshold = triggerThreshold;
    }
    
    [SerializeField] Eventbase.TravelEventType m_EventType;
    [SerializeField] int m_Value;
    [SerializeField] int m_TriggerThreshold;
    public Eventbase.TravelEventType EventType => m_EventType;
    public int TriggerTreshold => m_TriggerThreshold;
    public int Value => m_Value;
}
