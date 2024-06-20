using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITransition_AnimStartLoopEnd : UITransitionBase
{
    [SerializeField] 
    private Animation _anim;

    [SerializeField]
    private AnimationClip _animClip_Start;
    [SerializeField] 
    private AnimationClip _animClip_Loop;
    [SerializeField] 
    private AnimationClip _animClip_End;

    private void Awake()
    {
        gameObject.SafeAddAnimClip(_animClip_Start);
        gameObject.SafeAddAnimClip(_animClip_Loop);
        gameObject.SafeAddAnimClip(_animClip_End);
        
        _anim = GetComponent(typeof(Animation)) as Animation;
        if(_anim != null)
            _anim.playAutomatically = false;
    }

    public override void PlayAnim_Open(System.Action endCallback)
    {
        gameObject.SetActive(true);

        if (_anim == null || !_animClip_Start)
            return;
        
        _anim.Play(_animClip_Start.name, PlayMode.StopAll);
        StartCoroutine(_Cor_Check_AnimEnd(() =>
        {
            if (!_animClip_Loop)
                _anim.CrossFade(_animClip_Loop.name, 0.3f);
                
            endCallback.Invoke();
        }));
    }

    public override void PlayAnim_Close(System.Action endCallback)
    {
        if (_anim == null || !_animClip_End)
            return;
        
        _anim.CrossFade(_animClip_End.name, 0.3f);
        StartCoroutine(_Cor_Check_AnimEnd(() =>
        {
            gameObject.SetActive(false);
            endCallback.Invoke();
        }));
    }

    private IEnumerator _Cor_Check_AnimEnd(System.Action endCallback)
    {
        while (_anim != null && _anim.isPlaying)
            yield return null;

        endCallback?.Invoke();
    }
}