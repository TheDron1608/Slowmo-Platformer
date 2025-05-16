using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour
{
    public float Accuracy = 1f;
    public List<AbstractEffect> HitEffects = new();
    public List<AbstractEffect> SelfEffects = new();
    public bool FiendlyFire = false;

    private Weapon _weapon;
    private CharacterHoldingObjects _owner;
    protected List<Collider2D> _currentHittingColliders = new();

    public event EventHandler<GameObject> OnHitSomeOne;
    public event EventHandler OnDestroyed;

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        layer.UpdateLayerForGameObject(gameObject);
        transform.parent = layer.transform;
    }

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

    public List<AbstractProjectile> SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null)
    {
        List<AbstractProjectile> result = OnSpawnProjectile(direction, accuracityMultiplier, weapon);
        ApplySelfEffectOnWeaponUser(result, weapon);
        return result;
    }

    protected abstract List<AbstractProjectile> OnSpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null);

    private void ApplySelfEffectOnWeaponUser(List<AbstractProjectile> projectiles, Weapon weapon)
    {
        if (
            weapon != null && 
            weapon.TryGetComponent(out Holdable holdableWeapon) &&
            holdableWeapon != null &&
            holdableWeapon.CurrentHolder != null &&
            holdableWeapon.CurrentHolder.TryGetComponent(out CharacterComponentsManager holderCharComponents)
            )
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                holderCharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffects, projectiles[i]);
            }
        }
    }

    public virtual void OnHit(GameObject hitObject)
    {
        if (hitObject.transform.parent.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            charComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(HitEffects, this, hitObject.transform.parent.GetComponent<CharacterPart>());
            OnHitSomeOne?.Invoke(this, hitObject);
        }
    }

    public abstract void OnDeflected(MeleeProjectile deflector);

    public void RemoveSelf()
    {
        if (Weapon != null)
        {
            Weapon.Projectiles.Remove(this);
        }
        Destroy(gameObject);
    }

    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
    }

    protected virtual bool HitCondition(Collider2D[] totalHitObjects, Collider2D currentHitObjet)
    {
        return
            (
                !currentHitObjet.TryGetComponent(out AbstractCharacterComponent charComponent) ||
                (
                    charComponent.CharComponents.CharacterHolding != Owner &&
                    (FiendlyFire || !charComponent.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(Owner.CharComponents.CharacterTeam))
                )
            ) &&
            (
                !currentHitObjet.TryGetComponent(out CharacterHitbox charHitbox) ||
                (
                    charHitbox.HitableByProjectiles &&
                    GetIsHighestHitPriority(totalHitObjects, charHitbox) &&
                    currentHitObjet.transform.parent.GetComponent<CharacterPart>() != null
                )
            ) &&
            !_currentHittingColliders.Contains(currentHitObjet);
    }

    private bool GetIsHighestHitPriority(Collider2D[] colliders, CharacterHitbox currentHitBox)
    {
        int currentHighestPriority = currentHitBox.HitPriority;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (
                colliders[i].TryGetComponent(out CharacterHitbox charHitbox) &&
                AbstractCharacterComponent.GetCharacterComponentsEqual(currentHitBox, charHitbox)
                )
            {
                currentHighestPriority = Mathf.Max(currentHighestPriority, charHitbox.HitPriority);
            }
        }

        return currentHighestPriority <= currentHitBox.HitPriority;
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this, EventArgs.Empty);
    }
}
