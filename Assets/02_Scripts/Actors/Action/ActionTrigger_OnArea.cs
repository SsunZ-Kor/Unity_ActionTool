using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    [System.Serializable]   
    public partial class ActionTrigger_OnArea : ActionTriggerDurationBase
    {
        public LinkedListNode<ActionTrigger_OnArea> Node { get; private set; }

        public TriggerAreaTypes areaType;

        public ActionTrigger_OnArea()
        {
            Node = new(this);
        }
        
        public ActionTrigger_OnArea(ActionTrigger_OnArea prvTriggerData) : base(prvTriggerData)
        {
            Node = new(this);

            areaType = prvTriggerData.areaType;
        }
        
        public override void OnFinalize()
        {
            Node.Value = null;
            Node = null;
        }
    }
    
#if UNITY_EDITOR
    public partial class ActionTrigger_OnArea
    {
        public override string Editor_TimelineItemDesc =>
            $"{Editor_DisplayTypeName} :: {areaType}";

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayoutEx.TextField(actionData, _guiContent_NextAction, ref nextActionName);
            EditorGUILayoutEx.EnumFlagsField(actionData, "Area", ref areaType);
        }

        public override IActionEditorItem Copy() => new ActionTrigger_OnArea(this);
    }
#endif
}