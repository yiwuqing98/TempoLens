using UnityEngine;
using UnityEngine.Rendering; // URP 使用的 Volume 命名空间
using UnityEngine.Rendering.Universal; // URP 的具体后处理效果命名空间

public class ScreenGlowURP : MonoBehaviour
{
    public Volume volume; // 这里变成了 URP 的 Volume
    private Vignette vignette;

    [Header("呼吸参数")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.0f;
    public float cycleDuration = 2f; // 按你截图设为2秒

    void Start()
    {
        // URP 获取 Vignette 的方式
        if (volume != null)
        {
            volume.profile.TryGet(out vignette);
        }
    }

    void Update()
    {
        if (vignette != null)
        {
            // 计算 0 到 1 的往返进度
            float progress = Mathf.PingPong(Time.time * 2 / cycleDuration, 1f);
            
            // 动态改变强度
            vignette.intensity.value = Mathf.Lerp(minIntensity, maxIntensity, progress);
        }
    }
}