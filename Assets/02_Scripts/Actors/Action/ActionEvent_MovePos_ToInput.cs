using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_MovePos_ToInput : ActionEventData_MovePosBase
    {
        public enum DstTypes
        {
            Input_ActionStart,
            Input_EventStart,
            Input_Current,
        }

        public DstTypes dstType = DstTypes.Input_ActionStart;
        public bool bUseInputPower = false;

        
        public ActionEventData_MovePos_ToInput() : base() {}

        public ActionEventData_MovePos_ToInput(ActionEventData_MovePos_ToInput prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
            bUseInputPower = prvEventData.bUseInputPower;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_MovePos_ToInput(this);
            return result;
        }
    }

#if UNITY_EDITOR
    public partial class ActionEventData_MovePos_ToInput
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "Input_ActionStart :: Action 시작 시점의 조이스틱 방향\n" +
                "Input_EventStart :: Event 시작 의 조이스틱 방향\n" +
                "Input_Current :: 현재 조이스틱 방향");
        public static readonly GUIContent guiContent_UseInputPower =
            new("Use Input Power", "조이스틱의 당김 정도를 이동 속도에 반영합니다.");

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            base.Editor_OnGUI_InspectorContent(actionData);
            
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: To Input", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.EnumPopup(actionData, guiContent_DstType, ref dstType);
            EditorGUILayoutEx.Toggle(actionData, guiContent_UseInputPower, ref bUseInputPower);
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_MovePos_ToInput(this);
    }
#endif

    public class ActionEventRuntime_MovePos_ToInput : ActionEventRuntime_MovePosBase<ActionEventData_MovePos_ToInput>
    {
        private Func<Vector3> _getInputDir;
        private Func<float> _getInputPower;

        public ActionEventRuntime_MovePos_ToInput(ActionEventData_MovePos_ToInput eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_MovePos_ToInput.DstTypes.Input_ActionStart:
                    {
                        bRefreshDst = false;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                            : () => master.ActionCtrl.StartInfo.LookDir;
                        _getInputPower = _eventData.bUseInputPower
                            ? () => master.ActionCtrl.StartInfo.LookPower
                            : () => 1f;
                    }
                    break;
                case ActionEventData_MovePos_ToInput.DstTypes.Input_EventStart:
                    {
                        bRefreshDst = false;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.BrainCtrl.LookDirOnPlane
                            : () => master.BrainCtrl.LookDir;
                        _getInputPower = _eventData.bUseInputPower
                            ? () => master.BrainCtrl.LookPower
                            : () => 1f;

                    }
                    break;
                case ActionEventData_MovePos_ToInput.DstTypes.Input_Current:
                    {
                        bRefreshDst = true;
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.BrainCtrl.LookDirOnPlane
                            : () => master.BrainCtrl.LookDir;
                        _getInputPower = _eventData.bUseInputPower
                            ? () => master.BrainCtrl.LookPower
                            : () => 1f;
                    }
                    break;
            }

            yield return base.OnAction_Init(master, endCallback);
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            base.OnAction_Start(master, fElapsedTime);
        }

        public override bool OnAction_Update(Actor master, float fEventElapsedTime, float fDeltaTime)
        {
            var bUpdateBreak = base.OnAction_Update(master, fEventElapsedTime, fDeltaTime);
            if (bUpdateBreak)
                return true;

            /* vMoveDelta 계산 */ 
            var vMoveDelta = _Cal_MoveDelta(master, fEventElapsedTime, fDeltaTime) * _getInputPower();

            /* vMoveDelta 적용 */ 
            master.MoveCtrl.AddMoveDelta(vMoveDelta, _eventData.bGroundMoving);
            return false;
        }

        protected override Vector3 _GetDir(Actor master)
        {
            return _getInputDir();
        }
    }
}