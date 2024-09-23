// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Any class implementing this can be checked for being valid using our project check window
    /// </summary>
    public interface IExtendDefaultEditor
    {
#if UNITY_EDITOR
        public void ExtendDefaultEditor();
#endif
    }
}