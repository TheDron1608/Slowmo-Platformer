using Unity.Mathematics;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    public GameObject TrackObject;
    public float TrackSpeed = 5f;

    public bool FreezeXTransform = false;
    public bool FreezeYTransform = false;

    private GameObject _trackObject;

    private Rigidbody2D _rigidBodyComponent;

    public void InstantMoveToTrackObject()
    {
        if (TrackObject == null) return;

        transform.position = new Vector3(
            TrackObject.transform.position.x,
            TrackObject.transform.position.y,
            transform.position.z
            );
    }

    private void LateUpdate()
    {
        if (TrackObject == null) return;

        if (!FreezeXTransform)
        {
            _rigidBodyComponent.linearVelocityX = (TrackObject.transform.position.x - transform.position.x) * TrackSpeed;
        }
        if (!FreezeYTransform)
        {
            _rigidBodyComponent.linearVelocityY = (TrackObject.transform.position.y - transform.position.y) * TrackSpeed;
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBodyComponent not found");

        InstantMoveToTrackObject();
    }
}
