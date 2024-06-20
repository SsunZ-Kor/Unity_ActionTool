using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    public partial class ActionInspectorEditor : EditorWindow
    {
        private Vector2 vScroll;

        public static ActionInspectorEditor ShowWindow()
        {
            var wnd = GetWindow<ActionInspectorEditor>("Action Inspector");
            return wnd;
        }

        private void OnEnable()
        {
            ActionEditor.editorForInspector = this;
        }

        private void OnGUI()
        {
            OnGUI_CheckAndInit();

            OnGUI_ActionDataInfo();

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(vScroll))
            {
                vScroll = scrollScope.scrollPosition;
                OnGUI_ActionEndTriggerInfo();
                OnGUI_ActionEventInfo();
            }
        }

        private void OnGUI_ActionDataInfo()
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Please select Action", guiStyle_MiddelCenterLargeLabel);
                GUILayout.FlexibleSpace();
                return;
            }


            using (new EditorGUILayout.HorizontalScope(guiStyle_TitleBar))
            {
                EditorGUILayout.LabelField("ACTION", guiStyle_MiddelCenterLargeLabel);
            }

            actionData.Editor_OnGUI_Inspector();
        }

        private void OnGUI_ActionEndTriggerInfo()
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null || actionData.Loop)
                return;

            var listTriggerData = ActionEditor.SelectedActionData.listTriggersOnEndAction;
            if (listTriggerData == null)
                return;
            
            using (new EditorGUILayout.HorizontalScope(guiStyle_TitleBar))
            {
                EditorGUILayout.LabelField("END TRIGGERS", guiStyle_MiddelCenterLargeLabel);
                if (GUILayout.Button(menuContent, GUIStyle.none, GUILayout.Width(15f)))
                {
                    var menu = new GenericMenu();
                    
                    menu.AddItem(new GUIContent("Add Trigger OnEndAction"), false, 
                        () => listTriggerData.Add(new()));
                    
                    menu.ShowAsContext();
                }
            }
            
            if (listTriggerData.Count == 0)
            {
                using (new EditorGUILayoutEx.GUIContentColorScope(Color.red))
                    EditorGUILayout.LabelField("List is Empty", guiStyle_MiddelCenterLargeLabel);

                return;
            }

            int delIdx = -1;
            for (var i = 0; i < listTriggerData.Count; i++)
            {
                var eventData = listTriggerData[i];
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(eventData.Editor_DisplayDesc, guiStyle_MiddleLeftLargeLabel);
                        GUILayout.FlexibleSpace();
                        using (new EditorGUILayoutEx.GUIBackgroundColorScope(Color.red))
                        {
                            if (GUILayout.Button("X"))
                                delIdx = i;
                        }
                    }

                    eventData.Editor_OnGUI_InspectorContent(actionData);
                }
            }
            
            if (listTriggerData.CheckIndex(delIdx))
                actionData.Editor_RemoveActionTrigger(listTriggerData[delIdx]);
        }

        private void OnGUI_ActionEventInfo()
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
                return;

            var listEventData = ActionEditor.ItemInSelectedActionData;
            if (listEventData == null)
                return;

            var timelineEditor = ActionEditor.editorForTimeline;
            if (timelineEditor == null)
                return;
            
            using (new EditorGUILayout.HorizontalScope(guiStyle_TitleBar))
            {
                var title = timelineEditor.editTargetType switch
                {
                    ActionTimelineEditor.EditTargetTypes.Event => "EVENTS",
                    ActionTimelineEditor.EditTargetTypes.Trigger => "TRIGGERS",
                    _ => string.Empty,
                };
                
                EditorGUILayout.LabelField(title, guiStyle_MiddelCenterLargeLabel);
            }

            if (listEventData.Count == 0)
            {
                EditorGUILayout.LabelField("List is Empty", guiStyle_MiddelCenterLargeLabel);
                return;
            }


            bool bEventChanged = false;
            foreach (var eventData in listEventData)
            {
                if (ActionEditor.IsSelectedActionItem(eventData))
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUILayout.LabelField(eventData.Editor_TimelineItemDesc, guiStyle_MiddleLeftLargeLabel);
                        bEventChanged |= eventData.Editor_OnGUI_InspectorHeader(actionData);
                    }
                }
            }

            if (bEventChanged && ActionEditor.editorForTimeline != null)
                ActionEditor.editorForTimeline.Repaint();

        }
    }

    public partial class ActionInspectorEditor
    {
        private GUIStyle guiStyle_TitleBar;
        private GUIStyle guiStyle_MiddleCenterLabel;
        private GUIStyle guiStyle_MiddelCenterLargeLabel;
        private GUIStyle guiStyle_MiddleLeftLargeLabel;
        private GUIContent menuContent;

        private Color color_TitleBar = new Color(0f, 0f, 0.5f, 1f);

        private void OnGUI_CheckAndInit()
        {
            if (guiStyle_TitleBar == null)
            {
                guiStyle_TitleBar = new GUIStyle(GUI.skin.box);
                guiStyle_TitleBar.normal.background = new Texture2D(1, 1);
                guiStyle_TitleBar.normal.background.SetColor(color_TitleBar);
            }

            guiStyle_MiddleCenterLabel ??= new(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            guiStyle_MiddelCenterLargeLabel ??= new(EditorStyles.largeLabel) { alignment = TextAnchor.MiddleCenter };

            if (guiStyle_MiddleLeftLargeLabel == null)
            {
                guiStyle_MiddleLeftLargeLabel = new(EditorStyles.largeLabel) { alignment = TextAnchor.MiddleLeft };
                guiStyle_MiddleLeftLargeLabel.fontStyle = FontStyle.Bold;
                guiStyle_MiddleLeftLargeLabel.normal.textColor = Color.white;
            }
            
            menuContent ??= EditorGUIUtility.IconContent("_Menu@2x", "");
        }
    }
}