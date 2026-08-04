using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro;
using System.Net.Sockets; // --- 新增：网络库 ---
using System.Text;        // --- 新增：文本编码库 ---

public class PaceUIOnPhone : MonoBehaviour
{
    public TMP_Text speedText;
    
    [Header("--- 遥测网络设置 (发给手机) ---")]
    [Tooltip("通常手机开热点时的网段广播地址即可")]
    public string targetIP = "10.87.180.1"; 
    [Tooltip("发送给手机的端口，必须和之前接收的 8888 区分开")]
    public int targetPort = 8889; 
    [Tooltip("发送间隔(秒)。0.1即每秒发10次，防止高频刷爆网络")]
    public float sendInterval = 0.5f; 

    private UdpClient udpClient;
    private float timer = 0f;

    void Start()
    {
        udpClient = new UdpClient();
    }

    void Update()
    {
        if (PacePointManager.Instance != null)
        {
            float currentPace = PacePointManager.Instance.forwardMovingSpeed;
            float targetPace = PacePointManager.Instance.targetMovingSpeed;
            float cadence_ui = PacePointManager.Instance.cadence;
            
            string currentSceneName = SceneManager.GetActiveScene().name;

            float targetPaceInMinuteKilometer = 60f / 3.6f / targetPace;
            
            float currentPaceInMinuteKilometer = 0f;
            if (currentPace > 0.05f)
            {
                currentPaceInMinuteKilometer = 60f / 3.6f / currentPace;
            }

            // 统一进行字符串拼接
            string uiString = $"Current: {Mathf.FloorToInt(currentPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (currentPaceInMinuteKilometer - Mathf.FloorToInt(currentPaceInMinuteKilometer))):00}s [{currentPace:f2}m/s]\n" + 
                              $"Target: {Mathf.FloorToInt(targetPaceInMinuteKilometer)}m{Mathf.RoundToInt(60f * (targetPaceInMinuteKilometer - Mathf.FloorToInt(targetPaceInMinuteKilometer))):00}s [{targetPace:f2}m/s]\n" + 
                              $"Cadence: {cadence_ui}\n" +
                              $"Scene: {currentSceneName}"; 
                              
            // 1. 更新本地头显/屏幕的 UI
            if(speedText != null) speedText.text = uiString;

            // 2. 定时通过 UDP 发送给手机
            timer += Time.deltaTime;
            if (timer >= sendInterval)
            {
                timer = 0f;
                if (!string.IsNullOrEmpty(UDPCommandReceiver.MasterPhoneIP))
                {
                    SendToPhone("TELEMETRY:" + uiString, UDPCommandReceiver.MasterPhoneIP);
                }
            }
        }
    }

    // 👇 方法参数里加一个 targetIP
    private void SendToPhone(string data, string targetIP) 
    {
        if (udpClient != null)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                // 向锁定的真实 IP 的 8889 端口发送数据
                udpClient.Send(bytes, bytes.Length, targetIP, targetPort);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[PaceUI] 遥测数据发送失败: " + e.Message);
            }
        }
    }

    void OnDestroy()
    {
        if (udpClient != null) udpClient.Close();
    }
}