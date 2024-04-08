// 
// Copyright (c) 2024 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Gameplay 
{ 
    public abstract class ITutorialStep : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract void OnStart(UnityAction callback = null);
        public abstract void OnComplete(UnityAction callback = null);
        public abstract bool AllowsAction(string action, List<GameObject> objects);
        public abstract void TriggerAction(string action, List<GameObject> objects);
    }
}