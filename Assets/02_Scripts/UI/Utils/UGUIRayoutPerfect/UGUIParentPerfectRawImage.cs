using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class UGUIParentPerfectRawImage : UGUIParentPerfect
{
    [Header("Target (Auto)")]
    [SerializeField][Tooltip("타겟 RawImage 컴포넌트 (부착 시 최초 1회 자동 세팅)")]
    private RawImage _uiImg_Target;

    public RawImage uiImg_Target => _uiImg_Target;

    protected override float AspectTarget
    {
        get
        {
            if (_uiImg_Target == null || _uiImg_Target.mainTexture == null)
                return 1f;

            return _uiImg_Target.mainTexture.width / (float)_uiImg_Target.mainTexture.height;
        }
    }

    protected override void Awake()
    {
        if (_uiImg_Target == null)
            _uiImg_Target = this.GetComponent(typeof(RawImage)) as RawImage;
    }
}