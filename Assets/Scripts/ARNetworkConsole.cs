using UnityEngine;
using UnityEngine.UI;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ARNetworkConsole : MonoBehaviour
{
    [Header("UI 引用")]
    public Text consoleText;
    public ScrollRect scrollRect;
    
    [Header("设置")]
    public int maxLines = 30; // 最多保留几行，防止内存爆炸
    
    // 多线程安全队列：拦截来自后台 UDP 线程的 Log
    private ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
    private List<string> logLines = new List<string>();

    // 【核心：白名单过滤】只有包含这些词的 Log 才会被显示到屏幕上
    private string[] allowedKeywords = new string[] 
    { 
        "[UDP", 
        "收到", 
        "指令", 
        "AimTrack", 
        "监听"
    };

    void OnEnable()
    {
        // 订阅 Unity 的全局 Log 事件（支持多线程）
        Application.logMessageReceivedThreaded += HandleLog;
        logQueue.Enqueue("<color=#00FF00>> 网络监控控制台已启动...</color>");
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 1. 过滤掉没用的系统 Log
        bool shouldShow = false;
        foreach (var keyword in allowedKeywords)
        {
            if (logString.Contains(keyword))
            {
                shouldShow = true;
                break;
            }
        }

        if (!shouldShow) return;

        // 2. 根据状态上色
        string color = "#FFFFFF"; // 默认白色
        if (type == LogType.Warning) color = "#FFFF00"; // 警告黄色
        else if (type == LogType.Error || type == LogType.Exception) color = "#FF0000"; // 报错红色
        else if (logString.Contains("监听端口")) color = "#00FFFF"; // 启动提示青色
        else if (logString.Contains("收到")) color = "#00FF00"; // 成功收到指令绿色

        // 3. 压入队列，等待主线程更新 UI
        string formattedLog = $"<color={color}>{logString}</color>";
        logQueue.Enqueue(formattedLog);
    }

    void Update()
    {
        bool uiNeedsUpdate = false;
        
        // 把队列里的 Log 掏出来显示
        while (logQueue.TryDequeue(out string newLog))
        {
            logLines.Add(newLog);
            if (logLines.Count > maxLines)
            {
                logLines.RemoveAt(0);
            }
            uiNeedsUpdate = true;
        }

        // 如果有新消息，刷新文本并自动滚到底部
        if (uiNeedsUpdate && consoleText != null)
        {
            consoleText.text = string.Join("\n", logLines);
            
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}