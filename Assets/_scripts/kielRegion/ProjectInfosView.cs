using TMPro;
using UnityEngine;

public class ProjectInfosView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_ProjectNameText, m_ProjectDescriptionText;
    string m_ProjectName, m_ProjectDescription;
    
    public void SetProjectInfos(string projectName, string projectDescription)
    {
        m_ProjectName = projectName;
        m_ProjectDescription = projectDescription;
        m_ProjectNameText.text = m_ProjectName;
        m_ProjectDescriptionText.text = m_ProjectDescription;
    }
}
