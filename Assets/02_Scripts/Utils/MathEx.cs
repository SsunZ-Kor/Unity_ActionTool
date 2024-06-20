using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathEx
{
    public static float ClampAngle360(float fAngle)
    {
        fAngle %= 360;
        if (fAngle < 0f)
            fAngle += 360f;

        return fAngle;
    }
    
    public static float ClampAngle180(float fAngle)
    {
        fAngle %= 360;
        if (fAngle < -180f)
            fAngle += 360f;
        else if (fAngle > 180f)
            fAngle -= 360f;

        return fAngle;
    }

    public static Vector3 SlerpByAngle(Vector3 vFrom, Vector3 vTo, float fAngle)
    {
        var fAngleGap = Vector3.Angle(vFrom, vTo);
        if (fAngleGap <= float.Epsilon)
            return vTo;
        
        return Vector3.Slerp(vFrom, vTo,  Mathf.Clamp01(fAngle / fAngleGap));
    }
    
    public static Vector3 SlerpByAngleUnclamped(Vector3 vFrom, Vector2 vTo, float fAngle)
    {
        var fAngleGap = Vector3.Angle(vFrom, vTo);
        if (fAngleGap < float.Epsilon)
            return vTo;
        
        return Vector3.Slerp(vFrom, vTo, fAngle / fAngleGap);
    }
}