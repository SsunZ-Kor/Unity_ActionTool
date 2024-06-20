using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class WindowBase : MonoBehaviour
{
    public enum BgmType
    {
        None,           // BGM 없음
        Loop,
        KeepPrev,    // 이전 BGM 유지
    }
    
    public LinkedListNode<WindowBase> Node { get; private set; }

    [SerializeField][HideInInspector]
    protected Animation _anim = null;
    [SerializeField][HideInInspector]
    protected AnimationClip _animClip_Open;
    [SerializeField][HideInInspector]
    protected AnimationClip _animClip_Close;
    
    // Popup형태 (이전 스택의 UI와 겹침) 여부
    [HideInInspector]
    public bool isOverlay = false;
    [HideInInspector]
    public string bgSpaceTypeName = UIManager.BgSpaceType_None;
    [HideInInspector] 
    public RawImage uiImg_Bg;

    public UGUIParentPerfectRawImage uiImgPerfect { get; private set; }

    // BGM 관련
    [SerializeField][HideInInspector]
    private BgmType _useBgm = BgmType.None;
    [SerializeField][HideInInspector]
    private AudioClip _bgm_audioClip = null;
    [SerializeField][HideInInspector]
    private bool _bgm_keepPrevNormalizedTime = false;
    [SerializeField][HideInInspector]
    private BGMChangeType _bgm_FadeType = BGMChangeType.CrossFade;
    [SerializeField][HideInInspector]
    private float _bgm_FadeOutTime = 1f;
    [SerializeField][HideInInspector]
    private float _bgm_FaddInTime = 1f;

    protected BgSpaceBase _bgSpace { get; private set; }

    public bool HasBgSpace => !isOverlay
                              && uiImg_Bg != null
                              && HasValidBgSpaceType;
    
    private bool HasValidBgSpaceType => !string.IsNullOrWhiteSpace(bgSpaceTypeName) 
                                        && bgSpaceTypeName != UIManager.BgSpaceType_None
                                        && bgSpaceTypeName != UIManager.BgSpaceType_MainCam;
    
    protected virtual void Awake()
    {
        Node = new(this);

        /* Anim */
        gameObject.SafeAddAnimClip(_animClip_Open);
        gameObject.SafeAddAnimClip(_animClip_Close);

        _anim = GetComponent(typeof(Animation)) as Animation;
        if (_anim != null)
            _anim.playAutomatically = false;

        /* BgSpace */
        if (!isOverlay)
        {
            if(!uiImg_Bg && HasValidBgSpaceType)
            {
                var trBg = transform.Find("BG");
                if (trBg != null)
                    uiImg_Bg = trBg.GetOrAddComponent<RawImage>();
            }
        }

        if (!uiImgPerfect)
            uiImgPerfect = uiImg_Bg.GetOrAddComponent<UGUIParentPerfectRawImage>();
    }

    protected virtual void OnDestroy()
    {
        if (Node != null)
        {
            Node.RemoveSelf();
            Node.Value = null;
            Node = null;
        }

        _bgSpace = null;
    }

    public virtual void CloseWindow()
    {
        Managers.UI.CloseWindow(this, true);
    }

    public virtual void OnWindow_InTopStack(bool isOpened){}
    
    public virtual void OnWindow_OutStack(){}
    
    public virtual void OnWindow_OutTopStack(bool isClosed){}
    
    public virtual void OnWindow_Show(bool isOpened){}

    public virtual void OnWindow_Hide(bool isClosed){}

    public virtual void OnWindow_OpenAnimEnd(){}

    public virtual void OnWindow_CloseAnimEnd(){}

    public virtual void OnWindow_Refresh(){}

    public virtual void OnWindow_SetBgSpace(BgSpaceBase bgSpace)
    {
        _bgSpace = bgSpace;
    }
    
    #region Window Anim

    public void PlayAnim(string clipName)
    {
        _anim.CrossFade(clipName,0.1f);
    }

    public bool HasAnim_Open() => _animClip_Open != null;
    public bool HasAnim_Close() => _animClip_Close != null;

    public void PlayAnim_Open()
    {
        if (_anim == null)
            return;

        if (_animClip_Open == null)
        {
            _anim.Stop();
            return;
        }

        _anim.SampleAnimClip(_animClip_Open.name, 0f);
        _anim.Play(_animClip_Open.name);
        _anim[_animClip_Open.name].wrapMode = WrapMode.Clamp;
    }

    public void PlayAnim_Close()
    {
        if (_anim == null)
            return;

        if (_animClip_Close == null)
        {
            _anim.Stop();
            return;
        }

        _anim.SampleAnimClip(_animClip_Close.name, 0f);
        _anim.Play(_animClip_Close.name);
        _anim[_animClip_Close.name].wrapMode = WrapMode.Clamp;
    }
    
    
    public bool IsAnimPlaying_Open()
    {
        return _anim != null
               && _anim.enabled
               && _animClip_Open != null
               && _anim.IsPlaying(_animClip_Open.name);
    }

    public bool IsAnimPlaying_Close()
    {
        return _anim != null
               && _anim.enabled
               && _animClip_Close != null
               && _anim.IsPlaying(_animClip_Close.name);
    }

    public void SampleAnim_Open(float factor)
    {
        if (_animClip_Open != null)
            _anim.SampleAnimClip(_animClip_Open.name, factor);
    }

    public void SampleAnim_Close(float factor)
    {
        if (_animClip_Close != null)
            _anim.SampleAnimClip(_animClip_Close.name, factor);
    }
    
    #endregion Window Anim
}