using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // --- 新增：单例实例 ---
    public static SceneSwitcher Instance { get; private set; }

    [Header("Scene Settings")]
    public string scene1Name = "AudioOnly";
    public string scene2Name = "Cadence_Visual";
    public string scene3Name = "Cadence_Audio";
    public string scene4Name = "Cadence_Both";

    // 使用 static (静态) 变量，保证即使场景切换了，这个计数器也不会丢失
    private static int clickCount = 0; 

    // --- 新增：Awake 方法来初始化单例 ---
    private void Awake()
    {
        // 确保全局只有一个 SceneSwitcher
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 绑定到 XREAL 射线点击或 UI 按钮的事件上
    /// </summary>
    public void SceneClick()
    {
        clickCount++; // 每次点击，计数器 +1

        if (clickCount == 1)
        {
            // 第一次点击
            LoadScene(scene1Name);
        }
        else if (clickCount == 2)
        {
            // 第二次点击
            LoadScene(scene2Name);
        }
        else if (clickCount == 3)
        {
            LoadScene(scene3Name);
        }
        else if (clickCount >= 4)
        {
            LoadScene(scene4Name);
            
            // 重置计数器，这样下一次点击又会回到第一次（场景1）
            clickCount = 0; 
        }
    }

    /// <summary>
    /// 重置当前场景：重新加载当前激活的场景，清空所有状态回到初始
    /// 可绑定到 UI 按钮或 XREAL 射线按键
    /// </summary>
    public void ResetCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[SceneSwitcher] 正在重置场景: {currentSceneName}");
        SceneManager.LoadScene(currentSceneName);
    }

    private void LoadScene(string targetScene)
    {
        // 防呆判断：确保场景名不为空，且不要原地加载自己
        if (!string.IsNullOrEmpty(targetScene) && SceneManager.GetActiveScene().name != targetScene)
        {
            Debug.Log($"第 {clickCount} 次点击，准备加载场景: {targetScene}");
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.Log($"当前已经在 {targetScene} 场景中，无需切换。");
        }
    }
}