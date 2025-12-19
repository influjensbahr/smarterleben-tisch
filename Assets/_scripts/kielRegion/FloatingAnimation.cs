using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple vertical floating animation using a sine wave.
/// </summary>
public class FloatingAnimation : MonoBehaviour
{
    [SerializeField] float amplitude = 0.5f;
    [SerializeField] float frequency = 1f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = startPosition + new Vector3(0, yOffset, 0);
    }
}
