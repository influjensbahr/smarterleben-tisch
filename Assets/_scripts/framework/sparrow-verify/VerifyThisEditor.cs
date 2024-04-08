//
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sparrow.Verification
{
    [CustomEditor(typeof(VerifyThis))]
    public class VerifyThisEditor : Editor
    {
        private static Dictionary<int, List<VerifyResult>> cache = new Dictionary<int, List<VerifyResult>>();
        private static Dictionary<int, DateTime> lastUpdated = new Dictionary<int, DateTime>();
        private static readonly TimeSpan cacheValidityDuration = TimeSpan.FromSeconds(1f);

        public override void OnInspectorGUI()
        {
            var targ = target as VerifyThis;
            int hashCode = target.GetHashCode();
            DateTime currentTime = DateTime.UtcNow;
            if (cache.ContainsKey(hashCode) && lastUpdated.ContainsKey(hashCode) && currentTime - lastUpdated[hashCode] <= cacheValidityDuration)
            {
                // Use cached results
                RenderResults(cache[hashCode]);
                return;
            }
            var results = VerifyWindow.PerformSelectedChecks(new List<UnityEngine.Object>() { targ.gameObject }, VerifyResult.CheckType.All, "", false);
            cache[hashCode] = results;
            lastUpdated[hashCode] = currentTime;
            RenderResults(results);
        }

        public static void DrawVerifyEditor(GameObject go)
        {
            var results = VerifyWindow.PerformSelectedChecks(new List<UnityEngine.Object>() { go }, VerifyResult.CheckType.All, "", false);
            RenderResults(results, false);
        }

        private static void RenderResults(List<VerifyResult> results, bool drawHeader = true)
        {
            if (drawHeader)
            {
                if (results.Count == 0)
                {
                    GUILayout.Label("✔ Sparrow Verification found no problems");
                }
                else
                {
                    GUILayout.Label("✘ Found " + results.Count + " problems");
                    EditorGUILayout.Space(10);
                }
            }
            string lastCategory = "";
            for (int i = results.Count - 1; i >= 0; i--)
            {
                if (!results[i].category.Equals(lastCategory))
                {
                    lastCategory = results[i].category;
                    EditorUtils.VerifyLabel(lastCategory);
                    if(!results[i].tooltip.Equals("")) EditorUtils.VerifyLabelDescription(results[i].tooltip);
                }
                results[i].Print();
            }
        }
    }
}
#endif