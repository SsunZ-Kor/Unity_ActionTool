using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using Unity.Collections;
using UnityEditor;
#endif

namespace Actor
{
    public abstract partial class ActionEventData_MovePosBase : ActionEventDataBase
    {
        public override bool HasEndTime => true;
        
        private static readonly Keyframe keyFrame_DefaultStart = new(0, 0, 1f, 1f);
        private static readonly Keyframe keyFrame_DefaultEnd = new(1f, 1f, 1f, 1f);
        
        public Vector3 vPos;
        public AnimationCurve curveX = new(keyFrame_DefaultStart, keyFrame_DefaultEnd);
        public AnimationCurve curveY = new(keyFrame_DefaultStart, keyFrame_DefaultEnd);
        public AnimationCurve curveZ = new(keyFrame_DefaultStart, keyFrame_DefaultEnd);
        
        public bool bGroundMoving = true;
        public bool bCrrDirOnStart = false;
        public float fCrrDirUpdateSpd = -1f;

        protected ActionEventData_MovePosBase() : base() {}

        protected ActionEventData_MovePosBase(ActionEventData_MovePosBase prvEventData) : base(prvEventData)
        {
            vPos = prvEventData.vPos;
            curveX = new();
            curveX.CopyFrom(prvEventData.curveX);
            curveY = new();
            curveY.CopyFrom(prvEventData.curveY);
            curveZ = new();
            curveZ.CopyFrom(prvEventData.curveZ);

            bGroundMoving = prvEventData.bGroundMoving;
            bCrrDirOnStart = prvEventData.bCrrDirOnStart;
            fCrrDirUpdateSpd = prvEventData.fCrrDirUpdateSpd;
        }
        
        public Vector3 GetPos(float fEventElapsedTime)
        {
            var tCurve = duration > float.Epsilon 
                ? Mathf.Clamp01(fEventElapsedTime / duration)
                : 0f;
            
            return new (
                vPos.x * curveX.Evaluate(tCurve),
                vPos.y * curveY.Evaluate(tCurve),
                vPos.z * curveZ.Evaluate(tCurve));
        }
    }
    
    #if UNITY_EDITOR
    public abstract partial class ActionEventData_MovePosBase
    {
        public static readonly GUIContent guiContent_GroundMoving 
            = new("Is Ground Moving", "활성화 시, 방향의 높이를 무시합니다. 경사를 자연스럽게 이동합니다.");

        public static readonly GUIContent guiContent_DstDirUpdateType 
            = new("Refresh Dst Dir OnUpdate", "이벤트 실행 중, 설정 이동 방향을 갱신합니다.\nOff시 처음 설정 방향을 유지합니다.");

        public static readonly GUIContent guiContent_RefreshCrrDirOnStart 
            = new("Refresh Crr Dir OnStart", "이벤트 시작 시, 현재 이동 방향을 설정된 방향으로 즉시 동기화합니다.\nOff시 시작 이동 방향은 캐릭터의 정면입니다.");

        public static readonly GUIContent guiContent_RefreshDirAngleSpeedOnUpdate 
            = new("Crr Dir AngleSpeed OnUpdate (Degree)", "이벤트 실행 중, 현재 이동 방향을 설정된 방향으로 해당 각속도만큼 동기화 합니다.\n-1 입력 시, 즉시 동기화 합니다.");

        public override string Editor_TimelineItemDesc => $"{Editor_DisplayTypeName} :: {vPos.x}, {vPos.y}, {vPos.z}";
        public override string Editor_TimelineItemName => Editor_DisplayTypeName.Replace("_", "\n");

        public override void Editor_OnGUI_TimelineItemHeader(ActionData actionData)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutEx.Vector3Field(actionData, (GUIContent)null, ref vPos);
            }
        }
        
        protected override void Editor_OnGUI_InspectorContent(ActionData actionData)
        {
            EditorGUILayout.LabelField("Position", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.Vector3Field(actionData, (GUIContent)null, ref vPos);
            EditorGUILayoutEx.Curve3Field(actionData, (GUIContent)null, ref curveX, ref curveY, ref curveZ);
            --EditorGUI.indentLevel;

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Options :: Common", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            EditorGUILayoutEx.Toggle(actionData, guiContent_GroundMoving, ref bGroundMoving);
            EditorGUILayoutEx.Toggle(actionData, guiContent_RefreshCrrDirOnStart, ref bCrrDirOnStart);
            EditorGUILayoutEx.FloatField(actionData, guiContent_RefreshDirAngleSpeedOnUpdate, ref fCrrDirUpdateSpd);
            --EditorGUI.indentLevel;
        }
    }
#endif
    
    public abstract class ActionEventRuntime_MovePosBase<TMovePosDataBase>
        : ActionEventRuntimeBase<TMovePosDataBase> where TMovePosDataBase : ActionEventData_MovePosBase
    {
        protected bool bRefreshDst = false;

        private Vector3 _vDstDir;
        private Vector3 _vCrrDir;
        private Quaternion _qCrrDir;
        private Vector3 _vPrvPos;
        
        protected ActionEventRuntime_MovePosBase(TMovePosDataBase eventData) : base(eventData)
        {
        }

        public override void OnAction_Start(Actor master, float fElapsedTime)
        {
            base.OnAction_Start(master, fElapsedTime);

            _vDstDir = _GetDir(master);
            _vCrrDir = _eventData.bCrrDirOnStart 
                ? _vDstDir
                : master.transform.forward;

            _qCrrDir = Quaternion.LookRotation(_vCrrDir);
            _vPrvPos = Vector3.zero;
        }

        protected abstract Vector3 _GetDir(Actor master);

        protected Vector3 _Cal_MoveDelta(Actor master, float fEventElapsedTime, float fDeltaTime)
        {
            if (bRefreshDst)
                _vDstDir = _GetDir(master);

            /* 음수라면 즉시 적용 */
            if (_eventData.fCrrDirUpdateSpd < -float.Epsilon)
            {
                _vCrrDir = _vDstDir;
                _qCrrDir = Quaternion.LookRotation(_vCrrDir);
            }
            /* 양수라면 각속도에 따른 각도 적용 */ 
            else if (_eventData.fCrrDirUpdateSpd > float.Epsilon)
            {
                // 양수라면 각속도에 따른 각도 보간
                _vCrrDir = MathEx.SlerpByAngle(
                    _vCrrDir,
                    _vDstDir,
                    _eventData.fCrrDirUpdateSpd * fDeltaTime);
                
                _qCrrDir = Quaternion.LookRotation(_vCrrDir);
            }
            
            /* vMoveDelta 계산 */ 
            var vCrrPos = _eventData.GetPos(fEventElapsedTime);
            var vMoveDelta = _qCrrDir * (vCrrPos - _vPrvPos);
            _vPrvPos = vCrrPos;
            return vMoveDelta;
        }
    }
}