//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckBrokenUnityEvents : VerifyCheckBase
    {
        public override string description => "Broken Unity Events";
        public override string longDescription => "Checks for unity events that were setup at some point but no longer seem to work, most often due to a missing reference.";
        const BindingFlags k_FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public override void PerformCheckForProject() { }
        public override void PerformCheck(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();

            List<(Type, UnityEvent)> events = new List<(Type, UnityEvent)>();
            foreach (var component in components)
            {
                var componentEvents = GetAllEvents(component);
                events.AddRange(componentEvents);
            }

            foreach (var unityEvent in events)
            {
                if (!IsEventValid(unityEvent.Item1, unityEvent.Item2))
                    AddFailedCheck($"Invalid Unity Event {unityEvent.Item1.DeclaringType}", gameObject);
            }
        }

        bool IsEventValid(Type type, UnityEvent unityEvent)
        {
            var eventCount = unityEvent.GetPersistentEventCount();

            for (int i = 0; i < eventCount; i++)
            {
                var eventListener = unityEvent.GetPersistentTarget(i);
                if (eventListener == null) return false;

                var methodName = unityEvent.GetPersistentMethodName(i);
                var method = eventListener.GetType().GetMethods().FirstOrDefault(method => method.Name == methodName);

                if (method == null) return false;
            }

            return true;
        }

        List<(Type, UnityEvent)> GetAllEvents(Component component)
        {
            List<(Type, UnityEvent)> events = new List<(Type, UnityEvent)>();
            if (component != null)
            {
                var type = component.GetType();
                FieldInfo[] fields = type.GetFields(k_FieldBindingFlags);

                foreach (var field in fields)
                {
                    if (!field.FieldType.IsSubclassOf(typeof(UnityEventBase))) continue;

                    var eventBase = field.GetValue(component) as UnityEventBase;
                    if (eventBase is UnityEvent unityEvent) events.Add((type, unityEvent));
                }
            }

            return events;
        }

        public override void PerformCheck(ScriptableObject sobj)
        {
        }
    }
}
#endif