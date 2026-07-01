using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 必须引用这个命名空间

public class PaceUIOnGlasses : MonoBehaviour
{
    public TMP_Text speedText;

    void Update()
    {
        if (PacePointManager.Instance != null)
        {
            float currentPace = PacePointManager.Instance.forwardMovingSpeed;
            float targetPace = PacePointManager.Instance.targetMovingSpeed;
            float cadence_ui = PacePointManager.Instance.cadence;
            
            // 获取当前场景名称
            string sceneName = SceneManager.GetActiveScene().name;

            float currentPaceInMinuteKilometer = 0f;
            if (currentPace > 0.05f)
            {
                currentPaceInMinuteKilometer = 60f / 3.6f / currentPace;
            }

            float targetPaceInMinuteKilometer = 60f / 3.6f / targetPace;

            // 在 UI 文本中加入场景名称 (Scene: {sceneName})
            speedText.text = $"Scene: {sceneName}\n" + 
                             $"Current: {Mathf.FloorToInt(currentPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (currentPaceInMinuteKilometer - Mathf.FloorToInt(currentPaceInMinuteKilometer)))}s [{currentPace:f2}m/s]\n" + 
                             $"Target: {Mathf.FloorToInt(targetPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (targetPaceInMinuteKilometer - Mathf.FloorToInt(targetPaceInMinuteKilometer)))}s [{targetPace:f2}m/s]\n" + 
                             $"Cadence: {cadence_ui}";
        }
    }
}