using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace  Actor
{
    public abstract partial class ActionEventData_MoveVelBase : ActionEventDataBase
    {
        public override bool HasEndTime => false;

        public Vector3 vVelocity;
        public bool bGroundMoving = true;
        public float fDragOnGround = -1;
        public bool bResetPrvVelocity = true;

        protected ActionEventData_MoveVelBase() : base() {}

        protected ActionEventData_MoveVelBase(ActionEventData_MoveVelBase prvEventData) : base(prvEventData)
        {
            vVelocity = prvEventData.vVelocity;
            bGroundMoving = prvEventData.bGroundMoving;
            fDragOnGround = prvEventData.fDragOnGround;
            bResetPrvVelocity = prvEventData.bResetPrvVelocity;
        }
    }
    
    #if UNITY_EDITOR
    public abstract partial class ActionEventData_MoveVelBase
    {
        public override string Editor_TimelineItemDesc => $"{Editor_DisplayTypeName} :: {vVelocity.x}, {vVelocity.y}, {vVelocity.z}";
        public override string Editor_TimelineItemName => Editor_DisplayTypeName.Replace("_", "\n");

        public static readonly GUIContent guiContent_GroundMoving 
            = new("Is Ground Moving", "활성화 시, 방향의 높이를 무시합니다.");
        
        public static readonly GUIContent guiContent_ResetPrvVelocity
            = new("Reset Prev Velocity", "기존 Vel을 초기화 한 후, 적용합니다. 체크하지 않았다면, 기존 Vel에 더해집니다.");
        public static readonly GUIContent guiContent_DragOnGround
            = new("Drag On Ground", "지면에 있을 경우, 마찰력 입니다.");
        
        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            EditorGUILayoutEx.Vector3Field(actionData, (GUIContent)null, ref vVelocity);
        }

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayout.LabelField("Velocity", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.Vector3Field(actionData, (GUIContent)null, ref vVelocity);
            --EditorGUI.indentLevel;
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: Common", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.Toggle(actionData, guiContent_GroundMoving, ref bGroundMoving);
            EditorGUILayoutEx.Toggle(actionData, guiContent_ResetPrvVelocity, ref bResetPrvVelocity);
            EditorGUILayoutEx.FloatField(actionData, guiContent_DragOnGround, ref fDragOnGround);
            --EditorGUI.indentLevel;
        }
    }
    #endif

    public abstract class ActionEventRuntime_MoveVelBase<TMoveVelDataBase> 
        : ActionEventRuntimeBase<TMoveVelDataBase> where TMoveVelDataBase : ActionEventData_MoveVelBase
    {
        protected ActionEventRuntime_MoveVelBase(TMoveVelDataBase eventData) : base(eventData)
        {
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            base.OnAction_Start(master, fElapsedTime);

            if (_eventData.bResetPrvVelocity)
                master.MoveCtrl.SetVelocity(_GetDir(master) * _eventData.vVelocity, _eventData.fDragOnGround );
            else
                master.MoveCtrl.AddVelocity(_GetDir(master) * _eventData.vVelocity, _eventData.fDragOnGround );
        }
        
        protected abstract Quaternion _GetDir(Actor master);
    }
}