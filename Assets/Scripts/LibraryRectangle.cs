using UnityEngine;
using System.Collections.Generic;

public class LibraryRectangle : MonoBehaviour
{
    // --- 单例 Instance，方便跨脚本/按键调用 ---
    public static LibraryRectangle Instance { get; private set; }

    [Header("打点设置")]
    [Tooltip("用于在空间中标记位置的可见小球 Prefab")]
    public GameObject waypointMarkerPrefab;

    [Header("长方形/多边形尺寸参数")]
    [Tooltip("点 C 离点 B 的倒退距离（线段 AB 上）")]
    public float distanceBC = 4.0f;
    [Tooltip("点 C 到点 D 的距离（AB 延长线上）")]
    public float lengthCD = 83.0f;
    [Tooltip("点 D 到点 E 的左转距离")]
    public float widthDE = 41.0f;
    [Tooltip("点 E 到点 F 的左转距离")]
    public float lengthEF = 83.0f;

    // 内部状态记录
    private Vector3 pointA;
    private Vector3 pointB;
    private int clickCount = 0; // 记录按键次数

    // 存储当前脚本生成的实体小球，方便清空
    private List<GameObject> spawnedMarkers = new List<GameObject>();

    private void Awake()
    {
        // 初始化单例
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 绑定给手柄按键的方法。按第一次记录 A，按第二次记录 B 并瞬间生成 C、D、E、F。
    /// </summary>
    public void OnClickRecord()
    {
        if (Camera.main == null) return;

        // 获取当前相机所在的水平面坐标（强制 Y = 0）
        Vector3 currentPos = Camera.main.transform.position;
        currentPos.y = 0; 

        if (clickCount == 0)
        {
            // --- 第一步：记录点 A ---
            pointA = currentPos;
            SpawnMarker(pointA, "A (起点参考)");
            clickCount++;
            
            Debug.Log("[LibraryRectangle] 点 A 已记录！请朝着目标方向直线走动一段距离（需大于4米），然后再次按下按键记录点 B。");
        }
        else if (clickCount == 1)
        {
            // --- 第二步：记录点 B，并推导全图 ---
            pointB = currentPos;

            // 防呆：防止两次按键在原地按，或者距离小于 4 米导致 C 点无法在线段 AB 内
            float distanceAB = Vector3.Distance(pointA, pointB);
            if (distanceAB <= distanceBC)
            {
                Debug.LogWarning($"[LibraryRectangle] 点 B 距离点 A 太近（当前: {distanceAB:F2}米）！必须大于 {distanceBC} 米，请走远一点再按键！");
                return;
            }

            SpawnMarker(pointB, "B (方向参考)");
            clickCount++;
            
            Debug.Log("[LibraryRectangle] 点 B 已记录，开始自动推导点 C, D, E, F...");
            GeneratePolygonPoints();
        }
        else
        {
            Debug.LogWarning("[LibraryRectangle] 点位已经生成完毕！如需重新生成，请先调用 ResetRectangle()。");
        }
    }

    /// <summary>
    /// 核心数学逻辑：根据 A 和 B 的方向，推导 C, D, E, F
    /// </summary>
    private void GeneratePolygonPoints()
    {
        // 1. 获取从 A 指向 B 的“标准正前方向量”
        Vector3 dirAB = (pointB - pointA).normalized;

        // 2. 计算点 C：在线段 AB 上，离 B 点 4 米（从 B 向 A 的方向往回退 4 米）
        Vector3 pointC = pointB - dirAB * distanceBC;
        SpawnMarker(pointC, "C");

        // 3. 计算点 D：在 AB 延长线上，离开点 C 90 米
        Vector3 pointD = pointC + dirAB * lengthCD;
        SpawnMarker(pointD, "D");

        // 4. 计算点 E：到达 D 后向左转 90 度，前进 50 米
        // 使用 Quaternion.Euler(0, -90, 0) 将原方向绕 Y 轴向左旋转 90 度
        Vector3 dirLeftDE = Quaternion.Euler(0, (float)-88.5, 0) * dirAB;
        Vector3 pointE = pointD + dirLeftDE * widthDE;
        SpawnMarker(pointE, "E");

        // 5. 计算点 F：到达 E 后继续向左转 87 度，前进 90 米
        // 在朝向 E 的方向（dirLeftDE）基础上，再左转 87 度
        Vector3 dirLeftEF = Quaternion.Euler(0, (float)-91.5, 0) * dirLeftDE;
        Vector3 pointF = pointE + dirLeftEF * lengthEF;
        SpawnMarker(pointF, "F");

        // 6. 【联动】将最终的 waypoint 点（C, D, E, F）存入全局 Waypoint 列表
        if (WaypointRecorder.Instance != null)
        {
            // 根据你的需求，这次 A 和 B 只是参考点，真正作为 waypoint 的是 C, D, E, F
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointC);
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointD);
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointE);
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointF);
            
            Debug.Log("[LibraryRectangle] 成功将 C, D, E, F 四个目标点同步至全局 Waypoint 列表！");
        }
    }

    // 生成实体小球的辅助方法
    private void SpawnMarker(Vector3 pos, string markerName)
    {
        if (waypointMarkerPrefab != null)
        {
            GameObject marker = Instantiate(waypointMarkerPrefab, pos, Quaternion.identity);
            marker.name = "Point_" + markerName;
            spawnedMarkers.Add(marker);
        }
    }

    /// <summary>
    /// 绑定给“重置”按钮，清理数据以便重新记录
    /// </summary>
    public void ResetRectangle()
    {
        clickCount = 0;
        
        foreach (var marker in spawnedMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        spawnedMarkers.Clear();

        Debug.Log("[LibraryRectangle] 已重置，您可以重新从点 A 开始标记了。");
    }
}