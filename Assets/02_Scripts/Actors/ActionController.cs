using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Actor
{
    public partial class ActionController
    {
        public Actor Master { get; private set; }

        /* Action Info */
        public float ElapsedTime { get; private set; }
        public float FixedElapsedTime { get; private set; }

        private ActionRuntime _crrActionRuntime = null;
        public ActionData CrrActionData => _crrActionRuntime?.Data;

        /* Event Info */
        private LinkedList<IActionEventRuntime> _crrActionEventRuntimesForUpdate = new();
        private int nIdx_EventRuntime;
        

        public ActionController(Actor master)
        {
            Master = master;
        }
        
        public void OnActor_Update(float fDeltaTime)
        {
            if (_crrActionRuntime == null)
                return;

            ElapsedTime += fDeltaTime;
            
            // EventRuntime :: Start
            for (; nIdx_EventRuntime < _crrActionRuntime.listEventRuntimes.Count; ++nIdx_EventRuntime)
            {
                var eventRuntime = _crrActionRuntime.listEventRuntimes[nIdx_EventRuntime];
                if (eventRuntime.StartTime > ElapsedTime)
                    break;
                
                eventRuntime.OnAction_Start(Master, ElapsedTime);
                if (eventRuntime.HasEndTime)
                    _crrActionEventRuntimesForUpdate.AddLast(eventRuntime.Node);
            }
            
            var node = _crrActionEventRuntimesForUpdate.First;
            while (node != null)
            {
                var eventRuntime = node.Value;
                node = node.Next;
                
                // EventRuntime :: Update
                if (eventRuntime.OnAction_Update(Master, ElapsedTime - eventRuntime.StartTime, fDeltaTime))
                    return;

                // EventRuntime :: End
                if (eventRuntime.EndTime <= ElapsedTime)
                {
                    eventRuntime.OnAction_End(Master, true);
                    _crrActionEventRuntimesForUpdate.Remove(eventRuntime.Node);
                }
            }

            // Action End
            var actionLength = _crrActionRuntime.Data.Length;
            if (ElapsedTime >= actionLength)
            {
                if (_crrActionRuntime.Data.Loop)
                {
                    ElapsedTime %= actionLength;
                    FixedElapsedTime %= actionLength;
                    nIdx_EventRuntime = 0;
                    
                    Master.BrainCtrl.OnAction_Stop(CrrActionData);
                    Master.MoveCtrl.OnAction_Stop();
                }
                else
                {
                    Master.BrainCtrl.OnAction_End(_crrActionRuntime.Data);
                }
            }
        }

        public void OnActor_FixedUpdate(float fDeltaTime)
        {
            if (_crrActionRuntime == null)
                return;

            FixedElapsedTime += fDeltaTime;
            
            var node = _crrActionEventRuntimesForUpdate.First;
            while (node != null)
            {
                var eventRuntime = node.Value;
                node = node.Next;
                
                // EventRuntime :: FixedUpdate
                if (eventRuntime.OnAction_FixedUpdate(Master, FixedElapsedTime - eventRuntime.StartTime, fDeltaTime))
                    return;
            }
        }

        public void OnActor_Destroy()
        {
            StopAction();
            ClearActionData();
            ActionTargets.Clear();
            
            Master = null;
        }

        public void PlayAction(string actionName)
        {
            // 기존 Action Release
            StopAction();

            // 재생할 액션 찾기
            if (_dicActionRuntimes.TryGetValue(actionName, out var actionRuntime))
            {
                _crrActionRuntime = actionRuntime;
                Master.BrainCtrl.OnAction_Start(_crrActionRuntime.Data);
                Master.ActionCtrl.OnPlayAction_Targeting();
#if UNITY_EDITOR
                Debug.Log($"PlayAction Succeeded // {Master.gameObject.name} // {actionName}");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"PlayAction Failed // {Master.gameObject.name} // {actionName}");
#endif
            }
        }

        public void StopAction()
        {
            if (_crrActionRuntime == null)
                return;
            
            foreach (var eventRuntime in _crrActionEventRuntimesForUpdate)
                eventRuntime.OnAction_End(Master, false);
            
            _crrActionEventRuntimesForUpdate.Clear();

            Master.BrainCtrl.OnAction_Stop(CrrActionData);
            Master.MoveCtrl.OnAction_Stop();

            ElapsedTime = 0f;
            FixedElapsedTime = 0f;
            nIdx_EventRuntime = 0;
            _crrActionRuntime = null;
        }
    }
}