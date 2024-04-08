//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using TMPro;
using UnityEngine;
using Sparrow.Verification;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace OTBT.Framework.Utils.DevConsole
{
    public class DevConsoleBehaviour : MonoBehaviour, IVerify
    {
        [SerializeField] CanvasGroup m_CanvasGroup;
        [SerializeField] CanvasGroup m_BackgroundGroup;
        [SerializeField] TMP_InputField m_InputField;
        [SerializeField] TMP_Text m_CommandLog;

        const string k_HelpURL = "https://wiki.beatentrack.games/doc/dev-console-ZFxbxgaxJq";
        const string k_CommandPrefix = "/";

        float m_InterruptedTimeScale;

        DevConsole m_Console;

        void Awake()
        {
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.alpha = 0f;
            m_BackgroundGroup.alpha = .9f;
            m_CommandLog.text = string.Empty;

            m_Console = new DevConsole(k_CommandPrefix);
            m_Console.onLog += PrintLine;

            CreateEssentialCommands();

            m_InputField.onSubmit.AddListener(SendCommand);
        }

        void PrintLine(string line)
        {
            if (line.Length == 0) return;
            m_CommandLog.text += $"\n{line}";
        }

        string ClearLog(string[] args)
        {
            m_CommandLog.text = string.Empty;
            return string.Empty;
        }

        string SetOpacity(string[] args)
        {
            if (args.Length < 1) return $"Current opacity is {m_BackgroundGroup.alpha}.";

            if (!float.TryParse(args[0], out float result))
                return "Invalid parameters.";

            m_BackgroundGroup.alpha = result;
            return $"Set opacity to {m_BackgroundGroup.alpha}";
        }

        string SetFontSize(string[] args)
        {
            if (args.Length < 1) return $"Current font size is {m_CommandLog.fontSize}";

            if (!float.TryParse(args[0], out float result))
                return "Invalid parameters.";

            m_CommandLog.fontSize = result;
            return $"Set font size to {m_CommandLog.fontSize}";
        }

        void CreateEssentialCommands()
        {
            DevConsoleCommand howto = new SimpleConsoleCommand("howto",
                "Opens online documentation for DevConsole.",
                null,
                _ =>
                {
                    Application.OpenURL(k_HelpURL);
                    return "Opening Documentation...";
                });
            DevConsoleCommand clear = new SimpleConsoleCommand("clear",
                "Clear dev console.",
                null,
                ClearLog);
            DevConsoleCommand opacity = new SimpleConsoleCommand("consoleopacity",
                "Sets the dev console opacity.",
                "<opacity:float0-1>",
                SetOpacity);
            DevConsoleCommand close = new SimpleConsoleCommand("close",
                "Close the dev console.",
                null,
                _ =>
                {
                    Close();
                    return string.Empty;
                });
            DevConsoleCommand fontsize = new SimpleConsoleCommand("fontsize",
                "Sets the font size of the console.",
                "<size:float>",
                SetFontSize);
            DevConsoleCommand fov = new SimpleConsoleCommand("fov",
                "Sets the FoV of the camera.",
                "<size:float>",
                args =>
                {
                    Camera.main.fieldOfView = float.Parse(args[0]);
                    return $"Camera FoV set to {args[0]}";
                });
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7)) ToggleConsole();

            if (m_CanvasGroup.alpha <= 0f) return;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_InputField.text = m_Console.TryAutoComplete(m_InputField.text);
                m_InputField.caretPosition = m_InputField.text.Length;
            }

            if (Input.GetKeyUp(KeyCode.UpArrow))
                m_InputField.text = m_Console.GetPrevious();

            if (Input.GetKeyUp(KeyCode.DownArrow))
                m_InputField.text = m_Console.GetNext();
        }

        void SendCommand(string input)
        {
            m_Console.ProcessCommand(input);

            m_InputField.text = string.Empty;
            m_InputField.ActivateInputField();
        }

        void ToggleConsole()
        {
            if (m_CanvasGroup.alpha > 0)
                Close();
            else
                Open();
        }

        void Close()
        {
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.alpha = 0f;
            Time.timeScale = m_InterruptedTimeScale;
        }

        void Open()
        {
            m_InterruptedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            m_CanvasGroup.interactable = true;
            m_CanvasGroup.alpha = 1f;
            m_InputField.ActivateInputField();
        }

        public void Verify(CheckVerifyInterface verify)
        {
            verify.CheckNotNull(m_CanvasGroup, nameof(m_CanvasGroup), this);
            verify.CheckNotNull(m_BackgroundGroup, nameof(m_BackgroundGroup), this);
            verify.CheckNotNull(m_InputField, nameof(m_InputField), this);
            verify.CheckNotNull(m_CommandLog, nameof(m_CommandLog), this);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Disables the console prior to building non-development builds.
    /// </summary>
    class DisableConsoleBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.options.HasFlag(BuildOptions.Development))
                return;

            DevConsoleBehaviour console = GameObject.FindObjectOfType<DevConsoleBehaviour>();
            if (console) console.enabled = false;
        }
    }
#endif
}
