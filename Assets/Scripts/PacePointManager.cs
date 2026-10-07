using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PacePointManager : MonoBehaviour
{
    public static PacePointManager Instance { get; private set; }

    public const float MinCadence = 120f;
    public const float MaxCadence = 200f;

    public enum MovementMode
    {
        Linear,          
        Rectangle,       
        CustomRecorded   
    }

    [Header("--- 核心：运动模式选择 ---")]
    public MovementMode currentMode = MovementMode.Linear;

    [Header("--- 核心：路径平滑引用 ---")]
    public PathSmoother pathSmoother; 

    [Header("Pace Point Status")]
    public float forwardMovingSpeed = 0f;   
    public float sphereHeight = 0.5f; 
    public float cadence = 180f;    
    public float targetMovingSpeed = 1f;
    public bool isRunning = false;  

    private bool _hasAutoSnappedToStart = false; 

    // 👇 新增：用于保证当前场景下，数据记录只被激活一次的锁
    private bool _hasStartedRecordingThisScene = false;

    [Header("--- 核心升级：动态高度调节 ---")]
    public bool enableDynamicHeight = true;         
    public float targetRelativeHeight = 1.3f;       
    public float heightHistoryDuration = 5f;        
    public float heightAdjustThreshold = 0.1f;      
    public float heightAdjustSpeed = 2f;            

    private struct HeightRecord 
    {
        public float time;
        public float camY;
    }
    private Queue<HeightRecord> _camHeightHistory = new Queue<HeightRecord>();
    private float _currentAverageCamY = 0f;
    private float _lockedTargetSphereHeight; 

    [Header("Sphere Reference")]
    public Transform sphereTransform;

    [Header("Linear Movement")]
    [SerializeField] private Vector3 _moveDirection = Vector3.forward; 

    [Header("Rectangle Waypoints")]
    public List<Vector3> localWaypoints = new List<Vector3>()
    {
        new Vector3(0, 0, 0), new Vector3(0, 0, 8), new Vector3(-6, 0, 8), new Vector3(-6, 0, 0)    
    };
    
    private int _currentWaypointIndex = 0; 
    private Vector3 _pathOrigin;           
    private Quaternion _pathRotation;      

    [Header("UI References")]
    public UnityEngine.UI.Slider cadenceSlider; 
    public UnityEngine.UI.Slider paceSlider;

    [Header("Jump Settings")]
    public int stepsPerJump = 1; 
    public TMP_Dropdown jumpStepDropdown; 

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
        _lockedTargetSphereHeight = sphereHeight;
    }

    private void Update()
    {
        if (enableDynamicHeight && Camera.main != null)
        {
            UpdateDynamicHeight();
        }

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
        sphereTransform.position = currentPos;
    }

    private void UpdateDynamicHeight()
    {
        float currentTime = Time.time;
        float currentCamY = Camera.main.transform.position.y;

        _camHeightHistory.Enqueue(new HeightRecord { time = currentTime, camY = currentCamY });

        while (_camHeightHistory.Count > 0 && _camHeightHistory.Peek().time < currentTime - heightHistoryDuration)
        {
            _camHeightHistory.Dequeue();
        }

        float sumY = 0f;
        foreach (var record in _camHeightHistory)
        {
            sumY += record.camY;
        }
        
        if (_camHeightHistory.Count > 0)
        {
            _currentAverageCamY = sumY / _camHeightHistory.Count;
        }

        float calculatedTargetSphereY = _currentAverageCamY - targetRelativeHeight;

        if (Mathf.Abs(_lockedTargetSphereHeight - calculatedTargetSphereY) > heightAdjustThreshold)
        {
            _lockedTargetSphereHeight = calculatedTargetSphereY;
        }

        sphereHeight = Mathf.Lerp(sphereHeight, _lockedTargetSphereHeight, Time.deltaTime * heightAdjustSpeed);
    }

    public void SphereReset()
    {
        forwardMovingSpeed = 0;
        isRunning = false;
        _hasAutoSnappedToStart = true;

        if (sphereTransform == null) return;

        if (currentMode == MovementMode.CustomRecorded)
        {
            if (pathSmoother != null && pathSmoother.SmoothedWaypoints.Count > 0)
            {
                List<Vector3> customPoints = pathSmoother.SmoothedWaypoints;
                int startIndex = 0;

                bool isLibraryReady = LibraryRectangle.Instance != null && LibraryRectangle.Instance.IsReady;
                bool isAimReady = AimTrackGenerator.Instance != null && AimTrackGenerator.Instance.IsReady;
                bool isNorthReady = NorthRectangle.Instance != null && NorthRectangle.Instance.IsReady;

                if (isLibraryReady || isAimReady)
                {
                    // 模式1：图书馆 或 视线瞄准。起点都在长方形角上，需要跳过入弯圆弧，防止一上来原地掉头
                    startIndex = Mathf.Min(pathSmoother.arcResolution, customPoints.Count - 1);
                }
                else if (isNorthReady)
                {
                    // 模式2：走路双点校准 (North)。录入顺序是 B->D->E->F->A，第 0 个点就是完美的起跑直道
                    startIndex = 0; 
                }
                else
                {
                    // 其他普通自定义路径
                    startIndex = 0;
                }

                Vector3 targetPosition = customPoints[startIndex];
                targetPosition.y = sphereHeight;
                sphereTransform.position = targetPosition;

                _currentWaypointIndex = (startIndex + 1) % customPoints.Count;

                Vector3 lookDir = (customPoints[_currentWaypointIndex] - targetPosition).normalized;
                lookDir.y = 0;
                if(lookDir != Vector3.zero) 
                    sphereTransform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
        }
        else 
        {
             if (Camera.main != null)
             {
                 Vector3 camPos = Camera.main.transform.position;
                 Vector3 resetDirection = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
                 if (resetDirection == Vector3.zero) resetDirection = Vector3.forward; 
                 _moveDirection = resetDirection;
                 Vector3 targetPosition = camPos + _moveDirection * 1.0f;
                 targetPosition.y = sphereHeight;
                 sphereTransform.position = targetPosition;
                 sphereTransform.rotation = Quaternion.LookRotation(_moveDirection, Vector3.up);
             }
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

        // 👇 --- 新增：单次激活机制 ---
        if (!_hasStartedRecordingThisScene)
        {
            if (RunDataRecorder.Instance != null)
            {
                RunDataRecorder.Instance.StartRecording();
                // 触发后立刻上锁，保证当前场景无论以后怎么点 Start，都不会再重置记录器
                _hasStartedRecordingThisScene = true; 
                Debug.Log("【PacePointManager】200m 数据记录已启动（本场景仅触发一次）。");
            }
        }
    }
    
    public void PaceStopSet() 
    { 
        isRunning = false; 
        forwardMovingSpeed = 0f; 
    }

    // 绑定到 UI 的 Clear 按钮
    public void ClearWaypoints()
    {
        PaceStopSet(); 
        _hasAutoSnappedToStart = false; 
        _currentWaypointIndex = 0; 

        // 保留并清空所有的打点模式器
        if (LibraryRectangle.Instance != null) LibraryRectangle.Instance.ResetRectangle();
        if (NorthRectangle.Instance != null) NorthRectangle.Instance.ResetRectangle(); 
        if (AimTrackGenerator.Instance != null) AimTrackGenerator.Instance.ResetRectangle();
        
        if (WaypointRecorder.Instance != null) WaypointRecorder.Instance.ClearAllPoints();
        if (pathSmoother != null && pathSmoother.SmoothedWaypoints != null) pathSmoother.SmoothedWaypoints.Clear();

        Debug.Log("【PacePointManager 总控】所有点位记录已完全清除！可以重新设置长方形。");
    }
    
    public void ResetAutoSnapFlag() { _hasAutoSnappedToStart = false; }

    public void PaceUp() 
    { 
        float speedInMinuteKilometer = 60f / 3.6f / targetMovingSpeed; 
        speedInMinuteKilometer -= (5f / 60f); 
        if (speedInMinuteKilometer < 3f) speedInMinuteKilometer = 3f; 
        targetMovingSpeed = 60f / 3.6f / speedInMinuteKilometer;
        SyncRunningSpeed();
        SyncPaceSlider(speedInMinuteKilometer);
    }

    public void PaceDown() 
    {
        float speedInMinuteKilometer = 60f / 3.6f / targetMovingSpeed; 
        speedInMinuteKilometer += (5f / 60f); 
        if (speedInMinuteKilometer > 18f) speedInMinuteKilometer = 18f; 
        targetMovingSpeed = 60f / 3.6f / speedInMinuteKilometer;
        SyncRunningSpeed();
        SyncPaceSlider(speedInMinuteKilometer);
    }
    
    public void CadenceUp() { SetCadenceFromSlider(cadence + 1f); }
    public void CadenceDown() { SetCadenceFromSlider(cadence - 1f); }
    public void HeightUp() { sphereHeight += 0.20f; UpdateRelativeHeightCalibration(); }
    public void HeightDown() { sphereHeight -= 0.20f; UpdateRelativeHeightCalibration(); }

    private void UpdateRelativeHeightCalibration()
    {
        _lockedTargetSphereHeight = sphereHeight; 
        if (Camera.main != null && _camHeightHistory.Count > 0)
        {
            targetRelativeHeight = _currentAverageCamY - sphereHeight;
        }
    }

    private void SyncRunningSpeed() { if (isRunning) forwardMovingSpeed = targetMovingSpeed; }

    public void SetCadenceFromSlider(float value)
    {
        float steppedValue = Mathf.Round(value); 
        cadence = Mathf.Clamp(steppedValue, MinCadence, MaxCadence);
        if (cadenceSlider != null && cadenceSlider.value != cadence) cadenceSlider.SetValueWithoutNotify(cadence);
    }

    public void SetPaceFromSlider(float value)
    {
        float stepSize = 5f / 60f; 
        float steppedValue = Mathf.Round(value / stepSize) * stepSize;
        float speedInMinuteKilometer = Mathf.Clamp(steppedValue, 3f, 18f); 
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
    }

    private void SyncJumpStepDropdown()
    {
        if (jumpStepDropdown != null && jumpStepDropdown.value != (stepsPerJump - 1))
        {
            jumpStepDropdown.SetValueWithoutNotify(stepsPerJump - 1);
        }
    }
}
