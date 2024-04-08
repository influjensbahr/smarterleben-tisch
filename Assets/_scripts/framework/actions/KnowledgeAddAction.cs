//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC

using OTBT.Framework.AI;
using OTBT.Framework.Core;
using OTBT.Framework.UI;
#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace AC
{
    public class KnowledgeAddAction : Action
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "AI/Add or modify fact";
        public override string Description => "Adds a piece of knowledge to our DB";

        // Declare variables here
        [SerializeField]
        StringOrAtomReference<KnowledgeAtom> m_Identifier = new StringOrAtomReference<KnowledgeAtom>();

        [SerializeField]
        int m_Operation = 0;

        [SerializeField]
        int m_OperationValue = 0;

        override public float Run()
        {
            switch(m_Operation)
            {
                case 0: // add
                    KnowledgeManager.instance.Add(m_Identifier, m_OperationValue);
                    break;
                case 1: // remove
                    KnowledgeManager.instance.Remove(m_Identifier);
                    break;
                case 2: // increase
                    KnowledgeManager.instance.Increase(m_Identifier, m_OperationValue);
                    break;
                case 3: // decrease
                    KnowledgeManager.instance.Decrease(m_Identifier, m_OperationValue);
                    break;
            }
            return 0f;
        }

        override public void Skip()
        {
            Run();
        }

#if UNITY_EDITOR
        override public void ShowGUI()
        {
            m_Operation = GUILayout.Toolbar(m_Operation, new string[] { "Add", "Remove", "Increase", "Decrease" });
            m_Identifier.ShowGUI(this, "Fact");
            if (m_Operation != 1)
                m_OperationValue = EditorGUILayout.IntField("Value", m_OperationValue);
        }


        public override string SetLabel()
        {
            return "Add or modify fact";
        }
#endif
    }
}

#endif