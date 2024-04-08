// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using System;
using OTBT.Framework.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace OTBT.Framework.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class UITextLinkHandler : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler
    {
        [SerializeField] TMP_Text m_Text;
        [SerializeField] public UnityEvent<string> onLinkClick;
        [SerializeField] public UnityEvent<string> onLinkHoverStart;
        [SerializeField] public UnityEvent<string> onLinkHoverEnd;

        string m_CurrentHoveredLink = string.Empty;
        void OnValidate()
        {
            if (m_Text == null) m_Text = GetComponent<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(m_Text, eventData.position, null);
            if (linkIndex < 0) return;
            var link = m_Text.textInfo.linkInfo[linkIndex];

            onLinkClick?.Invoke(link.GetLinkID());
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(m_Text, eventData.position, null);
            if (linkIndex < 0)
            {
                if (!string.IsNullOrWhiteSpace(m_CurrentHoveredLink))
                {
                    onLinkHoverEnd?.Invoke(m_CurrentHoveredLink);

                    m_CurrentHoveredLink = string.Empty;
                }
                return;
            }
            var link = m_Text.textInfo.linkInfo[linkIndex];

            if (m_CurrentHoveredLink.Equals(link.GetLinkID())) return;
            m_CurrentHoveredLink = link.GetLinkID();
            onLinkHoverStart?.Invoke(m_CurrentHoveredLink);
        }
    }

}
