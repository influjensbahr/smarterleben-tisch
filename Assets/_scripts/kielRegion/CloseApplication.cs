using TuioNet.Tuio11;
using UnityEngine;

/// <summary>
/// Quits the application when a TUIO cursor is detected within the attached RectTransform area.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CloseApplication : MonoBehaviour
{
    RectTransform m_Rect;

    void Awake()
    {
        m_Rect = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        CustomTuio11Visualizer.onCursorAdd += Close;
    }
    
    void OnDisable()
    {
        CustomTuio11Visualizer.onCursorAdd -= Close;
    }
    
    void Close(Tuio11Cursor tuioCursor)
    {
        if (m_Rect == null) return;

        var touchPosition = new Vector2(tuioCursor.Position.X * Screen.width, Screen.height - tuioCursor.Position.Y * Screen.height);

        if (RectTransformUtility.RectangleContainsScreenPoint(m_Rect, touchPosition))
        {
            Application.Quit();
        }
    }
}
