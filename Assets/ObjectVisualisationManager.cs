using TMPro;
using TuioNet.Tuio11;
using UnityEngine;
using UnityEngine.UI;

public class ObjectVisualisationManager : MonoBehaviour
{
    [SerializeField] private KielRegionProjectDataObject[] m_ProjectDataObjects;
    [SerializeField] private GameObject m_ProjectInfos;
    [SerializeField] private TextMeshProUGUI m_Title;
    [SerializeField] private Image m_ProjectImage;
    [SerializeField] private TextMeshProUGUI m_ShortDescription;
    [SerializeField] private TextMeshProUGUI m_AdditionalInfo;
    
    private Tuio11Object currentTuioObject = null;

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

        foreach (var projectDataObject in m_ProjectDataObjects)
        {
            if (projectDataObject.id == tuioObject.SymbolId)
            {
                currentTuioObject = tuioObject;
                m_ProjectInfos.SetActive(true);
                SetData(projectDataObject);
                break;
            }
        }
    }

    private void HideInfos(Tuio11Object tuioObject)
    { 
        if (currentTuioObject == tuioObject)
        {
            m_ProjectInfos.SetActive(false);
            currentTuioObject = null;
        }
    }

    void SetData(KielRegionProjectDataObject dataObject)
    {
        if (m_Title) m_Title.text = dataObject.title;
        if (m_ShortDescription) m_ShortDescription.text = dataObject.shortDescription;
        if (m_ProjectImage) m_ProjectImage.sprite = dataObject.projectImage;
        if (m_AdditionalInfo) m_AdditionalInfo.text = dataObject.additionalInfo;
    }
}