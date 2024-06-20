using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class SceneController_Intro : SceneControllerBase
{
    [SerializeField] 
    private GameObject _root_Intro;
    [SerializeField] 
    private Animation _anim_Intro;
    [SerializeField]
    private Canvas _canvas_Intro;
    [SerializeField] 
    private Camera _cam_Intro;
    
    
    private Window_Intro_Main _window_Intro_Main;
    
    protected override void Awake()
    {
        //base.Awake();
        gameObject.tag = TAG_SCENE_CONTROLLER;
        StartCoroutine(OnScene_Init(null));
    }

    public override IEnumerator OnScene_Init(InitParams initParams)
    {
        yield return Managers.Init(OnManager_InitState);
        yield return base.OnScene_Init(initParams);
    }

    private void OnManager_InitState(Managers.InitState initState)
    {
        if (_window_Intro_Main != null)
            _window_Intro_Main.SetTotalProgress((initState - Managers.InitState.UI_End) / (float)(Managers.InitState.Finish - Managers.InitState.UI_End));
        
        switch (initState)
        {
            case Managers.InitState.UI_End:
            {
                // UI Manager Camera에 Canvas 이양 및 Intro 전용 UIcam 파기
                _canvas_Intro.worldCamera = Managers.UI.UICam;
                Destroy(_cam_Intro.gameObject);
                _cam_Intro = null;
                
                // Open Intro Window
                _window_Intro_Main = Managers.UI.OpenWindow<Window_Intro_Main>();
                _anim_Intro.Play("IntroPanel_End");

                // IntroAnim 종료 대기 및 파기
                StartCoroutine(WaitForLogoEnd());
                IEnumerator WaitForLogoEnd()
                {
                    while (_anim_Intro.isPlaying)
                        yield return null;
                    
                    Destroy(_root_Intro);
                }
            }
            break;
            case Managers.InitState.Finish:
            {
                _window_Intro_Main.SetTouchToStart(MoveToNextScene);
            }
            break;
        }

    }

    private void MoveToNextScene()
    {
        Managers.Scene.ChangeScene(SceneTypes.World, UITransitionTypes.Fade_White, new SceneController_World.InitParams());
    }
}