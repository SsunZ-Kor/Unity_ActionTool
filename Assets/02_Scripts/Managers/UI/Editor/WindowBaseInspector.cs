using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WindowBase), true)]
public class WindowBaseInspector : Editor
{
    protected string[] _allBgSpaceType;
    
    protected virtual void OnEnable()
    {
        var t_base = typeof(BgSpaceBase);

        var listAllBgSpaceType = Assembly.GetAssembly(t_base).GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(t_base))
            .Select(t => t.Name)
            .ToList();
            
        listAllBgSpaceType.Insert(0, UIManager.BgSpaceType_None);
        listAllBgSpaceType.Insert(1, UIManager.BgSpaceType_MainCam);
        _allBgSpaceType = listAllBgSpaceType.ToArray();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayoutEx.ScriptField(serializedObject);
        OnInspectorGUI_WindowBase();
        EditorGUILayoutEx.OnInspectorGUIWithoutScript(serializedObject);
    }

    protected virtual void OnInspectorGUI_WindowBase()
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.UpdateIfRequiredOrScript();
        
        EditorGUILayout.LabelField("Window :: Anim", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayoutEx.PropertyField("_anim", serializedObject);
            EditorGUILayoutEx.PropertyField("_animClip_Open", serializedObject);
            EditorGUILayoutEx.PropertyField("_animClip_Close", serializedObject);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Window :: Background", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        {
            var prop_IsOverlay = EditorGUILayoutEx.PropertyField("isOverlay", serializedObject);
            if (prop_IsOverlay.boolValue == false)
            {
                var prop_BgSpaceType = EditorGUILayoutEx.PropertyStringPopupField("bgSpaceTypeName", "bgSpaceType", _allBgSpaceType, serializedObject);
                if (prop_BgSpaceType.stringValue != UIManager.BgSpaceType_None
                    && prop_BgSpaceType.stringValue  != UIManager.BgSpaceType_MainCam)
                    EditorGUILayoutEx.PropertyField("uiImg_Bg", serializedObject);
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        var defaultLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 85f;
        EditorGUILayout.LabelField("Window :: BGM", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        {
            WindowBase.BgmType crrBgmType = WindowBase.BgmType.None;
            EditorGUILayout.BeginHorizontal();
            {
                var prop_UseBgm = EditorGUILayoutEx.PropertyField("_useBgm", serializedObject, GUILayout.Width(EditorGUIUtility.labelWidth + 100f));
                crrBgmType = (WindowBase.BgmType)prop_UseBgm.enumValueIndex;
                if (WindowBase.BgmType.None != crrBgmType)
                    EditorGUILayoutEx.PropertyField("_bgm_audioClip", "AudioClip", serializedObject);

                if (WindowBase.BgmType.KeepPrev == crrBgmType)
                    EditorGUILayoutEx.PropertyField("_bgm_keepPrevNormalizedTime", "KeepPrvTime", serializedObject, GUILayout.Width(EditorGUIUtility.labelWidth + 16.5f));
            }
            EditorGUILayout.EndHorizontal();
            if (WindowBase.BgmType.None != crrBgmType)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayoutEx.PropertyField("_bgm_FadeType", "FadeType",serializedObject, GUILayout.Width(EditorGUIUtility.labelWidth + 100));
                    EditorGUILayoutEx.PropertyField("_bgm_FadeOutTime", "Prv Out Time",serializedObject);
                    EditorGUILayoutEx.PropertyField("_bgm_FaddInTime", "Crr In Time", serializedObject);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        EditorGUIUtility.labelWidth = defaultLabelWidth;

        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck()) 
            EditorUtility.SetDirty(target);

        var windowName = target.GetType().Name.Replace("Window_", "").Replace("_", " ");
        EditorGUILayout.LabelField(windowName, EditorStyles.boldLabel);
    }
}
