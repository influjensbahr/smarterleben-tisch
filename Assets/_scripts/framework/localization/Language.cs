// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using System;

namespace OTBT.Framework.Localization
{
    public enum TTSLocale
    {
        de_DE, ar_SA, cs_CZ, da_DK, el_GR, en_AU, en_GB, en_IE, en_US, en_ZA, es_ES, es_MX, fi_FI, fr_CA, fr_FR, he_IL, hi_IN, hu_HU, id_ID, it_IT, ja_JP, ko_KR, nl_BE, nl_NL, no_NO, pl_PL, pt_BR, pt_PT, ro_RO, ru_RU, sk_SK, sv_SE, th_TH, tr_TR, zh_CN, zh_HK, zh_TW
    }

    [Serializable]
    public class Language
    {
        public int id;
        public string caption;
        public bool rightToLeft;
        public bool correctForArabic;
        public bool isHidden;
        public bool isBaseLanguage;
        public string systemLanguageKey;
        public TTSLocale ttsLocale;
        public string languageShortCode;

        // for Hogrefe, remove at some point from framework
        public string testformQtiId;
        public string productId;

        public Language(int i, string t)
        {
            id = i;
            caption = t;
            rightToLeft = false;
            correctForArabic = false;
            isHidden = false;
            isBaseLanguage = false;
            systemLanguageKey = "English";
            ttsLocale = TTSLocale.en_US;
            languageShortCode = "en_GB";
            testformQtiId = "lji2standard";
            productId = "256193";
        }

        public void SetTo(Language l)
        {
            this.id = l.id;
            this.caption = l.caption;
            this.rightToLeft = l.rightToLeft;
            this.correctForArabic = l.correctForArabic;
            this.isHidden = l.isHidden;
            this.isBaseLanguage = l.isBaseLanguage;
            this.systemLanguageKey = l.systemLanguageKey;
            this.ttsLocale = l.ttsLocale;
            this.languageShortCode = l.languageShortCode;
            this.testformQtiId = l.testformQtiId;
            this.productId = l.productId;
        }
    }
}
