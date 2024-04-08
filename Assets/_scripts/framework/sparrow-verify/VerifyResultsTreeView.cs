//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sparrow.Verification
{
    public class VerifyResultsTreeView : TreeView
    {

        public bool showTypeError = true;
        public bool showTypeWarning = true;
        public bool showTypeInfo = true;
        public int numWarnings = 0;
        public int numInfo = 0;
        public int numProblems = 0;

        GroupSetting m_CurrentGrouping = GroupSetting.Scene;
        public GroupSetting groupMode => m_CurrentGrouping;
        public void SetGrouping(GroupSetting grou)
        {
            m_CurrentGrouping = grou;
            Reload();
        }

        public class VerifyResultItem : TreeViewItem
        {
            public VerifyResult data;
        }
        
        private List<VerifyResult> verifyResults;
        float descriptionWidth = 0f;
        float categoryWidth = 0f;
        GUIStyle wordWrapStyle = null;

        public VerifyResultsTreeView(TreeViewState state, MultiColumnHeaderState multiColumnHeaderState, List<VerifyResult> verifyResults)
            : base(state, new MultiColumnHeader(multiColumnHeaderState))
        {
            this.verifyResults = verifyResults;
            this.rowHeight = 20; // Or any other height
            Reload();
            UpdateCounts();
        }

        private GUIStyle WordWrapStyle()
        {
            if(wordWrapStyle == null)
                wordWrapStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            return wordWrapStyle;
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            VerifyResultItem verifyResultItem = item as VerifyResultItem;
            if (verifyResultItem == null)
                return 20f;
            
            return Mathf.Max(WordWrapStyle().CalcHeight(new GUIContent(verifyResultItem.data.category), categoryWidth) + 6f, WordWrapStyle().CalcHeight(new GUIContent(verifyResultItem.data.description), descriptionWidth) + 6f, 20f);
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            return base.GetRenameRect(cellRect, row, item);
        }
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            var columns = new[] {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 30,
                    minWidth = 30,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 20,
                    minWidth = 20,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Category"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Description"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Object"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Fix"),
                    headerTextAlignment = TextAlignment.Left,
                    width = 60,
                    minWidth = 60,
                    autoResize = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(),
                    headerTextAlignment = TextAlignment.Left,
                    width = 35,
                    minWidth = 35,
                    autoResize = false
                }
            };

            var state = new MultiColumnHeaderState(columns);
            return state;
        }

        public int GetIDForResult(VerifyResult res)
        {
            int ret = CheckChildrenForID(res, rootItem);
            return ret;
        }

        private int CheckChildrenForID(VerifyResult res, TreeViewItem curitem)
        {
            foreach (TreeViewItem item in curitem.children)
            {
                if (item is VerifyResultItem vri)
                {
                    if (vri.data == res)
                        return vri.id;
                } else if(curitem.hasChildren)
                {
                    int ret = CheckChildrenForID(res, item);
                    if (ret > 0)
                        return ret;
                }
            }
            return -1;
        }

        private string GetGroupName(VerifyResult result)
        {
            switch(m_CurrentGrouping)
            {
                case GroupSetting.Scene: return result.sceneName;
                case GroupSetting.Severity: return result.severity.ToString();
                case GroupSetting.CheckType: return result.description;
                case GroupSetting.Fixable: return result.fixAction != null ? "Fix available" : "No fix available";
                default: return "Results";
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            // This id should be unique for each tree element.
            int idForName = 0;

            var root = new TreeViewItem { id = idForName++, depth = -1, displayName = "Root" };
            var groupMap = new Dictionary<string, TreeViewItem>();
            
            List<int> sceneNodeIds = new List<int>();  // Store IDs of scene nodes to be expanded

            foreach (var verifyResult in verifyResults)
            {
                if ((showTypeError && verifyResult.severity == VerifyResult.Severity.Error) ||
                    (showTypeWarning && verifyResult.severity == VerifyResult.Severity.Warning) ||
                    (showTypeInfo && verifyResult.severity == VerifyResult.Severity.Info))
                {
                    string groupName = GetGroupName(verifyResult);
                    // Assuming verifyResult.sceneName exists, 
                    // otherwise you have to find a way to determine scene name
                    if (!groupMap.ContainsKey(groupName))
                    {
                        var sceneRoot = new TreeViewItem { id = idForName, depth = 0, displayName = groupName };
                        root.AddChild(sceneRoot);
                        groupMap[groupName] = sceneRoot;

                        sceneNodeIds.Add(idForName);  // Capture this ID for expanding later
                        idForName++;
                    }

                    groupMap[groupName].AddChild(new VerifyResultItem
                    {
                        id = idForName++,
                        depth = 1,
                        data = verifyResult
                    });
                }
            }

            if (!root.hasChildren)
            {
                // Add a new node with depth 0 and display "No failed checks found."
                root.AddChild(new TreeViewItem { id = idForName++, depth = 0, displayName = "No failed checks found" });
            }
            else
            {
                // Expand all scene nodes
                foreach (int id in sceneNodeIds)
                    SetExpanded(id, true);
            }

            // Return root of the tree
            return root;
        }


        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;
            Color selectedBackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Color parentNodeBackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            Color originalBackgroundColor = GUI.backgroundColor;

            // Additional code for scene parent nodes
            if (item.depth == 0)
            {
                var parentRowBackgroundRect = args.rowRect;
                EditorGUI.DrawRect(parentRowBackgroundRect, parentNodeBackgroundColor);

                Rect shiftedRect = new Rect(args.rowRect.x + 20, args.rowRect.y, args.rowRect.width - 20, args.rowRect.height);
                if (item.children != null)
                {
                    EditorGUI.LabelField(shiftedRect, $"{item.displayName} ({item.children.Count} results)");

                    Scene activeScene = SceneManager.GetActiveScene();
                    string loadedScenePath = activeScene == null ? "" : activeScene.path;

                    if (!item.displayName.StartsWith("Current"))
                    {
                        string scenePath = "";
                        foreach (var child in item.children)
                        {
                            VerifyResultItem vri = child as VerifyResultItem;
                            if (vri == null) continue;
                            scenePath = vri.data.checkSource == null ? "" : vri.data.checkSource;
                            if (scenePath.Length > 0) break;
                        }

                        if (scenePath.Length > 0 && !scenePath.Equals(loadedScenePath))
                        {
                            float labelWidth = GUI.skin.label.CalcSize(new GUIContent($"{item.displayName} ({item.children.Count} results)")).x;
                            Rect buttonRect = new Rect(shiftedRect.x + labelWidth + 15, args.rowRect.y + 1, 70, args.rowRect.height - 2);
                            GUI.enabled = !Application.isPlaying;
                            if (GUI.Button(buttonRect, "Load"))
                            {
                                EditorSceneManager.OpenScene(scenePath);
                            }
                            GUI.enabled = true;
                        }
                    }
                } else
                {

                    EditorGUI.LabelField(shiftedRect, $"{item.displayName}");
                }

                return;
            }

            // Draw the cell contents
            for (var i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                Rect cellRect = args.GetCellRect(i);
                CellGUI(cellRect, item, (Columns)args.GetColumn(i), ref args, originalBackgroundColor);
            }
            GUI.backgroundColor = originalBackgroundColor;
        }

        
        private void CellGUI(Rect cellRect, TreeViewItem item, Columns column, ref RowGUIArgs args, Color backgroundColor)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            // cast TreeViewItem to our custom class
            VerifyResultItem verifyResultItem = item as VerifyResultItem;
            if (verifyResultItem == null) return;

            switch (column)
            {
                case Columns.Severity:
                    Texture2D icon;
                    switch (verifyResultItem.data.severity)
                    {
                        case VerifyResult.Severity.Error:
                            icon = EditorGUIUtility.FindTexture("console.erroricon");
                            break;
                        case VerifyResult.Severity.Warning:
                            icon = EditorGUIUtility.FindTexture("console.warnicon");
                            break;
                        case VerifyResult.Severity.Info:
                        default:
                            icon = EditorGUIUtility.FindTexture("console.infoicon");
                            break;
                    }
                    GUI.DrawTexture(new Rect(cellRect.x, cellRect.y, 16, 16), icon);
                    break;
                case Columns.Category:
                    categoryWidth = cellRect.width;
                    float oldHeight = cellRect.height;
                    cellRect.height = GetCustomRowHeight(0, item);
                    cellRect.y -= (cellRect.height - oldHeight)/2f;
                    EditorGUI.LabelField(cellRect, new GUIContent(verifyResultItem.data.category, verifyResultItem.data.tooltip), WordWrapStyle());
                    break;
                case Columns.Description:
                    descriptionWidth = cellRect.width;
                    float oldHeight2 = cellRect.height;
                    cellRect.height = GetCustomRowHeight(0, item);
                    cellRect.y -= (cellRect.height - oldHeight2) / 2f;
                    EditorGUI.LabelField(cellRect, new GUIContent(verifyResultItem.data.description, verifyResultItem.data.tooltip), WordWrapStyle());
                    break;
                case Columns.Object:
                    if (verifyResultItem.data.obj != null)
                    {
                        EditorGUI.ObjectField(cellRect, verifyResultItem.data.obj, typeof(UnityEngine.Object), true);
                    } else
                    {
                        EditorGUI.LabelField(cellRect, verifyResultItem.data.objName);
                    }
                    break;
                case Columns.Fix:
                    GUIContent buttonContent = new GUIContent("Fix", EditorGUIUtility.FindTexture("d_ToolsToggle"));
                    if (verifyResultItem.data.fixAction != null && verifyResultItem.data.obj != null)
                    {
                        if (GUI.Button(cellRect, buttonContent))
                        {
                            verifyResultItem.data.fixAction.Invoke();
                            verifyResults.Remove(verifyResultItem.data);
                            Reload();
                            UpdateCounts();
                        }
                    }
                    break;
                case Columns.Selected:
                    // Calculate the position to center-align the toggle box
                    var toggleWidth = EditorGUIUtility.singleLineHeight;
                    var toggleX = cellRect.x + (cellRect.width - toggleWidth) * 0.5f;
                    //verifyResultItem.data.isSelected = EditorGUI.Toggle(new Rect(toggleX, cellRect.y, toggleWidth, cellRect.height), verifyResultItem.data.isSelected);
                    break;
                case Columns.Remove:
                    GUIContent buttonContentRemove = new GUIContent("", EditorGUIUtility.FindTexture("d_TreeEditor.Trash"));

                    if (GUI.Button(cellRect, buttonContentRemove))
                    {
                        verifyResults.Remove(verifyResultItem.data);
                        Reload();
                        UpdateCounts();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }

        public void UpdateCounts()
        {
            numWarnings = 0;
            numInfo = 0;
            numProblems = 0;
            foreach (VerifyResult result in verifyResults)
            {
                switch (result.severity)
                {
                    case VerifyResult.Severity.Error: numProblems++; break;
                    case VerifyResult.Severity.Warning: numWarnings++; break;
                    case VerifyResult.Severity.Info: numInfo++; break;
                }
            }
        }
        
        private enum Columns
        {
            Selected,
            Severity,
            Category,
            Description,
            Object,
            Fix,
            Remove, 
        }


        public enum GroupSetting
        {
            Scene, Severity, CheckType, Fixable
        }
    }
}
#endif