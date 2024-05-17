using OTBT.Framework.Core;
using OTBT.Framework.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIContoller : Singleton<GUIContoller>
{
    [SerializeField] GameObject m_CrushPrefab = default;

    [SerializeField] Transform m_OrderCrushers = default;
    [SerializeField] Transform m_HealthCrushers = default;
    [SerializeField] Transform m_CrewCrushers = default;
    [SerializeField] Transform m_NatureCrushers = default;

    [SerializeField] Image m_OrderValueBar;
    [SerializeField] Image m_ShipHealthBar;
    [SerializeField] Image m_CrewHealthBar;
    [SerializeField] Image m_BiodomeIntegrityBar;

    [SerializeField] CanvasGroup m_Buttons = default;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI m_ScenarioText = default;
    [SerializeField] CanvasGroup m_CanvasGroup = default;
    [SerializeField] private AudioClip criticalSound;
    [SerializeField] private AudioSource audioSource;

    bool fadingOut = false;
    bool fadingIn = false;

    private void Awake()
    {
        EventManager.instance.StartListening("valueUpdate", SetOrderBar);
        m_CanvasGroup.alpha = 0f;
        m_Buttons.alpha = 0f;
    }

    public void SetDisplay(SpaceshipEvent evt)
    {
        Debug.Log("SET DISPLAY: " + evt.m_Title);
        m_ScenarioText.text = "";
        m_Buttons.alpha = 0f;
        m_CanvasGroup.alpha = 0f;
        StartCoroutine(FadeInCanvas());
        StartCoroutine(TypingEffect(evt.m_Title, evt.m_Text));
    }

    private void HideScenario()
    {
        StartCoroutine(FadeOutCanvas());
    }

    private IEnumerator FadeInCanvas()
    {
        fadingIn = true;
        while (m_CanvasGroup.alpha < 1f && !fadingOut)
        {
            m_CanvasGroup.alpha += Time.deltaTime * 1.5f; // Dauer: 1 Sekunde
            yield return null;
        }
        fadingIn = false;
        m_CanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutCanvas()
    {
        fadingOut = true;
        while (m_CanvasGroup.alpha > 0f && !fadingIn)
        {
            m_CanvasGroup.alpha -= Time.deltaTime * 1.5f; // Dauer: 1 Sekunde
            yield return null;
        }
        fadingOut = false;
        m_CanvasGroup.alpha = 0f;
    }

    private static float TypingSpeed = 0.02f;

    private IEnumerator TypingEffect(string title, string text)
    {
        m_ScenarioText.text = "<b>" + title + "</b><br>" + text;
        EventManager.instance.TriggerInTime(m_ScenarioText.text.Length * TypingSpeed * 0.85f, () => StartCoroutine(FadeInButtons()));
        
        for (int i = 0; i <= m_ScenarioText.text.Length; i++)
        {
            m_ScenarioText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(TypingSpeed); // Anpassbare Tipp-Geschwindigkeit
        }
    }

    private IEnumerator FadeInButtons()
    {
        while (m_Buttons.alpha < 1f)
        {
            m_Buttons.alpha += Time.deltaTime / 1f; // Dauer: 1 Sekunde
                    yield return null;
        }
        m_Buttons.alpha = 1f;
    }

    public void YesButtonPressed()
    {
        SpaceshipController.instance.YesButton();
        HideScenario();
    }

    public void NoButtonPressed()
    {
        SpaceshipController.instance.NoButton();
        HideScenario();
    }

    private void SetOrderBar()
    {
        CheckAndAnimateCritical(m_OrderValueBar, SpaceshipController.instance.order == 0 ? 0 : SpaceshipController.instance.order / 10f, SpaceshipController.instance.orderCriticals, m_OrderCrushers, "order");
        CheckAndAnimateCritical(m_ShipHealthBar, SpaceshipController.instance.technology == 0 ? 0 : SpaceshipController.instance.technology / 10f, SpaceshipController.instance.technologyCriticals, m_HealthCrushers, "technology");
        CheckAndAnimateCritical(m_CrewHealthBar, SpaceshipController.instance.people == 0 ? 0 : SpaceshipController.instance.people / 10f, SpaceshipController.instance.peopleCriticals, m_CrewCrushers, "people");
        CheckAndAnimateCritical(m_BiodomeIntegrityBar, SpaceshipController.instance.nature == 0 ? 0 : SpaceshipController.instance.nature / 10f, SpaceshipController.instance.natureCriticals, m_NatureCrushers, "nature");
    }

    private void CheckAndAnimateCritical(Image bar, float targetAmount, int criticals, Transform crusherParent, string type)
    {
        if (targetAmount < 0f || targetAmount > 1f)
        {
            StartCoroutine(AnimateCritical(bar, targetAmount < 0 ? 0f : 1f, crusherParent, criticals));
        }
        else
        {
            StartCoroutine(AnimateFillAmount(bar, targetAmount));
        }
    }
    private IEnumerator AnimateFillAmount(Image bar, float targetAmount)
    {
        float startAmount = bar.fillAmount;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            bar.fillAmount = Mathf.Lerp(startAmount, targetAmount, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        bar.fillAmount = targetAmount;
    }

    private IEnumerator AnimateCritical(Image bar, float edgeAmount, Transform crusherParent, int maxCriticals)
    {
        yield return AnimateFillAmount(bar, edgeAmount);

        audioSource.PlayOneShot(criticalSound);
        SpaceshipController.instance.ActivateCriticals();

        StartCoroutine(BlinkBar(bar));
    }
    private IEnumerator BlinkBar(Image bar)
    {
        float blinkDuration = 3f; // Gesamtdauer des Blinkens
        float blinkInterval = 0.4f; // Intervalle in denen Farbe wechselt
        float elapsed = 0f;

        Color originalColor = bar.color;
        Color blurpColor = Color.Lerp(Color.black, bar.color, 0.5f);

        while (elapsed < blinkDuration)
        {
            bar.color = bar.color == blurpColor ? originalColor : blurpColor;
            elapsed += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        bar.color = originalColor; // Nach dem Blinken dauerhaft rot
    }
}
