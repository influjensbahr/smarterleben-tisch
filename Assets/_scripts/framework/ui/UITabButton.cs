//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    [RequireComponent(typeof(Button))]
    public class UITabButton : MonoBehaviour
    {
        [SerializeField, HideInInspector] Button m_Button;
        [SerializeField] UnityEvent onSelect, onDeselect;

        public Button button => m_Button;

        void OnValidate()
        {
            if (button == null) m_Button = GetComponent<Button>();
        }

        void Awake()
        {
            var group = GetComponentInParent<UITabGroup>();
            if (group != null) group.Rebuild();
        }

        public void SelectTab() => onSelect?.Invoke();
        public void DeselectTab() => onDeselect?.Invoke();

        public void RegisterTabGroup(UITabGroup uiTabGroup)
        {
            button.onClick.AddListener(() => uiTabGroup.SelectTab(this));
        }
    }
}
