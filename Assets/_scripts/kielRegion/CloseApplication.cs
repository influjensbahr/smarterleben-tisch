using TuioNet.Tuio11;
using UnityEngine;

public class CloseApplication : MonoBehaviour
{
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
        var touchPosition = new Vector2(tuioCursor.Position.X * Screen.width, Screen.height - tuioCursor.Position.Y * Screen.height);

        if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), touchPosition))
        {
            Application.Quit();
        }
    }
    
}
