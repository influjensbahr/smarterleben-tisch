using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global application settings for the KielRegion prototype.
/// </summary>
public class KielRegionManager : MonoBehaviour
{
    void Start()
    {
        // Keep network/timers running even when app is not focused
        Application.runInBackground = true;
    }
}
