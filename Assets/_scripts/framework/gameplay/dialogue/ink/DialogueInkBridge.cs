//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer:Alex Brühl
//

#if OTBT_INK
using OTBT.Framework.Ink;

namespace OTBT.Framework.Gameplay
{
    public class DialogueInkBridge
    {
        public static SingleDialogueChoice InkChoiceToSingleDialogueChoice(Choice inkChoice)
        {
            return new SingleDialogueChoice(inkChoice.text,inkChoice.index,InkpieceController.instance.MakeChoice);
        }
    }
}
#endif