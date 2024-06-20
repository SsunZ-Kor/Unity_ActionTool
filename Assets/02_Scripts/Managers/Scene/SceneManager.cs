using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum SceneTypes
{
    Intro = 0,
    World = 1,
}

public class SceneManager : IManager
{
    public SceneTypes CrrSceneType { get; private set; } = SceneTypes.Intro;
    public SceneControllerBase CrrSceneCtrl { get; private set; } = null;
    public bool IsLoading { get; private set; } = false;

    private SceneControllerBase.InitParams _initParamsForNextScene = null;

    public IEnumerator Init()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += _OnScene_Loaded;
        
        var goSceneCtrl = GameObject.FindGameObjectWithTag(SceneControllerBase.TAG_SCENE_CONTROLLER);
        CrrSceneCtrl = goSceneCtrl.GetComponent<SceneControllerBase>();
        
        yield break;
    }

    public void Release()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= _OnScene_Loaded;

        CrrSceneCtrl = null;
    }

    public void ChangeScene(SceneTypes sceneType, UITransitionTypes transitionType, SceneControllerBase.InitParams initParams = null)
    {
        if (IsLoading || CrrSceneType == sceneType)
            return;

        IsLoading = true;
        CrrSceneType = sceneType;
        _initParamsForNextScene = initParams;

        if (CrrSceneCtrl != null)
            CrrSceneCtrl.OnScene_End();
        
        Managers.UI.ShowTransition(
            transitionType, 
            () =>
            {
                /* Manager Release */
                Managers.UI.ClearAllWindow(true);
                Managers.UI.ClearAllTransition();
                Managers.Sfx.OnScene_Closed();
                Managers.Fx.OnScene_Closed();
                Managers.Actor.OnScene_Closed();
                
                /* SceneLoad. 이후 OnScene_Loaded가 호출됨 */
                CrrSceneCtrl.OnScene_Close();
                UnityEngine.SceneManagement.SceneManager.LoadScene((int)sceneType, LoadSceneMode.Single);
            });
    }

    private void _OnScene_Loaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex != (int)CrrSceneType)
            return;

        var rootGameObjects = scene.GetRootGameObjects();
        foreach (var rootObject in rootGameObjects)
        {
            CrrSceneCtrl = rootObject.GetComponent<SceneControllerBase>();
            if (CrrSceneCtrl != null)
                break;
        }
        
        CrrSceneCtrl.StartCoroutine(CrrSceneCtrl.OnScene_Init(_initParamsForNextScene));
        _initParamsForNextScene = null;
    }

    public void OnScene_InitDone(SceneControllerBase crrSceneController)
    {
        System.GC.Collect();
        IsLoading = false;
        Managers.UI.HideTransition(CrrSceneCtrl.OnScene_Start);
    }
}