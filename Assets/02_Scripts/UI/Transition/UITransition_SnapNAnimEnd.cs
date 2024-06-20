using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITransition_SnapNAnimEnd : UITransitionBase
{
    [SerializeField] 
    private Animation _anim;

    [SerializeField] 
    private AnimationClip _animClip_End;
    
    [SerializeField]
    protected RawImage _img_ScreenShot;
    [SerializeField] 
    protected CanvasGroup _canvasGroup;
    
    private void Awake()
    {
        gameObject.SafeAddAnimClip(_animClip_End);
        
        _anim = GetComponent(typeof(Animation)) as Animation;
        if(_anim != null)
            _anim.playAutomatically = false;
    }
    
    private void OnEnable()
    {
        // 텍스쳐 생성
        _img_ScreenShot.texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    }

    private void OnDisable()
    {
        //텍스쳐 해제
        Destroy(_img_ScreenShot.texture);
        _img_ScreenShot.texture = null;
    }

    public override void PlayAnim_Open(System.Action endCallback)
    {
        // 스크린 샷에 방해 되지 않도록 알파를 0으로 만듦
        _canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
            
        // 애니메이션을 초기화 하되, Play는 하지 않는다.
        if (_animClip_End)
            _anim.SampleAnimClip(_animClip_End.name, 0f);

        StopAllCoroutines();

        // 스크린 샷 코루틴 시작
        StartCoroutine( Cor_ScreenShot(endCallback));
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
    
    private IEnumerator Cor_ScreenShot(System.Action endCallback)
    {
        // 프레임 종료 대기
        yield return new WaitForEndOfFrame();

        // 스크린샷
        var tex = _img_ScreenShot.texture as Texture2D;
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();

        // 바로 보여준다.
        _canvasGroup.alpha = 1f;

        // 콜백 처리
        endCallback?.Invoke();
    }
}