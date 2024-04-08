//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework
{
    public class CloudSaveResolve : MonoBehaviour
    {
        [SerializeField] Button m_UseRemoteButton = default;
        [SerializeField] Button m_UseLocalButton = default;
        [SerializeField] TextMeshProUGUI m_LocalDescription = default;
        [SerializeField] TextMeshProUGUI m_CloudSaveDescription = default;
        CloudSaveScreen m_CloudSaveScreen;

        private string GetDescription(GenericDictionary localMeta)
        {
            TimeSpan playtimeSpan = TimeSpan.FromSeconds(localMeta.GetFloat("playtime", 0f));
            string ret = "Spielzeit: " + playtimeSpan.ToString(@"hh\:mm") + "\n";
            ret += "Speichervorgänge: " + localMeta.GetInt("saveVersion", 0) + "\n\n";

            string date = localMeta.GetString("saveDate", "");
            if(date.Equals(""))
            {
                ret += "(ohne Datum)\n";
            } else
            {
                DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                ret += "Datum: " + parsedDate.ToString("dd.MM.yyyy HH:mm") + "\n";
            }
           
            return ret;
        }

        public void Show(CloudSaveScreen screen, GenericDictionary localMeta, GenericDictionary remoteMeta)
        {
            gameObject.SetActive(true);
            m_LocalDescription.text = GetDescription(localMeta);
            m_CloudSaveDescription.text = GetDescription(remoteMeta);

            ActivateListeners();
            m_CloudSaveScreen = screen;
        }
        void Hide()
        {
            DeActivateListeners();
            m_CloudSaveScreen.DoneShowing();
        }


        private void ActivateListeners()
        {
            m_UseRemoteButton.onClick.AddListener(UseRemote);
            m_UseLocalButton.onClick.AddListener(UseLocal);
        }

        private void DeActivateListeners()
        {
            m_UseRemoteButton.onClick.RemoveListener(UseRemote);
            m_UseLocalButton.onClick.RemoveListener(UseLocal);
        }

        void UseLocal()
        {
            m_CloudSaveScreen.SetResolveResult(false);
            Hide();
        }

        void UseRemote()
        {
            m_CloudSaveScreen.SetResolveResult(true);
            Hide();
        }
    }
}
