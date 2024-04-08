using System;
using TMPro;
using UnityEngine;
using OTBT.Framework.Core;

public class OTBT_UITextField : OTBT_TextFieldWithGameSettingsBase {
    // stuff for text display
    internal int localizationID;
    public string textOverride;
    internal float textFieldFontSize;
    public TextMeshProUGUI textField;

    // stuff for alpha lerp
    public RectTransform myTransform;
    public CanvasGroup canvasGroup;
    private float visibility = 0f;
    public float LerpSpeed = 0.4f;

    // stuff for positioning
    public float DistanceBetween = 30f;
    private RectTransform imageRightFromMe;
    private float targetXLerp = 0;
    private Vector2 targetPositio, startPosition;

    public bool instantActive = false;
    private bool isRight = false;

    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = instantActive ? 1f : 0f;
        visibility = canvasGroup.alpha;

        textFieldFontSize = textField.fontSize;
    }

    public void UpdateFontSize(int delta) {
        //textField.fontSize = textFieldFontSize + (delta - 5) * 2 - (OTBT_GameManager.instance.settings.ActiveSubtitleFont.GetValue() == GameSettings.SubtitleFont.DYSLEXIC ? 6 : 0);
    }

    public override void AfterCurrentSetupUpdate() {
        UpdateDisplay();
    }

    /// <summary>
    /// Updates the text display of this text field
    /// </summary>
    public virtual void UpdateDisplay() {
   //     textField.text = OTBT_GameManager.instance.localization.GetText(localizationID);
    }

    // ------------------------------------ POSITIONINGS --------------------------------

    /// <summary>
    /// Sets the controls right of this control, and will start the process of slowly moving into our new position
    /// </summary>
    /// <param name="instant">If true, will not lerp over but teleport</param>
    /// <param name="target">Target transform, this is the leftmost point of the element right to us, i.e. the left border of the control icon</param>
    public void SetTransformNextToMe(RectTransform target, bool _isRight = true, bool instant = false) {
        imageRightFromMe = target;
        this.isRight = _isRight;
        UpdateTargetPosition();

        if (instant) {
            myTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPositio, 1f);
        } else {
            targetXLerp = 0f;
            myTransform.anchoredPosition = Vector2.zero;
            startPosition = myTransform.anchoredPosition;
            //EventManager.instance.StartListening(EventManager.EventType.UNSAFE_UPDATE, PositionLerp);
        }
    }

    private void UpdateTargetPosition() {
        targetPositio = imageRightFromMe == null ? Vector2.zero : new Vector2(
            isRight ? (transform.parent.InverseTransformPoint(imageRightFromMe.position).x - DistanceBetween) : 0,
            isRight ? 0 : (transform.parent.InverseTransformPoint(imageRightFromMe.position).y + DistanceBetween));
    }

    public void PositionLerp() {
        targetXLerp += Time.unscaledDeltaTime * LerpSpeed;
        UpdateTargetPosition();
        myTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPositio, targetXLerp);
        if (targetXLerp > 1f) {
           // EventManager.instance.StopListening(EventManager.EventType.UNSAFE_UPDATE, PositionLerp);
        }
    }


    // --------------------------------------- FADES ----------------------------
    public virtual void FadeOutDone() { }
    public virtual void FadeInDone() { }

    public bool fadingDown = false;

    /// <summary>
    /// Starts fading this object out until it's hidden!
    /// </summary>
    public void StartFadeOut(float _targetAlpha = 0f) {
        fadingDown = true;
        //EventManager.instance.StopListening(EventManager.EventType.UNSAFE_UPDATE, FadeUp);
        //EventManager.instance.StartListening(EventManager.EventType.UNSAFE_UPDATE, FadeDown);
    }

    private void FadeDown() {
        visibility = Mathf.Max(visibility - Time.unscaledDeltaTime * LerpSpeed, 0f);
        canvasGroup.alpha = visibility;
        if (visibility <= 0f) {
            FadeOutDone();
            fadingDown = false;
            //EventManager.instance.StopListening(EventManager.EventType.UNSAFE_UPDATE, FadeDown);
        }
    }

    public void StartAtZero() {
        visibility = 0f;
    }

    /// <summary>
    /// Starts fading this object in until it's visible!
    /// </summary>
    public void StartFadeIn() {
        fadingDown = false; 
        //EventManager.instance.StopListening(EventManager.EventType.UNSAFE_UPDATE, FadeDown);
        //EventManager.instance.StartListening(EventManager.EventType.UNSAFE_UPDATE, FadeUp);
    }

    private void FadeUp() {
        visibility = Mathf.Min(visibility + Time.unscaledDeltaTime * LerpSpeed, 1f);
        canvasGroup.alpha = visibility;
        if (visibility >= 1f) {
            FadeInDone();
            //EventManager.instance.StopListening(EventManager.EventType.UNSAFE_UPDATE, FadeUp);
        }
    }
}
