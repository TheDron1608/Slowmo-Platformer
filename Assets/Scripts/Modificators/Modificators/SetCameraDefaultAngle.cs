using UnityEngine;

public class SetCameraDefaultAngle : AbstractModificator
{
    public float CameraAngle = 0f;

    private float _defaultCameraAngle;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (SceneList.GetCurrentSceneIsGameplay())
        {
            _defaultCameraAngle = Camera.main.GetComponent<CameraTrack>().DefaultCameraAngle;
            Camera.main.GetComponent<CameraTrack>().DefaultCameraAngle = CameraAngle;
            Camera.main.GetComponent<CameraTrack>().InstantRotateToTrackVelocity();
        }
    }

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        _defaultCameraAngle = Camera.main.GetComponent<CameraTrack>().DefaultCameraAngle;
        Camera.main.GetComponent<CameraTrack>().DefaultCameraAngle = CameraAngle;
        Camera.main.GetComponent<CameraTrack>().InstantRotateToTrackVelocity();
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (Camera.main != null && Camera.main.TryGetComponent(out CameraTrack ct))
        {
            ct.DefaultCameraAngle = _defaultCameraAngle;
        }
    }
}