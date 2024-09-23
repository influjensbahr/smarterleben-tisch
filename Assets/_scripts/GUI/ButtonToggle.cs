using TuioNet.Tuio11;
using UnityEngine;
using UnityEngine.UI;

public class ButtonToggle : MonoBehaviour
{
    [SerializeField] Button m_TargetButton = default;
    [SerializeField] RectTransform m_TargetClickArea = default;
    [SerializeField] CanvasGroup m_CanvasGroupInteractibility = default;

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
        if (m_CanvasGroupInteractibility != null)
            if (!m_CanvasGroupInteractibility.interactable)
                return;

        Vector2 touchPosition = new Vector2(tuioCursor.Position.X * Screen.width, Screen.height - tuioCursor.Position.Y * Screen.height);

        if (RectTransformUtility.RectangleContainsScreenPoint(m_TargetClickArea, touchPosition))
        {
            m_TargetButton.onClick?.Invoke();
        }
    }
}
