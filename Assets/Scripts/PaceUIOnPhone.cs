using UnityEngine;
using UnityEngine.SceneManagement; // --- 新增：引入场景管理命名空间 ---
using TMPro;

public class PaceUIOnPhone : MonoBehaviour
{
    public TMP_Text speedText;
    
    void Update()
    {
        if (PacePointManager.Instance != null)
        {
            float currentPace = PacePointManager.Instance.forwardMovingSpeed;
            float targetPace = PacePointManager.Instance.targetMovingSpeed;
            float cadence_ui = PacePointManager.Instance.cadence;
            
            // --- 新增：获取当前活跃场景的名称 ---
            string currentSceneName = SceneManager.GetActiveScene().name;

            // 计算目标配速
            float targetPaceInMinuteKilometer = 60f / 3.6f / targetPace;
            
            // 计算当前配速（当速度极低时归零，避免除以近乎 0 的数导致显示异常）
            float currentPaceInMinuteKilometer = 0f;
            if (currentPace > 0.05f)
            {
                currentPaceInMinuteKilometer = 60f / 3.6f / currentPace;
            }

            // 统一进行字符串拼接，避免 if/else 中写两遍重复的代码
            speedText.text = $"Current: {Mathf.FloorToInt(currentPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (currentPaceInMinuteKilometer - Mathf.FloorToInt(currentPaceInMinuteKilometer))):00}s [{currentPace:f2}m/s]\n" + 
                             $"Target: {Mathf.FloorToInt(targetPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (targetPaceInMinuteKilometer - Mathf.FloorToInt(targetPaceInMinuteKilometer))):00}s [{targetPace:f2}m/s]\n" + 
                             $"Cadence: {cadence_ui}\n" +
                             $"Scene: {currentSceneName}"; // --- 新增：追加场景名显示 ---
        }
    }
}