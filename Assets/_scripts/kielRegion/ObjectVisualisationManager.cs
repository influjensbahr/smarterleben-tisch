using System;
using System.Collections.Generic;
using TuioNet.Tuio11;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

[Serializable]
public struct ObjectMapping
{
    public uint id;
    public ProjectCategory category;

    public string CategoryToString()
    {
        switch(category)
        {
            case ProjectCategory.RegionaleDatenplattform: return "Regionale Datenplattform";
            case ProjectCategory.SmarteMobilitaet: return "Smarte Mobilität";
            case ProjectCategory.Quartiersentwicklung: return "Quartiersentwicklung";
            case ProjectCategory.KuestenUndMeeresschutz: return "Küsten- und Meeresschutz";
            case ProjectCategory.Kompetenzaufbau: return "Kompetenzaufbau";
            case ProjectCategory.Beteiligung: return "Beteiligung";
            default: return "";
        }
    }
}

public class ObjectVisualisationManager : MonoBehaviour
{
    [SerializeField] ProjectInfoButton m_ProjectInfo;
    [SerializeField] ProjectInfosView m_ProjectInfosView;
    [SerializeField] CanvasGroup m_ProjectInfosViewCanvasGroup;
    [SerializeField] ProjectDataContainer m_ProjectDataContainer;
    [SerializeField] ObjectMapping[] m_ProjectObjectMappings;
    [SerializeField] float m_Radius = 100f; // Radius of the circle around the Button Object where the ProjectInfos will be displayed
    [SerializeField] float m_animDelay = 0.2f;
    [SerializeField] float m_RotationSpeed = 10f; // Rotation speed of the ProjectInfos

    [SerializeField] TextMeshProUGUI m_GroupCaption = default;
    [SerializeField] Image m_TriggerRing = default;

    ProjectCategory m_Category;
    public ProjectCategory category => m_Category;

    Tuio11Object currentTuioObject = null;
    List<ProjectInfoButton> m_ProjectInfosList = new ();

    DisplayState m_CurrentDisplayState = DisplayState.NOT_SHOWING;
    float m_StateSwitchDelta = 0f;
    bool m_Destroying = false;

    private List<Tween> activeTweens = new();

    enum DisplayState
    {
        NOT_SHOWING, SHOWING_RING, SHOWING_INFO
    }

    public List<ProjectInfoButton> ProjectInfosList => m_ProjectInfosList;

    void OnEnable()
    {
        if (m_Destroying) return;
        CustomTuio11Visualizer.onObjectAdd += ShowInfos;
        CustomTuio11Visualizer.onObjectRemove += HideInfos;
    }

    void OnDisable()
    {
        if (m_Destroying) return;
        CustomTuio11Visualizer.onObjectAdd -= ShowInfos;
        CustomTuio11Visualizer.onObjectRemove -= HideInfos;
    }
    private void KillAllTweens()
    {
        foreach (var tween in activeTweens)
        {
            tween.Kill();
        }
        activeTweens.Clear();
    }

    public void ShowDetailInfo(ProjectInfoButton infoButton, KielRegionProjectDataObject info)
    {
        if (m_Destroying) return;
        if (m_StateSwitchDelta > 0f) return;
        KillAllTweens();
        if (m_CurrentDisplayState == DisplayState.SHOWING_RING)
        {
            m_ProjectInfosView.SetProjectInfos(info);
            m_CurrentDisplayState = DisplayState.SHOWING_INFO;
            foreach (var projectButton in m_ProjectInfosList)
            {
                projectButton.GetComponent<CanvasGroup>().interactable = false;
                activeTweens.Add(projectButton.GetComponent<CanvasGroup>().DOFade(0f, .75f));
            }
            m_ProjectInfosView.Show(m_ProjectInfosList.Count > 1);
            m_ProjectInfosViewCanvasGroup.interactable = true;
            m_StateSwitchDelta = .75f;
        }
    }

    public void HideDetailInfo()
    {
        if (m_Destroying) return;
        if (m_StateSwitchDelta > 0f) return;
        if (m_ProjectInfosList.Count <= 0) return;
        KillAllTweens();
        if (m_CurrentDisplayState == DisplayState.SHOWING_INFO)
        {
            m_CurrentDisplayState = DisplayState.SHOWING_RING;
            foreach (var projectButton in m_ProjectInfosList)
            {
                projectButton.GetComponent<CanvasGroup>().interactable = true;
                activeTweens.Add(projectButton.GetComponent<CanvasGroup>().DOFade(1f, .75f));
            }
            m_ProjectInfosView.Hide();
            m_ProjectInfosViewCanvasGroup.interactable = false;
            m_StateSwitchDelta = .75f;
        }
    }

    async void SpawnRingAnimation()
    {
        if (m_Destroying) return;
        for (int i = 0; i < 2; i++)
        {
            var ring = Instantiate(m_TriggerRing, transform.position, Quaternion.identity);
            ring.transform.SetParent(transform);
            ring.transform.localScale = Vector3.zero; // Startgröße 0
            ring.gameObject.SetActive(true);
            // Skaliere den Ring auf die gewünschte Größe
            ring.transform.DOScale(15f, 1f);

            ring.DOFade(0, 1f).OnComplete(() => Destroy(ring.gameObject));
            await Task.Delay(200);
        }
    }

    private void Update()
    {
        if (m_Destroying) return;
        m_StateSwitchDelta -= Time.deltaTime;
        if (m_CurrentDisplayState == DisplayState.SHOWING_RING)
        {
            var angleStep = 360f / m_ProjectInfosList.Count;
            var radius = m_Radius;
            var centerPosition = new Vector3(currentTuioObject.Position.X, currentTuioObject.Position.Y, 0);

            for (var i = 0; i < m_ProjectInfosList.Count; i++)
            {
                var angle = -i * angleStep;
                var x = Mathf.Cos(angle * Mathf.Deg2Rad - Time.time * m_RotationSpeed) * radius;
                var y = Mathf.Sin(angle * Mathf.Deg2Rad - Time.time * m_RotationSpeed) * radius;

                m_ProjectInfosList[i].transform.localPosition = centerPosition + new Vector3(x, y, 0);
            }
        }
    }

     void ShowInfos(Tuio11Object tuioObject)
    {
        if (m_Destroying) return;
        if (currentTuioObject != null)
        {
            return;
        }
        
        currentTuioObject = tuioObject;

        SpawnRingAnimation();
        CustomTuio11Visualizer.instance.objectVisManagers.Add(this);

        foreach (var objectMapping in m_ProjectObjectMappings)
        {
            if (objectMapping.id != tuioObject.SymbolId) continue;

            m_GroupCaption.text = objectMapping.CategoryToString();
            var projects = m_ProjectDataContainer.GetProjectsByCategory(objectMapping.category);
            m_Category = objectMapping.category;
            if (projects.Count <= 0) continue;
            
            if(projects.Count == 1)
            {
                m_CurrentDisplayState = DisplayState.SHOWING_INFO;
                var project = projects[0];
                m_ProjectInfosView.SetProjectInfos(project);
                m_ProjectInfosView.Show(m_ProjectInfosList.Count > 1);
                m_ProjectInfosViewCanvasGroup.interactable = true;
                m_StateSwitchDelta = .75f;
            } else
            {
                m_CurrentDisplayState = DisplayState.SHOWING_RING;
                var angleStep = 360f / projects.Count;
                var radius = m_Radius;
                var centerPosition = new Vector3(tuioObject.Position.X, tuioObject.Position.Y, 0);

                for (var i = 0; i < projects.Count; i++)
                {
                    var project = projects[i];
                    var angle = -i * angleStep;
                    var x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                    var y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

                    var projectObject = Instantiate(m_ProjectInfo, transform);
                    projectObject.transform.localPosition = centerPosition + new Vector3(x, y, 0);
                    projectObject.SetProjectInfos(project.title, this, project);
                    projectObject.GetComponent<CanvasGroup>().alpha = 0f;
                    projectObject.GetComponent<CanvasGroup>().DOFade(1f, .75f);

                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(projectObject.transform.DOScale(Vector3.zero, 0f));
                    sequence.AppendInterval(i * m_animDelay);
                    sequence.Append(projectObject.transform.DOScale(Vector3.one, 0.5f));
                    sequence.Play();
                    activeTweens.Add(sequence);

                    m_ProjectInfosList.Add(projectObject);
                }
            }
            
        }
        
    }

    async void HideInfos(Tuio11Object tuioObject)
    {
        if (tuioObject.SessionId != currentTuioObject.SessionId) return;
        KillAllTweens();
        var sequence2 = DOTween.Sequence();
        sequence2.Join(m_GroupCaption.DOFade(0f, 1f));
        sequence2.Join(m_TriggerRing.DOFade(0f, 1f));
        var hideTaskList = new List<Task>();

        hideTaskList.Add(sequence2.AsyncWaitForCompletion());
        CustomTuio11Visualizer.instance.objectVisManagers.Remove(this);
        if (m_CurrentDisplayState == DisplayState.SHOWING_RING)
        {
            foreach (var projectObject in m_ProjectInfosList)
            {
                var sequence = DOTween.Sequence();
                sequence.Join(projectObject.transform.DOScale(Vector3.zero, 0.5f));
                hideTaskList.Add(sequence.AsyncWaitForCompletion());
                sequence.Play();
            }
        }
        else if (m_CurrentDisplayState == DisplayState.SHOWING_INFO)
        {
            hideTaskList.Add(m_ProjectInfosView.Hide().AsyncWaitForCompletion());
        }

        m_CurrentDisplayState = DisplayState.NOT_SHOWING;
        m_Destroying = true;

        await Task.WhenAll(hideTaskList);
        Destroy(gameObject);
    }
}