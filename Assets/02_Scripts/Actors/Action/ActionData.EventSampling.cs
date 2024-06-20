#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Actor
{
    public partial class ActionData
    {

        public void Editor_SampleActionEvent(Actor actor, float fTime, HashSet<IActionEditorItem> selectedEventData)
        {
            if (Application.isPlaying)
                return;
            
            _Editor_SampleActionEvent_Animation(actor, fTime);
            _Editor_SampleActionEvent_Transform(actor, fTime);
            _Editor_SampleActionEvent_Hit(actor, fTime, selectedEventData);
        }

        private void _Editor_SampleActionEvent_Animation(Actor actor, float fTime)
        {
            var anim = actor.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                var animCtrl = anim.runtimeAnimatorController as AnimatorOverrideController;
                if (animCtrl != null)
                {
                    // Override 목록 구성
                    var animOverrides =
                        new List<KeyValuePair<AnimationClip, AnimationClip>>(animCtrl.overridesCount);
                    animCtrl.GetOverrides(animOverrides);
                    var dicAnimOverrides = new Dictionary<AnimationClip, AnimationClip>();
                    foreach (var pair in animOverrides)
                        dicAnimOverrides.Add(pair.Key, pair.Value);

                    void Sample_PlayAnim(AnimLayerTypes layerType)
                    {
                        // 마지막 EventData_PlayAnim 찾기
                        var eventData = _listAnimation.FindLast((x) =>
                            x.layerType == layerType
                            && x.startTime <= fTime);

                        // 아예 없다면
                        if (eventData == null || string.IsNullOrWhiteSpace(eventData.animStateName))
                        {
                            anim.Play("Default", (int)layerType, 0f);
                            return;
                        }

                        // 스테이트 검색
                        var animCtrlBase = animCtrl.runtimeAnimatorController as AnimatorController;
                        var state = animCtrlBase.FindState(eventData.animStateName);
                        if (state != null)
                        {
                            // 스테이트의 Motion Length를 찾고 정규화된 시간으로 변경
                            var fMotionLength = state.motion.GetMotionMinLength(dicAnimOverrides);
                            var fNormalizedTime = fMotionLength > float.Epsilon
                                ? (fTime - eventData.startTime) / fMotionLength
                                : 0f;

                            // Play
                            anim.Play(eventData.animStateName, (int)layerType, fNormalizedTime * eventData.speed);
                        }
                    }

                    Sample_PlayAnim(AnimLayerTypes.Base);
                    Sample_PlayAnim(AnimLayerTypes.Face);
                    Sample_PlayAnim(AnimLayerTypes.UpperBody);
                    anim.Update(0);
                }
            }
        }

        
        private static List<ActionEventDataBase> _listMoveEventForSample = new();
        private void _Editor_SampleActionEvent_Transform(Actor actor, float fTime)
        {
            actor.transform.position = _Editor_GetSamplePosition(fTime);
        }

        
        public class TargetingObjectPoolForSampling
        {
            public static Dictionary<string, TargetingObjectPoolForSampling> s_dicPools;
            
            public string guid;
            public GameObject prefab;
            public List<TargetingObject> listInst = new();
            public int usedCount;

            private TargetingObject _Pop()
            {
                if (listInst.Count <= usedCount)
                {
                    if (prefab == null)
                    {
                        if (string.IsNullOrWhiteSpace(guid))
                            return null;
                    
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        if (string.IsNullOrWhiteSpace(path))
                            return null;
                    
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    }
                    
                    var goTargetingObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    listInst.Add(goTargetingObject.GetComponent<TargetingObject>());
                }

                var result = listInst[usedCount++];
                result.gameObject.SetActive(true);
                return result;
            }

            private void _Clear()
            {
                for (int i = listInst.Count - 1; i >= 0; --i)
                {
                    var targetingObject = listInst[i];
                    if (targetingObject == null)
                    {
                        listInst.RemoveAt(i);
                        continue;
                    }

                    targetingObject.transform.parent = null;
                    targetingObject.gameObject.SetActive(false);
                    targetingObject.OnEditor_SetGizmoColor(TargetingObject.s_gizmoColor);
                }

                usedCount = 0;
            }

            public static void Init()
            {
                if (s_dicPools == null)
                {
                    s_dicPools = new();

                    // 기존 인스턴스 재활용
                    var targetingObjects = FindObjectsByType<TargetingObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (var targetingObject in targetingObjects)
                    {
                        var prfTargetingObject = PrefabUtility.GetCorrespondingObjectFromOriginalSource(targetingObject.gameObject);

                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prfTargetingObject, out var guid, out var localId);
                        
                        var pool = s_dicPools.GetOrCreate(guid);
                        pool.guid = guid;
                        pool.listInst.Add(targetingObject);
                    }
                }
                
                // 릴리즈
                foreach (var pool in s_dicPools.Values)
                    pool._Clear();
            }

            public static TargetingObject Pop(string guid)
            {
                var pool = s_dicPools.GetOrCreate(guid);
                pool.guid = guid;
                return pool._Pop();
            }
        }
        
        private void _Editor_SampleActionEvent_Hit(Actor actor, float fTime, HashSet<IActionEditorItem> selectedEventData)
        {
            if (actor == null)
                return;
            
            /* TargetingObject 초기화 */
            TargetingObjectPoolForSampling.Init();

            /* TargetingObjectRoot 활성화 */
            {
                foreach (var eventData in _listHit)
                {
                    if (string.IsNullOrWhiteSpace(eventData.guidTargetingObject))
                        continue;

                    Color gizmoColor;

                    if (selectedEventData != null && selectedEventData.Contains(eventData))
                        gizmoColor = TargetingObject.s_gizmoColor_SelectedEventItem;
                    else if (eventData.startTime + eventData.delayTime + eventData.lifeTime <= fTime)
                        continue;
                    else if (eventData.startTime + eventData.delayTime <= fTime)
                        gizmoColor = TargetingObject.s_gizmoColor_EventItem;
                    else if (eventData.startTime <= fTime)
                        gizmoColor = TargetingObject.s_gizmoColor_DelayedEventItem;
                    else
                        continue;

                    var targetingObject = TargetingObjectPoolForSampling.Pop(eventData.guidTargetingObject);
                    if (targetingObject == null)
                        continue;
                    
                    targetingObject.OnEditor_SetGizmoColor(gizmoColor);

                    if (!eventData.followActor)
                    {
                        targetingObject.transform.position = _Editor_GetSamplePosition(fTime);
                    }
                    else
                    {
                        var transform = targetingObject.transform;
                        transform.parent = actor.ModelCtrl.transform;
                        transform.SetLocalTRS(eventData.vOffsetPos, eventData.qOffsetRot, eventData.vOffsetScale);
                    }
                }
            }
        }
        
        private Vector3 _Editor_GetSamplePosition(float fTime)
        {
            
            var fFixedDeltaTime = Time.fixedDeltaTime;
            
            _listMoveEventForSample.Clear();
            _listMoveEventForSample.AddRange(_listMovePos_ToForward);
            _listMoveEventForSample.AddRange(_listMovePos_ToInput);
            _listMoveEventForSample.AddRange(_listMovePos_ToBrainTarget);
            _listMoveEventForSample.AddRange(_listMoveVel_ToForward);
            _listMoveEventForSample.AddRange(_listMoveVel_ToInput);
            _listMoveEventForSample.AddRange(_listMoveVel_ToBrainTarget);
            _listMoveEventForSample.AddRange(_listIgnoreGravitie);

            LinkedList<ActionEventDataBase> llistUpdateMoveEvent = new();

            var vTotalPos = Vector3.zero;
            var vTotalVel = Vector3.zero;
            var nLockGravityCount = 0;
            var fElapsedTime = 0f;

            var nIdx_MoveEvent = 0;
            while (fElapsedTime <= fTime)
            {
                // 시작된 이벤트 수집
                for (; nIdx_MoveEvent < _listMoveEventForSample.Count; ++nIdx_MoveEvent)
                {
                    var moveEvent = _listMoveEventForSample[nIdx_MoveEvent];
                    if (moveEvent.startTime > fElapsedTime)
                        break;

                    llistUpdateMoveEvent.AddLast(moveEvent);
                }

                // 수집된 이벤트 재생
                var node = llistUpdateMoveEvent.First;
                while (node != null)
                {
                    var crrNode = node;
                    node = node.Next;

                    var moveEvent = crrNode.Value;
                    var fEventElapsedTime = fElapsedTime - moveEvent.startTime;

                    switch (moveEvent)
                    {
                        case ActionEventData_MovePosBase movePos:
                        {
                            vTotalPos += movePos.GetPos(fEventElapsedTime) -
                                         movePos.GetPos(fEventElapsedTime - fFixedDeltaTime);
                        }
                            break;
                        case ActionEventData_MoveVelBase moveVel:
                        {
                            if (moveVel.bResetPrvVelocity)
                                vTotalVel = moveVel.vVelocity;
                            else
                                vTotalVel += moveVel.vVelocity;
                        }
                            break;
                        case ActionEventData_IgnoreGravity ignoreGravity:
                        {
                            ++nLockGravityCount;
                        }
                            break;
                    }

                    // 단발성 이거나, EndTime이 지났다면 수집 목록에서 제외
                    if (!moveEvent.HasEndTime && moveEvent.endTime <= fElapsedTime)
                    {
                        switch (moveEvent)
                        {
                            case ActionEventData_IgnoreGravity ignoreGravity:
                                --nLockGravityCount;
                                break;
                        }

                        crrNode.RemoveSelf();
                    }
                }


                // 포지션이 0보다 작으면 Vel.y 초기화
                if (vTotalPos.y > float.Epsilon)
                {
                    if (nLockGravityCount == 0)
                        vTotalVel.y -= 9.8f * fFixedDeltaTime;
                }
                else
                {
                    vTotalVel.y = 0f;
                }

                // Vel 업데이트 누적
                vTotalPos += vTotalVel * fFixedDeltaTime;

                if (fElapsedTime < fTime)
                {
                    fElapsedTime += fFixedDeltaTime;
                    if (fElapsedTime > fTime)
                    {
                        fFixedDeltaTime -= fElapsedTime - fTime;
                        fElapsedTime = fTime;
                    }
                }
                else
                {
                    break;
                }
            }

            return vTotalPos;
        }
    }
}
#endif
