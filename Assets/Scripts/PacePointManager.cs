using UnityEngine;
using System.Collections.Generic;

public class PacePointManager : MonoBehaviour
{
    public static PacePointManager Instance { get; private set; }

    // --- 核心升级：模式选择器 ---
    public enum MovementMode
    {
        Linear,          // 原本的直线模式
        Rectangle,       // 预设的长方形模式
        CustomRecorded   // 新增的自定义打点模式
    }


    [Header("--- 核心：运动模式选择 ---")]
    public MovementMode currentMode = MovementMode.Linear;

    // --- 新增：路径平滑脚本引用 ---
    [Header("--- 核心：路径平滑引用 ---")]
    public PathSmoother pathSmoother; // 请在面板中把挂载了PathSmoother的物体拖进来

    [Header("Pace Point Status")]
    public float forwardMovingSpeed = 0f;   
    public float sphereHeight = 0.5f; 
    public float cadance = 180f;    
    public float targetMovingSpeed = 1f;
    public bool isRunning = false;  

    // --- 新增：用于记录在自定义模式下，小球是否已经传送到了起点 ---
    private bool _hasAutoSnappedToStart = false; 

    [Header("Sphere Reference")]
    public Transform sphereTransform;

    [Header("Linear Movement (直线模式用)")]
    [SerializeField] private Vector3 _moveDirection = Vector3.forward; 

    [Header("Rectangle Waypoints (长方形模式用)")]
    public List<Vector3> localWaypoints = new List<Vector3>()
    {
        new Vector3(0, 0, 0), new Vector3(0, 0, 8), new Vector3(-6, 0, 8), new Vector3(-6, 0, 0)    
    };
    
    // 用于长方形和自定义模式的共用索引
    private int _currentWaypointIndex = 0; 
    private Vector3 _pathOrigin;           
    private Quaternion _pathRotation;      

    [Header("UI References")]
    public UnityEngine.UI.Slider cadanceSlider; 
    public UnityEngine.UI.Slider paceSlider;

    [Header("Jump Settings")]
    public int stepsPerJump = 1; // 记录当前是几步一跳，默认 1
    public UnityEngine.UI.Dropdown jumpStepDropdown; // 如果你用的是TMP，请改成 TMPro.TMP_Dropdown

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (sphereTransform != null)
        {
            _pathOrigin = sphereTransform.position;
            _pathOrigin.y = 0; 
            _pathRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (sphereTransform == null) return;
        
        Vector3 currentPos = sphereTransform.position;

        switch (currentMode)
        {
            case MovementMode.Linear:
                // ==========================================
                // 模式 A：原来的直线运动
                // ==========================================
                currentPos.y = sphereHeight;
                currentPos += _moveDirection * forwardMovingSpeed * Time.deltaTime;

                if (_moveDirection != Vector3.zero && forwardMovingSpeed > 0.01f)
                    sphereTransform.rotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
                break;

            case MovementMode.Rectangle:
                // ==========================================
                // 模式 B：长方形循环运动 (基于相对坐标)
                // ==========================================
                if (localWaypoints.Count > 0 && forwardMovingSpeed > 0.001f)
                {
                    Vector3 targetLocalPos = localWaypoints[_currentWaypointIndex];
                    Vector3 targetWorldPos = _pathOrigin + (_pathRotation * targetLocalPos);
                    targetWorldPos.y = sphereHeight; 
                    currentPos.y = sphereHeight;     

                    float step = forwardMovingSpeed * Time.deltaTime;
                    currentPos = Vector3.MoveTowards(currentPos, targetWorldPos, step);

                    Vector3 moveDir = (targetWorldPos - currentPos).normalized;
                    if (moveDir != Vector3.zero)
                        sphereTransform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

                    if (Vector3.Distance(currentPos, targetWorldPos) < 0.01f)
                        _currentWaypointIndex = (_currentWaypointIndex + 1) % localWaypoints.Count;
                }
                else currentPos.y = sphereHeight;
                break;

            case MovementMode.CustomRecorded:
                // ==========================================
                // 模式 C：自定义打点运动 (现在读取的是平滑后的坐标)
                // ==========================================
                // 确保平滑器存在，且已经生成了平滑点
                if (pathSmoother != null && pathSmoother.SmoothedWaypoints.Count > 0 && forwardMovingSpeed > 0.001f)
                {
                    // --- 修改：直接读取 SmoothedWaypoints ---
                    List<Vector3> customPoints = pathSmoother.SmoothedWaypoints;
                    
                    Vector3 targetWorldPos = customPoints[_currentWaypointIndex];
                    targetWorldPos.y = sphereHeight; 
                    currentPos.y = sphereHeight;

                    float step = forwardMovingSpeed * Time.deltaTime;
                    currentPos = Vector3.MoveTowards(currentPos, targetWorldPos, step);

                    Vector3 moveDir = (targetWorldPos - currentPos).normalized;
                    if (moveDir != Vector3.zero)
                        sphereTransform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

                    // 到达该点后，飞向下一个点（无限循环）
                    if (Vector3.Distance(currentPos, targetWorldPos) < 0.01f)
                        _currentWaypointIndex = (_currentWaypointIndex + 1) % customPoints.Count;
                }
                else currentPos.y = sphereHeight;
                break;
        }

        // 应用位移
        sphereTransform.position = currentPos;
    }

    public void SphereReset()
    {
        forwardMovingSpeed = 0;
        isRunning = false;

        // --- 新增标记：只要执行过一次重置，就标记为已归位 ---
        _hasAutoSnappedToStart = true;

        if (sphereTransform == null || Camera.main == null) return;

        Transform camTransform = Camera.main.transform;
        Vector3 camPos = camTransform.position;
        Vector3 camForward = camTransform.forward;
        Vector3 resetDirection = new Vector3(camForward.x, 0, camForward.z).normalized;
        if (resetDirection == Vector3.zero) resetDirection = Vector3.forward; 

        if (currentMode == MovementMode.Rectangle)
        {
            _pathOrigin = new Vector3(camPos.x, 0, camPos.z) + resetDirection * 1.0f;
            _pathRotation = Quaternion.LookRotation(resetDirection, Vector3.up);
            _currentWaypointIndex = localWaypoints.Count > 1 ? 1 : 0; 

            Vector3 targetPosition = _pathOrigin;
            targetPosition.y = sphereHeight;
            sphereTransform.position = targetPosition;
            
            if (localWaypoints.Count > 1)
            {
                Vector3 nextPos = _pathOrigin + (_pathRotation * localWaypoints[_currentWaypointIndex]);
                Vector3 lookDir = (nextPos - targetPosition).normalized;
                if(lookDir != Vector3.zero) sphereTransform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
            else sphereTransform.rotation = _pathRotation;
        }
        else if (currentMode == MovementMode.CustomRecorded)
        {
            // --- 自定义模式重置逻辑 (改为读取平滑点) ---
            if (pathSmoother != null && pathSmoother.SmoothedWaypoints.Count > 0)
            {
                List<Vector3> customPoints = pathSmoother.SmoothedWaypoints;
                
                // 把球直接放到平滑路线的第一个点上
                Vector3 targetPosition = customPoints[0];
                targetPosition.y = sphereHeight;
                sphereTransform.position = targetPosition;

                // 目标指向第二个点（如果只有一个点，就留在原地）
                _currentWaypointIndex = customPoints.Count > 1 ? 1 : 0;

                if (customPoints.Count > 1)
                {
                    Vector3 lookDir = (customPoints[_currentWaypointIndex] - targetPosition).normalized;
                    lookDir.y = 0;
                    if(lookDir != Vector3.zero) sphereTransform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
                }
            }
        }
        else // Linear
        {
            _moveDirection = resetDirection;
            Vector3 targetPosition = camPos + _moveDirection * 1.0f;
            targetPosition.y = sphereHeight;
            sphereTransform.position = targetPosition;
            sphereTransform.rotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
        }
    }

    // ==========================================
    // 下面是你原本的配速和 UI 控制代码
    // ==========================================
    
    public void PaceStartSet() 
    { 
        // --- 新增防呆机制 ---
        // 如果是自定义模式，且还没被传送到过起点，自动调用一次重置归位
        if (currentMode == MovementMode.CustomRecorded && !_hasAutoSnappedToStart)
        {
            if (WaypointRecorder.Instance != null && WaypointRecorder.Instance.recordedWorldPoints.Count > 0)
            {
                // --- 新增：在重置小球位置前，先调用 PathSmoother 生成路线 ---
                if (pathSmoother != null)
                {
                    pathSmoother.GenerateFilletPath(WaypointRecorder.Instance.recordedWorldPoints, sphereHeight);
                }
                SphereReset(); 
            }
        }

        isRunning = true; 
        SyncRunningSpeed(); 
    }
    
    public void PaceStopSet() { isRunning = false; forwardMovingSpeed = 0f; }

    /// <summary>
    /// 新增：供其他脚本（如清理打点数据时）调用，重置防呆状态
    /// </summary>
    public void ResetAutoSnapFlag()
    {
        _hasAutoSnappedToStart = false;
    }

    public void PaceUp()
    { 
        if (targetMovingSpeed >= 8f) return; 
        float speedInMinuteKilometer = 60f / 3.6f / targetMovingSpeed; 
        speedInMinuteKilometer -= 1f / 6f;  
        targetMovingSpeed = 60f / 3.6f / speedInMinuteKilometer;
        SyncRunningSpeed();
        SyncPaceSlider(speedInMinuteKilometer);
    }

    public void PaceDown()
    {
        if (targetMovingSpeed <= 0.5f) return; 
        float speedInMinuteKilometer = 60f / 3.6f / targetMovingSpeed; 
        speedInMinuteKilometer += 1f / 6f;
        targetMovingSpeed = 60f / 3.6f / speedInMinuteKilometer;
        SyncRunningSpeed();
        SyncPaceSlider(speedInMinuteKilometer);
    }
    
    public void CadanceUp() { if (cadance >= 240f) return; cadance += 5f; SyncCandanceSlider(); }
    public void CadanceDown() { if (cadance <= 120f) return; cadance -= 5f; SyncCandanceSlider(); }
    public void HeightUp() { sphereHeight += 0.20f; }
    public void HeightDown() { sphereHeight -= 0.20f; }

    private void SyncRunningSpeed() { if (isRunning) forwardMovingSpeed = targetMovingSpeed; }

    public void SetCadanceFromSlider(float value)
    {
        float steppedValue = Mathf.Round(value / 5f) * 5f;
        cadance = Mathf.Clamp(steppedValue, 80f, 240f);
        if (cadanceSlider != null && cadanceSlider.value != cadance) cadanceSlider.SetValueWithoutNotify(cadance);
    }

    public void SetPaceFromSlider(float value)
    {
        float stepSize = 1f / 6f;
        float steppedValue = Mathf.Round(value / stepSize) * stepSize;
        float speedInMinuteKilometer = Mathf.Clamp(steppedValue, 2f, 20f);
        targetMovingSpeed = 60f / 3.6f / speedInMinuteKilometer;
        SyncRunningSpeed();
        SyncPaceSlider(speedInMinuteKilometer);
    }

    private void SyncCandanceSlider() { if (cadanceSlider != null) cadanceSlider.SetValueWithoutNotify(cadance); }
    private void SyncPaceSlider() { if (paceSlider != null) paceSlider.SetValueWithoutNotify(targetMovingSpeed); }
    private void SyncPaceSlider(float value) { if (paceSlider != null) paceSlider.SetValueWithoutNotify(value); }

    /// <summary>
    /// 暴露给 Dropdown 的 OnValueChanged 事件调用
    /// </summary>
    public void SetStepsFromDropdown(int dropdownIndex)
    {
        // 下拉框的 index 是从 0 开始的（0对应第一个选项）
        // 所以我们用 index + 1 就能完美对应 1到6 步
        stepsPerJump = dropdownIndex + 1;
        
        Debug.Log($"当前节奏更改为：{stepsPerJump} 步一跳");
    }

    /// <summary>
    /// 用于防止代码修改值时引发死循环的同步方法
    /// </summary>
    private void SyncJumpStepDropdown()
    {
        if (jumpStepDropdown != null && jumpStepDropdown.value != (stepsPerJump - 1))
        {
            jumpStepDropdown.SetValueWithoutNotify(stepsPerJump - 1);
        }
    }
}