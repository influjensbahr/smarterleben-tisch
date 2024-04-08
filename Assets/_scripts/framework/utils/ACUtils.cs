

#if OTBT_AC && UNITY_EDITOR
using AC;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace OTBT.Framework.Utils
{
    public class ACUtils 
    {
        public static void CreateAction(ActionListAsset actionList, Action currentAction)
        {
            if (currentAction != null && currentAction.NumSockets == 1 && currentAction.endings[0].resultAction == ResultAction.Continue)
            {
                currentAction.endings[0].resultAction = ResultAction.Stop;
            }

            ModifyAction(actionList, currentAction, "Insert after");

            foreach (Action action in actionList.actions)
            {
                if (action != null)
                {
                    action.SkipActionGUI(actionList.actions, false);
                }
            }
            EditorUtility.SetDirty(actionList);
        }

        public static void CreateAction(ActionList actionList, Action currentAction)
        {
            if (currentAction != null && currentAction.NumSockets == 1 && currentAction.endings[0].resultAction == ResultAction.Continue)
            {
                currentAction.endings[0].resultAction = ResultAction.Stop;
            }

            ModifyActionEditor(actionList, currentAction, "Insert after");

            foreach (Action action in actionList.actions)
            {
                if (action != null)
                {
                    action.SkipActionGUI(actionList.actions, false);
                }
            }
            EditorUtility.SetDirty(actionList);
        }

        public static void ModifyActionEditor(ActionList _target, AC.Action _action, string callback)
        {
            int i = -1;
            if (_action != null && _target.actions.IndexOf(_action) > -1)
            {
                i = _target.actions.IndexOf(_action);
            }

            bool doUndo = (callback != "Copy");

            if (doUndo)
            {
                Undo.SetCurrentGroupName(callback);
                Undo.RecordObjects(new Object[] { _target }, callback);
#if !AC_ActionListPrefabs
                if (_target.actions != null) Undo.RecordObjects(_target.actions.ToArray(), callback);
#endif
            }

            switch (callback)
            {
                case "Enable":
                    _action.isEnabled = true;
                    break;

                case "Disable":
                    _action.isEnabled = false;
                    break;

                case "Cut":
                    List<Action> actionsToCut = new List<Action>();
                    actionsToCut.Add(_action);
                    JsonAction.ToCopyBuffer(actionsToCut, false);
                    DeleteAction(_action, _target);
                    break;

                case "Copy":
                    List<Action> actionsToCopy = new List<Action>();
                    actionsToCopy.Add(_action);
                    JsonAction.ToCopyBuffer(actionsToCopy);
                    break;

                case "Paste after":
                    List<Action> pasteList = JsonAction.CreatePasteBuffer(false);
                    _target.actions.InsertRange(i + 1, pasteList);
                    break;

                case "Insert end":
                    AddAction(ActionsManager.GetDefaultAction(), -1, _target);
                    break;

                case "Insert after":
                    Action insertAfterAction = AddAction(ActionsManager.GetDefaultAction(), i + 1, _target);
                    if (_action.endings.Count > 0)
                    {
                        insertAfterAction.endings.Add(new ActionEnd(_action.endings[0]));
                    }
                    break;

                case "Delete":
                    Undo.RecordObject(_target, "Delete action");
                    DeleteAction(_action, _target);
                    break;

                case "Move to top":
                    Vector2 newPosition = _target.actions[0].NodeRect.position + new Vector2(30, 30);
                    _target.actions[0].NodeRect = new Rect(newPosition, _target.actions[0].NodeRect.size);
                    _target.actions.Remove(_action);
                    _target.actions.Insert(0, _action);
                    break;

                case "Move up":
                    _target.actions.Remove(_action);
                    _target.actions.Insert(i - 1, _action);
                    break;

                case "Move to bottom":
                    _target.actions.Remove(_action);
                    _target.actions.Insert(_target.actions.Count, _action);
                    break;

                case "Move down":
                    _target.actions.Remove(_action);
                    _target.actions.Insert(i + 1, _action);
                    break;

                case "Toggle breakpoint":
                    _action.isBreakPoint = !_action.isBreakPoint;
                    break;

                case "EditSource":
                    Action.EditSource(_action);
                    break;

                default:
                    break;
            }

            if (doUndo)
            {
                Undo.RecordObjects(new Object[] { _target }, callback);
#if !AC_ActionListPrefabs
                if (_target.actions != null) Undo.RecordObjects(_target.actions.ToArray(), callback);
#endif
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                EditorUtility.SetDirty(_target);
            }
        }


        public static void DeleteAction(AC.Action action, ActionList _target)
        {
            if (action != null)
            {
                DialogueSpeechAction dsa = action as DialogueSpeechAction;
                if (dsa != null)
                    dsa.OnDeleteAction();
                _target.actions.Remove(action);

#if !AC_ActionListPrefabs
                Undo.DestroyObjectImmediate(action);
#endif
                //SyncAssetObjects (_target);
            }
        }


        public static Action AddAction(string className, int i, ActionList _target)
        {
            if (string.IsNullOrEmpty(className))
            {
                return null;
            }

            List<int> idArray = new List<int>();
            foreach (AC.Action _action in _target.actions)
            {
                if (_action == null) continue;
                idArray.Add(_action.id);
            }
            idArray.Sort();

            Action newAction = Action.CreateNew(className);

            // Update id based on array
            foreach (int _id in idArray.ToArray())
            {
                if (newAction.id == _id)
                    newAction.id++;
            }

            return AddAction(newAction, i, _target);
        }


        public static Action AddAction(AC.Action newAction, int i, ActionList _target)
        {
            if (i < 0)
            {
                _target.actions.Add(newAction);
            }
            else
            {
                _target.actions.Insert(i, newAction);
            }

            //SyncAssetObjects (_target);

            return newAction;
        }



        public static void ModifyAction(ActionListAsset _target, AC.Action _action, string callback)
        {
            ActionsManager actionsManager = AdvGame.GetReferences().actionsManager;
            if (actionsManager == null)
            {
                return;
            }

            int i = -1;
            if (_action != null && _target.actions.IndexOf(_action) > -1)
            {
                i = _target.actions.IndexOf(_action);
            }

            bool doUndo = (callback != "Copy");

            if (doUndo)
            {
                Undo.SetCurrentGroupName(callback);
                Undo.RecordObjects(new Object[] { _target }, callback);
#if !AC_ActionListPrefabs
                if (_target.actions != null) Undo.RecordObjects(_target.actions.ToArray(), callback);
#endif
            }

            switch (callback)
            {
                case "Enable":
                    _target.actions[i].isEnabled = true;
                    break;

                case "Disable":
                    _target.actions[i].isEnabled = false;
                    break;

                case "Cut":
                    List<Action> actionsToCut = new List<Action>();
                    actionsToCut.Add(_action);
                    JsonAction.ToCopyBuffer(actionsToCut, false);
                    DeleteAction(_action, _target);
                    break;

                case "Copy":
                    List<Action> actionsToCopy = new List<Action>();
                    actionsToCopy.Add(_action);
                    JsonAction.ToCopyBuffer(actionsToCopy);
                    break;

                case "Paste after":
                    int j = i + 1;
                    List<Action> pasteList = JsonAction.CreatePasteBuffer(false);
                    foreach (Action action in pasteList)
                    {
                        AddAction(action, j, _target);
                        j++;
                    }
                    break;

                case "Insert after":
                    Action newAction = AddAction(ActionsManager.GetDefaultAction(), i + 1, _target);
                    if (_action.endings.Count > 0)
                    {
                        newAction.endings.Add(new ActionEnd(_action.endings[0]));
                    }
                    break;

                case "Delete":
                    DeleteAction(_action, _target);
                    break;

                case "Move to top":
                    _target.actions.Remove(_action);
                    _target.actions.Insert(0, _action);
                    break;

                case "Move up":
                    _target.actions.Remove(_action);
                    _target.actions.Insert(i - 1, _action);
                    break;

                case "Move to bottom":
                    _target.actions.Remove(_action);
                    _target.actions.Insert(_target.actions.Count, _action);
                    break;

                case "Move down":
                    _target.actions.Remove(_action);
                    _target.actions.Insert(i + 1, _action);
                    break;

                default:
                    break;
            }

            if (doUndo)
            {
                Undo.RecordObjects(new Object[] { _target }, callback);
#if !AC_ActionListPrefabs
                if (_target.actions != null) Undo.RecordObjects(_target.actions.ToArray(), callback);
#endif
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                EditorUtility.SetDirty(_target);
            }
        }

        public static void DeleteAction(AC.Action action, ActionListAsset _target)
        {
            if (action != null)
            {
                _target.actions.Remove(action);
                ActionListAsset.SyncAssetObjects(_target);
            }
        }

        public static Action AddAction(string className, int i, ActionListAsset _target)
        {
            if (string.IsNullOrEmpty(className))
            {
                return null;
            }

            List<int> idArray = new List<int>();
            foreach (AC.Action _action in _target.actions)
            {
                if (_action == null) continue;
                idArray.Add(_action.id);
            }
            idArray.Sort();

            Action newAction = Action.CreateNew(className);

            // Update id based on array
            foreach (int _id in idArray.ToArray())
            {
                if (newAction.id == _id)
                    newAction.id++;
            }

            return AddAction(newAction, i, _target);
        }


        public static Action AddAction(AC.Action newAction, int i, ActionListAsset _target)
        {
            if (i < 0)
            {
                _target.actions.Add(newAction);
            }
            else
            {
                _target.actions.Insert(i, newAction);
            }

            ActionListAsset.SyncAssetObjects(_target);

            return newAction;
        }
    }
}

#endif