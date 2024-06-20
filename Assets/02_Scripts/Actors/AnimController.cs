using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Actor
{
    public enum AnimLayerTypes
    {
        Base = 0,
        Face = 1, 
        UpperBody = 2
    }

    public class AnimController
    {
        public Actor Master { get; private set; }

        public Animator Anim
        {
            get
            {
                if (Master.ModelCtrl == null)
                    return null;
                
                return Master.ModelCtrl.Anim;
            }
        }


        public AnimController(Actor master)
        {
            Master = master;
        }

        public void Play(int shortNameHash, float fFadeNormalizedTime, AnimLayerTypes layer)
        {
            if (Anim)
                Anim.CrossFade(shortNameHash, fFadeNormalizedTime, (int)layer);
        }
    
        public void Play(string stateName, float fFadeNormalizedTime, AnimLayerTypes layer)
        {
            if (!string.IsNullOrWhiteSpace(stateName))
                Play(Animator.StringToHash(stateName), fFadeNormalizedTime, layer);
        }

        public void OnActor_OnUpdate()
        {
            if (Anim)
            {
                var vTotalMoveDelta = Master.MoveCtrl.vTotalMoveDelta;
                vTotalMoveDelta.y = 0f;
                var sqrMag = vTotalMoveDelta.sqrMagnitude;
                if (sqrMag > float.Epsilon)
                {
                    vTotalMoveDelta *= 1f / Mathf.Sqrt(sqrMag);
                    
                    var vMasterForward = Master.transform.forward;
                    vMasterForward.y = 0;
                    vMasterForward.Normalize();
                    
                    var fAngleGap = Vector3.Angle(vMasterForward, vTotalMoveDelta);
                    if (Vector3.Cross(vMasterForward, vTotalMoveDelta).y < 0f)
                        fAngleGap *= -1f;
                        
                    Anim.SetFloat("Angle_ForwardToMoveDir", fAngleGap);   
                }
                else
                {
                    Anim.SetFloat("Angle_ForwardToMoveDir", 0f);   
                }
            }
        }
    }   
}