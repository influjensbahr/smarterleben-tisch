// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

namespace OTBT.Framework.Utils
{
    public interface IScriptableObjectCollection 
    {
#if UNITY_EDITOR
        void RefreshList();
        string GetTypeName();
#endif
    }
}
