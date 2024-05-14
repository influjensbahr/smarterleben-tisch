using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIContoller : MonoBehaviour
{
    [SerializeField] Image m_OrderValueBar;
    [SerializeField] Image m_ShipHealthBar;
    [SerializeField] Image m_CrewHealthBar;
    [SerializeField] Image m_BiodomeIntegrityBar;
    
    
    void SetOrderValueBar(float value)
    {
        if(m_OrderValueBar != null)
            m_OrderValueBar.fillAmount = value;
    }
    
    void SetShipHealthBar(float value)
    {
        if(m_ShipHealthBar != null)
            m_ShipHealthBar.fillAmount = value;
    }
    
    void SetCrewHealthBar(float value)
    {
        if(m_CrewHealthBar != null)
            m_CrewHealthBar.fillAmount = value;
    }
    
    void SetBiodomeIntegrityBar(float value)
    {
        if(m_BiodomeIntegrityBar != null)
            m_BiodomeIntegrityBar.fillAmount = value;
    }
}
