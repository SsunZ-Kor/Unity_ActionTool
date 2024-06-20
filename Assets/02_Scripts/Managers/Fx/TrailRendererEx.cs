using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.WSA;

[RequireComponent(typeof(TrailRenderer))]
public class TrailRendererEx : MonoBehaviour
{
    private enum StateTypes
    {
        None,
        Wait,
        Live,
        Close,
    }
    
    [SerializeField] 
    private TrailRenderer _trail;
    
    [SerializeField] 
    private float _fWaitTime;
    [Tooltip("-1 is Infinity")]
    [SerializeField] 
    private float _fLifeTime;
    [SerializeField] 
    private float _fCloseTime;

    public bool IsLoop => _fLifeTime < float.Epsilon;

    private StateTypes _stateType;
    private float _fOriginTime;
    private float _fElapsedTime;
    private System.Action _updateCall;
    
    private void Awake()
    {
        _fOriginTime = _trail.time;
        _trail.enabled = false;
    }

    private void Update()
    {
        _updateCall?.Invoke();
    }

    public void Play()
    {
        _trail.Clear();
        _ChangeState(_fWaitTime <= float.Epsilon ? StateTypes.Live : StateTypes.Wait);
    }
    

    public void Stop(bool bForced)
    {
        if (bForced)
            _ChangeState(StateTypes.None);
        else
            _ChangeState(StateTypes.Close);
    }

    public bool IsAlive()
    {
        return isActiveAndEnabled && (_stateType == StateTypes.Live || _stateType == StateTypes.Close);
    }
    
    private void _ChangeState(StateTypes stateType, float fStartTime = 0f)
    {
        switch (stateType)
        {
            case StateTypes.None:
            {
                _trail.Clear();
                _trail.enabled = false;
                _updateCall = null;
            }
            break;                
            case StateTypes.Wait:
            {
                _trail.Clear();
                _trail.enabled = false;
                _updateCall = OnUpdate_Wait;
            }
            break;
            case StateTypes.Live:
            {
                _trail.time = _fOriginTime;
                _trail.enabled = true;
                _updateCall = OnUpdate_Live;
            }
            break;
            case StateTypes.Close:
            {
                _trail.enabled = true;
                _updateCall = OnUpdate_Close;
            }
            break;
        }
        
        _fElapsedTime = fStartTime;
    }

    private void OnUpdate_Wait()
    {
        _fElapsedTime += Time.deltaTime;
        var fGapTime = _fWaitTime - _fElapsedTime;
        if (fGapTime <= 0f)
            _ChangeState(StateTypes.Live, Mathf.Abs(fGapTime));
    }
    
    private void OnUpdate_Live()
    {
        _fElapsedTime += Time.deltaTime;
        var fGapTime = _fLifeTime - _fElapsedTime;
        if (fGapTime <= 0f)
            _ChangeState(StateTypes.Close, Mathf.Abs(fGapTime));
    }

    private void OnUpdate_Close()
    {
        _fElapsedTime += Time.deltaTime;
        var fGapTime = _fCloseTime - _fElapsedTime;
        if (fGapTime <= 0f)
        {
            _ChangeState(StateTypes.None);
            return;
        }

        _trail.time = 1f - _fElapsedTime / _fCloseTime;
    }
}
