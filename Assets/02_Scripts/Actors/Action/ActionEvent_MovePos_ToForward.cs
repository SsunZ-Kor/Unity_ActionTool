using System;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_MovePos_ToForward : ActionEventData_MovePosBase
    {
        public enum DstTypes
        {
            Forward_ActionStart,
            Forward_EventStart,
            Forward_Current,
        }
        
        public DstTypes dstType = DstTypes.Forward_ActionStart;

        public ActionEventData_MovePos_ToForward() : base() {}

        public ActionEventData_MovePos_ToForward(ActionEventData_MovePos_ToForward prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_MovePos_ToForward(this);
            return result;
        }
    }

    #if UNITY_EDITOR
    public partial class ActionEventData_MovePos_ToForward
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "Input_ActionStart :: Action 시작 시점의 캐릭터 방향\n" +
                "Input_EventStart :: Event 시작 의 캐릭터 방향\n" +
                "Input_Current :: 현재 캐릭터 방향");

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            base.Editor_OnGUI_InspectorContent(actionData);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: To Forward", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.EnumPopup(actionData, guiContent_DstType, ref dstType);
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_MovePos_ToForward(this);
    }
    #endif

    public class ActionEventRuntime_MovePos_ToForward : ActionEventRuntime_MovePosBase<ActionEventData_MovePos_ToForward>
    {
        private System.Func<Vector3> _getForward;

        public ActionEventRuntime_MovePos_ToForward(ActionEventData_MovePos_ToForward eventData) : base(eventData)
        {

        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_MovePos_ToForward.DstTypes.Forward_ActionStart:
                    bRefreshDst = false;
                    _getForward = _eventData.bGroundMoving
                        ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                        : () => master.ActionCtrl.StartInfo.LookDir;
                    break;
                case ActionEventData_MovePos_ToForward.DstTypes.Forward_EventStart:
                    bRefreshDst = false;
                    _getForward = _eventData.bGroundMoving
                        ? () => Vector3.ProjectOnPlane(master.transform.forward, Vector3.up)
                        : () => master.transform.forward;
                    break;
                case ActionEventData_MovePos_ToForward.DstTypes.Forward_Current:
                    bRefreshDst = true;
                    _getForward = _eventData.bGroundMoving
                        ? () => Vector3.ProjectOnPlane(master.transform.forward, Vector3.up)
                        : () => master.transform.forward;
                    break;
            }

            yield return base.OnAction_Init(master, endCallback);
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
            return false;
        }
        
        protected override Vector3 _GetDir(Actor master)
        {
            return _getForward();
        }
    }
}