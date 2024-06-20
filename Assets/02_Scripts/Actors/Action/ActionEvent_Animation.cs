using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_Animation : ActionEventDataBase
    {
        public override bool HasEndTime => false;

        public AnimLayerTypes layerType = AnimLayerTypes.Base;
        public string animStateName;
        public float speed = 1f;
        public float fadeNormalizedTime = 0.1f;
        public bool rootMotion = false;

        public ActionEventData_Animation() : base() {}

        public ActionEventData_Animation(ActionEventData_Animation prvEventData) : base(prvEventData)
        {
            layerType = prvEventData.layerType;
            animStateName = string.Copy(prvEventData.animStateName);
            speed = prvEventData.speed;
            fadeNormalizedTime = prvEventData.fadeNormalizedTime;
            rootMotion = prvEventData.rootMotion;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
            => new ActionEventRuntime_Animation(this);
    }

#if UNITY_EDITOR
    public partial class ActionEventData_Animation
    {
        public override string Editor_TimelineItemDesc =>
            $"{Editor_DisplayTypeName} :: {layerType} - {(string.IsNullOrEmpty(animStateName) ? "None" : animStateName)}";

        public override string Editor_TimelineItemName => Editor_DisplayTypeName;

        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutEx.EnumPopup(actionData, ref layerType, GUILayout.Width(90f));
                EditorGUILayoutEx.TextField(actionData, ref animStateName);
            }
        }

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayoutEx.EnumPopup(actionData, "Layer", ref layerType);
            EditorGUILayoutEx.TextField(actionData, "AnimStateName", ref animStateName);
            EditorGUILayoutEx.FloatField(actionData, "Speed", ref speed);
            EditorGUILayoutEx.FloatField(actionData, "FadeNormalizedTime", ref fadeNormalizedTime);
            EditorGUILayoutEx.Toggle(actionData, "RootMotion", ref rootMotion);
        }

        public override IActionEditorItem Copy() => new ActionEventData_Animation(this);
    }
#endif

    public class ActionEventRuntime_Animation : ActionEventRuntimeBase<ActionEventData_Animation>
    {
        private int _animShortNameHash;

        public ActionEventRuntime_Animation(ActionEventData_Animation eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            _animShortNameHash = string.IsNullOrWhiteSpace(_eventData.animStateName)
                ? 0
                : Animator.StringToHash(_eventData.animStateName);
            
            yield return base.OnAction_Init(master, endCallback);
        }
        
        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            base.OnAction_Start(master, fElapsedTime);

            if (_animShortNameHash != 0)
            {
                master.AnimCtrl.Play(_animShortNameHash, _eventData.fadeNormalizedTime, _eventData.layerType);
                master.AnimCtrl.Anim.speed = _eventData.speed;
            }
            else
            {
                master.AnimCtrl.Anim.speed = 1f;
            }

            master.AnimCtrl.Anim.applyRootMotion = _eventData.rootMotion;
        }
    }
}