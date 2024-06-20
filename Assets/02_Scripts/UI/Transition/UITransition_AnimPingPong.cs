using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITransition_AnimPingPong : UITransitionBase
{
    [SerializeField] 
    private Animation _anim;

    [SerializeField]
    private AnimationClip _animClip_PingPong;

    private void Awake()
    {
        gameObject.SafeAddAnimClip(_animClip_PingPong);

        _anim = GetComponent(typeof(Animation)) as Animation;
        if (_anim != null)
        {
            _anim.playAutomatically = false;
            _anim.clip = _animClip_PingPong;
        }
    }

    public override void PlayAnim_Open(System.Action endCallback)
    {
        gameObject.SetActive(true);

        if (_anim == null || !_animClip_PingPong)
            return;
        
        AnimationState animState = _anim[_anim.clip.name];
        animState.time = 0;
        animState.speed = 1f;
        _anim.Play();

        StartCoroutine(_Cor_Check_AnimEnd(() =>
        {
            endCallback?.Invoke();
        }));
    }

    public override void PlayAnim_Close(System.Action endCallback)
    {
        if (_anim == null || !_animClip_PingPong)
            return;

        _anim.Play(_anim.clip.name);
        AnimationState animState = _anim[_anim.clip.name];
        animState.time = animState.length;
        animState.speed = -1f;
        
        StartCoroutine(_Cor_Check_AnimEnd(() =>
        {
            gameObject.SetActive(false);
            endCallback?.Invoke();
        }));
    }
    
    private IEnumerator _Cor_Check_AnimEnd(System.Action endCallback)
    {
        while (_anim != null && _anim.isPlaying)
            yield return null;

        endCallback?.Invoke();
    }
}