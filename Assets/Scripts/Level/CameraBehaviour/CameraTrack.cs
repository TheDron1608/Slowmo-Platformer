using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    public GameObject TrackObject;
    public float TrackSpeed = 5f;
    public float TrackMouseVelocity = 0.1625f;

    private Rigidbody _rigidBodyComponent;
    private MultiZLayerCamera _multiZLayerCameraComponent;

    public void InstantMoveToTrackObject()
    {
        if (TrackObject == null) return;

        transform.position = new Vector3(
            TrackObject.transform.position.x,
            TrackObject.transform.position.y,
            TrackObject.transform.position.z - _multiZLayerCameraComponent.ZoomOutDistance
            );
    }

    private void Update()
    {
        UpdateCameraVecloity();
    }

    private void UpdateCameraVecloity()
    {
        if (TrackObject == null) return;

        Vector2 trackTargetPositionXY = TrackObject.transform.position - (TrackObject.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * TrackMouseVelocity * (1920f / Screen.width);
        Vector3 trackTargetPosition = new Vector3(
            trackTargetPositionXY.x,
            trackTargetPositionXY.y,
            TrackObject.transform.position.z - _multiZLayerCameraComponent.ZoomOutDistance
            );

        _rigidBodyComponent.linearVelocity = (trackTargetPosition - transform.position) * TrackSpeed;
    }

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBodyComponent not found");
        if (!TryGetComponent(out _multiZLayerCameraComponent)) throw new UnityException("MultiZLayerCamera component not found");

        InstantMoveToTrackObject();
    }
}
