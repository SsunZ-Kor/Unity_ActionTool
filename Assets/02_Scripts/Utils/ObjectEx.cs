using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectEx
{
    public static void SetColor(this Texture2D tex2D, Color color)
    {
        var fillColorArray = tex2D.GetPixels32();

        for (var i = 0; i < fillColorArray.Length; ++i)
            fillColorArray[i] *= color;

        tex2D.SetPixels32(fillColorArray);
        tex2D.Apply();
    }

    public static Mesh CreateSphere(int segmentsForCone, int segmentsForSphere, float radius, float angle)
    {
        if (angle <= float.Epsilon || 360 <= angle)
            return Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

        if (radius <= float.Epsilon)
            return null;
        
        Mesh mesh = new Mesh();

        int verticesCount = segmentsForCone * segmentsForSphere + 2;
        Vector3[] vertices = new Vector3[verticesCount];

        int trianglesCount = segmentsForCone * (segmentsForSphere * 2) * 3;
        int[] triangles = new int[trianglesCount];
        
        // 중심 버택스 생성
        var topCenterIndex = verticesCount - 2;
        var botCenterIndex = verticesCount - 1;

        vertices[topCenterIndex] = Vector3.zero;
        vertices[botCenterIndex] = Vector3.forward * radius;
        
        // 밑바닥 버텍스 생성
        var vForward = Quaternion.Euler(-angle * 0.5f,0f, 0f) * vertices[botCenterIndex];
        var fDeltaAngle_Cone = 360f / segmentsForCone;
        
        for (int i = 0; i < segmentsForCone; ++i)
        {
            var vStart = Quaternion.Euler(0f, 0f, fDeltaAngle_Cone * i) * vForward;
            for (int j = 0; j < segmentsForSphere; ++j)
            {
                var index = (i * segmentsForSphere) + j;
                vertices[index] = Vector3.Slerp(vStart, vertices[botCenterIndex], j / (float)segmentsForSphere);
            }
        }

        for (int i = 0; i < segmentsForCone; ++i)
        {
            var offset = i * (segmentsForSphere * 2) * 3;

            var startIndex_curr = i * segmentsForSphere;
            var startIndex_next = ((i + 1) % segmentsForCone ) * segmentsForSphere;
            
            // 뿔 폴리곤 생성
            triangles[offset] = topCenterIndex;
            triangles[offset + 1] = startIndex_next;
            triangles[offset + 2] = startIndex_curr;
            
            triangles[offset + 3] = botCenterIndex;
            triangles[offset + 4] = startIndex_curr + segmentsForSphere - 1;
            triangles[offset + 5] = startIndex_next + segmentsForSphere - 1;

            offset += 6;

            for (int j = 0; j < segmentsForSphere - 1; ++j, offset += 6)
            {
                triangles[offset] = startIndex_curr + j;
                triangles[offset + 1] = startIndex_next + j;
                triangles[offset + 2] = startIndex_curr + j + 1;
                
                triangles[offset + 3] = startIndex_next + j;
                triangles[offset + 4] = startIndex_next + j + 1;
                triangles[offset + 5] = startIndex_curr + j + 1;
            }
        }

        for (int i = 0; i < triangles.Length; ++i)
        {
            if (triangles[i] >= verticesCount)
            {
                Debug.LogError(i);
            }
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        
        return mesh;
    }

    public static Mesh CreateCylinder(int segments, float radius, float height)
    {
        if (height <= float.Epsilon || radius <= float.Epsilon)
            return null;
        
        Mesh mesh = new Mesh();

        int verticesCount = segments * 2 + 2;
        Vector3[] vertices = new Vector3[verticesCount];

        int trianglesCount = segments * 12;
        int[] triangles = new int[trianglesCount];

        float deltaAngle = 360f / segments;

        float top = height * 0.5f;
        float bot = -top;

        // 중심 버택스 생성
        var topCenterIndex = verticesCount - 2;
        var botCenterIndex = verticesCount - 1;

        vertices[topCenterIndex] = new Vector3(0f, top, 0f);
        vertices[botCenterIndex] = new Vector3(0f, bot, 0f);
        
        // 원의 버텍스 생성
        for (int i = 0; i < segments; ++i)
        {
            float currentAngle = (deltaAngle * i);

            float x = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * radius;

            vertices[i] = new Vector3(x, top, z);
            vertices[i + segments] = new Vector3(x, bot, z);
        }


        for (int i = 0; i < segments; i++)
        {
            int offset = i * 12;
            int topIndex1 = i;
            int topIndex2 = (topIndex1 + 1) % segments;
            int botIndex1 = i + segments;
            int botIndex2 = (botIndex1 + 1) % segments * 2;

            /* 부채꼴 기둥의 삼각형 생성 */
            triangles[offset] = topIndex1;
            triangles[offset + 1] = topIndex2;
            triangles[offset + 2] = botIndex1;

            triangles[offset + 3] = botIndex1;
            triangles[offset + 4] = topIndex2;
            triangles[offset + 5] = botIndex2;

            /* 부채꼴 윗면의 삼각형 생성 */
            triangles[offset + 6] = topCenterIndex;
            triangles[offset + 7] = topIndex2;
            triangles[offset + 8] = topIndex1;

            /* 부채꼴 아랫면의 삼각형 생성 */
            triangles[offset + 9] = botCenterIndex;
            triangles[offset + 10] = botIndex1;
            triangles[offset + 11] = botIndex2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh CreateCylinder(int segments, float radius, float height, float angle)
    {
        if (angle <= float.Epsilon || 360 <= angle)
            return CreateCylinder(segments, radius, height);

        if (height <= float.Epsilon || radius <= float.Epsilon)
            return null;
        
        Mesh mesh = new Mesh();

        int verticesCount = (segments + 1) * 2 + 2;
        Vector3[] vertices = new Vector3[verticesCount];

        int trianglesCount = (segments + 1) * 12;
        int[] triangles = new int[trianglesCount];

        float startAngle = -angle * 0.5f + 90f;
        float deltaAngle = angle / segments;

        float top = height * 0.5f;
        float bot = -top;

        // 원의 버텍스 생성
        for (int i = 0; i <= segments; ++i)
        {
            float currentAngle = startAngle + deltaAngle * i;

            float x = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * radius;

            vertices[i] = new Vector3(x, top, z);
            vertices[i + segments + 1] = new Vector3(x, bot, z);
        }

        var topCenterIndex = verticesCount - 2;
        var botCenterIndex = verticesCount - 1;

        vertices[topCenterIndex] = new Vector3(0f, top, 0f);
        vertices[botCenterIndex] = new Vector3(0f, bot, 0f);

        for (int i = 0; i < segments; i++)
        {
            int offset = i * 12;
            int topIndex1 = i;
            int topIndex2 = topIndex1 + 1;
            int botIndex1 = i + segments + 1;
            int botIndex2 = botIndex1 + 1;

            /* 부채꼴 기둥의 삼각형 생성 */
            triangles[offset] = topIndex1;
            triangles[offset + 1] = topIndex2;
            triangles[offset + 2] = botIndex1;

            triangles[offset + 3] = botIndex1;
            triangles[offset + 4] = topIndex2;
            triangles[offset + 5] = botIndex2;

            /* 부채꼴 윗면의 삼각형 생성 */
            triangles[offset + 6] = topCenterIndex;
            triangles[offset + 7] = topIndex2;
            triangles[offset + 8] = topIndex1;

            /* 부채꼴 아랫면의 삼각형 생성 */
            triangles[offset + 9] = botCenterIndex;
            triangles[offset + 10] = botIndex1;
            triangles[offset + 11] = botIndex2;
        }

        /* 부채꼴 벽면의 삼각형 생성 */

        triangles[trianglesCount - 12] = segments + 1;
        triangles[trianglesCount - 11] = topCenterIndex;
        triangles[trianglesCount - 10] = 0;

        triangles[trianglesCount - 9] = segments + 1;
        triangles[trianglesCount - 8] = botCenterIndex;
        triangles[trianglesCount - 7] = topCenterIndex;

        triangles[trianglesCount - 6] = botCenterIndex;
        triangles[trianglesCount - 5] = segments;
        triangles[trianglesCount - 4] = topCenterIndex;

        triangles[trianglesCount - 3] = botCenterIndex;
        triangles[trianglesCount - 2] = segments + 1 + segments;
        triangles[trianglesCount - 1] = segments;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }

    public static Mesh CreateHollowCylinder(int segments, float outerSurfaceRadius, float innerSurfaceRadius, float height)
    {
        if (outerSurfaceRadius <= float.Epsilon || height <= float.Epsilon)
            return null;
        
        if (innerSurfaceRadius > outerSurfaceRadius)
            innerSurfaceRadius = outerSurfaceRadius;

        Mesh mesh = new Mesh();

        int verticesCount = (segments) * 4;
        Vector3[] vertices = new Vector3[verticesCount];

        int trianglesCount = segments * 24;
        int[] triangles = new int[trianglesCount];

        float deltaAngle = 360f / segments;

        float top = height * 0.5f;
        float bot = -top;

        // 원의 버텍스 생성
        for (int i = 0; i < segments; ++i)
        {
            var offset = i * 4;
            
            float currentAngle = deltaAngle * i;

            float x = Mathf.Cos(Mathf.Deg2Rad * currentAngle);
            float z = Mathf.Sin(Mathf.Deg2Rad * currentAngle);

            float xIn = x * innerSurfaceRadius;
            float zIn = z * innerSurfaceRadius;
            float xOut = x * outerSurfaceRadius;
            float zOut = z * outerSurfaceRadius;

            vertices[offset] = new Vector3(xIn, top, zIn);          // InTop
            vertices[offset + 1] = new Vector3(xOut, top, zOut);    // OutTop
            
            vertices[offset + 2] = new Vector3(xIn, bot, zIn);      // InBot
            vertices[offset + 3] = new Vector3(xOut, bot, zOut);    // OutBot
        }

        for (int i = 0; i < segments; i++)
        {
            int offset = i * 24;
            var startIndex1 = i * 4;
            var startIndex2 = ((i + 1) % segments) * 4;
            
            int innerTopIndex1 = startIndex1;
            int innerBotIndex1 = startIndex1 + 2;
            int outerTopIndex1 = startIndex1 + 1;
            int outerBotIndex1 = startIndex1 + 3;

            int innerTopIndex2 = startIndex2;
            int innerBotIndex2 = startIndex2 + 2;
            int outerTopIndex2 = startIndex2 + 1;
            int outerBotIndex2 = startIndex2 + 3;
            
            /* 부채꼴 안쪽면의 삼각형 생성 */
            triangles[offset] = innerBotIndex1;
            triangles[offset + 1] = innerTopIndex2;
            triangles[offset + 2] = innerTopIndex1;
            
            triangles[offset + 3] = innerTopIndex2;
            triangles[offset + 4] = innerBotIndex1;
            triangles[offset + 5] = innerBotIndex2;
            
            /* 부채꼴 바깥면의 삼각형 생성 */
            triangles[offset + 6] = outerBotIndex1;
            triangles[offset + 7] = outerTopIndex1;
            triangles[offset + 8] = outerTopIndex2;
            
            triangles[offset + 9] = outerTopIndex2;
            triangles[offset + 10] = outerBotIndex2;
            triangles[offset + 11] = outerBotIndex1;

            /* 부채꼴 윗면의 삼각형 생성 */
            triangles[offset + 12] = innerTopIndex1;
            triangles[offset + 13] = outerTopIndex2;
            triangles[offset + 14] = outerTopIndex1;
            
            triangles[offset + 15] = outerTopIndex2;
            triangles[offset + 16] = innerTopIndex1;
            triangles[offset + 17] = innerTopIndex2;

            /* 부채꼴 아랫면의 삼각형 생성 */
            triangles[offset + 18] = innerBotIndex1;
            triangles[offset + 19] = outerBotIndex1;
            triangles[offset + 20] = outerBotIndex2;
            
            triangles[offset + 21] = outerBotIndex2;
            triangles[offset + 22] = innerBotIndex2;
            triangles[offset + 23] = innerBotIndex1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }
    
    public static Mesh CreateHollowCylinder(int segments, float outerSurfaceRadius, float innerSurfaceRadius, float height, float angle)
    {
        if (innerSurfaceRadius <= float.Epsilon)
            return CreateCylinder(segments, outerSurfaceRadius, height, angle);

        if (angle <= float.Epsilon || 360f <= angle)
            return CreateHollowCylinder(segments, outerSurfaceRadius, innerSurfaceRadius, height);

        if (outerSurfaceRadius <= float.Epsilon || height <= float.Epsilon)
            return null;


        if (innerSurfaceRadius > outerSurfaceRadius)
            innerSurfaceRadius = outerSurfaceRadius;
        
        Mesh mesh = new Mesh();

        int verticesCount = (segments + 1) * 4;
        Vector3[] vertices = new Vector3[verticesCount];

        int trianglesCount = segments * 24 + 12;
        int[] triangles = new int[trianglesCount];

        float startAngle = -angle * 0.5f + 90f;
        float deltaAngle = angle / segments;

        float top = height * 0.5f;
        float bot = -top;

        // 원의 버텍스 생성
        for (int i = 0; i <= segments; ++i)
        {
            var offset = i * 4;
            
            float currentAngle = startAngle + deltaAngle * i;

            float x = Mathf.Cos(Mathf.Deg2Rad * currentAngle);
            float z = Mathf.Sin(Mathf.Deg2Rad * currentAngle);

            float xIn = x * innerSurfaceRadius;
            float zIn = z * innerSurfaceRadius;
            float xOut = x * outerSurfaceRadius;
            float zOut = z * outerSurfaceRadius;

            vertices[offset] = new Vector3(xIn, top, zIn);          // InTop
            vertices[offset + 1] = new Vector3(xOut, top, zOut);    // OutTop
            
            vertices[offset + 2] = new Vector3(xIn, bot, zIn);      // InBot
            vertices[offset + 3] = new Vector3(xOut, bot, zOut);    // OutBot
        }

        for (int i = 0; i < segments; i++)
        {
            int offset = i * 24;
            var startIndex1 = i * 4;
            var startIndex2 = (i + 1) * 4;
            
            int innerTopIndex1 = startIndex1;
            int innerBotIndex1 = startIndex1 + 2;
            int outerTopIndex1 = startIndex1 + 1;
            int outerBotIndex1 = startIndex1 + 3;

            int innerTopIndex2 = startIndex2;
            int innerBotIndex2 = startIndex2 + 2;
            int outerTopIndex2 = startIndex2 + 1;
            int outerBotIndex2 = startIndex2 + 3;
            
            /* 부채꼴 안쪽면의 삼각형 생성 */
            triangles[offset] = innerBotIndex1;
            triangles[offset + 1] = innerTopIndex2;
            triangles[offset + 2] = innerTopIndex1;
            
            triangles[offset + 3] = innerTopIndex2;
            triangles[offset + 4] = innerBotIndex1;
            triangles[offset + 5] = innerBotIndex2;
            
            /* 부채꼴 바깥면의 삼각형 생성 */
            triangles[offset + 6] = outerBotIndex1;
            triangles[offset + 7] = outerTopIndex1;
            triangles[offset + 8] = outerTopIndex2;
            
            triangles[offset + 9] = outerTopIndex2;
            triangles[offset + 10] = outerBotIndex2;
            triangles[offset + 11] = outerBotIndex1;

            /* 부채꼴 윗면의 삼각형 생성 */
            triangles[offset + 12] = innerTopIndex1;
            triangles[offset + 13] = outerTopIndex2;
            triangles[offset + 14] = outerTopIndex1;
            
            triangles[offset + 15] = outerTopIndex2;
            triangles[offset + 16] = innerTopIndex1;
            triangles[offset + 17] = innerTopIndex2;

            /* 부채꼴 아랫면의 삼각형 생성 */
            triangles[offset + 18] = innerBotIndex1;
            triangles[offset + 19] = outerBotIndex1;
            triangles[offset + 20] = outerBotIndex2;
            
            triangles[offset + 21] = outerBotIndex2;
            triangles[offset + 22] = innerBotIndex2;
            triangles[offset + 23] = innerBotIndex1;
        }

        /* 부채꼴 벽면의 삼각형 생성 */
        {
            int innerTopIndex = 0;
            int innerBotIndex = 2;
            int outerTopIndex = 1;
            int outerBotIndex = 3;
            
            triangles[trianglesCount - 12] = outerBotIndex;
            triangles[trianglesCount - 11] = innerBotIndex;
            triangles[trianglesCount - 10] = outerTopIndex;
        
            triangles[trianglesCount - 9] = outerTopIndex;
            triangles[trianglesCount - 8] = innerBotIndex;
            triangles[trianglesCount - 7] = innerTopIndex;
        }
        {
            int innerTopIndex = segments * 4;
            int innerBotIndex = innerTopIndex + 2;
            int outerTopIndex = innerTopIndex + 1;
            int outerBotIndex = innerTopIndex + 3;
            
            triangles[trianglesCount - 6] = innerTopIndex;
            triangles[trianglesCount - 5] = innerBotIndex;
            triangles[trianglesCount - 4] = outerBotIndex;
        
            triangles[trianglesCount - 3] = outerBotIndex;
            triangles[trianglesCount - 2] = outerTopIndex;
            triangles[trianglesCount - 1] = innerTopIndex;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }
}