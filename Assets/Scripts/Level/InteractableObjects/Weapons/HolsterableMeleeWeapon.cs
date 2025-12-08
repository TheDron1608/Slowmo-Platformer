using System.Collections;
using UnityEngine;

public class HolsterableMeleeWeapon : MeleeWeapon
{
    const string ANIMATOR_IS_HOLSTERED_PROP_NAME = "IsHolstered";

    [Header("holsterable melee weapon")]
    public float TimeToHolsterBackSeconds = 6.5f;
    public AbstractSoundPlayer SoundOnHolster;
    public AbstractSoundPlayer SoundOnUnholster;

    private bool _isHolstered = false;
    private float _timeToHolsterBack = 0f;
    private Coroutine _currentHolsterBackCoroutine;

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

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        if (!base.OnTryAttackSuccess(direction)) return false;

        if (_currentHolsterBackCoroutine != null)
        {
            StopCoroutine(_currentHolsterBackCoroutine);
        }

        return true;
    }

    public override void OnFinishAttack()
    {
        base.OnFinishAttack();

        IsHolstered = false;
    }

    //unused for now
    private IEnumerator AwaitTimeAndHolsterBack()
    {
        _timeToHolsterBack = TimeToHolsterBackSeconds;
        while (_timeToHolsterBack > 0f)
        {
            yield return new WaitForFixedUpdate();

            _timeToHolsterBack -= Time.fixedDeltaTime;
        }

        if (TryGetComponent(out Holdable holdable) && holdable.CurrentHolder != null)
        {
            IsHolstered = true;
            _currentHolsterBackCoroutine = null;
        }
    }
}
