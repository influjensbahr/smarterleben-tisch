using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBoundsToObject : MonoBehaviour
{
    public RectTransform targetRectTransform; // Das Ziel-RectTransform
    public RectTransform referenceRectTransform; // Das Referenz-RectTransform

    void Update()
    {
        if (targetRectTransform == null || referenceRectTransform == null)
            return;

        // Hole die untere Position des Referenz-RectTransforms
        float referenceBottom = referenceRectTransform.rect.yMin + referenceRectTransform.anchoredPosition.y;

        // Aktualisiere den Bottom-Abstand des Ziel-RectTransforms
        targetRectTransform.offsetMin = new Vector2(targetRectTransform.offsetMin.x, referenceBottom);
    }
}
