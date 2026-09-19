using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class AnimatorPhaseAudioController : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private Transform runningModelManager;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    [Header("动画状态")]
    [SerializeField] private string animationStateName = "mixamo_com";

    [Header("步频切换保护")]
    [Tooltip("步频改变后暂停播放声音的时间，避免切换瞬间多个声音叠加。")]
    [SerializeField, Min(0f)] private float silenceAfterCadenceChange = 1f;

    [Header("运行时监控（只读参考）")]
    [SerializeField] private Animator activeAnimator;
    [SerializeField] private float normalizedPhase;
    [SerializeField] private int playedBeatCount;
    [SerializeField] private float silenceRemaining;

    private AudioSource audioSource;
    private Animator[] animators;
    private Animator trackedAnimator;
    private int trackedStateHash;
    private int lastHalfCycleIndex;
    private int animationStateHash;
    private float trackedAnimatorSpeed;
    private float silenceUntilTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        animationStateHash = Animator.StringToHash(animationStateName);

        if (runningModelManager == null)
        {
            GameObject manager = GameObject.Find("Running Model Manager");
            if (manager != null)
            {
                runningModelManager = manager.transform;
            }
        }

        RefreshAnimators();
    }

    private void OnEnable()
    {
        ResetTracking();
    }

    private void OnValidate()
    {
        animationStateHash = Animator.StringToHash(animationStateName);
    }

    private void LateUpdate()
    {
        if (footstepSound == null || runningModelManager == null)
        {
            return;
        }

        activeAnimator = FindActiveAnimator();
        if (activeAnimator == null || !activeAnimator.enabled || activeAnimator.layerCount == 0)
        {
            ResetTracking();
            return;
        }

        AnimatorStateInfo stateInfo = activeAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash != animationStateHash)
        {
            ResetTracking();
            return;
        }

        normalizedPhase = Mathf.Repeat(stateInfo.normalizedTime, 1f);
        int currentHalfCycleIndex = Mathf.FloorToInt(stateInfo.normalizedTime * 2f);
        bool hasTrackedAnimator = trackedAnimator != null;
        bool animatorChanged = hasTrackedAnimator && activeAnimator != trackedAnimator;
        bool speedChanged = hasTrackedAnimator
            && activeAnimator == trackedAnimator
            && !Mathf.Approximately(activeAnimator.speed, trackedAnimatorSpeed);

        // 切换 Running，或者直接改变当前 Animator.speed，都视为步频变化。
        if (animatorChanged || speedChanged)
        {
            TrackCurrentState(stateInfo, currentHalfCycleIndex);
            BeginCadenceChangeSilence();
            return;
        }

        // 初次运行或重新进入动画状态时，在动画开头立即给一次声音提示。
        if (!hasTrackedAnimator || stateInfo.fullPathHash != trackedStateHash)
        {
            TrackCurrentState(stateInfo, currentHalfCycleIndex);
            if (!IsSilenced())
            {
                PlayBeat();
            }
            return;
        }

        trackedAnimatorSpeed = activeAnimator.speed;

        // 静音期间持续同步动画相位，结束后不会补播已经错过的节拍。
        if (IsSilenced())
        {
            lastHalfCycleIndex = currentHalfCycleIndex;
            return;
        }

        if (currentHalfCycleIndex < lastHalfCycleIndex)
        {
            lastHalfCycleIndex = currentHalfCycleIndex;
            PlayBeat();
            return;
        }

        // normalizedTime 每增加 0.5，分别对应动画中点和下一轮动画起点。
        if (currentHalfCycleIndex > lastHalfCycleIndex)
        {
            lastHalfCycleIndex = currentHalfCycleIndex;

            // 即使卡顿时跨过多个边界，也只播一次，避免同一帧声音叠加。
            PlayBeat();
        }
    }

    private void TrackCurrentState(AnimatorStateInfo stateInfo, int currentHalfCycleIndex)
    {
        trackedAnimator = activeAnimator;
        trackedStateHash = stateInfo.fullPathHash;
        trackedAnimatorSpeed = activeAnimator.speed;
        lastHalfCycleIndex = currentHalfCycleIndex;
    }

    private void BeginCadenceChangeSilence()
    {
        audioSource.Stop();
        silenceUntilTime = Time.unscaledTime + silenceAfterCadenceChange;
        silenceRemaining = silenceAfterCadenceChange;
    }

    private bool IsSilenced()
    {
        silenceRemaining = Mathf.Max(0f, silenceUntilTime - Time.unscaledTime);
        return silenceRemaining > 0f;
    }

    public void RefreshAnimators()
    {
        animators = runningModelManager != null
            ? runningModelManager.GetComponentsInChildren<Animator>(true)
            : new Animator[0];
    }

    private Animator FindActiveAnimator()
    {
        if (animators == null || animators.Length == 0)
        {
            RefreshAnimators();
        }

        foreach (Animator animator in animators)
        {
            if (animator != null && animator.gameObject.activeInHierarchy)
            {
                return animator;
            }
        }

        return null;
    }

    private void PlayBeat()
    {
        audioSource.PlayOneShot(footstepSound, volume);
        playedBeatCount++;
    }

    private void ResetTracking()
    {
        activeAnimator = null;
        trackedAnimator = null;
        trackedStateHash = 0;
        lastHalfCycleIndex = 0;
        trackedAnimatorSpeed = 0f;
        normalizedPhase = 0f;
        silenceRemaining = Mathf.Max(0f, silenceUntilTime - Time.unscaledTime);
    }
}
