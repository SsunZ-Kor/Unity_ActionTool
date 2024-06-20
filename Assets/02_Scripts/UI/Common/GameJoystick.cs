using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private bool _bStaticStick = true;
    [SerializeField] private bool _bOnlyTouchStickBounds = true;
    [SerializeField] private bool _bOnlyDragableInTouchBounds = true;

    [SerializeField] private RectTransform _rttr_TouchBounds = null;
    [SerializeField] private RectTransform _rttr_StickBounds = null;
    [SerializeField] private RectTransform _rttr_Stick = null;

    private int _touchId = -2;
    private Canvas _myCanvas;

    private Vector3 _stickOriginPos = Vector3.zero;

    /// <summary>
    /// Get Joystick Vector ( Dir * Power )
    /// </summary>
    public Vector2 Asix { get; private set; }

    /// <summary>
    /// Get Joystick Dir
    /// </summary>
    public Vector2 Dir { get; private set; }

    /// <summary>
    /// Get Joystick Magnitude
    /// </summary>
    public float Power { get; private set; }

    public bool IsPressed { get; private set; }
    public bool IsDown { get; private set; }
    public bool IsUp { get; private set; }

    public void Awake()
    {
        _stickOriginPos = _rttr_Stick.anchoredPosition;
    }

    public void OnEnable()
    {
        this._touchId = -2;
        _rttr_Stick.anchoredPosition = _stickOriginPos;
        _rttr_StickBounds.anchoredPosition = _stickOriginPos;

        // 캔버스 찾아오기
        this._myCanvas = this.GetComponentInParent(typeof(Canvas)) as Canvas;
    }

    public void OnDisable()
    {
        this._touchId = -2;
        this.Asix = Vector3.zero;
        this.Dir = Vector3.zero;
        this.Power = 0f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 이미 터치 되어있는지 체크
        if (this._touchId != -2)
            return;

        // 터치 포지션 -> 터치 바운드의 로컬 포지션
        Vector2 localTouchPos;
        switch (this._myCanvas.renderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
            {
                localTouchPos = this._rttr_TouchBounds.InverseTransformPoint(eventData.position);
            }
                break;
            default:
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    this._rttr_TouchBounds, eventData.position, this._myCanvas.worldCamera, out localTouchPos);
            }
                break;
        }

        // 스틱의 터치만 허용한다면
        if (this._bOnlyTouchStickBounds)
        {
            // 스틱 바운드와의 거리체크
            Vector3 stickLocalPos = this._rttr_StickBounds.localPosition;
            Vector2 vStickBoundToTouchPos = localTouchPos - new Vector2(stickLocalPos.x, stickLocalPos.y);
            if (vStickBoundToTouchPos.magnitude >= this._rttr_StickBounds.rect.width * 0.5f)
                return;
        }
        else
        {
            this._rttr_StickBounds.localPosition = localTouchPos;
            this._rttr_Stick.localPosition = localTouchPos;
        }

        // 터치 아이디 저장
        this._touchId = eventData.pointerId;
        IsPressed = true;
        IsDown = true;
        IsUp = false;

        if (this._bOnlyTouchStickBounds)
            OnDrag(eventData);

        StartCoroutine(Cor_ReleaseDown());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (this._touchId == -2)
            return;

        // 터치 아이디 해제
        this._touchId = -2;

        // 이미지 원위치
        this._rttr_StickBounds.anchoredPosition = _stickOriginPos;
        this._rttr_Stick.anchoredPosition = _stickOriginPos;

        // 스틱 값 초기화
        this.Asix = Vector3.zero;
        this.Dir = Vector3.zero;
        this.Power = 0f;

        IsPressed = false;
        IsDown = false;
        IsUp = true;

        StartCoroutine(Cor_ReleaseUp());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this._touchId == -2 || this._touchId != eventData.pointerId)
            return;

        // 터치 포지션 -> 터치 바운드의 로컬 포지션
        Vector2 localTouchPos;
        switch (this._myCanvas.renderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
            {
                localTouchPos = this._rttr_TouchBounds.InverseTransformPoint(eventData.position);
            }
                break;
            default:
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    this._rttr_TouchBounds, eventData.position, this._myCanvas.worldCamera, out localTouchPos);
            }
                break;
        }

        // 터치 바운드 안에서만 가능할 경우 터치 포지션을 조정한다.
        if (_bOnlyDragableInTouchBounds)
        {
            var rect_TouchBounds = this._rttr_TouchBounds.rect;
            if (localTouchPos.x < rect_TouchBounds.xMin)
                localTouchPos.x = rect_TouchBounds.xMin;
            else if (localTouchPos.x > rect_TouchBounds.xMax)
                localTouchPos.x = rect_TouchBounds.xMax;
            if (localTouchPos.y < rect_TouchBounds.yMin)
                localTouchPos.y = rect_TouchBounds.yMin;
            else if (localTouchPos.y > rect_TouchBounds.yMax)
                localTouchPos.y = rect_TouchBounds.yMax;
        }

        Vector3 stickLocalPos = this._rttr_StickBounds.localPosition;
        Vector2 stickLocalPosV2 = new Vector2(stickLocalPos.x, stickLocalPos.y);
        Vector2 vStickBoundToTouchPos = localTouchPos - stickLocalPosV2;
        float fStickRadius = this._rttr_StickBounds.rect.width * 0.5f;

        // 스틱의 방향과 거리
        float fStickDistance = vStickBoundToTouchPos.magnitude;
        Vector2 vStickDir = vStickBoundToTouchPos * (1f / fStickDistance);

        // 정적 스틱일 경우
        if (this._bStaticStick)
        {
            // 바운드를 벗어났을 경우 바운드에서 가장 가까운 포지션으로 다시 터치 포지션을 세팅
            if (fStickDistance >= fStickRadius)
            {
                localTouchPos = stickLocalPosV2 + (vStickDir * fStickRadius);
                this.Power = 1f;
            }
            else
            {
                this.Power = fStickDistance / fStickRadius;
            }
        }
        // 동적 스틱일 경우
        else
        {
            // 바운드를 벗어났을 경우 바운드를 옮겨준다.
            if (fStickDistance >= fStickRadius)
            {
                this._rttr_StickBounds.localPosition = localTouchPos - (vStickDir * fStickRadius);
                this.Power = 1f;
            }
            else
            {
                this.Power = fStickDistance / fStickRadius;
            }
        }

        // Dir 세팅, Asix 계산
        this.Dir = vStickDir;
        this.Asix = this.Dir * this.Power;

        // 스틱의 위치를 옮겨준다.
        this._rttr_Stick.localPosition = localTouchPos;
    }

    IEnumerator Cor_ReleaseDown()
    {
        yield return new WaitForEndOfFrame();

        IsDown = false;
    }

    IEnumerator Cor_ReleaseUp()
    {
        yield return new WaitForEndOfFrame();

        IsUp = false;
    }
}
