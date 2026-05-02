using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    const float VELOCITY_FOR_MAX_CAMERA_ROTATION = 5f;

    public List<Transform> TrackTargets;
    public float TrackSpeed = 5f;
    public float TrackMouseVelocity = 0.1625f;
    public float CameraTrackRotatingDeg = 0f;
    public float DefaultCameraAngle = 0f;

    public float? LockPositionX = null;
    public float? LockPositionY = null;

    private Rigidbody _rigidBodyComponent;
    private MultiZLayerCamera _multiZLayerCameraComponent;
    private Vector3 _lastTrackPosition = Vector3.zero;
    private Vector3 _lastTrackAngle = Vector3.zero;

    public void InstantMoveToTrackObject()
    {
        if (TrackTargets == null) return;

        _lastTrackPosition = transform.position;
        transform.position = PickAvgTrackTargetsPosition();
    }

    public void InstantRotateToTrackVelocity()
    {
        Quaternion defaultRotation = new();
        defaultRotation.eulerAngles.Set(0f, 0f, DefaultCameraAngle);
        transform.rotation = defaultRotation;
        _lastTrackAngle = new Vector3(0f, 0f, DefaultCameraAngle);
    }

    public bool GetCameraFlipped()
    {
        return DefaultCameraAngle > 150f && DefaultCameraAngle < 210f;
    }

    private void Update()
    {
        if (TrackTargets == null)
        {
            _rigidBodyComponent.linearVelocity = 
                VectorMath.Vec3ToVec2(_lastTrackPosition - transform.position);
        }
        else
        {
            Vector3 trackTargetPosition = PickAvgTrackTargetsPosition();
            Vector2 trackTargetVelocity = PickAvgTrackTargetLinearVelocity();

            _rigidBodyComponent.linearVelocity =
                (trackTargetPosition - transform.position) * TrackSpeed;
            _lastTrackPosition = trackTargetPosition;

            Quaternion newAngle = new();
            Vector3 targetAngleVec3 = new(
                0f,
                0f,
                math.lerp(
                    _lastTrackAngle.z,
                    DefaultCameraAngle + (CameraTrackRotatingDeg * NumberMath.LimitFloatBetweenMinusOneAndOne(trackTargetVelocity.x / VELOCITY_FOR_MAX_CAMERA_ROTATION)),
                    Time.deltaTime
                    )
                );
            newAngle.eulerAngles = targetAngleVec3;
            transform.rotation = newAngle;
            _lastTrackAngle = targetAngleVec3;
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

    private Vector2 PickAvgTrackTargetLinearVelocity()
    {
        if (TrackTargets.Count != 0)
        {
            Vector2 result = Vector3.zero;
            foreach (Transform trackTarget in TrackTargets)
            {
                if (trackTarget.TryGetComponent(out Rigidbody2D rigidBody))
                {
                    result += rigidBody.linearVelocity;
                }
            }
            result /= TrackTargets.Count;

            return result;
        }
        else
        {
            return Vector2.zero;
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBodyComponent not found");
        if (!TryGetComponent(out _multiZLayerCameraComponent)) throw new UnityException("MultiZLayerCamera component not found");

        InstantMoveToTrackObject();
        InstantRotateToTrackVelocity();
    }
}
