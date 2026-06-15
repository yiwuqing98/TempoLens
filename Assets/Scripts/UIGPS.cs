using System.Collections;
using UnityEngine;
using TMPro; // 必须引入这个命名空间才能控制 TMP
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class StandaloneGPSViewerTMP : MonoBehaviour
{
    [Header("--- TMP UI 引用 ---")]
    [Tooltip("请把场景中的 TextMeshPro - Text (UI) 拖到这里")]
    public TMP_Text gpsDisplayText; 

    [Header("--- 设置 ---")]
    [Tooltip("每秒刷新几次 UI")]
    public float updateRate = 0.5f;

    private bool _isServiceStarted = false;

    void Start()
    {
        // 启动 GPS 硬件初始化协程
        StartCoroutine(SetupLocationService());
    }

    private IEnumerator SetupLocationService()
    {
        // 1. 安卓权限检查
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
        }
#endif

        // 2. 检查手机定位开关
        if (!Input.location.isEnabledByUser)
        {
            gpsDisplayText.text = "Turn on the GPS";
            yield break;
        }

        // 3. 启动位置服务 (精度5米，移动1米更新)
        Input.location.Start(5f, 1f);

        // 4. 等待初始化
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            gpsDisplayText.text = $"Loading GPS... ({maxWait}s)";
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            gpsDisplayText.text = "GPS Failure";
            yield break;
        }

        // 5. 启动成功，开启循环刷新
        _isServiceStarted = true;
        InvokeRepeating(nameof(RefreshDisplay), 0f, updateRate);
    }

    private void RefreshDisplay()
    {
        if (!_isServiceStarted) return;

        // 直接从 Unity 底层 API 获取最新数据
        var data = Input.location.lastData;

        // 格式化 GPS 信息
        // \n 是换行，F6 表示 6 位小数（经纬度标准）
        string content = "<b>[ GPS ]</b>\n" +
                         $"Latitude: {data.latitude:F6}°\n" +
                         $"Longtitude: {data.longitude:F6}°\n" +
                         $"Altitude: {data.altitude:F1} m\n" +
                         $"Accuracy: ±{data.horizontalAccuracy:F1} m\n" +
                         $"Time: {System.DateTime.Now:HH:mm:ss}";

        gpsDisplayText.text = content;
    }

    private void OnDisable()
    {
        // 场景关闭时务必停止硬件，否则非常耗电
        if (_isServiceStarted)
        {
            Input.location.Stop();
            _isServiceStarted = false;
        }
    }
}