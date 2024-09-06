using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TuioNet.Tuio11;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkerVisualSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] m_LandmarkerPrefab;
    [SerializeField] float m_AnimationDuration = 0.5f;
    [SerializeField] float m_AnimationDelay = 0.2f;

    private void OnEnable()
    {
        CustomTuio11Visualizer.onCursorAdd += CreateLandmarker;
    }
    
    private void OnDisable()
    {
        CustomTuio11Visualizer.onCursorAdd -= CreateLandmarker;
    }

    public void CreateLandmarker(Tuio11Cursor tuioCursor)
    {
        Vector2 touchPosition = new Vector2(tuioCursor.Position.X * Screen.width, Screen.height - tuioCursor.Position.Y * Screen.height);

        if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), touchPosition))
        {
            Debug.Log("CreateLandmarker");
            for (int i = 0; i < m_LandmarkerPrefab.Length; i++)
            {
                var landmarker = m_LandmarkerPrefab[i];
                landmarker.SetActive(true);
                landmarker.transform.localScale = Vector3.zero;

                float delay = i * m_AnimationDelay;
                landmarker.transform.DOScale(Vector3.one, m_AnimationDuration).SetDelay(delay).SetEase(Ease.OutBack);
            }
        }
    }
}
