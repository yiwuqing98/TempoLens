using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Android;
using System.Collections;

public class RunningDataTracker : MonoBehaviour
{
    public static RunningDataTracker Instance { get; private set; }

    [Header("实时运动数据 (供外部读取)")]
    public float CadenceSPM { get; private set; } = 0f;    // 步频 (步/分钟)
    public int TotalSteps { get; private set; } = 0;       // 【新增】自设备启动以来的总步数（用于调试）
    public float SpeedKmH { get; private set; } = 0f;      // 速度 (公里/小时)
    public string PaceString { get; private set; } = "--'--\""; // 配速 (分'秒"/公里)
    
    [Header("硬件状态")]
    public string GpsStatus { get; private set; } = "initializing";
    public string StepStatus { get; private set; } = "initializing";

    [Header("配置参数")]
    public float cadenceUpdateInterval = 1.0f; 
    public float minDistanceThreshold = 1.0f;  

    private int lastStepCount = 0;
    private float stepTimer = 0f;
    private LocationInfo lastLocation;
    private bool isGpsReady = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    IEnumerator Start()
    {
#if UNITY_ANDROID
        // 1. 先请求 GPS 权限
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            // 等待直到用户做出选择（无论同意还是拒绝）
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
        }
            
        // 2. 再请求 计步器 权限
        if (!Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION"))
        {
            Permission.RequestUserPermission("android.permission.ACTIVITY_RECOGNITION");
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION"));
        }

        // 给系统一点缓冲时间初始化传感器
        yield return new WaitForSeconds(1.0f);
#endif
        if (StepCounter.current != null)
        {
            InputSystem.EnableDevice(StepCounter.current);
            StepStatus = "active";
            Debug.Log("cadence counter activated");
        }
        else
        {
            StepStatus = "unsupported";
        }

        if (!Input.location.isEnabledByUser)
        {
            GpsStatus = "GPS disabled";
            yield break;
        }

        Input.location.Start(2f, 2f); 

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            GpsStatus = "GPS failure";
            yield break;
        }

        lastLocation = Input.location.lastData;
        isGpsReady = true;
        GpsStatus = "tracking";
    }

    void Update()
    {
        UpdateCadence();
        if (isGpsReady)
        {
            UpdateSpeed();
        }
    }

    private void UpdateCadence()
    {
        if (StepCounter.current != null && StepCounter.current.enabled)
        {
            // 【核心改动】每一帧都把底层硬件的步数读出来赋值给 TotalSteps
            // 这样你在界面上能立刻看到原生的数据有没有在动
            TotalSteps = StepCounter.current.stepCounter.ReadValue();

            stepTimer += Time.deltaTime;

            if (stepTimer >= cadenceUpdateInterval)
            {
                if (lastStepCount == 0) lastStepCount = TotalSteps;

                int stepsTaken = TotalSteps - lastStepCount;
                
                CadenceSPM = (stepsTaken / stepTimer) * 60f;

                if (stepsTaken == 0) CadenceSPM = 0;

                lastStepCount = TotalSteps;
                stepTimer = 0f;
            }
        }
    }

    private void UpdateSpeed()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            GpsStatus = "tracking";
            LocationInfo currentLocation = Input.location.lastData;

            if (currentLocation.timestamp > lastLocation.timestamp)
            {
                float distance = CalculateDistance(
                    lastLocation.latitude, lastLocation.longitude,
                    currentLocation.latitude, currentLocation.longitude);

                float timeDelta = (float)(currentLocation.timestamp - lastLocation.timestamp);

                if (timeDelta > 0)
                {
                    if (distance < minDistanceThreshold)
                    {
                        SpeedKmH = 0f;
                        PaceString = "--'--\"";
                    }
                    else
                    {
                        float speedMeterPerSec = distance / timeDelta;
                        SpeedKmH = speedMeterPerSec * 3.6f; 

                        if (speedMeterPerSec > 0.1f) 
                        {
                            float secondsPerKm = 1000f / speedMeterPerSec;
                            int minutes = Mathf.FloorToInt(secondsPerKm / 60f);
                            int seconds = Mathf.FloorToInt(secondsPerKm % 60f);

                            if (minutes > 99) PaceString = ">99'59\"";
                            else PaceString = string.Format("{0:00}'{1:00}\"", minutes, seconds);
                        }
                        else
                        {
                            PaceString = "--'--\"";
                        }
                    }
                }
                lastLocation = currentLocation;
            }
        }
        else
        {
            GpsStatus = "GPS weak";
            SpeedKmH = 0f; 
            PaceString = "--'--\"";
        }
    }

    private float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371000f; 
        float dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        float dLon = (lon2 - lon1) * Mathf.Deg2Rad;
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(lat1 * Mathf.Deg2Rad) * Mathf.Cos(lat2 * Mathf.Deg2Rad) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c;
    }
}