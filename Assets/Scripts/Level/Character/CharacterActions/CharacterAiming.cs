using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class CharacterAiming : MonoBehaviour
{
    public bool IsAbleToAim = true;
    public float AimSpeed = 35f;

    private Vector2 _targetAimPoint;
    private Vector2 _currentAimPoint;

    private CharacterChildNodes _characterChildNodesComponent;

    public Vector2 TargetAimPoint
    {
        get => _targetAimPoint;
        set => _targetAimPoint = value;
    }
    public Vector2 CurrentAimPoint
    {
        get => _currentAimPoint;
        private set => _currentAimPoint = value;
    }

    private void Awake()
    {
        if (!TryGetComponent(out _characterChildNodesComponent)) throw new UnityException("CharacterChildNodes component not found");
        _targetAimPoint = transform.position;
        _currentAimPoint = transform.position;
    }

    private void Update()
    {
        if (!IsAbleToAim) return;

        _currentAimPoint = Vector2.Lerp(_currentAimPoint, TargetAimPoint, AimSpeed * Time.deltaTime);
    }

    public Vector2 GetCurrentAimNormalized()
    {
        return (CurrentAimPoint - VectorMath.Vec3ToVec2(_characterChildNodesComponent.Center.transform.position)).normalized;
    }
}
