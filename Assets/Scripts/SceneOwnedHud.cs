using UnityEngine;
using UnityEngine.SceneManagement;

// 必须早于 XREAL SingletonMonoBehaviour（-500），在父级被移入
// DontDestroyOnLoad 场景之前记录 HUD 原本所属的场景。
[DefaultExecutionOrder(-10000)]
[DisallowMultipleComponent]
public sealed class SceneOwnedHud : MonoBehaviour
{
    private int ownerSceneHandle;

    private void Awake()
    {
        ownerSceneHandle = gameObject.scene.handle;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.handle != ownerSceneHandle)
        {
            return;
        }

        // 即使相机被 SDK 保留，这份 HUD 也只属于原场景。
        // 先隐藏，避免延迟销毁期间与新场景 UI 同时渲染。
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
