//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer:Alex Brühl
//

#if OTBT_INK

using System;
using System.Collections.Generic;
using UnityEngine;
using OTBT.Framework.Utils;
using OTBT.Framework.Gameplay;
using OTBT.Framework.Localization;

namespace OTBT.Framework.Ink
{
    public class InkpieceController : Singleton<InkpieceController>
    {
        public Story ActiveStory { get; private set; }

        public event Action<Story> onStoryStart;
        public event Action<Story> onStoryEnd;
        //text + tags
        //public event Action<string, List<string>> onContinueText;
        public event Action<SpeakingCharacter,bool, string, ILocalizedText, AudioClip> onContinueText;
        public event Action<List<SingleDialogueChoice>> onChoicesAvailable;

        public float currentDialogueTime = 0.0f;

        /// <summary>
        /// Start story by providing a valid story file.
        /// </summary>
        /// <param name="json">The text asset containing a JSON representation of an Ink story.</param>
        public void StartStory(TextAsset json)
        {
            ActiveStory = new Story(json.text);
            onStoryStart?.Invoke(ActiveStory);
            onContinueText += DialogueManager.instance.PlayInkDialogue;
            onChoicesAvailable += DialogueManager.instance.ShowDialogueChoices;

            DialogueManager.instance.onLineEnd += DialogueManager.instance.HideDialogueChoices;
            DialogueManager.instance.onLineEnd += UpdateState;
            UpdateState();
        }

        /// <summary>
        /// Called whenever the story state changes; rebuilds the UI
        /// </summary>
        void UpdateState()
        {
            //this is continuing until a choice exists.
            while (ActiveStory.canContinue)
            {
                string text = ActiveStory.Continue();
                text = text.Trim();
                Dictionary<string,string> tags = InkTagExtractor.ExtractTagsFromStory(ActiveStory);
                SpeakingCharacter character = null;
                bool isPlayer = false;
                if(tags.Count > 0)
                {
                    //TODO: Set character here by finding it using the provided "Speaker" tag.
                    //TODO: Also set, if given character is the player
                }
                onContinueText?.Invoke(null,isPlayer, text, null, null);
            }

            int numChoices = ActiveStory.currentChoices.Count;
            if (numChoices > 0)
            {
                List<SingleDialogueChoice> choices = new();
                foreach (var choice in ActiveStory.currentChoices) choices.Add(DialogueInkBridge.InkChoiceToSingleDialogueChoice(choice));
                onChoicesAvailable?.Invoke(choices);
                currentDialogueTime = 1.0f; //If there are choices, we try to stop the dialogue each second. If no choice was made, another second is given.
            }
            else
            {
                currentDialogueTime = 0.0f;
                onStoryEnd?.Invoke(ActiveStory);
            }
        }

        public void MakeChoice(int index)
        {
            ActiveStory.ChooseChoiceIndex(index);
            UpdateState();
        }
    }

}

#endif