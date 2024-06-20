using System.Collections;
using System.Collections.Generic;
using Actor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TargetingObject))]
public class TargetingObjectInspector : Editor
{
    private static float _fAngle = 0;
    private static float _fDist = 0;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        using (new EditorGUILayout.VerticalScope("box"))
        {
            _fAngle = EditorGUILayout.FloatField("Angles", _fAngle);
            _fDist = EditorGUILayout.FloatField("Dists", _fDist);

            if (GUILayout.Button("Adjust For Children"))
            {
                var to = target as TargetingObject;
                var trTO = to.transform;
                if (trTO.childCount <= 0)
                    return;

                if (trTO.childCount == 1)
                {
                    var trChild = trTO.GetChild(0);

                    trChild.localPosition = new Vector3(0f, 0f, _fDist);
                    trChild.localRotation = Quaternion.identity;
                    return;
                }

                var fStartingOfAngles = _fAngle * -0.5f;
                var fIntervalOfAngles = _fAngle / (trTO.childCount - 1);
                
                for (int i = 0; i < trTO.childCount; ++i)
                {
                    var trChild = trTO.GetChild(i);

                    trChild.localRotation = Quaternion.Euler(0f, fStartingOfAngles + fIntervalOfAngles * i, 0f);
                    trChild.localPosition = trChild.forward * _fDist;
                }
                
                EditorUtility.SetDirty(this);
            }

            if (GUILayout.Button("Refresh Child Comp"))
            {
                var toChildren = target.GetComponentsInChildren<TargetingObjectChild>();
                foreach (var toChild in toChildren)
                    toChild.RefreshComponent();
                
                EditorUtility.SetDirty(this);
            }
        }
    }
}