using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a clickable project info entry that notifies its manager to show details.
/// </summary>
public class ProjectInfoButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_ProjectNameText;
    ObjectVisualisationManager m_ParentVisualizaionManager = default;
    KielRegionProjectData m_ProjectInfo = default;

    public void OnClick()
    {
        if (m_ParentVisualizaionManager == null || m_ProjectInfo == null)
        {
            Debug.LogWarning("ProjectInfoButton not initialized properly.", this);
            return;
        }
        m_ParentVisualizaionManager.ShowDetailInfo(this, m_ProjectInfo);
    }

    public void SetProjectInfos(string projectName, ObjectVisualisationManager manager, KielRegionProjectData projectInfo)
    {
        m_ProjectNameText.text = projectName;
        m_ParentVisualizaionManager = manager;
        m_ProjectInfo = projectInfo;
    }
}
