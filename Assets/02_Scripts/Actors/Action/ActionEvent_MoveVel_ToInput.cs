using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_MoveVel_ToInput : ActionEventData_MoveVelBase
    {
        public enum DstTypes
        {
            Input_ActionStart,
            Input_EventStart,
        }

        public DstTypes dstType = DstTypes.Input_ActionStart;

        public ActionEventData_MoveVel_ToInput() : base() {}

        public ActionEventData_MoveVel_ToInput(ActionEventData_MoveVel_ToInput prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_MoveVel_ToInput(this);
            return result;
        }
    }
    
    #if UNITY_EDITOR
    public partial class ActionEventData_MoveVel_ToInput
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "Input_ActionStart :: Action 시작 시점의 조이스틱 방향\n" +
                "Input_EventStart :: Event 시작 의 조이스틱 방향");

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            base.Editor_OnGUI_InspectorContent(actionData);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: To Input", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.EnumPopup(actionData, guiContent_DstType, ref dstType);
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_MoveVel_ToInput(this);
    }
    #endif

    public class ActionEventRuntime_MoveVel_ToInput : ActionEventRuntime_MoveVelBase<ActionEventData_MoveVel_ToInput>
    {
        private Func<Vector3> _getInputDir;

        public ActionEventRuntime_MoveVel_ToInput(ActionEventData_MoveVel_ToInput eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_MoveVel_ToInput.DstTypes.Input_ActionStart:
                    {
                        _getInputDir = _eventData.bGroundMoving
                            ? () => master.ActionCtrl.StartInfo.LookDirOnPlane
                            : () => master.ActionCtrl.StartInfo.LookDir;
                    }
                    break;
                case ActionEventData_MoveVel_ToInput.DstTypes.Input_EventStart:
                    {
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
            return Quaternion.LookRotation(_getInputDir());
        }
    }
}