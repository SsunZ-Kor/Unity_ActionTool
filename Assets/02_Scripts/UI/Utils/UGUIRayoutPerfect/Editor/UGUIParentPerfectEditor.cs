using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UGUIParentPerfect), true)]
[CanEditMultipleObjects]
public class UGUIParentPerfectEditor : Editor
{
    GUIContent _guiContent_RefreshButton;
    private void OnEnable()
    {
        _guiContent_RefreshButton = new("Refresh", "현재 설정에 맞게 RectTransform을 갱신합니다.");
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        bool bNeedUpdate = false;
        if (EditorGUI.EndChangeCheck())
            bNeedUpdate = true;

        if (GUILayout.Button(_guiContent_RefreshButton))
            bNeedUpdate = true;

        if (bNeedUpdate)
        {
            foreach (var lTarget in targets)
            {
                var parentPerfect = lTarget as UGUIParentPerfect;
                parentPerfect.Update_SizeDelta();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}