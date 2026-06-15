using UnityEngine;
using TMPro; // 必须引用 TMP 命名空间
using System.Collections.Generic;
using System.Text;

public class UIDebugLogger : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI targetText; 

    [Header("过滤设置")]
    public string filterTag = "[abc]";
    public int maxLines = 15; // 限制显示行数，防止溢出

    private Queue<string> logQueue = new Queue<string>();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 1. 过滤逻辑
        if (!logString.Contains(filterTag)) return;

        // 2. 格式化日志（带颜色区分）
        string color = "white";
        if (type == LogType.Error || type == LogType.Exception) color = "red";
        else if (type == LogType.Warning) color = "yellow";

        string formattedLog = $"<color={color}>[{type}] {logString}</color>";

        // 3. 队列管理
        logQueue.Enqueue(formattedLog);
        if (logQueue.Count > maxLines)
        {
            logQueue.Dequeue();
        }

        // 4. 更新 UI
        UpdateTextDisplay();
    }

    void UpdateTextDisplay()
    {
        if (targetText == null) return;

        StringBuilder sb = new StringBuilder();
        foreach (string line in logQueue)
        {
            sb.AppendLine(line);
        }
        targetText.text = sb.ToString();
    }
}