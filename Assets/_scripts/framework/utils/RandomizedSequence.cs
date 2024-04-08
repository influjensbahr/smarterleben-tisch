//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using OTBT.Framework.Utils.Editor;
#endif
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Provides functionality for cycling through a number of options in sequencial or random ways (i.e. random playback of barks or sound alternatives)
    /// </summary>
    public class RandomizedSequence 
    {
        enum Playmode
        {
            Sequence, Random, RandomNoImmediateRepeat, RandomEvenlyDistributed
        }

        [SerializeField] Playmode m_Playmode = Playmode.Sequence;
        [SerializeField] bool m_Repeat = true;
        int m_LastClip = -1;
        List<int> m_PlayCounts = new List<int>();
        int m_NumOptions = 0;
 
        public void UpdateCount(int count)
        {
            m_NumOptions = count;
            while (m_PlayCounts.Count < m_NumOptions)
                m_PlayCounts.Add(0);
        }

        public RandomizedSequence(int options)
        {
            UpdateCount(options);
        }

        internal void IncreasePlayCount(int clipnum)
        {
            m_PlayCounts[m_LastClip]++;
        }

        internal int GetNextIndex()
        {
            if (m_NumOptions == 0) return -1;

            switch (m_Playmode)
            {
                case Playmode.Sequence:
                    if (m_Repeat)
                    {
                        m_LastClip = m_LastClip < 0 ? 0 : ((m_LastClip + 1) % m_NumOptions);
                    } else
                    {
                        m_LastClip = m_LastClip < 0 ? 0 : Mathf.Min((m_LastClip + 1),  m_NumOptions -1);
                    }
                    
                    break;
                case Playmode.Random:
                    m_LastClip = (int)UnityEngine.Random.Range(0, m_NumOptions);
                    break;
                case Playmode.RandomNoImmediateRepeat:
                    if (m_NumOptions == 2)
                    {
                        m_LastClip = m_LastClip < 0 ? 0 : ((m_LastClip + 1) % m_NumOptions);
                    }
                    else
                    {
                        int nextClip = m_LastClip;
                        while (nextClip == m_LastClip)
                        {
                            nextClip = (int)UnityEngine.Random.Range(0, m_NumOptions);
                        }
                        m_LastClip = nextClip;
                    }
                    break;
                case Playmode.RandomEvenlyDistributed:
                    List<int> indexes = new List<int>();
                    int curPlayCount = int.MaxValue;
                    for (int i = 0; i < m_NumOptions; i++)
                    {
                        if (m_PlayCounts[i] > curPlayCount) continue;
                        if (m_PlayCounts[i] < curPlayCount) indexes.Clear();
                        indexes.Add(i);
                        curPlayCount = m_PlayCounts[i];
                    }
                    if (indexes.Count == 1)
                    {
                        m_LastClip = indexes[0];
                    }
                    else
                    {
                        int nextClip = m_LastClip;
                        while (nextClip == m_LastClip)
                        {
                            nextClip = indexes[(int)UnityEngine.Random.Range(0, indexes.Count)];
                        }
                        m_LastClip = nextClip;
                    }
                    break;
            }
            m_PlayCounts[m_LastClip]++;
            return m_LastClip;
        }

#if UNITY_EDITOR
        public void ShowGUI(Object obj)
        {
            switch(m_Playmode)
            {
                case Playmode.Random:
                    EditorGUILayout.HelpBox("Completely random order of playback", MessageType.Info);
                    break;
                case Playmode.RandomEvenlyDistributed:
                    EditorGUILayout.HelpBox("Random playback, but all options are played once before repeating", MessageType.Info);
                    break;
                case Playmode.RandomNoImmediateRepeat:
                    EditorGUILayout.HelpBox("Random order, but never the same option twice in a row", MessageType.Info);
                    break;
                case Playmode.Sequence:
                    EditorGUILayout.HelpBox("Played in sequence: 0, 1, 2, 3, 4..." + (m_Repeat ? " in endless cycle" : " and last option is repeated infinitely"), MessageType.Info);
                    break;
            }
            EditorUtils.Space(5);
            Playmode pm = (Playmode)EditorGUILayout.EnumPopup(m_Playmode);
            if(pm != m_Playmode)
            {
                m_Playmode = pm;
                EditorUtility.SetDirty(obj);
            }
            if (pm == Playmode.Sequence)
            {
                bool repeat = EditorGUILayout.Toggle("Repeat", m_Repeat);
                if (repeat != m_Repeat)
                {
                    m_Repeat = repeat;
                    EditorUtility.SetDirty(obj);
                }
            }
            EditorUtils.Space(5);
        }
#endif
    }
}
