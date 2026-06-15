using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance { get; private set; }
    
    [Header("--- 目标地理坐标 (你要把模型放在哪) ---")]
    public double targetLatitude = 38.251481;
    public double targetLongitude = 140.875819;
    public float targetAltitude = 32.3f;

    [Header("--- 场景引用 ---")]
    [Tooltip("把包含目标模型的父物体拖到这里")]
    public Transform environmentParent;

    private bool _isServiceStarted = false;
    
    // 现实中1纬度大约对应的米数
    private const double MetersPerDegree = 111319.9; 

    private Vector3 _initialParentPosition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 2. 在游戏刚启动时，记录下父节点在场景里的原本位置
        if (environmentParent != null)
        {
            _initialParentPosition = environmentParent.position;
        }

        StartCoroutine(SetupLocationService());
    }

    private IEnumerator SetupLocationService()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
        }
#endif

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("[abc] Please enable location services.");
            yield break;
        }

        Input.location.Start(5f, 1f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("[abc] GPS initialization failed.");
            yield break;
        }

        _isServiceStarted = true;
        Debug.Log("[abc] GPS is ready.");
    }

    /// <summary>
    /// 【核心】：计算目标坐标减去当前坐标的差值向量
    /// </summary>
    public void RelocateScene()
    {
        if (environmentParent == null) return;
        if (!_isServiceStarted || Input.location.lastData.latitude == 0)
        {
            Debug.LogWarning("[abc] GPS data is not valid yet.");
            return;
        }

        // 1. 获取当前 GPS
        double currentLat = Input.location.lastData.latitude;
        double currentLon = Input.location.lastData.longitude;
        float currentAlt = Input.location.lastData.altitude;

        // 2. 直接计算差值 (目标 - 当前)
        double latDiff = targetLatitude - currentLat;
        double lonDiff = targetLongitude - currentLon;
        float altDiff = targetAltitude - currentAlt;

        // 3. 将 GPS 差值转化为 Unity 的米数向量 (Vector3)
        // 使用两者的平均纬度来计算经度收缩比例，这样就完全不需要设置什么 Base Origin 了！
        double meanLat = (targetLatitude + currentLat) / 2.0; 
        double lonScale = Mathf.Cos((float)meanLat * Mathf.Deg2Rad);

        // 依据你之前的轴向映射：纬度差->X轴，经度差->-Z轴
        float posX = (float)(latDiff * MetersPerDegree);
        float posZ = (float)(-lonDiff * MetersPerDegree * lonScale);
        float posY = altDiff;

        Vector3 distanceVector = new Vector3(posX, posY, posZ);

        // ... (前面的 GPS 获取和 Vector3 计算逻辑不变) ...
        // 算出了 distanceVector

        // 3. 完美赋值：永远等于 “原本在场景里的位置” 加上 “GPS 需要偏移的量”
        environmentParent.position = _initialParentPosition + distanceVector;
        
        Debug.Log($"[abc] Updated Position based on initial offset: {environmentParent.position}");
    }

    private void OnDisable()
    {
        if (_isServiceStarted) Input.location.Stop();
    }
}