using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // --- 新增：引入场景管理 ---

[System.Serializable]
public struct GPSPoint
{
    public double latitude;   // 纬度（使用 double 保证高精度）
    public double longitude;  // 经度
    public float altitude;    // 海拔

    public GPSPoint(double lat, double lon, float alt)
    {
        latitude = lat;
        longitude = lon;
        altitude = alt;
    }
}

public class GPSMotionAligner : MonoBehaviour
{
    public static GPSMotionAligner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            // 如果你的脚本挂在全局物体上，可以取消下一行的注释保证切换场景时不被销毁
            // DontDestroyOnLoad(gameObject); 
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    [Header("场景设置")]
    [Tooltip("指定在哪一个场景才开始计算 GPS 运动轨迹")]
    public string targetSceneName = "LibraryRun";

    [Header("打点设置")]
    [Tooltip("用于在空间中标记位置的可见小球 Prefab")]
    public GameObject waypointMarkerPrefab;
    
    [Header("运动对齐设置")]
    [Tooltip("触发对齐所需的最小走动距离（米）。")]
    public float minMoveDistance = 20.0f;

    // 存储你提供的 4 个固定 GPS 坐标
    private List<GPSPoint> targetGPSPoints = new List<GPSPoint>();
    
    private bool isGPSReady = false;
    
    // 起始点 A（进入场景时的位置）
    private GPSPoint startGPS;
    private Vector3 startUnityPos;
    
    // 地球半径（米）
    private const float EarthRadius = 6378137.0f;

    // --- 新增：注册场景加载的事件监听 ---
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- 新增：当任何场景被加载完毕时触发 ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 判断当前加载的场景是不是我们指定的 "LibraryRun"
        if (scene.name == targetSceneName)
        {
            Debug.Log($"[GPS] 已进入 {targetSceneName} 场景，准备开始计算 20 米轨迹！");
            
            // 1. 初始化 4 个目标点 (先 Clear 是为了防止反复进出场景导致重复添加)
            targetGPSPoints.Clear();
            targetGPSPoints.Add(new GPSPoint(38.251694, 140.876141, 76.90f));
            targetGPSPoints.Add(new GPSPoint(38.251989, 140.877006, 76.50f));
            targetGPSPoints.Add(new GPSPoint(38.251673, 140.877202, 75.70f));
            targetGPSPoints.Add(new GPSPoint(38.251396, 140.876360, 75.35f));

            // 2. 如果 GPS 还没启动过，启动它；如果已经启动了，直接重置 A 点
            if (!isGPSReady)
            {
                StartCoroutine(StartContinuousGPS());
            }
            else
            {
                RecordPointA(); 
                Debug.Log("[GPS] 起始点 A 已重置，20 米轨迹将从当前位置重新开始计算。");
            }
        }
    }

    private void Start()
    {
        // 兼容性处理：如果你是在 Unity Editor 里直接打开 LibraryRun 场景点击播放，
        // OnSceneLoaded 可能会错过，所以在这里做一次补救检查
        if (SceneManager.GetActiveScene().name == targetSceneName && !isGPSReady)
        {
            // 触发一次伪加载
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }

    private IEnumerator StartContinuousGPS()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("[GPS] 用户未开启定位权限或设备未开启 GPS！");
            yield break;
        }

        // 启动定位服务和罗盘
        Input.location.Start(0.5f, 0.5f);
        Input.compass.enabled = true;

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("[GPS] 定位服务初始化超时或失败！");
            yield break;
        }

        // GPS 初始化成功，立刻将当前位置记录为【起始点 A】
        isGPSReady = true;
        RecordPointA();
        Debug.Log("[GPS] 硬件启动成功！正在持续追踪您的位置，请向前直行 20 米...");
    }

    /// <summary>
    /// 记录进入场景时的 A 点（或重置 A 点）
    /// </summary>
    public void RecordPointA()
    {
        if (Camera.main == null) return;

        startGPS = new GPSPoint(
            Input.location.lastData.latitude,
            Input.location.lastData.longitude,
            Input.location.lastData.altitude
        );
        
        startUnityPos = Camera.main.transform.position;
        startUnityPos.y = 0;

        Debug.Log($"[GPS] 确立/重置初始点 A -> GPS:({startGPS.latitude}, {startGPS.longitude}), Unity空间坐标:{startUnityPos}");
    }

    public void OnClickGenerateWaypoints()
    {
        if (!isGPSReady || Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("[GPS] 信号未就绪或定位已断开，无法生成！");
            return;
        }

        if (Camera.main == null) return;

        GPSPoint currentGPS = new GPSPoint(
            Input.location.lastData.latitude,
            Input.location.lastData.longitude,
            Input.location.lastData.altitude
        );

        Vector3 currentUnityPos = Camera.main.transform.position;
        currentUnityPos.y = 0;

        float unityMoveDist = Vector3.Distance(startUnityPos, currentUnityPos);
        float gpsMoveDist = CalculateGPSDistance(startGPS, currentGPS);

        Debug.Log($"[对齐检查] 自进入 LibraryRun 场景后，您在 Unity 中走了 {unityMoveDist:F2} 米，GPS 显示走了 {gpsMoveDist:F2} 米。");

        float finalGeographicBearing = 0f;

        if (unityMoveDist >= minMoveDistance && gpsMoveDist >= minMoveDistance)
        {
            finalGeographicBearing = CalculateGPSBearing(startGPS, currentGPS);
            Debug.Log($"[方向判定] 走动距离充足(>= {minMoveDistance}m)，使用 GPS 轨迹方位角: {finalGeographicBearing:F2}°");
        }
        else
        {
            finalGeographicBearing = Input.compass.trueHeading;
            Debug.LogWarning($"[方向判定] 走动距离不足({gpsMoveDist:F2}m < {minMoveDistance}m)，已自动切换为硬件罗盘方位角: {finalGeographicBearing:F2}°");
        }

        float unityHeading = 0f;
        if (unityMoveDist >= 0.1f)
        {
            Vector3 unityDir = (currentUnityPos - startUnityPos).normalized;
            unityHeading = Mathf.Atan2(unityDir.x, unityDir.z) * Mathf.Rad2Deg;
        }
        else
        {
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            unityHeading = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;
        }
        if (unityHeading < 0) unityHeading += 360f;

        float rotationOffset = unityHeading - finalGeographicBearing;
        Quaternion alignmentRotation = Quaternion.Euler(0, rotationOffset, 0);

        Debug.Log($"[空间对齐成功!] 真实地理方位角: {finalGeographicBearing:F2}°, Unity空间朝向角: {unityHeading:F2}° -> 修正旋转偏差: {rotationOffset:F2}°");

        foreach (var targetGPS in targetGPSPoints)
        {
            float deltaLat = (float)(targetGPS.latitude - currentGPS.latitude) * Mathf.Deg2Rad;
            float deltaLon = (float)(targetGPS.longitude - currentGPS.longitude) * Mathf.Deg2Rad;

            float rawX = deltaLon * EarthRadius * Mathf.Cos((float)currentGPS.latitude * Mathf.Deg2Rad);
            float rawZ = deltaLat * EarthRadius;
            
            Vector3 rawOffsetVector = new Vector3(rawX, 0, rawZ);
            Vector3 rotatedOffset = alignmentRotation * rawOffsetVector;
            Vector3 finalSpawnPos = currentUnityPos + rotatedOffset;

            if (waypointMarkerPrefab != null)
            {
                GameObject marker = Instantiate(waypointMarkerPrefab, finalSpawnPos, Quaternion.identity);
            }

            if (WaypointRecorder.Instance != null)
            {
                WaypointRecorder.Instance.recordedWorldPoints.Add(finalSpawnPos);
            }

            Debug.Log($"[生成成功] 目标 GPS({targetGPS.latitude}, {targetGPS.longitude}) -> 已映射至 Unity 坐标: {finalSpawnPos}");
        }
    }

    private float CalculateGPSDistance(GPSPoint p1, GPSPoint p2)
    {
        float deltaLat = (float)(p2.latitude - p1.latitude) * Mathf.Deg2Rad;
        float deltaLon = (float)(p2.longitude - p1.longitude) * Mathf.Deg2Rad;
        float x = deltaLon * EarthRadius * Mathf.Cos((float)p1.latitude * Mathf.Deg2Rad);
        float z = deltaLat * EarthRadius;
        return Mathf.Sqrt(x * x + z * z);
    }

    private float CalculateGPSBearing(GPSPoint p1, GPSPoint p2)
    {
        float lat1 = (float)p1.latitude * Mathf.Deg2Rad;
        float lat2 = (float)p2.latitude * Mathf.Deg2Rad;
        float deltaLon = (float)(p2.longitude - p1.longitude) * Mathf.Deg2Rad;

        float y = Mathf.Sin(deltaLon) * Mathf.Cos(lat2);
        float x = Mathf.Cos(lat1) * Mathf.Sin(lat2) - Mathf.Sin(lat1) * Mathf.Cos(lat2) * Mathf.Cos(deltaLon);
        
        float bearing = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        return (bearing + 360f) % 360f; 
    }
}