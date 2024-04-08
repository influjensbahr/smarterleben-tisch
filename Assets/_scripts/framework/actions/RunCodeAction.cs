//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

#if OTBT_AC
using UnityEngine;
using AC;
#if UNITY_EDITOR
using OTBT.Framework.Utils.Editor;
using UnityEditor;
#endif

[System.Serializable]
public class RunCodeAction : Action
{
    public override ActionCategory Category => ActionCategory.OTBT;
    public override string Title => "Utils/Runs Code and waits for Callback";
    public override string Description => "Prints text to the console.";


    [SerializeField] ActionCallback m_CodeToRun;


#if UNITY_EDITOR
    public override void ShowGUI()
    {
        base.ShowGUI();
        m_CodeToRun = EditorGUILayout.ObjectField(m_CodeToRun, typeof(ActionCallback), true) as ActionCallback;
    }
    #endif

    public override float Run()
    {
        if (isRunning)
            return defaultPauseTime;

        if (m_CodeToRun == null)
        {
            Debug.LogWarning("No code to run specified, will skip", this);
            return 0f;
        }

        isRunning = true;
        willWait = true;
        m_CodeToRun.Run(this);
        return defaultPauseTime;
    }

    public void Finish()
    {
        isRunning = false;
    }
}
#endif
