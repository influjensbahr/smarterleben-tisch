using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InteractionTest : MonoBehaviour
{
    [SerializeField] private Image _imageInfoPanel;
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
    
    private void ShowInfos()
    {
        Debug.Log("InteractionTest: Object added");
        _imageInfoPanel.gameObject.SetActive(true);
    }
    
    private void HideInfos()
    {
        Debug.Log("InteractionTest: Object removed");
        _imageInfoPanel.gameObject.SetActive(false);
    }
}