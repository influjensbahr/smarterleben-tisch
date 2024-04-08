//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine;

namespace OTBT.Framework.Localization
{
    public interface ILocalizedTextLink 
    {
        public LocalizedTextObject localizedText { get; }
        public bool setAtRuntime { get; }
        public bool hasTextGatherObject { get; }
        public void SetAtRuntime(bool setAtRuntime);
        public void SetLocalizedTextObject(LocalizedTextObject obj);

#if UNITY_EDITOR
        public void SetEditorText(string text);
        public string currentEditorText { get; }
        public string creationNote { get; }
        public GameObject gameObject { get; }
        public void AttemptLocaSystemUpdate();
        public void UpdateAllVoicedInformation();
#endif
    }
}
