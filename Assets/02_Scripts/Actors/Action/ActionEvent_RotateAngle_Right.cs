using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_RotateAngle_Right : ActionEventData_RotateAngleBase
    {
        public ActionEventData_RotateAngle_Right() : base() {}

        public ActionEventData_RotateAngle_Right(ActionEventData_RotateAngle_Right prvEventData) : base(prvEventData)
        {
            
        }
        
        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_RotateAngle_Right(this);
            return result;
        }
    }
    
#if UNITY_EDITOR
    public partial class ActionEventData_RotateAngle_Right
    {
        public override IActionEditorItem Copy() => new ActionEventData_RotateAngle_Right(this);
    }
#endif

    public class ActionEventRuntime_RotateAngle_Right : ActionEventRuntime_RotateAngleBase<ActionEventData_RotateAngle_Right>
    {
        public ActionEventRuntime_RotateAngle_Right(ActionEventData_RotateAngle_Right eventData) : base(eventData)
        {
        }
    }
}