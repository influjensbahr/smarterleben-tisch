using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectInfoButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_ProjectNameText;
    ObjectVisualisationManager m_ParentVisualizaionManager = default;
    KielRegionProjectDataObject m_ProjectInfo = default;

    public void OnClick()
    {
        m_ParentVisualizaionManager.ShowDetailInfo(this, m_ProjectInfo);
    }

    public void SetProjectInfos(string projectName, ObjectVisualisationManager manager, KielRegionProjectDataObject projectInfo)
    {
        m_ProjectNameText.text = projectName;
        m_ParentVisualizaionManager = manager;
        m_ProjectInfo = projectInfo;
    }
}
