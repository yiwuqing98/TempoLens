using UnityEngine;

public class VirtualControllerLinker : MonoBehaviour
{
    // 利用单例直接调用你的方法。
    // 加入 ?. 操作符防止在某些没有 PacePointManager 的场景下报错
    public void OnPaceStartSet() => PacePointManager.Instance?.PaceStartSet();
    public void OnPaceStopSet() => PacePointManager.Instance?.PaceStopSet();
    
    public void OnPaceUp() => PacePointManager.Instance?.PaceUp();
    public void OnPaceDown() => PacePointManager.Instance?.PaceDown();
    
    public void OnCadenceUp() => PacePointManager.Instance?.CadenceUp();
    public void OnCadenceDown() => PacePointManager.Instance?.CadenceDown();
    
    public void OnHeightUp() => PacePointManager.Instance?.HeightUp();
    public void OnHeightDown() => PacePointManager.Instance?.HeightDown();

    public void OnSphereReset() => PacePointManager.Instance?.SphereReset();

    // 接收滑块传来的动态 float 值，并传递给 Manager
    public void OnCadenceSliderChanged(float value) => PacePointManager.Instance?.SetCadenceFromSlider(value);

    public void OnPaceSliderChanged(float value) => PacePointManager.Instance?.SetPaceFromSlider(value);

    public void OnSceneClick() => SceneSwitcher.Instance?.SceneClick();

    public void OnWaypointRecorder() => WaypointRecorder.Instance?.RecordCurrentPosition();

    public void OnClearAllPoints() => WaypointRecorder.Instance?.ClearAllPoints();
    
    public void OnSetStepsFromDropdown (int dropdownIndex) => PacePointManager.Instance?.SetStepsFromDropdown(dropdownIndex);
    public void OnGPSCalibration() => GPSManager.Instance?.RelocateScene();
    public void OnGPSLibrary() => GPSMotionAligner.Instance?.OnClickGenerateWaypoints();
    public void OnGenerateLibraryRectangle() => LibraryRectangle.Instance?.OnClickRecord();
    public void OnGenerateNorthRectangle() => NorthRectangle.Instance?.OnClickRecord();
    public void OnGenerateAimTrack() => AimTrackGenerator.Instance?.OnClickRecord();
}