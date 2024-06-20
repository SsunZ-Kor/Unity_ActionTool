using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_RotateAngle_Left : ActionEventData_RotateAngleBase
    {
        public ActionEventData_RotateAngle_Left() : base() {}

        public ActionEventData_RotateAngle_Left(ActionEventData_RotateAngle_Left prvEventData) : base(prvEventData)
        {
            
        }
        
        public override IActionEventRuntime CreateRuntime(Actor master)
        {
            var result = new ActionEventRuntime_RotateAngle_Left(this);
            return result;
        }

        public override float GetAngle(float fEventElapsedTime)
        {
            return -base.GetAngle(fEventElapsedTime);
        }
    }
    
#if UNITY_EDITOR
    public partial class ActionEventData_RotateAngle_Left
    {
        public override IActionEditorItem Copy() => new ActionEventData_RotateAngle_Left(this);
    }
#endif

    public class ActionEventRuntime_RotateAngle_Left : ActionEventRuntime_RotateAngleBase<ActionEventData_RotateAngle_Left>
    {
        public ActionEventRuntime_RotateAngle_Left(ActionEventData_RotateAngle_Left eventData) : base(eventData)
        {
        }
    }
}