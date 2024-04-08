using System;
using UnityEngine;
using OTBT.Framework.Core;
/// <summary>
/// This class handles the listening to various game settings
/// </summary>
public class OTBT_TextFieldWithGameSettingsBase : MonoBehaviour
{
    internal readonly string SpeakerColorPreset = "<color=#{0}>{1}</color>";
    internal readonly string SpeakerBold = "{0}";//"<b>{0}</b>";

    internal static readonly string BlackBackground = "<mark=#000000ff padding=\"30, 30, 25,25\">{0}</mark>";
    internal static readonly string TintedBackground = "<mark=#00000055 padding=\"30, 30, 25,25\">{0}</mark>";

    internal static readonly string FontRegular = "<font=\"FjallaOne-Regular SDF\">{0}</font>";
    internal static readonly string FontClear = "<font=\"Roboto-Regular SDF\">{0}</font>";
    internal static readonly string FontDyslexic = "<font=\"OpenDyslexic3-Regular SDF\">{0}</font>";

    // settings buffer
    internal bool SpeakerName = true;
    internal bool SpeakerTone = true;
    internal bool ShowSubtitles = true;
    internal string CurrentSetup = "";
    internal SubtitleSpeakerColor SpeakerColor = SubtitleSpeakerColor.NAME_ONLY;
    internal int fontSize = 5;

    internal void SetupListeners(bool font, bool bg, bool speakName, bool speakColor, bool speakTone, bool closedCap, bool subtitle, bool subtitlesize) {
        /* OTBT_GameManager.WhenReady(() => {
             if (font) OTBT_GameManager.instance.settings.ActiveSubtitleFont.onValueChange += UpdateCurrentSetup;
             if (bg) OTBT_GameManager.instance.settings.ActiveSubtitleBackground.onValueChange += UpdateCurrentSetup;
             if (speakName) OTBT_GameManager.instance.settings.DisplaySpeakerName.onValueChange += UpdateCurrentSetup;
             if (speakColor) OTBT_GameManager.instance.settings.DisplaySpeakerColor.onValueChange += UpdateCurrentSetup;
             if (speakTone) OTBT_GameManager.instance.settings.DisplaySpeakerTone.onValueChange += UpdateCurrentSetup;

             if (subtitle) OTBT_GameManager.instance.settings.Subtitles.onValueChange += UpdateCurrentSetup;
             if (subtitlesize) OTBT_GameManager.instance.settings.SubtitleSize.onValueChange += UpdateCurrentSetup;
            
        // also listen for language changes
        // OTBT_GameManager.instance.localization.onLanguageChange += UpdateLanguage;

        //  fontSize = OTBT_GameManager.instance.settings.SubtitleSize.GetIntValue();
        UpdateCurrentSetup(1);
            UpdateLanguage(1);
        });*/
    }

    internal void UpdateCurrentSetup(int i) {
        // update our display status
     /*   SpeakerName =   OTBT_GameManager.instance.settings.DisplaySpeakerName.GetValue() == GameSettings.OnOff.ON;
        SpeakerTone =   OTBT_GameManager.instance.settings.DisplaySpeakerTone.GetValue() == GameSettings.OnOff.ON;
        SpeakerColor =  OTBT_GameManager.instance.settings.DisplaySpeakerColor.GetValue();
        ShowSubtitles = OTBT_GameManager.instance.settings.Subtitles.GetValue() == GameSettings.OnOff.ON;
        fontSize = OTBT_GameManager.instance.settings.SubtitleSize.GetIntValue();

        // setting for the font, which surrounds all other textx
        string font = "{0}";
        switch (OTBT_GameManager.instance.settings.ActiveSubtitleFont.GetValue()) {
            case GameSettings.SubtitleFont.DYSLEXIC:
                font = FontDyslexic;
                break;
            case GameSettings.SubtitleFont.REGULAR:
                font = FontRegular;
                break;
            case GameSettings.SubtitleFont.CLEAR:
                font = FontClear;
                break;
        }

        // set the background color of the text
        string background = "{0}";
        switch (OTBT_GameManager.instance.settings.ActiveSubtitleBackground.GetValue()) {
            case GameSettings.SubtitleBackground.BLACK:
                background = BlackBackground;
                break;
            case GameSettings.SubtitleBackground.TINT:
                background = TintedBackground;
                break;
        }

        // update text to show the current values
        CurrentSetup = string.Format(font, background);
     */
        AfterCurrentSetupUpdate();
    }

    internal string GetSetupWithoutBackground() {
        // setting for the font, which surrounds all other textx
        string font = "{0}";
      /*  switch (OTBT_GameManager.instance.settings.ActiveSubtitleFont.GetValue()) {
            case GameSettings.SubtitleFont.DYSLEXIC:
                font = FontDyslexic;
                break;
            case GameSettings.SubtitleFont.REGULAR:
                font = FontRegular;
                break;
            case GameSettings.SubtitleFont.CLEAR:
                font = FontClear;
                break;
        }
      */
        return string.Format(font, "{0}");
    }

    private void UpdateLanguage(int language) {
        AfterLanguageUpdate();
    }

    public virtual void AfterCurrentSetupUpdate() { }
    public virtual void AfterLanguageUpdate() { 
        AfterCurrentSetupUpdate();  
    }
}
