using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ButtonEx), true)]
[CanEditMultipleObjects]
public class ButtonExInspector : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayoutEx.ScriptField(serializedObject);
        OnInspectorGUI_ForButtonState();
        EditorGUILayoutEx.OnInspectorGUIWithoutScript(serializedObject);
    }

    protected virtual void OnInspectorGUI_ForButtonState()
    {
        if (targets == null || targets.Length > 1)
            return;
        
        EditorGUI.BeginChangeCheck();
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUILayout.BeginHorizontal("box");
        {
            var widthLabel = GUILayout.Width(50f);
            var widthContents = GUILayout.MinWidth(50f);
            var widthAnim = GUILayout.Width(90f);
            
            /* Row */
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("", widthLabel);
                for (var ePtr = ButtonEx.PointerStateTypes.Normal; ePtr <= ButtonEx.PointerStateTypes.Exited; ++ePtr)
                    EditorGUILayout.LabelField(ePtr.ToString(), widthLabel);
            }
            EditorGUILayout.EndVertical();
            
            /* Cell :: Anim */
            EditorGUILayout.BeginVertical();
            {
                var prop_PtrAnimType = serializedObject.FindProperty("_pointerStateAnimType");
                EditorGUILayout.PropertyField(prop_PtrAnimType, GUIContent.none, widthAnim);

                var propName_Anims = (ButtonEx.StateAnimTypes)prop_PtrAnimType.enumValueIndex switch
                {
                    ButtonEx.StateAnimTypes.Scale => "_pointerStateScales",
                    ButtonEx.StateAnimTypes.Anim => "_pointerStateAnims",
                    _ => null,
                };
                
                var prop_Anims = serializedObject.FindProperty(propName_Anims);
                for (var ePtr = ButtonEx.PointerStateTypes.Normal; ePtr <= ButtonEx.PointerStateTypes.Exited; ++ePtr)
                {
                    var prop_Anim = prop_Anims.GetArrayElementAtIndex((int)ePtr);
                    EditorGUILayout.PropertyField(prop_Anim, GUIContent.none, widthAnim);
                }
            }
            EditorGUILayout.EndVertical();

            var prop_ItemInfos = serializedObject.FindProperty("_pointerStateItems");
            for (int i = 0; i < prop_ItemInfos.arraySize; ++i)
            {
                EditorGUILayout.BeginVertical();
                {
                    /* Column */
                    EditorGUILayout.LabelField(((ButtonEx.ButtonStateTypes)i).ToString(), widthContents);
                    
                    /* Cell :: GameObjects */
                    var prop_ItemInfo = prop_ItemInfos.GetArrayElementAtIndex(i);
                    var prop_ItemInfoObjects = prop_ItemInfo.FindPropertyRelative("_rootItem");
                    for (int j = 0; j < prop_ItemInfoObjects.arraySize; ++j)
                    {
                        var prop_Objects = prop_ItemInfoObjects.GetArrayElementAtIndex(j);
                        EditorGUILayout.PropertyField(prop_Objects, GUIContent.none, widthContents);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(target);
    }
}