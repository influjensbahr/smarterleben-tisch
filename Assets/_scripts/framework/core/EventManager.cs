//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Gameplay;
using OTBT.Framework.Utils;
#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
#endif
using Sparrow.Verification;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OTBT.Framework.Core
{
    /// <summary>
    /// Simple event manager than can handle events based on scriptable objects and string-based events. Also has capabilities for counting down time until an event happens.
    /// </summary>
    public class EventManager : Utils.Singleton<EventManager>, IVerify, IExtendDefaultEditor
    {
        [SerializeField] VanillaAtom m_EventUpdate = default;
        [SerializeField] VanillaAtom m_EventEverySecond = default;
        [SerializeField] VanillaAtom m_EventPauseGame = default;
        [SerializeField] VanillaAtom m_EventUnpauseGame = default;
        [SerializeField] VanillaAtom m_EventChangeScene = default;

        [SerializeField] EventAtom m_EventEscapeAndroidBackButton = default;
        [SerializeField] EventAtom m_SpacePressed = default;

        EventQueue<string> m_StringDictionary = new();
        List<TimedEvent> m_TriggerInTime = new();
        List<IUpdateFrame> m_UpdateFrame = new();
        List<IUpdateFrame> m_RemoveUpdateFrame = new();

        List<IGamePauseTrigger> m_CurrentPauseReasons = new List<IGamePauseTrigger>();

        bool m_GameRunning = true;
        bool m_InteractionDisabled = false;

        public bool gameRunning => m_GameRunning;
        public bool interactionDisabled => m_InteractionDisabled || PausingThirdPartyAssetsPresent();
        public static EventAtom spacePressed => EventManager.instance.m_SpacePressed;
        public static EventAtom eventUpdate => EventManager.instance.m_EventUpdate as EventAtom;
        public static EventAtom eventEverySecond => EventManager.instance.m_EventEverySecond as EventAtom;
        public static EventAtom pauseGameEvent => EventManager.instance.m_EventPauseGame as EventAtom;
        public static EventAtom unpauseGameEvent => EventManager.instance.m_EventUnpauseGame as EventAtom;
        public static EventAtom changeSceneEvent => EventManager.instance.m_EventChangeScene as EventAtom;

        public void SetInteractionDisabled(bool v)
        {
            m_InteractionDisabled = v;
        }

        bool PausingThirdPartyAssetsPresent()
        {
#if OTBT_AC
            foreach (AC.ActiveList list in AC.KickStarter.actionListManager.ActiveLists)
                if (list.pausesGameplay)
                    return true;
#endif
            return false;
        }

        public void PauseGame(IGamePauseTrigger trigger)
        {
            m_CurrentPauseReasons.Add(trigger);
            if (!m_GameRunning) return;

            m_GameRunning = false;
            TriggerEvent(m_EventPauseGame);
        }

        public void ResumeGame(IGamePauseTrigger trigger)
        {
            m_CurrentPauseReasons.Remove(trigger);
            if (m_CurrentPauseReasons.Count > 0) return;
            if (m_GameRunning) return;

            m_GameRunning = true;
            TriggerEvent(m_EventUnpauseGame);
        }


        public class TimedEvent
        {
            public float timeDelta;
            public UnityAction triggeredEvent;
            bool m_Paused = false;
#if UNITY_EDITOR
            public string debugDescription = "";
#endif

            public bool paused => m_Paused;

            public void SetPaused(bool paused)
            {
                m_Paused = paused;
            }

            public bool PassTime(float _timeDelta)
            {
                if (m_Paused) return false;
                this.timeDelta -= _timeDelta;
                if (timeDelta <= 0f)
                {
                    triggeredEvent.Invoke();
                    return true;
                }
                return false;
            }
        }

        private int lastSecondTime = 0;

        protected override void InitializeInherit()
        {
            if (m_StringDictionary == null) m_StringDictionary = new EventQueue<string>();
        }

        public void Update()
        {
            if (Input.GetKeyDown("escape"))
                TriggerEvent(m_EventEscapeAndroidBackButton);

            if (Input.GetKeyDown("space"))
                TriggerEvent(m_SpacePressed);

            TriggerEvent(m_EventUpdate);

            for (int i = m_UpdateFrame.Count - 1; i >= 0; i--)
                m_UpdateFrame[i].UpdateFrame(Time.deltaTime);

            foreach (IUpdateFrame update in m_RemoveUpdateFrame)
                m_UpdateFrame.Remove(update);
            m_RemoveUpdateFrame.Clear();

            // triggering the once per second events
            if (lastSecondTime != (int)Time.unscaledTime)
            {
                lastSecondTime = (int)Time.unscaledTime;
                TriggerEvent(m_EventEverySecond);
            }

            // these are the elements we will trigger after a given time has passed... count them down
            for (int i = m_TriggerInTime.Count - 1; i >= 0; i--)
                if (m_TriggerInTime[i].PassTime(Time.deltaTime))
                    m_TriggerInTime.RemoveAt(i);
        }

        public TimedEvent TriggerInTime(float timeToPass, UnityAction evt)
        {
            TimedEvent ret = new TimedEvent
            {
                timeDelta = timeToPass,
                triggeredEvent = evt
            };
            m_TriggerInTime.Add(ret);
            return ret;
        }

        public void RemoveTimeTrigger(TimedEvent m_EndingTrigger)
        {
            m_TriggerInTime.Remove(m_EndingTrigger);
        }

        public void StartListening(StringOrAtomReference<EventAtom> eventName, UnityAction listener)
        {
            StartListening(eventName.ToString(), listener);
        }

        public void StopListening(StringOrAtomReference<EventAtom> eventName, UnityAction listener)
        {
            StopListening(eventName.ToString(), listener);
        }

        public void StartListening(IUpdateFrame start)
        {
            m_UpdateFrame.Add(start);
        }

        public void StopListening(IUpdateFrame remove)
        {
            m_RemoveUpdateFrame.Add(remove);
        }

        public void TriggerEvent(StringOrAtomReference<EventAtom> eventName)
        {
            TriggerEvent(eventName.ToString());
        }

        public void ClearEventListeners(StringOrAtomReference<EventAtom> eventName)
        {
            ClearEventListeners(eventName.ToString());
        }

        public void TriggerEvent(string eventName)
        {
            m_StringDictionary.TriggerEvent(eventName);
        }
        public void TriggerEvent(VanillaAtom eventName)
        {
            m_StringDictionary.TriggerEvent(eventName.identifier);
        }

        public void StartListening(VanillaAtom eventName, UnityAction listener)
        {
            m_StringDictionary.StartListening(eventName.identifier, listener);
        }

        public void StartListening(string eventName, UnityAction listener)
        {
            m_StringDictionary.StartListening(eventName, listener);
        }
        public void StopListening(VanillaAtom eventName, UnityAction listener)
        {
            m_StringDictionary.StopListening(eventName.identifier, listener);
        }
        public void StopListening(string eventName, UnityAction listener)
        {
            m_StringDictionary.StopListening(eventName, listener);
        }

        public void ClearEventListeners(string eventName)
        {
            m_StringDictionary.ClearEventListeners(eventName);
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.Check(m_EventUpdate != null, "EventManager has no update event", gameObject);
            checker.Check(m_EventEverySecond != null, "EventManager has no every second event", gameObject);
        }


#if UNITY_EDITOR
        public void ExtendDefaultEditor()
        {
            if(Application.isPlaying)
            {
                if(!gameRunning)
                {
                    GUILayout.Label("The game is currently PAUSED!");
                    foreach(IGamePauseTrigger trigger in m_CurrentPauseReasons)
                        GUILayout.Label(trigger.ToString());

                    OTBT.Framework.Utils.Editor.EditorUtils.Space();
#if OTBT_AC
                    GUILayout.Label("PAUSING AC action lists:");
                    foreach (AC.ActiveList list in AC.KickStarter.actionListManager.ActiveLists)
                        if(list.pausesGameplay) GUILayout.Label(list.actionList.name);
                    OTBT.Framework.Utils.Editor.EditorUtils.Space();
#endif
                }

#if OTBT_AC
                GUILayout.Label("Current running AC action lists:");
                foreach (AC.ActiveList list in AC.KickStarter.actionListManager.ActiveLists)
                    GUILayout.Label(list.actionList.name);
                OTBT.Framework.Utils.Editor.EditorUtils.Space();
#endif

                GUILayout.Label("Timed events registered: ");
                if (m_TriggerInTime.Count == 0) GUILayout.Label("-none-");
                foreach(TimedEvent t in m_TriggerInTime)
                {
                    GUILayout.Label(t.timeDelta + " / " + (t.paused ? "PAUSED / " : "") + t.debugDescription + " // " + t.triggeredEvent.ToString());
                }
            }
        }
#endif
            }
}
