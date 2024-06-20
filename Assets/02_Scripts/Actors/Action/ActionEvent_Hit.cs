using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    [System.Serializable]
    public partial class ActionEventData_Hit : ActionEventDataBase
    {
        public override bool HasEndTime => false;
        
        public string guidTargetingObject;
        public bool followActor = false;
        public Vector3 vOffsetPos = Vector3.zero;
        public Quaternion qOffsetRot = Quaternion.identity;
        public Vector3 vOffsetScale = Vector3.one;

        public float delayTime = 0f;
        public float lifeTime = ActionData.FRAME_PER_SEC;
        public float damageWeight = 1f;

        public override IActionEventRuntime CreateRuntime(Actor master)
            => new ActionEventRuntime_Hit(this);
        
        public ActionEventData_Hit() : base () {}

        public ActionEventData_Hit(ActionEventData_Hit prvEventData) : base(prvEventData)
        {
            guidTargetingObject = string.Copy(prvEventData.guidTargetingObject);
            followActor = prvEventData.followActor;
            vOffsetPos = prvEventData.vOffsetPos;
            qOffsetRot = prvEventData.qOffsetRot;
            vOffsetScale = prvEventData.vOffsetScale;
            
            delayTime = prvEventData.delayTime;
            lifeTime = prvEventData.lifeTime;
            damageWeight = prvEventData.damageWeight;
        }
    }

#if UNITY_EDITOR
    public partial class ActionEventData_Hit
    {
        public static readonly GUIContent guiContent_DamageWeight = new("DamageWeight", "현재 액션 내에서의 데미지 비중 값 입니다.");
        public static readonly GUIContent guiContent_FollowActor = new("FollowActor", "lifeTime 동안 Actor를 따라다닙니다.");
        public static readonly GUIContent guiContent_DelayTime = new("Delay", "현재 액션 내에서의 데미지 비중 값 입니다.");
        public static readonly GUIContent guiContent_LifeTime = new("Life", "현재 액션 내에서의 데미지 비중 값 입니다.");

        public override string Editor_TimelineItemDesc =>  $"{Editor_DisplayTypeName} :: {damageWeight}";
        public override string Editor_TimelineItemName => Editor_DisplayTypeName;

        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            EditorGUILayoutEx.FloatField(actionData, "DamageWeight", ref damageWeight);
        }

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayout.LabelField("TargetingObject", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayoutEx.GuidField<GameObject>(actionData, "Prefab", ref guidTargetingObject);
                EditorGUILayoutEx.Toggle(actionData, guiContent_FollowActor, ref followActor);
                EditorGUILayoutEx.Vector3Field(actionData, "Pos", ref vOffsetPos);
                EditorGUILayoutEx.QuaternionField(actionData, "Rot", ref qOffsetRot);
                EditorGUILayoutEx.Vector3Field(actionData, "Scale", ref vOffsetScale);
            }
            --EditorGUI.indentLevel;
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Option", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayoutEx.ClampedFrameField(actionData, guiContent_DelayTime, ref delayTime, ActionData.FRAME_RATE_INT, 0f, 3600f);
                EditorGUILayoutEx.ClampedFrameField(actionData, guiContent_LifeTime, ref lifeTime, ActionData.FRAME_RATE_INT, ActionData.FRAME_PER_SEC, 3600f);
                EditorGUILayoutEx.FloatField(actionData, guiContent_DamageWeight, ref damageWeight);
            }
            --EditorGUI.indentLevel;
        }

        public override IActionEditorItem Copy() => new ActionEventData_Hit(this);
    }
#endif

    public class ActionEventRuntime_Hit : ActionEventRuntimeBase<ActionEventData_Hit>
    {
        public ActionEventRuntime_Hit(ActionEventData_Hit eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            Managers.Actor.RegistTargetingObjectAddress(_eventData.guidTargetingObject);
            
            yield return base.OnAction_Init(master, endCallback);
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            base.OnAction_Start(master, fElapsedTime);

            var to = Managers.Actor.GenerateTargetingObject(
                _eventData.guidTargetingObject,
                master,
                _eventData.delayTime,
                _eventData.lifeTime,
                (victimActorTarget) =>
                {
                    // Todo
                    Debug.Log(victimActorTarget.gameObject.name);
                },
                master.ModelCtrl.transform,
                _eventData.vOffsetPos,
                _eventData.qOffsetRot,
                _eventData.vOffsetScale,
                _eventData.followActor
                );
        }

        public override void OnAction_Finalize(Actor master)
        {
            base.OnAction_Finalize(master);
            
            Managers.Actor.UnregistTargetingObjectAddress(_eventData.guidTargetingObject);
        }
    }
}
