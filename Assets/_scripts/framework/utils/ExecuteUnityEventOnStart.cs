using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Utils
{
    public class ExecuteUnityEventOnStart : MonoBehaviour
    {
        [SerializeField] UnityEvent onStart;

        // Start is called before the first frame update
        void Start()
        {
            onStart?.Invoke();
        }
    }
}
