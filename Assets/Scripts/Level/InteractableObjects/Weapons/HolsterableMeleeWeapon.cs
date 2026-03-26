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
    private float _timeToHolsterBack = 0f;
    private int _comboPrevFrame = 0;

    public bool IsHolstered
    {
        get => _isHolstered;
        set
        {
            if (_isHolstered == value) return;
            if (value) Debug.Log(value);

            _animator.SetBool(ANIMATOR_IS_HOLSTERED_PROP_NAME, value);
            _isHolstered = value;
        }
    }

    public override void OnFinishAttack()
    {
        base.OnFinishAttack();

        IsHolstered = false;
        _timeToHolsterBack = TimeToHolsterBackSeconds;
    }

    private void Update()
    {
        if (TryGetComponent(out Holdable holdable) && holdable.CurrentHolder != null)
        {
            if (holdable.CurrentHolder.CharComponents.CharacterTeam.Team != ScoreManager.TRACKED_TEAM)
            {
                _timeToHolsterBack -= Time.deltaTime;
                if (_timeToHolsterBack < 0f)
                {
                    IsHolstered = true;
                }
            }
            else if (!IsInCooldown)
            {
                if (ScoreManager.Instance.CurrentCombo == 0 && _comboPrevFrame != 0)
                {
                    IsHolstered = true;
                }
                _comboPrevFrame = ScoreManager.Instance.CurrentCombo;
            }
        }
    }
}
