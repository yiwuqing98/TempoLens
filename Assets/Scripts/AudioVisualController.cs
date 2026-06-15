using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioVisualController : MonoBehaviour
{
    [Header("--- 音频设置 ---")]
    [Tooltip("把外部找好的脚步声或节拍器音效拖到这里")]
    public AudioClip footstepSound;
    [Range(0f, 1f)] public float volume = 0.8f;

    [Header("--- 状态监控 (自动同步，无需修改) ---")]
    [Tooltip("当前步频 (自动从PacePointManager读取)")]
    public float cadance = 180f; 
    public bool isPlaying = false;

    private AudioSource _audioSource;
    private float _timer = 0f;

    void Start()
    {
        // 获取并配置 AudioSource 组件
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        
        if (footstepSound == null)
        {
            Debug.LogWarning("AudioVisualController: 没有挂载音效片段 (AudioClip)！请在面板中拖入你找好的音源。");
        }
    }

    void Update()
    {
        // 确保主管理器存在，否则跳过
        if (PacePointManager.Instance == null) return;

        // 1. 实时读取并同步主管理器的核心状态
        isPlaying = PacePointManager.Instance.isRunning;
        cadance = PacePointManager.Instance.cadance;

        // 2. 如果主程序处于停止状态，或者没有挂载音效，直接中断并重置计时器
        if (!isPlaying || footstepSound == null || cadance <= 0)
        {
            // 【关键】：这里必须把 _timer 归零。
            // 确保 PacePointManager 的 isRunning 再次变成 true 时，能立刻播第一声
            _timer = 0f; 
            return;
        }

        // 3. 计算当前步频下的每次节拍间隔时间（秒）
        float interval = 60f / cadance;

        // 4. 计时器累加
        _timer += Time.deltaTime;

        // 5. 达到间隔时间，播放声音
        if (_timer >= interval)
        {
            PlayBeat();
            
            // 💡 节奏精准度优化：保留 Time.deltaTime 溢出的几毫秒微小误差
            _timer -= interval; 
        }
    }

    /// <summary>
    /// 播放单次脚步声
    /// </summary>
    private void PlayBeat()
    {
        // PlayOneShot 允许在同一时间重叠播放音效，非常适合高频率的脚步声
        _audioSource.PlayOneShot(footstepSound, volume);
        Debug.Log($"beat played");
    }
}