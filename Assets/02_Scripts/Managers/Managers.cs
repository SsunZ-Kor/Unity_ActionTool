using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class Managers
{
    public enum InitState
    {
        None,
        Start,
        
        BGM_Start,
        BGM_End,
        Sfx_Start,
        Sfx_End,
        Fx_Start,
        Fx_End,
        Cam_Start,
        Cam_End,
        Input_Start,
        Input_End,
        UI_Start,
        UI_End,
        Asset_Start,
        Asset_End,
        Scene_Start,
        Scene_End,
        Actor_Start,
        Actor_End,
        
        Finish,
        Editor,
    }
    
    private static GameObject _s_goManagerRoot; 
    
    private static BGMManager _bgm = null;
    public static BGMManager BGM => _bgm ??= CreateManager<BGMManager>(true);
    
    private static SfxManager _sfx = null;
    public static SfxManager Sfx => _sfx ??= CreateManager<SfxManager>(true);

    private static FxManager _fx = null;
    public static FxManager Fx => _fx ??= CreateManager<FxManager>(true);

    private static CameraManager _cam = null;
    public static CameraManager Cam => _cam ??= CreateManager<CameraManager>(true);
    
    private static GameInputManager _input = null;
    public static GameInputManager Input => _input ??= CreateManager<GameInputManager>(true);

    private static UIManager _ui = null;
    public static UIManager UI => _ui ??= CreateManager<UIManager>(true);
    
    private static AssetManager _asset = null;
    public static AssetManager Asset => _asset ??= CreateManager<AssetManager>(true);

    private static SceneManager _scene = null;
    public static SceneManager Scene => _scene ??= CreateManager<SceneManager>(true);

    private static ActorManager _actor = null;
    public static ActorManager Actor => _actor ??= CreateManager<ActorManager>(true);

    private static InitState _initState = InitState.None;

    public static bool IsValid => _initState == InitState.Finish || _initState == InitState.Editor;
    
    public static IEnumerator Init(System.Action<InitState> initStateCallback)
    {
        _initState = InitState.Start;
        
        _bgm = CreateManager<BGMManager>(false);
        yield return InitManager(_bgm,   InitState.BGM_Start,   InitState.BGM_End,   initStateCallback);
        
        _sfx = CreateManager<SfxManager>(false);
        yield return InitManager(_sfx,   InitState.Sfx_Start,   InitState.Sfx_End,   initStateCallback);
        
        _fx = CreateManager<FxManager>(false);
        yield return InitManager(_fx,    InitState.Fx_Start,    InitState.Fx_End,    initStateCallback);

        _cam = CreateManager<CameraManager>(false);
        yield return InitManager(_cam,    InitState.Cam_Start,    InitState.Cam_End,    initStateCallback);
        
        _input = CreateManager<GameInputManager>(false);
        yield return InitManager(_input,    InitState.Input_Start,    InitState.Input_End,    initStateCallback);
        
        _ui = CreateManager<UIManager>(false);
        yield return InitManager(_ui,    InitState.UI_Start,    InitState.UI_End,    initStateCallback);
        
        _asset = CreateManager<AssetManager>(false);
        yield return InitManager(_asset, InitState.Asset_Start, InitState.Asset_End, initStateCallback);
        
        _scene = CreateManager<SceneManager>(false);
        yield return InitManager(_scene, InitState.Scene_Start, InitState.Scene_End, initStateCallback);
        
        _actor = CreateManager<ActorManager>(false);
        yield return InitManager(_actor, InitState.Actor_Start, InitState.Actor_End, initStateCallback);

        _initState = InitState.Finish;
        initStateCallback?.Invoke(InitState.Finish);
    }

    private static IEnumerator InitManager<T>(T initTarget, InitState startState, InitState endState, Action<InitState> callback) where T : IManager, new()
    {
        _initState = startState;
        callback?.Invoke(startState);

        yield return null;
        yield return initTarget.Init();
        yield return null;

        _initState = endState;
        callback?.Invoke(endState);
    }

    public static void RestartGame()
    {
        _ui.ShowTransition(UITransitionTypes.Fade_Black, () =>
        {
            if (_bgm   != null) { _bgm  .Release(); _bgm   = null; }
            if (_sfx   != null) { _sfx  .Release(); _sfx   = null; }
            if (_fx    != null) { _fx   .Release(); _fx    = null; }
            if (_cam   != null) { _cam  .Release(); _cam   = null; }
            if (_input != null) { _input.Release(); _input = null; }
            if (_ui    != null) { _ui   .Release(); _ui    = null; }
            if (_asset != null) { _asset.Release(); _asset = null; }
            if (_scene != null) { _scene.Release(); _scene = null; }
            if (_actor != null) { _actor.Release(); _actor = null; }
        
            Object.Destroy(_s_goManagerRoot);

            UnityEngine.SceneManagement.SceneManager.LoadScene("zz_Restart", LoadSceneMode.Single);
        });
        
    }

    private static T CreateManager<T>(bool bWithInit) where T : IManager, new()
    {
        if (_initState == InitState.None)
            _initState = InitState.Editor;

        T result;

        var type = typeof(T);
        if (type.IsSubclassOf(typeof(MonoBehaviour)))
        {
            if (_s_goManagerRoot == null)
            {
                _s_goManagerRoot = new GameObject("Managers");
                Object.DontDestroyOnLoad(_s_goManagerRoot);
            }
            
            var goManager = new GameObject(type.Name, type);
            goManager.transform.SetParent(_s_goManagerRoot.transform);
            result = goManager.GetComponent<T>();
        }
        else
        {
            result = new T();
        }

        if (bWithInit)
        {
            var cor = result.Init();
            while (cor.MoveNext()) { }
        }
        
        return result;
    }
}

public interface IManager
{
    IEnumerator Init();
    void Release();
}