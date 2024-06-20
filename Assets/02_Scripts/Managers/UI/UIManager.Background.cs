using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public partial class UIManager
{
    public class BgSpaceInstInfo
    {
        public BgSpaceBase bgSpace;
        public RenderTexture rt;
        public HashSet<WindowBase> setWindowInstRef = new();
        public HashSet<WindowBase> setWindowRTRef = new();
    }
    
    private Dictionary<string, BgSpaceInstInfo> _dicBgSpace = new();
    private HashSet<BgSpaceInstInfo> _setBgSpaceDirty = new();

    private bool _isBgDirty = false;
    private HashSet<WindowBase> _setWindowMainCamRef = new();
    private Queue<RenderTexture> _queueRenderTextures = new();

    public bool NeedFlipY { get; private set; }

    private void _OnInit_Background()
    {
        for (int i = 0 ; i < 2; ++i)
            _queueRenderTextures.Enqueue(new(Screen.width, Screen.height, 32));
        
        NeedFlipY = SystemInfo.graphicsDeviceType switch
        {
            GraphicsDeviceType.Vulkan => false,
            GraphicsDeviceType.OpenGLCore => false,
            GraphicsDeviceType.OpenGLES2 => false,
            GraphicsDeviceType.OpenGLES3 => false, 
            _ => true
        };
    }

    private void _OnLateUpdate_Background()
    {
        if (!_isBgDirty)
            return;

        /* BgSpace */
        if (_setBgSpaceDirty.Count > 0)
        {
            foreach (var bgSpaceInstInfo in _setBgSpaceDirty)
            {
                var bgSpace = bgSpaceInstInfo.bgSpace;
                if (bgSpaceInstInfo.setWindowRTRef.Count > 0)
                {
                    bgSpaceInstInfo.rt = PopScreenRenderTexture();

                    if (bgSpace)
                    {
                        bgSpace.gameObject.SetActive(true);
                        bgSpace.Cam.targetTexture = bgSpaceInstInfo.rt;

                        if (NeedFlipY)
                        {
                            foreach (var window in bgSpaceInstInfo.setWindowRTRef)
                            {
                                window.uiImg_Bg.texture = bgSpaceInstInfo.rt;
                                window.uiImgPerfect.Update_SizeDelta();
                                window.uiImg_Bg.uvRect = new Rect(0f, 0f, 1f, 1f);
                            }
                        }
                        else
                        {
                            foreach (var window in bgSpaceInstInfo.setWindowRTRef)
                            {
                                window.uiImg_Bg.texture = bgSpaceInstInfo.rt;
                                window.uiImgPerfect.Update_SizeDelta();
                            }
                        }
                    }
                    else
                    {
                        ScreenCapture.CaptureScreenshotIntoRenderTexture(bgSpaceInstInfo.rt);

                        if (NeedFlipY)
                        {
                            foreach (var window in bgSpaceInstInfo.setWindowRTRef)
                            {
                                window.uiImg_Bg.texture = bgSpaceInstInfo.rt;
                                window.uiImgPerfect.Update_SizeDelta();
                                window.uiImg_Bg.uvRect = new Rect(0f, 1f, 1f, -1f);
                            }
                        }
                        else
                        {
                            foreach (var window in bgSpaceInstInfo.setWindowRTRef)
                            {
                                window.uiImg_Bg.texture = bgSpaceInstInfo.rt;
                                window.uiImgPerfect.Update_SizeDelta();
                            }
                        }
                    }
                }
                else
                {
                    PushScreenRenderTexture(bgSpaceInstInfo.rt);
                    bgSpaceInstInfo.rt = null;
                    if (bgSpace)
                    {
                        bgSpace.gameObject.SetActive(false);
                        bgSpace.Cam.targetTexture = null;
                    }
                }
            }
            _setBgSpaceDirty.Clear();
        }
        
        /* MainCam */
        {
            var mainCam = Managers.Cam.MainCam;
            var mainCamData = mainCam.GetUniversalAdditionalCameraData();
            var uiCamData = UICam.GetUniversalAdditionalCameraData();

            if (_setWindowMainCamRef.Count > 0)
            {
                if (uiCamData.renderType != CameraRenderType.Overlay)
                {
                    uiCamData.renderType = CameraRenderType.Overlay;
                    mainCamData.cameraStack.Add(UICam);
                    mainCam.enabled = true;
                }
            }
            else 
            {
                if (uiCamData.renderType != CameraRenderType.Base)
                {
                    mainCam.enabled = false;
                    mainCamData.cameraStack.Clear();
                    uiCamData.renderType = CameraRenderType.Base;   
                }
            }

            _isBgDirty = false;
        }
    }

    private void _OnDestroy_Background()
    {
        foreach (var rt in _queueRenderTextures)
            DestroyImmediate(rt);
        
        _queueRenderTextures.Clear();
    }

    private void _RegistBgSpaceInst(WindowBase windowInst)
    {
        if (!windowInst || !windowInst.HasBgSpace)
            return;
        
        if (!_dicBgSpace.TryGetValue(windowInst.bgSpaceTypeName, out var bgSpaceInstInfo))
        {
            /* Load BgSpace */
            var bgSpaceType = System.Type.GetType(windowInst.bgSpaceTypeName);
            var bgSpaceInfo = bgSpaceType.GetCustomAttribute<BgSpaceInfo>();
            
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(bgSpaceInfo.sceneAddress))
                Debug.LogError($"{bgSpaceType.Name} :: Scene경로 세팅 안됨");
#endif
            Managers.Asset.LoadScene(bgSpaceInfo.sceneAddress, LoadSceneMode.Additive, true);
            
            bgSpaceInstInfo = new();
            _dicBgSpace.Add(bgSpaceType.Name, bgSpaceInstInfo);
        }
                    
        bgSpaceInstInfo.setWindowInstRef.Add(windowInst);
        if (bgSpaceInstInfo.bgSpace)
            windowInst.OnWindow_SetBgSpace(bgSpaceInstInfo.bgSpace);
    }

    private void _UnregistBgSpaceInst(WindowBase windowInst)
    {
        if (!windowInst
            || !windowInst.HasBgSpace
            || !_dicBgSpace.TryGetValue(windowInst.bgSpaceTypeName, out var bgSpaceInstInfo))
            return;

        windowInst.OnWindow_SetBgSpace(null);
        bgSpaceInstInfo.setWindowInstRef.Remove(windowInst);
        if (bgSpaceInstInfo.setWindowInstRef.Count > 0)
            return;
        
        var bgSpaceType = System.Type.GetType(windowInst.name);
        var bgSpaceInfo = bgSpaceType.GetCustomAttribute<BgSpaceInfo>();
        
        _dicBgSpace.Remove(windowInst.bgSpaceTypeName);
        Managers.Asset.UnloadScene(bgSpaceInfo.sceneAddress);
    }
    
    public BgSpaceInstInfo GetBgSpaceInfo(string bgSpaceTypeName)
    {
        return _dicBgSpace.GetOrNull(bgSpaceTypeName);
    }
    
    public RenderTexture PopScreenRenderTexture()
    {
        if (_queueRenderTextures.Count == 0)
            return new(Screen.width, Screen.height, 32);

        return _queueRenderTextures.Dequeue();
    }

    public void PushScreenRenderTexture(RenderTexture rt)
    {
        if (rt == null)
            return;
        
        _queueRenderTextures.Enqueue(rt);
    }
}