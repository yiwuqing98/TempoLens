using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
    public float cadence = 180f;    
    public float targetMovingSpeed = 1f;
    public bool isRunning = false;  

    // --- 新增：用于记录在自定义模式下，小球是否已经传送到了起点 ---
    private bool _hasAutoSnappedToStart = false; 

    // ==========================================
    // --- 新增：动态相对高度调节参数 ---
    // ==========================================
    [Header("--- 核心升级：动态高度调节 ---")]
    public bool enableDynamicHeight = true;         // 是否开启高度自适应
    public float targetRelativeHeight = 1.3f;       // 期望的相对高度差 (Camera.y - Sphere.y)
    public float heightHistoryDuration = 5f;        // 历史记录时长(秒)
    public float heightAdjustThreshold = 0.1f;      // 触发高度调整的阈值 (偏离多少米才调整)
    public float heightAdjustSpeed = 2f;            // 高度调整的平滑渐变速度

    // 用于记录历史高度的结构体
    private struct HeightRecord 
    {
        public float time;
        public float camY;
    }
    private Queue<HeightRecord> _camHeightHistory = new Queue<HeightRecord>();
    private float _currentAverageCamY = 0f;
    private float _lockedTargetSphereHeight; // 锁定小球的目标高度，用于平滑过渡
    // ==========================================


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
    public UnityEngine.UI.Slider cadenceSlider; 
    public UnityEngine.UI.Slider paceSlider;

    [Header("Jump Settings")]
    public int stepsPerJump = 1; // 记录当前是几步一跳，默认 1
    public TMP_Dropdown jumpStepDropdown; // 如果你用的是TMP，请改成 TMPro.TMP_Dropdown

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

        // 初始化动态高度目标
        _lockedTargetSphereHeight = sphereHeight;
    }

    private void Update()
    {
        // ==========================================
        // --- 新增：每帧执行动态高度检测与计算 ---
        // ==========================================
        if (enableDynamicHeight && Camera.main != null)
        {
            UpdateDynamicHeight();
        }
        // ==========================================

        if (sphereTransform == null) return;
        
        Vector3 currentPos = sphereTransform.position;

        switch (currentMode)
        {
            case MovementMode.Linear:
                currentPos.y = sphereHeight;
                currentPos += _moveDirection * forwardMovingSpeed * Time.deltaTime;

                if (_moveDirection != Vector3.zero && forwardMovingSpeed > 0.01f)
                    sphereTransform.rotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
                break;

            case MovementMode.Rectangle:
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
                if (pathSmoother != null && pathSmoother.SmoothedWaypoints.Count > 0 && forwardMovingSpeed > 0.001f)
                {
                    List<Vector3> customPoints = pathSmoother.SmoothedWaypoints;
                    
                    Vector3 targetWorldPos = customPoints[_currentWaypointIndex];
                    targetWorldPos.y = sphereHeight; 
                    currentPos.y = sphereHeight;

                    float step = forwardMovingSpeed * Time.deltaTime;
                    currentPos = Vector3.MoveTowards(currentPos, targetWorldPos, step);

                    Vector3 moveDir = (targetWorldPos - currentPos).normalized;
                    if (moveDir != Vector3.zero)
                        sphereTransform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

                    if (Vector3.Distance(currentPos, targetWorldPos) < 0.01f)
                        _currentWaypointIndex = (_currentWaypointIndex + 1) % customPoints.Count;
                }
                else currentPos.y = sphereHeight;
                break;
        }

        // 应用位移
        sphereTransform.position = currentPos;
    }

    // ==========================================
    // --- 新增：计算 5 秒滑动窗口平均高度 ---
    // ==========================================
    private void UpdateDynamicHeight()
    {
        float currentTime = Time.time;
        float currentCamY = Camera.main.transform.position.y;

        // 1. 记录当前高度进队列
        _camHeightHistory.Enqueue(new HeightRecord { time = currentTime, camY = currentCamY });

        // 2. 剔除超过 5 秒的旧数据
        while (_camHeightHistory.Count > 0 && _camHeightHistory.Peek().time < currentTime - heightHistoryDuration)
        {
            _camHeightHistory.Dequeue();
        }

        // 3. 计算队列中高度的平均值
        float sumY = 0f;
        foreach (var record in _camHeightHistory)
        {
            sumY += record.camY;
        }
        
        if (_camHeightHistory.Count > 0)
        {
            _currentAverageCamY = sumY / _camHeightHistory.Count;
        }

        // 4. 根据当前计算的平均相机高度，算出理论上小球应该在的高度
        float calculatedTargetSphereY = _currentAverageCamY - targetRelativeHeight;

        // 5. 判断理论高度与当前锁定的目标高度差距是否超过阈值 (过滤微小抖动)
        if (Mathf.Abs(_lockedTargetSphereHeight - calculatedTargetSphereY) > heightAdjustThreshold)
        {
            // 超过阈值，锁定新的目标高度
            _lockedTargetSphereHeight = calculatedTargetSphereY;
        }

        // 6. 使用 Lerp 平滑过渡球体高度变量
        // 这样小球不会突兀地闪现到新高度，而是像悬浮一样慢慢跟上来
        sphereHeight = Mathf.Lerp(sphereHeight, _lockedTargetSphereHeight, Time.deltaTime * heightAdjustSpeed);
    }


    public void SphereReset()
    {
        forwardMovingSpeed = 0;
        isRunning = false;

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
            if (pathSmoother != null && pathSmoother.SmoothedWaypoints.Count > 0)
            {
                List<Vector3> customPoints = pathSmoother.SmoothedWaypoints;
                
                Vector3 targetPosition = customPoints[0];
                targetPosition.y = sphereHeight;
                sphereTransform.position = targetPosition;

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

    public void PaceStartSet() 
    { 
        if (currentMode == MovementMode.CustomRecorded && !_hasAutoSnappedToStart)
        {
            if (WaypointRecorder.Instance != null && WaypointRecorder.Instance.recordedWorldPoints.Count > 0)
            {
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
    
    public void CadenceUp() { if (cadence >= 240f) return; cadence += 5f; SyncCandanceSlider(); }
    public void CadenceDown() { if (cadence <= 120f) return; cadence -= 5f; SyncCandanceSlider(); }

    // ==========================================
    // --- 优化：手动调节高度时反向校准相对高度 ---
    // ==========================================
    public void HeightUp() 
    { 
        sphereHeight += 0.20f; 
        UpdateRelativeHeightCalibration();
    }
    
    public void HeightDown() 
    { 
        sphereHeight -= 0.20f; 
        UpdateRelativeHeightCalibration();
    }

    /// <summary>
    /// 当玩家手动调节小球高度时，反向更新 targetRelativeHeight。
    /// 这样就不会发生“我刚点上升，系统又自动把它降下去”的冲突。
    /// </summary>
    private void UpdateRelativeHeightCalibration()
    {
        _lockedTargetSphereHeight = sphereHeight; 
        if (Camera.main != null && _camHeightHistory.Count > 0)
        {
            // 重新计算期望的相对高度 = 当前平均相机高度 - 你刚才手动设定的球体高度
            targetRelativeHeight = _currentAverageCamY - sphereHeight;
        }
    }
    // ==========================================


    private void SyncRunningSpeed() { if (isRunning) forwardMovingSpeed = targetMovingSpeed; }

    public void SetCadenceFromSlider(float value)
    {
        float steppedValue = Mathf.Round(value / 5f) * 5f;
        cadence = Mathf.Clamp(steppedValue, 80f, 240f);
        if (cadenceSlider != null && cadenceSlider.value != cadence) cadenceSlider.SetValueWithoutNotify(cadence);
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

    private void SyncCandanceSlider() { if (cadenceSlider != null) cadenceSlider.SetValueWithoutNotify(cadence); }
    private void SyncPaceSlider() { if (paceSlider != null) paceSlider.SetValueWithoutNotify(targetMovingSpeed); }
    private void SyncPaceSlider(float value) { if (paceSlider != null) paceSlider.SetValueWithoutNotify(value); }

    public void SetStepsFromDropdown(int dropdownIndex)
    {
        stepsPerJump = dropdownIndex + 1;
        Debug.Log($"当前节奏更改为：{stepsPerJump} 步一跳");
    }

    private void SyncJumpStepDropdown()
    {
        if (jumpStepDropdown != null && jumpStepDropdown.value != (stepsPerJump - 1))
        {
            jumpStepDropdown.SetValueWithoutNotify(stepsPerJump - 1);
        }
    }
}