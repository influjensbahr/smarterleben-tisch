using System.Collections;
using System.Collections.Generic;
using OTBT.Framework.Utils;
using UnityEngine;

namespace OTBT.Framework.Gameplay
{
    public class DragAndDropManager : MonoBehaviour
    {
        [SerializeField] List<SnapTarget> m_SnapTargets = new List<SnapTarget>();
        [SerializeField] protected float m_MinDistanceToSnapPoint = 2f;
        [SerializeField] bool m_parentToSnapTarget;
        protected Dictionary<Draggable, List<SnapTarget>> assignedDraggables = new Dictionary<Draggable, List<SnapTarget>>();
        
        public virtual void DraggablePickedUp(Draggable draggable)
        {
            List <SnapTarget> snapTargets;
            if (assignedDraggables.TryGetValue(draggable, out snapTargets))
            {
                foreach (var snapTarget in snapTargets)
                {
                    snapTarget.RemoveAssignedDraggable();
                }
                assignedDraggables.Remove(draggable);
            }
        }
        
        public virtual void DraggableReleased(Draggable draggable)
        {
            float smallestDistance = m_MinDistanceToSnapPoint;
            SnapTarget closestTarget = null;
            foreach (SnapTarget snapTarget in m_SnapTargets)
            {
                var distance = Vector3.Distance(snapTarget.transform.position, draggable.transform.position);
                if (distance < m_MinDistanceToSnapPoint)
                {
                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        closestTarget = snapTarget;
                    }
                }
            }

            if (closestTarget != null)
            {
                SnapToTarget(draggable, closestTarget);
            }
        }

        protected virtual void SnapToTarget(Draggable draggable, SnapTarget snapTarget)
        {
            //Instant snap
            draggable.transform.position = snapTarget.snapTransform.position;
            draggable.transform.localRotation = snapTarget.snapTransform.rotation;
            //TODO: Add check for overlapping more snap points
            
            if (assignedDraggables.ContainsKey(draggable))
            {
                assignedDraggables[draggable].Add(snapTarget);
            }
            else
            {
                assignedDraggables.Add(draggable, new List<SnapTarget>{snapTarget});
            }
            
            snapTarget.AssignDraggable(draggable);
            if (m_parentToSnapTarget)
            {
                ParentToSnapTarget(draggable, snapTarget);
            }
        }

        protected virtual void ParentToSnapTarget(Draggable itemToParent, SnapTarget snapTarget)
        {
            itemToParent.transform.parent = snapTarget.snapTransform;
        }

        protected virtual void ReleaseFromSnapTarget(Transform itemToRelease)
        {
            itemToRelease.parent = null;
        }
    }
}
