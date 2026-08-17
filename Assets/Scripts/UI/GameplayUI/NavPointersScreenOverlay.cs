using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class NavPointersScreenOverlay : MonoBehaviour
{
    public static NavPointersScreenOverlay Instance = null;

    public float PointerMoveSpeed = 10f;
    public float FixedPointerPositionMargin = 100f;

    [SerializeField] private Image _pointerInstance;
    [SerializeField] private Transform _pointersSpawnPoint;

    private List<INavPointersScreenOverlayTrackableObject> _trackedObjects = new();
    private List<Image> _currentPointers = new();
    private ZIndexLayer _currentTrackedLayer = null;

    private void Awake()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnObjectSpawned += Instance_OnObjectSpawned;
        }

        Instance = this;
    }

    private void Instance_OnObjectSpawned(object sender, GameObject e)
    {
        if (
            e.TryGetComponent(out INavPointersScreenOverlayTrackableObject navTrackedObj) &&
            navTrackedObj.PointingCondition() &&
            LayerManager.Instance.GetZLayerOfGameObject(e) == _currentTrackedLayer
            )
        {
            _trackedObjects.Add(navTrackedObj);
        }
    }

    public void UpdateNavTargets()
    {
        if (Camera.main == null || !Camera.main.TryGetComponent(out MultiZLayerCamera layerCamera) || layerCamera.CurrentZLayer == null) return;

        _currentTrackedLayer = layerCamera.CurrentZLayer;
        _trackedObjects.Clear();

        foreach (Transform furniture in _currentTrackedLayer.FurnitureContainer)
        {
            if (furniture.TryGetComponent(out INavPointersScreenOverlayTrackableObject navTrackedObj))
            {
                if (navTrackedObj.PointingCondition())
                {
                    _trackedObjects.Add(navTrackedObj);
                }
                else
                {
                    _trackedObjects.Remove(navTrackedObj);
                }
            }
        }
    }

    private void Update()
    {
        if (Camera.main == null || !Camera.main.TryGetComponent(out MultiZLayerCamera layerCamera) || layerCamera.CurrentZLayer == null) return;

        if (layerCamera.CurrentZLayer != _currentTrackedLayer)
        {
            UpdateNavTargets();

            foreach (Image pointer in _currentPointers)
            {
                pointer.transform.position = _pointersSpawnPoint.position;
            }
        }

        for (int i = 0; i < MathF.Max(_trackedObjects.Count, _currentPointers.Count); i++)
        {
            if (_trackedObjects.Count <= i)
            {
                if (_currentPointers.Count > i)
                {
                    _currentPointers[i].gameObject.SetActive(false);
                }
            }
            else
            {
                if (_currentPointers.Count - 1 < i)
                {
                    _currentPointers.Add(Instantiate(
                        _pointerInstance,
                        _pointersSpawnPoint.transform.position,
                        transform.rotation,
                        transform
                        ));
                }
                _currentPointers[i].gameObject.SetActive(true);

                Vector2 trackedObjectViewportPoint = Camera.main.WorldToScreenPoint((_trackedObjects[i] as MonoBehaviour).transform.position);
                Vector2 trackedObjectViewportPointWithOffset = trackedObjectViewportPoint + Vector2.up * _trackedObjects[i].GetOffsetForPointerPosition();

                if (
                    trackedObjectViewportPointWithOffset.x < FixedPointerPositionMargin ||
                    trackedObjectViewportPointWithOffset.x > Screen.width - FixedPointerPositionMargin ||
                    trackedObjectViewportPointWithOffset.y < FixedPointerPositionMargin ||
                    trackedObjectViewportPointWithOffset.y > Screen.height - FixedPointerPositionMargin
                    )
                {
                    Vector2 targetDirection = ((_trackedObjects[i] as MonoBehaviour).transform.position - layerCamera.transform.position).normalized;

                    _currentPointers[i].transform.position = math.lerp(
                        _currentPointers[i].transform.position,
                        _pointersSpawnPoint.position + new Vector3(targetDirection.x * Screen.width / 2f, targetDirection.y * Screen.height / 2f, 0f),
                        Time.deltaTime * PointerMoveSpeed
                        );
                    _currentPointers[i].transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(targetDirection);
                }
                else
                {
                    _currentPointers[i].transform.position = math.lerp(
                        _currentPointers[i].transform.position,
                        VectorMath.Vec2ToVec3(trackedObjectViewportPointWithOffset),
                        Time.deltaTime * PointerMoveSpeed
                        );
                    _currentPointers[i].transform.rotation = 
                        VectorMath.Vec2ToQuaternion2DNoMirroring(trackedObjectViewportPoint - VectorMath.Vec3ToVec2(_currentPointers[i].transform.position));
                }

                if ((_trackedObjects[i] as MonoBehaviour).TryGetComponent(out SpriteRenderer renderer))
                {
                    _currentPointers[i].material = renderer.material;
                }
                else
                {
                    _currentPointers[i].material = null;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnObjectSpawned -= Instance_OnObjectSpawned;
        }
        Instance = null;
    }
}