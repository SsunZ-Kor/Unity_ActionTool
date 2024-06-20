using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_RotateDir_ToBrainTarget : ActionEventData_RotateDirBase
    {
        public enum DstTypes
        {
            Target_ActionStart_Dir_ActionStart,
            Target_ActionStart_Dir_EventStart,
            Target_ActionStart_Dir_Current,
            Target_EventStart_Dir_EventStart,
            Target_EventStart_Dir_Current,
        }

        public DstTypes dstType = DstTypes.Target_ActionStart_Dir_ActionStart;

        public ActionEventData_RotateDir_ToBrainTarget() : base() {}

        public ActionEventData_RotateDir_ToBrainTarget(ActionEventData_RotateDir_ToBrainTarget prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_RotateDir_ToBrainTarget(this);
            return result;
        }
    }
    
    #if UNITY_EDITOR
    public partial class ActionEventData_RotateDir_ToBrainTarget
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "Target_ActionStart_Dir_ActionStart :: Action 시작 시점의 타겟 / Action 시작 시점의 방향\n" +
                "Target_ActionStart_Dir_EventStart :: Action 시작 시점의 타겟 / Event 시작 시점의 방향\n" +
                "Target_ActionStart_Dir_Current :: Action 시작 시점의 타겟 / 현재 방향\n" +
                "Target_EventStart_Dir_EventStart :: Event 시작 시점의 타겟 / Event 시작 시점의 방향\n" +
                "Target_EventStart_Dir_Current :: Event 시작 시점의 타겟 / 현재 방향");

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            base.Editor_OnGUI_InspectorContent(actionData);
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: To Brain Target", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.EnumPopup(actionData, guiContent_DstType, ref dstType);
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_RotateDir_ToBrainTarget(this);
    }
#endif

    public class ActionEventRuntime_RotateDir_ToBrainTarget
        : ActionEventRuntime_RotateDirBase<ActionEventData_RotateDir_ToBrainTarget>
    {
        private ActorTarget _target;
        private System.Func<ActorTarget> _getTarget = null;
        private System.Func<Vector3> _getTargetPos = null;
        private System.Func<Vector3> _getInputDir = null;

        public ActionEventRuntime_RotateDir_ToBrainTarget(ActionEventData_RotateDir_ToBrainTarget eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_RotateDir_ToBrainTarget.DstTypes.Target_ActionStart_Dir_ActionStart:
                    {
                        bRefreshDst = false;
                        _getTarget = () => master.ActionCtrl.StartInfo.Target;
                        _getTargetPos = () => master.ActionCtrl.StartInfo.TargetPos;
                        _getInputDir = () => master.ActionCtrl.StartInfo.LookDirOnPlane;
                    }
                    break;
                case ActionEventData_RotateDir_ToBrainTarget.DstTypes.Target_ActionStart_Dir_EventStart:
                    {
                        bRefreshDst = false;
                        _getTarget = () => master.ActionCtrl.StartInfo.Target;
                        _getTargetPos = () => master.ActionCtrl.StartInfo.Target.transform.position;
                        _getInputDir = () => master.ActionCtrl.StartInfo.LookDirOnPlane;
                    }
                    break;
                case ActionEventData_RotateDir_ToBrainTarget.DstTypes.Target_ActionStart_Dir_Current:
                    {
                        bRefreshDst = true;
                        _getTarget = () => master.ActionCtrl.StartInfo.Target;
                        _getTargetPos = () => master.ActionCtrl.StartInfo.Target.transform.position;
                        _getInputDir = () => master.ActionCtrl.StartInfo.LookDirOnPlane;
                    }
                    break;
                case ActionEventData_RotateDir_ToBrainTarget.DstTypes.Target_EventStart_Dir_EventStart:
                    {
                        bRefreshDst = false;
                        _getTarget = () => master.BrainCtrl.Target;
                        _getTargetPos = () => master.BrainCtrl.Target.transform.position;
                        _getInputDir = () => master.BrainCtrl.LookDirOnPlane;
                    }
                    break;
                case ActionEventData_RotateDir_ToBrainTarget.DstTypes.Target_EventStart_Dir_Current:
                    {
                        bRefreshDst = true;
                        _getTarget = () => master.BrainCtrl.Target;
                        _getTargetPos = () => master.BrainCtrl.Target.transform.position;
                        _getInputDir = () => master.BrainCtrl.LookDirOnPlane;
                    }
                    break;
            }

            yield return base.OnAction_Init(master, endCallback);
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            _target = _getTarget();

            base.OnAction_Start(master, fElapsedTime);
        }

        protected override Vector3 _GetDir(Actor master)
        {
            /* BrainTarget이 없다면 Input으로 대체 */
            if (_target == null)
                return _getInputDir();

            /* 동일한 Pos라면 Input으로 대체 */
            var result = _getTargetPos() - master.transform.position;
            if (result.sqrMagnitude <= float.Epsilon)
                return _getInputDir();

            result = Vector3.ProjectOnPlane(result, Vector3.up);
            return result.normalized;
        }
    }
}