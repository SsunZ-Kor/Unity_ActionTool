using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_MoveVel_ToBrainTarget : ActionEventData_MoveVelBase
    {
        public enum DstTypes
        {
            Target_ActionStart_Dir_ActionStart,
            Target_ActionStart_Dir_EventStart,
            Target_EventStart_Dir_EventStart,
        }

        public DstTypes dstType = DstTypes.Target_ActionStart_Dir_ActionStart;

        public ActionEventData_MoveVel_ToBrainTarget() : base() {}

        public ActionEventData_MoveVel_ToBrainTarget(ActionEventData_MoveVel_ToBrainTarget prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_MoveVel_ToBrainTarget(this);
            return result;
        }
    }
    
#if UNITY_EDITOR
    public partial class ActionEventData_MoveVel_ToBrainTarget
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "Target_ActionStart_Dir_ActionStart :: Action 시작 시점의 타겟 / Action 시작 시점의 방향\n" +
                "Target_ActionStart_Dir_EventStart :: Action 시작 시점의 타겟 / Event 시작 시점의 방향\n" +
                "Target_EventStart_Dir_EventStart :: Event 시작 시점의 타겟 / Event 시작 시점의 방향");

        public override IActionEditorItem Copy() => new ActionEventData_MoveVel_ToBrainTarget(this);
    }
#endif

    public class ActionEventRuntime_MoveVel_ToBrainTarget : ActionEventRuntime_MoveVelBase<ActionEventData_MoveVel_ToBrainTarget>
    {
        private System.Func<ActorTarget> _getTarget = null;
        private System.Func<Vector3> _getTargetPos = null;
        private System.Func<Vector3> _getInputDir = null;

        public ActionEventRuntime_MoveVel_ToBrainTarget(ActionEventData_MoveVel_ToBrainTarget eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_MoveVel_ToBrainTarget.DstTypes.Target_ActionStart_Dir_ActionStart:
                    {
                        _getTarget = () => master.ActionCtrl.StartInfo.Target;
                        _getTargetPos = () => master.ActionCtrl.StartInfo.TargetPos;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                            : () => master.ActionCtrl.StartInfo.LookDir;
                    }
                    break;
                case ActionEventData_MoveVel_ToBrainTarget.DstTypes.Target_ActionStart_Dir_EventStart:
                    {
                        _getTarget = () => master.ActionCtrl.StartInfo.Target;
                        _getTargetPos = () => master.ActionCtrl.StartInfo.Target.transform.position;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                            : () => master.ActionCtrl.StartInfo.LookDir;
                    }
                    break;
                case ActionEventData_MoveVel_ToBrainTarget.DstTypes.Target_EventStart_Dir_EventStart:
                    {
                        _getTarget = () => master.BrainCtrl.Target;
                        _getTargetPos = () => master.BrainCtrl.Target.transform.position;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.BrainCtrl.LookDirOnPlane
                            : () => master.BrainCtrl.LookDir;
                    }
                    break;
            }

            yield return base.OnAction_Init(master, endCallback);
        }

        protected override Quaternion _GetDir(Actor master)
        {
            var target = _getTarget();
            
            /* BrainTarget이 없다면 Input으로 대체 */
            if (target == null)
                return Quaternion.LookRotation(_getInputDir(), Vector3.up);

            /* 동일한 Pos라면 Input으로 대체 */
            var result = _getTargetPos() - master.transform.position;
            if (result.sqrMagnitude <= float.Epsilon)
                return Quaternion.LookRotation(_getInputDir(), Vector3.up);

            /* 지상 이동일 시, 바닥으로 투영 */
            if (_eventData.bGroundMoving)
                result = Vector3.ProjectOnPlane(result, Vector3.up);

            return Quaternion.LookRotation(result, Vector3.up);
        }
    }
}