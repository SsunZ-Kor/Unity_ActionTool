using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class UGUICanvasPerfect : UIBehaviour, ILayoutSelfController
{
    [Header("Size Settings")]
    [SerializeField]
    [Tooltip("RectTransform 최대 사이즈 설정")]
    private Vector2 _maxResolution = Vector2.positiveInfinity;
    [SerializeField]
    [Tooltip("RectTransform 갱신 시 SafeArea 반영 여부")]
    private bool _useSafeArea = true;

    private RectTransform _rttr_Canvas = null;
    private bool _isUpdating = false;

#if UNITY_EDITOR
    public class UpdateResultInfo
    {
        public bool _isEditor;
        public Vector2 _vResolution;
        public Rect _vSafeArea;
        public Vector2 _vPosOffset;
        public Vector2 _vSizeDelta;

        public void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal("box");
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("환경", GUILayout.Width(80));
                    EditorGUILayout.LabelField("해상도", GUILayout.Width(80));
                    EditorGUILayout.LabelField("SafeArea", GUILayout.Width(80));
                    EditorGUILayout.LabelField("PosOffset", GUILayout.Width(80));
                    EditorGUILayout.LabelField("SizeDelta", GUILayout.Width(80));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField(" : ", GUILayout.Width(20));
                    EditorGUILayout.LabelField(" : ", GUILayout.Width(20));
                    EditorGUILayout.LabelField(" : ", GUILayout.Width(20));
                    EditorGUILayout.LabelField(" : ", GUILayout.Width(20));
                    EditorGUILayout.LabelField(" : ", GUILayout.Width(20));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField(_isEditor ? "에디터" : "시뮬레이터");
                    EditorGUILayout.LabelField($"{_vResolution}");
                    EditorGUILayout.LabelField($"{_vSafeArea}");
                    EditorGUILayout.LabelField($"{_vPosOffset}");
                    EditorGUILayout.LabelField($"{_vSizeDelta}");
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    [System.NonSerialized]
    public UpdateResultInfo updateResultInfo;
#endif

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
        if (_isUpdating || !isActiveAndEnabled)
            return;

        // Canvas의 RectTransform 찾기
        var canvas = FindRootComponent<Canvas>(_rttr_Canvas == null || this.transform.IsChildOf(_rttr_Canvas)
            ? this
            : _rttr_Canvas);

        if (canvas == null || !canvas.isActiveAndEnabled)
            return;

        _rttr_Canvas = canvas.transform as RectTransform;

        // Holder의 Anchor 다시 잡기
        var rttr_Mine = this.transform as RectTransform;
        if (rttr_Mine == null)
            return;

        _isUpdating = true;

        var vCanvasPos = _rttr_Canvas.position;
        var vCanvasSize = _rttr_Canvas.sizeDelta;

        if (vCanvasSize.x == 0f || float.IsNaN(vCanvasSize.x)
            || vCanvasSize.y == 0f || float.IsNaN(vCanvasSize.y))
        {
            _isUpdating = false;
            return;
        }

        if (_useSafeArea)
        {
#if UNITY_EDITOR
            var res = UnityEditor.UnityStats.screenRes.Split('x');
            var rt_ScreenArea = new Rect(0, 0, int.Parse(res[0]), int.Parse(res[1]));
            var rt_SafeArea = UnityEngine.Device.Application.isEditor ? rt_ScreenArea : Screen.safeArea;
#else
            var rt_ScreenArea = new Rect(0, 0, Screen.width, Screen.height);
            var rt_SafeArea = Screen.safeArea;
#endif
            if (rt_ScreenArea.width == 0f || float.IsNaN(rt_ScreenArea.width)
                || rt_ScreenArea.height == 0f || float.IsNaN(rt_ScreenArea.height)
                || rt_SafeArea.width == 0f || float.IsNaN(rt_SafeArea.width)
                || rt_SafeArea.height == 0f || float.IsNaN(rt_SafeArea.height))
            {
                _isUpdating = false;
                return;
            }

            var factorX_ScreenToCanvas = vCanvasSize.x / rt_ScreenArea.width;
            var factorY_ScreenToCanvas = vCanvasSize.y / rt_ScreenArea.height;

            var posOffset = rt_SafeArea.center - rt_ScreenArea.center;
            var newSizeDelta = rt_SafeArea.size;

            posOffset.x *= factorX_ScreenToCanvas;
            posOffset.y *= factorY_ScreenToCanvas;
            newSizeDelta.x *= factorX_ScreenToCanvas;
            newSizeDelta.y *= factorY_ScreenToCanvas;

            if (newSizeDelta.x >= _maxResolution.x)
                newSizeDelta.x = _maxResolution.x;
            if (newSizeDelta.y >= _maxResolution.y)
                newSizeDelta.y = _maxResolution.y;

            rttr_Mine.position = vCanvasPos;
            rttr_Mine.anchoredPosition += posOffset;
            rttr_Mine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSizeDelta.x);
            rttr_Mine.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeDelta.y);

#if UNITY_EDITOR
            updateResultInfo ??= new();
            updateResultInfo._isEditor = UnityEngine.Device.Application.isEditor;
            updateResultInfo._vResolution = rt_ScreenArea.size;
            updateResultInfo._vSafeArea = rt_SafeArea;
            updateResultInfo._vPosOffset = posOffset;
            updateResultInfo._vSizeDelta = newSizeDelta;
#endif
        }
        else
        {
            if (vCanvasSize.x >= _maxResolution.x)
                vCanvasSize.x = _maxResolution.x;
            if (vCanvasSize.y >= _maxResolution.y)
                vCanvasSize.y = _maxResolution.y;

            rttr_Mine.position = vCanvasPos;
            rttr_Mine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vCanvasSize.x);
            rttr_Mine.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vCanvasSize.y);

#if UNITY_EDITOR
            updateResultInfo = null;
#endif
        }

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