using TuioNet.Tuio11;
using UnityEngine;

public class ButtonToggle : MonoBehaviour
{
    [SerializeField] GameObject m_targetObject;
    [SerializeField] bool m_isActive = false;

    private void OnEnable()
    {
        CustomTuio11Visualizer.onCursorAdd += Toggle;
    }
    
    private void OnDisable()
    {
        CustomTuio11Visualizer.onCursorAdd -= Toggle;
    }

    void Start()
    {
        m_targetObject.SetActive(m_isActive);
    }

    void Toggle(Tuio11Cursor tuioCursor)
    {
        Debug.Log("Toggle");
        Vector2 touchPosition = new Vector2(tuioCursor.Position.X * Screen.width, Screen.height - tuioCursor.Position.Y * Screen.height);
        
        if (m_targetObject.activeSelf)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(m_targetObject.GetComponent<RectTransform>(), touchPosition))
            {
                m_targetObject.SetActive(false);
            }
        }
        else
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), touchPosition))
            {
                m_targetObject.SetActive(true);
            }
        }
    }
}
