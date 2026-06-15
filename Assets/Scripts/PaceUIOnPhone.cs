using UnityEngine;
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
            float cadance_ui = PacePointManager.Instance.cadance;
            if (currentPace > 0.05f)
            {
                float currentPaceInMinuteKilometer = 60f / 3.6f / currentPace;
                float targetPaceInMinuteKilometer = 60f / 3.6f / targetPace;

                speedText.text = $"Current: {Mathf.FloorToInt(currentPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (currentPaceInMinuteKilometer - Mathf.FloorToInt(currentPaceInMinuteKilometer)))}s [{currentPace:f2}m/s]\n" + 
                $"Target: {Mathf.FloorToInt(targetPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (targetPaceInMinuteKilometer - Mathf.FloorToInt(targetPaceInMinuteKilometer)))}s [{targetPace:f2}m/s]\n" + 
                $"Cadance: {cadance_ui}";
            }
            else
            {
                float currentPaceInMinuteKilometer = 0f;
                float targetPaceInMinuteKilometer = 60f / 3.6f / targetPace;
                speedText.text = $"Current: {Mathf.FloorToInt(currentPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (currentPaceInMinuteKilometer - Mathf.FloorToInt(currentPaceInMinuteKilometer)))}s [{currentPace:f2}m/s]\n" + 
                $"Target: {Mathf.FloorToInt(targetPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (targetPaceInMinuteKilometer - Mathf.FloorToInt(targetPaceInMinuteKilometer)))}s [{targetPace:f2}m/s]\n" + 
                $"Cadance: {cadance_ui}";
            }
        }
    }
}
