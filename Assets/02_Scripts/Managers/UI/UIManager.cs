using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public partial class UIManager : MonoBehaviour, IManager
{
    public Canvas Canvas { get; private set; }
    public RectTransform RttrCanvas { get; private set; }
    public EventSystem EventSystem { get; private set; }
    public Camera UICam { get; private set; }

    public IEnumerator Init()
    {
        // Create :: UICamera
        var prfCamera = Resources.Load("UI_Prefabs/UICamera");
        var goCamera = Instantiate(prfCamera, this.transform) as GameObject;
        UICam = goCamera.GetComponent(typeof(Camera)) as Camera;
        UICam.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
        Managers.Cam.MainCam.GetUniversalAdditionalCameraData().cameraStack.Add(UICam);
        
        // Create :: Canvas
        var prfCanvas = Resources.Load("UI_Prefabs/CanvasMain");
        var goCanvas = Instantiate(prfCanvas, this.transform) as GameObject;
        Canvas = goCanvas.GetComponent(typeof(Canvas)) as Canvas;

        Canvas.worldCamera = UICam;
        RttrCanvas = goCanvas.transform as RectTransform;

        // Create :: EventSystem
        var prfEventSystem = Resources.Load("UI_Prefabs/EventSystem");
        var goEventSystem = Instantiate(prfEventSystem, this.transform) as GameObject;
        EventSystem = goEventSystem.GetComponent(typeof(EventSystem)) as EventSystem;

        yield return null;
        
        OnInit_Window();
        OnInit_Transition();
        _OnInit_Background();
        yield break;
    }

    public void Release()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        OnUpdate_Window();
    }

    private void LateUpdate()
    {
        _OnLateUpdate_Background();
    }

    private void OnDestroy()
    {
        _OnDestroy_Background();
    }
}