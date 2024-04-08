//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//


#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditor.SceneManagement;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.IO;
using System.Collections;
using System.Text;
using UnityEditor.Animations;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static Sparrow.Verification.VerifyResult;
using UnityEngine.UIElements;
using static Sparrow.Verification.VerifyResultsTreeView;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Sparrow.Verification
{
    /// <summary>
    /// This is our editor window for all scene checks. It gathers all checks implemented in the project and provides
    /// ways to run these checks
    /// </summary>
    [InitializeOnLoad]
    public class VerifyWindow : EditorWindow
    {
        private const string EditorPrefsKey = "SparrowVerify_SelectedScanProfile";

        private VerifyResultsTreeView treeView;
        private TreeViewState treeViewState = new TreeViewState();
        private MultiColumnHeaderState multiColumnHeaderState;
        private List<VerifyResult> verifyResults;

        Vector2 m_ScrollViewPos;

        private int selectedScanType = 1;

        private static int selectedScanProfile = 1;
        private static VerifyProfile[] scanProfiles;

        private bool showTypeError = true;
        private bool showTypeWarning = true;
        private bool showTypeInfo = true;

        public List<VerifyResult> results => verifyResults;

        public static VerifyWindow instance { get; private set; }

        public static VerifyProfile selectedProfile => 
            scanProfiles == null || scanProfiles.Length < selectedScanProfile ? null : scanProfiles[selectedScanProfile];

        public static VerifyProfile[] profiles => scanProfiles;
        public static string[] scanTypes => new[] { "Selection", "Open Scene", "Project", "Assets", "All" };

        public int callbackOrder => throw new NotImplementedException();

        [MenuItem("Window/Sparrow/Verification window _&V", false, 100)]
        public static void ShowWindow()
        {
            GetWindow<VerifyWindow>();
        }

        void OnEnable()
        {
            instance = this;

            // Pfad zum Icon
            string iconPath = FindIconPath("verification_editorIcon");
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            // Setze das Icon und den Titel des Fensters
            this.titleContent = new GUIContent("Verification", icon);
        }

        public static string FindIconPath(string iconName)
        {
            string[] guids = AssetDatabase.FindAssets(iconName);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".png")) // oder die spezifische Erweiterung deines Icons
                {
                    return path;
                }
            }
            return null;
        }

        static VerifyWindow()
        {
            EditorApplication.projectChanged += OnProjectChanged;
        }

        static void OnProjectChanged()
        {
            instance?.FindProfiles();
        }

        public void FindProfiles()
        {
            string[] guids = AssetDatabase.FindAssets("t:VerifyProfile");
            if (guids.Length == 0)
            {
                Debug.LogError("No Verification Profiles found. Please create one!", this);
                return;
            }

            scanProfiles = new VerifyProfile[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                scanProfiles[i] = AssetDatabase.LoadAssetAtPath<VerifyProfile>(assetPath);
            }

            // Check EditorPrefs for a saved profile selection
            string savedProfile = EditorPrefs.GetString(EditorPrefsKey, "Default");
            if (string.IsNullOrEmpty(savedProfile)) return;
            int index = Array.FindIndex(scanProfiles, profile => profile.caption == savedProfile);
            if (index != -1)
            {
                selectedScanProfile = index;
            } else
            {
                selectedScanProfile = 0;
            }
        }


        private void ExportResultsToTXT(List<VerifyResult> results, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var result in results)
                {
                    sw.WriteLine("Category: " + result.category);
                    sw.WriteLine("Description: " + result.description);
                    sw.WriteLine("Object Name: " + (result.obj == null ? result.objName : result.obj.name));
                    sw.WriteLine("Severity: " + result.severity);
                    sw.WriteLine("-----------------------");
                }
            }
        }

        private IEnumerator PostToAPI(string json, string url)
        {
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
        }

        private void ExportResultsToJSON(List<VerifyResult> results, string path)
        {
            using StreamWriter sw = new StreamWriter(path);
            foreach (var result in results)
            {
                sw.WriteLine(result.ToJson());
            }
        }

        public static List<VerifyCheckBase> InstantiateChecklist()
        {
            List<VerifyCheckBase> ret = new List<VerifyCheckBase>();

            IEnumerable<System.Type> checkTypes = ReflectionUtil.TypesImplementingInterface(typeof(VerifyCheckBase));
            foreach (System.Type checkType in checkTypes)
            {
                try
                {
                    var type = Activator.CreateInstance(checkType);
                    ret.Add((VerifyCheckBase)type);
                }
                catch (Exception e)
                {
                    if (ret.Count < 0) Debug.LogException(e, null);
                }
            }

            return ret;
        }
    
        public static List<VerifyResult> PerformSelectedChecks(List<UnityEngine.Object> checkObjects, CheckType type, string checkSource, bool progressBar)
        {
            if (progressBar) EditorUtility.DisplayProgressBar("Sparrow Verification", "Performing checks now.", 0);
            List<VerifyCheckBase> ret = selectedProfile == null
                ? InstantiateChecklist()
                : selectedProfile.checks;

            List<VerifyResult> results = new List<VerifyResult>();
            float progressBarScale = 1f / (float) ret.Count;
            float progressBarOffset = 0f;
            foreach (VerifyCheckBase check in ret)
            {
                results.AddRange(check.PerformCheckWrapper(checkObjects, type, checkSource,
                    progressBar: progressBar, progressBarOffset: progressBarOffset, progressBarScale: progressBarScale));
                progressBarOffset += progressBarScale;
            }
            if (progressBar) EditorUtility.ClearProgressBar();
            return results;
        }

        private static List<UnityEngine.Object> AddAssetsByType<T>(string searchType, bool progressBar = false) where T : UnityEngine.Object
        {
            if (progressBar) EditorUtility.DisplayProgressBar("Sparrow Verification", "Gathering " + searchType + " objects to verify...", 0);
            List<UnityEngine.Object> checkObjects = new List<UnityEngine.Object>();
            string[] shader_guids = AssetDatabase.FindAssets("t:" + searchType); // Find shader assets
            for (int i = 0; i < shader_guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(shader_guids[i]);
                if (!path.StartsWith("Assets/")) continue;
                T shader = AssetDatabase.LoadAssetAtPath<T>(path);
                if (!checkObjects.Contains(shader))
                    checkObjects.Add(shader);
            }

            return checkObjects;
        }

        public void AddResultRange(List<VerifyResult> li)
        {
            verifyResults.AddRange(li);
            treeView = null;
        }

        // Add a new menu item in the Hierarchy view

        [MenuItem("Assets/✔ Validate selection", false, -1000)]
        static void ValidateCurrentSelectionAssets() {
            List<UnityEngine.Object> m_CheckObjects = new List<UnityEngine.Object>();

            string[] selectedAssetGUIDs = Selection.assetGUIDs;
            foreach (string guid in selectedAssetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // Check if the asset is a folder
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    string[] assetGUIDsInFolder = AssetDatabase.FindAssets("", new[] { assetPath });
                    foreach (string assetGUID in assetGUIDsInFolder)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(assetGUID);
                        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        m_CheckObjects.Add(asset);
                    }
                }
                else
                {
                    // If it's not a folder, just add the single asset
                    UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    m_CheckObjects.Add(asset);
                }
            }

            VerifyWindow.instance.Show();
            VerifyWindow.instance.AddResultRange(PerformSelectedChecks(m_CheckObjects, CheckType.Selection, "", true));
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("GameObject/✔ Validate selection", false, -1000)]
        static void ValidateCurrentSelectionGameObjects()
        {
            List<UnityEngine.Object> m_CheckObjects = new List<UnityEngine.Object>();
            foreach (UnityEngine.Object x in Selection.gameObjects)
            {
                if (!m_CheckObjects.Contains(x))
                    m_CheckObjects.Add(x);
                var gameObj = x as GameObject;
                if (gameObj != null)
                    foreach (Transform child in gameObj.GetComponentsInChildren<Transform>())
                        if (!m_CheckObjects.Contains(child.gameObject))
                            m_CheckObjects.Add(child.gameObject);
            }

            VerifyWindow.instance.Show();
            VerifyWindow.instance.AddResultRange(PerformSelectedChecks(m_CheckObjects, CheckType.Selection, "", true));
            EditorUtility.ClearProgressBar();
        }

        private void PerformSubCheck(List<VerifyResult> ret, CheckType type, string openSceneName = "Current Scene",
            bool progressBar = false)
        {
            List<UnityEngine.Object> m_CheckObjects = new List<UnityEngine.Object>();
            if (progressBar)
                EditorUtility.DisplayProgressBar("Sparrow Verification", "Gathering objects to verify...", 0);

            switch (type)
            {
                case CheckType.Selection:
                    foreach (UnityEngine.Object x in Selection.gameObjects)
                    {
                        if (!m_CheckObjects.Contains(x))
                            m_CheckObjects.Add(x);
                        var gameObj = x as GameObject;
                        if (gameObj != null)
                            foreach (Transform child in gameObj.GetComponentsInChildren<Transform>())
                                if (!m_CheckObjects.Contains(child.gameObject))
                                    m_CheckObjects.Add(child.gameObject);
                    }

                    ret.AddRange(PerformSelectedChecks(m_CheckObjects, type, "", true));
                    break;
                case CheckType.CurrentScene:
                    Scene activeScene = SceneManager.GetActiveScene();
                    string scenePath = activeScene == null ? "" : activeScene.path;
                    ret.AddRange(CheckOpenScene(type, scenePath));
                    break;
                case CheckType.Project:
                    string[] guids = AssetDatabase.FindAssets("t:ScriptableObject"); //FindAssets uses tags check documentation for more info
                    for (int i = 0; i < guids.Length; i++) //probably could get optimized 
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        ScriptableObject a = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                        if (!m_CheckObjects.Contains(a))
                            m_CheckObjects.Add(a);
                    }

                    guids = AssetDatabase.FindAssets("t:Prefab"); //FindAssets uses tags check documentation for more info
                    for (int i = 0; i < guids.Length; i++) //probably could get optimized 
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        GameObject a = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (!m_CheckObjects.Contains(a))
                            m_CheckObjects.Add(a);
                    }
                    
                    ret.AddRange(PerformSelectedChecks(m_CheckObjects, type, "", true));
                    PerformSubCheck(ret, CheckType.Assets, openSceneName, progressBar);
                    break;
                
                case CheckType.Assets:
                    m_CheckObjects.AddRange(AddAssetsByType<GameObject>("Prefab", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<GameObject>("fbx", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<Mesh>("Mesh", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<ScriptableObject>("ScriptableObject", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<Texture2D>("Texture2D", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<Shader>("Shader", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<Sprite>("Sprite", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<LightingDataAsset>("LightingDataAsset", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<PhysicMaterial>("PhysicMaterial", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<PhysicsMaterial2D>("PhysicsMaterial2D", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<AnimatorController>("AnimatorController", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<Material>("Material", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<AudioClip>("AudioClip", progressBar));
                    m_CheckObjects.AddRange(AddAssetsByType<MonoScript>("MonoScript", progressBar));
                    ret.AddRange(PerformSelectedChecks(m_CheckObjects, type, "", true));
                    break;

                case CheckType.Scenes:
                    // Get scenes from build settings
                    EditorBuildSettingsScene[] scenesToCheck = EditorBuildSettings.scenes;
                    if (scenesToCheck.Length == 0)
                    {
                        Debug.LogError(
                            "No scenes found in build settings. Please add at least one scene to the build settings!");
                        return;
                    }

                    if (progressBar)
                        EditorUtility.DisplayProgressBar("Sparrow Verification", "Performing verification now...",
                            1f / (scenesToCheck.Length + 1));

                    // Iterate through each scene in the build settings
                    for (int i = 0; i < scenesToCheck.Length; i++)
                    {
                        EditorBuildSettingsScene sceneToCheck = scenesToCheck[i];

                        // Skip if scene is not enabled in the build settings
                        if (!sceneToCheck.enabled) continue;

                        if (progressBar)
                            EditorUtility.DisplayProgressBar("Sparrow Verification", "Checking scene " + sceneToCheck.path,
                                (float)(i + 1) / (scenesToCheck.Length + 1));

                        // Open the scene
                        EditorSceneManager.OpenScene(sceneToCheck.path);

                        // Perform your internal check
                        ret.AddRange(CheckOpenScene(type, sceneToCheck.path));
                    }

                    break;
            }
        }

        private List<VerifyResult> CheckOpenScene(CheckType type, string path)
        {
            List<UnityEngine.Object> checkObjects = new List<UnityEngine.Object>();
            foreach (UnityEngine.Object x in (GameObject.FindObjectsOfType<GameObject>()))
                if (!checkObjects.Contains(x))
                    checkObjects.Add(x);
            return PerformSelectedChecks(checkObjects, type, path, true);
        }

        public List<VerifyResult> PerformCheck(CheckType type, string openSceneName = "Current Scene",
            bool progressBar = false)
        {
            List<VerifyResult> ret = new List<VerifyResult>();

            // SELECTION OR OPEN SCENE
            if (type == CheckType.Selection || type == CheckType.CurrentScene || type == CheckType.Project ||
                type == CheckType.Assets)
            {
                PerformSubCheck(ret, type, openSceneName, progressBar);
            }
            else if (type == CheckType.All)
            {
                if (progressBar) EditorUtility.DisplayProgressBar("Sparrow Verification", "Checking scriptable objects...", 0f);
                // ------------ SCRIPTABLE OBJECTS -------------
                PerformSubCheck(ret, CheckType.Project, openSceneName, progressBar);

                // ----------- ALL SCENES ------------------
                PerformSubCheck(ret, CheckType.Scenes, openSceneName, progressBar);

                // ----------- ASSETS ------------------
                PerformSubCheck(ret, CheckType.Assets, openSceneName, progressBar);
            }

            if (progressBar) EditorUtility.ClearProgressBar();

            return ret;
        }

        public static int CountProblems(Dictionary<string, List<VerifyResult>> problems)
        {
            int count = 0;
            foreach (List<VerifyResult> l in problems.Values)
                count += l.Count;
            return count;
        }

        public void UpdateResultDisplay(CheckType checkType, VerifyProfile profile = null)
        {
            verifyResults.Clear();
            if (profile != null && scanProfiles.Contains(profile))
                for(int i = 0; i < scanProfiles.Length; i++)
                    if (scanProfiles[i] == profile)
                        selectedScanProfile = i;
            verifyResults.AddRange(PerformCheck(checkType, progressBar: true));
            treeView = null;
        }

        private void CreateNewProfile()
        {
            string basePath = "Assets/";
            if (System.IO.Directory.Exists("Assets/Sparrow/Verification/profiles/"))
                basePath = "Assets/Sparrow/Verification/profiles/";
            VerifyProfile newProfile = ScriptableObject.CreateInstance<VerifyProfile>();
            string baseName = "NewVerifyProfile";
            string assetName = baseName + ".asset";
            string assetPath = basePath + assetName;

            int counter = 1;
            while (AssetDatabase.LoadAssetAtPath<VerifyProfile>(assetPath) != null)
            {
                assetName = baseName + counter + ".asset";
                assetPath = basePath + assetName;
                counter++;
            }

            AssetDatabase.CreateAsset(newProfile, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            FindProfiles();
            EditorPrefs.SetString(EditorPrefsKey, newProfile.caption);
            int index = Array.FindIndex(scanProfiles, profile => profile == newProfile);
            if (index != -1)
                selectedScanProfile = index;

            newProfile.UpdateCheckList();
            Selection.SetActiveObjectWithContext(newProfile, this);
        }


        public void OnGUI()
        {
            this.titleContent = new GUIContent("Verification", "Tool to verify the project for common errors");

            if (verifyResults == null) verifyResults = new List<VerifyResult>();
            if (Application.isPlaying)
            {
                GUILayout.Label("Not available in play mode.");
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // selected profile
            if (scanProfiles == null || scanProfiles.Length == 0) FindProfiles();

            List<string> profileNames = new List<string>();
            if (scanProfiles != null)
                profileNames.AddRange(scanProfiles.Select(profile => profile.caption).ToArray());

            for(int i = 0; i < profileNames.Count; i++)
            {
                for(int j = 0; j < profileNames.Count; j++)
                {
                    if (i == j) continue;
                    if (profileNames[i].Equals(profileNames[j]))
                    {
                        profileNames[j] = profileNames[j] + " (" + j + ")";
                    }
                }
            }
            // Add "Add Profile" option to the list
            profileNames.Add("(+) Add new profile");

            selectedScanProfile = EditorGUILayout.Popup(selectedScanProfile, profileNames.ToArray(), EditorStyles.toolbarDropDown, GUILayout.Width(120));

            if (scanProfiles != null && selectedScanProfile < scanProfiles.Length)  // An existing profile was selected
            {
                EditorPrefs.SetString(EditorPrefsKey, scanProfiles[selectedScanProfile].caption);
            }
            else  // "Add Profile" was selected
            {
                CreateNewProfile();
            }

            // Add an Edit button next to the popup to edit the selected profile
            if (GUILayout.Button("Edit", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                if (selectedScanProfile < scanProfiles.Length)
                {
                    Selection.activeObject = scanProfiles[selectedScanProfile];
                }
            }


            selectedScanType = EditorGUILayout.Popup(selectedScanType, scanTypes, EditorStyles.toolbarDropDown,
                GUILayout.Width(100));

            if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton"), EditorStyles.toolbarButton))
            {
                UpdateResultDisplay(CheckTypeFromString(scanTypes[selectedScanType]));
            }

            // Problem counter
            // Calculate the width of each label
            var labelStyle = EditorStyles.toolbarButton;
            float infoWidth = labelStyle.CalcSize(new GUIContent((treeView?.numInfo ?? 0).ToString()))
                .x + 20;
            float warningsWidth =
                labelStyle.CalcSize(new GUIContent((treeView?.numWarnings ?? 0).ToString())).x + 20;
            float problemsWidth =
                labelStyle.CalcSize(new GUIContent((treeView?.numProblems ?? 0).ToString())).x + 20;

            // Display the labels with the calculated widths
            showTypeInfo = GUILayout.Toggle(showTypeInfo,
                new GUIContent((treeView?.numInfo ?? 0).ToString(),
                    EditorGUIUtility.FindTexture("console.infoicon")), labelStyle, GUILayout.Width(infoWidth));
            showTypeWarning = GUILayout.Toggle(showTypeWarning,
                new GUIContent((treeView?.numWarnings ?? 0).ToString(),
                    EditorGUIUtility.FindTexture("console.warnicon")), labelStyle, GUILayout.Width(warningsWidth));
            showTypeError = GUILayout.Toggle(showTypeError,
                new GUIContent((treeView?.numProblems ?? 0).ToString(),
                    EditorGUIUtility.FindTexture("console.erroricon")), labelStyle, GUILayout.Width(problemsWidth));

            // Clear button
            bool isSelected = treeView != null && treeView.GetSelection().Count > 0;
            if (GUILayout.Button(new GUIContent(isSelected ? "Clear selected" : "Clear all", EditorGUIUtility.FindTexture("TreeEditor.Trash")),
                    EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                var selected = treeView.GetSelection();
                if(isSelected)
                {
                    for(int i = verifyResults.Count - 1; i >= 0; i--)
                    {
                        int id = treeView.GetIDForResult(verifyResults[i]);
                        if (id < 0) continue;
                        if (selected.Contains(id))
                            verifyResults.RemoveAt(i);
                    }
                    treeView = null;
                    if (treeView != null) treeView.UpdateCounts();
                } else
                {
                    verifyResults.Clear();
                    treeView = null;
                    if (treeView != null) treeView.UpdateCounts();
                }
            }
            
            // Fix selected button
           
            using (new EditorGUI.DisabledGroupScope(!isSelected))
            {
                var fixSelectedBtn = GUILayout.Button(
                    new GUIContent(isSelected ? "Fix selected" : "Fix all", EditorGUIUtility.FindTexture("d_ToolsToggle@2x")),
                    EditorStyles.toolbarButton, GUILayout.Width(95));
        
                if (fixSelectedBtn && isSelected)
                {
                    bool showWarning = EditorPrefs.GetBool("ShowFixAllWarning", true);
                    if (showWarning)
                    {
                        int option = EditorUtility.DisplayDialogComplex("Warning: Automatic fixes",
                            "You are about to run a lot of automated actions that cannot be undone. Are you sure you want to continue?",
                            "Yes", "Cancel", "Yes and don't show again");

                        switch (option)
                        {
                            case 0: // Yes
                                FixSelected(!isSelected);
                                break;
                            case 1: // Cancel
                                // Do nothing
                                break;
                            case 2: // Yes and don't show again
                                EditorPrefs.SetBool("ShowFixAllWarning", false);
                                FixSelected(!isSelected);
                                break;
                        }
                    }
                    else
                    {
                        FixSelected(!isSelected);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (treeView == null)
            {
                var headerState = VerifyResultsTreeView.CreateDefaultMultiColumnHeaderState();
                treeView = new VerifyResultsTreeView(treeViewState, headerState, verifyResults);
                treeView.multiColumnHeader.ResizeToFit();
                treeView.UpdateCounts();
            }

            float treeViewY = GUILayoutUtility.GetLastRect().yMax;
            float treeViewHeight = position.height - treeViewY;

            Rect treeViewRect = new Rect(0, treeViewY, position.width, treeViewHeight);
            Rect scrollRect = new Rect(0, treeViewY, position.width, position.height - treeViewY);

            GUILayout.BeginArea(new Rect(0, position.height - treeViewHeight, position.width, 625));
            
            m_ScrollViewPos = GUI.BeginScrollView(scrollRect, m_ScrollViewPos,
                new Rect(0, 0, position.width, treeViewHeight), false, false);

            if (this.treeView.showTypeError != this.showTypeError)
            {
                this.treeView.showTypeError = this.showTypeError;
                treeView.Reload();
            }

            if (this.treeView.showTypeWarning != this.showTypeWarning)
            {
                this.treeView.showTypeWarning = this.showTypeWarning;
                treeView.Reload();
            }

            if (this.treeView.showTypeInfo != this.showTypeInfo)
            {
                this.treeView.showTypeInfo = this.showTypeInfo;
                treeView.Reload();
            }

            treeView.OnGUI(new Rect(0, 0, position.width, treeViewHeight));

            GUI.EndScrollView();
            
            GUILayout.EndArea();

            // ------------------------- BOTTOM TOOLBAR --------------------
            GUILayout.BeginArea(new Rect(0, position.height - 20, position.width, 20));

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            
            // Add an Edit button next to the popup to edit the selected profile
            if (GUILayout.Button("Select all", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                treeView.SelectAllRows();
            }
            if (GUILayout.Button("Select none", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                treeView.SetSelection(new List<int>());
            }

            GUILayout.Label("");

            // innerhalb deiner OnGUI Methode oder wo auch immer du die Buttons erstellst
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Group by: ", GUILayout.Width(75));
            foreach(GroupSetting s in new GroupSetting[] { GroupSetting.Scene, GroupSetting.Severity, GroupSetting.CheckType, GroupSetting.Fixable }) 
                if (GUILayout.Toggle(treeView.groupMode == s, s.ToString(), EditorStyles.toolbarButton, GUILayout.Width(75)))
                    treeView.SetGrouping(s);

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("");

            // Export results button
            Rect buttonRect = GUILayoutUtility.GetRect(
                new GUIContent("Export", EditorGUIUtility.IconContent("SaveActive").image),
                EditorStyles.toolbarDropDown, GUILayout.Width(75));
            if (EditorGUI.DropdownButton(buttonRect,
                    new GUIContent("Export", EditorGUIUtility.IconContent("SaveActive").image), FocusType.Passive,
                    EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Export to TXT"), false, () =>
                {
                    var option = EditorUtility.DisplayDialogComplex("Export to TXT",
                        "Do you want to export all results or only the selected ones?",
                        "All", "Only Selected", "Cancel");
                    switch (option)
                    {
                        case 0: // All
                            var pathAll = EditorUtility.SaveFilePanel("Save Verify Results as TXT", "", "VerifyResults",
                                "txt");
                            if (!string.IsNullOrEmpty(pathAll))
                                ExportResultsToTXT(verifyResults, pathAll);
                            break;
                        case 1: // Only Selected
                            var pathSelected = EditorUtility.SaveFilePanel("Save Verify Results as TXT", "", "VerifyResults",
                                "txt");
                            if (!string.IsNullOrEmpty(pathSelected))
                                ExportResultsToTXT(GetSelectedItems(), pathSelected);
                            break;
                        case 2: // Cancel
                            Debug.Log("Cancel");
                            break;
                    }
                });
                menu.AddItem(new GUIContent("Export to JSON"), false, () =>
                {
                    var option = EditorUtility.DisplayDialogComplex("Export to JSON",
                        "Do you want to export all results or only the selected ones?",
                        "All", "Only Selected", "Cancel");
                    switch (option)
                    {
                        case 0: // All
                            string pathAll = EditorUtility.SaveFilePanel("Save Verify Results as JSON", "", "VerifyResults",
                                "json");
                            if (!string.IsNullOrEmpty(pathAll))
                                ExportResultsToJSON(verifyResults, pathAll);
                            break;
                        case 1: // Only Selected
                            string pathSelected = EditorUtility.SaveFilePanel("Save Verify Results as JSON", "", "VerifyResults",
                                "json");
       
                            if (!string.IsNullOrEmpty(pathSelected))
                                ExportResultsToJSON(GetSelectedItems(), pathSelected);
                            break;
                        case 2: // Cancel
                            Debug.Log("Cancel");
                            break;
                    }
                });
                menu.AddItem(new GUIContent("Send to API"), false, () =>
                {
                    if (selectedProfile != null)
                    {
                        var option = EditorUtility.DisplayDialogComplex("Export to JSON",
                            "Do you want to export all results or only the selected ones?",
                            "All", "Only Selected", "Cancel");
                        switch (option)
                        {
                            case 0: // All
                                SetupAPICall(verifyResults);
                                break;
                            case 1: // Only Selected
                                SetupAPICall(GetSelectedItems());
                                break;
                            case 2: // Cancel
                                // Do nothing
                                break;
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "No Verify Profile selected. Please create or select one!",
                            "Ok");
                    }
                });
                menu.DropDown(buttonRect);
            }


            // Help button
            if (GUILayout.Button(new GUIContent("Help", EditorGUIUtility.FindTexture("_Help")),
                    EditorStyles.toolbarButton, GUILayout.Width(75)))
            {
                Application.OpenURL("https://wiki.beatentrack.games/s/sparrow-verification");
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private List<VerifyResult> GetSelectedItems()
        {
            var selected = treeView.GetSelection();
            List<VerifyResult> save = new List<VerifyResult>();
            foreach (VerifyResult r in verifyResults)
            {
                int id = treeView.GetIDForResult(r);
                if (id < 0) continue;
                if (selected.Contains(id))
                    save.Add(r);
            }
            return save;
        }
        private void SetupAPICall(List<VerifyResult> results)
        {
            if (string.IsNullOrEmpty(selectedProfile.apiEndpoint))
            {
                EditorUtility.DisplayDialog("Error",
                    "API endpoint URL is not set. Please set it in the selected Verify Profile", "Ok");
                return;
            }
            
            var json = "[" + string.Join(",", results.Select(result => result.ToJson())) + "]";

            myCoroutine = EditorCoroutine.Start(PostToAPI(json, selectedProfile.apiEndpoint));
        }

        EditorCoroutine myCoroutine;

      

        void OnDestroy()
        {
            myCoroutine?.Stop();
        }

        private void FixSelected(bool fixAll)
        {
            var selected = GetSelectedItems();
            foreach (var result in (fixAll ? verifyResults : selected).Where(result => result.fixAction != null))
            {
                result.fixAction.Invoke();
            }
        }



        private CheckType CheckTypeFromString(string s)
        {
            if (s.Equals("Selection")) return CheckType.Selection;
            if (s.Equals("Open Scene")) return CheckType.CurrentScene;
            if (s.Equals("Project")) return CheckType.Project;
            if (s.Equals("Assets")) return CheckType.Assets;
            return CheckType.All;
        }

      
        private class EditorCoroutine
        {
            public static EditorCoroutine Start(IEnumerator routine)
            {
                EditorCoroutine coroutine = new EditorCoroutine(routine);
                coroutine.Start();
                return coroutine;
            }

            private readonly IEnumerator m_routine;
            private bool m_isRunning;
            
            EditorCoroutine(IEnumerator routine)
            {
                this.m_routine = routine;
            }

            void Start()
            {
                if (m_isRunning) return;
                EditorApplication.update += Update;
                m_isRunning = true;
            }

            public void Stop()
            {
                if (!m_isRunning) return;
                EditorApplication.update -= Update;
                m_isRunning = false;
            }
            
            public void UpdateCoroutine()
            {
                if (m_routine.MoveNext()) return;
                Stop();
            }

            void Update()
            {
                UpdateCoroutine();
            }
        }
    }
}
#endif