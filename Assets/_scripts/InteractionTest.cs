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
    [SerializeField] private KielRegionProjectDataObject m_projectDataObject;
    
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
        
    }
    
    private void HideInfos(Tuio11Object tuioObject)
    {
        
    }
}