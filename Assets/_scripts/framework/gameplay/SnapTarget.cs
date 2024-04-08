using System.Collections;
using System.Collections.Generic;
using OTBT.Framework.AI;
using Unity.VisualScripting;
using UnityEngine;

namespace OTBT.Framework.Gameplay
{
    public class SnapTarget : MonoBehaviour
    {
        public Transform snapTransform;
        public KnowledgeAtom knowledgeAtom;
        protected Draggable assignedDraggable;
        
        public virtual void AssignDraggable(Draggable draggable)
        {
            if (assignedDraggable == null)
            {
                assignedDraggable = draggable;
            }
            else
            {
                RemoveAssignedDraggable();
                assignedDraggable = draggable;
            }
        }

        public virtual void RemoveAssignedDraggable()
        {
            assignedDraggable.ResetParent();
            assignedDraggable = null;
        }
    }
}
