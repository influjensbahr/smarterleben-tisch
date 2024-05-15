using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

[CustomEditor(typeof(SpaceshipEvent))]
public class SpaceshipEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SpaceshipEvent evt = (SpaceshipEvent) target;

        string text = EditorGUILayout.TextArea("");
        if(!text.Equals(""))
        {
            var fragments = text.Split("\t");
            evt.m_Title = fragments[1];
            evt.m_Text = fragments[2];

            evt.m_NatureYes = float.Parse(fragments[9]);
            evt.m_TechnologyYes = float.Parse(fragments[10]);
            evt.m_PeopleYes = float.Parse(fragments[11]);
            evt.m_OrderYes = float.Parse(fragments[12]);
            evt.m_NatureNo = float.Parse(fragments[13]);
            evt.m_TechnologyNo = float.Parse(fragments[14]);
            evt.m_PeopleNo = float.Parse(fragments[15]);
            evt.m_OrderNo = float.Parse(fragments[16]);
            EditorUtility.SetDirty(evt);
            string assetPath = AssetDatabase.GetAssetPath(evt.GetInstanceID());
            var newName = fragments[0] + "-" + fragments[1];
            newName = newName.Replace("?", "");
            AssetDatabase.RenameAsset(assetPath, newName);
            Debug.Log("Asset Path: " + assetPath + " //" + newName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Neues Asset erstellen
            var newEvent = ScriptableObject.CreateInstance<SpaceshipEvent>();
            AssetDatabase.CreateAsset(newEvent, "Assets/ZZZ.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
     }
    }
}
