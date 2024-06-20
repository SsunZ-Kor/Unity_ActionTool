using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public partial class ActionEventData_SFX : ActionEventDataBase
    {
        public override bool HasEndTime => isLoop;
        
        public string guidAudioClip;
        public bool isLoop = false;
        public bool followActor = false;
        public ModelRigTypes rigType;
        public Vector3 vOffsetPos = Vector3.zero;
        public bool ReleaseOnExitAction = false;
        
        public ActionEventData_SFX() : base() {}

        public ActionEventData_SFX(ActionEventData_SFX prvEventData) : base(prvEventData)
        {
            guidAudioClip = string.Copy(prvEventData.guidAudioClip);
            isLoop = prvEventData.isLoop;
            followActor = prvEventData.followActor;
            rigType = prvEventData.rigType;
            vOffsetPos = prvEventData.vOffsetPos;
            ReleaseOnExitAction = prvEventData.ReleaseOnExitAction;
        }
        
        public override IActionEventRuntime CreateRuntime(Actor master)
            => new ActionEventRuntime_SFX(this);
    }

    #if UNITY_EDITOR
    public partial class ActionEventData_SFX
    {
        public static readonly GUIContent guiContent_FollowActor = new("FollowActor", "Actor를 따라다닙니다.");
        
        public override string Editor_TimelineItemDesc => $"{Editor_DisplayTypeName} :: {GetFXObjectPrefabName()}";
        public override string Editor_TimelineItemName => Editor_DisplayTypeName;
        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            EditorGUILayoutEx.GuidField<GameObject>(actionData, ref guidAudioClip);
        }

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayout.LabelField("FXObject", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayoutEx.GuidField<AudioClip>(actionData, "AudioClip", ref guidAudioClip);
                EditorGUILayoutEx.Toggle(actionData, "isLoop", ref isLoop);
                EditorGUILayoutEx.Toggle(actionData, guiContent_FollowActor, ref followActor);
                EditorGUILayoutEx.EnumPopup(actionData, "ModelRig", ref rigType);
                EditorGUILayoutEx.Vector3Field(actionData, "Pos", ref vOffsetPos);
            }
        }

        private string GetFXObjectPrefabName()
        {
            var path = AssetDatabase.GUIDToAssetPath(guidAudioClip);
            if (string.IsNullOrWhiteSpace(path))
                return "None";

            return System.IO.Path.GetFileNameWithoutExtension(guidAudioClip);
        }

        public override IActionEditorItem Copy() => new ActionEventData_SFX(this);
    }
    #endif

    public class ActionEventRuntime_SFX : ActionEventRuntimeBase<ActionEventData_SFX>
    {
        private AudioClip _audioClip;
        private SfxObject _sfxObject;
        
        public ActionEventRuntime_SFX(ActionEventData_SFX eventData) : base(eventData)
        {
        }

        public override IEnumerator OnAction_Init(Actor master, Action endCallback)
        {
            _audioClip = Managers.Asset.Load<AudioClip>(_eventData.guidAudioClip);
            
            return base.OnAction_Init(master, endCallback);
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            _sfxObject = Managers.Sfx.Play3D(
                _audioClip,
                _eventData.isLoop,
                master.ModelCtrl.GetRig(_eventData.rigType),
                _eventData.vOffsetPos,
                _eventData.followActor);
            
            base.OnAction_Start(master, fElapsedTime);
        }

        public override void OnAction_End(Actor master, bool bTimeEnd)
        {
            if (_sfxObject != null && _eventData.ReleaseOnExitAction)
            {
                var delFxObject = _sfxObject;
                _sfxObject = null;
                delFxObject.ReturnToPool(false);
            }
            
            base.OnAction_End(master, bTimeEnd);
        }

        public override void OnAction_Finalize(Actor master)
        {
            base.OnAction_Finalize(master);

            Managers.Asset.ReleaseHandle(_eventData.guidAudioClip);
        }
    }
}