using System.Collections.Generic;
using OTBT.Framework.UI;
using UnityEngine;

public class CreateLineRenderer : MonoBehaviour
{
    [SerializeField] UILineRenderer uiLineRenderer;
    [SerializeField] ObjectVisualisationManager objectVisualisationManager;
    List<ProjectInfosView> projectInfoClones;

    void Start()
    {
        objectVisualisationManager = FindObjectOfType<ObjectVisualisationManager>();
        if (objectVisualisationManager != null)
        {
            projectInfoClones = objectVisualisationManager.ProjectInfosList;
            uiLineRenderer.points = new List<Vector2>(new Vector2[projectInfoClones.Count * 2]);
        }
        else
        {
            Debug.LogError("ObjectVisualisationManager not found");
        }
    }

    void Update()
    {
        if (projectInfoClones == null) return;

        for (int i = 0; i < projectInfoClones.Count; i++)
        {
            uiLineRenderer.points[i * 2] = RectTransformUtility.WorldToScreenPoint(null, transform.position);
            uiLineRenderer.points[i * 2 + 1] =
                RectTransformUtility.WorldToScreenPoint(null, projectInfoClones[i].transform.position);
        }

        uiLineRenderer.SetAllDirty();
    }

}