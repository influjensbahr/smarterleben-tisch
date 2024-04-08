//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OTBT.Framework.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] RectTransform m_RectTransform;

        protected RectTransform rectTransform => m_RectTransform;

        public event Action<PointerEventData> onHoverStart;
        public event Action<PointerEventData> onHoverEnd;

        protected virtual void OnValidate()
        {
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onHoverStart?.Invoke(eventData);
            OnHoverStart(eventData);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            onHoverEnd?.Invoke(eventData);
            OnHoverEnd(eventData);
        }

        protected virtual void OnHoverStart(PointerEventData eventData) { }
        protected virtual void OnHoverEnd(PointerEventData eventData) { }
    }
}
