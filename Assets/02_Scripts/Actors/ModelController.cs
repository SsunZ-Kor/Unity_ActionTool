using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public enum ModelRigTypes
    {
        None,
        Head,
        Chest,
        Spine,
        Shoulder_L,
        Shoulder_R,
        Arm_L,
        Arm_R,
        Hand_L,
        Hand_R,
        Leg_L,
        Leg_R,
        Foot_L,
        Foot_R,
        Special_01,
        Special_02,
        Special_03,
        Special_04,
        Special_05,
        Special_06,
    }
    
    
    public class ModelController : MonoBehaviour
    {
        public Actor Master { get; private set; }

        [SerializeField] 
        private Animator _anim;

        [SerializeField]
        private ActorTarget[] _actorTarget;
        
        [SerializeField] private Transform trHead;
        [SerializeField] private Transform trChest;
        [SerializeField] private Transform trSpine;
        [SerializeField] private Transform trShoulder_L;
        [SerializeField] private Transform trShoulder_R;
        [SerializeField] private Transform trArm_L;
        [SerializeField] private Transform trArm_R;
        [SerializeField] private Transform trHand_L;
        [SerializeField] private Transform trHand_R;
        [SerializeField] private Transform trLeg_L;
        [SerializeField] private Transform trLeg_R;
        [SerializeField] private Transform trFoot_L;
        [SerializeField] private Transform trFoot_R;
        [SerializeField] private Transform trSpecial_01;
        [SerializeField] private Transform trSpecial_02;
        [SerializeField] private Transform trSpecial_03;
        [SerializeField] private Transform trSpecial_04;
        [SerializeField] private Transform trSpecial_05;
        [SerializeField] private Transform trSpecial_06;

        
        public Animator Anim => _anim;
        

        public void Init(Actor master)
        {
            Master = master;
        }

        public Transform GetRig(ModelRigTypes rigType)
        {
            var result = rigType switch
            {
                ModelRigTypes.Head       => trHead,
                ModelRigTypes.Chest      => trChest,
                ModelRigTypes.Spine      => trSpine,
                ModelRigTypes.Shoulder_L => trShoulder_L,
                ModelRigTypes.Shoulder_R => trShoulder_R,
                ModelRigTypes.Arm_L      => trArm_L,
                ModelRigTypes.Arm_R      => trArm_R,
                ModelRigTypes.Hand_L     => trHand_L,
                ModelRigTypes.Hand_R     => trHand_R,
                ModelRigTypes.Leg_L      => trLeg_L,
                ModelRigTypes.Leg_R      => trLeg_R,
                ModelRigTypes.Foot_L     => trFoot_L,
                ModelRigTypes.Foot_R     => trFoot_R,
                ModelRigTypes.Special_01 => trSpecial_01,
                ModelRigTypes.Special_02 => trSpecial_02,
                ModelRigTypes.Special_03 => trSpecial_03,
                ModelRigTypes.Special_04 => trSpecial_04,
                ModelRigTypes.Special_05 => trSpecial_05,
                ModelRigTypes.Special_06 => trSpecial_06,
                _                        => this.transform
            };

            if (result == null)
                result = this.transform;

            return result;
        }
    }
}