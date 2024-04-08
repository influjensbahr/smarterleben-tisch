//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OTBT.Framework.Localization
{
    [CustomEditor(typeof(LocalizeTextUI))]
    public class LocalizeTextUIEditor : Editor
    {
        private LocalizeTextUI smTarget;

        public override void OnInspectorGUI()
        {
            if (smTarget == null) smTarget = target as LocalizeTextUI;
            EditorUtils.BeginColoredEditor();
            if (smTarget.localizedText != null)
                EditorUtils.DrawTinyLogoHeader("Localized Text (ID: " + smTarget.localizedText.textID + ")" + (smTarget.localizedText.needsProcessing ? "*" : ""));
            else
                EditorUtils.DrawTinyLogoHeader("Localized Text");

            EditorUtils.DrawPropertyField(serializedObject, "m_AutoUpdateOnLanguageChange");

            LocalizationEditorHelpers.LocalizedTextLinkEditor(smTarget, true, hideDialogue: true);

            EditorUtils.DrawVerify(smTarget);
            EditorUtils.EndColoredEditor();
        }

        #region Context Menu Additions

        const string k_Path = "CONTEXT/TMP_Text/🧭 Localize";
        const string k_LocalizeAllPath = "OTBT/Localization/Localize All";
        const string k_PushAllPath = "OTBT/Localization/Push all to server";
        const string k_PullAllPath = "OTBT/Localization/Load all from server";
        

        [MenuItem(k_PushAllPath)]
        static void PushAll()
        {
            _ = ProcessAll(true);
        }

        [MenuItem(k_PullAllPath)]
        static void PullAll()
        {
            _ = ProcessAll(false);
        }

        static async Task ProcessAll(bool push = true) {
            string txt = push ? "Push" : "pull";
            if (!EditorUtility.DisplayDialog(txt + " All?", "This action will go through all LocalizeText components and "+txt+" their current content "+(push?"to":"from")+" the server. You might also want to do this for the databases on file.", "Proceed", "Cancel")) return;

            var components = FindObjectsOfType<LocalizeTextUI>();

            Undo.RecordObjects(components, txt + " All");

            for (var index = 0; index < components.Length; index++)
            {
                LocalizeTextUI component = components[index];
                EditorUtility.DisplayProgressBar(txt + "ing Texts...", $"Processing {component.gameObject.name}", (float)index / components.Length);
                if(push) await component.localizedText.PushToServer();
                if (!push) await component.localizedText.LoadFromServer();
            }

            EditorUtility.ClearProgressBar();
        }

        [MenuItem(k_LocalizeAllPath)]
        static void LocalizeAll()
        {
            if (!EditorUtility.DisplayDialog("Localize All?", "This action will add LocalizeText-Components to any valid TMP_Text-Components in the active scene. Some objects might not need to be localized, please check the results after.", "Proceed", "Cancel")) return;

            var components = FindObjectsOfType<TMP_Text>();
            var processedComponents = new List<LocalizeTextUI>();

            Undo.RecordObjects(components, "Localize All");

            for (var index = 0; index < components.Length; index++)
            {
                TMP_Text component = components[index];
                EditorUtility.DisplayProgressBar("Localizing Texts...", $"Localizing {component.gameObject.name}", (float)index / components.Length);
                if (component.gameObject.GetComponent<LocalizeTextUI>() != null) continue;
                var localizeTextComponent = component.gameObject.AddComponent<LocalizeTextUI>();
                processedComponents.Add(localizeTextComponent);
            }

            EditorUtility.ClearProgressBar();
            Console.Clear();

            foreach (var component in processedComponents)
            {
                Debug.Log($"{component.gameObject.name} has been localized. Click to select.", component);
            }
        }

        [MenuItem(k_Path, true)]
        static bool CanBeLocalized(MenuCommand menuCommand)
        {
            var gameObject = ((Component)menuCommand.context).gameObject;
            return gameObject.GetComponent<LocalizeTextUI>() == null;
        }

        [MenuItem(k_Path)]
        static void AddComponentToText(MenuCommand command)
        {
            var target = (Component)command.context;
            if (target.gameObject.GetComponent<LocalizeTextUI>() != null) return;
            Undo.AddComponent<LocalizeTextUI>(target.gameObject);
        }

        #endregion
    }
}
