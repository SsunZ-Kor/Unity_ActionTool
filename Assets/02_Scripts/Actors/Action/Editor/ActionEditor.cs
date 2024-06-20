using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Actor
{
    public static class ActionEditor
    {
        public const string PATH_ACTOR_ROOT = "Assets/01_Resources/Actor";
        public const string FOLDERNAME_ACTIONDATA = "ActionData";

        public class ActionDataInfo
        {
            public string path;
            public string name_Group;
            public string name_Data;
        }

        public static ActionDataInfo SelectedActionDataInfo { get; private set; }
        public static ActionData SelectedActionData { get; private set; }

        public static List<IActionEditorItem> ItemInSelectedActionData { get; private set; } = new();

        public static HashSet<IActionEditorItem> SelectedActionItems { get; } = new();
        public static IActionEditorItem LastSelectedActionItem { get; private set; }

        public static ActionListEditor editorForList;
        public static ActionInspectorEditor editorForInspector;
        public static ActionTimelineEditor editorForTimeline;

        private static List<IActionEditorItem> CopyedActionItems = new();

        [MenuItem("Nori Tools/Action Tool")]
        public static void ShowWindow()
        {
            editorForList = ActionListEditor.ShowWindow();
            editorForInspector = ActionInspectorEditor.ShowWindow();
            editorForTimeline = ActionTimelineEditor.ShowWindow();
        }

        [MenuItem("Assets/Create Nori/ActionData", false, 1)]
        public static void CreateActionData()
        {
            if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
                return;

            var path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            path = path.Replace("\\", "/");

            if (Path.HasExtension(path))
                path = Path.GetDirectoryName(path);

            var newActionData = ActionData.Editor_CreateActionData(path);
        }

        [MenuItem("Assets/Create Nori/ActionData", true, 1)]
        public static bool CheckPathActionData()
        {
            if (Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
                return false;

            var path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            path = path.Replace("\\", "/");

            return path.StartsWith(PATH_ACTOR_ROOT) && path.Contains(FOLDERNAME_ACTIONDATA);
        }

        public static void SetSelectedActionDataInfo(ActionDataInfo selectedActionDataInfo)
        {
            SelectedActionDataInfo = selectedActionDataInfo;
            SelectedActionData = selectedActionDataInfo != null
                ? AssetDatabase.LoadAssetAtPath<ActionData>(selectedActionDataInfo.path)
                : null;
            
            ClearSelectedActionItem();

            if (SelectedActionData != null)
                SelectedActionData.OnAfterDeserialize();

            OnChangeActionItemList();

            editorForTimeline.timeline_Time = 0f;
            editorForTimeline.Repaint();
            editorForInspector.Repaint();
        }

        public static void OnChangeActionItemList(bool bClearSelectedActionItems = true)
        {
            ItemInSelectedActionData.Clear();
            if (bClearSelectedActionItems)
                SelectedActionItems.Clear();

            if (SelectedActionData == null)
                return;
            
            switch (editorForTimeline.editTargetType)
            {
                case ActionTimelineEditor.EditTargetTypes.Event:
                    foreach (var eventData in SelectedActionData.listEventData)
                        ItemInSelectedActionData.Add(eventData);
                    break;
                case ActionTimelineEditor.EditTargetTypes.Trigger:
                    foreach (var eventData in SelectedActionData.listTriggersOnInput)
                        ItemInSelectedActionData.Add(eventData);
                    foreach (var eventData in SelectedActionData.listTriggersOnArea)
                        ItemInSelectedActionData.Add(eventData);
                    break;
            }
            
            if (editorForInspector)
                editorForInspector.Repaint();
        }

        public static void DeleteActionItem(IActionEditorItem actionItem)
        {
            ItemInSelectedActionData.Remove(actionItem);
            SelectedActionItems.Remove(actionItem);
            if (LastSelectedActionItem == actionItem)
                LastSelectedActionItem = null;

            if (SelectedActionData == null)
                return;

            switch (actionItem)
            {
                case ActionEventDataBase eventData :
                    SelectedActionData.Editor_RemoveActionEvent(eventData);
                    break;
                case ActionTriggerBase triggerData:
                    SelectedActionData.Editor_RemoveActionTrigger(triggerData);
                    break;
            }
        }

        public static void DeleteActionItem(List<IActionEditorItem> listActionItem)
        {
            if (listActionItem == null)
                return;

            foreach (var actionItem in listActionItem)
                DeleteActionItem(actionItem);
        }

        public static void AddSelectedActionItem(IActionEditorItem actionItem)
        {
            SelectedActionItems.Add(actionItem);
            LastSelectedActionItem = actionItem;

            if (editorForInspector != null)
                editorForInspector.Repaint();
        }

        public static void RemoveSelectedActionItem(IActionEditorItem actionItem)
        {
            SelectedActionItems.Remove(actionItem);
            if (LastSelectedActionItem == actionItem)
                LastSelectedActionItem = null;

            if (editorForInspector != null)
                editorForInspector.Repaint();
        }

        public static void ClearSelectedActionItem()
        {
            SelectedActionItems.Clear();
            LastSelectedActionItem = null;

            if (editorForInspector != null)
                editorForInspector.Repaint();
        }

        public static bool IsSelectedActionItem(IActionEditorItem actionItem)
        {
            return SelectedActionItems.Contains(actionItem);
        }
        
        public static int GetSelectedActionItemCount()
        {
            return SelectedActionItems?.Count ?? 0;
        }

        public static void CopySelectedActionItem()
        {
            CopyedActionItems.Clear();
            if (editorForTimeline == null)
                return;

            switch (editorForTimeline.editTargetType)
            {
                case ActionTimelineEditor.EditTargetTypes.Event:
                    foreach (var actionEditorItem in SelectedActionItems)
                    {
                        if (actionEditorItem is ActionEventDataBase data)
                            CopyedActionItems.Add(data.Copy());
                    }
                    break;
                case ActionTimelineEditor.EditTargetTypes.Trigger:
                    foreach (var actionEditorItem in SelectedActionItems)
                    {
                        if (actionEditorItem is ActionTriggerDurationBase data)
                            CopyedActionItems.Add(data.Copy());
                    }
                    break;
            }
        }

        public static void PasteActionItemToActionData()
        {
            if (SelectedActionData == null || CopyedActionItems.Count == 0)
                return;
            
            SelectedActionItems.Clear();
            
            switch (editorForTimeline.editTargetType)
            {
                case ActionTimelineEditor.EditTargetTypes.Event:
                    foreach (var actionEditorItem in CopyedActionItems)
                    {
                        if (actionEditorItem is ActionEventDataBase)
                        {
                            SelectedActionData.Editor_AddActionEventData(actionEditorItem.Copy() as ActionEventDataBase);
                            SelectedActionItems.Add(actionEditorItem);
                        }
                    }
                    break;
                case ActionTimelineEditor.EditTargetTypes.Trigger:
                    foreach (var actionEditorItem in CopyedActionItems)
                    {
                        if (actionEditorItem is ActionTriggerBase)
                        {
                            SelectedActionData.Editor_AddActionTriggerData(actionEditorItem.Copy() as ActionTriggerBase);
                            SelectedActionItems.Add(actionEditorItem);
                        }
                    }
                    break;
            }
            

            OnChangeActionItemList(false);
        }
    }
}