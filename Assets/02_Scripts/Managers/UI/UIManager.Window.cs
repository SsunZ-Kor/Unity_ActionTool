using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public partial class UIManager
{
    private RectTransform _root_Window;
    
    public void OnInit_Window()
    {
        _root_Window = Canvas.transform.Find("root_Window") as RectTransform;
    }

    private Dictionary<System.Type, WindowBase> _dicwindowInst = new();
    private LinkedList<WindowBase> _llistWindowStack = new();

    private Coroutine _corWaitForEndOpenAnim = null;
    private Dictionary<WindowBase, Coroutine> _dicCorWaitForEndCloseAnim = new();

    private void OnUpdate_Window()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var window = _llistWindowStack.Last?.Value;
            if (window == null)
                return;
            
            window.CloseWindow();
        }
    }
    
    public T OpenWindow<T>(bool withAnim = true) where T : WindowBase
    {
        // 최상단 UI가 현재 UI 인지 체크
        {
            var node = _llistWindowStack.Last;
            if (node != null && node.Value.GetType() == typeof(T))
                return node.Value as T;
        }
        
        var openWindow = GetWindow<T>(true);
        if (openWindow == null)
            return null;

        // 꺼지던 중인지 체크
        if (_dicCorWaitForEndCloseAnim.TryGetValue(openWindow, out var corWaitForEndCloseAnim))
        {
            StopCoroutine(corWaitForEndCloseAnim);
            _dicCorWaitForEndCloseAnim.Remove(openWindow);
        }

        // 노드 초기화
        openWindow.Node.RemoveSelf();
        _llistWindowStack.AddLast(openWindow.Node);
        
        // 이전 UI 이벤트 처리 :: OnEvent_OutTopStack
        var prvNode = openWindow.Node.Previous;
        if (prvNode != null)
            prvNode.Value.OnWindow_OutTopStack(false);
        
        // 현재 UI 이벤트 처리 :: OnEvent_InTopStack, OnEvent_Show
        openWindow.OnWindow_InTopStack(true);
        openWindow.transform.SetSiblingIndex(openWindow.transform.parent.childCount - 1);
        
        _ActiveWindow(openWindow, true);
        if (openWindow.HasAnim_Open())
        {
            if (withAnim)
                openWindow.PlayAnim_Open();
            else
                openWindow.SampleAnim_Open(1f);
        }

        // 일반 이면 뒷 UI Off
        if (!openWindow.isOverlay)
        {
            // Open Anim 처리
            if (_corWaitForEndOpenAnim != null)
            {
                StopCoroutine(_corWaitForEndOpenAnim);
                _corWaitForEndOpenAnim = null;
            }

            if (openWindow.IsAnimPlaying_Open())
            {
                _corWaitForEndOpenAnim = StartCoroutine(Cor_WaitForEndAnim());
                IEnumerator Cor_WaitForEndAnim()
                {
                    while (openWindow.IsAnimPlaying_Open())
                        yield return null;

                    _corWaitForEndOpenAnim = null;
                    _HidePrvWindows();
                }
            }
            else
            {
                _HidePrvWindows();
            }
            
            void _HidePrvWindows()
            {
                openWindow.OnWindow_OpenAnimEnd();
                prvNode = openWindow.Node.Previous;
                while (prvNode != null && prvNode.Value.gameObject.activeSelf)
                {
                    var prvWindow =  prvNode.Value;
                    prvNode = prvNode.Previous;
                
                    // 이전 UI 이벤트 처리 :: OnEvent_Hide
                    _DeactiveWindow(prvWindow, false);
                }
            }
        }
        
        return openWindow;
    }
    
    public void CloseWindow<T>(bool withAnim) where T : WindowBase
    {
        _dicwindowInst.TryGetValue(typeof(T), out var closeWindow);
        CloseWindow(closeWindow, withAnim);
    }

    public void CloseWindow(WindowBase closeWindow, bool withAnim)
    {
        // 이미 닫혀있는지 체크
        if (closeWindow == null || closeWindow.Node.List == null)
            return;

        // Node 제거 및 Top Stack 처리
        var isTop = closeWindow.Node == _llistWindowStack.Last;
        closeWindow.Node.RemoveSelf();
        closeWindow.OnWindow_OutStack();
        if (isTop)
        {
            // 켜지던 중이면 취소
            if (_corWaitForEndOpenAnim != null)
            {
                StopCoroutine(_corWaitForEndOpenAnim);
                _corWaitForEndOpenAnim = null;
            }
            
            closeWindow.OnWindow_OutTopStack(true);
        }
        
        // 이미 꺼져있다면 호출스택 탈출
        if (!closeWindow.gameObject.activeSelf)
            return;

        // CloseAnim 처리
        if (withAnim && closeWindow.HasAnim_Close())
        {
            closeWindow.PlayAnim_Close();
            var cor = StartCoroutine(WaitForEndAnim());
            _dicCorWaitForEndCloseAnim.Add(closeWindow, cor);

            IEnumerator WaitForEndAnim()
            {
                var wnd = closeWindow;
                while (wnd.IsAnimPlaying_Close())
                    yield return null;

                _dicCorWaitForEndCloseAnim.Remove(closeWindow);
                HideCrrWindow();
            }
        }
        else
        {
            HideCrrWindow();
        }

        // 기존 UI Show
        var prvNode = _llistWindowStack.Last;
        if (prvNode != null)
        {
            prvNode.Value.OnWindow_InTopStack(false);
            if (!closeWindow.isOverlay)
            {
                while (prvNode != null)
                {
                    var prvWindow =  prvNode.Value;
                    prvNode = prvNode.Previous;
            
                    // 이전 UI 이벤트 처리 :: OnWindow_Show
                    _ActiveWindow(prvWindow, false);
                    prvWindow.SampleAnim_Open(1f);
                    if (!prvWindow.isOverlay)
                        break;
                }
            }
        }

        void HideCrrWindow()
        {
            closeWindow.OnWindow_CloseAnimEnd();
            _DeactiveWindow(closeWindow, true);
        }
    }

    public void ClearAllWindow(bool bDestroy)
    {
        var node = _llistWindowStack.Last;
        while (node != null)
        {
            var window = node.Value;
            node = node.Previous;
            
            window.OnWindow_OutTopStack(true);
            window.OnWindow_OutStack();
            _DeactiveWindow(window, true);
        }
        
        _llistWindowStack.Clear();

        if (bDestroy)
        {
            foreach (var pair in _dicwindowInst)
            {
                var window = pair.Value;
                _UnregistBgSpaceInst(window);
                Destroy(pair.Value.gameObject);
                Managers.Asset.ReleaseHandle($"UI/{pair.Key.Name}");
            }
            
            _dicwindowInst.Clear();
        }
    }

    public void RefreshWindow(bool bOnlyVisible)
    {
        var node = _llistWindowStack.Last;
        while (node != null)
        {
            var window = node.Value;
            node = node.Previous;

            if (bOnlyVisible && !window.isActiveAndEnabled)
                break;
                
            window.OnWindow_Refresh();
        }
    }

    private void _ActiveWindow(WindowBase window, bool isOpen)
    {
        window.gameObject.SetActive(true);
        window.OnWindow_Show(isOpen);
        
        if (window.isOverlay)
            return;
        
        _isBgDirty = true;

        switch (window.bgSpaceTypeName)
        {
            case BgSpaceType_None:
                break;
            case BgSpaceType_MainCam:
                _setWindowMainCamRef.Add(window);
                break;
            default:
                if (_dicBgSpace.TryGetValue(window.bgSpaceTypeName, out var bgSpaceInstInfo))
                {
                    bgSpaceInstInfo.setWindowRTRef.Add(window);
                    _setBgSpaceDirty.Add(bgSpaceInstInfo);
                }
                break;
        }   
    }

    private void _DeactiveWindow(WindowBase window, bool isClose)
    {
        window.gameObject.SetActive(false);
        window.OnWindow_Hide(isClose);

        if (window.isOverlay)
            return;
        
        _isBgDirty = true;

        switch (window.bgSpaceTypeName)
        {
            case BgSpaceType_None:
                break;
            case BgSpaceType_MainCam:
                _setWindowMainCamRef.Remove(window);
                break;
            default:
                if (_dicBgSpace.TryGetValue(window.bgSpaceTypeName, out var bgSpaceInstInfo))
                {
                    window.uiImg_Bg.texture = null;
                    bgSpaceInstInfo.setWindowRTRef.Remove(window);
                    _setBgSpaceDirty.Add(bgSpaceInstInfo);
                }
                break;
        }   
    }
    
    public T GetWindow<T>(bool bCreateIfNotFound) where T : WindowBase
    {
        var t = typeof(T);
        
        if (_dicwindowInst.TryGetValue(t, out var window))
            return window as T;

        if (bCreateIfNotFound)
        {
            var isSystemUI = t.GetCustomAttribute<SystemUI>() != null;
            
            var prfWindow = isSystemUI 
                ? Resources.Load<GameObject>($"UI_Prefabs/{t.Name}")
                : Managers.Asset.Load<GameObject>($"UI/{t.Name}.prefab");
            
            if (prfWindow == null)
                throw new System.Exception($"UIManager :: Not Found Window Prefab \"{t.Name}\"");

            var goWindow = Instantiate(prfWindow, _root_Window);
            window = goWindow.GetComponent<T>();

            _dicwindowInst.Add(t, window);
            _RegistBgSpaceInst(window);
        }
        
        return window as T;
    }
}