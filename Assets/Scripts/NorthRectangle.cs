using UnityEngine;
using System.Collections.Generic;

public class NorthRectangle : MonoBehaviour
{
    public static NorthRectangle Instance { get; private set; }

    [Header("打点设置")]
    public GameObject waypointMarkerPrefab;

    [Header("长方形/多边形尺寸参数")]
    public float minDistanceAB = 4.0f; // 改了下名字，仅用于防止A和B点太近
    public float lengthCD = 80.0f;
    public float widthDE = 1.2f;
    public float lengthEF = 80.0f;

    // 对外暴露 B 点的信息，供 PacePointManager 读取作为复原锚点
    public Vector3 PointB => pointB;
    public Vector3 DirAB => clickCount >= 2 ? (pointB - pointA).normalized : Vector3.forward;
    public bool IsReady => clickCount >= 2; 

    private Vector3 pointA;
    private Vector3 pointB;
    private int clickCount = 0; 
    private List<GameObject> spawnedMarkers = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnClickRecord()
    {
        if (Camera.main == null) return;

        Vector3 currentPos = Camera.main.transform.position;
        currentPos.y = 0; 

        if (clickCount == 0)
        {
            pointA = currentPos;
            SpawnMarker(pointA, "A (起点参考 & 80米起点)");
            clickCount++;
            Debug.Log("[NorthRectangle] 点 A 已记录！请向前走动一段距离后再次按下。");
        }
        else if (clickCount == 1)
        {
            pointB = currentPos;
            float distanceAB = Vector3.Distance(pointA, pointB);
            if (distanceAB <= minDistanceAB)
            {
                Debug.LogWarning($"[NorthRectangle] 点 B 距离太近！必须大于 {minDistanceAB} 米！");
                return;
            }

            SpawnMarker(pointB, "B (方向参考 & 实际起跑点)");
            clickCount++;
            
            Debug.Log("[NorthRectangle] 点 B 已记录，正在以 A 为起点推导 80 米长方形...");
            GeneratePolygonPoints();
        }
        else
        {
            Debug.LogWarning("[NorthRectangle] 已经生成完毕！如需重新校准，请按 ClearWaypoint。");
        }
    }

    private void GeneratePolygonPoints()
    {
        Vector3 dirAB = (pointB - pointA).normalized;

        // 【核心修改 1】不再推导 C 点，直接从 A 点沿方向算 80 米得到 D 点
        Vector3 pointD = pointA + dirAB * lengthCD;
        SpawnMarker(pointD, "D");

        // 左转 90 度，走 1.2 米得到 E 点
        Vector3 dirLeftDE = Quaternion.Euler(0, (float)-90.0, 0) * dirAB;
        Vector3 pointE = pointD + dirLeftDE * widthDE;
        SpawnMarker(pointE, "E");

        // 再左转 90 度（此时朝向与 A->B 相反），走 80 米得到 F 点
        Vector3 dirLeftEF = Quaternion.Euler(0, (float)-90.0, 0) * dirLeftDE;
        Vector3 pointF = pointE + dirLeftEF * lengthEF;
        SpawnMarker(pointF, "F (应与 A 齐平)");

        if (WaypointRecorder.Instance != null)
        {
            WaypointRecorder.Instance.recordedWorldPoints.Clear();
            
            // 【核心修改 2】录入顺序：B -> D -> E -> F -> A
            // 因为 A-B-D 是直线，平滑器会自动把 B 作为一个独立的直线路点保留，不会做弯道处理
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointB); 
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointD); 
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointE);
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointF);
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointA);
            
            Debug.Log("[NorthRectangle] 已将 B, D, E, F, A 同步至全局 Waypoint 列表！");
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
        clickCount = 0;
        foreach (var marker in spawnedMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        spawnedMarkers.Clear();
        Debug.Log("[NorthRectangle] 自身数据已清空，可重新记录 A 点。");
    }
}