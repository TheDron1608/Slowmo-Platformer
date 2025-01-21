using System.Collections;
using UnityEngine;

public class HolsterableMeleeWeapon : MeleeWeapon
{
    const string ANIMATOR_IS_HOLSTERED_PROP_NAME = "IsHolstered";

    [Header("holsterable melee weapon")]
    public float TimeToHolsterBackSeconds = 6.5f;

    private bool _isHolstered = false;
    private float _timeToHolsterBack = 0f;
    private Coroutine _currentHolsterBackCoroutine;

    public bool IsHolstered
    {
        get => _isHolstered;
        set
        {
            _animator.SetBool(ANIMATOR_IS_HOLSTERED_PROP_NAME, value);
            _isHolstered = value;
        }
    }

    protected override bool OnTryAttackSuccess()
    {
        if (!base.OnTryAttackSuccess()) return false;

        if (_currentHolsterBackCoroutine != null)
        {
            StopCoroutine(_currentHolsterBackCoroutine);
        }
        IsHolstered = false;
        _currentHolsterBackCoroutine = StartCoroutine(AwaitTimeAndHolsterBack());

        return true;
    }

    protected override void OnThrow()
    {
        base.OnThrow();

        if (_currentHolsterBackCoroutine != null)
        {
            StopCoroutine(_currentHolsterBackCoroutine);
        }
    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();

        if (LastHolder != CurrentHolder)
        {
            IsHolstered = false;
        }
    }

    private IEnumerator AwaitTimeAndHolsterBack()
    {
        _timeToHolsterBack = TimeToHolsterBackSeconds;
        while (_timeToHolsterBack > 0f)
        {
            yield return new WaitForEndOfFrame();

            _timeToHolsterBack -= Time.deltaTime;
        }
        IsHolstered = true;

        _currentHolsterBackCoroutine = null;
    }
}
