using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class MonoEx
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
            return null;

        var result = gameObject.GetComponent<T>();
        if (result != null)
            return result;
        
        result = gameObject.AddComponent<T>();
        return result;
    }
    
    public static T GetOrAddComponent<T>(this Component component) where T : Component
    {
        if (component == null)
            return null;

        return component.gameObject.GetOrAddComponent<T>();
    }

    public static void Reset(this Transform tr)
    {
        if (tr == null)
            return;
        
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;
    }

    public static void SetWorldTRS(this Transform tr, Vector3 vPos, Quaternion qRot, Vector3 vScale)
    {
        if (tr == null)
            return;

        var trParent = tr.parent;
        tr.SetParent(null);

        tr.position = vPos;
        tr.rotation = qRot;
        tr.localScale = vScale;
        
        tr.SetParent(trParent);
    }

    public static void SetLocalTRS(this Transform tr, Vector3 vPos, Quaternion qRot, Vector3 vScale)
    {
        if (tr == null)
            return;
        
        tr.localPosition = vPos;
        tr.localRotation = qRot;
        tr.localScale = vScale;
    }
    
    public static void SampleAnimClip(this Animation anim, string clipName, float factor)
    {
        if (anim == null
            || string.IsNullOrEmpty(clipName)
            || anim.GetClip(clipName) == null)
            return;

        anim.Stop();
        anim[clipName].enabled = true;
        anim[clipName].time = anim[clipName].length * factor;
        anim[clipName].weight = 1;
        anim.Sample();
        anim[clipName].enabled = false;
    }
    
    public static void SafeAddAnimClip(this Animation anim, AnimationClip animClip)
    {
        if (anim == null || animClip == null)
            return;
        
        var oldAnimClip = anim.GetClip(animClip.name);
        if (oldAnimClip == animClip)
            return;
        
        anim.RemoveClip(animClip.name);
        anim.AddClip(animClip, animClip.name);
    }

    public static void SafeAddAnimClip(this GameObject go, AnimationClip animClip)
    {
        if (go == null || animClip == null)
            return;

        var anim = go.GetOrAddComponent<Animation>() as Animation;
        anim.SafeAddAnimClip(animClip);
    }

    public static void AddListener(this Button.ButtonClickedEvent onClick, System.Action action)
    {
        if (onClick == null)
            return;
        
        onClick.AddListener(new UnityAction(action));
    }
    
    
    public static void AddListener(this ButtonEx.ButtonClickedEvent onClick, System.Action<ButtonEx.ButtonStateTypes> action)
    {
        if (onClick == null)
            return;
        
        onClick.AddListener(new UnityAction<ButtonEx.ButtonStateTypes>(action));
    }

    public static AnimatorState FindState(this AnimatorController animCtrl, string stateName, int nIdx_Layer = -1)
    {
        if (animCtrl == null || animCtrl.layers == null)
            return null;

        if (nIdx_Layer == -1)
        {
            foreach (var layer in animCtrl.layers)
            {
                var result = FindState(layer.stateMachine, stateName);
                if (result != null)
                    return result;
            }
        }

        if (animCtrl.layers.CheckIndex(nIdx_Layer))
            return animCtrl.layers[nIdx_Layer].stateMachine.FindState(stateName);

        return null;
    }

    public static AnimatorState FindState(this AnimatorStateMachine stateMachine, string stateName)
    {
        if (stateMachine == null)
            return null;

        if (stateMachine.states != null)
        {
            foreach (var childAnimatorState in stateMachine.states)
            {
                if (childAnimatorState.state.name == stateName)
                    return childAnimatorState.state;
            }
        }

        if (stateMachine.stateMachines != null)
        {
            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                var result = FindState(childStateMachine.stateMachine, stateName);
                if (result != null)
                    return result;
            }
        }
        
        return null;
    }

    public static float GetMotionMinLength(this Motion motion, Dictionary<AnimationClip, AnimationClip> overrides = null)
    {
        if (motion == null)
            return 0f;
        
        switch (motion)
        {
            case AnimationClip clip:
            {
                if (clip == null)
                    return 0f;
                        
                if (overrides == null)
                    return clip.length;

                return overrides.TryGetValue(clip, out var overrideClip) && overrideClip != null
                    ? overrideClip.length
                    : clip.length;   
            }
            case BlendTree tree:
            {
                var result = float.MaxValue;
                foreach (var childMotion in tree.children)
                {
                    var fChildLength = GetMotionMinLength(childMotion.motion, overrides);
                    if (fChildLength > float.Epsilon
                        && fChildLength < result)
                    {
                        result = fChildLength;
                    }
                }

                if (result == float.MaxValue)
                    return 0f;
                
                return result;   
            }
        }

        return 0f;
    }

    public static float GetMotionMaxLength(this Motion motion)
    {
        if (motion == null)
            return 0f;
        
        switch (motion)
        {
            case AnimationClip clip:
                return clip.length;
            case BlendTree tree:
                var result = 0f;
                foreach (var childMotion in tree.children)
                {
                    var fChildLength = GetMotionMinLength(childMotion.motion);
                    if (fChildLength > float.Epsilon
                        && fChildLength > result)
                    {
                        result = fChildLength;
                    }
                }
                return result;
        }

        return 0f;
    }
}