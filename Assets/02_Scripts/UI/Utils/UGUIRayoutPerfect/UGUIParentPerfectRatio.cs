using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UGUIParentPerfectRatio : UGUIParentPerfect
{
    [Header("Target Resolution")]
    [SerializeField] [Tooltip("비율 직접 입력")]
    private Vector2 resolution = new Vector2(16f, 9f);

    protected override float AspectTarget
    {
        get
        {
            if (resolution.x < float.Epsilon || resolution.y < float.Epsilon)
                return 1f;

            return resolution.x / resolution.y;
        }
    }

    public void SetResolution(Vector2 pResolution, bool pForcedUpdate)
    {
        SetResolution(pResolution.x, pResolution.y, pForcedUpdate);
    }
    
    public void SetResolution(float width, float height, bool pForcedUpdate)
    {
        resolution.x = width;
        resolution.y = height;
        
        if (pForcedUpdate)
            Update_SizeDelta();
    }
}