using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TuioNet.Common;
using TuioNet.Tuio11;
using UnityEngine;
using UnityEngine.UI;

public class InteractionTest : MonoBehaviour
{
    [SerializeField] private Image _imageInfoPanel;
    [SerializeField] private float _offset = 100;
    
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
        _imageInfoPanel.gameObject.SetActive(true);
        _imageInfoPanel.DOFade(1, 0.5f);
    }
    
    private void HideInfos(Tuio11Object tuioObject)
    {
        _imageInfoPanel.DOFade(0, 0.5f);
        _imageInfoPanel.gameObject.SetActive(false);
    }
}