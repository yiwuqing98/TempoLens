using UnityEngine;
using UnityEngine.UI; // 如果使用 TextMeshPro，请换成 TMPro
using TMPro; 

public class SpeedCadenceController : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text speedText;   // 绑定你的 速度 UI Text
    public TMP_Text cadenceText; // 绑定你的 步频 UI Text
    public TMP_Text gpsStatusText;  // 绑定你的 状态 UI Text
    public TMP_Text cadenceStatusText;
    public TMP_Text totalStepsText;  // 【新增】总步数 UI（用于测试）
    void Update()
    {
        // 确保追踪器存在，防止报错
        if (RunningDataTracker.Instance == null) return;

        // 1. 读取速度并格式化 (保留1位小数，如 "12.5 km/h")
        // float speed = RunningDataTracker.Instance.SpeedKmH;
        speedText.text = RunningDataTracker.Instance.PaceString;

        // 2. 读取步频 (取整，如 "160 SPM")
        float cadence = RunningDataTracker.Instance.CadenceSPM;
        cadenceText.text = $"{Mathf.RoundToInt(cadence)} SPM";

        // 3. 【新增】显示总步数
        totalStepsText.text = $"总步数: {RunningDataTracker.Instance.TotalSteps}";

        // 3. 读取当前硬件状态 (方便调试，如"GPS追踪中")
        gpsStatusText.text = RunningDataTracker.Instance.GpsStatus;
        cadenceStatusText.text = RunningDataTracker.Instance.StepStatus;
    }
}