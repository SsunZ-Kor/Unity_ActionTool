using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_RotateDir_ToInput : ActionEventData_RotateDirBase
    {
        public enum DstTypes
        {
            Input_ActionStart,
            Input_EventStart,
            Input_Current,
        }

        public DstTypes dstType = DstTypes.Input_ActionStart;

        public ActionEventData_RotateDir_ToInput() : base() {}

        public ActionEventData_RotateDir_ToInput(ActionEventData_RotateDir_ToInput prvEventData) : base(prvEventData)
        {
            dstType = prvEventData.dstType;
        }

        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_RotateDir_ToInput(this);
            return result;
        }
    }
    
#if UNITY_EDITOR
    public partial class ActionEventData_RotateDir_ToInput
    {
        public static readonly GUIContent guiContent_DstType
            = new("DirType",
                "Input_ActionStart :: Action 시작 시점의 조이스틱 방향\n" +
                "Input_EventStart :: Event 시작 의 조이스틱 방향\n" +
                "Input_Current :: 현재 조이스틱 방향");

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            base.Editor_OnGUI_InspectorContent(actionData);

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: To Input", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.EnumPopup(actionData, guiContent_DstType, ref dstType);
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_RotateDir_ToInput(this);
    }
#endif

    public class ActionEventRuntime_RotateDir_ToInput : ActionEventRuntime_RotateDirBase<ActionEventData_RotateDir_ToInput>
    {
        private Func<Vector3> _getInputDir;

        public ActionEventRuntime_RotateDir_ToInput(ActionEventData_RotateDir_ToInput eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            switch (_eventData.dstType)
            {
                case ActionEventData_RotateDir_ToInput.DstTypes.Input_ActionStart:
                    {
                        bRefreshDst = false;
                        _getInputDir = () => master.ActionCtrl.StartInfo.LookDirOnPlane;
                    }
                    break;
                case ActionEventData_RotateDir_ToInput.DstTypes.Input_EventStart:
                    {
                        bRefreshDst = false;
                        _getInputDir = () => master.BrainCtrl.LookDirOnPlane;

                    }
                    break;
                case ActionEventData_RotateDir_ToInput.DstTypes.Input_Current:
                    {
                        bRefreshDst = true;
                        _getInputDir = () => master.BrainCtrl.LookDirOnPlane;
                    }
                    break;
            }

            yield return base.OnAction_Init(master, endCallback);
        }

        protected override Vector3 _GetDir(Actor master)
        {
            return _getInputDir();
        }
    }
}