//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OTBT.Framework.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform m_RectTransform;

        Vector2 m_PointerOffset = Vector2.zero;

        protected RectTransform rectTransform => m_RectTransform;

        public event Action<PointerEventData> onPickup;
        public event Action<PointerEventData> onDrag;
        public event Action<PointerEventData> onDrop;

        protected virtual void OnValidate()
        {
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_PointerOffset = (Vector2)m_RectTransform.position - eventData.position;
            onPickup?.Invoke(eventData);
            OnPickup(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_RectTransform.position = eventData.position + m_PointerOffset;
            onDrag?.Invoke(eventData);
            OnDraggy(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_PointerOffset = Vector2.zero;
            onDrop?.Invoke(eventData);
            OnDrop(eventData);
        }

        public T GetRaycastResult<T>() where T : MonoBehaviour
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = m_RectTransform.position;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult raycastResult in results)
            {
                raycastResult.gameObject.TryGetComponent<T>(out T behaviour);
                if (behaviour != null) return behaviour;
            }

            return null;
        }

        protected virtual void OnPickup(PointerEventData eventData) { }
        protected virtual void OnDraggy(PointerEventData eventData) { }
        protected virtual void OnDrop(PointerEventData eventData) { }

    }
}
