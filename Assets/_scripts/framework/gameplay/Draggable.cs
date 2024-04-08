using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Gameplay
{
    public class Draggable : MonoBehaviour
    {
        [SerializeField] protected DragAndDropManager m_DragAndDropManager;
        [SerializeField] protected bool m_FixedZPosition;
        
        protected float zValue;
        protected Vector3 m_MousePosition;
        protected Transform originalParent;

        private void Awake()
        {
            originalParent = transform.parent;
        }

        private void OnMouseDown()
        {
            PickedUp();
        }

        private void OnMouseDrag()
        {
            Drag();
        }

        private void OnMouseUp()
        {
            Released();
        }

        private void OnMouseOver()
        {
            Hover();
        }

        private Vector3 GetMousePosition()
        {
            return Camera.main.WorldToScreenPoint(transform.position);
        }
        
        protected virtual void PickedUp()
        {
            zValue = transform.position.z;
            m_MousePosition = Input.mousePosition - GetMousePosition();
            m_DragAndDropManager.DraggablePickedUp(this);
        }

        protected virtual void Released()
        {
            m_DragAndDropManager.DraggableReleased(this);
            if (m_FixedZPosition)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, zValue);
            }
        }

        protected virtual void Drag()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - m_MousePosition);
        }

        protected virtual void Hover()
        {
            
        }

        public void ResetParent()
        {
            transform.parent = originalParent;
        }
    }
}
