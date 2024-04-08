//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using UnityEngine.Events;

namespace OTBT.Framework.Animation
{
    
    public interface IUIAnimationWithCallback <T>
    {
        public void AnimateToState(T target, float time, float delay = 0f, UnityAction callback = null);
        public void ResetState();
        public void SetInverted(bool inverted);
        public float RemainingTime();
    }
}
