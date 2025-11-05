using Unity.VisualScripting;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    [SerializeField] private GameObject TrackObject;
    public float TrackSpeed = 5f;
    public float TrackMouseVelocity = 0.1625f;

    public float? LockPositionX = null;
    public float? LockPositionY = null;

    private Rigidbody _rigidBodyComponent;
    private MultiZLayerCamera _multiZLayerCameraComponent;
    private Vector3 _lastTrackPosition ;

    public void InstantMoveToTrackObject()
    {
        if (TrackObject == null) return;

        _lastTrackPosition = TrackObject.transform.position;
        transform.position = new Vector3(
            TrackObject.transform.position.x,
            TrackObject.transform.position.y,
            TrackObject.transform.position.z - _multiZLayerCameraComponent.ZoomOutDistance
            );
    }

    private void Update()
    {
        if (TrackObject == null)
        {
            _rigidBodyComponent.linearVelocity = VectorMath.Vec3ToVec2(_lastTrackPosition - transform.position) * TrackSpeed;
        }
        else
        {
            if (TrackObject.gameObject == null) Debug.Log("no target");
            Vector2 trackTargetPositionXY =
                !TrackObject.gameObject.IsUnityNull() ?
                TrackObject.transform.position - (TrackObject.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * TrackMouseVelocity * (1920f / Screen.width) :
                transform.position;

            Vector3 trackTargetPosition = new Vector3(
                LockPositionX.GetValueOrDefault(trackTargetPositionXY.x),
                LockPositionY.GetValueOrDefault(Mathf.Max(trackTargetPositionXY.y, LayerManager.Instance.GetLevelBottom())),
                TrackObject.transform.position.z - _multiZLayerCameraComponent.ZoomOutDistance
                );

            _rigidBodyComponent.linearVelocity = (trackTargetPosition - transform.position) * TrackSpeed;
            _lastTrackPosition = trackTargetPosition;
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBodyComponent not found");
        if (!TryGetComponent(out _multiZLayerCameraComponent)) throw new UnityException("MultiZLayerCamera component not found");

        _lastTrackPosition = transform.position;
        InstantMoveToTrackObject();
    }
}
