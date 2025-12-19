using System;
using OSC.NET;
using TMPro;
using TuioNet.Common;
using TuioNet.Tuio11;
using UnityEngine;

/// <summary>
/// Simple status checker to display whether a TUIO client is connected.
/// </summary>
public class TUIOConnectionChecker : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI connectionStatusText;
    [SerializeField] string m_ipAddress = "127.0.0.1";
    TuioClient m_tuioClient;
    float m_timeSinceLastEvent = 0f;
    float m_timoutDuration = 5f;

    void Start()
    {
        try
        {
            m_tuioClient = new TuioClient(TuioConnectionType.UDP, m_ipAddress);
            m_tuioClient.Connect();
            if (connectionStatusText != null) connectionStatusText.text = "Checking...";
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to connect TUIO client: {ex.Message}");
            if (connectionStatusText != null) connectionStatusText.text = "Error";
        }
    }
    
    void Update()
    {
        if (m_tuioClient == null)
        {
            return;
        }

        if (m_tuioClient.IsConnected)
        {
            if (connectionStatusText != null) connectionStatusText.text = "Connected";
            m_timeSinceLastEvent = 0f;
        }
        else
        {
            m_timeSinceLastEvent += Time.deltaTime;
            if (m_timeSinceLastEvent > m_timoutDuration)
            {
                if (connectionStatusText != null) connectionStatusText.text = "Disconnected";
            }
        }
    }

    void OnDestroy()
    {
        try
        {
            m_tuioClient?.Disconnect();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error on disconnecting TUIO client: {ex.Message}");
        }
    }
}
