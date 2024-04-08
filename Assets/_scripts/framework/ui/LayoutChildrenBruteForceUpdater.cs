// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LayoutChildrenBruteForceUpdater 
{
    RectTransform m_ListParent;
    ScrollRect m_ScrollRect;
    List<HorizontalOrVerticalLayoutGroup> m_LayoutGroups = new List<HorizontalOrVerticalLayoutGroup>();
    List<RectTransform> m_RectTransforms = new List<RectTransform>();

    protected UnityAction m_FinishCallback = null;

    ScreenPreparationPhases currentPhase = ScreenPreparationPhases.WAITING;
    public enum ScreenPreparationPhases { WAITING, ONE, TWO, THREE, FOUR, FIVE }

    public void Collect(RectTransform parent, ScrollRect rect, UnityAction callback)
    {
        m_ScrollRect = rect;
        m_ListParent = parent;
        m_FinishCallback = callback;
    }

    public void StartScreenLayoutPreparation(ScreenPreparationPhases nextPhase = ScreenPreparationPhases.ONE)
    {
        if(nextPhase == ScreenPreparationPhases.ONE)
        {
            m_LayoutGroups.Clear();
            m_RectTransforms.Clear();

            foreach (HorizontalLayoutGroup group in m_ListParent.GetComponentsInChildren<HorizontalLayoutGroup>())
                m_LayoutGroups.Add(group);
            foreach (HorizontalLayoutGroup group in m_ListParent.GetComponents<HorizontalLayoutGroup>())
                m_LayoutGroups.Add(group);
            foreach (RectTransform trans in m_ListParent.GetComponentsInChildren<RectTransform>())
                m_RectTransforms.Add(trans);
            foreach (RectTransform trans in m_ListParent.GetComponents<RectTransform>())
                m_RectTransforms.Add(trans);
        }

        currentPhase = nextPhase;
        foreach(RectTransform r in m_RectTransforms)
            LayoutRebuilder.MarkLayoutForRebuild(r);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_ListParent);
    }

    public void ReportOnGUI(EventType type)
    {
        switch (currentPhase)
        {
            case ScreenPreparationPhases.WAITING: return;
            /* case ScreenPreparationPhases.ONE:
                 if (type == EventType.Layout)
                 {
                     foreach(HorizontalOrVerticalLayoutGroup layoutGroup in m_LayoutGroups)
                         layoutGroup.spacing += 1f;
                     StartScreenLayoutPreparation(ScreenPreparationPhases.TWO);
                     return;
                 }
                 break;
             case ScreenPreparationPhases.TWO:
                 if (type == EventType.Layout)
                 {
                     foreach (HorizontalOrVerticalLayoutGroup layoutGroup in m_LayoutGroups)
                         layoutGroup.spacing -= 1f;
                     StartScreenLayoutPreparation(ScreenPreparationPhases.FIVE);
                     return;
                 }
                 break;*/
            //case ScreenPreparationPhases.FIVE:
            default:
                if (type == EventType.Layout)
                {
                    m_FinishCallback?.Invoke();
                    if (m_FinishCallback != null && m_ScrollRect != null)
                    {
                        m_ScrollRect.verticalNormalizedPosition = 1f;
                    }
                    m_FinishCallback = null;
                    currentPhase = ScreenPreparationPhases.WAITING;
                }
                break;
        }
    }
}
