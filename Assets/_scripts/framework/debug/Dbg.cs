//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Debugging;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Editor-only logging functionality that enforces appending a context to log messages.
    /// </summary>
    public class Dbg
    {
        public static void Log(Object context, string message)
        {
#if UNITY_EDITOR
            Debug.Log(message, context);
#else
            ErrorLogManager.instance.HandleLog(message, "", LogType.Log);
#endif
        }

        public static void Warning(Object context, string message)
        {
#if UNITY_EDITOR
            Debug.LogWarning(message, context);
#else
            ErrorLogManager.instance.HandleLog(message, "", LogType.Warning);
#endif
        }

        public static void Error(Object context, string message)
        {
#if UNITY_EDITOR
            Debug.LogError(message, context);
#else
            ErrorLogManager.instance.HandleLog(message, "", LogType.Error);
#endif
        }

        public static void Exception(Object context, Exception exception)
        {
#if UNITY_EDITOR
            Debug.LogException(exception, context);
#else
            ErrorLogManager.instance.HandleLog(exception.Message, exception.StackTrace, LogType.Exception);
#endif
        }
    }
}
