using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeSpaceship : MonoBehaviour
{
    public float speed = 0.5f;
    public float range = 10f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float offsetX = Mathf.Sin(Time.time * speed) * range;
        float offsetY = Mathf.Cos(Time.time * speed) * range / 2;
        rectTransform.anchoredPosition = startPosition + new Vector2(offsetX, offsetY);
    }
}
