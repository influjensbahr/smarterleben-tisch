//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckExcessivePolygonCounts : VerifyCheckBase
    {
        public override string description => "Excessive polygon counts";
        public override string longDescription => "Checks for extremely high polygon counts which might cause performance or memory issues.";
        [SerializeField] int m_DefaultMaxPolygonCount = 10000; // Set a default value


        public override bool DrawSpecificProfileEditor()
        {
            int count= m_DefaultMaxPolygonCount;
            m_DefaultMaxPolygonCount = EditorGUILayout.IntField("Maximum Count", m_DefaultMaxPolygonCount);
            return count != m_DefaultMaxPolygonCount;
        }

        public override void PerformCheck(GameObject gameObject)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;
            PerformCheck(meshFilter.sharedMesh);
        }

        public override void PerformCheck(Mesh sobj) {
            int polygonCount = CountPolygons(sobj);

            if (polygonCount > m_DefaultMaxPolygonCount)
            {
                AddFailedCheck($"Mesh '{sobj.name}' has an excessive polygon count of {polygonCount}", sobj)
                    .WithSeverity(VerifyResult.Severity.Warning);
            }
        }

        private int CountPolygons(Mesh mesh)
        {
            return mesh.triangles.Length / 3;
        }
    }
}
#endif