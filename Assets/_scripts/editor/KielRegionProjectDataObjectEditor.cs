using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(KielRegionProjectDataObject))]
public class KielRegionProjectDataObjectEditor : Editor
{
    private bool m_isPicking;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        KielRegionProjectDataObject dataObject = (KielRegionProjectDataObject)target;

        if (GUILayout.Button("Pick Location"))
        {
            m_isPicking = true;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        if (m_isPicking)
        {
            EditorGUILayout.HelpBox("Click on the scene to pick a location.", MessageType.Info);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Debug.Log("MouseDown detected");
        }
    }
}
