using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
using System.IO;
using UnityEditor.Animations;
using UnityEngine;
#endif

namespace Actor
{

    [System.Serializable]
    public partial class ActionData : ScriptableObject
    {
        public const float FRAME_RATE = 60f;
        public const int FRAME_RATE_INT = 60;
        public const float FRAME_PER_SEC = 1f / 60f;

        [System.NonSerialized] public List<ActionEventDataBase> listEventData = new();

        public float Length => _length;
        public bool Loop => _loop;

        public int Frame
        {
            get => Mathf.RoundToInt(_length * ActionData.FRAME_RATE);
            set => _length = value / ActionData.FRAME_RATE;
        }

        [SerializeField] private float _length;
        [SerializeField] private bool _loop;


        public void OnAfterDeserialize()
        {
            _OnAfterDeserialize_ActionEvent();
            _OnAfterDeserialize_ActionTrigger();
        }
        
        #if UNITY_EDITOR
        private static GUIStyle _guiStyle_MiddleRightLabal = null;

        public static ActionData Editor_CreateActionData(string path)
        {
            var newActionData = CreateInstance<ActionData>();
            path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "Action_.asset"));
            AssetDatabase.CreateAsset(newActionData, path);
            return newActionData;
        }

        public void Editor_OnGUI_Length(Actor selectedActor)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var newFrame = EditorGUILayout.DelayedIntField(Frame);
                if (Frame != newFrame)
                {
                    Undo.RecordObject(this, "ActionData FrameLength");
                    Frame = newFrame;
                    EditorUtility.SetDirty(this);
                }

                EditorGUILayout.LabelField($"{Length} sec");
                
                if (selectedActor != null)
                {
                    var anim = selectedActor.GetComponentInChildren<Animator>();
                    if (anim != null)
                    {
                        var animCtrl = anim.runtimeAnimatorController as AnimatorOverrideController;
                        if (animCtrl != null)
                        {

                            if (GUILayout.Button("SetByBaseAnim"))
                            {
                                // 마지막 EventData_PlayAnim 찾기
                                var eventData = _listAnimation.FindLast((x) =>
                                    x.layerType == AnimLayerTypes.Base);

                                // 아예 없다면
                                if (eventData == null || string.IsNullOrWhiteSpace(eventData.animStateName))
                                {
                                    this.Frame = eventData.Editor_StartFrame;
                                    return;
                                }

                                // Override 목록 구성
                                var animOverrides =
                                    new List<KeyValuePair<AnimationClip, AnimationClip>>(animCtrl.overridesCount);
                                animCtrl.GetOverrides(animOverrides);
                                var dicAnimOverrides = new Dictionary<AnimationClip, AnimationClip>();
                                foreach (var pair in animOverrides)
                                    dicAnimOverrides.Add(pair.Key, pair.Value);

                                // 스테이트 검색
                                var animCtrlBase = animCtrl.runtimeAnimatorController as AnimatorController;
                                var state = animCtrlBase.FindState(eventData.animStateName);
                                if (state != null)
                                {
                                    // 스테이트의 Motion Length를 찾고 정규화된 시간으로 변경
                                    var fMotionLength = state.motion.GetMotionMinLength(dicAnimOverrides);

                                    this.Frame = eventData.Editor_StartFrame +
                                                 Mathf.RoundToInt(fMotionLength * FRAME_RATE);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Editor_OnGUI_Inspector()
        {
            _guiStyle_MiddleRightLabal ??= new(EditorStyles.label) { alignment = TextAnchor.MiddleRight };

            using (new EditorGUILayoutEx.LabelWidthScope(50f))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var newFrame = EditorGUILayout.DelayedIntField("Length", Frame);
                    if (Frame != newFrame)
                    {
                        Undo.RecordObject(this, "ActionData FrameLength");
                        Frame = newFrame;
                        EditorUtility.SetDirty(this);
                    }

                    EditorGUILayout.LabelField($"{_length:0.##} sec", _guiStyle_MiddleRightLabal, GUILayout.Width(90f));
                }
                
                EditorGUILayoutEx.Toggle(this, "Loop", ref _loop);
            }
        }
        #endif
    }

#if UNITY_EDITOR
    public interface IActionEditorItem
    {
        public bool HasEndTime { get; }

        public int Editor_StartFrame { get; set; }
        public int Editor_EndFrame { get; set; }

        public string Editor_DisplayTypeName { get; }
        public string Editor_TimelineItemDesc { get; }
        public string Editor_TimelineItemName { get; }

        public void Editor_OnGUI_TimelineItemHeader(ActionData actionData);
        public bool Editor_OnGUI_InspectorHeader(ActionData actionData);

        public IActionEditorItem Copy();
    }
#endif

    public class ActionRuntime
    {
        public ActionData Data { get; private set; }

        public List<IActionEventRuntime> listEventRuntimes = null;

        public ActionRuntime(Actor master, ActionData actionData)
        {
            Data = actionData;
            listEventRuntimes = new(Data.listEventData.Count);

            foreach (var eventData in Data.listEventData)
                listEventRuntimes.Add(eventData.CreateRuntime(master));
        }

        public IEnumerator Init(Actor master, System.Action endCallback)
        {
            foreach (var iEventRuntime in listEventRuntimes)
                yield return iEventRuntime.OnAction_Init(master, null);
            
            endCallback?.Invoke();
        }

        public void OnFinalize(Actor master)
        {
            Data = null;
            if (listEventRuntimes != null)
            {
                foreach (var eventRuntime in listEventRuntimes)
                    eventRuntime.OnAction_Finalize(master);
                
                listEventRuntimes.Clear();
                listEventRuntimes = null;
            }
        }
    }
}