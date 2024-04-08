//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Sparrow.BugTracking
{
    public class WebUtils 
    {
        [System.Serializable]
        public class ArrayResponse<T1>
        {
            public T1[] items;
        }

        public static async Task GetSimpleTextResponse(string request_url, UnityAction<string> callback, UnityAction<string> errorCallback, int timeout = 1, List<(string, string)> headers = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(request_url);
            request.SetRequestHeader("ContentType", "application/text");
            request.timeout = timeout;
            if (headers != null)
                foreach ((string, string) kvp in headers)
                    request.SetRequestHeader(kvp.Item1, kvp.Item2);

            UnityWebRequestAsyncOperation op = request.SendWebRequest();

            while (!op.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                errorCallback(request.error);
            }
            else
            {
                try
                {
                    callback(request.downloadHandler.text);
                }
                catch (ArgumentException e)
                {
                    Debug.Log("Error: " + request.downloadHandler.text);
                    Debug.Log("parse json failed: " + e.ToString() + " // " + request.downloadHandler);
                    errorCallback(request.downloadHandler.text);
                }
            }
        }

        public static async Task GetSingleObject<T>(string request_url, UnityAction<T> callback, UnityAction<string> errorCallback, int timeout = 1, List<(string, string)> headers = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(request_url);
            request.SetRequestHeader("ContentType", "application/json");
            request.timeout = timeout;
            if(headers != null)
                foreach((string,string) kvp in headers)
                    request.SetRequestHeader(kvp.Item1, kvp.Item2);

            UnityWebRequestAsyncOperation op = request.SendWebRequest();

            while (!op.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                errorCallback(request.error);
            }
            else
            {
                try
                {
                    callback(JsonUtility.FromJson<T>(request.downloadHandler.text));
                }
                catch (ArgumentException e)
                {
                    Debug.Log(request.downloadHandler.text);
                    Debug.Log("parse json failed: " + e.ToString() + " // " + request.downloadHandler);
                    errorCallback(request.downloadHandler.text);
                }
            }
        }


        public static async Task<bool> PostSingleObject(string request_url, string body, UnityAction<string> callback, UnityAction<string> errorCallback, List<(string, string)> headers = null, string expectedValue = "")
        {
            UnityWebRequest request = UnityWebRequest.Put(request_url, body);
            request.method = "POST";
            request.SetRequestHeader("Content-Type", "application/json");

            if (headers != null)
                foreach (var kvp in headers)
                    request.SetRequestHeader(kvp.Item1, kvp.Item2);

            UnityWebRequestAsyncOperation op = request.SendWebRequest();

            while (!op.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                errorCallback?.Invoke(request.error);
                return false;
            }
            else
            {
                if (!expectedValue.Equals("") && !expectedValue.Equals(request.downloadHandler.text))
                {
                    errorCallback?.Invoke(request.downloadHandler.text);
                    return false;
                } else
                {
                    callback?.Invoke(request.downloadHandler.text);
                    return true;
                }
            }
        }


        public static async Task<bool> PostSingleObject<T>(string request_url, string body, UnityAction<T> callback, UnityAction<string> errorCallback, List<(string, string)> headers = null)
        {
            UnityWebRequest request = UnityWebRequest.Put(request_url, body);
            request.method = "POST";
            request.SetRequestHeader("Content-Type", "application/json");

            if (headers != null)
                foreach (var kvp in headers)
                    request.SetRequestHeader(kvp.Item1, kvp.Item2);

            UnityWebRequestAsyncOperation op = request.SendWebRequest();

            while (!op.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                errorCallback(request.error);
                return false;
            }
            else
            {
                try
                {
                    callback(JsonUtility.FromJson<T>(request.downloadHandler.text));
                    return true;
                }
                catch (ArgumentException e)
                {
                    Debug.Log("parse json failed: " + e.ToString() + " // " + request.downloadHandler.text);
                    errorCallback(request.downloadHandler.text);
                    return false;
                }
            }
        }

        public static async Task<bool> GetArrayResponse<T>(string request_url, UnityAction<ArrayResponse<T>> callback, UnityAction<string> errorCallback, UnityAction loadFinishedCallback = null, bool augmentArrayNotation = false, List<(string, string)> headers = null)
        { 
            UnityWebRequest request = UnityWebRequest.Get(request_url);
            request.SetRequestHeader("ContentType", "application/json");

            if (headers != null)
                foreach (var kvp in headers)
                    request.SetRequestHeader(kvp.Item1, kvp.Item2);

            UnityWebRequestAsyncOperation op = request.SendWebRequest();

            while (!request.isDone)
                await Task.Yield();

            try
            {
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    errorCallback(request.error);
                    loadFinishedCallback?.Invoke();
                    return false;
                }
                else
                {
                    if (augmentArrayNotation)
                        callback(JsonUtility.FromJson<ArrayResponse<T>>($"{{\"items\":{request.downloadHandler.text}}}"));
                    else
                        callback(JsonUtility.FromJson<ArrayResponse<T>>(request.downloadHandler.text));
                }
                loadFinishedCallback?.Invoke();
                return true;
           }
            catch (Exception e)
            {
                errorCallback(e.ToString() + request.downloadHandler.text);
                loadFinishedCallback?.Invoke();
                return false;
            }
        }
    }
}