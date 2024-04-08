//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Core;
using Sparrow.Verification;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OTBT.Framework.AI
{
    /// <summary>
    /// Single option for a dialogue system that can be matched in the fuzzy knowledge database
    /// </summary>
    [Serializable]
    public class FuzzyOption
    {
        public enum TokenComparison {
            EXISTS, SMALLER, SMALLEQUAL, EQUAL, LARGEEQUAL, LARGER, NOT_EXISTS
        }

        [SerializeField] StringOrAtomReference<KnowledgeAtom> m_TokenObject = new StringOrAtomReference<KnowledgeAtom>();
        [SerializeField] TokenComparison m_Comparable = TokenComparison.EXISTS;
        [SerializeField] int m_Comparitor = 0;

        public string token => m_TokenObject.ToString();

        public override string ToString()
        {
            return m_TokenObject.ToString() + " " + m_Comparable + (!(m_Comparable == TokenComparison.EXISTS || m_Comparable == TokenComparison.NOT_EXISTS) ? " " + m_Comparitor : "");
        }

        public bool MatchesValue(bool exists, int number)
        {
            if (!exists) return m_Comparable == TokenComparison.NOT_EXISTS;
            switch(m_Comparable)
            {
                case TokenComparison.EXISTS: return exists;
                case TokenComparison.SMALLER: return number < m_Comparitor;
                case TokenComparison.SMALLEQUAL: return number <= m_Comparitor;
                case TokenComparison.EQUAL: return number == m_Comparitor;
                case TokenComparison.LARGER: return number > m_Comparitor;
                case TokenComparison.LARGEEQUAL: return number >= m_Comparitor;
                default: return false;
            }
        }

#if UNITY_EDITOR
        public void ShowGUI(Object parent)
        {
            m_TokenObject.ShowGUI(parent);

            GUILayout.BeginHorizontal();
            m_Comparable = (TokenComparison)EditorGUILayout.EnumPopup(m_Comparable);
            if(m_Comparable != TokenComparison.NOT_EXISTS && m_Comparable != TokenComparison.EXISTS)
            {
                m_Comparitor = EditorGUILayout.IntField(m_Comparitor);
            }
            GUILayout.EndHorizontal();
        }
#endif
    }

    /// <summary>
    /// A knowledge database that can be used for dialogue options and saving of other player-behavior specific data
    /// Based on GDC Talk "AI-driven Dynamic Dialog through Fuzzy Pattern Matching" https://www.youtube.com/watch?v=tAbBID3N64A
    /// </summary>
    public class KnowledgeManager : Utils.Singleton<KnowledgeManager>, ISaveData, IVerify
    {
        Dictionary<string, int> m_Database = new Dictionary<string, int>();

        public Dictionary<string, int> data => m_Database;

        public bool MatchOptionList(List<FuzzyOption> options, bool or = false)
        {
            for(int i = 0; i < options.Count; i++)
            {
                if(or)
                {
                    if (MatchSingleOption(options[i]))
                        return true;
                } else
                {
                    if (!MatchSingleOption(options[i]))
                        return false;
                }
            }
            return or ? false : true;
        }

        protected override void InitializeInherit()
        {
            base.InitializeInherit();
        }

        public bool ExistsCheck(string identifier)
        {
            return m_Database.ContainsKey(identifier);
        }

        public bool MatchSingleOption(FuzzyOption option)
        {
            if (m_Database.TryGetValue(option.token, out int value))
            {
                return option.MatchesValue(true, value);
            }
            return option.MatchesValue(false, 0);
        }

        public void Increase(StringOrAtomReference<KnowledgeAtom> identifier, int increaseBy)
        {
            Increase(identifier.ToString(), increaseBy);
        }

        public void Increase(string identifier, int increaseBy)
        {
            if(m_Database.ContainsKey(identifier))
            {
                m_Database[identifier] += increaseBy;
            } else
            {
                Add(identifier, increaseBy);
            }
        }

        public void Decrease(StringOrAtomReference<KnowledgeAtom> identifier, int increaseBy)
        {
            Decrease(identifier, increaseBy);
        }

        public void Decrease(string identifier, int decreaseBy)
        {
            if (m_Database.ContainsKey(identifier))
            {
                m_Database[identifier] -= decreaseBy;
            }
            else
            {
                Add(identifier, -decreaseBy);
            }
        }

        public void Remove(StringOrAtomReference<KnowledgeAtom> identifier)
        {
            Remove(identifier.ToString());
        }

        public void Remove(string identifier)
        {
            m_Database.Remove(identifier);
        }

        public void Set(StringOrAtomReference<KnowledgeAtom> identifier, int number)
        {
            Set(identifier.ToString(), number);
        }

        public void Set(string identifier, int number)
        {
            if (m_Database.TryGetValue(identifier, out int valueOld))
            {
                m_Database[identifier] = number;
            }
        }

        public void Add(StringOrAtomReference<KnowledgeAtom> identifier, int number = 0)
        {
            Add(identifier.ToString(), number);
        }

        public void Add(string identifier, int number = 0)
        {
            if(m_Database.TryGetValue(identifier, out int valueOld))
            {
                m_Database[identifier] = number;
            } else
            {
                m_Database.Add(identifier, number);
            }
        }

        public void Load()
        {
            Dictionary<string, int> load = SaveGame.instance.saveData.GetObject("fuzzy_knowledge_manager", new Dictionary<string, int>());
            SetToDictionary(load);
        }

        public void Save()
        {
            SaveGame.instance.saveData.SetObject<Dictionary<string, int>>("fuzzy_knowledge_manager", m_Database);
        }

        public void PreSaveAction()
        {
            Save();
        }

        public void WhenLoadReady() {
            Load();
        }

        private void SetToDictionary(Dictionary<string, int> loadingDict)
        {
            m_Database.Clear();
            foreach (string k in loadingDict.Keys)
                m_Database.Add(k, loadingDict[k]);
        }

        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}
