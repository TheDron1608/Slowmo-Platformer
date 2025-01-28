using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSingleProjectile : AbstractProjectile
{
    public float _damage = 1f;
    public float _attackCooldown = 0.25f; //in seconds
    /// <summary>
    /// 0 is perfect accuracy, 1 is 360deg spread
    /// </summary>
    public float _accuracy = 1;
    public float _knockBack = 0f;
    public ProjectilePiercing _pierce = ProjectilePiercing.NO_PIERCE;

    private CharacterHoldingObjects _owner;
    private Weapon _weapon;

    public override float Damage
    {
        get => _damage; set => _damage = value;
    }
    public override float AttackCooldown
    {
        get => _attackCooldown; set => _attackCooldown = value;
    }
    public override float Accuracy
    {
        get => _accuracy; set => _accuracy = value;
    }
    public override float KnockBack
    {
        get => _knockBack; set => _knockBack = value;
    }
    public override ProjectilePiercing Pierce {
        get => _pierce; set => _pierce = value;
    }
    public override CharacterHoldingObjects Owner
    {
        get => _owner;
    }
    public override Weapon Weapon
    {
        get => _weapon;
    }

    public override AbstractProjectile SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null)
    {
        _weapon = weapon;

        if (
            _weapon.TryGetComponent(out Holdable holdableWeapon) &&
            holdableWeapon.CurrentHolder != null &&
            holdableWeapon.CurrentHolder.TryGetComponent(out CharacterHoldingObjects ownerHoldaingObjectComponent)
            )
        {
            _owner = ownerHoldaingObjectComponent;
        }

        return null;
    }

    public void Remove()
    {
        Destroy(gameObject);
    }



    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        layer.UpdateLayerForGameObject(gameObject);
        if (!transform.parent.TryGetComponent(out AbstractCompositeProjectile compositeProjectile))
        {
            transform.parent = layer.transform;
        }
    }
}
