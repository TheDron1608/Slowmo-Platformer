using System;
using UnityEngine;

public class CharacterAiming : AbstractCharacterComponent
{
    const float AIM_EQUAL_DELTA = 0.5f;
    const float AIM_DOWN_Y_AXIS = -1f;
    const float AIM_DOWN_FLASHLIGHT_Y_AXIS = -0.1f;

    public bool IsAbleToAim = true;
    public float AimSpeed = 35f;
    public GameObject Debug_CurrentAimIcon;

    private Vector2 _targetAimPoint;
    private Vector2 _currentAimPoint;
    private bool _aimPerformed = false;
    private bool _aimWeaponDown = false;

    public event EventHandler<bool> OnAimWeaponDownChanged;

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
    public bool AimPerformed
    {
        get => _aimPerformed;
        set => _aimPerformed = value;
    }
    public bool AimWeaponDown
    {
        get => _aimWeaponDown;
        set
        {
            if (CharComponents.CharacterClumsyness.ClumsyRangedAttack && !value && GetHoldingValidForAimWeapon())
            {
                CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.AIM;
            }
            else if (CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.AIM)
            {
                CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.NONE;
            }

            if (value != _aimWeaponDown)
            {
                OnAimWeaponDownChanged?.Invoke(this, value);
            }
            _aimWeaponDown = value;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _targetAimPoint = CharComponents.Center.transform.position;
        _currentAimPoint = CharComponents.Center.transform.position;
        CharComponents.CharacterHolding.OnPickedUpHoldable += CharacterHolding_OnPickedUpHoldable;
        CharComponents.CharacterHolding.OnThrewHoldable += CharacterHolding_OnThrewHoldable;
    }

    private void OnEnable()
    {
        _targetAimPoint = CharComponents.Center.transform.position;
        _currentAimPoint = CharComponents.Center.transform.position;
        CharComponents.CharacterHolding.OnPickedUpHoldable += CharacterHolding_OnPickedUpHoldable;
        CharComponents.CharacterHolding.OnThrewHoldable += CharacterHolding_OnThrewHoldable;
    }

    private void CharacterHolding_OnPickedUpHoldable(object sender, Holdable e)
    {
        if (CharComponents.CharacterClumsyness.ClumsyRangedAttack)
        {
            AimWeaponDown = GetHoldingValidForAimWeapon();
        }
        else
        {
            AimWeaponDown = false;
        }
    }

    private void CharacterHolding_OnThrewHoldable(object sender, CharacterHoldingObjects.OnThewEventArgs e)
    {
        if (CharComponents.CharacterClumsyness.ClumsyRangedAttack)
        {
            AimWeaponDown = true;
        }
    }

    private void Update()
    {
        if (!IsAbleToAim) return;

        if (Debug_CurrentAimIcon != null)
        {
            Debug_CurrentAimIcon.transform.position = VectorMath.Vec2ToVec3(_targetAimPoint, LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform.position.z);
        }

        if (
            AimWeaponDown &&
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.RotatableWhenIsHolded
            )
        {
            Vector2 targetAim = new Vector2(
                CharComponents.CharacterVisual.FlippedH ? -1f : 1f, 
                CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<FlashLightHoldable>() == null ? AIM_DOWN_Y_AXIS : AIM_DOWN_FLASHLIGHT_Y_AXIS
                ) + VectorMath.Vec3ToVec2(CharComponents.Center.transform.position);
            _currentAimPoint = Vector2.Lerp(_currentAimPoint, targetAim, AimSpeed * Time.deltaTime);
        }
        else
        {
            _currentAimPoint = Vector2.Lerp(_currentAimPoint, TargetAimPoint, AimSpeed * Time.deltaTime);
        }
    }

    public Vector2 GetCurrentAimNormalized()
    {
        return (CurrentAimPoint - VectorMath.Vec3ToVec2(CharComponents.Center.transform.position)).normalized;
    }
    public Vector2 GetTargetAimNormalized()
    {
        return (TargetAimPoint - VectorMath.Vec3ToVec2(CharComponents.Center.transform.position)).normalized;
    }

    public bool GetCurrentAimReachedTargetAim(float precission = AIM_EQUAL_DELTA)
    {
        return VectorMath.GetNormalizedVectorsEqual(CurrentAimPoint, TargetAimPoint, precission);
    }

    public bool GetCurrentAimReachedAimDown()
    {
        return VectorMath.GetNormalizedVectorsEqual(
            CurrentAimPoint,
            new Vector2(CharComponents.CharacterVisual.FlippedH ? -1f : 1f, -1f) + VectorMath.Vec3ToVec2(CharComponents.Center.transform.position),
            AIM_EQUAL_DELTA
            );
    }

    public bool GetHoldingValidForAimWeapon()
    {
        return CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<RangedWeapon>() != null && CharComponents.CharacterHolding.CurrentHoldObject.RotatableWhenIsHolded;
    }

    public void InstantMoveToTargetAim()
    {
        CurrentAimPoint = TargetAimPoint;
    }

    private void OnDisable()
    {
        CharComponents.CharacterHolding.OnPickedUpHoldable -= CharacterHolding_OnPickedUpHoldable;
        CharComponents.CharacterHolding.OnThrewHoldable -= CharacterHolding_OnThrewHoldable;
    }
}
