
using UnityEngine;

public class ProjectileExtraHitEffect : AbstractProjectileEffect, IMultiplierableEffect
{
    public AbstractEffect ExtraEffect;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        Projectile.OnHitSomeOne += Projectile_OnHitSomeOne;
    }

    private void Projectile_OnHitSomeOne(object sender, UnityEngine.GameObject e)
    {
        if (GameObjectUtility.TryGetComponentInSelfOrParent(e, out ObjectEffectsReceiver effectReceiver))
        {
            effectReceiver.ApplyEffect(ExtraEffect, Projectile, EffectMultiplier);
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        Projectile.OnHitSomeOne -= Projectile_OnHitSomeOne;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && (ExtraEffect?.Equals((other as ProjectileExtraHitEffect).ExtraEffect) ?? ExtraEffect == (other as ProjectileExtraHitEffect).ExtraEffect);
    }
}