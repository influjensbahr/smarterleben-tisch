//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_SOCKET_IO
using Firesplash.UnityAssets.SocketIO;
#endif
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System;
using UnityEngine;

namespace OTBT.Framework.Networking
{
    /// <summary>
    /// Bridge between individual socketIO implementations, i.e. from assets, and our code. Only use this class to interface with
    /// socket IO and let this class handle the specifics of the implementation / asset we are using.
    /// </summary>
    public class SocketIOBridge : Singleton<SocketIOBridge>, IVerify
    {
        [Header("<Hostname>[:<Port>][/<path>]")]
        [SerializeField] public string m_SocketIOAddress = "example.com";

        [Header("Connection settings")]
        [SerializeField] public bool m_UseHTTPS = false;
        [SerializeField] public bool m_AutoConnectOnStart = false;
        [SerializeField] public bool m_AutoReconnectWhenLost = false;

#if OTBT_SOCKET_IO
        SocketIOCommunicator m_Communicator = null;
#endif

        private void Start()
        {
#if OTBT_SOCKET_IO
            m_Communicator = gameObject.AddComponent<SocketIOCommunicator>();
#endif

            On("connect", (string e) => {
                Debug.Log("SocketIO connected");
            });

            if (m_AutoConnectOnStart)
                Connect();
        }

        private void Connect()
        {
#if OTBT_SOCKET_IO
            m_Communicator.Instance.Connect((m_UseHTTPS ? "https" : "http") + "://" + m_SocketIOAddress, m_AutoReconnectWhenLost);
#endif
        }

        public void Emit(string s)
        {
#if OTBT_SOCKET_IO
            m_Communicator.Instance.Emit(s);
#endif
        }

        public void Emit(string s, string j, bool isPlainText = false)
        {
#if OTBT_SOCKET_IO
            m_Communicator.Instance.Emit(s, j, isPlainText);
#endif
        }

        internal void On(string v, Action<String> p)
        {
#if OTBT_SOCKET_IO
            m_Communicator.Instance.On(v, (r) => { p.Invoke(r); });
#endif
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(!m_SocketIOAddress.Equals("example.com"), "Socketaddress not set properly.", gameObject);
        }
    }
}