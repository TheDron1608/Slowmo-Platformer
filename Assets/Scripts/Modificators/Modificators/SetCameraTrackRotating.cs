using UnityEngine;

public class SetCameraTrackRotating : AbstractModificator
{
    public float TrackRotatingDeg = 0f;

    private float _defaultTrackRotatingDeg;

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        _defaultTrackRotatingDeg = Camera.main.GetComponent<CameraTrack>().CameraTrackRotatingDeg;
        Camera.main.GetComponent<CameraTrack>().CameraTrackRotatingDeg = TrackRotatingDeg;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (Camera.main != null)
        {
            Camera.main.GetComponent<CameraTrack>().CameraTrackRotatingDeg = _defaultTrackRotatingDeg;
        }
    }
}