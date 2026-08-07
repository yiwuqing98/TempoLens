using UnityEngine;
using System.Collections.Generic;

public class AimTrackGenerator : MonoBehaviour
{
    public static AimTrackGenerator Instance { get; private set; }

    [Header("--- 视觉辅助与打点 ---")]
    public GameObject waypointMarkerPrefab;
    [Tooltip("可选：拖入一个带LineRenderer的空物体，用于第一步和第二步之间的激光瞄准")]
    public LineRenderer aimLine; 

    [Header("--- 赛道尺寸参数 ---")]
    [Tooltip("直道总长度 (米)，从 A 点开始算")]
    public float lengthCD = 100.0f;
    [Tooltip("赛道宽度 (米)")]
    public float widthDE = 1.2f;
    [Tooltip("返程直道总长度 (米)")]
    public float lengthEF = 100.0f;

    public bool IsReady => clickCount >= 2; 

    private Vector3 pointA;
    private Vector3 lockedDirection;
    private int clickCount = 0; 
    private List<GameObject> spawnedMarkers = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (aimLine != null) aimLine.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (clickCount == 1 && Camera.main != null && aimLine != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0; 
            
            if (camForward != Vector3.zero)
            {
                aimLine.SetPosition(0, pointA);
                aimLine.SetPosition(1, pointA + camForward.normalized * lengthCD);
            }
        }
    }

    public void OnClickRecord()
    {
        if (Camera.main == null) return;

        Vector3 currentPos = Camera.main.transform.position;
        currentPos.y = 0; 

        if (clickCount == 0)
        {
            // --- 第 1 次点击：确定 A 点位置 ---
            pointA = currentPos;
            SpawnMarker(pointA, "A (起跑点)");
            clickCount++;
            
            if (aimLine != null)
            {
                aimLine.gameObject.SetActive(true);
                aimLine.positionCount = 2;
            }
            
            Debug.Log("[AimTrackGenerator] 起点 A 已记录！请用视线瞄准尽头，然后再次点击。");
        }
        else if (clickCount == 1)
        {
            // --- 第 2 次点击：确定方向 ---
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0; 
            if (camForward == Vector3.zero) camForward = Vector3.forward;

            lockedDirection = camForward.normalized;

            if (aimLine != null) aimLine.gameObject.SetActive(false);

            clickCount++;
            Debug.Log("[AimTrackGenerator] 方向已锁定！正在自动生成 100 米赛道...");
            GeneratePolygonPoints();
        }
        else
        {
            // --- 第 3 次点击：瞄歪了，清除重置 ---
            Debug.Log("[AimTrackGenerator] 第 3 次点击：检测到重新校准需求，自动清空所有数据！可以重新记录 A 点。");
            
            // 优先调用 PacePointManager 的全局清理（等同于点击了 UI 上的 Clear 按钮，最安全）
            if (PacePointManager.Instance != null)
            {
                PacePointManager.Instance.ClearWaypoints();
            }
            else
            {
                // 如果场景里没有主控，就单独清空自己和记录器
                ResetRectangle();
                if (WaypointRecorder.Instance != null) WaypointRecorder.Instance.ClearAllPoints();
            }
        }
    }

    private void GeneratePolygonPoints()
    {
        // 1. D 点 (从 A 沿锁定方向向前平移 100 米)
        Vector3 pointD = pointA + lockedDirection * lengthCD;
        SpawnMarker(pointD, "D (尽头折返点)");

        // 2. E 点 (向左转 90 度，平移 1.2 米)
        Vector3 dirLeftDE = Quaternion.Euler(0, -90.0f, 0) * lockedDirection;
        Vector3 pointE = pointD + dirLeftDE * widthDE;
        SpawnMarker(pointE, "E");

        // 3. F 点 (再向左转 90 度，即往回走 100 米)
        Vector3 dirLeftEF = Quaternion.Euler(0, -90.0f, 0) * dirLeftDE;
        Vector3 pointF = pointE + dirLeftEF * lengthEF;
        SpawnMarker(pointF, "F");

        // 4. 同步至全局记录器 (喂给平滑算法)
        if (WaypointRecorder.Instance != null)
        {
            WaypointRecorder.Instance.recordedWorldPoints.Clear();
            
            // 纯长方形：A -> D -> E -> F
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointA); 
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointD); 
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointE);
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointF);
            
            Debug.Log("[AimTrackGenerator] 已将 A, D, E, F 同步至全局 Waypoint 列表！");
        }
    }

    private void SpawnMarker(Vector3 pos, string markerName)
    {
        if (waypointMarkerPrefab != null)
        {
            GameObject marker = Instantiate(waypointMarkerPrefab, pos, Quaternion.identity);
            marker.name = "Point_" + markerName;
            spawnedMarkers.Add(marker);
        }
    }

    public void ResetRectangle()
    {
        clickCount = 0; // 重置点击次数
        foreach (var marker in spawnedMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        spawnedMarkers.Clear();
        
        if (aimLine != null) aimLine.gameObject.SetActive(false);

        Debug.Log("[AimTrackGenerator] 数据已清空。");
    }
}