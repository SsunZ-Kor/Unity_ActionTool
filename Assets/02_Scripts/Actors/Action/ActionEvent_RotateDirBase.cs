using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public abstract partial class ActionEventData_RotateDirBase : ActionEventDataBase
    {
        public override bool HasEndTime => true;

        public float fAngleSpd;

        protected ActionEventData_RotateDirBase() : base() {}

        protected ActionEventData_RotateDirBase(ActionEventData_RotateDirBase prvEventData) : base(prvEventData)
        {
            fAngleSpd = prvEventData.fAngleSpd;
        }
    }
    
    #if UNITY_EDITOR
    public abstract partial class ActionEventData_RotateDirBase
    {
        public override string Editor_TimelineItemDesc => Editor_DisplayTypeName;
        public override string Editor_TimelineItemName  => Editor_DisplayTypeName.Replace("_", "\n");

        public static readonly GUIContent guiContent_AngleSpd
            = new("AngleSpeed (Degree)", "회전할 각속도 입니다.\n-1 입력 시, 즉시 동기화 됩니다. ");
        
        public static readonly GUIContent guiContent_RefreshDstDirOnUpdate 
            = new("Refresh Dst Dir OnUpdate", "이벤트 실행 중, 설정 회전 방향을 갱신합니다.\nOff시 처음 설정 방향을 유지합니다.");

        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            EditorGUILayoutEx.FloatField(actionData, ref fAngleSpd);
        }

        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayout.LabelField(guiContent_AngleSpd, EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.FloatField(actionData, GUIContent.none, ref fAngleSpd);
            --EditorGUI.indentLevel;
        }
    }
#endif

    public abstract class ActionEventRuntime_RotateDirBase<TRotateDirDataBase>
        : ActionEventRuntimeBase<TRotateDirDataBase> where TRotateDirDataBase : ActionEventData_RotateDirBase
    {
        protected bool bRefreshDst = false;

        private Vector3 _vDstDir;
        
        protected ActionEventRuntime_RotateDirBase(TRotateDirDataBase eventData) : base(eventData)
        {
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            _vDstDir = _GetDir(master);
            
            base.OnAction_Start(master, fElapsedTime);
        }

        public override bool OnAction_Update(Actor master, float fEventElapsedTime, float fDeltaTime)
        {
            var bUpdateBreak = base.OnAction_Update(master, fEventElapsedTime, fDeltaTime);
            if (bUpdateBreak)
                return true;
            
            if (bRefreshDst)
                _vDstDir = _GetDir(master);

            if (_eventData.fAngleSpd < -float.Epsilon)
                master.transform.rotation =  Quaternion.LookRotation(_vDstDir);
            
            
            var vNewForward = MathEx.SlerpByAngle(master.transform.forward, _vDstDir, _eventData.fAngleSpd * fDeltaTime);
            master.transform.rotation = Quaternion.LookRotation(vNewForward);
            
            return false;
        }

        protected abstract Vector3 _GetDir(Actor master);
    }
}