using UnityEngine;

public class MeleeProjectile : AbstractSingleProjectile
{
    public enum RangedProjectileDeflectionType
    {
        NO_DEFLECT,
        ABSORB_PROJECTILE,
        DEFLECT_PROJECTILE
    }
    public enum MeleerojectileDeflectionType
    {
        NO_DEFLECT,
        ABSORB_PROJECTILE,
        RESET_COOLDOWN,
        DISARM
    }
}
