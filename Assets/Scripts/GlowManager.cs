using UnityEngine;
using UnityEngine.UI;

public class GlowManager : MonoBehaviour
{
    [Header("UI 引用")]
    public Image vignetteImage; 

    [Header("闪烁参数")]
    public float maxAlpha = 0.1f; // 红色最深时的透明度
    public Color glowColor = Color.red; // 锁死为红色

    // 独立计时器，保证闪烁节奏与 isRunning 状态完美同步
    private float _glowTimer = 0f;

    void Update()
    {
        // 确保主管理器存在且 UI 已关联，否则不执行
        if (PacePointManager.Instance == null || vignetteImage == null) return;

        // 1. 实时读取 PacePointManager 的核心状态
        bool isRunning = PacePointManager.Instance.isRunning;
        float cadance = PacePointManager.Instance.cadance;
        int steps = PacePointManager.Instance.stepsPerJump;

        // 2. 如果停止跑步，或步频无效，立即熄灭红光并重置节奏
        if (!isRunning || cadance <= 0)
        {
            _glowTimer = 0f; // 归零，保证下次启动时能从准确的第一拍开始亮起
            SetAlpha(0f);    // 设置为完全透明
            return;
        }

        // 3. 计算当前步频下的节拍间隔时间（秒）
        // 例如：cadance 为 180，interval 就是 0.333 秒
        float interval = 60f / cadance * steps;

        // 4. 累加计时器 (只在 isRunning 为 true 时才会执行到这里)
        _glowTimer += Time.deltaTime;

        // 5. 计算当前透明度比例 (0 到 1 之间往返)
        // 使用 Mathf.PingPong 并在公式里乘 2，确保在一个 interval 的时间里完成一次 0->1->0 的完整呼吸循环
        float breathRatio = Mathf.PingPong(_glowTimer * 2f / interval, 1f);

        // 6. 将进度映射到实际的 maxAlpha 并赋值
        float currentAlpha = Mathf.Lerp(0f, maxAlpha, breathRatio);
        SetAlpha(currentAlpha);
    }

    /// <summary>
    /// 辅助方法：快速设置图片的透明度
    /// </summary>
    private void SetAlpha(float targetAlpha)
    {
        glowColor.a = targetAlpha;
        vignetteImage.color = glowColor;
    }
}