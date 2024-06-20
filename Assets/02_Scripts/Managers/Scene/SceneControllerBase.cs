using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneControllerBase : MonoBehaviour
{
    public const string TAG_SCENE_CONTROLLER = "SceneController";

    public abstract class InitParams
    {
        
    }

    protected virtual void Awake()
    {
        gameObject.tag = TAG_SCENE_CONTROLLER;
        if (Managers.IsValid == false)
            StartCoroutine(OnScene_Init(null));
    }

    /// <summary>
    /// override시 반드시 마지막 줄에서 호출해줄 것
    /// </summary>
    public virtual IEnumerator OnScene_Init(InitParams initParams)
    {
        Managers.Scene.OnScene_InitDone(this);
        yield break;
    }

    /// <summary>
    /// Scene Load Transition의 StartAnim이 끝난 직후
    /// </summary>
    public virtual void OnScene_Start()
    {
        
    }

    
    /// <summary>
    /// Scene Load Transition의 EndAnim 시작 직전
    /// </summary>
    public virtual void OnScene_End()
    {
        
    }
    
    /// <summary>
    /// Scene Load Transition의 EndAnim 시작 직전
    /// </summary>
    public virtual void OnScene_Close()
    {
        
    }
}
