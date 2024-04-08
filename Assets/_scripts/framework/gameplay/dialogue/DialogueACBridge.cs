//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC

using AC;
#endif
using OTBT.Framework.Networking;
using Sparrow.Verification;

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace OTBT.Framework.Gameplay
{
    [RequireComponent(typeof(DialogueManager))]
    public class DialogueACBridge : MonoBehaviour, IVerify
    {
#if OTBT_AC
        public DialogueManager manager = null;

        private void OnValidate()
        {
            if(manager == null) manager = GetComponent<DialogueManager>();
        }

        void OnEnable()
        {
            AC.ACEventManager.OnStartConversation += HandleConversationStart;
            AC.ACEventManager.OnEndConversation += HandleConversationEnd;
            AC.ACEventManager.OnStartSpeech_Alt += HandleSpeechStart;
            AC.ACEventManager.OnStopSpeech_Alt += HandleSpeechEnd;
        }

      /*  public void AddACLine(Speech speech)
        {
            bool charFound = false;
            foreach (SpeakingCharacter chara in manager.speakers)
            {
                if (chara == null || chara.acChar == null) continue;
                // has to be via string name because chars are sometimes instantiated and references don't match
                if (chara.acChar.name.Equals(speech.speaker.name))
                {
                    manager.AddLine(chara, speech.OriginalText);
                    charFound = true;
                    break;
                }
            }
            if (!charFound)
                manager.AddLine(null, speech.OriginalText);
        }

        public void EndACLine(Speech speech)
        {
            bool charFound = false;
            foreach (SpeakingCharacter chara in manager.speakers)
            {
                if (chara == null || chara.acChar == null) continue;
                // has to be via string name because chars are sometimes instantiated and references don't match
                if (chara.acChar.name.Equals(speech.speaker.name))
                {
                    manager.EndLine(chara, speech.OriginalText);
                    charFound = true;
                }
            }
            if (!charFound)
                manager.EndLine(null, speech.OriginalText);
        }
      */
        void HandleSpeechEnd(Speech speech)
        {
            //EndACLine(speech);
        }

        void HandleSpeechStart(Speech speech)
        {
            //AddACLine(speech);
        }

        ActiveDialogue m_PlayingDialogue = null;

        public void ShowACChoices(Conversation conv)
        {
            if (conv == null) return;
            m_PlayingDialogue = null;
            List<SingleDialogueChoice> choices = new List<SingleDialogueChoice>();
            for (int i = 0; i < conv.options.Count; i++)
            {
                if (conv.options[i].isOn)
                {
                    SingleDialogueChoice newChoice = new SingleDialogueChoice(conv.options[i].label, i, async (o) =>
                    {
                        manager.HideDialogueChoices();
                        _ = TranslationServer.SendDialogueChoice(conv.guid, o);
                        if (conv.options[o].makePlayerSayThis)
                        {
                            m_PlayingDialogue = DialogueManager.instance.PlayDialogue(DialogueManager.instance.playerTrigger, conv.options[o].label, true, conv.options[o].localizedText);
                            while(m_PlayingDialogue != null && m_PlayingDialogue.isActive)
                            {
                                await Task.Delay(50);
                            }
                        }
                        conv.RunOption(o);


                    });
                    choices.Add(newChoice);
                }
            }
            manager.ShowDialogueChoices(choices);
        }

        void HandleConversationEnd(Conversation conversation)
        {
            manager.HideDialogueChoices();
        }

        void HandleConversationStart(Conversation conversation)
        {
            ShowACChoices(conversation);
        }
#endif

        public void Verify(CheckVerifyInterface checker)
        {
        }
    }
}
