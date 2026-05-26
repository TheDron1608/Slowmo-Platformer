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

        CharacterComponentsManager owner = null;

        if (Weapon.TryGetComponent(out Holdable holdableW))
        {
            owner = holdableW.CurrentHolder?.CharComponents;
        }
        else if (Weapon.TryGetComponent(out UnarmedWeapon unarmedW))
        {
            owner = unarmedW.CharComponents;
        }

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
                breakableWeapon.SpawnObjectsOnBreak.RemoveAll(e => e.TryGetComponent(out Weapon w));
            }

            if (_targetBreaker?.TryGetComponent(out AbstractCharacterComponent brekaerCharacter) ?? false)
            {
                breakableWeapon.BreakObject(brekaerCharacter.CharComponents.CharacterHolding);
            }
            else
            {
                breakableWeapon.BreakObject(null);
            }
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
