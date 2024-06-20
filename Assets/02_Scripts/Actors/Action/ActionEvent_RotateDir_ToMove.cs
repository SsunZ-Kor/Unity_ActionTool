using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public partial class ActionEventData_RotateDir_ToMove : ActionEventData_RotateDirBase
    {
        public enum DstTypes
        {
            MoveDir_EventStart,
            MoveDir_Current,
        }

        public DstTypes dstType = DstTypes.MoveDir_EventStart;

        public ActionEventData_RotateDir_ToMove() : base() { }

        public ActionEventData_RotateDir_ToMove(ActionEventData_RotateDir_ToMove prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_RotateDir_ToMove(this);
            return result;
        }
    }

#if UNITY_EDITOR
    public partial class ActionEventData_RotateDir_ToMove
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "MoveDir_EventStart :: 이벤트 시작 시점의 이동 방향\n" +
                "MoveDir_Current :: 현재 이동 방향");

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            base.Editor_OnGUI_InspectorContent(actionData);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: To Brain Target", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.EnumPopup(actionData, guiContent_DstType, ref dstType);
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_RotateDir_ToMove(this);
    }
#endif


    public class ActionEventRuntime_RotateDir_ToMove
        : ActionEventRuntime_RotateDirBase<ActionEventData_RotateDir_ToMove>
    {
        public ActionEventRuntime_RotateDir_ToMove(ActionEventData_RotateDir_ToMove eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_RotateDir_ToMove.DstTypes.MoveDir_EventStart:
                    bRefreshDst = false;
                    break;
                case ActionEventData_RotateDir_ToMove.DstTypes.MoveDir_Current:
                    bRefreshDst = true;
                    break;
            }

            return base.OnAction_Init(master, endCallback);
        }

        protected override Vector3 _GetDir(Actor master)
        {
            if (master.MoveCtrl.vTotalMoveDelta.sqrMagnitude <= float.Epsilon)
                return master.transform.forward;

            return master.MoveCtrl.vTotalMoveDelta.normalized;
        }
    }
}