//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Animation;
using OTBT.Framework.Core;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using System.Threading.Tasks;
using UnityEngine;

namespace OTBT.Framework
{
    public class CloudSaveScreen : Singleton<CloudSaveScreen>, IVerify
    {
        [SerializeField] CloudSaveUnreachable m_Unreachable = default;
        [SerializeField] CloudSaveResolve m_Resolve = default;
        [SerializeField] CloudSaveOptions m_Options = default;
        [SerializeField] CloudSaveDecryptionFailed m_DecryptionFailed = default;
        [SerializeField] CloudSaveUnreachable m_Info = default;

        [SerializeField] UICanvasGroupFade m_CanvasGroupFade = default;
        [SerializeField] UICanvasGroupFade m_ErrorIcon = default;

        bool m_DoneShowing = false;
        bool m_ResolveUseCloudSave = false;
        bool m_DecryptionAborted = false;
        string m_DecryptionCode = "";

        public bool resolveResult => m_ResolveUseCloudSave;
        public bool decryptionAborted => m_DecryptionAborted;
        public string decryptionCode => m_DecryptionCode;


        private void Start()
        {
            m_CanvasGroupFade.AnimateToState(UIAnimationStateLerper.START, 0f, callback: DeactivateAll);
            HideSaveErrorIcon(0f);
        }

        void StartShowing()
        {
            m_DoneShowing = false;
            m_CanvasGroupFade.AnimateToState(UIAnimationStateLerper.SHOWN, 1.5f);
        }

        public void DoneShowing()
        {
            m_DoneShowing = true;
            m_CanvasGroupFade.AnimateToState(UIAnimationStateLerper.START, 1.5f, callback: DeactivateAll);
        }

        void DeactivateAll()
        {
            m_Unreachable.gameObject.SetActive(false);
            m_Resolve.gameObject.SetActive(false);
            m_Options.gameObject.SetActive(false);
            m_DecryptionFailed.gameObject.SetActive(false);
            m_Info.gameObject.SetActive(false);
        }

        public void Success()
        {
            HideSaveErrorIcon(0f);
        }

        public async Task ShowInfo()
        {
            m_Info.Show(this);
            StartShowing();
            while (!m_DoneShowing) await Task.Delay(50);
        }

        public async Task Unreachable()
        {
            //m_Unreachable.Show(this);
            //StartShowing();
            ShowSaveErrorIcon();
            //while (!m_DoneShowing) await Task.Delay(50);
            await Task.Delay(1);
        }

        public async Task Resolve(GenericDictionary localMeta, GenericDictionary remoteMeta)
        {
            m_Resolve.Show(this, localMeta, remoteMeta);
            StartShowing();
            while (!m_DoneShowing) await Task.Delay(50);
        }

        public async Task Options(string id)
        {
            m_Options.Show(this, id);
            StartShowing();
            while (!m_DoneShowing) await Task.Delay(50);
        }

        public async Task DecryptionFailed(string testString)
        {
            m_DecryptionAborted = false; 
            m_DecryptionFailed.Show(this, testString);
            StartShowing();
            while (!m_DoneShowing) await Task.Delay(50);
        }

        public void SetOptionsResult(bool cloudSaveActive, string code)
        {
            SaveGame.instance.SetCloudSaveStatus(cloudSaveActive);
            SaveGame.instance.SetServerEncryption(code);
        }

        public void SetDecryptionResult(bool decryptionAborted, string code)
        {
            m_DecryptionAborted = decryptionAborted;
            m_DecryptionCode = code;
            SaveGame.instance.SetServerEncryption(code);
        }

        public void SetResolveResult(bool useCloudSave)
        {
            m_ResolveUseCloudSave = useCloudSave;
        }
        
        public void ShowSaveErrorIcon()
        {
            m_ErrorIcon.AnimateToState(UIAnimationStateLerper.SHOWN, 1f, 0f, null);
        }

        public void HideSaveErrorIcon(float delay = 2f)
        {
            EventManager.instance.TriggerInTime(delay, () => m_ErrorIcon.AnimateToState(UIAnimationStateLerper.START, 1f));
        }

        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
#endif
        }

    }
}
