//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//


using UnityEngine;
using System.Collections.Generic;
using System;
using OTBT.Framework.UI;
using Sparrow.Verification;
using UnityEditor;
using UnityEngine.SceneManagement;
using OTBT.Framework.Utils;
using System.Threading.Tasks;
using OTBT.Framework.Debugging;
using UnityEngine.Events;
using OTBT.Framework.Gameplay;

namespace OTBT.Framework.Core
{
    /// <summary>
    /// Manager for loading scene collections - at runtime and in editor
    /// </summary>
    public class SceneLoadManager : OTBT.Framework.Utils.Singleton<SceneLoadManager>, IVerify
    {
        SceneCollection m_CurrentScene;

        [SerializeField] UnityEvent m_OnSceneSwitchBlack = default;

        [Header("Scene Assets")]
        [SerializeField] SceneReference m_BaseScene = default;
        [SerializeField] SceneReference m_ReloadScene = default;

        [SerializeField] SceneCollectionCollection m_SceneDefinitonCollection = default;
        [SerializeField, HideInInspector] List<SceneCollection> m_SceneDefinitions = new List<SceneCollection>();
        [SerializeField] SceneCollection m_FirstSceneInBuild;



        SceneCollection m_SceneFadeTarget;
        List<AsyncOperation> m_LoadOperations = new List<AsyncOperation>();

        public List<SceneCollection> scenes => m_SceneDefinitonCollection == null ? m_SceneDefinitions : m_SceneDefinitonCollection.sceneCollections;
        public SceneReference baseScene => m_BaseScene;
        public SceneReference reloadScene => m_ReloadScene;
        public SceneCollection currentScene => m_CurrentScene;

        public event Action onSceneStarted;

        protected override void InitializeInherit() { }


        void Start()
        {

#if !UNITY_EDITOR
            if (m_FirstSceneInBuild != null && m_FirstSceneInBuild.objectRef != null) {
                SwitchToScene(m_FirstSceneInBuild.objectRef, false);
            } else {
                if (m_LoadOperations.Count == 0) SceneStarted();
            }
#else            
            if (m_LoadOperations.Count == 0) SceneStarted();
            if (m_FirstSceneInBuild != null && m_FirstSceneInBuild.objectRef != null)
                Dbg.Log(this, "Ignoring FirstSceneInBuild ("+m_FirstSceneInBuild+") because we are playing in editor.");
#endif
        }

        public SceneCollection GetSceneDefinition(StringOrAtomReference<SceneAtom> sceneRef)
        {
            return GetSceneDefinition(sceneRef.ToString());
        }

        public void TryReloadGame()
        {
            foreach (GameObject g in FindObjectsOfType<GameObject>())
                if (g.transform.parent == null)
                    Destroy(g);
            SceneManager.LoadSceneAsync(reloadScene.SceneName, LoadSceneMode.Single);
            ErrorLogManager.instance.AddLog($"Trying to reload game");
        }

        public SceneCollection GetSceneDefinition(string sceneRef)
        {
            foreach (SceneCollection sd in scenes)
                if (sd.sceneTitle.Equals(sceneRef, StringComparison.OrdinalIgnoreCase))
                    return sd;
            return null;
        }

        public void SwitchToScene(StringOrAtomReference<SceneAtom> sceneNum, bool fadeToBlack = true, Func<Task> FunctionToPerformWhileScreenIsBlack = null)
        {
            m_SceneFadeTarget = GetSceneDefinition(sceneNum); 
            ErrorLogManager.instance.AddLog($"Switching to scene: {sceneNum.identifier}");

            Action fadeAction = async () =>
            {
#if OTBT_AC
                AC.KickStarter.TurnOffAC();
#endif
                // unload all current scenes
                SceneCollection unloads = m_CurrentScene;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.path == m_BaseScene.ScenePath) continue;
                    AsyncOperation ao = SceneManager.UnloadSceneAsync(scene);
                    m_LoadOperations.Add(ao);
                }

                // kill all current dialogue
                DialogueManager.instance.SkipPressed();
                DialogueManager.instance.HideDialogueChoices();

                // load all upcoming scenes
                if (m_SceneFadeTarget != null)
                {
                    foreach (SceneReference sr in m_SceneFadeTarget.scenesToLoad)
                    {
                        AsyncOperation aso = SceneManager.LoadSceneAsync(sr.SceneName, LoadSceneMode.Additive);
                        m_LoadOperations.Add(aso);
                    }
                } else
                {
                    Debug.LogError($"Scene definition not found for: {m_SceneFadeTarget}");
                }

                if(FunctionToPerformWhileScreenIsBlack != null)
                    await FunctionToPerformWhileScreenIsBlack();

                m_OnSceneSwitchBlack?.Invoke();

                if (Application.isPlaying)
                    EventManager.instance.TriggerEvent(EventManager.changeSceneEvent);
                
                // wait for all them scene loadings!
                bool loadingDone = false;
                while (!loadingDone)
                {
                    loadingDone = true;
                    float progress = 0f;
                    foreach (AsyncOperation ao in m_LoadOperations)
                    {
                        progress += ao.progress;
                        if (ao != null && !ao.isDone)
                        {
                            loadingDone = false;
                            break;
                        }
                    }
                    AlwaysOnCanvas.instance.SetSliderValue(progress / (float)m_LoadOperations.Count);
                    await Task.Delay(TimeSpan.FromMilliseconds(100)); // Adjusted to 100ms for smoother updates
                }
#if OTBT_AC
                var singleSceneManager = FindObjectOfType<SingleSceneManager>();
                if (singleSceneManager != null)
                {
                    var gameEngine = singleSceneManager.gameEngine;
                    AC.KickStarter.SetGameEngine(gameEngine);
                }
                AC.KickStarter.TurnOnAC();
#endif
                AlwaysOnCanvas.instance.SetSliderValue(0f);
                SceneStarted();
            };

            // activate the lerper
            if (AlwaysOnCanvas.exists && fadeToBlack)
            {
                AlwaysOnCanvas.instance.StartFadeToBlack(fadeAction);
            } else
            {
                fadeAction.Invoke();
            }
        }



        public void SceneStarted()
        {
            m_CurrentScene = m_SceneFadeTarget;
            m_LoadOperations.Clear();
            if (AlwaysOnCanvas.exists)
            {
                AlwaysOnCanvas.instance.StartFadeToVisible();
            }

            onSceneStarted?.Invoke();
            onSceneStarted = null;
        }


        public void Verify(CheckVerifyInterface checker)
        {
#if UNITY_EDITOR
            checker.Check(SceneUtility.GetBuildIndexByScenePath(baseScene.ScenePath) == 0, "The base scene should always have build index 0", gameObject, null);
            checker.Check(SceneUtility.GetBuildIndexByScenePath(reloadScene.ScenePath) == 1, "The reload scene should always have build index 1", gameObject, null);

            if (scenes == null) return;
            foreach (SceneCollection sd in scenes)
            {
                if (sd == null) continue;
                foreach (SceneReference sr in sd.scenesToLoad)
                {
                    if (sr == null) continue;
                    checker.Check(!sr.IsEmpty, "Empty scene reference found for scene " + sd.sceneTitle, gameObject, null);
                    if (sr.IsEmpty) continue;

                    int sceneIndex = SceneUtility.GetBuildIndexByScenePath(sr.ScenePath);
                    checker.Check(sceneIndex >= 0, "Scene not in build settings, please add: " + sr.ScenePath, gameObject, () =>
                    {
                        // Find valid Scene paths and make a list of EditorBuildSettingsScene
                        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
                        foreach (var sc in EditorBuildSettings.scenes)
                        {
                            editorBuildSettingsScenes.Add(sc);
                        }
                        editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(sr.ScenePath, true));

                        // Set the Build Settings window Scene list
                        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
                    });
                }

                foreach (SceneCollection sd2 in scenes)
                {
                    if (sd2 == sd) continue;
                    checker.Check(!sd2.sceneTitle.Equals(sd.sceneTitle), "Duplicate scene definition title found: " + sd2.sceneTitle + " // " + sd.sceneTitle, gameObject, null);
                }
            }
#endif
        }
    }
}
