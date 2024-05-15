using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Eventmanager))]
public class EventManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Eventmanager eventManager = (Eventmanager)target;
        if (GUILayout.Button("Test EventManager"))
        {
            eventManager.TestEventManager();
        }
    }
}