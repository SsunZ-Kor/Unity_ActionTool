using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_MovePos_ToBrainTarget : ActionEventData_MovePosBase
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
        public string closeToTargetActionName;
        public float closeToTargetDistance;

        public ActionEventData_MovePos_ToBrainTarget() : base() {}

        public ActionEventData_MovePos_ToBrainTarget(ActionEventData_MovePos_ToBrainTarget prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
            closeToTargetActionName = string.Copy(prvEventData.closeToTargetActionName);
            closeToTargetDistance = prvEventData.closeToTargetDistance;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_MovePos_ToBrainTarget(this);
            return result;
        }
    }

#if UNITY_EDITOR
    public partial class ActionEventData_MovePos_ToBrainTarget
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "Target_ActionStart_Dir_ActionStart :: Action 시작 시점의 타겟 / Action 시작 시점의 방향\n" +
                "Target_ActionStart_Dir_EventStart :: Action 시작 시점의 타겟 / Event 시작 시점의 방향\n" +
                "Target_ActionStart_Dir_Current :: Action 시작 시점의 타겟 / 현재 방향\n" +
                "Target_EventStart_Dir_EventStart :: Event 시작 시점의 타겟 / Event 시작 시점의 방향\n" +
                "Target_EventStart_Dir_Current :: Event 시작 시점의 타겟 / 현재 방향");
        public static readonly GUIContent guiContent_CloseToTargetActionName 
            = new("Next Action Name", "이벤트 실행 중, Target과 가까워 졌을 떄 실행 할 Action의 이름입니다.");
        public static readonly GUIContent guiContent_CloseToTargetDistance 
            = new("TargetDist", "이벤트 실행 중, Target과 가까워 졌다고 판단 할 거리입니다.");
        
        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            base.Editor_OnGUI_InspectorContent(actionData);
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: To Brain Target", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.EnumPopup(actionData, guiContent_DstType, ref dstType);
            --EditorGUI.indentLevel;
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: If Close To Target", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.TextField(actionData, guiContent_CloseToTargetActionName, ref closeToTargetActionName);
            EditorGUILayoutEx.FloatField(actionData, guiContent_CloseToTargetDistance, ref closeToTargetDistance);
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_MovePos_ToBrainTarget(this);
    }
#endif

    public class ActionEventRuntime_MovePos_ToBrainTarget : ActionEventRuntime_MovePosBase<ActionEventData_MovePos_ToBrainTarget>
    {
        private ActorTarget _target;
        private System.Func<ActorTarget> _getTarget = null;
        private System.Func<Vector3> _getTargetPos = null;
        private System.Func<Vector3> _getInputDir = null;

        public ActionEventRuntime_MovePos_ToBrainTarget(ActionEventData_MovePos_ToBrainTarget eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_MovePos_ToBrainTarget.DstTypes.Target_ActionStart_Dir_ActionStart:
                    {
                        bRefreshDst = false;
                        _getTarget = () => master.ActionCtrl.StartInfo.Target;
                        _getTargetPos = () => master.ActionCtrl.StartInfo.TargetPos;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                            : () => master.ActionCtrl.StartInfo.LookDir;
                    }
                    break;
                case ActionEventData_MovePos_ToBrainTarget.DstTypes.Target_ActionStart_Dir_EventStart:
                    {
                        bRefreshDst = false;
                        _getTarget = () => master.ActionCtrl.StartInfo.Target;
                        _getTargetPos = () => master.ActionCtrl.StartInfo.Target.transform.position;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                            : () => master.ActionCtrl.StartInfo.LookDir;
                    }
                    break;
                case ActionEventData_MovePos_ToBrainTarget.DstTypes.Target_ActionStart_Dir_Current:
                    {
                        bRefreshDst = true;
                        _getTarget = () => master.ActionCtrl.StartInfo.Target;
                        _getTargetPos = () => master.ActionCtrl.StartInfo.Target.transform.position;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                            : () => master.ActionCtrl.StartInfo.LookDir;
                    }
                    break;
                case ActionEventData_MovePos_ToBrainTarget.DstTypes.Target_EventStart_Dir_EventStart:
                    {
                        bRefreshDst = false;
                        _getTarget = () => master.BrainCtrl.Target;
                        _getTargetPos = () => master.BrainCtrl.Target.transform.position;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.BrainCtrl.LookDirOnPlane
                            : () => master.BrainCtrl.LookDir;
                    }
                    break;
                case ActionEventData_MovePos_ToBrainTarget.DstTypes.Target_EventStart_Dir_Current:
                    {
                        bRefreshDst = true;
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

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            _target = _getTarget();

            base.OnAction_Start(master, fElapsedTime);
        }

        public override bool OnAction_Update(Actor master, float fEventElapsedTime, float fDeltaTime)
        {
            var bUpdateBreak = base.OnAction_Update(master, fEventElapsedTime, fDeltaTime);
            if (bUpdateBreak)
                return true;

            /* vMoveDelta 계산 */
            var vMoveDelta = _Cal_MoveDelta(master, fEventElapsedTime, fDeltaTime);

            /* vMoveDelta 적용 */
            master.MoveCtrl.AddMoveDelta(vMoveDelta, _eventData.bGroundMoving);

            /* 타겟 근접 체크 */
            return _CheckCloseToTarget(master, vMoveDelta);
        }

        public override void OnAction_Finalize(Actor master)
        {
            _target = null;
            _getTargetPos = null;
            _getInputDir = null;
            base.OnAction_Finalize(master);
        }

        private bool _CheckCloseToTarget(Actor master, Vector3 vMoveDelta)
        {
            /* 타겟 근접 액션 변경 처리 */
            if (_target == null && string.IsNullOrWhiteSpace(_eventData.closeToTargetActionName))
                return false;

            var isClose = false;

            // 단일 캐스팅을 위한 레이어 변경
            var castLayer = LayerMask.NameToLayer("World_Actor_PhysicCast");
            var goTarget = _target.gameObject;
            var prvlayer = goTarget.layer;
            goTarget.layer = castLayer;
            {
                var vTargetPos = _target.transform.position;

                var charCtrl = master.MoveCtrl.charCtrl;
                var hemiHeight = charCtrl.height * 0.5f;
                var center = charCtrl.center + vMoveDelta;

                // GroundMoving일 경우, 높이 맞춤
                if (_eventData.bGroundMoving)
                    center.y = vTargetPos.y;

                var vDirToTarget = vTargetPos - center;
                var fDistToTarget = vDirToTarget.magnitude;
                vDirToTarget *= (1f / fDistToTarget);

                // 캐스팅
                isClose = Physics.CapsuleCast(
                    center + Vector3.up * hemiHeight,
                    center + Vector3.down * hemiHeight,
                    charCtrl.radius,
                    vDirToTarget,
                    out var hitInfo,
                    Mathf.Infinity,
                    1 << castLayer
                    ) && hitInfo.distance <= _eventData.closeToTargetDistance;
            }
            goTarget.layer = prvlayer;

            if (isClose)
            {
                master.ActionCtrl.PlayAction(_eventData.closeToTargetActionName);
                return true;
            }

            return false;
        }

        protected override Vector3 _GetDir(Actor master)
        {
            /* BrainTarget이 없다면 Input으로 대체 */
            if (_target == null)
                return _getInputDir();

            /* 동일한 Pos라면 Input 대체 */
            var result = _getTargetPos() - master.transform.position;
            if (result.sqrMagnitude <= float.Epsilon)
                return _getInputDir();

            /* 지상 이동일 시, 바닥으로 투영 */
            if (_eventData.bGroundMoving)
                result = Vector3.ProjectOnPlane(result, Vector3.up);

            return result.normalized;
        }
    }
}