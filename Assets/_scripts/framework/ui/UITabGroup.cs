//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.UI
{
    public class UITabGroup : MonoBehaviour
    {
        [SerializeField, HideInInspector] List<UITabButton> m_TabButtons;
        [SerializeField] UITabButton m_FirstSelected;

        UITabButton m_Selected;

        void Awake()
        {
            Rebuild();
            if (m_FirstSelected != null) SelectTab(m_FirstSelected);
        }

        public void SelectFirst()
        {
            if (m_TabButtons.Count == 0) return;
            SelectTab(m_TabButtons[0]);
        }

        public void SelectTab(int index)
        {
            var tab = m_TabButtons[index];
            SelectTab(tab);
        }

        public void SelectTab(UITabButton button)
        {
            if (m_Selected == button) return; //don't select again
            if (m_Selected != null) m_Selected.DeselectTab();

            m_Selected = button;
            m_Selected.SelectTab();
        }

        void OnValidate()
        {
            m_TabButtons = new List<UITabButton>(GetComponentsInChildren<UITabButton>());
        }

        public void Rebuild()
        {
            m_TabButtons = new List<UITabButton>(GetComponentsInChildren<UITabButton>());

            foreach (var button in m_TabButtons)
            {
                button.RegisterTabGroup(this);
            }
        }
    }

}
