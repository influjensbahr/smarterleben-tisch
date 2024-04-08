//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine;

#if OTBT_AC && UNITY_EDITOR
namespace OTBT.Framework.Utils
{
    using AC;

    public class ActionUtils
    {
        public static T GetPreviousActionOfType<T>(Action a) where T : Action
        {
            Action current = a;
            while(current != null)
            {
                current = GetPreviousAction(current);
                if (current is T) return current as T;
            }
            return null;
        }

        public static Action GetPreviousAction(Action a)
        {
            if (a.parentActionListInEditor == null) return null;
            foreach(Action e in a.parentActionListInEditor.actions)
            {
                foreach(ActionEnd end in e.endings)
                {
                    if (end == null || end.skipActionActual == null) continue;
                    if (end.skipActionActual.Equals(a)) return e;
                }
            }
            return null;
        }
    }
}
#endif