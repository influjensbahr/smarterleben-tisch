using System;
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
        _imageInfoPanel.DOFade(1, 0.5f);
    }
    
    private void HideInfos()
    {
        Debug.Log("InteractionTest: Object removed");
        _imageInfoPanel.DOFade(0, 0.5f);
        _imageInfoPanel.gameObject.SetActive(false);
    }
}