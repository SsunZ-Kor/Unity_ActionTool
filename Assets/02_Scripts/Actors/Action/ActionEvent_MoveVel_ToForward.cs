using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_MoveVel_ToForward : ActionEventData_MoveVelBase
    {
        public enum DstTypes
        {
            Forward_ActionStart,
            Forward_EventStart,
        }

        public DstTypes dstType = DstTypes.Forward_ActionStart;

        public ActionEventData_MoveVel_ToForward() : base() {}

        public ActionEventData_MoveVel_ToForward(ActionEventData_MoveVel_ToForward prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_MoveVel_ToForward(this);
            return result;
        }
    }
    
#if UNITY_EDITOR
    public partial class ActionEventData_MoveVel_ToForward
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "Input_ActionStart :: Action 시작 시점의 캐릭터 방향\n" +
                "Input_EventStart :: Event 시작 의 캐릭터 방향");

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            base.Editor_OnGUI_InspectorContent(actionData);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: To Forward", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.EnumPopup(actionData, guiContent_DstType, ref dstType);
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_MoveVel_ToForward(this);
    }
#endif

    public class ActionEventRuntime_MoveVel_ToForward : ActionEventRuntime_MoveVelBase<ActionEventData_MoveVel_ToForward>
    {
        private System.Func<Vector3> _getForward;

        public ActionEventRuntime_MoveVel_ToForward(ActionEventData_MoveVel_ToForward eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_MoveVel_ToForward.DstTypes.Forward_ActionStart:
                    _getForward = _eventData.bGroundMoving
                        ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                        : () => master.ActionCtrl.StartInfo.LookDir;
                    break;
                case ActionEventData_MoveVel_ToForward.DstTypes.Forward_EventStart:
                    _getForward = _eventData.bGroundMoving
                        ? () => Vector3.ProjectOnPlane(master.transform.forward, Vector3.up)
                        : () => master.transform.forward;
                    break;
            }

            yield return base.OnAction_Init(master, endCallback);
        }

        protected override Quaternion _GetDir(Actor master)
        {
            return Quaternion.LookRotation(Vector3.ProjectOnPlane(_getForward(), Vector3.up));
        }
    }
}