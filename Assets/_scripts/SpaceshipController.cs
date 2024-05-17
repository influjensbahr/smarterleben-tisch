

using OTBT.Framework.Audio;
using OTBT.Framework.Core;
using OTBT.Framework.Gameplay;
using OTBT.Framework.UI;
using OTBT.Framework.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpaceshipController : Singleton<SpaceshipController>
{
    [SerializeField] StringOrAtomReference<UIScreenAtom> m_MainScreen = default;

    [SerializeField] SpaceshipEvent m_TutorialEvent = default;
    [SerializeField] List<SpaceshipEvent> m_Generation1 = new List<SpaceshipEvent>();

    [SerializeField] Image m_Earth = default;
    [SerializeField] Sprite m_CircleSprite = default;
    [SerializeField] Sprite m_CurrentRoundSprite = default;
    [SerializeField] Transform m_StepsFromEathToMoon = default;
    [SerializeField] Image m_Moon = default;

    [SerializeField] Button m_YesButton = default;

    [SerializeField] Button m_NoButton = default;
    [SerializeField] Sprite m_DefaultPerson = default;
    [SerializeField] Image m_PersonDisplay = default;

    [SerializeField] Transform m_CriticalsParent = default;
    [SerializeField] GameObject m_CriticalsNature = default;
    [SerializeField] GameObject m_CriticalsPeople = default;
    [SerializeField] GameObject m_CriticalsOrder = default;
    [SerializeField] GameObject m_CriticalsShip = default;

    [SerializeField] AudioSource m_SoundAlert = default;
    [SerializeField] AudioSource m_SoundYes = default;
    [SerializeField] AudioSource m_SoundNo = default;


    [SerializeField] SpaceshipEvent m_Event5;
    [SerializeField] SpaceshipEvent m_Event12;
    [SerializeField] SpaceshipEvent m_Event17;

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

    int currentRound = 0;

    private void Start()
    {
        UIScreenController.instance.ShowScreen(m_MainScreen);
        EventManager.instance.TriggerEvent("valueUpdate");
        EventManager.instance.TriggerInTime(3f, () => SetNextInTime(m_TutorialEvent));
        UpdateRundenAnzeige(0);
    }

    public void SetNextInTime(SpaceshipEvent ev)
    {
        EventManager.instance.TriggerInTime(2.5f, () => SetEvent(ev));
    }

    private void SetEvent(SpaceshipEvent ev)
    {
        if(ev == null)
        {
            // Finde den niedrigsten timesPlayed-Wert
            int minTimesPlayed = m_Generation1.Min(e => e.timesPlayed);

            // Filtere die Einträge mit dem niedrigsten timesPlayed-Wert
            var leastPlayedEvents = m_Generation1.Where(e => e.timesPlayed == minTimesPlayed).ToList();

            // Wähle zufällig einen dieser Einträge aus
            ev = leastPlayedEvents[Random.Range(0, leastPlayedEvents.Count)];
            currentRound++;
        } else if(currentRound > 0)
        {
            currentRound++;
        }
        if (currentRound == 3)
            ev = m_Event5;
        if (currentRound == 7)
            ev = m_Event12;
        if (currentRound == 12)
            ev = m_Event17;
        currentEvent = ev;
        ev.timesPlayed++;
        if (m_SoundAlert)
            m_SoundAlert.Play();
        UpdateRundenAnzeige(currentRound);
        m_YesButton.interactable = true;
        m_NoButton.interactable = true;
        m_PersonDisplay.sprite = ev.m_Person == null ? m_DefaultPerson : ev.m_Person;
        GUIContoller.instance.SetDisplay(currentEvent);
    }

    public void SetzePrefab(GameObject prefab)
    {
        if (prefab == null || currentRound <= 0 || currentRound > m_StepsFromEathToMoon.childCount) return;

        // Hole den Marker für die aktuelle Runde
        Transform currentMarker = m_StepsFromEathToMoon.GetChild(currentRound-1);

        // Instantiate das Prefab genau an der Position des Markers
        GameObject neuesObjekt = Instantiate(prefab, currentMarker.position, Quaternion.identity, m_CriticalsParent);
    }

    public void UpdateRundenAnzeige(int runde)
    {
        int totalElements = m_StepsFromEathToMoon.childCount;

        for (int i = 0; i < totalElements; i++)
        {
            Image sr = m_StepsFromEathToMoon.GetChild(i).GetComponent<Image>();
            sr.color = Color.white;
            sr.sprite = m_CircleSprite;
        }

        // Ab Runde 1 die Punkte grau färben
        for (int i = 0; i < runde; i++)
        {
            if (i < totalElements)
            {
                Image sr = m_StepsFromEathToMoon.GetChild(i).GetComponent<Image>();
                if(i == runde - 1) sr.sprite = m_CurrentRoundSprite;
                sr.color = Color.gray;
            }
        }

        m_Earth.color = runde == 0 ? Color.white : Color.grey;
    }

    public void YesButton()
    {
        currentEvent.Yes(); 
        if (m_SoundYes)
            m_SoundYes.Play();
        m_YesButton.interactable = false;
        m_NoButton.interactable = false;
    }

    public void NoButton()
    {
        currentEvent.No();
        if (m_SoundNo)
            m_SoundNo.Play();
        m_YesButton.interactable = false;
        m_NoButton.interactable = false;
    }

    public void ActivateCriticals()
    {
        if (m_Nature < 0 || m_Nature > 10)
        {
            if (m_Nature < 0)
                DialogueManager.instance.notifications.AddLine("Alert: Nature in collapse! Food is scarce, plant life is dying. Hope sprouts from new seeds and frozen samples. Situation stabilizes, but the crisis intensifies!", type: SingleNotificationUI.NotificationType.CLICKED);
            if (m_Nature > 10)
                DialogueManager.instance.notifications.AddLine("Warning: Nature overgrowing! Plants and animals are taking over. Security measures kick in, but resistance builds. Normalcy returns, yet the threat persists!", type: SingleNotificationUI.NotificationType.CLICKED);
            m_Nature = 5;
            SetzePrefab(m_CriticalsNature);
            m_NatureCriticals++; EventManager.instance.TriggerEvent("valueUpdate");
        }
        if (m_Technology < 0 || m_Technology > 10)
        {
            if (m_Technology < 0)
                DialogueManager.instance.notifications.AddLine("Critical failure: Technological disaster! Parts of the ship are falling apart, manual labor replaces automation. Spare parts breathe hope. Status quo restored, but danger looms!", type: SingleNotificationUI.NotificationType.CLICKED);
            if (m_Technology > 10)
                DialogueManager.instance.notifications.AddLine("Warning: AI uprising! Artificial intelligence is taking control. Hackers and programmers fight for power. Everything seems normal, but the threat is real!", type: SingleNotificationUI.NotificationType.CLICKED);
            m_Technology = 5;
            m_TechnologyCriticals++; EventManager.instance.TriggerEvent("valueUpdate");
            SetzePrefab(m_CriticalsShip);
        }
        if (m_People < 0 || m_People > 10)
        {
            if (m_People < 0)
                DialogueManager.instance.notifications.AddLine("Catastrophe: Mass dying! The crew is diminishing. Yet life goes on, though the situation becomes graver!", type: SingleNotificationUI.NotificationType.CLICKED);
            if (m_People > 10)
                DialogueManager.instance.notifications.AddLine("Overpopulation: Space is tight! Too many people, not enough room or resources. Life continues, but the pressure mounts!", type: SingleNotificationUI.NotificationType.CLICKED);
            m_People = 5;
            m_PeopleCriticals++; EventManager.instance.TriggerEvent("valueUpdate");
            SetzePrefab(m_CriticalsPeople);
        }
        if (m_Order < 0 || m_Order > 10)
        {
            if (m_Order < 0)
                DialogueManager.instance.notifications.AddLine("Chaos: Order is crumbling! Anarchy spreads. Peace returns, but the crisis deepens!", type: SingleNotificationUI.NotificationType.CLICKED);
            if (m_Order > 10)
                DialogueManager.instance.notifications.AddLine("Dictatorship: An iron fist! Rebels dare to fight. Daily life normalizes, but the danger remains!", type: SingleNotificationUI.NotificationType.CLICKED);
            m_Order = 5;
            m_OrderCriticals++; EventManager.instance.TriggerEvent("valueUpdate");
            SetzePrefab(m_CriticalsOrder);
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
