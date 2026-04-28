using UnityEngine;

public class CameraTrackObject : MonoBehaviour
{
    private void OnEnable()
    {
        Camera.main?.GetComponent<CameraTrack>().TrackTargets.Add(transform);
    }

    private void OnDisable()
    {
        Camera.main?.GetComponent<CameraTrack>().TrackTargets.Remove(transform);
    }
}