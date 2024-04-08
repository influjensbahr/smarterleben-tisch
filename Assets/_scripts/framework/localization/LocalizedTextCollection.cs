//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Localization
{
    [CreateAssetMenu(menuName = "OTBT/Localization/Localized Text Collection", fileName = "LocalizedText Collection", order = 0)]
    public class LocalizedTextCollection : ScriptableObjectCollection<LocalizedTextObject>
    {
        public List<LocalizedTextObject> textObjects => m_Objects;

        public LocalizedTextObject GetByID(int id)
        {
            foreach (LocalizedTextObject t in textObjects)
                if (t.textID.Equals(id))
                    return t;
            return null;
        }
    }
}