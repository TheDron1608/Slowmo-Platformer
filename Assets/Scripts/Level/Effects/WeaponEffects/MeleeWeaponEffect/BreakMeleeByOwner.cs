using Unity.VisualScripting;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class BreakMeleeByOwner : AbstractMeleeWeaponEffectWithSender
{
    public float BreakDelay = 0.5f;
    public bool IncludeDestroyBrokenWeapon = true;

    private float _timeSpent = 0f;
    private AbstractCharacterComponent _targetBreaker;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        _targetBreaker = sender.GetComponent<AbstractCharacterComponent>();
        Weapon.IsAbleToAttack = false;
    }

    private void FixedUpdate()
    {
        _timeSpent += Time.fixedDeltaTime;

        CharacterComponentsManager owner = 
            Weapon.GetComponent<Holdable>()?.CurrentHolder.CharComponents ?? 
            Weapon.GetComponent<UnarmedWeapon>()?.CharComponents;

        if (_targetBreaker != null && owner == _targetBreaker.CharComponents)
        {
            if (_timeSpent > BreakDelay)
            {
                OnTimeOut();
            }
        }
        else
        {
            OnTimeOut();
        }
    }

    private void OnTimeOut()
    {
        if (MeleeWeapon.TryGetComponent(out BreakableHoldable breakableWeapon))
        {
            if (IncludeDestroyBrokenWeapon)
            {
                breakableWeapon.SpawnObjectsOnBreak.RemoveAll(e => e.GetComponent<Weapon>() != null);
            }
            breakableWeapon.BreakObject(_targetBreaker?.GetComponent<AbstractCharacterComponent>()?.CharComponents.CharacterHolding);
        }
        else
        {
            Destroy(MeleeWeapon.gameObject);
        }

        RemoveSelf();
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (!Weapon.IsDestroyed())
        {
            Weapon.IsAbleToAttack = true;
        }
    }
}
