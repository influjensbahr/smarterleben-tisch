//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Core;
using Sparrow.Verification;
using System.Collections.Generic;
#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif
using UnityEngine;

namespace OTBT.Framework.AI
{
    /// <summary>
    /// A list of simple fact checks that can be tested if ALL these fact checks match or any of them match (OR/AND)
    /// </summary>
    [CreateAssetMenu(menuName = "OTBT/AI/Fact Check List", fileName = "New Fact check list")]
    public class FuzzyFactCheck : ScriptableObject, IVerify
    {
        enum ConditionOperator {  OR, AND }
        [SerializeField] ConditionOperator m_OrAnd = ConditionOperator.AND;
        [SerializeField] List<FuzzyOption> m_FuzzyOptions = new List<FuzzyOption>();

        public List<FuzzyOption> options => m_FuzzyOptions;
        public int score => m_FuzzyOptions.Count;
        public bool isMatch => KnowledgeManager.instance.MatchOptionList(m_FuzzyOptions, m_OrAnd == ConditionOperator.OR);


        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_FuzzyOptions.Count != 0, "Empty FuzzyFactCheck found!", this);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FuzzyFactCheck))]
    public class FuzzyFactCheckEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            OTBT.Framework.Utils.Editor.EditorUtils.DrawLogoHeader("Factcheck", "https://wiki.beatentrack.games/doc/ai-ZU8JMerUaX#h-knowledge-system-and-dialogue");

            base.OnInspectorGUI();
        }
    }
#endif
}
