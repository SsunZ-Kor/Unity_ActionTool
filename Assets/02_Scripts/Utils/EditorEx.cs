#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

public static class EditorGUILayoutEx
{
    public static void ScriptField(SerializedObject so)
    {
        using (new EditorGUI.DisabledScope(true))
        {
            var prop_Script = so.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop_Script);
        }
    }

    public static void OnInspectorGUIWithoutScript(SerializedObject so)
    {
        EditorGUI.BeginChangeCheck();
        so.UpdateIfRequiredOrScript();

        SerializedProperty iterator = so.GetIterator();
        for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
        {
            if ("m_Script" != iterator.propertyPath)
                EditorGUILayout.PropertyField(iterator, true);
        }

        so.ApplyModifiedProperties();
        if (!EditorGUI.EndChangeCheck())
            return;

        foreach (var target in so.targetObjects)
            EditorUtility.SetDirty(target);
    }

    public static SerializedProperty PropertyField(string propName, SerializedObject so,
        params GUILayoutOption[] options)
    {
        var prop = so.FindProperty(propName);
        return PropertyField(propName, prop.displayName, so, options);
    }

    public static SerializedProperty PropertyField(string propName, string displayName, SerializedObject so,
        params GUILayoutOption[] options)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
            EditorGUILayout.PropertyField(prop, new GUIContent(displayName), options);
        else
            EditorGUILayout.LabelField($"Not Found Prop :: {propName}");

        return prop;
    }

    public static SerializedProperty PropertyStringPopupField(string propName, string[] popup, SerializedObject so,
        params GUILayoutOption[] options)
    {
        var prop = so.FindProperty(propName);
        return PropertyStringPopupField(propName, prop.displayName, popup, so, options);
    }

    public static SerializedProperty PropertyStringPopupField(string propName, string displayName, string[] popup,
        SerializedObject so, params GUILayoutOption[] options)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
        {
            var idx_PrvBgSpaceType = System.Array.IndexOf(popup, prop.stringValue);
            var idx_CrrBgSpaceType = EditorGUILayout.Popup(displayName, idx_PrvBgSpaceType, popup, options);
            if (idx_PrvBgSpaceType != idx_CrrBgSpaceType)
                prop.stringValue = popup.CheckIndex(idx_CrrBgSpaceType) ? popup[idx_CrrBgSpaceType] : string.Empty;
        }
        else
        {
            EditorGUILayout.LabelField($"Not Found Prop :: {propName}");
        }

        return prop;
    }

    public static bool ObjectField<T>(Object dirtyTarget, GUIContent label, ref T obj, bool allowSceneObjects, params GUILayoutOption[] options) where T : Object
    {
        var newObj = EditorGUILayout.ObjectField(label, obj, typeof(T), allowSceneObjects, options) as T;
        if (Equals(newObj, obj))
            return false;

        obj = newObj;
        
        if (dirtyTarget != null)
            EditorUtility.SetDirty(dirtyTarget);
    
        return true;
    }

    public static bool ObjectField<T>(Object dirtyTarget, string label, ref T obj, bool allowSceneObjects, params GUILayoutOption[] options) where T : Object
        => ObjectField(dirtyTarget, new GUIContent(label), ref obj, allowSceneObjects, options );
    
    public static bool ObjectField<T>(Object dirtyTarget, ref T obj, bool allowSceneObjects, params GUILayoutOption[] options) where T : Object
        => ObjectField(dirtyTarget, GUIContent.none, ref obj, allowSceneObjects, options );
    
    public static bool IntField(Object dirtyTarget, GUIContent label, ref int value, params GUILayoutOption[] options)
    {
        var newValue = EditorGUILayout.IntField(label, value, options);
        if (newValue == value)
            return false;
        
        if (dirtyTarget)
            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

        value = newValue;

        if (dirtyTarget)
            EditorUtility.SetDirty(dirtyTarget);

        return true;
    }

    public static bool IntField(Object dirtyTarget, string label, ref int value, params GUILayoutOption[] options)
        => IntField(dirtyTarget, new GUIContent(label), ref value, options);

    public static bool IntField(Object dirtyTarget, ref int value, params GUILayoutOption[] options)
        => IntField(dirtyTarget, GUIContent.none, ref value, options);
    
    public static bool FloatField(Object dirtyTarget, GUIContent label, ref float value, params GUILayoutOption[] options)
    {
        var newValue = EditorGUILayout.FloatField(label, value, options);
        if (newValue == value)
            return false;
        
        if (dirtyTarget)
            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

        value = newValue;

        if (dirtyTarget)
            EditorUtility.SetDirty(dirtyTarget);

        return true;
    }

    public static bool FloatField(Object dirtyTarget, string label, ref float value, params GUILayoutOption[] options)
        => FloatField(dirtyTarget, new GUIContent(label), ref value, options);

    public static bool FloatField(Object dirtyTarget, ref float value, params GUILayoutOption[] options)
        => FloatField(dirtyTarget, GUIContent.none, ref value, options);
    
    public static bool ClampedFloatField(Object dirtyTarget, GUIContent label, ref float value, float fMin, float fMax, params GUILayoutOption[] options)
    {
        var newValue = Mathf.Clamp(EditorGUILayout.FloatField(label, value, options), fMin, fMax);
        if (newValue == value)
            return false;
        
        if (dirtyTarget)
            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

        value = newValue;

        if (dirtyTarget)
            EditorUtility.SetDirty(dirtyTarget);

        return true;
    }

    public static bool ClampedFloatField(Object dirtyTarget, string label, ref float value, float fMin, float fMax, params GUILayoutOption[] options)
        => ClampedFloatField(dirtyTarget, new GUIContent(label), ref value, fMin, fMax, options);

    public static bool ClampedFloatField(Object dirtyTarget, ref float value, float fMin, float fMax, params GUILayoutOption[] options)
        => ClampedFloatField(dirtyTarget, GUIContent.none, ref value, fMin, fMax, options);

    [System.Flags]
    public enum DisableAxis
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2
    }
    
    public static bool ClampedFrameField(Object dirtyTarget, GUIContent label, ref int nFrame, int nFrameRate, int nMin, int nMax,  params GUILayoutOption[] option)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));

            
            var prvIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            {
                var newValue = EditorGUILayout.IntField(GUIContent.none, nFrame);
                newValue = Mathf.Clamp(newValue, nMin, nMax);
                EditorGUILayout.LabelField("frame", GUILayout.Width(50f));

                var fInvFrameRate = 1f / nFrameRate;

                var newLength = EditorGUILayout.FloatField(GUIContent.none, newValue * fInvFrameRate);
                newValue = Mathf.RoundToInt(Mathf.Clamp(newLength, nMin * fInvFrameRate, nMax * fInvFrameRate) *
                                            nFrameRate);
                EditorGUILayout.LabelField("sec", GUILayout.Width(20f));

                if (nFrame == newValue)
                    return false;

                if (dirtyTarget)
                    Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

                nFrame = newValue;

                if (dirtyTarget)
                    EditorUtility.SetDirty(dirtyTarget);
            }
            EditorGUI.indentLevel = prvIndentLevel;
        }

        return true;
    }
    
    public static bool ClampedFrameField(Object dirtyTarget, string label, ref int nFrame, int nFrameRate, int nMin, int nMax, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, new GUIContent(label), ref nFrame, nFrameRate, nMin, nMax, options);
    
    public static bool ClampedFrameField(Object dirtyTarget, ref int frame, int nFrameRate, int nMin, int nMax, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, GUIContent.none, ref frame, nFrameRate, nMin, nMax, options);
    
    public static bool FrameField(Object dirtyTarget, GUIContent label, ref int frame, int nFrameRate, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, label, ref frame, nFrameRate, 0, int.MaxValue, options);

    public static bool FrameField(Object dirtyTarget, string label, ref int frame, int nFrameRate, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, new GUIContent(label), ref frame, nFrameRate, 0, int.MaxValue, options);

    public static bool FrameField(Object dirtyTarget, ref int frame, int nFrameRate, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, GUIContent.none, ref frame, nFrameRate, 0, int.MaxValue, options);
    
    public static bool ClampedFrameField(Object dirtyTarget, GUIContent label, ref float fSec, int nFrameRate, float fMin, float fMax, params GUILayoutOption[] option)
    {
        var fMaxFrame = int.MaxValue / 60f;

        fSec = Mathf.Clamp(fSec, 0f, fMaxFrame);
        fMin = Mathf.Clamp(fMin, 0f, fMaxFrame);
        fMax = Mathf.Clamp(fMax, 0f, fMaxFrame);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));

            using (new EditorGUI.IndentLevelScope())
            {
                var newValue = EditorGUILayout.IntField(GUIContent.none,  Mathf.RoundToInt(fSec * nFrameRate));
                newValue = Mathf.Clamp(newValue,  Mathf.RoundToInt(fMin * nFrameRate),  Mathf.RoundToInt(fMax * nFrameRate));
                EditorGUILayout.LabelField("frame", GUILayout.Width(50f));

                var fInvFrameRate = 1f / nFrameRate;

                var newLength = EditorGUILayout.FloatField(GUIContent.none, newValue * fInvFrameRate);
                newLength = Mathf.Clamp(newLength, fMin, fMax);
                EditorGUILayout.LabelField("sec", GUILayout.Width(20f));

                if (fSec == newLength)
                    return false;
                    
                if (dirtyTarget)
                    Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

                fSec = newValue * fInvFrameRate;
            
                if (dirtyTarget)
                    EditorUtility.SetDirty(dirtyTarget);       
            }
        }

        return true;
    }
    
    public static bool ClampedFrameField(Object dirtyTarget, string label, ref float fSec, int nFrameRate, float fMin, float fMax,  params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, new GUIContent(label), ref fSec, nFrameRate, fMin, fMax,  options);
    
    public static bool ClampedFrameField(Object dirtyTarget, ref float fSec, int nFrameRate, float fMin, float fMax, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, GUIContent.none, ref fSec, nFrameRate, fMin, fMax, options);
    
    public static bool FrameField(Object dirtyTarget, GUIContent label, ref float fSec, int nFrameRate, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, label, ref fSec, nFrameRate, 0f, float.MaxValue, options);

    public static bool FrameField(Object dirtyTarget, string label, ref float fSec, int nFrameRate, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget, new GUIContent(label), ref fSec, nFrameRate, 0f, float.MaxValue, options);
    
    public static bool FrameField(Object dirtyTarget, ref float fSec, int nFrameRate, params GUILayoutOption[] options)
        => ClampedFrameField(dirtyTarget,  GUIContent.none, ref fSec, nFrameRate, 0f, float.MaxValue, options);
    
    public static bool Vector3Field(Object dirtyTarget, GUIContent label, ref Vector3 value, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
    {
        var result = false;
        using (new EditorGUILayout.HorizontalScope())
        {
            if (label != null)
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            else
                EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 12f));

            var newValue = Vector3.zero;

            var prvIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            using (new EditorGUILayoutEx.LabelWidthScope(10f))
            {
                EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.X) != 0);
                newValue.x = EditorGUILayout.FloatField("X", value.x);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.Y) != 0);
                newValue.y = EditorGUILayout.FloatField("Y", value.y);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.Z) != 0);
                newValue.z = EditorGUILayout.FloatField("Z", value.z);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel = prvIndentLevel;

            if (newValue != value)
            {
                if (dirtyTarget)
                    Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label?.text ?? "Unknown"}");

                value = newValue;

                if (dirtyTarget)
                    EditorUtility.SetDirty(dirtyTarget);

                result = true;
            }
        }

        return result;
    }
    
    public static bool Vector3Field(Object dirtyTarget, string label, ref Vector3 value, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
        => Vector3Field(dirtyTarget, new GUIContent(label), ref value, disableAxis, options);

    public static bool Vector3Field(Object dirtyTarget, ref Vector3 value, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
        => Vector3Field(dirtyTarget, GUIContent.none, ref value, disableAxis, options);
    
    public static bool QuaternionField(Object dirtyTarget, GUIContent label, ref Quaternion value, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
    {
        if (value == default)
        {
            if (dirtyTarget)
                Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label?.text ?? "Unknown"}");

            value = Quaternion.identity;
            
            if (dirtyTarget)
                EditorUtility.SetDirty(dirtyTarget);
        }
        
        var vEuler = value.eulerAngles;
        
        var result = false;
        using (new EditorGUILayout.HorizontalScope())
        {
            if (label != null)
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            else
                EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 12f));

            var newValue = Vector3.zero;

            var prvIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            using (new EditorGUILayoutEx.LabelWidthScope(10f))
            {
                EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.X) != 0);
                newValue.x = EditorGUILayout.FloatField("X", vEuler.x);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.Y) != 0);
                newValue.y = EditorGUILayout.FloatField("Y", vEuler.y);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.Z) != 0);
                newValue.z = EditorGUILayout.FloatField("Z", vEuler.z);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel = prvIndentLevel;

            if (newValue != vEuler)
            {
                if (dirtyTarget)
                    Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label?.text ?? "Unknown"}");

                value = Quaternion.Euler(newValue);

                if (dirtyTarget)
                    EditorUtility.SetDirty(dirtyTarget);

                result = true;
            }
        }

        return result;
    }
    
    public static bool QuaternionField(Object dirtyTarget, string label, ref Quaternion value, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
        => QuaternionField(dirtyTarget, new GUIContent(label), ref value, disableAxis, options);

    public static bool QuaternionField(Object dirtyTarget, ref Quaternion value, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
        => QuaternionField(dirtyTarget, GUIContent.none, ref value, disableAxis, options);

    public static bool CurveField(Object dirtyTarget, GUIContent label, ref AnimationCurve value, params GUILayoutOption[] options)
    {
        var result = false;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (label != null)
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            else
                EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 12f));

            using (var checkScope = new EditorGUI.ChangeCheckScope())
            {
                var prvIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                using (new EditorGUILayoutEx.LabelWidthScope(0f))
                {
                    var newValue = EditorGUILayout.CurveField(string.Empty, value);
                    if (checkScope.changed)
                    {
                        if (dirtyTarget)
                            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label?.text ?? "Unknown"}");

                        value = newValue;

                        if (dirtyTarget)
                            EditorUtility.SetDirty(dirtyTarget);

                        result = true;
                    }
                }
                EditorGUI.indentLevel = prvIndentLevel;
            }
        }

        return result;
    }
    
    public static bool CurveField(Object dirtyTarget, string label, ref AnimationCurve value, params GUILayoutOption[] options)
        => CurveField(dirtyTarget, new GUIContent(label), ref value, options);

    public static bool CurveField(Object dirtyTarget,  ref AnimationCurve value, params GUILayoutOption[] options)
        => CurveField(dirtyTarget, (GUIContent)null,  ref value, options);
    
    public static bool Curve3Field(Object dirtyTarget, GUIContent label, ref AnimationCurve valueX, ref AnimationCurve valueY, ref AnimationCurve valueZ, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
    {
        var result = false;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (label != null)
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            else
                EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorGUI.indentLevel * 12f));

            using (var checkScope = new EditorGUI.ChangeCheckScope())
            {
                var prvIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                using (new EditorGUILayoutEx.LabelWidthScope(10f))
                {
                    EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.X) != 0);
                    var newValueX = EditorGUILayout.CurveField("X", valueX);
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.Y) != 0);
                    var newValueY = EditorGUILayout.CurveField("Y", valueY);
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup((disableAxis & DisableAxis.Z) != 0);
                    var newValueZ = EditorGUILayout.CurveField("Z", valueZ);
                    EditorGUI.EndDisabledGroup();

                    if (checkScope.changed)
                    {
                        if (dirtyTarget)
                            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label?.text ?? "Unknown"}");

                        valueX = newValueX;
                        valueY = newValueY;
                        valueZ = newValueZ;

                        if (dirtyTarget)
                            EditorUtility.SetDirty(dirtyTarget);

                        result = true;
                    }
                }
                EditorGUI.indentLevel = prvIndentLevel;
            }
        }

        return result;
    }
    
    public static bool Curve3Field(Object dirtyTarget, string label, ref AnimationCurve valueX, ref AnimationCurve valueY, ref AnimationCurve valueZ, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
        => Curve3Field(dirtyTarget, new GUIContent(label),  ref valueX, ref valueY, ref valueZ, disableAxis,options);

    public static bool Curve3Field(Object dirtyTarget,  ref AnimationCurve valueX, ref AnimationCurve valueY, ref AnimationCurve valueZ, DisableAxis disableAxis = 0, params GUILayoutOption[] options)
        => Curve3Field(dirtyTarget, GUIContent.none,  ref valueX, ref valueY, ref valueZ, disableAxis, options);

    public static bool TextField(Object dirtyTarget, GUIContent label, ref string text, params GUILayoutOption[] options)
    {
        var newText = EditorGUILayout.TextField(label, text, options);
        if (newText == text)
            return false;
        
        if (dirtyTarget)
            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

        text = newText;

        if (dirtyTarget)
            EditorUtility.SetDirty(dirtyTarget);

        return true;
    }

    public static bool TextField(Object dirtyTarget, string label, ref string text, params GUILayoutOption[] options)
        => TextField(dirtyTarget, new GUIContent(label), ref text, options);

    public static bool TextField(Object dirtyTarget, ref string text, params GUILayoutOption[] options)
        => TextField(dirtyTarget, GUIContent.none, ref text, options);
    
    
    public static bool Toggle(Object dirtyTarget, GUIContent label, ref bool value, params GUILayoutOption[] options)
    {
        var newValue = EditorGUILayout.Toggle(label, value, options);
        if (newValue == value)
            return false;
        
        if (dirtyTarget)
            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

        value = newValue;

        if (dirtyTarget)
            EditorUtility.SetDirty(dirtyTarget);
        
        return true;
    }

    public static bool Toggle(Object dirtyTarget, string label, ref bool value, params GUILayoutOption[] options)
        => Toggle(dirtyTarget, new GUIContent(label), ref value, options);

    public static bool Toggle(Object dirtyTarget, ref bool value, params GUILayoutOption[] options)
        => Toggle(dirtyTarget, GUIContent.none, ref value, options);
    
    public static bool EnumPopup<T>(Object dirtyTarget, GUIContent label, ref T selected, params GUILayoutOption[] options) where T : Enum
    {
        var newSelected = (T)EditorGUILayout.EnumPopup(label, selected, options);
        if (newSelected.Equals(selected))
            return false;
        
        if (dirtyTarget)
            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

        selected = newSelected;

        if (dirtyTarget)
            EditorUtility.SetDirty(dirtyTarget);

        return true;
    }

    public static bool EnumPopup<T>(Object dirtyTarget, string label, ref T selected, params GUILayoutOption[] options)  where T : Enum
        => EnumPopup(dirtyTarget, new GUIContent(label), ref selected, options);

    public static bool EnumPopup<T>(Object dirtyTarget, ref T selected, params GUILayoutOption[] options)  where T : Enum
        => EnumPopup(dirtyTarget, GUIContent.none, ref selected, options);
    
    public static bool EnumFlagsField<T>(Object dirtyTarget, GUIContent label, ref T selected, params GUILayoutOption[] options) where T : Enum
    {
        var newSelected = (T)EditorGUILayout.EnumFlagsField(label, selected, options);
        if (newSelected.Equals(selected))
            return false;
        
        if (dirtyTarget)
            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {label.text}");

        selected = newSelected;

        if (dirtyTarget)
            EditorUtility.SetDirty(dirtyTarget);

        return true;
    }

    public static bool EnumFlagsField<T>(Object dirtyTarget, string label, ref T selected, params GUILayoutOption[] options)  where T : Enum
        => EnumFlagsField(dirtyTarget, new GUIContent(label), ref selected, options);

    public static bool EnumFlagsField<T>(Object dirtyTarget, ref T selected, params GUILayoutOption[] options)  where T : Enum
        => EnumFlagsField(dirtyTarget, GUIContent.none, ref selected, options);

    public static bool GuidField<T>(Object dirtyTarget, GUIContent label, ref string guid, params GUILayoutOption[] options) where T : Object
    {
        T oldObj = null;
        if (!string.IsNullOrWhiteSpace(guid))
        {
            var oldPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrWhiteSpace(oldPath))
                oldObj = AssetDatabase.LoadAssetAtPath<T>(oldPath);
        }

        var newObj = EditorGUILayout.ObjectField(label, oldObj, typeof(T), false, options);
        if (oldObj == newObj)
            return false;
        
        if (dirtyTarget)
            Undo.RecordObject(dirtyTarget, $"{dirtyTarget.name} :: {guid}");
        
        if (newObj == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newObj, out guid, out var newLocalId))
            guid = string.Empty;
        
        
        if (dirtyTarget)
            EditorUtility.SetDirty(dirtyTarget);

        return true;
    }
    
    public static bool GuidField<T>(Object dirtyTarget, string label, ref string guid, params GUILayoutOption[] options)  where T : Object
        => GuidField<T>(dirtyTarget, new GUIContent(label), ref guid, options);

    public static bool GuidField<T>(Object dirtyTarget, ref string guid, params GUILayoutOption[] options)  where T : Object
        => GuidField<T>(dirtyTarget, GUIContent.none, ref guid, options);
    
    public class LabelWidthScope : GUI.Scope
    {
        private float defaultValue;

        public LabelWidthScope(float width)
        {
            defaultValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
        }


        protected override void CloseScope() => EditorGUIUtility.labelWidth = defaultValue;
    }
    
    public class GUIBackgroundColorScope : GUI.Scope
    {
        private Color defaultValue;

        public GUIBackgroundColorScope(Color color)
        {
            defaultValue = GUI.backgroundColor;
            GUI.backgroundColor = color;
        }


        protected override void CloseScope() => GUI.backgroundColor = defaultValue;
    }
    
    public class GUIContentColorScope : GUI.Scope
    {
        private Color defaultValue;

        public GUIContentColorScope(Color color)
        {
            defaultValue = GUI.contentColor;
            GUI.contentColor = color;
        }


        protected override void CloseScope() => GUI.contentColor = defaultValue;
    }
}
#endif