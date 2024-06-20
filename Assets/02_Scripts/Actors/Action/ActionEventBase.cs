using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public abstract partial class ActionEventDataBase
    {
        public abstract bool HasEndTime { get; }

        [SerializeField] protected float _startTime;
        [SerializeField] protected float _endTime;
        [SerializeField] protected float _duration;

        public float startTime => _startTime;
        public float endTime => _endTime;
        public float duration => _duration;

        protected ActionEventDataBase(){}

        protected ActionEventDataBase(ActionEventDataBase prvEventData)
        {
            _startTime = prvEventData._startTime;
            _endTime = prvEventData._endTime;
            _duration = prvEventData._duration;
        }

        public abstract IActionEventRuntime CreateRuntime(Actor master);
    }
    
    #if UNITY_EDITOR
    public abstract partial class ActionEventDataBase : IActionEditorItem
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
        
        public string Editor_DisplayTypeName => ActionData.Editor_GetActionEventDisplayTypeName(GetType());
        public abstract string Editor_TimelineItemDesc { get; }

        public abstract string Editor_TimelineItemName { get; }

        public abstract void Editor_OnGUI_TimelineItemHeader(ActionData actionData);

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

        protected abstract void Editor_OnGUI_InspectorContent(ActionData actionData);

        public abstract IActionEditorItem Copy();
    }
    #endif

    public interface IActionEventRuntime
    {
        LinkedListNode<IActionEventRuntime> Node { get;}

        bool HasEndTime { get; }
        float StartTime { get; }
        float EndTime { get; }

        IEnumerator OnAction_Init(Actor master, System.Action endCallback);
        void OnAction_Start(Actor master, float fElapsedTime);
        /// <returns>Stop Action</returns>
        bool OnAction_Update(Actor master, float fEventElapsedTime, float fDeltaTime);
        /// <returns>Stop Action</returns>
        bool OnAction_FixedUpdate(Actor master, float fEventElapsedTime, float fDeltaTime);
        void OnAction_End(Actor master, bool bTimeEnd);
        void OnAction_Finalize(Actor master);
        void OnActorModel_Changed(Actor master, ModelController prvActorModel, ModelController crrActorModel);
    }

    public abstract class ActionEventRuntimeBase<TDataType> : IActionEventRuntime
        where TDataType : ActionEventDataBase
    {
        protected TDataType _eventData;
        
        public LinkedListNode<IActionEventRuntime> Node { get; private set; }

        public bool HasEndTime => _eventData.HasEndTime;
        public float StartTime => _eventData.startTime;
        public float EndTime => _eventData.endTime;
        public float Duration => _eventData.duration;

        public ActionEventRuntimeBase(TDataType eventData)
        {
            _eventData = eventData;
            if (_eventData.HasEndTime)
                Node = new(this);
        }

        public virtual IEnumerator OnAction_Init(Actor master, System.Action endCallback)
        {
            endCallback?.Invoke();
            yield break;
        }

        public virtual void OnAction_Start(Actor master, float fElapsedTime)
        {

        }

        public virtual bool OnAction_Update(Actor master, float fEventElapsedTime, float fDeltaTime)
        {
            return false;
        }

        public virtual bool OnAction_FixedUpdate(Actor master, float fEventElaspedTime, float fDeltaTime)
        {
            return false;
        }

        public virtual void OnAction_End(Actor master, bool bTimeEnd)
        {

        }

        public virtual void OnAction_Finalize(Actor master)
        {
            if (Node != null)
            {
                Node.Value = null;
                Node = null;
            }
        }

        public virtual void OnActorModel_Changed(Actor master, ModelController prvActorModel, ModelController crrActorModel)
        {

        }
    }
}