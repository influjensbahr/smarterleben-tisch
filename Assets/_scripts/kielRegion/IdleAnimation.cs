using System.Collections;
using DG.Tweening;
using TuioNet.Tuio11;
using UnityEngine;
using UnityEngine.UI;

public class IdleAnimation : MonoBehaviour
{
    [SerializeField] Image whiteBackground;
    [SerializeField] Image logo;
    [SerializeField] GameObject idleScreen;
    [SerializeField] float idleTime = 5f;
    [SerializeField] float logoSpeed = 20f;

    float lastInputTime;
    bool isIdle;
    Vector3 direction;

    void Start()
    {
        whiteBackground.gameObject.SetActive(false);
        logo.gameObject.SetActive(false);
        lastInputTime = Time.time;
        isIdle = false;
        direction = GetRandomDirection();
    }

    void OnEnable()
    {
        CustomTuio11Visualizer.onCursorAdd += OnInputDetected;
        CustomTuio11Visualizer.onCursorUpdate += OnInputDetected;
        CustomTuio11Visualizer.onObjectAdd += OnInputDetected;
        CustomTuio11Visualizer.onObjectUpdate += OnInputDetected;
    }

    void OnDisable()
    {
        CustomTuio11Visualizer.onCursorAdd -= OnInputDetected;
        CustomTuio11Visualizer.onCursorUpdate -= OnInputDetected;
        CustomTuio11Visualizer.onObjectAdd -= OnInputDetected;
        CustomTuio11Visualizer.onObjectUpdate -= OnInputDetected;
    }

    void Update()
    {
        if (Time.time - lastInputTime >= idleTime && !isIdle)
        {
            StartCoroutine(ShowIdleScreen());
        }
    }

    void OnInputDetected(Tuio11Cursor cursor)
    {
        lastInputTime = Time.time;
        if (isIdle)
        {
            StopCoroutine(ShowIdleScreen());
            HideIdleScreen();
        }
    }

    void OnInputDetected(Tuio11Object obj)
    {
        lastInputTime = Time.time;
        if (isIdle)
        {
            StopCoroutine(ShowIdleScreen());
            HideIdleScreen();
        }
    }

    IEnumerator ShowIdleScreen()
    {
        isIdle = true;
        idleScreen.transform.SetAsLastSibling();
        whiteBackground.gameObject.SetActive(true);
        whiteBackground.color = new Color(1, 1, 1, 0.5f);
        logo.gameObject.SetActive(true);

        while (isIdle)
        {
            Vector3 targetPosition = logo.rectTransform.position + direction * (logoSpeed * Time.deltaTime);

            if (targetPosition.x <= 0 || targetPosition.x >= Screen.width - logo.rectTransform.rect.width)
            {
                direction.x = -direction.x;
                direction = direction.normalized;
            }

            if (targetPosition.y <= 0 || targetPosition.y >= Screen.height - logo.rectTransform.rect.height)
            {
                direction.y = -direction.y;
                direction = direction.normalized;
            }

            logo.rectTransform.position = Vector3.Lerp(logo.rectTransform.position, targetPosition, Time.deltaTime * logoSpeed);

            yield return null;
        }
    }

    void HideIdleScreen()
    {
        isIdle = false;
        whiteBackground.gameObject.SetActive(false);
        logo.gameObject.SetActive(false);
    }

    Vector3 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
    }
}
