using UnityEngine;
using System.Collections.Generic;

public class WaypointRecorder : MonoBehaviour
{
    public static WaypointRecorder Instance { get; private set; }

    [Header("打点设置")]
    [Tooltip("用于在空间中标记位置的可见小球 Prefab")]
    public GameObject waypointMarkerPrefab; 
    
    [Tooltip("存储所有打好的真实世界坐标")]
    public List<Vector3> recordedWorldPoints = new List<Vector3>();

    // 用于存储生成在场景里的实体标记球
    private List<GameObject> spawnedMarkers = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RecordCurrentPosition()
    {
        if (Camera.main != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            Vector3 dropPos = new Vector3(camPos.x, 0, camPos.z);

            recordedWorldPoints.Add(dropPos);

            if (waypointMarkerPrefab != null)
            {
                GameObject newMarker = Instantiate(waypointMarkerPrefab, dropPos, Quaternion.identity);
                spawnedMarkers.Add(newMarker); 
            }
        }
    }

    /// <summary>
    /// 【防呆升级】：无论你的 UI 按钮绑定的谁，这里都会联动清理全场数据。
    /// 确保你随时可以重新进行 LibraryRectangle 打点。
    /// </summary>
    public void ClearAllPoints()
    {
        // 1. 销毁自己的小球并清空数据
        foreach (GameObject marker in spawnedMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        spawnedMarkers.Clear();
        recordedWorldPoints.Clear();

        // 2. 联动强制清理长方形数据 (A, B参考点)
        if (LibraryRectangle.Instance != null)
        {
            LibraryRectangle.Instance.ResetRectangle();
        }

        // 3. 联动强制清理平滑路径并重置管理器的状态
        if (PacePointManager.Instance != null)
        {
            PacePointManager.Instance.ResetAutoSnapFlag();
            if (PacePointManager.Instance.pathSmoother != null)
            {
                PacePointManager.Instance.pathSmoother.SmoothedWaypoints.Clear();
            }
        }

        Debug.Log("[WaypointRecorder] 防呆机制生效，已彻底清空所有打点记录，现在可以重新设置长方形了！");
    }
}