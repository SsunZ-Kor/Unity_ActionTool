using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{

    public partial class ActionController
    {
        /// <summary>
        /// 액션 시작 시의 정보들
        /// </summary>
        public class ActionStartInfos
        {
            /* Master Info*/
            public Vector3 MasterForward { get; private set; }

            /* Brain Target */
            public ActorTarget Target { get; private set; }
            public Vector3 TargetPos { get; private set; }
            public Quaternion TargetRot { get; private set; }

            /*  */
            public Vector3 LookDirOnPlane { get; private set; }
            public Vector3 LookDir { get; private set; }
            public float LookPower { get; private set; }

            public void UpdateInfos(Actor master)
            {
                if (master == null)
                    return;

                MasterForward = master.transform.forward;
                
                var brainCtrl = master.BrainCtrl;

                Target = brainCtrl.Target;
                if (Target != null)
                {
                    var trTarget = Target.transform;
                    TargetPos = trTarget.position;
                    TargetRot = trTarget.rotation;
                }

                LookDir = brainCtrl.LookDir;
                LookDirOnPlane = brainCtrl.LookDirOnPlane;
                LookPower = brainCtrl.LookPower;
            }
        }

        public ActionStartInfos StartInfo { get; private set; } = new ();

        /* Action Target */
        public List<ActorTarget> ActionTargets { get; private set; } = new();

        public void OnPlayAction_Targeting()
        {
            StartInfo.UpdateInfos(Master);
        }
    }
}