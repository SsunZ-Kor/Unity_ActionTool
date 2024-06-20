using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Serializable]   
    public class ActionTrigger_OnEndAction : ActionTriggerBase
    {
        public TriggerAreaTypes areaType = (TriggerAreaTypes)int.MaxValue;
        public TriggerJoysickTypes JoysickType = (TriggerJoysickTypes)int.MaxValue;
        public GameKeyCode keyCode = GameKeyCode.None;
        
#if UNITY_EDITOR
        public string Editor_DisplayDesc => $"{ActionData.Editor_GetTriggerDisplayTypeName(GetType())}";

        public bool Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayoutEx.TextField(actionData, _guiContent_NextAction, ref nextActionName);
            var result = false;
            result |= EditorGUILayoutEx.EnumFlagsField(actionData, "Area", ref areaType);  
            result |= EditorGUILayoutEx.EnumFlagsField(actionData, "Joystick", ref JoysickType);
            result |= EditorGUILayoutEx.EnumPopup(actionData, "KeyCode", ref keyCode);

            return result;
        }
#endif
    }
}