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
    public interface ILocalizedText
    {
        public Task<string> GetTranslation(Language language, bool forceExternalUpdate = false);
        public Task<bool> HasTranslation(Language language);
        public void UpdateLanguage(int languageID, string stringText);
        public Task<string> localizedString { get; }
        public int textID { get; }
        public List<Translation> translations{ get; }
    }
}