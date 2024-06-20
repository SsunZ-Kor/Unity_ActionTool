using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class FollowVCamera : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera _vCam;
    [SerializeField]
    private CinemachineTargetGroup _targetGroup;

    [SerializeField] 
    private Transform _trBody;
    [SerializeField] 
    private Transform _trSpine;
    [SerializeField] 
    private Transform _trNeck;
    [SerializeField] 
    private Transform _trHead;
    [SerializeField] 
    private Transform _trCam;

    /* Target */
    [SerializeField] 
    private Transform _followTarget;
    public Transform FollowTarget => _followTarget;
    private HashSet<Transform> _setLookTargets = new();

    /* Pos & Rot */
    [SerializeField] 
    private Vector2 _vRotFactor;
    private Vector2 _vRot;
    [SerializeField] 
    private Vector3 _vPosOffset = new(1f,1f, -5f);
    
    /* Raycast */
    private RaycastHit[] _rayResult = new RaycastHit[1];
    private int _rayLayerMask;
    
    public CinemachineVirtualCamera VCam => _vCam;

    private void Awake()
    {
        SetOffset(_vPosOffset, true);
        _rayLayerMask = LayerMask.GetMask("World_Env");
        
        _targetGroup.AddMember(null, 1f, 1f);
    }

    public void Update()
    {
        if (Managers.IsValid)
        {
            var vInput = Managers.Input.GetJoystickAxis(GameJoystickCode.Cam);
            vInput += Managers.Input.GetSwipeDelta(GameSwipeCode.Cam);
            if (vInput.sqrMagnitude > float.Epsilon)
            {
                _vRot.y += vInput.x * _vRotFactor.x;
                _trBody.transform.localRotation = Quaternion.AngleAxis(_vRot.y, Vector3.up);

                _vRot.x += vInput.y * _vRotFactor.y;
                _vRot.x = MathEx.ClampAngle180(_vRot.x);
                _vRot.x = Mathf.Clamp(_vRot.x, -80f, +80f);
                _trNeck.transform.localRotation = Quaternion.AngleAxis(_vRot.x, Vector3.right);
            }
        }

        {
            Vector3 vNewHeadPos;
            var vOrigin = _trSpine.position;
            var vDir = _trSpine.right * _vPosOffset.x + _trNeck.forward * _vPosOffset.z;
            var fDist = vDir.magnitude;
            if (fDist > float.Epsilon)
            {
                vDir *= (1f / fDist);

                if (Physics.RaycastNonAlloc(vOrigin, vDir, _rayResult, fDist, _rayLayerMask) > 0)
                    _trHead.position = _rayResult[0].point;
                else
                    _trHead.localPosition = Vector3.Lerp(_trHead.localPosition, Vector3.forward * _vPosOffset.z, Time.deltaTime * 10f);
            }
        }
    }

    public void LateUpdate()
    {
        if (_followTarget != null)
        {
            /* Position */
            _trBody.position = _followTarget.position;
        }

        if (_setLookTargets.Count > 0)
        {
            var vLookPos = _targetGroup.transform.position;
            
            /* Yaw :: Y축*/
            {
                var vDelta = vLookPos - _trBody.position;
                var vDeltaOnPlane = Vector3.ProjectOnPlane(vDelta, Vector3.up);
                _trBody.rotation = Quaternion.LookRotation(vDeltaOnPlane);
            }

            /* Pitch :: X 축 */
            {
                var vDelta = vLookPos - _trNeck.position;
                var vDeltaOnPlane = Vector3.ProjectOnPlane(vDelta, _trBody.right);

                var vForwardNeck = vDeltaOnPlane.normalized;
                vForwardNeck = Vector3.Slerp(_trNeck.forward, vForwardNeck, Time.deltaTime);
                
                /* 최대 X축(Pitch) 회전각 보정 */
                var vForwardBody = _trBody.forward;
                var fAngle = Vector3.Angle(vForwardBody, vForwardNeck);
                if (fAngle > 20f)
                    vForwardNeck = Vector3.Slerp(vForwardBody, vForwardNeck, 20f / fAngle);

                _trNeck.rotation = Quaternion.LookRotation(vForwardNeck);
                _vRot.x = MathEx.ClampAngle180(_trNeck.localRotation.eulerAngles.x);
            }
        }
        else
        {
            _trCam.rotation = Quaternion.LookRotation(Vector3.Slerp(_trCam.forward, _trHead.forward, Time.deltaTime * 10f));
        }
    }
    
    public void SetFollowTarget(Transform trFollow)
    {
        _followTarget = trFollow;
        _targetGroup.m_Targets[0].target = trFollow;
    }

    public void AddLookTarget(Transform trLook)
    {
        if (trLook == null)
            return;

        if (_setLookTargets.Add(trLook))
        {
            _targetGroup.AddMember(trLook, 1f, 1f);
            _vCam.LookAt = _targetGroup.transform;
        }
    }

    public void RemoveLookTarget(Transform trLook)
    {
        if (trLook == null)
            return;
        
        if (_setLookTargets.Remove(trLook))
            _targetGroup.AddMember(trLook, 1f, 1f);
        
        if (_setLookTargets.Count == 0)
            _vCam.LookAt = null;
    }

    public void ClearLookTarget()
    {
        foreach (var trTarget in _setLookTargets)
            _targetGroup.RemoveMember(trTarget);
    }

    public void SetOffset(Vector3 vOffset, bool bImmediately)
    {
        _vPosOffset = vOffset;
        
        if (bImmediately)
        {
            _trSpine.localPosition = new(0f, vOffset.y, 0f);
            _trNeck.localPosition = new(vOffset.x, 0f, 0f);
            _trHead.localPosition = new(0f, 0f, vOffset.z);
        }
    }
    
    public void SetOffset(float fOffsetX, float fOffsetY, float fOffsetZ, bool bImmediately)
    {
        SetOffset(new(fOffsetX, fOffsetY, fOffsetZ), bImmediately);
    }
}