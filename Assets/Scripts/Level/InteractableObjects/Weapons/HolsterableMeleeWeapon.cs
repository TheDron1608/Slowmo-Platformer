using System.Collections;
using UnityEngine;

public class HolsterableMeleeWeapon : MeleeWeapon
{
    const string ANIMATOR_IS_HOLSTERED_PROP_NAME = "IsHolstered";

    [Header("holsterable melee weapon")]
    public float TimeToHolsterBackSeconds = 6.5f;
    public AbstractSoundPlayer SoundOnHolster;
    public AbstractSoundPlayer SoundOnUnholster;

    private bool _isHolstered = true;

    public bool IsHolstered
    {
        get => _isHolstered;
        set
        {
            if (_isHolstered == value) return;

            _animator.SetBool(ANIMATOR_IS_HOLSTERED_PROP_NAME, value);
            _isHolstered = value;
        }
    }

    public override void OnFinishAttack()
    {
        base.OnFinishAttack();

        IsHolstered = false;

        SoundOnHolster.BreakAllSounds();
        SoundOnUnholster.BreakAllSounds();
    }
}
