using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour
{
    public PhysicsParticle BulletCasingParticle;

    public AbstractProjectile SpawnProjectile(Vector2 direction, float accuracityMultiplier = 1, Weapon weapon = null)
    {
        return SpawnProjectile(VectorMath.Vec2ToQuarterninon2D(direction), accuracityMultiplier, weapon);
    }
    public abstract AbstractProjectile SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null);

    public enum ProjectilePiercing
    {
        NO_PIERCE,
        PIERCE_ARMOR,
        PIERCE_HEAVY_ARMOR
    }

    public abstract float Damage { get; set; }
    public abstract float AttackCooldown { get; set; } //in seconds
    /// <summary>
    /// 0 is perfect accuracy, 1 is 360deg spread
    /// </summary>
    public abstract float Accuracy { get; set; }
    public abstract float KnockBack { get; set; }
    public abstract ProjectilePiercing Pierce { get; set; }

    public abstract CharacterHoldingObjects Owner { get; }
    public abstract Weapon Weapon { get; }
}