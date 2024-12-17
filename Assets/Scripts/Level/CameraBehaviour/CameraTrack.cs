using Unity.Mathematics;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    public GameObject TrackObject;
    public float TrackSpeed = 5f;
    public float TrackMouseVelocity = 0.1625f;
    public float LayerAppearDistance = 0.5f;

    private Rigidbody _rigidBodyComponent;
    private MultiZLayerCamera _multiZLayerCameraComponent;
    private Camera _cameraComponent;

    public void InstantMoveToTrackObject()
    {
        if (TrackObject == null) return;

        transform.position = new Vector3(
            TrackObject.transform.position.x,
            TrackObject.transform.position.y,
            TrackObject.transform.position.z - _multiZLayerCameraComponent.ZoomOutDistance
            );
    }

    private void LateUpdate()
    {
        UpdateCameraVecloity();
        UpdateLayerAlpha();
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

    private void UpdateLayerAlpha()
    {
        for (int i = 0; i < LayerManager.Instance.ZLayers.Count; i++)
        {
            float distanceToLayer = LayerManager.Instance.ZLayers[i].transform.position.z - transform.position.z - _cameraComponent.nearClipPlane;

            //if (LayerManager.Instance.ZLayers[i] == LayerManager.Instance.GetZLayerOfGameObject(TrackObject))
            //{
            //    Debug.Log("distance to " + LayerManager.Instance.ZLayers[i].gameObject.name + " is " + distanceToLayer);
            //}

            if ( distanceToLayer > 0f && distanceToLayer < LayerAppearDistance)
            {
                float targetLayerAlpha = distanceToLayer / LayerAppearDistance;

                LayerManager.Instance.ZLayers[i].Alpha = targetLayerAlpha;
            }
            else if (LayerManager.Instance.ZLayers[i].Alpha != 1f)
            {
                LayerManager.Instance.ZLayers[i].Alpha = 1f;
            }
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBodyComponent not found");
        if (!TryGetComponent(out _multiZLayerCameraComponent)) throw new UnityException("MultiZLayerCamera component not found");
        if (!TryGetComponent(out _cameraComponent)) throw new UnityException("Camera component not found");

        InstantMoveToTrackObject();
    }
}
