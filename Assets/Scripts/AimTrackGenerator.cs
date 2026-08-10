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
    public float lengthCD = 98.8f;
    public float widthDE = 1.2f;
    public float lengthEF = 98.8f;

    [Header("--- 狙击防抖设置 ---")]
    [Range(0.05f, 1.0f)] public float aimSensitivity = 0.3f;
    [Range(1f, 15f)] public float smoothSpeed = 5f;

    // 👇 修改：现在第三次点击后，才算完全 Ready
    public bool IsReady => clickCount >= 3; 

    private Vector3 pointA;
    private Vector3 lockedDirection;
    private int clickCount = 0; 
    private List<GameObject> spawnedMarkers = new List<GameObject>();

    private Vector3 _baseAimDirection; 
    private Vector3 _currentSmoothedDirection;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (aimLine != null) aimLine.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 状态 1：线跟随头部移动（带有防抖处理）
        if (clickCount == 1 && Camera.main != null && aimLine != null)
        {
            Vector3 rawForward = GetFlatForward();
            float angleDelta = Vector3.SignedAngle(_baseAimDirection, rawForward, Vector3.up);
            float dampenedAngle = angleDelta * aimSensitivity;
            Vector3 targetDirection = Quaternion.Euler(0, dampenedAngle, 0) * _baseAimDirection;

            _currentSmoothedDirection = Vector3.Slerp(_currentSmoothedDirection, targetDirection, Time.deltaTime * smoothSpeed);

            if (_currentSmoothedDirection != Vector3.zero)
            {
                aimLine.SetPosition(0, pointA);
                aimLine.SetPosition(1, pointA + _currentSmoothedDirection * lengthCD);
            }
        }
        // 👇 新增状态 2：方向已锁定，此时线不再跟随头部，而是固定显示 lockedDirection，方便遥控微调
        else if (clickCount == 2 && aimLine != null)
        {
            aimLine.SetPosition(0, pointA);
            aimLine.SetPosition(1, pointA + lockedDirection * lengthCD);
        }
    }

    public void OnClickRecord()
    {
        if (Camera.main == null) return;

        if (clickCount == 0)
        {
            // --- 第 1 次点击：记录 A 点并开启射线跟随 ---
            Vector3 currentPos = Camera.main.transform.position;
            currentPos.y = 0; 
            pointA = currentPos;
            SpawnMarker(pointA, "A (起跑点)");
            
            _baseAimDirection = GetFlatForward();
            _currentSmoothedDirection = _baseAimDirection;

            clickCount++; // 变成 1
            
            if (aimLine != null)
            {
                aimLine.gameObject.SetActive(true);
                aimLine.positionCount = 2;
            }
            
            Debug.Log("[AimTrackGenerator] 第 1 步完成：起点 A 已记录！请用视线瞄准尽头，按下以锁定基准线。");
        }
        else if (clickCount == 1)
        {
            // --- 第 2 次点击：剥离头部跟随，仅仅锁定“线”的方向 ---
            lockedDirection = _currentSmoothedDirection.normalized;
            // 注意：这里不再隐藏射线，射线会变成红色的固定直线(或者保持原样)，留在原地供微调
            
            clickCount++; // 变成 2
            Debug.Log("[AimTrackGenerator] 第 2 步完成：基准线已锁定！现在可以通过遥控器左右微调。调准后再次按下生成赛道点。");
        }
        else if (clickCount == 2)
        {
            // --- 第 3 次点击：微调完毕，正式生成柱子并隐藏射线 ---
            if (aimLine != null) aimLine.gameObject.SetActive(false);
            
            GeneratePolygonPoints();

            clickCount++; // 变成 3
            Debug.Log("[AimTrackGenerator] 第 3 步完成：正式赛道已生成并同步！");
        }
        else
        {
            // --- 第 4 次点击：清空重来 ---
            Debug.Log("[AimTrackGenerator] 第 4 步触发：重新校准需求，自动清空所有数据！");
            if (PacePointManager.Instance != null) PacePointManager.Instance.ClearWaypoints();
            else
            {
                ResetRectangle();
                if (WaypointRecorder.Instance != null) WaypointRecorder.Instance.ClearAllPoints();
            }
        }
    }

    // --- 遥控器调用的微调方法 ---
    public void RotateTrack(float angleDegrees)
    {
        if (clickCount >= 2)
        {
            // 修改锁定的方向
            lockedDirection = Quaternion.Euler(0, angleDegrees, 0) * lockedDirection;
            Debug.Log($"[AimTrackGenerator] 赛道微调 {angleDegrees} 度");
            
            // 如果是在第 2 步微调，Update() 里的代码会自动刷新射线的视觉表现
            // 如果是在第 3 步（点都已经生成了）还想微调，那么连带着重新生成一遍赛道点
            if (clickCount >= 3) 
            {
                GeneratePolygonPoints(); 
            }
        }
    }

    private void GeneratePolygonPoints()
    {
        for (int i = spawnedMarkers.Count - 1; i >= 1; i--) 
        {
            Destroy(spawnedMarkers[i]);
            spawnedMarkers.RemoveAt(i);
        }

        Vector3 pointD = pointA + lockedDirection * lengthCD;
        SpawnMarker(pointD, "D (尽头折返点)");

        Vector3 dirLeftDE = Quaternion.Euler(0, -90.0f, 0) * lockedDirection;
        Vector3 pointE = pointD + dirLeftDE * widthDE;
        SpawnMarker(pointE, "E");

        Vector3 dirLeftEF = Quaternion.Euler(0, -90.0f, 0) * dirLeftDE;
        Vector3 pointF = pointE + dirLeftEF * lengthEF;
        SpawnMarker(pointF, "F");

        if (WaypointRecorder.Instance != null)
        {
            WaypointRecorder.Instance.recordedWorldPoints.Clear();
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointA); 
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointD); 
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointE);
            WaypointRecorder.Instance.recordedWorldPoints.Add(pointF);
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
        if (aimLine != null) aimLine.gameObject.SetActive(false);
    }

    private Vector3 GetFlatForward()
    {
        if (Camera.main == null) return Vector3.forward;
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0; 
        return forward == Vector3.zero ? Vector3.forward : forward.normalized;
    }
}