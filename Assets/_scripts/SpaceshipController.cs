

using OTBT.Framework.Audio;
using OTBT.Framework.Core;
using OTBT.Framework.Gameplay;
using OTBT.Framework.UI;
using OTBT.Framework.Utils;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipController : Singleton<SpaceshipController>
{
    [SerializeField] StringOrAtomReference<UIScreenAtom> m_MainScreen = default;

    [SerializeField] SpaceshipEvent m_TutorialEvent = default;
    [SerializeField] List<SpaceshipEvent> m_Generation1 = new List<SpaceshipEvent>();

    [SerializeField] AudioSource m_SoundAlert = default;
    [SerializeField] AudioSource m_SoundYes = default;
    [SerializeField] AudioSource m_SoundNo = default;

    private SpaceshipEvent currentEvent;

    private int m_Nature = 5;
    private int m_Technology = 5;
    private int m_People = 5;
    private int m_Order = 5;
    private int m_NatureCriticals = 0;
    private int m_TechnologyCriticals = 0;
    private int m_PeopleCriticals = 0;
    private int m_OrderCriticals = 0;

    public static float minWaitTime => .5f;
    public static float maxWaitTime => 2f;

    public int nature => m_Nature;
    public int technology => m_Technology;
    public int people => m_People;
    public int order => m_Order;
    public int natureCriticals => m_NatureCriticals;
    public int technologyCriticals => m_TechnologyCriticals;
    public int peopleCriticals => m_PeopleCriticals;
    public int orderCriticals => m_OrderCriticals;

    private void Start()
    {
        UIScreenController.instance.ShowScreen(m_MainScreen);
        EventManager.instance.TriggerEvent("valueUpdate");
        EventManager.instance.TriggerInTime(3f, () => SetNextInTime(m_TutorialEvent));
    }

    public void SetNextInTime(SpaceshipEvent ev)
    {
        EventManager.instance.TriggerInTime(2.5f, () => SetEvent(ev));
    }

    private void SetEvent(SpaceshipEvent ev)
    {
        if(ev == null)
        {
            ev = m_Generation1[Random.Range(0, m_Generation1.Count)];
        }
        currentEvent = ev;
        if (m_SoundAlert)
            m_SoundAlert.Play();
        GUIContoller.instance.SetDisplay(currentEvent);
    }

    public void YesButton()
    {
        currentEvent.Yes(); 
        if (m_SoundYes)
            m_SoundYes.Play();
    }

    public void NoButton()
    {
        currentEvent.No();
        if (m_SoundNo)
            m_SoundNo.Play();
    }

    public void ActivateCriticals()
    {
        if (m_Nature < 0 || m_Nature > 10)
        {
            if (m_Nature < 0)
                DialogueManager.instance.notifications.AddLine("Nature below 0: no more animals & plants, no more food.renew with new seeds, frozen specimen. Things go back to normal, but have gotten more critical!");
            if (m_Nature > 10)
                DialogueManager.instance.notifications.AddLine("Nature above 10: animals and plants take over control. can only be fought back with security system, but are stronger now. Things go back to normal, but have gotten more critical!");
            m_Nature = 5;
            m_NatureCriticals++; EventManager.instance.TriggerEvent("valueUpdate");
        }
        if (m_Technology < 0 || m_Technology > 10)
        {
            if (m_Technology < 0)
                DialogueManager.instance.notifications.AddLine("Technology below 0: parts of the ship get destroyed, less automation, more manual labor. can be restored with replacement parts (but less replacement parts left. Things go back to normal, but have gotten more critical!");
            if (m_Technology > 10)
                DialogueManager.instance.notifications.AddLine("Technology above 10: the AI takes over, people lose control. AI can be fought back by hackers / programmers. Things go back to normal, but have gotten more critical!");
            m_Technology = 5;
            m_TechnologyCriticals++; EventManager.instance.TriggerEvent("valueUpdate");
        }
        if (m_People < 0 || m_People > 10)
        {
            if (m_People < 0)
                DialogueManager.instance.notifications.AddLine("People below 0: people die. reduces number of people aboard (counter?). Things go back to normal, but have gotten more critical!");
            if (m_People > 10)
                DialogueManager.instance.notifications.AddLine("People above 10: too many people, over-population. population rises, not enough space for everyone. People die. Things go back to normal, but have gotten more critical!");
            m_People = 5;
            m_PeopleCriticals++; EventManager.instance.TriggerEvent("valueUpdate");
        }
        if (m_Order < 0 || m_Order > 10)
        {
            if (m_Order < 0)
                DialogueManager.instance.notifications.AddLine("Order below 0: chaos ensues. Things go back to normal, but have gotten more critical!");
            if (m_Order > 10)
                DialogueManager.instance.notifications.AddLine("Order above 10: dictator takes over. small group of rebels can prevent it. Things go back to normal, but have gotten more critical!");
            m_Order = 5;
            m_OrderCriticals++; EventManager.instance.TriggerEvent("valueUpdate");
        }

    }

    public void ChangeValues(int n, int t, int p, int o)
    {
        m_Nature += n;
        m_Technology += t;
        m_People += p;
        m_Order += o;
        EventManager.instance.TriggerEvent("valueUpdate");
    }
}
