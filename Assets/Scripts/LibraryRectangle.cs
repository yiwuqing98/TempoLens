using UnityEngine;
using System.Collections.Generic;

public class LibraryRectangle : MonoBehaviour
{
    public static LibraryRectangle Instance { get; private set; }

    [Header("打点设置")]
    public GameObject waypointMarkerPrefab;

    [Header("长方形/多边形尺寸参数")]
    public float distanceBC = 4.0f;
    public float lengthCD = 83.0f;
    public float widthDE = 41.0f;
    public float lengthEF = 83.0f;

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
            SpawnMarker(pointA, "A (起点参考)");
            clickCount++;
            Debug.Log("[LibraryRectangle] 点 A 已记录！请走动一段距离后再次按下。");
        }
        else if (clickCount == 1)
        {
            pointB = currentPos;
            float distanceAB = Vector3.Distance(pointA, pointB);
            if (distanceAB <= distanceBC)
            {
                Debug.LogWarning($"[LibraryRectangle] 点 B 距离太近！必须大于 {distanceBC} 米！");
                return;
            }

            SpawnMarker(pointB, "B (方向参考/实际起跑点)");
            clickCount++;
            
            Debug.Log("[LibraryRectangle] 点 B 已记录，自动推导 C, D, E, F...");
            GeneratePolygonPoints();
        }
        else
        {
            Debug.LogWarning("[LibraryRectangle] 已经生成完毕！如需重新校准，请按 ClearWaypoint。");
        }
    }

    private void GeneratePolygonPoints()
    {
        Vector3 dirAB = (pointB - pointA).normalized;

        Vector3 pointC = pointB - dirAB * distanceBC;
        SpawnMarker(pointC, "C");

        Vector3 pointD = pointC + dirAB * lengthCD;
        SpawnMarker(pointD, "D");

        Vector3 dirLeftDE = Quaternion.Euler(0, (float)-88.5, 0) * dirAB;
        Vector3 pointE = pointD + dirLeftDE * widthDE;
        SpawnMarker(pointE, "E");

        Vector3 dirLeftEF = Quaternion.Euler(0, (float)-91.5, 0) * dirLeftDE;
        Vector3 pointF = pointE + dirLeftEF * lengthEF;
        SpawnMarker(pointF, "F");

        if (WaypointRecorder.Instance != null)
        {
            // 用 B 代替 C 作为轨迹起点，避免小球往回跑
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointB); 
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointD); 
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointE);
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointF);
            
            Debug.Log("[LibraryRectangle] 已将 B, D, E, F 同步至全局 Waypoint 列表！");
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

    /// <summary>
    /// 仅负责清空 LibraryRectangle 自己的点击次数和参考小球
    /// </summary>
    public void ResetRectangle()
    {
        clickCount = 0;
        foreach (var marker in spawnedMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        spawnedMarkers.Clear();

        Debug.Log("[LibraryRectangle] 自身数据已清空，可重新记录 A 点。");
    }
}