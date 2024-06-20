using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public partial class ActionEventData_FX : ActionEventDataBase
    {
        public override bool HasEndTime => false;
        
        public string guidFXObject;
        public bool followActor = false;
        public ModelRigTypes rigType;
        public Vector3 vOffsetPos = Vector3.zero;
        public Quaternion qOffsetRot = Quaternion.identity;
        public Vector3 vOffsetScale = Vector3.one;
        public bool ReleaseOnExitAction = false;
        
        public ActionEventData_FX() : base() {}

        public ActionEventData_FX(ActionEventData_FX prvEventData) : base(prvEventData)
        {
            guidFXObject = string.Copy(prvEventData.guidFXObject);
            followActor = prvEventData.followActor;
            rigType = prvEventData.rigType;
            vOffsetPos = prvEventData.vOffsetPos;
            qOffsetRot = prvEventData.qOffsetRot;
            vOffsetScale = prvEventData.vOffsetScale;
            ReleaseOnExitAction = prvEventData.ReleaseOnExitAction;
        }
        
        public override IActionEventRuntime CreateRuntime(Actor master)
            => new ActionEventRuntime_FX(this);
    }

    #if UNITY_EDITOR
    public partial class ActionEventData_FX
    {
        public static readonly GUIContent guiContent_FollowActor = new("FollowActor", "Actor를 따라다닙니다.");
        
        public override string Editor_TimelineItemDesc => $"{Editor_DisplayTypeName} :: {Editor_GetFXObjectPrefabName()}";
        public override string Editor_TimelineItemName => Editor_DisplayTypeName;
        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            EditorGUILayoutEx.GuidField<GameObject>(actionData, ref guidFXObject);
        }

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayout.LabelField("FXObject", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayoutEx.GuidField<GameObject>(actionData, "Prefab", ref guidFXObject);
                EditorGUILayoutEx.Toggle(actionData, guiContent_FollowActor, ref followActor);
                EditorGUILayoutEx.EnumPopup(actionData, "ModelRig", ref rigType);
                EditorGUILayoutEx.Vector3Field(actionData, "Pos", ref vOffsetPos);
                EditorGUILayoutEx.QuaternionField(actionData, "Rot", ref qOffsetRot);
                EditorGUILayoutEx.Vector3Field(actionData, "Scale", ref vOffsetScale);
            }
        }

        private string Editor_GetFXObjectPrefabName()
        {
            var path = AssetDatabase.GUIDToAssetPath(guidFXObject);
            if (string.IsNullOrWhiteSpace(path))
                return "None";

            return System.IO.Path.GetFileNameWithoutExtension(guidFXObject);
        }

        public override IActionEditorItem Copy() => new ActionEventData_FX(this);
    }
    #endif

    public class ActionEventRuntime_FX : ActionEventRuntimeBase<ActionEventData_FX>
    {
        private FxObject _fxObject;
        
        public ActionEventRuntime_FX(ActionEventData_FX eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            Managers.Fx.RegistFxAddress(_eventData.guidFXObject, 1);
            
            return base.OnAction_Init(master, endCallback);
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            _fxObject = Managers.Fx.PlayFx(
                _eventData.guidFXObject,
                master.ModelCtrl.GetRig(_eventData.rigType),
                _eventData.vOffsetPos,
                _eventData.qOffsetRot,
                _eventData.vOffsetScale,
                _eventData.followActor);
            
            base.OnAction_Start(master, fElapsedTime);
        }

        public override void OnAction_End(Actor master, bool bTimeEnd)
        {
            if (_fxObject != null && _eventData.ReleaseOnExitAction)
            {
                var delFxObject = _fxObject;
                _fxObject = null;
                delFxObject.ReturnToPool(false);
            }
            
            base.OnAction_End(master, bTimeEnd);
        }

        public override void OnAction_Finalize(Actor master)
        {
            base.OnAction_Finalize(master);

            Managers.Fx.UnregistFxAddress(_eventData.guidFXObject);
        }
    }
}