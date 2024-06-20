using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using JetBrains.Annotations;
using UnityEngine;

namespace Actor
{
    public enum BrainTypes
    {
        UserInput,
        AI,
        Net,
    }

    public class BrainTargetInfo
    {

    }
    
    public class BrainController
    {
        public Actor Master { get; private set; }

        public BrainTypes CrrBrainType { get; private set; }
        private BrainBase _crrBrain = null;
        private BrainBase[] _brains;

        public ActorTarget Target { get; private set; }
        public Vector3 LookDirOnPlane { get; private set; }
        public Vector3 LookDir { get; private set; }
        public float LookPower { get; private set; }

        public BrainController(Actor master)
        {
            Master = master;

            _brains = new BrainBase[] // 반드시 BrainTypes와 매칭할 것
            {
                new Brain_UserInput(), 
                new Brain_AI(), 
                new Brain_Net()
            };
        }

        public void ChangeBrainType(BrainTypes brainType)
        {
            _crrBrain = _brains[(int)brainType];
        }
        
        public void OnActor_Update()
        {
            if (_crrBrain != null)
                _crrBrain.UpdateWorldDir(this);   
        }

        public void OnAction_Start(ActionData actionData)
        {
            if (_crrBrain != null)
                _crrBrain.OnAction_Start(this, actionData);   
        }

        public void OnAction_Stop(ActionData actionData)
        {
            if (_crrBrain != null)
                _crrBrain.OnAction_Stop(this, actionData);
        }
        
        public void OnAction_End(ActionData actionData)
        {
            if (_crrBrain != null)
                _crrBrain.OnAction_End(this, actionData);
        }

        public void SetTarget(ActorTarget target)
        {
            Target = target;
        }

        public void SetLookDir(Vector3 vLookDir, float fLookPower)
        {
            LookDir = vLookDir;
            LookDirOnPlane = Vector3.ProjectOnPlane(vLookDir, Vector3.up).normalized;
            LookPower = fLookPower;
        }
    }

    public abstract class BrainBase
    {
        public virtual void UpdateWorldDir(BrainController brainCtrl)
        {
            
        }
        
        public virtual void OnAction_Start(BrainController brainCtrl, ActionData actionData)
        {
            
        }
        
        public virtual void OnAction_Stop(BrainController brainCtrl, ActionData actionData)
        {
            
        }
        
        public virtual void OnAction_End(BrainController brainCtrl, ActionData actionData)
        {
            
        }

    }

    public class Brain_UserInput : BrainBase
    {
        TriggerJoysickTypes _joystickType = 0;

        private LinkedList<ActionTrigger_OnInput> _llistTriggers = new();
        private int _nIdx_Trigger;

        public override void UpdateWorldDir(BrainController brainCtrl)
        {
            base.UpdateWorldDir(brainCtrl);

            _UpdateTargetInfos_Look(brainCtrl);
            
            /* Input Trigger */
            var actionCtrl = brainCtrl.Master.ActionCtrl; 
            var actionData = actionCtrl.CrrActionData;
            if (actionData != null)
            {
                // 루프일 경우 시간 체크 하지 않음
                if (actionData.Loop)
                {
                    foreach (var trigger in  actionData.listTriggersOnInput)
                    {
                        if (CheckKey(trigger.keyCode) && CheckJoystick(trigger.JoysickType))
                        {
                            actionCtrl.PlayAction(trigger.nextActionName);
                            break;
                        }
                    }
                }
                else
                {
                    for (; _nIdx_Trigger < actionData.listTriggersOnInput.Count; ++_nIdx_Trigger)
                    {
                        var trigger = actionData.listTriggersOnInput[_nIdx_Trigger];
                        if (trigger.startTime > actionCtrl.ElapsedTime)
                            break;

                        _llistTriggers.AddLast(trigger.Node);
                    }

                    var node = _llistTriggers.First;
                    while (node != null)
                    {
                        var trigger = node.Value;
                        node = node.Next;

                        if (CheckKey(trigger.keyCode) && CheckJoystick(trigger.JoysickType))
                        {
                            actionCtrl.PlayAction(trigger.nextActionName);
                            break;
                        }
                    
                        if (trigger.endTime <= actionCtrl.ElapsedTime)
                            trigger.Node.RemoveSelf();
                    }
                }           
            }
        }
        
        public override void OnAction_Start(BrainController brainCtrl, ActionData actionData)
        {
            _llistTriggers.Clear();
            _nIdx_Trigger = 0;
        }

        public override void OnAction_Stop(BrainController brainCtrl, ActionData actionData)
        {
            _llistTriggers.Clear();
            _nIdx_Trigger = 0;
        }

        public override void OnAction_End(BrainController brainCtrl, ActionData actionData)
        {
            base.OnAction_End(brainCtrl, actionData);
            
            /* ActionEnd Trigger */
            if (!actionData.Loop)
            {
                foreach (var trigger in actionData.listTriggersOnEndAction)
                {
                    if ((trigger.areaType & brainCtrl.Master.MoveCtrl.CrrAreaType) != 0
                        && CheckKey(trigger.keyCode) && CheckJoystick(trigger.JoysickType))
                    {
                        brainCtrl.Master.ActionCtrl.PlayAction(trigger.nextActionName);
                        break;
                    }
                }
            }
        }

        private void _UpdateTargetInfos_Look(BrainController brainCtrl)
        {
            var camMgr = Managers.Cam;
            var inputMgr = Managers.Input;

            /* Update Look :: joysick 입력이 없다면 */
            var fLookPower = inputMgr.GetJoystickPower(GameJoystickCode.Move);
            if (fLookPower < float.Epsilon)
            {
                brainCtrl.SetLookDir(camMgr.MainCam.transform.forward, fLookPower);
                _joystickType =  TriggerJoysickTypes.Neutral;
            }
            /* Update Look :: joysick 입력이 있다면 */
            else
            {
                var vInput = inputMgr.GetJoystickDir(GameJoystickCode.Move);
                var trMainCam = camMgr.MainCam.transform;

                var vLookDir = (trMainCam.forward * vInput.y) + (trMainCam.right * vInput.x);
                brainCtrl.SetLookDir(vLookDir, fLookPower);

                // Todo :: Update Target

                /* Update :: joysickType */
                if (brainCtrl.Target != null)
                {
                    var vTargetDir = brainCtrl.Target.transform.position - brainCtrl.Master.transform.position;
                    vTargetDir = Vector3.ProjectOnPlane(vTargetDir, Vector3.up);
                    vTargetDir.Normalize();

                    var cos = Vector3.Dot(vTargetDir, vLookDir);
                    if (cos > 0.70710678118f)
                    {
                        _joystickType = TriggerJoysickTypes.Forward;
                    }
                    else if (cos < -0.70710678118f)
                    {
                        _joystickType = TriggerJoysickTypes.Backward;
                    }
                    else
                    {
                        _joystickType = Vector3.Cross(vTargetDir, vLookDir).y > 0
                            ? TriggerJoysickTypes.Right
                            : TriggerJoysickTypes.Left;
                    }
                }
                else
                {
                    if (Mathf.Abs(vInput.y) >= Mathf.Abs(vInput.x))
                    {
                        _joystickType = vInput.y >= 0f
                            ? TriggerJoysickTypes.Forward
                            : TriggerJoysickTypes.Backward;
                    }
                    else
                    {
                        _joystickType = vInput.x >= 0f
                            ? TriggerJoysickTypes.Right
                            : TriggerJoysickTypes.Left;
                    }
                }
            }
        }
        
        private bool CheckJoystick(TriggerJoysickTypes triggerJoystickType)
        {
            return triggerJoystickType == _joystickType 
                   || (triggerJoystickType & _joystickType) != 0;
        }

        private bool CheckKey(GameKeyCode triggerKeyCode)
        {
            return triggerKeyCode == GameKeyCode.Any
                   || (triggerKeyCode == GameKeyCode.None && !Managers.Input.GetAnyKey())
                   || Managers.Input.GetKey(triggerKeyCode, GameKeyState.Pressed);
        }
    }

    public class Brain_AI : BrainBase
    {
        public override void OnAction_End(BrainController brainCtrl, ActionData actionData)
        {
            base.OnAction_End(brainCtrl, actionData);
            
            /* ActionEnd Trigger */
            if (!actionData.Loop)
            {
                foreach (var trigger in actionData.listTriggersOnEndAction)
                {
                    if (trigger.areaType.HasFlag(brainCtrl.Master.MoveCtrl.CrrAreaType)
                        && trigger.keyCode == GameKeyCode.None
                        && trigger.JoysickType == 0)
                    {
                        brainCtrl.Master.ActionCtrl.PlayAction(trigger.nextActionName);
                        break;
                    }
                }   
            }
        }
    }

    public class Brain_Net : BrainBase
    {
        
    }
}