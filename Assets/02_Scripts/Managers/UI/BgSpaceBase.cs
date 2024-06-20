using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

[BgSpaceInfo("")]
public abstract class BgSpaceBase : MonoBehaviour
{
    [SerializeField]
    private Camera _cam;

    public Camera Cam => _cam;

    private UIManager.BgSpaceInstInfo _instInfo;

    protected void Awake()
    {
#if UNITY_EDITOR
        if (!Managers.IsValid)
            return;
#endif

        _instInfo = Managers.UI.GetBgSpaceInfo(GetType().Name);
        if (_instInfo != null)
        {
            _instInfo.bgSpace = this;

            if (Managers.UI.NeedFlipY)
            {
                foreach (var window in _instInfo.setWindowInstRef)
                {
                    window.uiImg_Bg.uvRect = new Rect(0, 0, 1, 1);
                    window.OnWindow_SetBgSpace(this);
                }
            }
            else
            {
                foreach (var window in _instInfo.setWindowInstRef)
                    window.OnWindow_SetBgSpace(this);
            }
            
            gameObject.SetActive(_instInfo.setWindowRTRef.Count > 0);
            _cam.targetTexture = _instInfo.rt;
        }
        else
        {
            var bgSpaceInfo = GetType().GetCustomAttribute<BgSpaceInfo>();
            Managers.Asset.UnloadScene(bgSpaceInfo.sceneAddress);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instInfo != null)
        {
            _instInfo.setWindowInstRef.Clear();
            _instInfo.setWindowRTRef.Clear();
            _instInfo.setWindowInstRef = null;
            _instInfo.setWindowRTRef = null;
            _instInfo = null;   
        }
    }
}

public class BgSpaceInfo : Attribute
{
    public string sceneAddress = null;
    
    public BgSpaceInfo(string sceneAddress)
    {
        this.sceneAddress = sceneAddress;
    }
}