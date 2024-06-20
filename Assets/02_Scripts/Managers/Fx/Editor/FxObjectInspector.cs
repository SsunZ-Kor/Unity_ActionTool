using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FxObject), true)]
[CanEditMultipleObjects]
public class FxObjectInspector : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayoutEx.ScriptField(serializedObject);
    
        if (GUILayout.Button("Update Component"))
        {
            foreach (var t in targets)
            {
                var fxObject = t as FxObject;
                if (fxObject != null)
                    fxObject.UpdateComponents();

                EditorUtility.SetDirty(fxObject);
            }
        }
        
        if (targets.Length <= 1)
            EditorGUILayoutEx.OnInspectorGUIWithoutScript(serializedObject);
    }
}
