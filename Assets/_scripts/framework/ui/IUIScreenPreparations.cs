// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using System.Threading.Tasks;
using UnityEngine.Events;

namespace OTBT.Framework.UI
{
    public interface IUIScreenPreparations
    {
        public void PrepareShow(UnityAction callback);
        public void PrepareHide(UnityAction callback);
    }
}
