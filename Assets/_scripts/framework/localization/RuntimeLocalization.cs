// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Networking;
using OTBT.Framework.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OTBT.Framework.Localization
{
    public class RuntimeLocalization : Singleton<RuntimeLocalization>
    {
        Dictionary<int, RuntimeLocalizedText> m_Translations = new Dictionary<int, RuntimeLocalizedText>();

        public void AddLanguageEntry(int textID, string stringText, int languageID = -1)
        {
            if (languageID < 0) languageID = LocalizationDatabase.baseLanguageID;
            if (!m_Translations.ContainsKey(textID))
                m_Translations.Add(textID, new RuntimeLocalizedText(textID));
            m_Translations[textID].UpdateLanguage(languageID, stringText);
        }

        public async Task<RuntimeLocalizedText> GetLocalized(int textID, bool forceExternalUpdate = false)
        {
            if (m_Translations.ContainsKey(textID))
                return m_Translations[textID];
            if (forceExternalUpdate) 
            {
                if (!m_Translations.ContainsKey(textID)) {
                    await TranslationServer.LoadSingleRuntimeTranslation(textID, false, null, null);
                }
            }
            if (m_Translations.ContainsKey(textID))
                return m_Translations[textID];
            return null;
        }

        public async Task<bool> ContainsTranslation(int textID, int languageID, bool forceExternalUpdate = false)
        {
            if (m_Translations.ContainsKey(textID))
                return await m_Translations[textID].HasTranslation(languageID);
            RuntimeLocalizedText t = await GetLocalized(textID, forceExternalUpdate);
            if(t == null) return false;
            return await t.HasTranslation(languageID);
        }

        public async Task<bool> ContainsID(int textID, bool forceExternalUpdate = false)
        {
            if (m_Translations.ContainsKey(textID))
                return true;
            RuntimeLocalizedText t = await GetLocalized(textID, forceExternalUpdate);
            return t != null;
        }
    }
}