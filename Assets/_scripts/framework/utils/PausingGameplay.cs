// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Core;

namespace OTBT.Framework.Utils
{
    public abstract class PausingGameplay { 
        public void RegisterPauseEvents()
        {
            EventManager.instance.StartListening(EventManager.pauseGameEvent.identifier, OnPause);
            EventManager.instance.StartListening(EventManager.unpauseGameEvent.identifier, OnResume);
        }

        public void UnRegister()
        {
            EventManager.instance.StopListening(EventManager.pauseGameEvent.identifier, OnPause);
            EventManager.instance.StopListening(EventManager.unpauseGameEvent.identifier, OnResume);
        }

        public abstract void OnPause();
        public abstract void OnResume();
        
    }
}
