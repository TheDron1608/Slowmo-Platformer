using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour
{
    public float Accuracy = 1f;
    public float KnockBack = 0f;

    private Weapon _weapon;
    private CharacterHoldingObjects _owner;

    public Weapon Weapon
    {
        get => _weapon;
        protected set => _weapon = value;
    }
    public CharacterHoldingObjects Owner
    {
        get => _owner;
        protected set => _owner = value;
    }

    public List<AbstractProjectile> SpawnProjectile(Vector2 direction, float accuracityMultiplier = 1f, Weapon weapon = null)
    {
        return SpawnProjectile(VectorMath.Vec2ToQuarterninon2D(direction), accuracityMultiplier, weapon);
    }

    public abstract List<AbstractProjectile> SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null);

    public void RemoveSelf()
    {
        Destroy(gameObject);
    }
}
