using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public abstract partial class ActionEventData_RotateAngleBase : ActionEventDataBase
    {
        public override bool HasEndTime => true;
        
        public float fAngle;
        public AnimationCurve curve;
        
        protected ActionEventData_RotateAngleBase() : base() {}

        protected ActionEventData_RotateAngleBase(ActionEventData_RotateAngleBase prvEventData) : base(prvEventData)
        {
            fAngle = prvEventData.fAngle;
            curve = new();
            curve.CopyFrom(prvEventData.curve);
        }

        public virtual float GetAngle(float fEventElapsedTime)
        {
            var t_Curve = duration > float.Epsilon 
                ? Mathf.Clamp01(fEventElapsedTime / duration)
                : 1f;
            
            return fAngle * curve.Evaluate(t_Curve);
        }
    }
    
    #if UNITY_EDITOR
    public abstract partial class ActionEventData_RotateAngleBase
    {
        public override string Editor_TimelineItemDesc => Editor_DisplayTypeName;
        public override string Editor_TimelineItemName  => Editor_DisplayTypeName.Replace("_", "\n");
        
        public static readonly GUIContent guiContent_Angle
            = new("Angle", "회전할 각도 입니다. 단위는 Degree");
        
        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            EditorGUILayoutEx.FloatField(actionData, ref fAngle);
        }

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayout.LabelField(guiContent_Angle, EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.FloatField(actionData, ref fAngle);
            EditorGUILayoutEx.CurveField(actionData, ref curve);
            --EditorGUI.indentLevel;
        }
    }
    #endif
    
    public abstract class ActionEventRuntime_RotateAngleBase<TRotateAngleDataBase>
        : ActionEventRuntimeBase<TRotateAngleDataBase> where TRotateAngleDataBase : ActionEventData_RotateAngleBase
    {
        private float _fPrvAngle;
        public ActionEventRuntime_RotateAngleBase(TRotateAngleDataBase eventData) : base(eventData)
        {
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            base.OnAction_Start(master, fElapsedTime);

            _fPrvAngle = 0f;
        }

        public override bool OnAction_Update(Actor master, float fEventElapsedTime, float fDeltaTime)
        {
            var bUpdateBreak = base.OnAction_Update(master, fEventElapsedTime, fDeltaTime);
            if (bUpdateBreak)
                return true;

            var fCrrAngle = _eventData.GetAngle(fEventElapsedTime);
            var fRotDelta = fCrrAngle - _fPrvAngle;
            _fPrvAngle = fCrrAngle;
            
            master.transform.Rotate(Vector3.up, fRotDelta);
            return false;
        }
    }
}