using UnityEngine;

public class RunningModelManager : MonoBehaviour
{
    [Header("--- 模型管理器引用 ---")]
    [Tooltip("将层级中的 'Running Model Manager' 物体拖入此处")]
    public Transform modelManagerParent;

    // 记录上一次的步频，避免在 Update 中每帧重复调用 SetActive 浪费性能
    private float _lastCadence = -1f;

    void Update()
    {
        // 确保 PacePointManager 单例和模型父节点都已经准备好
        if (PacePointManager.Instance == null || modelManagerParent == null) return;

        // 从你的单例中实时获取当前的步频
        float currentCadence = PacePointManager.Instance.cadence;

        // 核心优化：只有当步频数值发生真正的改变时，才执行遍历和切换逻辑
        if (currentCadence != _lastCadence)
        {
            SwitchModelByCadence(currentCadence);
            _lastCadence = currentCadence;
        }
    }

    /// <summary>
    /// 根据传入的步频激活对应的模型，隐藏其余模型
    /// </summary>
    private void SwitchModelByCadence(float targetCadence)
    {
        // 将 float 转为 int 拼接字符串（防止 float 精度产生类似 "Running 180.0" 的错误命名）
        int cadenceInt = Mathf.RoundToInt(targetCadence);
        string targetName = $"Running {cadenceInt}";
        
        bool foundMatch = false;

        // 遍历 'Running Model Manager' 下的所有子物体
        foreach (Transform child in modelManagerParent)
        {
            // 名字完全匹配的目标：激活
            if (child.name == targetName)
            {
                child.gameObject.SetActive(true);
                foundMatch = true;
            }
            // 不匹配的其他模型：隐藏
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        // 防呆提示：如果在 PacePointManager 里把步频调到了 120，但你的图里模型只有 150 起步，这里会报错提醒你
        if (!foundMatch)
        {
            Debug.LogWarning($"[RunningModelSwitcher] 找不到名为 '{targetName}' 的模型！目前步频为 {cadenceInt}，请检查是否缺少该模型或超出范围。");
        }
    }
}