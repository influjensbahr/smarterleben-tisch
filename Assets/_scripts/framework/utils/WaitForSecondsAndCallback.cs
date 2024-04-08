using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WaitForSecondsAndCallback : MonoBehaviour
{
    [SerializeField] Image m_WaitIndicator = default;
    float m_TimeDuration = 0f;
    UnityAction m_Callback;
    float m_TimeDone = 0f;
    bool m_Showing = false;
    
    public void Prepare(float time, UnityAction callback)
    {
        m_TimeDone = 0f;
        m_TimeDuration = time;
        m_Showing = true;
        m_WaitIndicator.fillAmount = 0f;
        m_Callback = callback;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_Showing) return;
        m_TimeDone += Time.deltaTime;
        m_WaitIndicator.fillAmount = m_TimeDone / m_TimeDuration;
        if(m_TimeDone >= m_TimeDuration)
        {
            m_Showing = false;
            m_Callback?.Invoke();
        }
    }
}
