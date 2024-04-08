//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC
using UnityEngine;
using System.Collections.Generic;
using OTBT.Framework.Gameplay;
using OTBT.Framework.Localization;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System;
using OTBT.Framework.UI;
using TMPro;

#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

namespace AC
{

    [System.Serializable]
    public class NotificationAction : Action, ILocalizedTextLink, IVerify
    {
        public override ActionCategory Category => ActionCategory.OTBT;
        public override string Title => "UI/Notification";
        public override string Description => "Shows a notification on screen";

        [SerializeField] string m_MessageText = "";
        [SerializeField] LocalizedTextObject m_LocalizedTextObject = null;
        [SerializeField] Sprite m_Sprite = null;
        [SerializeField] SingleNotificationUI.NotificationType m_NotificationType = SingleNotificationUI.NotificationType.TIMED;

        public LocalizedTextObject localizedText => m_LocalizedTextObject;


        public void SetEditorText(string text)
        {
            m_MessageText = text;
        }
        public bool setAtRuntime => false;
        public void SetAtRuntime(bool setAtRuntime) { }
        public bool hasTextGatherObject => m_LocalizedTextObject != null;
        public string currentEditorText => m_MessageText;
        public void AttemptLocaSystemUpdate()
        {
            if (!hasTextGatherObject && localizedText != null)
            {
                m_LocalizedTextObject = LocalizationDatabase.instance.GetByID(localizedText.textID);
            }
        }
        public void SetLocalizedTextObject(LocalizedTextObject obj)
        {
#if UNITY_EDITOR
            UpdateLocalizedTextObject(obj);
#endif
        }

#if UNITY_EDITOR
        bool m_LocalizationFoldout = false;
        public GameObject gameObject => parentActionListInEditor == null ? null : parentActionListInEditor.gameObject;

        public string creationNote => "";
#endif

        public override float Run()
        {
            DialogueManager.instance.notifications.AddLine(m_MessageText, m_LocalizedTextObject, m_Sprite, m_NotificationType);
            return 0f;
        }

#if UNITY_EDITOR

 
        public void OnDeleteAction()
        {
            if (m_LocalizedTextObject != null)
            {
                if (EditorUtility.DisplayDialog("Remove loca object",
                                        $"You are removing a dialogue action. Shall we also delete the linked loca object?", "Delete", "Do Not Delete"))
                {
                    var path = AssetDatabase.GetAssetPath(m_LocalizedTextObject);
                    m_LocalizedTextObject.CheckBeforeDestruction();
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        public override void ShowGUI(List<ActionParameter> parameters)
        {

            EditorGUILayout.LabelField("Notification icon");
            m_Sprite = (Sprite)EditorGUILayout.ObjectField(m_Sprite, typeof(Sprite), false);
            m_NotificationType = (SingleNotificationUI.NotificationType)EditorGUILayout.EnumPopup("Notification type", m_NotificationType);

            EditorGUILayout.LabelField("Notification Text");
            GUIStyle wrapped = new GUIStyle(EditorStyles.textArea);
            wrapped.wordWrap = true;
            wrapped.stretchHeight = true;
            m_MessageText = EditorGUILayout.TextArea(m_MessageText, wrapped, GUILayout.MinHeight(80), GUILayout.ExpandHeight(true));

            OTBT.Framework.Utils.Editor.EditorUtils.Separator(1);

            m_LocalizationFoldout = LocalizationEditorHelpers.LocalizedTextLinkEditor(this, compact: true, foldout: m_LocalizationFoldout);

            OTBT.Framework.Utils.Editor.EditorUtils.DrawVerify(this);
        }


        public override string SetLabel()
        {
            return "Notification";
        }

        public void UpdateLocalizedTextObject(LocalizedTextObject gather)
        {
            m_LocalizedTextObject = gather;
            EditorUtility.SetDirty(LocalizationDatabase.instance);
            AssetDatabase.SaveAssets();
        }

        public void UpdateAllVoicedInformation()
        {
        }
#endif
        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            checker.Check(!m_MessageText.Equals(""), "No line to be said.", gameObject);
#endif
        }

    }
}
#endif
