#if OTBT_AC
using UnityEngine;
using UnityEngine.Events;

public abstract class ActionCallback : MonoBehaviour
{
    RunCodeAction m_ReturnAction;

    [SerializeField] UnityEvent onGameFinished;

    public virtual void Run(RunCodeAction action)
    {
        m_ReturnAction = action;
    }

    protected void Finish()
    {
        onGameFinished.Invoke();
        m_ReturnAction?.Finish();
    }
}
#endif
