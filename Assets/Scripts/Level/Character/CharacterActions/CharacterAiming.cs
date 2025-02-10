using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class CharacterAiming : AbstractCharacterComponent
{
    public bool IsAbleToAim = true;
    public float AimSpeed = 35f;
    public GameObject Debug_CurrentAimIcon;

    private Vector2 _targetAimPoint;
    private Vector2 _currentAimPoint;

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

    protected override void OnAwake()
    {
        base.OnAwake();
        _targetAimPoint = _charComponents.Center.transform.position;
        _currentAimPoint = _charComponents.Center.transform.position;
    }

    private void Update()
    {
        if (!IsAbleToAim) return;

        if (Debug_CurrentAimIcon != null)
        {
            Debug_CurrentAimIcon.transform.position = _targetAimPoint;
        }

        _currentAimPoint = Vector2.Lerp(_currentAimPoint, TargetAimPoint, AimSpeed * Time.deltaTime);
    }

    public Vector2 GetCurrentAimNormalized()
    {
        return (CurrentAimPoint - VectorMath.Vec3ToVec2(_charComponents.Center.transform.position)).normalized;
    }
}
