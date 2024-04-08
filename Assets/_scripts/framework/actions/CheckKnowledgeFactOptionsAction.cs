
//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//
#if OTBT_AC

using UnityEngine;
using System.Collections.Generic;
using OTBT.Framework.AI;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC { 

	[System.Serializable]
	public class CheckKnowledgeFactOptionsAction : Action {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "AI/List of factchecks";
        public override string Description => "Checks a number of factchecks and executes the most specific option";
        public override int NumSockets => m_FuzzyChecks.Count + 1;

        [SerializeField] List<FuzzyFactCheck> m_FuzzyChecks = new List<FuzzyFactCheck>();

		public override int GetNextOutputIndex()
        {
			List<FuzzyFactCheck> matches = new List<FuzzyFactCheck>();
			int bestMatch = 0;
			foreach (FuzzyFactCheck check in m_FuzzyChecks)
			{
				if(check.isMatch)
				{
					if(bestMatch < check.score)
					{
						bestMatch = check.score;
						matches.Clear();
					}
					matches.Add(check);
				}
			}
			return matches.Count == 0 ? NumSockets - 1 : m_FuzzyChecks.IndexOf(matches[Random.Range(0, matches.Count)]);
        }
		
		#if UNITY_EDITOR

		override public void ShowGUI () {
            EditorGUILayout.HelpBox("There is one exit per list item, and the default exit as LAST which is taken when no option matches", MessageType.Info);

			int removeAt = -1;
			for(int i = 0; i < m_FuzzyChecks.Count; i++)
			{
				m_FuzzyChecks[i] = (FuzzyFactCheck)EditorGUILayout.ObjectField("Object: ", m_FuzzyChecks[i], typeof(FuzzyFactCheck), false);
				if (m_FuzzyChecks[i] != null)
				{
					for (int j = 0; j < m_FuzzyChecks[i].score; j++)
					{
						EditorGUILayout.LabelField(m_FuzzyChecks[i].options[j].ToString());
					}
				}
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                {
					removeAt = i;
                }
                EditorGUILayout.LabelField("This option has exit #" + i + (m_FuzzyChecks[i] == null ? "" : " and score " + m_FuzzyChecks[i].score));
				EditorGUILayout.EndHorizontal();
				EditorUtils.GuiLine();
			}
			if(removeAt >= 0)
			{
				m_FuzzyChecks.RemoveAt(removeAt);
			}

			if (GUILayout.Button("+"))
			{
				m_FuzzyChecks.Add(null);
			}
			
            EditorUtils.GuiLine(3);
        }

		public override string SetLabel () {
			return "Factcheck matching";
		}

		#endif
		
	}

}
#endif