using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// used for pump shotguns and sniper rifles
/// </summary>
public class BoltReloadingWeapon : BulletReloadingWeapon
{
    const string ANIMATOR_UNLOAD_BULLET_TRIGGER_NAME = "UnloadBullet";
    const string ANIMATOR_OUT_OF_AMMO_PROP_NAME = "OutOfAmmo";
    const string ANIMATOR_ATTACK_COOLDOWN_MULTIPLIER_PROP_NAME = "AttackCooldownMultiplier";
    const float WAIT_DURATION_TO_UNLOAD_BULLET_AFTER_ATTACK = 0.25f; //in seconds
    private static readonly string[] ATTACK_COOLDOWN_ANIMATON_CLIP_NAMES = new string[] { "Load", "Unload" };

    private bool _outOfAmmo = false;
    private float _loadBulletAnimationClipsDuration; //in seconds

    public bool OutOfAmmo
    {
        get => _outOfAmmo;
        set
        {
            _animator.SetBool(ANIMATOR_OUT_OF_AMMO_PROP_NAME, value);
            _outOfAmmo = value;
        }
    }

    public override float AttackCooldownMultiplier
    { 
        get => base.AttackCooldownMultiplier;
        set
        {
            base.AttackCooldownMultiplier = value;
            UpdateAnimatorAttackCooldownMultiplier();
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        UpdateLoadBulletAnimatioClipsDuration();
        UpdateAnimatorAttackCooldownMultiplier();
    }
    private void UpdateLoadBulletAnimatioClipsDuration()
    {
        _loadBulletAnimationClipsDuration = 0f;
        AnimationClip[] clipInfos = _animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clipInfos.Length; i++)
        {
            if (ATTACK_COOLDOWN_ANIMATON_CLIP_NAMES.Contains(clipInfos[i].name))
            {
                _loadBulletAnimationClipsDuration += clipInfos[i].length;
            }
        }
    }
    private void UpdateAnimatorAttackCooldownMultiplier()
    {
        _animator.SetFloat(ANIMATOR_ATTACK_COOLDOWN_MULTIPLIER_PROP_NAME, 1 / ((AttackCooldownMultiplier * AttackCooldown) / _loadBulletAnimationClipsDuration) - WAIT_DURATION_TO_UNLOAD_BULLET_AFTER_ATTACK);
    }

    public void UnloadBullet()
    {
        _animator.SetTrigger(ANIMATOR_UNLOAD_BULLET_TRIGGER_NAME);
    }

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        base.OnTryAttackSuccess(direction);

        StartCoroutine(UnloadBulletAfterDelay());
        return true;
    }

    public override bool AttackCondition()
    {
        return base.AttackCondition() && LoadedSpentAmmoLeft < 1;
    }

    private IEnumerator UnloadBulletAfterDelay()
    {
        yield return new WaitForSeconds(WAIT_DURATION_TO_UNLOAD_BULLET_AFTER_ATTACK);
        UnloadBullet();
    }

    public override void OnLoadFinish()
    {
        base.OnLoadFinish();

        if (LoadedLivingAmmoLeft >= MaxLoadedAmmo || AmmoLeft <= 1)
        {
            OutOfAmmo = false;
        }
    }

    public override void OnUnloadFinish()
    {
        base.OnUnloadFinish();

        if (AmmoLeft <= 0 && LoadedLivingAmmoLeft <= 0)
        {
            OutOfAmmo = true;
        }
    }
}
