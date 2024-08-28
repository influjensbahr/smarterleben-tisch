using System;
using System.Collections.Generic;
using TMPro;
using TuioNet.Tuio11;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

[Serializable]
public struct ObjectMapping
{
    public uint id;
    public ProjectCategory category;
}

public class ObjectVisualisationManager : MonoBehaviour
{
    [SerializeField] ProjectInfosView m_ProjectInfo;
    [SerializeField] ProjectDataContainer m_ProjectDataContainer;
    [SerializeField] ObjectMapping[] m_ProjectObjectMappings;
    [SerializeField] float m_Radius = 100f;
    [SerializeField] float m_animDelay = 0.2f;
    
    Tuio11Object currentTuioObject = null;
    List<ProjectInfosView> m_ProjectInfosList = new List<ProjectInfosView>();

    private void OnEnable()
    {
        CustomTuio11Visualizer.onObjectAdd += ShowInfos;
        CustomTuio11Visualizer.onObjectRemove += HideInfos;
    }

    private void OnDisable()
    {
        CustomTuio11Visualizer.onObjectAdd -= ShowInfos;
        CustomTuio11Visualizer.onObjectRemove -= HideInfos;
    }

    private void ShowInfos(Tuio11Object tuioObject)
    {
        if (currentTuioObject != null)
        {
            return;
        }
        
        currentTuioObject = tuioObject;
        
        foreach (var objectMapping in m_ProjectObjectMappings)
        {
            if (objectMapping.id != tuioObject.SymbolId) continue;
            
            var projects = m_ProjectDataContainer.GetProjectsByCategory(objectMapping.category);
            if (projects.Count <= 0) continue;
            
            float angleStep = 360f / projects.Count;
            float radius = m_Radius;
            Vector3 centerPosition = new Vector3(tuioObject.Position.X, tuioObject.Position.Y, 0);
            
            for (int i = 0; i < projects.Count; i++)
            {
                var project = projects[i];
                var angle = -i * angleStep;
                var x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                var y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                
                var projectObject = Instantiate(m_ProjectInfo, transform);
                projectObject.transform.localPosition = centerPosition + new Vector3(x, y, 0);
                projectObject.SetProjectInfos(project.title, project.shortDescription);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(projectObject.transform.DOScale(Vector3.zero, 0f));
                sequence.AppendInterval(i * m_animDelay);
                sequence.Append(projectObject.transform.DOScale(Vector3.one, 0.5f));
                //sequence.AppendCallback(() => AddFloatingEffect(projectObject.transform));
                sequence.Play();
            }
        }
        
    }

    private void Update()
    {
        if(currentTuioObject == null) return;
        foreach (var project in m_ProjectInfosList)
        {
            project.transform.rotation = Quaternion.identity;
        }
    }

    private void AddFloatingEffect(Transform projectTransform)
    {
        float floatAmount = 5f;
        float floatDuration = 2f;

        Sequence floatSequence = DOTween.Sequence();
        floatSequence.Append(projectTransform.DOMoveY(projectTransform.position.y + floatAmount, floatDuration).SetEase(Ease.InOutSine));
        floatSequence.Append(projectTransform.DOMoveY(projectTransform.position.y - floatAmount, floatDuration).SetEase(Ease.InOutSine));
        floatSequence.SetLoops(-1, LoopType.Yoyo);
        floatSequence.Play();
    }

    private void HideInfos(Tuio11Object tuioObject)
    {
        if (currentTuioObject == tuioObject)
        {
            currentTuioObject = null;
        }
    }
}