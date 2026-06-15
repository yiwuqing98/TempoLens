using UnityEngine;

public class SphereColorManager : MonoBehaviour
{
    public Transform positionSourceSphere; 
    
    public Renderer visualTargetRenderer;
    
    public Transform targetCamera; 

    [Header("--- 距离阈值设置 ---")]
    public float closeDistance = 4f;
    public float farDistance = 10f;
    
    [Tooltip("防抖缓冲区大小(米)。防止相机在边界微小抖动时引起颜色狂闪")]
    public float bufferZone = 0.2f; 

    [Header("--- 颜色设置 ---")]
    public Color closeColor = Color.yellow; 
    public Color midColor = Color.red;      
    public Color farColor = Color.yellow;   

    private Material _visualMaterial;
    
    // 状态机记录：0=近(黄), 1=中(红), 2=远(黄)
    private int _currentState = -1;

    void Start()
    {
        // 1. 初始化材质球
        if (visualTargetRenderer != null)
        {
            _visualMaterial = visualTargetRenderer.material;
        }
        else
        {
            Debug.LogError("请将负责显示的球体拖入 visualTargetRenderer 槽位！");
        }

        // 2. 检查隐形球引用
        if (positionSourceSphere == null)
        {
            Debug.LogError("请将负责算位置的隐形球拖入 positionSourceSphere 槽位！");
        }

        // 3. 检查相机引用
        if (targetCamera == null)
        {
            Debug.LogError("请将相机拖入 targetCamera 槽位！");
        }
    }

    void Update()
    {
        // 确保三个核心引用都不为空，否则直接跳过计算避免报错
        if (targetCamera == null || _visualMaterial == null || positionSourceSphere == null) return;

        // 每帧实时获取手动指定相机的最新位置，并计算距离
        float distance = Vector3.Distance(positionSourceSphere.position, targetCamera.position);

        int targetState = _currentState;

        // 带有“滞回/防抖”逻辑的状态判断
        if (_currentState == 0) // 当前处于“近区” (小于4米)
        {
            // 必须超过 4 + 0.2 米，才允许变成“中区”
            if (distance > closeDistance + bufferZone) targetState = 1; 
        }
        else if (_currentState == 1) // 当前处于“中区” (4米到10米)
        {
            // 必须小于 4 - 0.2 米，才变回“近区”
            if (distance < closeDistance - bufferZone) targetState = 0;
            // 必须大于 10 + 0.2 米，才变成“远区”
            else if (distance > farDistance + bufferZone) targetState = 2;
        }
        else if (_currentState == 2) // 当前处于“远区” (大于10米)
        {
            // 必须小于 10 - 0.2 米，才变回“中区”
            if (distance < farDistance - bufferZone) targetState = 1;
        }
        else // 游戏刚开始的初始判断
        {
            if (distance < closeDistance) targetState = 0;
            else if (distance <= farDistance) targetState = 1;
            else targetState = 2;
        }

        // 如果状态发生了实质性改变，才去修改材质的颜色
        if (targetState != _currentState)
        {
            _currentState = targetState;
            Color newColor = closeColor; // 默认
            
            if (_currentState == 0) newColor = closeColor;
            else if (_currentState == 1) newColor = midColor;
            else if (_currentState == 2) newColor = farColor;

            // 应用颜色到材质球
            if (_visualMaterial.HasProperty("_BaseColor"))
            {
                _visualMaterial.SetColor("_BaseColor", newColor);
            }
            else
            {
                _visualMaterial.color = newColor;
            }
        }
    }
}