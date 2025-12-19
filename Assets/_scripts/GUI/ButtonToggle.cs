using TuioNet.Tuio11;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Invokes a configured Unity UI Button when a TUIO cursor appears inside a given click area.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ButtonToggle : MonoBehaviour
{
    [SerializeField] Button m_TargetButton = default;
    [SerializeField] RectTransform m_TargetClickArea = default;
    [SerializeField] CanvasGroup m_CanvasGroupInteractibility = default;

    bool m_HasLoggedMissingRefs = false;

    private void OnValidate()
    {
        // Auto-assign common references if not set
        if (m_TargetClickArea == null)
            m_TargetClickArea = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        CustomTuio11Visualizer.onCursorAdd += Toggle;
    }
    
    private void OnDisable()
    {
        CustomTuio11Visualizer.onCursorAdd -= Toggle;
    }


    void Toggle(Tuio11Cursor tuioCursor)
    {
        if (m_CanvasGroupInteractibility != null && !m_CanvasGroupInteractibility.interactable)
            return;

        if (m_TargetClickArea == null || m_TargetButton == null)
        {
            if (!m_HasLoggedMissingRefs)
            {
                Debug.LogWarning("ButtonToggle missing references: assign TargetButton and TargetClickArea.", this);
                m_HasLoggedMissingRefs = true;
            }
            return;
        }

        Vector2 touchPosition = new Vector2(tuioCursor.Position.X * Screen.width, Screen.height - tuioCursor.Position.Y * Screen.height);

        if (RectTransformUtility.RectangleContainsScreenPoint(m_TargetClickArea, touchPosition))
        {
            m_TargetButton.onClick?.Invoke();
        }
    }
}
