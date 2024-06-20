using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common;
using UnityEditor;
using UnityEngine;

namespace Actor
{

    public partial class ActionTimelineEditor : EditorWindow
    {
        public enum EditTargetTypes
        {
            Event,
            Trigger,
        }
        
        private enum MouseDragJobTypes
        {
            None,
            MoveScroll,
            MoveEventItem,
            MoveEventStartTime,
            MoveEventEndTime,
            MoveAreaSlider,
            MoveTimelineTime,
        }

        public EditTargetTypes editTargetType { get; private set; } = EditTargetTypes.Event;
        private bool isPlaying = false;
        private double dLastTimeSceneStartup = 0f;
        
        private MouseDragJobTypes _crrMouseDragJop = MouseDragJobTypes.None;
        private int _nMouseDragJobStartFrame;
        private int _nMouseDragJobDeltaFrame;

        private Vector2 _vScroll_Viewport;
        private bool _bHasVerticalScrollBar;
        private int _timeline_ScaleX = 0;
        public float timeline_Time = 0f;

        public static ActionTimelineEditor ShowWindow()
        {
            var wnd = GetWindow<ActionTimelineEditor>("Action Timeline");
            return wnd;
        }

        private void OnEnable()
        {
            ActionEditor.editorForTimeline = this;
            Undo.undoRedoPerformed += Repaint;
            dLastTimeSceneStartup = EditorApplication.timeSinceStartup;
        }

        private void Update()
        {
            var fDeltaTime = System.Convert.ToSingle(EditorApplication.timeSinceStartup - dLastTimeSceneStartup);
            dLastTimeSceneStartup = EditorApplication.timeSinceStartup;
            
            var actionData = ActionEditor.SelectedActionData;
            if (isPlaying && actionData != null && _crrMouseDragJop != MouseDragJobTypes.MoveTimelineTime)
            {
                timeline_Time += fDeltaTime;
                timeline_Time %= actionData.Length;
                Repaint();
                SampleActor();
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
        }
        
        private void OnGUI()
        {
            _OnGUI_CheckAndInit();

            _bHasVerticalScrollBar = Height_TotalEventItem > position.height - (Height_TopToolbar + Height_Timeline);

            _OnGUI_ExecuteDragJob();
            _OnGUI_TopMenu();
            _OnGUI_ItemHeader();
            _OnGUI_ItemList();
            _OnGUI_EventTimeline();
            _OnGUI_EventTimelineList();
            _OnGUI_AreaSlider();
        }

        private void _OnGUI_TopMenu()
        {
            Rect rtRootArea = default;
            rtRootArea.x = 0f;
            rtRootArea.y = 0f;
            rtRootArea.width = position.width;
            rtRootArea.height = Height_TopToolbar;

            var actionData = ActionEditor.SelectedActionData;
            if (isPlaying && actionData == null)
                isPlaying = false;
            
            using (new GUILayout.AreaScope(new Rect(0f, 0f, position.width, Height_TopToolbar)))
            {
                using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    if (GUILayout.Button(gotoBeginingContent, EditorStyles.toolbarButton) && actionData != null)
                    {
                        isPlaying = false;
                        timeline_Time = 0f;
                        SampleActor();
                    }

                    if (GUILayout.Button(previousFrameContent, EditorStyles.toolbarButton) && actionData != null)
                    {
                        isPlaying = false;
                        timeline_Time = (Mathf.CeilToInt(timeline_Time * ActionData.FRAME_RATE) - 1) / ActionData.FRAME_RATE;
                        SampleActor();
                    }

                    isPlaying = GUILayout.Toggle(isPlaying, playContent, EditorStyles.toolbarButton) && actionData != null;

                    if (GUILayout.Button(nextFrameContent, EditorStyles.toolbarButton) && actionData != null)
                    {
                        isPlaying = false;
                        timeline_Time = (Mathf.FloorToInt(timeline_Time * ActionData.FRAME_RATE) + 1) / ActionData.FRAME_RATE;
                        SampleActor();
                    }

                    if (GUILayout.Button(gotoEndContent, EditorStyles.toolbarButton) && actionData != null)
                    {
                        isPlaying = false;
                        timeline_Time = actionData.Length;
                        SampleActor();
                    }

                    if (actionData != null)
                        actionData.Editor_OnGUI_Length(GetSampleActor());

                    GUILayout.FlexibleSpace();

                    if (actionData != null && GUILayout.Button(actionData.name, EditorStyles.toolbarButton))
                        EditorGUIUtility.PingObject(actionData);

                    void DraeChangeEditTargetButton(EditTargetTypes editTarget)
                    {
                        var color = editTarget == editTargetType
                            ? Color_SelectedEvenEventItemBg
                            : GUI.backgroundColor;

                        using (new EditorGUILayoutEx.GUIBackgroundColorScope(color))
                        {
                            if (GUILayout.Button(editTarget.ToString()))
                            {
                                if (editTargetType == editTarget)
                                    return;

                                editTargetType = editTarget;
                                ActionEditor.OnChangeActionItemList();
                            }
                        }
                    }

                    DraeChangeEditTargetButton(EditTargetTypes.Event);
                    DraeChangeEditTargetButton(EditTargetTypes.Trigger);
                }
            }
        }

        private void _OnGUI_ItemHeader()
        {
            Rect rtRootArea = default;
            rtRootArea.x = 0f;
            rtRootArea.y = Height_TopToolbar;
            rtRootArea.width = Width_ItemList;
            rtRootArea.height = Height_Timeline;

            using (new GUILayout.AreaScope(new Rect(0f, Height_TopToolbar, Width_ItemList, Height_Timeline)))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(15f);
                    if (EditorGUILayout.DropdownButton(newContent, FocusType.Passive, EditorStyles.toolbarPopup))
                    {
                        if (ActionEditor.SelectedActionData == null)
                        {
                            ShowNotification(new GUIContent("Please select an Action"));
                        }
                        else
                        {
                            void _CreateSuccessCallback(IActionEditorItem newActionItem)
                            {
                                ActionEditor.OnChangeActionItemList();
                                ActionEditor.AddSelectedActionItem(newActionItem);
                                Repaint();
                            }
                            
                            switch (editTargetType)
                            {
                                case EditTargetTypes.Event:
                                    ActionEditor.SelectedActionData.Editor_ShowCreateActionEventMenu(
                                        newEventData
                                            => _CreateSuccessCallback(newEventData as IActionEditorItem));
                                    break;
                                case EditTargetTypes.Trigger:
                                    ActionEditor.SelectedActionData.Editor_ShowCreateTriggerMenu(
                                        newTriggerData
                                            => _CreateSuccessCallback(newTriggerData as IActionEditorItem));
                                    break;
                            }
                        }
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void _OnGUI_ItemList()
        {
            Rect rtRootArea = default;
            rtRootArea.x = 0f;
            rtRootArea.y = Height_TopToolbar + Height_Timeline;
            rtRootArea.width = Width_ItemList;
            rtRootArea.height = position.height - rtRootArea.y;

            var actionData = ActionEditor.SelectedActionData;
            var listActionItem = ActionEditor.ItemInSelectedActionData;
            if (actionData == null || listActionItem == null || listActionItem.Count == 0)
                return;

            using (new GUILayout.AreaScope(rtRootArea))
            {
                for (int i = 0; i < listActionItem.Count; ++i)
                {
                    var actionItem = listActionItem[i];
                    var fAreaStartPosY = -_vScroll_Viewport.y + i * Height_EventItem;
                    var rtEventArea = new Rect(0f, fAreaStartPosY, Width_ItemList - 1, Height_EventItem);
                    using (new GUILayout.AreaScope(rtEventArea))
                    {
                        var rtBg = new Rect(0f, 0f, Width_ItemList - 1, Height_EventItem);
                        if (ActionEditor.IsSelectedActionItem(actionItem))
                        {
                            EditorGUI.DrawRect(rtBg,
                                i % 2 == 0 ? Color_SelectedEvenEventItemBg : Color_SelectedOddEventItemBg);
                        }
                        else
                        {
                            if (i % 2 == 0)
                                EditorGUI.DrawRect(rtBg, Color_EventItemBg);
                        }

                        var rtEventName = rtBg;
                        rtEventName.x += 10f;
                        rtEventName.width = 120f;

                        var rtButton = rtBg;
                        rtButton.width = 130f;

                        GUI.Label(rtEventName, actionItem.Editor_TimelineItemName, GUIStyle_WhiteLabelMiddleLeft);

                        var rtFieldArea = rtBg;
                        rtFieldArea.x += rtEventName.xMax;
                        rtFieldArea.width -= rtFieldArea.x + 25f;

                        using (new GUILayout.AreaScope(rtFieldArea))
                        {
                            GUILayout.FlexibleSpace();
                            actionItem.Editor_OnGUI_TimelineItemHeader(actionData);
                            GUILayout.FlexibleSpace();
                        }

                        var rtMenu = rtFieldArea;
                        rtMenu.x = rtFieldArea.xMax;
                        rtMenu.width = 25f;

                        using (new GUILayout.AreaScope(rtMenu))
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(menuContent, GUIStyle.none))
                            {
                                var menu = new GenericMenu();

                                /* Tag :: Define New Event :: AddEventMenu */
                                menu.AddItem(new GUIContent("Delete This"), false,
                                    () =>
                                    {
                                        ActionEditor.DeleteActionItem(actionItem);
                                        
                                        if (EditorUtility.DisplayDialog("Delete Action Event", $"Delete {ActionEditor.LastSelectedActionItem.Editor_DisplayTypeName}", "OK", "Cancel"))
                                        {
                                            Undo.RecordObject(actionData, "ActionData :: Delete Action Event");

                                            foreach (var delActionItem in ActionEditor.SelectedActionItems)
                                                ActionEditor.DeleteActionItem(delActionItem);

                                            ActionEditor.ClearSelectedActionItem();
                                            EditorUtility.SetDirty(actionData);
                                            Repaint();
                                        }
                                    });

                                if (ActionEditor.GetSelectedActionItemCount() > 0)
                                {
                                    menu.AddItem(new GUIContent("Delete Selected With This"), false,
                                        () =>
                                        {
                                            var list_Item = ActionEditor.SelectedActionItems.ToList();
                                            list_Item.Add(actionItem);
                                            ActionEditor.DeleteActionItem(list_Item);
                                        });
                                    menu.AddItem(new GUIContent("Delete Selected Without This"), false,
                                        () =>
                                        {
                                            var list_Item = ActionEditor.SelectedActionItems.ToList();
                                            list_Item.Remove(actionItem);
                                            ActionEditor.DeleteActionItem(list_Item);
                                        });
                                }
                                else
                                {
                                    menu.AddDisabledItem(new GUIContent("Delete Selected With This"), false);
                                    menu.AddDisabledItem(new GUIContent("Delete Selected Without This"), false);
                                }
                                menu.ShowAsContext();
                            }
                            GUILayout.FlexibleSpace();
                        }
                    }
                }
            }
        }

        private void _OnGUI_EventTimeline()
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
                return;

            Rect rtRootArea = default;
            rtRootArea.x = Width_ItemList;
            rtRootArea.y = Height_TopToolbar;
            rtRootArea.width = position.width - (Width_ItemList + (_bHasVerticalScrollBar ? Width_ScrollBar : 0f));
            rtRootArea.height = Height_Timeline;

            switch (Event.current.type)
            {
                case EventType.KeyDown:
                {
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.C:
                        {
                            if (Event.current.control && Event.current.type == EventType.KeyDown)
                            {
                                ActionEditor.CopySelectedActionItem();
                                Event.current.Use();
                                Repaint();
                            }
                        } break;
                        case KeyCode.V:
                        {
                            if (Event.current.control && Event.current.type == EventType.KeyDown)
                            {
                                ActionEditor.PasteActionItemToActionData();
                                Event.current.Use();
                                Repaint();
                            }
                        } break;
                    }
                } break;
                case EventType.MouseDown:
                {
                    if (rtRootArea.Contains(Event.current.mousePosition))
                    {
                        _crrMouseDragJop = MouseDragJobTypes.MoveTimelineTime;
                        timeline_Time = GetFrameByEditorWindowPosX_Float(Event.current.mousePosition.x) / ActionData.FRAME_RATE;
                        timeline_Time = Mathf.Clamp(timeline_Time, 0f, actionData.Length);
                        Event.current.Use();

                        SampleActor();
                    }
                }
                break;
                case EventType.MouseUp:
                {
                    if (_crrMouseDragJop == MouseDragJobTypes.MoveTimelineTime)
                        _SetMouseDragJob(MouseDragJobTypes.None);
                } break;
            }

            Rect GetRect_Label(int nFrame)
            {
                var posX = GetTimelineRulerAreaPosXByFrame(nFrame);
                var result = Rect.zero;
                result.x = posX + 1f;
                result.y = -5f;
                result.width = 200f;
                result.height = Height_Timeline;
                return result;
            }

            Rect GetRect_RulerLine(float nFrame, int lineSize)
            {
                var posX = GetTimelineRulerAreaPosXByFrame(nFrame);
                var result = Rect.zero;
                result.x = posX - 0.5f;
                result.xMax = posX + 0.5f;
                result.height = Height_Timeline * (0.75f - 0.15f * lineSize);
                result.y = Height_Timeline - result.height;
                return result;
            }

            using (new GUILayout.AreaScope(rtRootArea))
            {
                var nInterval = GetRulerFrameInterval();
                var nIntervalUnit = nInterval / 10;

                var startFrame = GetFrameByEditorWindowPosX_Int(rtRootArea.x) / nInterval * nInterval;
                var endFrame = GetFrameByEditorWindowPosX_Int(rtRootArea.xMax) / nInterval * nInterval;

                for (int i = startFrame; i <= endFrame; i += nInterval)
                {
                    EditorGUI.LabelField(GetRect_Label(i), i.ToString());

                    EditorGUI.DrawRect(GetRect_RulerLine(i, 0), Color_Ruler);
                    var halfFrame = i + nInterval / 2;
                    EditorGUI.DrawRect(GetRect_RulerLine(halfFrame, 1), Color_Ruler);
                    for (int j = i + nIntervalUnit; j < halfFrame; j += nIntervalUnit)
                        EditorGUI.DrawRect(GetRect_RulerLine(j, 2), Color_Ruler);
                    for (int j = halfFrame + nIntervalUnit; j < i + nInterval; j += nIntervalUnit)
                        EditorGUI.DrawRect(GetRect_RulerLine(j, 2), Color_Ruler);
                }
                
                // Draw :: _timeline_Time
                var crrFrame = timeline_Time * ActionData.FRAME_RATE; 
                EditorGUI.DrawRect(GetRect_RulerLine(crrFrame, 2), Color.white);
            }
        }

        private void _OnGUI_EventTimelineList()
        {
            Rect rtRootArea = rtTimelineListArea;

            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
            {
                EditorGUI.LabelField(rtRootArea, "Please select an Action", GUIStyle_WhiteLabelMiddleCenter);
                return;
            }

            if (actionData.Length < float.Epsilon)
            {
                EditorGUI.LabelField(rtRootArea, "length of the Action is 0", GUIStyle_WhiteLabelMiddleCenter);
                return;
            }

            var listEventData = ActionEditor.ItemInSelectedActionData;
            if (listEventData == null || listEventData.Count == 0)
            {
                EditorGUI.LabelField(rtRootArea, "Please Add Data", GUIStyle_WhiteLabelMiddleCenter);
                return;
            }

            if (_crrMouseDragJop == MouseDragJobTypes.None)
            {
                for (int i = 0; i < listEventData.Count; ++i)
                {
                    GetRect_EventTime(i, out var rtBody, out var rtStartTime, out var rtEndTime);

                    EditorGUIUtility.AddCursorRect(rtBody, MouseCursor.SlideArrow);
                    EditorGUIUtility.AddCursorRect(rtStartTime, MouseCursor.SplitResizeLeftRight);
                    if (listEventData[i].HasEndTime)
                        EditorGUIUtility.AddCursorRect(rtEndTime, MouseCursor.SplitResizeLeftRight);
                }
            }

            Rect rtViewportArea = rtRootArea;
            if (_bHasVerticalScrollBar)
                rtViewportArea.xMax -= Width_ScrollBar;
            if (_timeline_ScaleX > 0)
                rtViewportArea.yMax -= Width_ScrollBar;

            /* User Input Event */
            switch (Event.current.type)
            {
                case EventType.KeyDown:
                {
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Delete:
                        {
                            _DeleteActionItems(ActionEditor.SelectedActionItems.ToList());
                            Event.current.Use();
                        } break;
                    }
                } break;
                case EventType.ScrollWheel:
                {
                    if (Event.current.control
                        && rtRootArea.Contains(Event.current.mousePosition))
                    {
                        var deltaValue = Mathf.RoundToInt(Event.current.delta.y);
                        if (deltaValue != 0)
                        {
                            deltaValue /= -Mathf.Abs(deltaValue);
                            _timeline_ScaleX += deltaValue;
                            if (_timeline_ScaleX < 0)
                                _timeline_ScaleX = 0;

                            Event.current.Use();
                        }
                    }
                }
                    break;
                case EventType.MouseDown:
                {
                    _SetMouseDragJob(MouseDragJobTypes.None);
                    switch (Event.current.button)
                    {
                        case 0: // Click Left
                            if (rtViewportArea.Contains(Event.current.mousePosition))
                            {
                                var mouseViewportPos = Event.current.mousePosition - rtViewportArea.position;
                                var idx_EventItem =
                                    Mathf.FloorToInt((mouseViewportPos.y + _vScroll_Viewport.y) / Height_EventItem);

                                if (listEventData.CheckIndex(idx_EventItem))
                                {
                                    GetRect_EventTime(idx_EventItem, out var rtBody, out var rtStartTime,
                                        out var rtEndTime);

                                    /* On Click Event */
                                    if (rtBody.Contains(Event.current.mousePosition))
                                        _SetMouseDragJob(MouseDragJobTypes.MoveEventItem);
                                    else if (rtStartTime.Contains(Event.current.mousePosition))
                                        _SetMouseDragJob(MouseDragJobTypes.MoveEventStartTime);
                                    else if (listEventData[idx_EventItem].HasEndTime &&
                                             rtEndTime.Contains(Event.current.mousePosition))
                                        _SetMouseDragJob(MouseDragJobTypes.MoveEventEndTime);

                                    /* On Click BG */
                                    if (Event.current.control == false)
                                        ActionEditor.ClearSelectedActionItem();

                                    ActionEditor.AddSelectedActionItem(listEventData[idx_EventItem]);
                                    Event.current.Use();
                                }
                            }

                            break;
                        case 1: // Click Right
                            if (Event.current.control && rtViewportArea.Contains(Event.current.mousePosition))
                            {
                                /* On Click BG */
                                var mouseViewportPos = Event.current.mousePosition - rtViewportArea.position;
                                var idx_EventItem =
                                    Mathf.FloorToInt((mouseViewportPos.y + _vScroll_Viewport.y) / Height_EventItem);
                                if (listEventData.CheckIndex(idx_EventItem))
                                {
                                    ActionEditor.RemoveSelectedActionItem(listEventData[idx_EventItem]);
                                }
                            }
                            else
                            {
                                ActionEditor.ClearSelectedActionItem();
                            }

                            Event.current.Use();
                            Repaint();
                            break;
                        case 2: // Click Wheel
                            _SetMouseDragJob(MouseDragJobTypes.MoveScroll);
                            break;
                    }
                }
                    break;
                case EventType.MouseUp:
                {
                    switch (Event.current.button)
                    {
                        case 0: // Click Left
                        {
                            switch (_crrMouseDragJop)
                            {
                                case MouseDragJobTypes.MoveEventItem:
                                {
                                    if (_nMouseDragJobDeltaFrame != 0)
                                    {
                                        Undo.RecordObject(actionData, "ActionData :: Move Event");
                                        foreach (var eventData in ActionEditor.SelectedActionItems)
                                        {
                                            eventData.Editor_StartFrame =
                                                Mathf.Clamp(eventData.Editor_StartFrame + _nMouseDragJobDeltaFrame, 0,
                                                    actionData.Frame);
                                            eventData.Editor_EndFrame =
                                                Mathf.Clamp(eventData.Editor_EndFrame + _nMouseDragJobDeltaFrame, 0,
                                                    actionData.Frame);
                                        }

                                        if (ActionEditor.editorForInspector != null)
                                            ActionEditor.editorForInspector.Repaint();

                                    }
                                }
                                    break;
                                case MouseDragJobTypes.MoveEventStartTime:
                                {
                                    Undo.RecordObject(actionData, "ActionData :: Move Event's StartTime");
                                    if (_nMouseDragJobDeltaFrame != 0)
                                    {
                                        foreach (var eventData in ActionEditor.SelectedActionItems)
                                        {
                                            eventData.Editor_StartFrame =
                                                Mathf.Clamp(eventData.Editor_StartFrame + _nMouseDragJobDeltaFrame, 0,
                                                    actionData.Frame);
                                        }

                                        if (ActionEditor.editorForInspector != null)
                                            ActionEditor.editorForInspector.Repaint();
                                    }
                                }
                                    break;
                                case MouseDragJobTypes.MoveEventEndTime:
                                {
                                    Undo.RecordObject(actionData, "ActionData :: Move Event's EndTime");
                                    if (_nMouseDragJobDeltaFrame != 0)
                                    {
                                        foreach (var eventData in ActionEditor.SelectedActionItems)
                                            eventData.Editor_EndFrame =
                                                Mathf.Clamp(eventData.Editor_EndFrame + _nMouseDragJobDeltaFrame, 0,
                                                    actionData.Frame);

                                        if (ActionEditor.editorForInspector != null)
                                            ActionEditor.editorForInspector.Repaint();
                                    }
                                }
                                    break;
                            }
                        }
                            break;
                        case 1: // Click Right
                        {

                        }
                            break;
                        case 2: // Click Wheel
                        {

                        }
                            break;
                    }

                    _SetMouseDragJob(MouseDragJobTypes.None);
                }
                    break;
            }

            /* Draw */
            using (new GUILayout.AreaScope(rtRootArea))
            {
                using (var scrollScope = new EditorGUILayout.ScrollViewScope(_vScroll_Viewport))
                {
                    var vViewportSize = vTimelineListViewportSize;
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(vViewportSize.x),
                               GUILayout.Height(vViewportSize.y)))
                    {
                        var fFrameToPosX = 1 / (actionData.Length * ActionData.FRAME_RATE) * vViewportSize.x;

                        GUILayout.FlexibleSpace();

                        // Draw BG
                        for (int i = 0; i < listEventData.Count; ++i)
                        {
                            var eventData = listEventData[i];
                            var rtBg = new Rect(_vScroll_Viewport.x, i * Height_EventItem, rtRootArea.width,
                                Height_EventItem);

                            if (ActionEditor.IsSelectedActionItem(eventData))
                            {
                                EditorGUI.DrawRect(rtBg,
                                    i % 2 == 0 ? Color_SelectedEvenEventItemBg : Color_SelectedOddEventItemBg);
                            }
                            else
                            {
                                if (i % 2 == 0)
                                    EditorGUI.DrawRect(rtBg, Color_EventItemBg);
                            }
                        }

                        // Draw Ruler
                        var nInterval = GetRulerFrameInterval() / 2;
                        var startFrame = GetFrameByEditorWindowPosX_Int(rtRootArea.x) / nInterval * nInterval;
                        var endFrame = GetFrameByEditorWindowPosX_Int(rtRootArea.xMax) / nInterval * nInterval;

                        for (int i = startFrame; i <= endFrame; i += nInterval)
                        {
                            var rtRuler = Rect.zero;
                            rtRuler.x = i * fFrameToPosX - 0.5f;
                            rtRuler.width = 1f;
                            rtRuler.y = _vScroll_Viewport.y;
                            rtRuler.height = rtRootArea.height;
                            EditorGUI.DrawRect(rtRuler, Color_Ruler);
                        }

                        var rtStartFrameLine = Rect.zero;
                        var rtEndFrameLine = Rect.zero;

                        // Draw Event
                        for (int i = 0; i < listEventData.Count; ++i)
                        {
                            var eventData = listEventData[i];

                            int nStartAddFrame = 0, nEndAddFrame = 0;
                            bool bUseStartFrameLine = false, bUseEndFrameLine = false;
                            if (ActionEditor.IsSelectedActionItem(eventData))
                            {
                                switch (_crrMouseDragJop)
                                {
                                    case MouseDragJobTypes.MoveEventItem:
                                    {
                                        nStartAddFrame = _nMouseDragJobDeltaFrame;
                                        nEndAddFrame = _nMouseDragJobDeltaFrame;
                                        if (ActionEditor.LastSelectedActionItem == eventData)
                                        {
                                            bUseStartFrameLine = true;
                                            bUseEndFrameLine = eventData.HasEndTime;
                                        }
                                    }
                                        break;
                                    case MouseDragJobTypes.MoveEventStartTime:
                                    {
                                        nStartAddFrame = _nMouseDragJobDeltaFrame;
                                        if (ActionEditor.LastSelectedActionItem == eventData)
                                            bUseStartFrameLine = true;

                                    }
                                        break;
                                    case MouseDragJobTypes.MoveEventEndTime:
                                    {
                                        if (eventData.HasEndTime)
                                        {
                                            nEndAddFrame = _nMouseDragJobDeltaFrame;
                                            if (ActionEditor.LastSelectedActionItem == eventData)
                                                bUseEndFrameLine = eventData.HasEndTime;
                                        }

                                    }
                                        break;
                                }
                            }

                            var rtEventDataAreaOutside = Rect.zero;
                            rtEventDataAreaOutside.x = (eventData.Editor_StartFrame + nStartAddFrame) * fFrameToPosX;
                            rtEventDataAreaOutside.y = i * Height_EventItem;
                            rtEventDataAreaOutside.height = Height_EventItem;

                            if (eventData.HasEndTime)
                            {
                                rtEventDataAreaOutside.xMax = (eventData.Editor_EndFrame + nEndAddFrame) * fFrameToPosX;

                                var rtEventDataAreaInside = rtEventDataAreaOutside;
                                rtEventDataAreaInside.x += 1f;
                                rtEventDataAreaInside.width -= 2f;
                                rtEventDataAreaInside.y += 1f;
                                rtEventDataAreaInside.height -= 2f;

                                EditorGUI.DrawRect(rtEventDataAreaOutside, Color.white);
                                EditorGUI.DrawRect(rtEventDataAreaInside, Color_EventItem);
                                EditorGUI.LabelField(rtEventDataAreaInside, eventData.Editor_TimelineItemDesc,
                                    GUIStyle_WhiteLabelMiddleCenter);
                            }
                            else
                            {
                                rtEventDataAreaOutside.width = Tex2D_HasNotEndTime.width;
                                GUI.DrawTexture(rtEventDataAreaOutside, Tex2D_HasNotEndTime);

                                rtEventDataAreaOutside.width = GUIStyle_WhiteLabelMiddleCenter
                                    .CalcSize(new GUIContent(eventData.Editor_TimelineItemDesc)).x + 20f;
                                EditorGUI.LabelField(rtEventDataAreaOutside, eventData.Editor_TimelineItemDesc,
                                    GUIStyle_WhiteLabelMiddleCenter);
                            }

                            if (bUseStartFrameLine)
                            {
                                rtStartFrameLine.x = rtEventDataAreaOutside.x - 0.5f;
                                rtStartFrameLine.width = 1f;
                                rtStartFrameLine.y = _vScroll_Viewport.y;
                                rtStartFrameLine.height = rtRootArea.height;
                            }

                            if (bUseEndFrameLine)
                            {
                                rtEndFrameLine.x = rtEventDataAreaOutside.xMax - 0.5f;
                                rtEndFrameLine.width = 1f;
                                rtEndFrameLine.y = _vScroll_Viewport.y;
                                rtEndFrameLine.height = rtRootArea.height;
                            }
                        }

                        if (rtStartFrameLine != Rect.zero)
                            EditorGUI.DrawRect(rtStartFrameLine, Color.white);
                        if (rtEndFrameLine != Rect.zero)
                            EditorGUI.DrawRect(rtEndFrameLine, Color.white);

                        // Draw :: _timeline_Time
                        var crrFrame = timeline_Time * ActionData.FRAME_RATE;
                        var rtCrrFrame = Rect.zero;
                        rtCrrFrame.x = crrFrame * fFrameToPosX - 0.5f;
                        rtCrrFrame.width = 1f;
                        rtCrrFrame.y = _vScroll_Viewport.y;
                        rtCrrFrame.height = rtRootArea.height;
                        EditorGUI.DrawRect(rtCrrFrame, Color.white);
                    }


                    if (_vScroll_Viewport != scrollScope.scrollPosition)
                    {
                        _vScroll_Viewport = scrollScope.scrollPosition;
                        Repaint();
                    }
                }
            }
        }

        private void _OnGUI_AreaSlider()
        {
            /* User Input Event */
            if (_crrMouseDragJop == MouseDragJobTypes.None)
                EditorGUIUtility.AddCursorRect(rtAreaSlider, MouseCursor.SplitResizeLeftRight);

            if (Event.current.isMouse && Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (rtAreaSlider.Contains(Event.current.mousePosition))
                        {
                            _SetMouseDragJob(MouseDragJobTypes.MoveAreaSlider);
                            Event.current.Use();
                        }
                        break;
                    case EventType.MouseUp:
                        if (_crrMouseDragJop == MouseDragJobTypes.MoveAreaSlider)
                        {
                            _SetMouseDragJob(MouseDragJobTypes.None);
                            Event.current.Use();
                        }
                        break;
                }
            }

            /* Draw */
            EditorGUI.DrawRect(rtAreaSlider, Color.gray);
        }

        private void _OnGUI_ExecuteDragJob()
        {
            if (_crrMouseDragJop == MouseDragJobTypes.None)
                return;

            var cursor = MouseCursor.Arrow;
            _nMouseDragJobDeltaFrame =
                GetFrameByEditorWindowPosX_Int(Event.current.mousePosition.x) - _nMouseDragJobStartFrame;
            switch (_crrMouseDragJop)
            {
                case MouseDragJobTypes.MoveEventItem:
                {
                    cursor = MouseCursor.SlideArrow;
                }
                    break;
                case MouseDragJobTypes.MoveEventStartTime:
                {
                    cursor = MouseCursor.SplitResizeLeftRight;
                }
                    break;
                case MouseDragJobTypes.MoveEventEndTime:
                {
                    cursor = MouseCursor.SplitResizeLeftRight;
                }
                    break;
                case MouseDragJobTypes.MoveScroll:
                {
                    cursor = MouseCursor.Pan;
                    if (Event.current.type == EventType.MouseDrag)
                        _vScroll_Viewport -= Event.current.delta;
                }
                    break;
                case MouseDragJobTypes.MoveAreaSlider:
                {
                    cursor = MouseCursor.SplitResizeLeftRight;
                    Width_ItemList = Mathf.Clamp(Event.current.mousePosition.x, MinWidth_EventItem,
                        MaxWidth_EventItem);
                }
                    break;
                case MouseDragJobTypes.MoveTimelineTime:
                {
                    var actionData = ActionEditor.SelectedActionData;
                    if (actionData != null)
                    {
                        var newTimeline_Time = GetFrameByEditorWindowPosX_Float(Event.current.mousePosition.x) / ActionData.FRAME_RATE;
                        newTimeline_Time = Mathf.Clamp(newTimeline_Time, 0f, actionData.Length);

                        if (newTimeline_Time != timeline_Time)
                        {
                            timeline_Time = newTimeline_Time;
                            SampleActor();
                        }
                    }
                }
                    break;
            }

            EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), cursor);
            Repaint();
        }

        private void _DeleteActionItems(List<IActionEditorItem> listActionItems)
        {
            var actionData = ActionEditor.SelectedActionData;
            if (listActionItems == null || listActionItems.Count == 0)
                return;
            
            var title = "Delete Action Event";
            var Desc = listActionItems.Count == 1
                ? $"Delete \"{listActionItems[0].Editor_DisplayTypeName}\" Action Item"
                : $"Delete \"{listActionItems[0].Editor_DisplayTypeName}\" And {listActionItems.Count - 1} ActionItems";

            if (EditorUtility.DisplayDialog(title, Desc, "OK", "Cancel"))
            {
                Undo.RecordObject(actionData, "ActionData :: Delete Action Event");
                ActionEditor.DeleteActionItem(listActionItems);
                EditorUtility.SetDirty(actionData);
                Repaint();
            }
        }

        private void _SetMouseDragJob(MouseDragJobTypes mouseDragJobType)
        {
            _crrMouseDragJop = mouseDragJobType;
            _nMouseDragJobStartFrame = GetFrameByEditorWindowPosX_Int(Event.current.mousePosition.x);
        }

        public void SampleActor()
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
                return;

            var actor = GetSampleActor();
            if (actor == null)
                return;

            actionData.Editor_SampleActionEvent(actor, timeline_Time, ActionEditor.SelectedActionItems);
        }

        public Actor GetSampleActor()
        {
            if (Selection.activeGameObject == null)
                return null;

            var result = Selection.activeGameObject.GetComponent<Actor>();
            if (result)
                return result;

            result = Selection.activeGameObject.GetComponentInParent<Actor>();
            return result;
        }
    }

    public partial class ActionTimelineEditor
    {
        private GUIContent playContent;
        private GUIContent gotoBeginingContent;
        private GUIContent gotoEndContent;
        private GUIContent nextFrameContent;
        private GUIContent previousFrameContent;
        private GUIContent newContent;
        private GUIContent menuContent;

        private const float Height_TopToolbar = 20f;
        private const float Height_Timeline = 25f;
        private const float Height_EventItem = 40f;
        private const float MinWidth_EventItem = 200f;
        private const float MaxWidth_EventItem = 600f;
        private const float Width_ScrollBar = 13f;

        private static readonly Color Color_EventItem = new Color(0f, 0f, 0.5f, 1f);
        private static readonly Color Color_EventItemBg = Color.gray * 0.75f;

        private static readonly Color Color_SelectedOddEventItemBg = new(0.5f, 165f / 255f * 0.5f, 0f);
        private static readonly Color Color_SelectedEvenEventItemBg = new(0.75f, 165f / 255f * 0.75f, 0f);

        private static readonly Color Color_Ruler = Color.white * 0.8f;

        private GUIStyle GUIStyle_WhiteLabelMiddleLeft;
        private GUIStyle GUIStyle_WhiteLabelMiddleCenter;

        private Texture2D Tex2D_HasNotEndTime;

        private float Width_ItemList = 300f;

        private Rect rtAreaSlider =>
            new Rect(Width_ItemList - 2f, Height_TopToolbar, 2f, position.height - Height_TopToolbar);

        private Rect rtTimelineListArea
        {
            get
            {
                Rect rtRootArea = default;
                rtRootArea.x = Width_ItemList;
                rtRootArea.y = Height_TopToolbar + Height_Timeline;
                rtRootArea.width = position.width - Width_ItemList;
                rtRootArea.height = position.height - rtRootArea.y;

                return rtRootArea;
            }
        }

        private Vector2 vTimelineListViewportSize => new()
        {
            x = (1f + _timeline_ScaleX * 0.1f) *
                (rtTimelineListArea.width - (_bHasVerticalScrollBar ? Width_ScrollBar : 0f)),
            y = ActionEditor.ItemInSelectedActionData == null
                ? 0
                : ActionEditor.ItemInSelectedActionData.Count * Height_EventItem,
        };

        private float Height_TotalEventItem
        {
            get
            {
                var actionData = ActionEditor.SelectedActionData;
                if (actionData == null || actionData.listEventData == null)
                    return 0;

                return actionData.listEventData.Count * Height_EventItem;
            }
        }

        private void _OnGUI_CheckAndInit()
        {
            newContent ??= L10n.IconContent("CreateAddNew", "Add new ActionEvent.");
            playContent ??= L10n.IconContent("Animation.Play", "Play the Action");
            gotoBeginingContent ??= L10n.IconContent("Animation.FirstKey", "Go to the beginning of the Action");
            gotoEndContent ??= L10n.IconContent("Animation.LastKey", "Go to the end of the Action");
            nextFrameContent ??= L10n.IconContent("Animation.NextKey", "Go to the next frame");
            previousFrameContent ??= L10n.IconContent("Animation.PrevKey", "Go to the previous frame");
            menuContent ??= EditorGUIUtility.IconContent("_Menu@2x", "");

            GUIStyle_WhiteLabelMiddleLeft ??= new GUIStyle(EditorStyles.whiteLabel)
                { alignment = TextAnchor.MiddleLeft };
            GUIStyle_WhiteLabelMiddleCenter ??= new GUIStyle(EditorStyles.whiteLabel)
                { alignment = TextAnchor.MiddleCenter };

            if (Tex2D_HasNotEndTime == null)
                Tex2D_HasNotEndTime = CreateEventItemTexture(Color_EventItem, Color.white);
        }

        private void GetRect_EventTime(int idx, out Rect rtBody, out Rect rtStartTime, out Rect rtEndTime)
        {
            rtBody = default;
            rtStartTime = default;
            rtEndTime = default;

            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
                return;

            var listEventData = ActionEditor.ItemInSelectedActionData;
            if (!listEventData.CheckIndex(idx))
                return;

            var eventData = listEventData[idx];

            var fStartPosX = Width_ItemList + (eventData.Editor_StartFrame / (float)actionData.Frame * vTimelineListViewportSize.x) -
                             _vScroll_Viewport.x;
            var fEndPosX = eventData.HasEndTime
                ? Width_ItemList + (eventData.Editor_EndFrame / (float)actionData.Frame * vTimelineListViewportSize.x) -
                  _vScroll_Viewport.x
                : fStartPosX +
                  GUIStyle_WhiteLabelMiddleCenter.CalcSize(new GUIContent(eventData.Editor_TimelineItemDesc)).x + 20f -
                  _vScroll_Viewport.x;

            var yPos = 0f;
            yPos += Height_Timeline + Height_TopToolbar;
            yPos -= _vScroll_Viewport.y;
            yPos += idx * Height_EventItem;

            rtBody.x = fStartPosX + 2f;
            rtBody.y = yPos;
            rtBody.width = fEndPosX - fStartPosX - 4f;
            rtBody.height = Height_EventItem;

            rtStartTime.x = fStartPosX - 2f;
            rtStartTime.y = yPos;
            rtStartTime.height = Height_EventItem;
            rtStartTime.width = 4f;

            rtEndTime.x = fEndPosX - 2f;
            rtEndTime.y = yPos;
            rtEndTime.height = Height_EventItem;
            rtEndTime.width = 4f;
        }

        private int GetFrameByEditorWindowPosX_Int(float fPosX)
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
                return 0;

            fPosX -= rtTimelineListArea.x;
            var fTime = (fPosX + _vScroll_Viewport.x) / vTimelineListViewportSize.x * actionData.Length;
            return Mathf.FloorToInt(fTime * ActionData.FRAME_RATE);
        }

        private float GetFrameByEditorWindowPosX_Float(float fPosX)
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
                return 0;

            fPosX -= rtTimelineListArea.x;
            var fTime = (fPosX + _vScroll_Viewport.x) / vTimelineListViewportSize.x * actionData.Length;
            return fTime * ActionData.FRAME_RATE;
        }


        private float GetTimelineRulerAreaPosXByFrame(float nFrame)
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
                return 0;

            var fTime = nFrame / ActionData.FRAME_RATE;
            return fTime * vTimelineListViewportSize.x / actionData.Length - _vScroll_Viewport.x;
        }

        private int GetRulerFrameInterval()
        {
            var actionData = ActionEditor.SelectedActionData;
            if (actionData == null)
                return 0;

            var f = actionData.Frame / vTimelineListViewportSize.x * 200f;
            var nInterval = Mathf.FloorToInt(f / 10) * 10;
            if (nInterval <= 0)
                nInterval = 10;

            return nInterval;
        }

        private Texture2D CreateEventItemTexture(Color32 color, Color32 color_Outline)
        {
            var tex2D = new Texture2D(150, (int)Height_EventItem);
            tex2D.wrapMode = TextureWrapMode.Clamp;
            var fillColorArray = tex2D.GetPixels32();
            var fInvWidth = 1f / (float)tex2D.width;

            Color32 color32 = color;

            for (var i = 0; i < tex2D.width; ++i)
            {
                color32.a = i < 50
                    ? (byte)255
                    : (byte)(255 * (1f - (i - 49f) * 0.01f));

                for (int j = 0; j < tex2D.height; ++j)
                    fillColorArray[j * tex2D.width + i] = color32;
            }

            color32 = color_Outline;
            var outlineIndex = (tex2D.height - 1) * tex2D.width;
            for (var i = 0; i < tex2D.width; ++i)
            {
                color32.a = fillColorArray[i].a;
                fillColorArray[i] = color32;
                fillColorArray[outlineIndex + i] = color32;
            }

            for (var i = 0; i < tex2D.height; ++i)
                fillColorArray[tex2D.width * i] = color_Outline;


            tex2D.SetPixels32(fillColorArray);
            tex2D.Apply();
            return tex2D;
        }
    }
}