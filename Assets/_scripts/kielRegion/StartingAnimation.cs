using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StartingAnimation : MonoBehaviour
{
    [SerializeField] private CanvasRenderer[] m_Sprites;
    [SerializeField] private float m_AnimSpeed = 1f;
    [SerializeField] private float m_Delay = 0.2f;

    private void Start()
    {
        StartAnimation();
    }

    private void StartAnimation()
    {
        for (int i = 0; i < m_Sprites.Length; i++)
        {
            var sprite = m_Sprites[i];
            Vector3 startPos = new Vector3(-Screen.width / 2, sprite.transform.position.y, sprite.transform.position.z);
            Vector3 endPos = new Vector3(Screen.width * 2, sprite.transform.position.y, sprite.transform.position.z);

            sprite.transform.position = startPos;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(i * m_Delay);
            sequence.Append(sprite.transform.DOMove(endPos, m_AnimSpeed).SetEase(Ease.InOutSine));
            sequence.Play();
        }
    }
}
