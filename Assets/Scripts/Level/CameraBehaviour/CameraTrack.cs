using Unity.Mathematics;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    public GameObject TrackObject;
    public float TrackSpeed = 5f;

    public float TrackMouseVelocity = 0.1625f;
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

        Vector2 trackTargetPosition = TrackObject.transform.position - (TrackObject.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * TrackMouseVelocity * (1920f / Screen.width);

        if (!FreezeXTransform)
        {
            _rigidBodyComponent.linearVelocityX = (trackTargetPosition.x - transform.position.x) * TrackSpeed;
        }
        if (!FreezeYTransform)
        {
            _rigidBodyComponent.linearVelocityY = (trackTargetPosition.y - transform.position.y) * TrackSpeed;
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBodyComponent not found");

        InstantMoveToTrackObject();
    }
}
