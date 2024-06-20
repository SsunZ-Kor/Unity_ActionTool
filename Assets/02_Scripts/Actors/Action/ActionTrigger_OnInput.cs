using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Serializable]   
    public partial class ActionTrigger_OnInput : ActionTriggerDurationBase
    {
        public LinkedListNode<ActionTrigger_OnInput> Node { get; private set; }

        public TriggerJoysickTypes JoysickType;
        public GameKeyCode keyCode;
        public GameKeyState keyState;

        public ActionTrigger_OnInput()
        {
            Node = new(this);
        }
        
        public ActionTrigger_OnInput(ActionTrigger_OnInput prvTriggerData) : base(prvTriggerData)
        {
            Node = new(this);

            JoysickType = prvTriggerData.JoysickType;
            keyCode = prvTriggerData.keyCode;
            keyState = prvTriggerData.keyState;
        }

        public override void OnFinalize()
        {
            Node.Value = null;
            Node = null;
        }
    }
    
    #if UNITY_EDITOR
    public partial class ActionTrigger_OnInput
    {
        public override string Editor_TimelineItemDesc =>
            $"{Editor_DisplayTypeName} :: {JoysickType} - {keyCode} - {keyState}";
        
        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayoutEx.TextField(actionData, _guiContent_NextAction, ref nextActionName);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutEx.EnumFlagsField(actionData, "Joystick", ref JoysickType);
                EditorGUILayoutEx.EnumPopup(actionData, "KeyCode", ref keyCode);
                EditorGUILayoutEx.EnumPopup(actionData, "KeyState", ref keyState);
            }
        }

        public override IActionEditorItem Copy() => new ActionTrigger_OnInput(this);
    }
    #endif
}