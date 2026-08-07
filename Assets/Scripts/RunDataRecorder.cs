using UnityEngine;
using UnityEngine.InputSystem; // 依赖新版 Input System
using System.Text;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class RunDataRecorder : MonoBehaviour
{
    public static RunDataRecorder Instance;

    [Header("追踪与反馈")]
    [Tooltip("拖入代表玩家位置的物体（例如 Main Camera）")]
    public Transform playerTransform;
    [Tooltip("拖入一个屏幕上的 TextMeshPro UI，用于显示 200m 达成提示")]
    public TMP_Text feedbackText; 

    private bool isRecording = false;
    private bool hasReached200m = false;
    
    private float totalDistance = 0f;
    private Vector3 lastPosition;
    private float timer = 0f;
    
    // 计步器与时间相关
    private int startSteps = 0;
    private float startTime = 0f;
    private StringBuilder fileContent;

    void Awake()
    {
        Instance = this;
        
        // 尝试激活手机的内置物理计步器
        if (StepCounter.current != null)
        {
            InputSystem.EnableDevice(StepCounter.current);
            Debug.Log("[Recorder] 计步器传感器已成功激活！");
        }
        else
        {
            Debug.LogWarning("[Recorder] 未检测到计步器，请确认手机支持或权限已开启。");
        }
    }

    public void StartRecording()
    {
        isRecording = true;
        hasReached200m = false;
        totalDistance = 0f;
        timer = 0f;
        startTime = Time.time;
        
        if (playerTransform != null) lastPosition = playerTransform.position;

        // 获取起跑瞬间的底子步数
        if (StepCounter.current != null)
        {
            startSteps = StepCounter.current.stepCounter.ReadValue();
        }

        fileContent = new StringBuilder();
        // 表头：Time, DistanceMeters, Speed, Cadence
        // fileContent.AppendLine("Time,DistanceMeters,Speed,Cadence"); 
        
        if (feedbackText != null) feedbackText.text = "0 / 200m";
    }

    public void StopRecording()
    {
        isRecording = false;
        if (feedbackText != null && !hasReached200m) feedbackText.text = "⏹ Record stopped";
    }

    void Update()
    {
        if (!isRecording || playerTransform == null || hasReached200m) return;

        timer += Time.deltaTime;
        
        // 每 1 秒采一次样
        if (timer >= 1.0f) 
        {
            timer -= 1.0f;
            
            // 1. 计算距离与速度
            Vector3 currentPos = playerTransform.position;
            float distThisSecond = Vector2.Distance(
                new Vector2(currentPos.x, currentPos.z), 
                new Vector2(lastPosition.x, lastPosition.z)
            );
            lastPosition = currentPos;
            
            totalDistance += distThisSecond;
            float speed = distThisSecond / 1.0f; // m/s

            if (feedbackText != null) 
            {
                feedbackText.text = $"{Mathf.FloorToInt(totalDistance)} / 200m";
            }
            
            // 2. 读取物理步数并计算平滑步频 (Cadence)
            float cadence = 0f;
            if (StepCounter.current != null)
            {
                int currentSteps = StepCounter.current.stepCounter.ReadValue();
                int stepsTaken = currentSteps - startSteps;
                float minutesPassed = (Time.time - startTime) / 60f;
                
                if (minutesPassed > 0) 
                {
                    cadence = stepsTaken / minutesPassed; // 实时平均步频(SPM)
                }
            }
            
            // 3. 记录日志
            string timeStr = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            
            fileContent.AppendLine("<Trackpoint>");
            fileContent.AppendLine($"  <Time>{timeStr}</Time>");
            fileContent.AppendLine($"  <DistanceMeters>{totalDistance:F2}</DistanceMeters>");
            fileContent.AppendLine($"  <Cadence>{Mathf.RoundToInt(cadence)}</Cadence>");
            fileContent.AppendLine("  <Extensions>");
            fileContent.AppendLine($"    <Speed>{speed:F6}</Speed>");
            fileContent.AppendLine("  </Extensions>");
            fileContent.AppendLine("</Trackpoint>");

            // 4. 判断是否跑满 200m
            if (totalDistance >= 200f)
            {
                hasReached200m = true;
                SaveFileLocally();
            }
        }
    }

    // 核心：直接保存在被控端手机内部
    private void SaveFileLocally()
    {
        string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + SceneManager.GetActiveScene().name + ".txt";
        
        // 👇 修改这里：直接指定为 emulated/0/ 下的公共 Documents 文件夹
        string directoryPath = "/storage/emulated/0/Documents";

        string filePath = Path.Combine(directoryPath, fileName);
        
        try
        {
            File.WriteAllText(filePath, fileContent.ToString());
            
            // 强烈的本地视觉反馈
            if (feedbackText != null)
            {
                feedbackText.text = $"200m reached! Saved in: \n<size=50%>{filePath}</size>";
            }
            Debug.Log("[Recorder] 200m 数据已完美保存: " + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("[Recorder] 保存文件失败: " + e.Message);
            if (feedbackText != null) feedbackText.text = $"Save failed: {e.Message}";
        }
    }
}