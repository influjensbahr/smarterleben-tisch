using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour
{
    [SerializeField] string m_URL = "";

    public void OnClickOpenURL()
    {
        Application.OpenURL(m_URL);
    }
}
