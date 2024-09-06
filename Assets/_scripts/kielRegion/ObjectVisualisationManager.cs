using System;
using System.Collections.Generic;
using TuioNet.Tuio11;
using UnityEngine;
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
    [SerializeField] float m_Radius = 100f; // Radius of the circle around the Button Object where the ProjectInfos will be displayed
    [SerializeField] float m_animDelay = 0.2f;
    [SerializeField] float m_RotationSpeed = 10f; // Rotation speed of the ProjectInfos
    
    Tuio11Object currentTuioObject = null;
    List<ProjectInfosView> m_ProjectInfosList = new ();
    
    public List<ProjectInfosView> ProjectInfosList => m_ProjectInfosList;

    void OnEnable()
    {
        CustomTuio11Visualizer.onObjectAdd += ShowInfos;
        CustomTuio11Visualizer.onObjectRemove += HideInfos;
    }

    void OnDisable()
    {
        CustomTuio11Visualizer.onObjectAdd -= ShowInfos;
        CustomTuio11Visualizer.onObjectRemove -= HideInfos;
    }

    void ShowInfos(Tuio11Object tuioObject)
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
                projectObject.SetProjectInfos(project.title, project.shortDescription);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(projectObject.transform.DOScale(Vector3.zero, 0f));
                sequence.AppendInterval(i * m_animDelay);
                sequence.Append(projectObject.transform.DOScale(Vector3.one, 0.5f));
                sequence.Play();
                
                m_ProjectInfosList.Add(projectObject);
            }
        }
        
    }

    void HideInfos(Tuio11Object tuioObject)
    {
        if (currentTuioObject == tuioObject)
        {
            currentTuioObject = null;
        }
    }
}