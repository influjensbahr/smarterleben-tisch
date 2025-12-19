using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple left-right oscillation animation for an object.
/// </summary>
public class ArrowLeftRightMovement : MonoBehaviour
{
    [SerializeField] [Tooltip("Amplitude of the horizontal movement in units.")] float m_MoveScale = 10f;
    [SerializeField] [Tooltip("Phase offset in seconds.")] float m_Offset = 0.4f;
    [SerializeField] [Tooltip("Speed multiplier for the oscillation.")] float m_Timescale = 1f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + Vector3.right * (m_MoveScale * Mathf.Sin(Mathf.Max(0f, m_Timescale) * (Time.time - m_Offset)));
    }
}
