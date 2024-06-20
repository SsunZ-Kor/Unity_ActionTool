using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_IgnoreGravity : ActionEventDataBase
    {
        public override bool HasEndTime => true;
        
        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_IgnoreGravity(this);
            return result;
        }
    }

    #if UNITY_EDITOR
    public partial class ActionEventData_IgnoreGravity
    {
        public override string Editor_TimelineItemDesc => Editor_DisplayTypeName;
        public override string Editor_TimelineItemName => Editor_DisplayTypeName;
        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
        }

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
        }

        public override IActionEditorItem Copy() => new ActionEventData_IgnoreGravity();
    }
    #endif

    public class ActionEventRuntime_IgnoreGravity : ActionEventRuntimeBase<ActionEventData_IgnoreGravity>
    {
        public ActionEventRuntime_IgnoreGravity(ActionEventData_IgnoreGravity eventData) : base(eventData)
        {
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            base.OnAction_Start(master, fElapsedTime);
            master.MoveCtrl.SetLockGravity(true);
        }

        public override void OnAction_End(Actor master, bool bTimeEnd)
        {
            base.OnAction_End(master, bTimeEnd);
            master.MoveCtrl.SetLockGravity(false);
        }
    }
}
