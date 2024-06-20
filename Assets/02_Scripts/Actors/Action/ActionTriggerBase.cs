using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Flags]
    public enum TriggerAreaTypes
    {
        Ground = 1 << 0,
        Slope = 1 << 1,
        Air = 1 << 2,
    }
    
    [System.Flags]
    public enum TriggerJoysickTypes
    {
        Neutral = 1 << 0,
        Forward = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        Backward = 1 << 4,
    }
    
    public abstract class ActionTriggerBase
    {
        public virtual bool HasEndTime => false;
        public string nextActionName;

        public ActionTriggerBase() {}

        public ActionTriggerBase(ActionTriggerBase prvEventData)
        {
            nextActionName = string.Copy(prvEventData.nextActionName);
        }
        
        #if UNITY_EDITOR
        protected GUIContent _guiContent_NextAction = new GUIContent("Next", "재생 할 Action 파일명");
        #endif
    }

    public abstract partial class ActionTriggerDurationBase : ActionTriggerBase
    {
        public override bool HasEndTime => true;
        
        [SerializeField] protected float _startTime;
        [SerializeField] protected float _endTime;
        [SerializeField] protected float _duration;

        public float startTime => _startTime;
        public float endTime => _endTime;
        public float duration => _duration;
        
        public ActionTriggerDurationBase() : base() {}

        public ActionTriggerDurationBase(ActionTriggerDurationBase prvTriggerData) : base(prvTriggerData)
        {
            _startTime = prvTriggerData._startTime;
            _endTime = prvTriggerData._endTime;
            _duration = prvTriggerData._duration;
        }
        
        public abstract void OnFinalize();
        
    }
    
    #if UNITY_EDITOR
    public abstract partial class ActionTriggerDurationBase : IActionEditorItem
    {
        private static GUIStyle _guiStyle_MiddleRightLabal = null;

        public int Editor_StartFrame
        {
            get { return Mathf.RoundToInt(startTime * ActionData.FRAME_RATE); }
            set
            {
                _startTime = value / ActionData.FRAME_RATE;
                if (_endTime < _startTime)
                    _endTime = _startTime;

                _duration = _endTime - _startTime;
            }
        }

        public int Editor_EndFrame
        {
            get { return Mathf.RoundToInt(endTime * ActionData.FRAME_RATE); }
            set
            {
                _endTime = value / ActionData.FRAME_RATE;
                if (_startTime > _endTime)
                    _startTime = _endTime;

                _duration = _endTime - _startTime;
            }
        }

        public string Editor_DisplayTypeName => ActionData.Editor_GetTriggerDisplayTypeName(GetType());

        public string Editor_TimelineItemName => Editor_DisplayTypeName;

        public abstract string Editor_TimelineItemDesc { get; }
        
        public void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutEx.TextField(actionData, ref nextActionName);
            }
        }

        public bool Editor_OnGUI_InspectorHeader(ActionData actionData)
        {
            _guiStyle_MiddleRightLabal ??= new(EditorStyles.label) { alignment = TextAnchor.MiddleRight };

            bool bChanged = false;
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayoutEx.LabelWidthScope(40f))
                    {
                        var newStartFrame = EditorGUILayout.IntSlider("Start", Editor_StartFrame, 0, actionData.Frame);
                        if (newStartFrame != Editor_StartFrame)
                        {
                            Undo.RecordObject(actionData, "ActionData :: Set Event's StartTime");
                            Editor_StartFrame = newStartFrame;
                            EditorUtility.SetDirty(actionData);
                            bChanged = true;
                        }

                        if (HasEndTime)
                        {
                            var newEndFrame = EditorGUILayout.IntSlider("End", Editor_EndFrame, 0, actionData.Frame);
                            if (newEndFrame < Editor_StartFrame)
                                Editor_StartFrame = newEndFrame;

                            if (newEndFrame != Editor_EndFrame)
                            {
                                Undo.RecordObject(actionData, "ActionData :: Set Event's StartTime");
                                Editor_EndFrame = newEndFrame;
                                EditorUtility.SetDirty(actionData);
                                bChanged = true;
                            }
                        }
                    }
                }

                var timeWidth = GUILayout.Width(90f);
                using (new EditorGUILayout.VerticalScope(timeWidth))
                {
                    EditorGUILayout.LabelField($"{startTime:0.##} sec", _guiStyle_MiddleRightLabal, timeWidth);
                    if (HasEndTime)
                        EditorGUILayout.LabelField($"{endTime:0.##} sec", _guiStyle_MiddleRightLabal, timeWidth);
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    Editor_OnGUI_InspectorContent(actionData);
                    bChanged |= scope.changed;
                }
            }

            return bChanged;
        }

        public abstract IActionEditorItem Copy();

#if UNITY_EDITOR
        protected abstract void Editor_OnGUI_InspectorContent(ActionData actionData);
#endif
    }
    #endif
}