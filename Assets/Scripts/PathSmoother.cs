using UnityEngine;
using System.Collections.Generic;

public class PathSmoother : MonoBehaviour
{
    [Header("--- 路径可视化与平滑设置 ---")]
    public LineRenderer pathLineRenderer;
    
    [Range(0.1f, 10f)] 
    [Tooltip("期望的内切圆角半径")]
    public float turnRadius = 1.0f; 
    
    [Range(2, 20)] 
    [Tooltip("每个圆弧的顶点数量（数值越大越圆滑）")]
    public int arcResolution = 10; 
    
    [Tooltip("是否将首尾连接形成闭合赛道")]
    public bool isClosedLoop = true;
    
    public bool showPathLine = true;

    // 外部读取的最终路线
    public List<Vector3> SmoothedWaypoints { get; private set; } = new List<Vector3>();

    /// <summary>
    /// 生成标准内切圆角（Fillet）的闭合赛道
    /// </summary>
    public void GenerateFilletPath(List<Vector3> rawPoints, float fixedHeight)
    {
        SmoothedWaypoints.Clear();

        // 如果打点少于3个，无法形成多边形闭合环，直接返回原点
        if (rawPoints == null || rawPoints.Count < 3)
        {
            if (pathLineRenderer != null) pathLineRenderer.positionCount = 0;
            if (rawPoints != null) SmoothedWaypoints.AddRange(rawPoints);
            return;
        }

        List<Vector3> filletPathPoints = new List<Vector3>();
        int pointCount = rawPoints.Count;

        // 遍历所有打点（包含将最后一个点与第一个点连接的闭合逻辑）
        for (int i = 0; i < pointCount; i++)
        {
            // 利用取模运算 % 获取 前一个、当前、后一个点，完美实现首尾闭合
            Vector3 prev = rawPoints[(i - 1 + pointCount) % pointCount];
            Vector3 current = rawPoints[i];
            Vector3 next = rawPoints[(i + 1) % pointCount];

            prev.y = fixedHeight; 
            current.y = fixedHeight; 
            next.y = fixedHeight;

            // 获取从当前顶点“向外”指出的两个方向向量 (对应你图中的直线切段)
            Vector3 d1 = (prev - current).normalized;
            Vector3 d2 = (next - current).normalized;

            float dotProduct = Vector3.Dot(d1, d2);
            
            // 如果三个点几乎在一条直线上，无需倒角，直接加入原点
            if (dotProduct > 0.999f || dotProduct < -0.999f)
            {
                filletPathPoints.Add(current);
                continue;
            }

            // --- 核心内切圆角几何计算 ---
            float angleRad = Mathf.Acos(dotProduct);
            float halfAngle = angleRad / 2f;

            // 【关键防呆设计】：计算当前线段允许的最大切线距离（不超过线段长度的一半）
            // 如果不加这个限制，当设置的半径过大时，路径会发生扭曲交叉
            float maxDist1 = Vector3.Distance(prev, current) / 2f;
            float maxDist2 = Vector3.Distance(next, current) / 2f;
            float maxAllowedTangentDist = Mathf.Min(maxDist1, maxDist2);

            // 理论上的切点距离 ( D = R / tan(θ/2) )
            float theoreticalTangentDist = turnRadius / Mathf.Tan(halfAngle);

            // 限制切点距离，并反推实际使用的安全半径
            float actualTangentDist = Mathf.Min(theoreticalTangentDist, maxAllowedTangentDist);
            float actualRadius = actualTangentDist * Mathf.Tan(halfAngle);

            // 1. 求出两个确切的切点坐标
            Vector3 tangent1 = current + d1 * actualTangentDist; // 进弯点
            Vector3 tangent2 = current + d2 * actualTangentDist; // 出弯点

            // 2. 求出内切圆的圆心
            // 将两个向外的方向相加，恰好得到指向夹角内部的平分线方向
            Vector3 bisectorDir = (d1 + d2).normalized; 
            float distToCenter = actualRadius / Mathf.Sin(halfAngle);
            Vector3 center = current + bisectorDir * distToCenter;

            // 3. 开始将计算好的点装入最终路径列表
            filletPathPoints.Add(tangent1);

            // 使用 Slerp 沿圆弧球面插值，完美画出从 tangent1 到 tangent2 的内弧
            Vector3 v1 = tangent1 - center;
            Vector3 v2 = tangent2 - center;
            
            for (int j = 1; j < arcResolution; j++)
            {
                float t = (float)j / arcResolution;
                // Slerp 会自动走两向量之间的最短圆弧
                Vector3 arcVec = Vector3.Slerp(v1, v2, t);
                Vector3 arcPoint = center + arcVec;
                filletPathPoints.Add(arcPoint);
            }

            filletPathPoints.Add(tangent2);
        }

        SmoothedWaypoints = filletPathPoints;

        // --- 渲染到 LineRenderer ---
        if (pathLineRenderer != null)
        {
            pathLineRenderer.gameObject.SetActive(showPathLine);
            pathLineRenderer.positionCount = SmoothedWaypoints.Count;
            pathLineRenderer.SetPositions(SmoothedWaypoints.ToArray());
            
            // 这一句控制了视觉上路线的首尾相连
            pathLineRenderer.loop = isClosedLoop; 
        }
    }
}