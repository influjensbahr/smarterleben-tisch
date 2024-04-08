// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using System.Collections;
using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.Events;


namespace OTBT.Framework.Mobile
{
    public class PermissionsAndroidAndIOS : MonoBehaviour
    {
#if UNITY_IOS
        private class IOSPermissionRequest
        {
            public IEnumerator RequestWebcamPermissionIOS(UnityAction<bool, bool> callback)
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
                callback.Invoke(Application.HasUserAuthorization(UserAuthorization.WebCam), true);
            }
        }
#endif

        public static void RequestCameraPermission(MonoBehaviour requestSource, UnityAction<bool, bool> callback)
        {
            if (HasCameraPermission()) 
            {
                callback?.Invoke(true, false);
            } 
            else 
            {
#if PLATFORM_ANDROID
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += (s) =>  callback?.Invoke(false, false);
                callbacks.PermissionGranted += (s) => callback?.Invoke(true, false);
                callbacks.PermissionDeniedAndDontAskAgain += (s) => callback?.Invoke(false, true);
                Permission.RequestUserPermission(Permission.Camera, callbacks);
#elif UNITY_IOS
                requestSource.StartCoroutine(new IOSPermissionRequest().RequestWebcamPermissionIOS(callback));
#endif
            }
        }

        public static bool HasCameraPermission()
        {
#if PLATFORM_ANDROID && !UNITY_EDITOR
             return Permission.HasUserAuthorizedPermission(Permission.Camera);
#elif UNITY_IOS && !UNITY_EDITOR
             return Application.HasUserAuthorization(UserAuthorization.WebCam);
#else
            return true;
#endif
        }

    }
}