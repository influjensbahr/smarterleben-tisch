using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncBottomAnchorWithYCoordinate : MonoBehaviour
{
    [SerializeField] RectTransform m_BottomAnchorTransform = default;
    [SerializeField] RectTransform m_YCoordinateTransform = default;

    void Update()
    {
        if (m_BottomAnchorTransform == null || m_YCoordinateTransform == null) return;
        Vector2 anchor = m_BottomAnchorTransform.offsetMin;
        anchor.y = m_YCoordinateTransform.anchoredPosition.y;
        m_BottomAnchorTransform.offsetMin = anchor;
    }
}
