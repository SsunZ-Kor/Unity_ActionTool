using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class MoveController
    {
        public Actor Master { get; private set; }
        public CharacterController charCtrl { get; private set; }
        
        /* Action Trigger On Area */
        private LinkedList<ActionTrigger_OnArea> _llistTriggers = new();
        private int _nIdx_Trigger;

        /* 이번 프레임에서 처리할 MoveDelta */
        public Vector3 vMoveDeltaOnGround { get; private set; } = Vector3.zero;
        public Vector3 vMoveDeltaOnAir    { get; private set; } = Vector3.zero;
        public Vector3 vMoveDeltaVel      { get; private set; } = Vector3.zero;
        public Vector3 vTotalMoveDelta    { get; private set; } = Vector3.zero;


        /* Fake Physic Velocity */
        private bool _hasVel = false;
        private Vector3 _vVel = Vector3.zero;
        private float _fDragOnGround;
        
        /* Fake Physic Gravity*/
        private int _nLockGravityCount = 0;
        
        /* 현재 지형 정보 */
        public TriggerAreaTypes CrrAreaType { get; private set; } = 0;
        private Vector3 _vGroundNormal = Vector3.up;
        private float _fAngleToGroundNormal = 0f;
        private float _fDistToGround = 0f;
        private Action _updateMoveCall = null;

        public MoveController(Actor master, CharacterController charCtrl)
        {
            Master = master;
            this.charCtrl = charCtrl;
        }
        
        public void AddMoveDelta(Vector3 vMoveDelta, bool isGround)
        {
            if (isGround)
                vMoveDeltaOnGround += vMoveDelta;
            else
                vMoveDeltaOnAir += vMoveDelta;
        }

        public void AddVelocity(Vector3 vVel, float fDragOnGround)
        {
            _vVel += vVel;
            _hasVel = vVel.sqrMagnitude > float.Epsilon;
            _fDragOnGround = fDragOnGround;
        }

        public void SetVelocity(Vector3 vVel, float fDragOnGround)
        {
            _vVel = vVel;
            _hasVel = vVel.sqrMagnitude > float.Epsilon;
            _fDragOnGround = fDragOnGround;
        }

        public void SetLockGravity(bool bLock)
        {
            _nLockGravityCount += bLock ? 1 : -1;
        }

        public void OnActor_OnControllerColliderHit(ControllerColliderHit hit)
        {
        }

        private void ClearGroundInfo()
        {
            _vGroundNormal = Vector3.down;
            _fAngleToGroundNormal = 180f;
            _fDistToGround = float.PositiveInfinity;         

            if (CrrAreaType != TriggerAreaTypes.Air)
            {
                _updateMoveCall = _OnActor_Update_OnAir;
                CrrAreaType = TriggerAreaTypes.Air;
                OnChangeArea(TriggerAreaTypes.Air);
            }
        }

        public bool RefreshGroundInfo()
        {
            if((vMoveDeltaOnAir.y + _vVel.y) > float.Epsilon
               || !Physics.SphereCast(
                Master.transform.position + charCtrl.center,
                charCtrl.radius,
                -Master.transform.up,
                out var hitInfo,
                float.PositiveInfinity,
                LayerMask.GetMask("World_Env")))
            {
                ClearGroundInfo();
                return false;
            }

            _vGroundNormal = hitInfo.normal;
            _fAngleToGroundNormal = Vector3.Angle(Vector3.up, hitInfo.normal);
            _fDistToGround = hitInfo.distance - (charCtrl.height * 0.5f) + charCtrl.radius;

            if (_fDistToGround > charCtrl.stepOffset)
            {
                ClearGroundInfo();
                return false;
            }
            else
            {
                if (_fAngleToGroundNormal <= charCtrl.slopeLimit)
                {
                    if (CrrAreaType != TriggerAreaTypes.Ground)
                    {
                        _updateMoveCall = _OnActor_Update_OnGround;
                        CrrAreaType = TriggerAreaTypes.Ground;
                        _vVel.y = 0f;
                        _hasVel = _vVel.sqrMagnitude > float.Epsilon;
                        OnChangeArea(TriggerAreaTypes.Ground);
                    }
                }
                else
                {
                    if (CrrAreaType != TriggerAreaTypes.Slope)
                    {
                        _updateMoveCall = _OnActor_Update_OnSlope;
                        CrrAreaType = TriggerAreaTypes.Slope;
                        OnChangeArea(TriggerAreaTypes.Slope);
                    }
                }
            }
            
            return true;
        }
        
        public void OnActor_Update(float fDeltaTime)
        {
            if (CrrAreaType != TriggerAreaTypes.Air && (vMoveDeltaOnAir.y + _vVel.y) > float.Epsilon)
                ClearGroundInfo();
            
            _updateMoveCall?.Invoke();
            
            vMoveDeltaOnGround = Vector3.zero;
            vMoveDeltaOnAir = Vector3.zero;
            vMoveDeltaVel = Vector3.zero;
        }

        private void _OnActor_Update_OnGround()
        { 
            // 지면 이동
            var fDist = vMoveDeltaOnGround.magnitude;
            if (fDist > float.Epsilon)
            {
                var vDir = vMoveDeltaOnGround * (1f / fDist);

                vMoveDeltaOnGround = Vector3.Cross(Vector3.Cross(_vGroundNormal, vDir), _vGroundNormal) * fDist;
                charCtrl.Move(vMoveDeltaOnGround);

                RefreshGroundInfo();
                if (CrrAreaType == TriggerAreaTypes.Ground)
                    charCtrl.Move(Vector3.down * _fDistToGround);
                        
                vTotalMoveDelta = vMoveDeltaOnAir + vMoveDeltaVel;
                charCtrl.Move(vTotalMoveDelta);
                RefreshGroundInfo();

                vTotalMoveDelta += vMoveDeltaOnGround;
            }
            else
            {
                vTotalMoveDelta = vMoveDeltaOnAir + vMoveDeltaVel;
                charCtrl.Move(vTotalMoveDelta);
                RefreshGroundInfo();
            }
        }
        
        private void _OnActor_Update_OnSlope()
        {
            // 공중 이동
            vTotalMoveDelta = vMoveDeltaOnGround + vMoveDeltaOnAir + vMoveDeltaVel;
            charCtrl.Move(vTotalMoveDelta);
            RefreshGroundInfo();
        }
        
        private void _OnActor_Update_OnAir()
        {
            // 공중 이동
            vTotalMoveDelta = vMoveDeltaOnGround + vMoveDeltaOnAir + vMoveDeltaVel;
            charCtrl.Move(vTotalMoveDelta);
            RefreshGroundInfo();
        }

        public void OnActor_FixedUpdate(float fDeltaTime)
        {
            /* Gacvity */
            if (_nLockGravityCount == 0)
            {
                switch (CrrAreaType)
                {
                    case TriggerAreaTypes.Slope:
                    {
                        var vVelSlope = new Vector3(0f, -9.8f * fDeltaTime, 0f);
                        vVelSlope = Vector3.ProjectOnPlane(vVelSlope, _vGroundNormal);
                        _vVel += vVelSlope;
                        _hasVel = true;
                    } break;
                    case TriggerAreaTypes.Air:
                    {
                        if (_fAngleToGroundNormal < 90f)
                        {
                            goto case TriggerAreaTypes.Slope;
                        }
                        else
                        {
                            _vVel.y -= 9.8f * fDeltaTime;
                            _hasVel = true;
                        }            
                    } break;
                }
            }

            /* Face Velocity */
            if (_hasVel)
            {
                vMoveDeltaVel = _vVel * fDeltaTime;
                if (CrrAreaType == TriggerAreaTypes.Ground)
                {
                    if (_fDragOnGround <= float.Epsilon)
                    {
                        _hasVel = false;
                        _vVel = Vector3.zero;
                    }
                    else
                    {
                        var fScala = _vVel.magnitude;
                        var vVelDir = _vVel * (1f / fScala);
                        fScala -= _fDragOnGround * fDeltaTime;
                        if (fScala <= float.Epsilon)
                        {
                            _hasVel = false;
                            _vVel = Vector3.zero;
                        }
                        else
                        {
                            _vVel = vVelDir * fScala;
                        }
                    }
                }
            }
        }

        public void OnAction_Stop()
        {
            _llistTriggers.Clear();
            _nIdx_Trigger = 0;
        }

        public void OnChangeArea(TriggerAreaTypes areaType)
        {
            Debug.Log(areaType);
            
            /* Trigger_OnArea 처리 */
            var actionData = Master.ActionCtrl.CrrActionData;
            if (actionData != null)
            {
                var fElapsedTime = Master.ActionCtrl.ElapsedTime;

                var listTriggersOnArea = actionData.listTriggersOnArea;
                for (; _nIdx_Trigger < listTriggersOnArea.Count; ++_nIdx_Trigger)
                {
                    var triggerData = listTriggersOnArea[_nIdx_Trigger];
                    if (triggerData.startTime > fElapsedTime)
                        break;

                    _llistTriggers.AddLast(triggerData.Node);
                }

                var node = _llistTriggers.First;
                while (node != null)
                {
                    var trigger = node.Value;
                    node = node.Next;

                    if (trigger.areaType.HasFlag(areaType))
                    {
                        Master.ActionCtrl.PlayAction(trigger.nextActionName);
                        break;
                    }

                    if (trigger.endTime <= fElapsedTime)
                        _llistTriggers.Remove(trigger.Node);
                }
            }
        }
    }   
}