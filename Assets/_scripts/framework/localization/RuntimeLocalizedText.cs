//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using System.Collections.Generic;
using System.Threading.Tasks;
using static OTBT.Framework.Localization.LocalizedTextObject;

namespace OTBT.Framework.Localization
{
    public class RuntimeLocalizedText : ILocalizedText
    {
        int m_LocaID = -1;
        List<Translation> m_Translations = new List<Translation>();
        public List<Translation> translations => m_Translations;

        public RuntimeLocalizedText(int id)
        {
            m_LocaID = (id);
        }

        public Task<string> localizedString => GetTranslation(LocalizationDatabase.instance.currentLanguage);

        public int textID => m_LocaID;

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public async Task<string> GetTranslation(Language language, bool forceExternalUpdate = false)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            string baseLanguage = "";
            foreach (Translation t in m_Translations)
            {
                if (t.languageId == language.id) return t.txt;
                if (t.languageId == LocalizationDatabase.instance.baseLanguage.id)
                    baseLanguage = t.txt;
            }
            return baseLanguage;
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public async Task<bool> HasTranslation(int languageID)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            foreach (Translation t in m_Translations)
            {
                if (t.languageId == languageID) return true;
            }
            return false;
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public async Task<bool> HasTranslation(Language language)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            foreach (Translation t in m_Translations)
            {
                if (t.languageId == language.id) return true;
            }
            return false;
        }

        public void UpdateLanguage(int languageID, string stringText)
        {
            for (int i = 0; i < m_Translations.Count; i++)
            {
                if (m_Translations[i].languageId == languageID)
                {
                    m_Translations[i].SetText(stringText);
                    return;
                }
            }
            m_Translations.Add(new Translation(languageID, stringText));
        }
    }
}