using TuioNet.Tuio11;
using UnityEngine;

public class MovementWithTouch : MonoBehaviour
{
    /**
     * Prinzipiell geht das Script, aber die Movement Schritte sind noch sehr klein
     * und die Bewegung ist nicht flüssig.
     */
    void OnEnable()
    {
        CustomTuio11Visualizer.onCursorAdd += OnCursorEvent;
        CustomTuio11Visualizer.onCursorUpdate += OnCursorEvent;
        Debug.Log("MovementWithTouch enabled");
    }

    void OnDisable()
    {
        CustomTuio11Visualizer.onCursorAdd -= OnCursorEvent;
        CustomTuio11Visualizer.onCursorUpdate -= OnCursorEvent;
        Debug.Log("MovementWithTouch disabled");
    }

    void OnCursorEvent(Tuio11Cursor tuioCursor)
    {
        Debug.Log($"Cursor event: {tuioCursor.CursorId}, Position: {tuioCursor.Position}");
        MoveObject(tuioCursor);
    }

    void MoveObject(Tuio11Cursor tuioCursor)
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main camera not found");
            return;
        }

        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(tuioCursor.Position.X * Screen.width, Screen.height - tuioCursor.Position.Y * Screen.height, Camera.main.nearClipPlane));
        targetPosition.z = transform.position.z;
        Debug.Log($"Moving object to: {targetPosition}");

        transform.localPosition = targetPosition;
    }
}