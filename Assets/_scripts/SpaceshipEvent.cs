
using OTBT.Framework.Core;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "SpaceshipEvent")]
public class SpaceshipEvent : ScriptableObject
{
    [SerializeField] public SpaceshipEvent m_NextYes;
    [SerializeField] public SpaceshipEvent m_NextNo;
    public int timesPlayed = 0;

    [SerializeField] public string m_Title = "";
    [SerializeField] public string m_Text = "";
    [SerializeField] public int m_Generation = 0;

    [Header("Conditions")]
    [SerializeField] public float m_NatureMin = 0f;
    [SerializeField] public float m_NatureMax = 10f;
    [SerializeField] public float m_TechnologyMin = 0f;
    [SerializeField] public float m_TechnologyMax = 10f;
    [SerializeField] public float m_PeopleMin = 0f;
    [SerializeField] public float m_PeopleMax = 10f;
    [SerializeField] public float m_OrderMin = 0f;
    [SerializeField] public float m_OrderMax = 10f;

    [Header("Consequences - Yes")]
    [SerializeField] public float m_NatureYes = 0f;
    [SerializeField] public float m_TechnologyYes = 0f;
    [SerializeField] public float m_PeopleYes = 0f;
    [SerializeField] public float m_OrderYes = 0f;

    [Header("Consequences - No")]
    [SerializeField] public float m_NatureNo = 0f;
    [SerializeField] public float m_TechnologyNo = 0f;
    [SerializeField] public float m_PeopleNo = 0f;
    [SerializeField] public float m_OrderNo = 0f;

    [SerializeField] public Sprite m_Person = null;
    public void Yes()
    {
        SpaceshipController.instance.ChangeValues((int) m_NatureYes, (int)m_TechnologyYes, (int)m_PeopleYes, (int)m_OrderYes);
        SpaceshipController.instance.SetNextInTime(m_NextYes);
    }

    public void No()
    {
        SpaceshipController.instance.ChangeValues((int)m_NatureNo, (int)m_TechnologyNo, (int)m_PeopleNo, (int)m_OrderNo);
        SpaceshipController.instance.SetNextInTime(m_NextNo);
    }
}
