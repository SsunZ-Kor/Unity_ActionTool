using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonEx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private static ButtonEx _crrButton;
    private static ButtonEx _lastHoveredButton;

    public class ButtonClickedEvent : UnityEvent<ButtonStateTypes>
    {
        
    }
    
    public enum ButtonStateTypes
    {
        Normal,
        Disable,
        Lock,
        Highlight,
    }

    public enum PointerStateTypes
    {
        Normal,
        Hover,
        Pressed,
        Exited,
    }

    public enum StateAnimTypes
    {
        Scale,
        Anim
    }

    [System.Serializable]
    private class PointerStateItems
    {
        [SerializeField]
        public GameObject[] _rootItem = new GameObject[4];
    }

    [SerializeField][HideInInspector] 
    public Animation _anim;
    
    [SerializeField][HideInInspector]
    private PointerStateItems[] _pointerStateItems = new PointerStateItems[4];

    [SerializeField] [HideInInspector] 
    private Vector2[] _pointerStateScales =
        { new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f) };
   
    [SerializeField][HideInInspector] 
    private AnimationClip[] _pointerStateAnims = new AnimationClip[4];

    [SerializeField] [HideInInspector] 
    private StateAnimTypes _pointerStateAnimType = StateAnimTypes.Scale;
    
    public ButtonStateTypes CrrButtonState { get; private set; } = ButtonStateTypes.Normal;
    public PointerStateTypes CrrPointerState { get; private set; } = PointerStateTypes.Normal;

    public ButtonClickedEvent onClick = new();
    public Button.ButtonClickedEvent onClick_NormalOnly = new();
    
    private int _touchID = -2;
    private bool _isEntered = false;
    private Coroutine _cor_ScaleAnim = null;

    private void Awake()
    {
        if (_pointerStateAnimType == StateAnimTypes.Anim)
        {
            _anim = this.GetOrAddComponent<Animation>();
            foreach (var animClip in _pointerStateAnims)
                _anim.SafeAddAnimClip(animClip);
        }
        
        /* Clear All State Items */
        foreach (var itemStateInfo in _pointerStateItems)
        {
            foreach (var rootItem in itemStateInfo._rootItem)
            {
                if (rootItem != null)
                    rootItem.SetActive(false);
            }
        }

        /* Set First State */
        var rootItem_Normal = GetStateItemRoot(CrrButtonState, CrrPointerState);
        if (rootItem_Normal != null)
            rootItem_Normal.SetActive(true);
    }

    private void OnEnable()
    {
        /* Set State Item :: Normal */
        SetPointerState(PointerStateTypes.Normal, false);
        var rootItem_Crr = GetStateItemRoot(CrrButtonState, CrrPointerState);
        if (rootItem_Crr != null)
        {
            switch (_pointerStateAnimType)
            {
                case StateAnimTypes.Scale:
                {
                    var vDst = _pointerStateScales[(int)CrrPointerState];
                    rootItem_Crr.transform.localScale = vDst;
                }
                break;
                case StateAnimTypes.Anim:
                {
                    var clip = _pointerStateAnims[(int)CrrPointerState];
                    if (clip != null)
                        _anim.SampleAnimClip(clip.name, 1f);
                }
                break;
            }   
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        
        _touchID = -2;
        _isEntered = false;
        
        if (_crrButton == this)
            _crrButton = null;

        if (_lastHoveredButton == this)
            _lastHoveredButton = null;
    }

    public void SetButtonState(ButtonStateTypes buttonStateType)
    {
        if (CrrButtonState == buttonStateType)
            return;
        
        var rootItem_Prv = GetStateItemRoot(CrrButtonState, CrrPointerState);
        if (rootItem_Prv != null)
            rootItem_Prv.SetActive(false);
        
        CrrButtonState = buttonStateType;
        
        var rootItem_Crr = GetStateItemRoot(CrrButtonState, CrrPointerState);
        if (rootItem_Crr != null)
            rootItem_Crr.SetActive(true);
    }

    public void SetPointerState(PointerStateTypes pointerStateType, bool bWithAnim = true)
    {
        if (CrrPointerState == pointerStateType)
            return;
        
        var rootItem_Prv = GetStateItemRoot(CrrButtonState, CrrPointerState);
        if (rootItem_Prv != null)
            rootItem_Prv.SetActive(false);

        var prvPointerState = CrrPointerState;
        CrrPointerState = pointerStateType;
        
        var rootItem_Crr = GetStateItemRoot(CrrButtonState, CrrPointerState);
        if (rootItem_Crr != null)
            rootItem_Crr.SetActive(true);

        if (bWithAnim)
        {
            switch (_pointerStateAnimType)
            {
                case StateAnimTypes.Scale:
                {
                    if (rootItem_Crr != null)
                    {
                        var tr_RootTime = rootItem_Crr.transform;
                        if (rootItem_Prv != null)
                            tr_RootTime.localScale = rootItem_Prv.transform.localScale;
                    
                        if (_cor_ScaleAnim != null)
                            StopCoroutine(_cor_ScaleAnim);
                    
                        _cor_ScaleAnim = StartCoroutine(_Cor_ScaleAnim());
                        IEnumerator _Cor_ScaleAnim()
                        {
                            var fAnimStartTime = Time.realtimeSinceStartup;
                            var vSrc = _pointerStateScales[(int)prvPointerState];
                            var vDst = _pointerStateScales[(int)CrrPointerState];

                            var fSrcScala = vSrc.magnitude;
                            var fDstScala = vDst.magnitude;
                        
                            var fScaleGap = fDstScala - fSrcScala;
                            var fDuration = 0.2f;
                            fAnimStartTime -= Mathf.Abs(fScaleGap) <= float.Epsilon
                                ? fDuration
                                : (tr_RootTime.localScale.magnitude - fSrcScala) / fScaleGap * fDuration;

                            while (true)
                            {
                                yield return null;

                                var fElaspedTime = Time.realtimeSinceStartup - fAnimStartTime;
                                var fProgress = fElaspedTime / fDuration;
                                if (fProgress < 1f)
                                {
                                    tr_RootTime.localScale = Vector2.Lerp(vSrc, vDst, fProgress);
                                    continue;
                                }
                            
                                tr_RootTime.localScale = vDst;
                                break;
                            }
                        }
                    }
                }
                break;
                case StateAnimTypes.Anim:
                {
                    var clip = _pointerStateAnims[(int)CrrPointerState];
                    if (clip != null)
                        _anim.CrossFade(clip.name, 0.1f);
                }
                break;
            }   
        }
    }

    private GameObject GetStateItemRoot(ButtonStateTypes btnState, PointerStateTypes stateItem)
    {
        return  _pointerStateItems[(int)CrrButtonState]._rootItem[(int)CrrPointerState];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_crrButton != null || _touchID != -2)
            return;
        
        _touchID = eventData.pointerId;
        _isEntered = true;
        _crrButton = this;

        SetPointerState(PointerStateTypes.Pressed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if ( _crrButton != this || _touchID != eventData.pointerId)
            return;

        if (_isEntered)
        {
            onClick.Invoke(CrrButtonState);
            if (CrrButtonState == ButtonStateTypes.Normal)
                onClick_NormalOnly.Invoke();
        }
        
        _touchID = -2;
        _crrButton = null;
        
        SetPointerState(_isEntered ? PointerStateTypes.Hover : PointerStateTypes.Normal);

        if (_lastHoveredButton != null)
            _lastHoveredButton.SetPointerState(PointerStateTypes.Hover);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isEntered = true;
        _lastHoveredButton = this;

        if (_crrButton == null && _touchID == -2)
            SetPointerState(PointerStateTypes.Hover);
        else if (_touchID == eventData.pointerId)
            SetPointerState(PointerStateTypes.Pressed);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isEntered = false;
        if (_lastHoveredButton == this)
            _lastHoveredButton = null;
        
        if (_crrButton == null && _touchID == -2)
            SetPointerState(PointerStateTypes.Normal);
        else if (_touchID == eventData.pointerId)
            SetPointerState(PointerStateTypes.Exited);
    }
}
