using System.Text.RegularExpressions;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class CadenceAnimationSpeed : MonoBehaviour
{
    [Header("自动查找")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string preferredClipName = "mixamo.com";

    [Header("步频计算")]
    [Tooltip("0 表示从对象名（例如 Running 178）自动读取。")]
    [SerializeField, Min(0f)] private float targetCadence;
    [Tooltip("一次完整动画循环包含的步数。跑步动画通常是左右脚各一步，所以是 2。")]
    [SerializeField, Min(1f)] private float stepsPerCycle = 2f;

    [Header("运行时计算结果（只读参考）")]
    [SerializeField] private float clipLengthSeconds;
    [SerializeField] private float nativeCadence;
    [SerializeField] private float desiredEffectiveSpeed;
    [SerializeField] private float appliedAnimatorSpeed = 1f;

    private static readonly Regex CadencePattern =
        new Regex(@"Running\s+(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase);

    private void Start()
    {
        if (Application.isPlaying)
        {
            ApplySpeed();
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            ApplySpeed();
        }
        else
        {
            RefreshPreview();
        }
    }

    private void Update()
    {
        if (!Application.isPlaying || targetAnimator == null)
        {
            return;
        }

        // 防止其他组件、场景切换或 Animator 重初始化后把速度恢复为 1。
        if (!Mathf.Approximately(targetAnimator.speed, appliedAnimatorSpeed))
        {
            targetAnimator.speed = appliedAnimatorSpeed;
        }
    }

    private void Reset()
    {
        FindAnimator();
    }

    private void OnValidate()
    {
        FindAnimator();
        RefreshPreview();
    }

    /// <summary>
    /// 在编辑模式下刷新 Inspector 中的计算结果，不播放动画。
    /// </summary>
    public void RefreshPreview()
    {
        if (!TryCalculate(out _, out _))
        {
            return;
        }
    }

    [ContextMenu("重新计算并应用播放速度")]
    public void ApplySpeed()
    {
        if (!TryCalculate(out float cadence, out AnimationClip clip))
        {
            return;
        }

        targetAnimator.speed = appliedAnimatorSpeed;

        Debug.Log(
            $"[{nameof(CadenceAnimationSpeed)}] {name}: clip={clip.name}, " +
            $"length={clipLengthSeconds:F6}s, nativeCadence={nativeCadence:F6}, " +
            $"targetCadence={cadence:F3}, Animator.speed={appliedAnimatorSpeed:F6}",
            this);
    }

    private bool TryCalculate(out float cadence, out AnimationClip clip)
    {
        FindAnimator();
        cadence = 0f;
        clip = null;

        if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null)
        {
            return false;
        }

        cadence = targetCadence > 0f ? targetCadence : ReadCadenceFromName();
        if (cadence <= 0f)
        {
            return false;
        }

        clip = FindAnimationClip();
        if (clip == null || clip.length <= 0f)
        {
            return false;
        }

        clipLengthSeconds = clip.length;
        nativeCadence = 60f / clipLengthSeconds * stepsPerCycle;
        desiredEffectiveSpeed = cadence / nativeCadence;
        appliedAnimatorSpeed = desiredEffectiveSpeed;
        return true;
    }

    private void FindAnimator()
    {
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
        }

        if (targetAnimator == null)
        {
            targetAnimator = GetComponentInChildren<Animator>(true);
        }
    }

    private float ReadCadenceFromName()
    {
        Transform current = transform;
        while (current != null)
        {
            Match match = CadencePattern.Match(current.name);
            if (match.Success && float.TryParse(match.Groups[1].Value, out float cadence))
            {
                return cadence;
            }

            current = current.parent;
        }

        return 0f;
    }

    private AnimationClip FindAnimationClip()
    {
        AnimationClip[] clips = targetAnimator.runtimeAnimatorController.animationClips;
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        foreach (AnimationClip clip in clips)
        {
            if (clip != null && string.Equals(clip.name, preferredClipName,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return clip;
            }
        }

        return clips[0];
    }
}
