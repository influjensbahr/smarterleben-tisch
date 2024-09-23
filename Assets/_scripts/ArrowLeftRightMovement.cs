using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowLeftRightMovement : MonoBehaviour
{
    [SerializeField] float m_MoveScale = 10f;
    [SerializeField] float m_Offset = 0.4f;
    [SerializeField] float m_Timescale = 1f;

    Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;    
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPos + Vector3.right * (m_MoveScale * Mathf.Sin(m_Timescale * (Time.time - m_Offset)));
    }
}
