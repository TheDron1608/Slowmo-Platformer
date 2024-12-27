using System.Collections;
using UnityEngine;

public class HolsterableMeleeWeapon : MeleeWeapon
{
    const string ANIMATOR_IS_HOLSTERED_PROP_NAME = "IsHolstered";

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

    protected override void OnAttack()
    {
        base.OnAttack();

        if (_currentHolsterBackCoroutine != null)
        {
            StopCoroutine(_currentHolsterBackCoroutine);
        }
        IsHolstered = false;
        _currentHolsterBackCoroutine = StartCoroutine(AwaitTimeAndHolsterBack());
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
