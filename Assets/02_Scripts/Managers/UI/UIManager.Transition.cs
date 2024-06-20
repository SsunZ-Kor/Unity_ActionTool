using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public enum UITransitionTypes
{
    Fade_Black,
    Fade_White,
    Fade_Snap,
    LToR_Black,
    LToR_White,
}

public enum UITransitionState
{
    None,
    Start,
    Normal,
    End,
}
    
public partial class UIManager
{
    private SortedDictionary<UITransitionTypes, string> _dicTransitionPrefabName = new()
    {
        { UITransitionTypes.Fade_Black, "UI/UITransition_Fade_Black.prefab" },
        { UITransitionTypes.Fade_White, "UI/UITransition_Fade_White.prefab" },
        { UITransitionTypes.Fade_Snap , "UI/UITransition_Fade_Snap.prefab"  },
        { UITransitionTypes.LToR_Black, "UI/UITransition_LToR_Black.prefab" },
        { UITransitionTypes.LToR_White, "UI/UITransition_LToR_White.prefab" },
    };
    
    private RectTransform _root_Transition;

    private Dictionary<UITransitionTypes, UITransitionBase> _dicTransitionInst = new();

    private UITransitionBase _crrTransition = null;

    public UITransitionState TransitionState { get; private set; }

    public void OnInit_Transition()
    {
        _root_Transition = Canvas.transform.Find("root_Transition") as RectTransform;
    }
    
    public void ShowTransition(UITransitionTypes transitionType, System.Action endCallback)
    {
        if (_crrTransition != null)
            return;
        
        _crrTransition = GetTransition(transitionType);
        if (_crrTransition == null)
            return;

        TransitionState = UITransitionState.Start;
        endCallback += () => TransitionState = UITransitionState.Normal;
        _crrTransition.PlayAnim_Open(endCallback);
    }

    public void HideTransition(System.Action endCallback)
    {
        if (_crrTransition == null)
            return;

        System.Action closeEndCallback = () =>
        {
            _crrTransition = null;
            TransitionState = UITransitionState.None;
        };
        if (endCallback != null)
            closeEndCallback += endCallback;
        
        TransitionState = UITransitionState.End;
        _crrTransition.PlayAnim_Close(closeEndCallback);
    }

    public void ClearAllTransition()
    {
        var crrTransitionType = UITransitionTypes.Fade_Black;
        foreach (var pair in _dicTransitionInst)
        {
            // 현재 진행중인 트렌지션은 제외
            if (_crrTransition == pair.Value)
            {
                crrTransitionType = pair.Key;
                continue;
            }

            Destroy(pair.Value.gameObject);
            Managers.Asset.ReleaseHandle(_dicTransitionPrefabName[pair.Key]);
        }
        
        _dicTransitionInst.Clear();
        
        // 현재 진행중인 트랜지션 다시 세팅
        if (_crrTransition != null)
            _dicTransitionInst.Add(crrTransitionType, _crrTransition);
    }

    private UITransitionBase GetTransition(UITransitionTypes transitionType)
    {
        if (_dicTransitionInst.TryGetValue(transitionType, out var transition))
            return transition;

        if (!_dicTransitionPrefabName.TryGetValue(transitionType, out var prfAddress) || prfAddress == null)
            return null;

        var prfTransition = Managers.Asset.Load<GameObject>(prfAddress);
        var goTransition = Instantiate(prfTransition, _root_Transition);

        transition = goTransition.GetComponent(typeof(UITransitionBase)) as UITransitionBase;
        if (transition == null)
        {
            Destroy(goTransition);
            Managers.Asset.ReleaseHandle(prfAddress);
            return null;
        }
        
        _dicTransitionInst.Add(transitionType, transition);
        return transition;
    }
}