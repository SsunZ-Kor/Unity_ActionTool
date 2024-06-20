using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UGUIParentPerfectImage : UGUIParentPerfect
{
    [Header("Target (Auto)")]
    [SerializeField][Tooltip("타겟 Image 컴포넌트 (부착 시 최초 1회 자동 세팅)")]
    private Image _uiImg_Target;

    protected override float AspectTarget 
    {
        get
        {
            if (_uiImg_Target == null || _uiImg_Target.sprite == null)
                return 1f;

            return _uiImg_Target.mainTexture.width / (float) _uiImg_Target.mainTexture.height;
        }
    }

    protected override void Awake()
    {
        if (_uiImg_Target == null)
            _uiImg_Target = this.GetComponent(typeof(Image)) as Image;
    }
}