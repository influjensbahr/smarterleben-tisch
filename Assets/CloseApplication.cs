using TuioNet.Tuio11;
using UnityEngine;

public class CloseApplication : MonoBehaviour
{
    private void OnEnable()
    {
        CustomTuio11Visualizer.onCursorAdd += Close;
    }
    
    private void OnDisable()
    {
        CustomTuio11Visualizer.onCursorAdd -= Close;
    }
    
    private void Close(Tuio11Cursor tuioCursor)
    {
        Vector2 touchPosition = new Vector2(tuioCursor.Position.X * Screen.width, Screen.height - tuioCursor.Position.Y * Screen.height);

        if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), touchPosition))
        {
            Application.Quit();
        }
    }
    
}
