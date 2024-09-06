using System;
using OSC.NET;
using TMPro;
using TuioNet.Common;
using TuioNet.Tuio11;
using UnityEngine;

public class TUIOConnectionChecker : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI connectionStatusText;
    [SerializeField] string m_ipAddress = "127.0.0.1";
    TuioClient m_tuioClient;
    float m_timeSinceLastEvent = 0f;
    float m_timoutDuration = 5f;

    void Start()
    {
        m_tuioClient = new TuioClient(TuioConnectionType.UDP, m_ipAddress);
        m_tuioClient.Connect();
        connectionStatusText.text = "Checking...";
    }
    
    void Update()
    {
        if (m_tuioClient.IsConnected)
        {
            connectionStatusText.text = "Connected";
            m_timeSinceLastEvent = 0f;
        }
        else
        {
            m_timeSinceLastEvent += Time.deltaTime;
            if (m_timeSinceLastEvent > m_timoutDuration)
            {
                connectionStatusText.text = "Disconnected";
            }
        }
    }

    void OnDestroy()
    {
        m_tuioClient.Disconnect();
    }
}
