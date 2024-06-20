using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public abstract class UGUIParentPerfect : UIBehaviour, ILayoutSelfController
{
    public enum ModeTypes
    {
        Width,
        Height,
        Inscription,
        Circumscription,
    }

    public enum TargetType
    {
        Parent,
        Canvas,
    }

    [Header("Settings")]
    [SerializeField] [Tooltip("모드 설정\n\n[Width] 너비\n[Height] 높이\n[Inscribed] 내접\n[Circumscription] 외접")]
    private ModeTypes _modeType = ModeTypes.Circumscription;
    [SerializeField][Tooltip("타겟 설정\n\n[Parent] 부모 오브젝트 RectTransform\n[Canvas] Canvas RectTransform")]
    private TargetType _targetType = TargetType.Canvas;

    protected abstract float AspectTarget { get; }
    private bool _isUpdating = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        Update_SizeDelta();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        Update_SizeDelta();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        Update_SizeDelta();
    }

    public void Update_SizeDelta()
    {
        if (_isUpdating)
            return;

        var rttr_Mine = this.transform as RectTransform;
        if (rttr_Mine == null)
            return;

        rttr_Mine.anchorMin = new Vector2(0.5f, 0.5f);
        rttr_Mine.anchorMax = new Vector2(0.5f, 0.5f);

        RectTransform rttr_Parent = null;
        if (_targetType == TargetType.Canvas)
        {
            rttr_Parent = FindRootComponent<Canvas>(rttr_Mine)?.transform as RectTransform;
        }
        else if (_targetType == TargetType.Parent)
        {
            rttr_Parent = this.transform.parent as RectTransform;
        }

        if (rttr_Parent == null)
            return;

        _isUpdating = true;

        rttr_Mine.pivot = rttr_Parent.pivot;
        rttr_Mine.localPosition = Vector3.zero;

        float aspect_Parent = rttr_Parent.rect.width / rttr_Parent.rect.height;
        float aspect_Target = AspectTarget;

        switch (_modeType)
        {
            case ModeTypes.Width:
                {
                    rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.width, rttr_Parent.rect.width / aspect_Target);
                }
                break;
            case ModeTypes.Height:
                {
                    rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.height * aspect_Target, rttr_Parent.rect.height);
                }
                break;
            case ModeTypes.Inscription:
                {
                    // 높이에 맞춤
                    if (aspect_Target < aspect_Parent)
                    {
                        rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.height * aspect_Target, rttr_Parent.rect.height);
                    }
                    // 너비에 맞춤
                    else
                    {
                        rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.width, rttr_Parent.rect.width / aspect_Target);
                    }
                }
                break;
            case ModeTypes.Circumscription:
                {
                    // 높이에 맞춤
                    if (aspect_Target > aspect_Parent)
                    {
                        rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.height * aspect_Target, rttr_Parent.rect.height);
                    }
                    // 너비에 맞춤
                    else
                    {
                        rttr_Mine.sizeDelta = new Vector2(rttr_Parent.rect.width, rttr_Parent.rect.width / aspect_Target);
                    }
                }
                break;
        }

        var newPos = rttr_Parent.position;
        newPos.z = this.transform.position.z;

        this.transform.position = newPos;

        _isUpdating = false;
    }

    public T FindRootComponent<T>(Component target) where T : Component
    {
        T result = null;

        while (true)
        {
            var parentComponent = target.GetComponentInParent(typeof(T), true) as T;
            if (parentComponent == null)
                break;

            target = parentComponent.transform.parent;
            result = parentComponent;

            if (target == null)
                break;
        }

        return result;
    }

    public void SetLayoutHorizontal()
    {
        // 어차피 둘다 들어온다. 한군데서만 처리하자
        // Update_SizeDelta();
    }

    public void SetLayoutVertical()
    {
        Update_SizeDelta();
    }
}