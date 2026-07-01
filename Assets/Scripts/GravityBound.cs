using UnityEngine;

public class GravityBound : MonoBehaviour
{
    private float frequency;
    private float baseHeight;

    public Transform pacePointTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseHeight = pacePointTransform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (PacePointManager.Instance != null)
        {
            baseHeight = pacePointTransform.position.y;

            int steps = PacePointManager.Instance.stepsPerJump;

            frequency = PacePointManager.Instance.cadence / 30f / steps;
            float period = 2f / frequency;
            float t = Mathf.Repeat(Time.time, period);
            float gravityOffset = 0.5f * 9.8f * Mathf.Pow(1f / frequency, 2f) - 0.5f * 9.8f * Mathf.Pow((1f / frequency) - t, 2f);
            transform.position = new Vector3(pacePointTransform.position.x, baseHeight + gravityOffset, pacePointTransform.position.z);
        }
    }
}
