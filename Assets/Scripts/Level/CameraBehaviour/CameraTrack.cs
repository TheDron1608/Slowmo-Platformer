using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    public List<Transform> TrackTargets;
    public float TrackSpeed = 5f;
    public float TrackMouseVelocity = 0.1625f;

    public float? LockPositionX = null;
    public float? LockPositionY = null;

    private Rigidbody _rigidBodyComponent;
    private MultiZLayerCamera _multiZLayerCameraComponent;
    private Vector3 _lastTrackPosition ;

    public void InstantMoveToTrackObject()
    {
        if (TrackTargets == null) return;

        _lastTrackPosition = transform.position;
        transform.position = PickAvgTrackTargetsPosition();
    }

    private void Update()
    {
        if (TrackTargets == null)
        {
            _rigidBodyComponent.linearVelocity = VectorMath.Vec3ToVec2(_lastTrackPosition - transform.position) * TrackSpeed;
        }
        else
        {
            Vector3 trackTargetPosition = PickAvgTrackTargetsPosition();

            _rigidBodyComponent.linearVelocity = (trackTargetPosition - transform.position) * TrackSpeed;
            _lastTrackPosition = trackTargetPosition;
        }
    }

    private Vector3 PickAvgTrackTargetsPosition()
    {
        if (TrackTargets.Count != 0)
        {
            Vector2 resultXY = Vector3.zero;
            float targetZPosition = LayerManager.Instance.GetZLayerOfGameObject(TrackTargets.First().gameObject).transform.position.z;
            foreach (Transform trackTarget in TrackTargets)
            {
                resultXY += VectorMath.Vec3ToVec2(trackTarget.position);
                targetZPosition = math.min(targetZPosition, LayerManager.Instance.GetZLayerOfGameObject(trackTarget.gameObject).transform.position.z);
            }
            resultXY /= TrackTargets.Count;

            return VectorMath.Vec2ToVec3(
                new Vector2(
                    LockPositionX.GetValueOrDefault(resultXY.x),
                    LockPositionY.GetValueOrDefault(Mathf.Max(resultXY.y, LayerManager.Instance.GetLevelBottom()))
                    ),
                targetZPosition - _multiZLayerCameraComponent.ZoomOutDistance
                );
        }
        else
        {
            return transform.position;
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
