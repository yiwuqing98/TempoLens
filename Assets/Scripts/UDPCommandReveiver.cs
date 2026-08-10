using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class UDPCommandReceiver : MonoBehaviour
{
    // 👇 新增这一行：用于全局共享主控机的真实 IP
    public static string MasterPhoneIP = "";

    [Header("网络设置")]
    [Tooltip("监听端口，必须与主控手机发送的端口一致")]
    public int listenPort = 8888; 

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isListening = false;

    // 线程安全队列：用于把后台线程收到的指令，排队塞给主线程执行
    private ConcurrentQueue<string> commandQueue = new ConcurrentQueue<string>();

    void Start()
    {
        StartReceiving();
    }

    private void StartReceiving()
    {
        try
        {
            udpClient = new UdpClient(listenPort);
            isListening = true;

            receiveThread = new Thread(ReceiveDataWorker);
            receiveThread.IsBackground = true; // 设为后台线程，防止阻止程序退出
            receiveThread.Start();
            
            Debug.Log($"[UDP Receiver] 开始监听端口: {listenPort}...");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UDP Receiver] 端口监听失败: {e.Message}");
        }
    }

    // 后台死循环，死死盯住有没有 UDP 信号飞过来
    private void ReceiveDataWorker()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, listenPort);

        while (isListening)
        {
            try
            {
                byte[] data = udpClient.Receive(ref anyIP); // 会阻塞直到有数据来

                // 👇 【新增这一行】：自动抓取并锁定发送这条指令的手机 IP！
                MasterPhoneIP = anyIP.Address.ToString();

                
                string text = Encoding.UTF8.GetString(data);
                
                // 收到消息后，不立刻执行，而是扔进队列
                commandQueue.Enqueue(text);
            }
            catch (SocketException)
            {
                // socket被关闭时会引发异常，正常忽略
            }
        }
    }

    // 主线程 Update，每一帧检查有没有收到新指令
    void Update()
    {
        while (commandQueue.TryDequeue(out string command))
        {
            Debug.Log($"[UDP Receiver] 收到主控机指令: {command}");
            ExecuteCommand(command);
        }
    }

    // 核心大管家：把字符串翻译成你写好的方法
    private void ExecuteCommand(string data)
    {
        string[] parts = data.Split(':');
        if (parts.Length < 2) return;

        string type = parts[0]; // CMD 或 VAL
        string action = parts[1]; // 具体的动作

        if (type == "CMD")
        {
            if (PacePointManager.Instance != null)
            {
                switch (action)
                {
                    case "START": PacePointManager.Instance.PaceStartSet(); break;
                    case "STOP": PacePointManager.Instance.PaceStopSet(); break;
                    case "CLEAR": PacePointManager.Instance.ClearWaypoints(); break;
                    case "PACE_UP": PacePointManager.Instance.PaceUp(); break;
                    case "PACE_DOWN": PacePointManager.Instance.PaceDown(); break;
                    case "HEIGHT_UP": PacePointManager.Instance.HeightUp(); break;
                    case "HEIGHT_DOWN": PacePointManager.Instance.HeightDown(); break;
                    case "CADENCE_UP": PacePointManager.Instance.CadenceUp(); break;
                    case "CADENCE_DOWN": PacePointManager.Instance.CadenceDown(); break;
                    case "RESET": PacePointManager.Instance?.SphereReset(); break;
                }
            }

            // 针对打点相关的单独处理
            switch (action)
            {
                case "AIM_TRACK":
                    if (AimTrackGenerator.Instance != null) AimTrackGenerator.Instance.OnClickRecord();
                    break;
                case "NORTH_RECTANGLE":
                    if (NorthRectangle.Instance != null) NorthRectangle.Instance.OnClickRecord();
                    break;
                case "LIBRARY_RECTANGLE":
                    if (LibraryRectangle.Instance != null) LibraryRectangle.Instance.OnClickRecord();
                    break;
                case "SCENE_SWITCH":
                    if (SceneSwitcher.Instance != null) SceneSwitcher.Instance.SceneClick();
                    break;
                case "SET_WAYPOINT":
                    if (WaypointRecorder.Instance != null) WaypointRecorder.Instance.RecordCurrentPosition();
                    break;
                // 👇 新增这两个微调指令
                case "TRACK_LEFT":
                    if (AimTrackGenerator.Instance != null) AimTrackGenerator.Instance.RotateTrack(-0.25f); // 每次向左转0.5度
                    break;
                case "TRACK_RIGHT":
                    if (AimTrackGenerator.Instance != null) AimTrackGenerator.Instance.RotateTrack(0.25f);  // 每次向右转0.5度
                    break;
            }
        }
        else if (type == "VAL" && parts.Length >= 3)
        {
            // 处理滑动条的传值 (例如 VAL:CADENCE:180)
            if (float.TryParse(parts[2], out float value))
            {
                if (PacePointManager.Instance != null)
                {
                    if (action == "CADENCE") PacePointManager.Instance.SetCadenceFromSlider(value);
                    else if (action == "PACE") PacePointManager.Instance.SetPaceFromSlider(value);
                }
            }
        }
    }

    private void OnDestroy()
    {
        isListening = false;
        if (udpClient != null) udpClient.Close();
        if (receiveThread != null) receiveThread.Abort();
        Debug.Log("[UDP Receiver] 监听已关闭。");
    }
}