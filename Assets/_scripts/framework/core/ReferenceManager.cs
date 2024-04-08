// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Localization;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using UnityEngine;

namespace OTBT.Framework.Core
{
    public class ReferenceManager : Singleton<ReferenceManager>, IVerify
    {
        [SerializeField] LocalizationDatabase m_LocaDB;

        public LocalizationDatabase locaDB => m_LocaDB == null ? Resources.Load<LocalizationDatabase>("LocaDB") : m_LocaDB;


        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            // prefabs only
            if(gameObject != null && gameObject.scene != null && gameObject.scene.name != null)
             checker.CheckNotNull(m_LocaDB, "LocaDB", this, () => m_LocaDB = Resources.Load<LocalizationDatabase>("LocaDB"));
#endif
        }
    }
}
