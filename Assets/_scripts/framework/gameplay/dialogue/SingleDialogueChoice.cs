// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using UnityEngine.Events;

namespace OTBT.Framework.Gameplay
{
    public class SingleDialogueChoice 
    {
        public int optionID = 0;
        public string lineText = "";
        public UnityAction<int> onSelectionCallback;

        public SingleDialogueChoice(string line, int id, UnityAction<int> callback)
        {
            lineText = line;
            optionID = id;
            onSelectionCallback = callback;
        }
    }
}
