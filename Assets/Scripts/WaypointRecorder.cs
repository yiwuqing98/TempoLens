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

    // --- 新增：用于存储生成在场景里的实体标记球 ---
    private List<GameObject> spawnedMarkers = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 绑定给你的 UI 按钮或手柄按键，按一次记录一个点
    /// </summary>
    public void RecordCurrentPosition()
    {
        if (Camera.main != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            // 强制 Y 轴为 0
            Vector3 dropPos = new Vector3(camPos.x, 0, camPos.z);

            // 1. 存入坐标列表
            recordedWorldPoints.Add(dropPos);

            // 2. 生成实体小球，并存入实体列表
            if (waypointMarkerPrefab != null)
            {
                GameObject newMarker = Instantiate(waypointMarkerPrefab, dropPos, Quaternion.identity);
                spawnedMarkers.Add(newMarker); // 存起来，为了重置的时候能找到它并销毁
            }

            Debug.Log($"成功记录第 {recordedWorldPoints.Count} 个点：{dropPos}");
        }
    }

    /// <summary>
    /// --- 核心升级：彻底清空数据和实体 ---
    /// </summary>
    public void ClearAllPoints()
    {
        // 1. 遍历销毁场景里的所有标记小球
        foreach (GameObject marker in spawnedMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
        
        // 2. 清空实体列表
        spawnedMarkers.Clear();

        // 3. 清空坐标数据列表
        recordedWorldPoints.Clear();

        Debug.Log("已彻底清空所有打点记录，并销毁了场景中的标记球！");
        
        // 告诉 PacePointManager，路线清空了，下次按开始时需要重新防呆传送
        if (PacePointManager.Instance != null)
        {
            PacePointManager.Instance.ResetAutoSnapFlag();
        }

        Debug.Log("已彻底清空所有打点记录，并重置了防呆状态！");
    }
}