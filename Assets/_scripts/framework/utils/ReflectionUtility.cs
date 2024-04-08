//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Boilerplate for working with Reflection.
    /// </summary>
    public static class ReflectionUtil
    {
        /// <summary>
        /// Checks all objects in the active scene if they implement an interface and returns a list of them
        /// </summary>
        /// <typeparam name="T">Type of the interface to search for</typeparam>
        /// <returns>List of components that implement an interface</returns>
        public static List<T> FindInterfaceImplementations<T>()
        {
            List<T> interfaces = new List<T>();
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var rootGameObject in rootGameObjects)
            {
                T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>();
                foreach (var childInterface in childrenInterfaces)
                {
                    interfaces.Add(childInterface);
                }
            }
            return interfaces;
        }

        public static bool DoesTypeSupportInterface(Type type, Type inter)
        {
            if (inter.IsAssignableFrom(type))
                return true;
            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == inter))
                return true;
            return false;
        }

        public static IEnumerable<Assembly> GetReferencingAssemblies(Assembly assembly)
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies().Where(asm => asm.GetReferencedAssemblies().Any(asmName => AssemblyName.ReferenceMatchesDefinition(asmName, assembly.GetName())));
        }

        public static IEnumerable<Type> TypesImplementingInterface(Type desiredType)
        {
            var assembliesToSearch = new Assembly[] {
                    desiredType.Assembly
                }
                .Concat(GetReferencingAssemblies(desiredType.Assembly));
            return assembliesToSearch.SelectMany(assembly => assembly.GetTypes())
                .Where(type => DoesTypeSupportInterface(type, desiredType));
        }

        public static IEnumerable<Type> NonAbstractTypesImplementingInterface(Type desiredType)
        {
            return TypesImplementingInterface(desiredType).Where(t => !t.IsAbstract);
        }
    }
}