using UnityEngine;

public class PacerMonitor : MonoBehaviour
{
    private static readonly int MixamoStateHash = Animator.StringToHash("mixamo_com");

    private Animator anim;
    private bool initialized;
    private float windowStartNormalizedTime;
    private double windowStartTime;

    [Header("实时监控数据")]
    public float animatorSpeed;
    public float elapsedTime;
    public float loopsPerMinute;
    public float estimatedCadence;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        if (anim == null)
        {
            return;
        }

        AnimatorStateInfo stateInfo =
            anim.GetCurrentAnimatorStateInfo(0);

        animatorSpeed = anim.speed;

        // shortNameHash 不受 Animator Layer 名称影响，比只写 IsName 更稳定。
        if (stateInfo.shortNameHash != MixamoStateHash)
        {
            initialized = false;
            return;
        }

        if (!initialized)
        {
            initialized = true;
            windowStartNormalizedTime = stateInfo.normalizedTime;
            windowStartTime = Time.timeAsDouble;
            return;
        }

        double actualElapsed =
            Time.timeAsDouble - windowStartTime;

        elapsedTime = (float)actualElapsed;

        float completedCycles =
            stateInfo.normalizedTime -
            windowStartNormalizedTime;

        // 每帧显示实时估算结果。开始后的前 0.25 秒样本太短，暂不刷新。
        if (actualElapsed >= 0.25)
        {
            loopsPerMinute =
                completedCycles * 60f /
                (float)actualElapsed;

            // 一次完整跑步动画循环对应左右脚各一步。
            estimatedCadence = loopsPerMinute * 2f;
        }

        if (actualElapsed >= 60.0)
        {
            Debug.Log(
                $"【配速统计】循环次数/分钟: {loopsPerMinute:F3}，" +
                $"估算步频: {estimatedCadence:F3}");

            windowStartNormalizedTime =
                stateInfo.normalizedTime;

            windowStartTime = Time.timeAsDouble;
            elapsedTime = 0f;
        }
    }
}
