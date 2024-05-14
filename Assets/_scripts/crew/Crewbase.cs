using UnityEngine;

public class Crewbase : MonoBehaviour
{
    int m_GenerationNr = 1;
    bool m_IsCrewHealthy = true;
    int m_ConditionSatisfaction = 100;
    int m_ConditionOrder = 50;
    int m_ConditionHope = 100;
    int m_MaxConditionValue = 100;

    public int GenerationNr => m_GenerationNr;
    public bool IsCrewHealthy => m_IsCrewHealthy;
    public int ConditionSatisfaction => m_ConditionSatisfaction;
    public int ConditionOrder => m_ConditionOrder;
    public int ConditionHope => m_ConditionHope;

    public void SetConditionSatisfaction(int value)
    {
        m_ConditionSatisfaction = value;
    }

    public void SetConditionOrder(int value)
    {
        m_ConditionOrder = value;
    }

    public void SetConditionHope(int value)
    {
        m_ConditionHope = value;
    }

    public void SetIsCrewHealthy(bool value)
    {
        m_IsCrewHealthy = value;
    }

    public void SetGenerationNr(int value)
    {
        m_GenerationNr = value;
    }
}