// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Gameplay;
using OTBT.Framework.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OTBT.Framework.Gameplay
{
    public class TutorialManager : Singleton<TutorialManager>
    {
        bool m_TutorialDone = false;

        private void Start()
        {
            foreach (ITutorialStep steps in m_TutorialSteps)
                steps.Initialize();
        }

        [SerializeField] List<ITutorialStep> m_TutorialSteps = new List<ITutorialStep>();
        ITutorialStep m_CurrentTutorialStep = null;

        public void StartTutorial()
        {
            StartNextTutorialStep();
        }

        private void StartNextTutorialStep()
        {
            if (m_TutorialSteps.Count > 0)
            {
                m_CurrentTutorialStep = m_TutorialSteps.First();
                m_TutorialSteps.Remove(m_CurrentTutorialStep);
                m_CurrentTutorialStep.OnStart();
            }
            else
            {
                m_TutorialDone = true;
            }
        }

        public void PhaseComplete(ITutorialStep step)
        {
            if (step == m_CurrentTutorialStep)
            {
                step.OnComplete(() =>
                {
                    StartNextTutorialStep();
                });
            }
            else
            {
                Debug.Log("wrong tutorial step completed: " + step.name);
            }
        }

        public bool AllowsAction(string actionName, List<GameObject> objects = null)
        {
            if (m_TutorialDone || m_CurrentTutorialStep == null) return true;
            return m_CurrentTutorialStep.AllowsAction(actionName, objects);
        }

        public void TriggerAction(string actionName)
        {
            TriggerAction(actionName, null);
        }

        public void TriggerAction(string actionName, List<GameObject> objects)
        {
            if (m_TutorialDone || m_CurrentTutorialStep == null) return;
            m_CurrentTutorialStep.TriggerAction(actionName, objects);
        }
    }
}