using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterBleedTeleportation : AbstractCharacterSpecial
{
    public List<AbstractEffect> EffectsOnTeleportIntoCharacter;
    public bool DropWeaponOnTeleport = false;

    [SerializeField] private BleedTeleportationVisualEffect _visualEffect;
    private bool _isTeleporting;
    private Holdable _teleportingHoldable = null;

    public bool IsTeleporting
    {
        get => _isTeleporting;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        _visualEffect.TeleportationUser = this;
    }

    public bool TryTeleport(CharacterComponentsManager teleportInto)
    {
        if (
            !IsAbleToDoSpecial ||
            !CharComponents.gameObject.activeSelf ||
            teleportInto == null ||
            teleportInto == CharComponents
            )
        {
            return false;
        }

        if (!GetHasEnoughForCost()) return false;

        _isTeleporting = true;

        if (DropWeaponOnTeleport)
        {
            CharComponents.CharacterHolding.ForceDisarm();
        }
        else if (CharComponents.CharacterHolding.CurrentHoldObject != null)
        {
            _teleportingHoldable = CharComponents.CharacterHolding.CurrentHoldObject;

            if (_teleportingHoldable.TryGetComponent(out DisableObjectOnDistanceFromCamera holdableDisabler))
            {
                holdableDisabler.enabled = false;
            }
            holdableDisabler.gameObject.SetActive(false);
        }

        TrySetDisableOnDistanceActive(false);

        _visualEffect.transform.position = CharComponents.Center.transform.position;
        _visualEffect.TargetTeleportTo = teleportInto;
        _visualEffect.gameObject.SetActive(true);

        CharComponents.gameObject.SetActive(false);

        SpendCost();
        return true;
    }

    public bool TryFinishTeleport(CharacterComponentsManager teleportInto)
    {
        if (!_isTeleporting) return false;

        _visualEffect.transform.SetParent(transform);
        _visualEffect.transform.position = CharComponents.Center.transform.position;
        _visualEffect.gameObject.SetActive(false);

        if (_teleportingHoldable != null)
        {
            _teleportingHoldable.transform.position = CharComponents.Center.transform.position;

            if (_teleportingHoldable.TryGetComponent(out DisableObjectOnDistanceFromCamera holdableDisabler))
            {
                holdableDisabler.enabled = true;
            }
            holdableDisabler.gameObject.SetActive(true);

            _teleportingHoldable = null;
        }

        if (teleportInto != null && !teleportInto.IsDestroyed())
        {
            CharComponents.transform.position = teleportInto.transform.position;
            LayerManager.Instance.ChangeZIndexForGameObject(teleportInto.CharacterCollision.CurrentZLayer, CharComponents.gameObject);

            teleportInto.CharacterEffectsReceiver.ApplyEffect(EffectsOnTeleportIntoCharacter, this);
        }

        _isTeleporting = false;

        CharComponents.gameObject.SetActive(true);
        TrySetDisableOnDistanceActive(true);

        return true;
    }

    private void TrySetDisableOnDistanceActive(bool value)
    {
        if (CharComponents.TryGetComponent(out DisableObjectOnDistanceFromCamera disableOnDistance))
        {
            disableOnDistance.enabled = value;
        }
    }

    private void OnDestroy()
    {
        if (_visualEffect != null && !_visualEffect.IsDestroyed())
        {
            Destroy(_visualEffect.gameObject);
        }
    }
}