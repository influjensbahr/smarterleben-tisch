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
    public enum ArrangementMode
    {
        Circle,
        List
    }


    [SerializeField] ProjectInfoButton m_ProjectInfo;
    [SerializeField] ProjectInfosView m_ProjectInfosView;
    [SerializeField] CanvasGroup m_ProjectInfosViewCanvasGroup;
    [SerializeField] ProjectDataContainer m_ProjectDataContainer;
    [SerializeField] ObjectMapping[] m_ProjectObjectMappings;

    [Header("Display details")]
    [SerializeField] float m_Radius = 100f; // Radius of the circle around the Button Object where the ProjectInfos will be displayed
    [SerializeField] float m_animDelay = 0.2f;
    [SerializeField] float m_RotationSpeed = 10f; // Rotation speed of the ProjectInfos

    [SerializeField] TextMeshProUGUI m_GroupCaption = default;
    [SerializeField] Image m_TriggerRing = default;

    [SerializeField] ArrangementMode m_CurrentMode = ArrangementMode.Circle;
    public RectTransform canvasRectTransform;
    [SerializeField] float m_HorizontalSpacingList = 300f;
    [SerializeField] float m_VerticalSpacingList = 125f;
    [SerializeField] float m_ListPositionDeltaX = 150f;
    [SerializeField] float m_ListPositionDeltaY = -90f;
    [SerializeField] float m_AngleDelta = 1f;

    ProjectCategory m_Category;
    public ProjectCategory category => m_Category;

    Tuio11Object currentTuioObject = null;
    List<ProjectInfoButton> m_ProjectInfosList = new ();

    DisplayState m_CurrentDisplayState = DisplayState.NOT_SHOWING;
    float m_StateSwitchDelta = 0f;
    bool m_Destroying = false;

    private List<Tween> activeTweens = new();

    bool m_InfoBottomMode = true;

    enum DisplayState
    {
        NOT_SHOWING, SHOWING_RING, SHOWING_INFO
    }

    public void SetMode(ArrangementMode mode)
    {
        m_CurrentMode = mode;
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
    
    private void RecordTween(Tween tween)
    {
        tween.OnKill(() => activeTweens.Remove(tween));
        activeTweens.Add(tween);
    }

    private bool HasRunningTweens()
    {
        foreach (var tween in activeTweens)
        {
            if (tween.IsPlaying()) return true;
        }
        return false;
    }

    public async void ShowDetailInfo(ProjectInfoButton infoButton, KielRegionProjectDataObject info)
    {
        if (m_Destroying) return;
        if (m_StateSwitchDelta > 0f) return;
        while (HasRunningTweens())
            await Task.Delay(100);
        if (m_CurrentDisplayState == DisplayState.SHOWING_RING)
        {
            m_ProjectInfosView.SetProjectInfos(info);
            m_CurrentDisplayState = DisplayState.SHOWING_INFO;
            foreach (var projectButton in m_ProjectInfosList)
            {
                projectButton.GetComponent<CanvasGroup>().interactable = false;
                RecordTween(projectButton.GetComponent<CanvasGroup>().DOFade(0f, .75f));
                await Task.Delay(50);
            }

            await Task.Delay(350);
            m_ProjectInfosView.Show(m_ProjectInfosList.Count > 1);
            m_ProjectInfosViewCanvasGroup.interactable = true;
            m_StateSwitchDelta = .75f;
        }
    }

    public async void HideDetailInfo()
    {
        if (m_Destroying) return;
        if (m_StateSwitchDelta > 0f) return;
        if (m_ProjectInfosList.Count <= 0) return;

        while (HasRunningTweens())
            await Task.Delay(100);

        if (m_CurrentDisplayState == DisplayState.SHOWING_INFO)
        {
            m_CurrentDisplayState = DisplayState.SHOWING_RING;
            foreach (var projectButton in m_ProjectInfosList)
            {
                projectButton.GetComponent<CanvasGroup>().interactable = true;
                RecordTween(projectButton.GetComponent<CanvasGroup>().DOFade(1f, .75f));
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

    void Update()
    {
        if (m_Destroying) return;
        m_StateSwitchDelta -= Time.deltaTime;

        if (m_CurrentDisplayState == DisplayState.SHOWING_RING)
        {
            if (m_CurrentMode == ArrangementMode.Circle)
            {
                ArrangeInCircle();
            }
            else if (m_CurrentMode == ArrangementMode.List)
            {
                ArrangeInList();
            }
        } else if (m_CurrentDisplayState == DisplayState.SHOWING_INFO)
        {
            if (HasRunningTweens()) return;
            // Berechne die theoretischen Ecken
            RectTransform rectTransform = m_ProjectInfosViewCanvasGroup.transform as RectTransform;
            Vector3 topLeftCorner = new Vector3(rectTransform.parent.position.x + rectTransform.rect.width, rectTransform.parent.position.y - 80f, 0);
            Vector3 topRightCorner = new Vector3(rectTransform.parent.position.x, rectTransform.parent.position.y - 80f, 0);
            Vector3 bottomLeftCorner = new Vector3(rectTransform.parent.position.x + rectTransform.rect.width, rectTransform.parent.position.y - 80f - rectTransform.rect.height, 0);
            Vector3 bottomRightCorner = new Vector3(rectTransform.parent.position.x, rectTransform.parent.position.y - 80f - rectTransform.rect.height, 0);

            // Überprüfe, ob alle diese Ecken sichtbar sind
            if (IsPositionVisibleOnScreen(topLeftCorner) && IsPositionVisibleOnScreen(topRightCorner) && IsPositionVisibleOnScreen(bottomLeftCorner) && IsPositionVisibleOnScreen(bottomRightCorner))
            {
                if (!m_InfoBottomMode)
                {
                    // Setze den Pivot auf 1 und animiere zur Zielposition (0, -80)
                    m_ProjectInfosViewCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
                    {
                        rectTransform.pivot = new Vector2(rectTransform.pivot.x, 1);
                        m_ProjectInfosViewCanvasGroup.transform.DOLocalMoveY(-80, 1f);
                        m_ProjectInfosViewCanvasGroup.DOFade(1f, 0.5f);
                    });
                }
                m_InfoBottomMode = true;
            }
            else
            {
                if (m_InfoBottomMode)
                {
                    // Setze den Pivot auf 0 und animiere zur Zielposition (0, 80)
                    m_ProjectInfosViewCanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
                    {
                        rectTransform.pivot = new Vector2(rectTransform.pivot.x, 0);
                        m_ProjectInfosViewCanvasGroup.transform.DOLocalMoveY(80, 1f);
                        m_ProjectInfosViewCanvasGroup.DOFade(1f, 0.5f);
                    });
                }
                m_InfoBottomMode = false;
            }
        }
    }

    private void ArrangeInCircle()
    {
        var angleStep = (360f / m_ProjectInfosList.Count) * m_AngleDelta;
        var radius = m_Radius;
        var centerPosition = new Vector3(currentTuioObject.Position.X, currentTuioObject.Position.Y, 0);

        for (var i = 0; i < m_ProjectInfosList.Count; i++)
        {
            var angle = -i * angleStep;
            var x = Mathf.Cos(angle * Mathf.Deg2Rad - Time.time * m_RotationSpeed) * radius;
            var y = Mathf.Sin(angle * Mathf.Deg2Rad - Time.time * m_RotationSpeed) * radius;

            var targetPosition = centerPosition + new Vector3(x, y, 0);
            AnimateToPosition(m_ProjectInfosList[i].transform, targetPosition);
        }
    }

    private void ArrangeInList()
    {
        // determine mode
        int mode = IsPositionVisibleOnScreen(m_ProjectInfosList[0].transform.parent.TransformPoint(GetTargetPosition(1, m_ProjectInfosList.Count - 1))) ? 1 : 2;

        for (var i = 0; i < m_ProjectInfosList.Count; i++)
            AnimateToPosition(m_ProjectInfosList[i].transform, GetTargetPosition(mode, i));
    }

    private Vector3 GetTargetPosition(int mode, int index)
    {
        Vector3 startPosition;
        Vector3 testPosition = Vector3.zero;
        int xIndex = index % 2;
        int yIndex = Mathf.FloorToInt((float)index / 2f);

        switch (mode)
        {
            case 1: // list downwards
                startPosition = new Vector3(currentTuioObject.Position.X + m_ListPositionDeltaX, currentTuioObject.Position.Y + m_ListPositionDeltaY, 0);
                testPosition = startPosition + Vector3.right * (xIndex * m_HorizontalSpacingList) + Vector3.down * (yIndex * m_VerticalSpacingList);
                break;
            case 2: // list upwards
                startPosition = new Vector3(currentTuioObject.Position.X + m_ListPositionDeltaX, currentTuioObject.Position.Y - m_ListPositionDeltaY + 40f, 0);
                testPosition = startPosition + Vector3.right * (xIndex * m_HorizontalSpacingList) + Vector3.up * (yIndex * m_VerticalSpacingList);
                break;
        }
        return testPosition;
    }

    private bool IsPositionVisibleOnScreen(Vector3 position)
    {
        Vector3[] canvasCorners = new Vector3[4];
        canvasRectTransform.GetWorldCorners(canvasCorners);

        if (position.x >= canvasCorners[0].x && position.x <= canvasCorners[2].x &&
            position.y >= canvasCorners[0].y && position.y <= canvasCorners[2].y)
        {
            return true;
        }

        return false;
    }

    private void AnimateToPosition(Transform objTransform, Vector3 targetPosition)
    {
        objTransform.DOLocalMove(targetPosition, 1f); // 1 Sekunde animierte Bewegung zur Zielposition
    }
    public void SwitchMode(ArrangementMode newMode)
    {
        m_CurrentMode = newMode;
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
                var angleStep = (360f / projects.Count) * m_AngleDelta;
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
                    sequence.Append(projectObject.transform.DOScale(Vector3.one * 0.8f, 0.5f));
                    sequence.Play();
                    RecordTween(sequence);

                    m_ProjectInfosList.Add(projectObject);
                }
            }
            
        }
        
    }

    async void HideInfos(Tuio11Object tuioObject)
    {
        if (tuioObject.SessionId != currentTuioObject.SessionId) return;

        while (HasRunningTweens())
            await Task.Delay(100);

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