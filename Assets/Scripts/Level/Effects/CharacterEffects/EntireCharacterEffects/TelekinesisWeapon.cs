using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
public class TelekinesisWeapon : AbstractCharacterEffect, IEntireCharacterEffect
{
    const float EXTRA_CHARACTER_VELOCITY_WEAPON_POSITION_AFFECTION_MULT = 0.1f;
    const float TELE_HOLDABLE_COLLIDE_OFFSET = 0.25f;

    const float RANGED_ATTACK_MAX_DISTANCE = 10f;
    const float MELEE_ATTACK_MAX_DISTANCE = 2f;
    const float INVINCIBLE_TIME = 0.25f;

    private class TeleHoldableTrack
    {
        public Weapon Weapon;
        public Holdable Holdable;
        public Vector3 Velocity;
    }

    public int MaxTelekinesisWeapons = 1;
    public float AffectDistance = 3.5f;
    public float TeleThrowForce = 0.5f;
    public float TeleMoveSpeed = 7.5f;
    public float TeleDistance = 2f;
    public float MinLiveTime = 15f;
    public float MaxLiveTime = 3.5f;
    public float UnableToTeleWeaponAfterHitTime = 1f;
    public int WeaponsOnStart = 1;
    public AbstractEffect EffectOnNoHoldables;
    public List<AbstractEffect> EffectsOnHoldables = new();

    [SerializeField] private StaticSoundPlayer SoundOnLoseWeapons;

    private List<TeleHoldableTrack> _currentTeleWeapons = new();
    private float _currentLiveTime = 0f;
    private float _timeSinceLastHit = 9999f;
    private List<Holdable> _lastHitLostHoldables = new();

    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterHealth.OnHitByProjectile += CharacterHealth_OnHitByProjectile;

        if (SpawnManager.Instance.EnemyPoolInstance.Count > 0)
        {
            for (int i = 0; i < WeaponsOnStart; i++)
            {
                AffectedCharacter.CharacterCollision.CurrentZLayer.TrySpawnObject(
                    SpawnManager.Instance.EnemyPoolInstance[0].Weapon.PickRandomWeapon().gameObject,
                    AffectedCharacter.Center.transform.position,
                    null,
                    null
                    );
            }
        }
    }

    private void CharacterHealth_OnHitByProjectile(object sender, AbstractProjectile e)
    {
        if (_currentTeleWeapons.Count > 0)
        {
            _lastHitLostHoldables.Clear();
            while (_currentTeleWeapons.Count > 0)
            {
                _lastHitLostHoldables.Add(_currentTeleWeapons[0].Holdable);
                RemoveTeleHoldableTrack(0, false);
            }
            SoundOnLoseWeapons.PlaySound();
            _timeSinceLastHit = 0f;
            AffectedCharacter.CharacterHealth.SetHealth(AffectedCharacter.CharacterHealth.MaxHealth, null);
            AffectedCharacter.CharacterHealth.Ressurect();
        }
        else if (_timeSinceLastHit < INVINCIBLE_TIME)
        {
            AffectedCharacter.CharacterHealth.SetHealth(AffectedCharacter.CharacterHealth.MaxHealth, null);
            AffectedCharacter.CharacterHealth.Ressurect();
        }
    }

    private void FixedUpdate()
    {
        AffectedCharacter.CharacterHolding.IsAbleToHoldObjects = false;

        _timeSinceLastHit += Time.deltaTime;

        for (int i = 0; i < _currentTeleWeapons.Count; i++)
        {
            float currentMaxLiveTime = _currentTeleWeapons.Count > 1 ? math.lerp(MaxLiveTime, MinLiveTime, _currentTeleWeapons.Count / MaxTelekinesisWeapons) : float.MaxValue;
            if (
                _currentTeleWeapons[i].Holdable.IsDestroyed() ||
                _currentTeleWeapons[i].Holdable.CurrentHolder != AffectedCharacter.CharacterHolding ||
                !_currentTeleWeapons[i].Weapon.GetIsAbleToAttack() ||
                _currentLiveTime > currentMaxLiveTime
                )
            {
                if (_currentTeleWeapons[i].Holdable.IsDestroyed())
                {
                    RemoveTeleHoldableTrack(i);
                    i--;
                }
                else if (_currentLiveTime > currentMaxLiveTime)
                {
                    if (_currentTeleWeapons[i].Holdable.TryGetComponent(out BreakableHoldable bh)) bh.BreakObjectWithoutConvertToBroken(null);
                    RemoveTeleHoldableTrack(i);
                    _currentLiveTime = 0f;
                    i--;
                }
                else if (_currentTeleWeapons[i].Weapon is RangedWeapon rw && (!rw.Unloaded || rw.IsUnloading))
                {
                    rw.TryUnload();
                }
                else
                {
                    RemoveTeleHoldableTrack(i);
                    i--;
                }
            }
            else if (_currentTeleWeapons[i].Weapon is RangedWeapon rw && rw.GetIsNeedReload())
            {
                rw.TryReload();
            }
        }

        foreach (Transform holdableT in AffectedCharacter.CharacterCollision.CurrentZLayer.HoldablesContainer)
        {
            if (_currentTeleWeapons.Count >= MaxTelekinesisWeapons) break;

            if (
                !holdableT.IsDestroyed() &&
                Vector2.Distance(holdableT.position, AffectedCharacter.Center.transform.position) < AffectDistance &&
                holdableT.TryGetComponent(out Holdable holdable) &&
                (_timeSinceLastHit > UnableToTeleWeaponAfterHitTime || !_lastHitLostHoldables.Contains(holdable)) &&
                holdable.CurrentHolder == null &&
                holdable.TryGetComponent(out Weapon weapon) &&
                weapon.GetIsAbleToAttack() &&
                !(weapon is Chainsaw)
                )
            {
                AddTeleHoldable(holdable, weapon);
            }
        }

        Vector3 holdablesCenter = AffectedCharacter.Center.transform.position + VectorMath.Vec2ToVec3(AffectedCharacter.CharacterRigidBody.linearVelocity) * EXTRA_CHARACTER_VELOCITY_WEAPON_POSITION_AFFECTION_MULT;
        for (int i = 0; i < _currentTeleWeapons.Count; i++)
        {
            _currentLiveTime += Time.deltaTime;

            LayerManager.Instance.ChangeZIndexForGameObject(AffectedCharacter.CharacterCollision.CurrentZLayer, _currentTeleWeapons[i].Holdable.gameObject);

            {
                Vector2 holdableAngle = VectorMath.RotateVec2(AffectedCharacter.CharacterAiming.GetCurrentAimNormalized(), ((i - (_currentTeleWeapons.Count - 1f) / 2f) / MaxTelekinesisWeapons) * 2f);
                RaycastHit2D holdableHit = Physics2D.Raycast(holdablesCenter, holdableAngle, TeleDistance + TELE_HOLDABLE_COLLIDE_OFFSET, 1 << AffectedCharacter.CharacterCollision.CurrentZLayer.EnviromentLayer);

                _currentTeleWeapons[i].Holdable.transform.position = Vector3.SmoothDamp(
                    _currentTeleWeapons[i].Holdable.transform.position,
                    holdablesCenter + VectorMath.Vec2ToVec3(holdableAngle) * (holdableHit.collider != null ? holdableHit.distance - TELE_HOLDABLE_COLLIDE_OFFSET : TeleDistance),
                    ref _currentTeleWeapons[i].Velocity,
                    1f / TeleMoveSpeed
                    );
            }

            if (_currentTeleWeapons[i].Weapon.AttackCondition() && !_currentTeleWeapons[i].Weapon.IsInCooldown)
            {
                float nearestTargetDistance = float.MaxValue;
                AbstractCharacterComponent nearestTarget = null;
                foreach (Transform characterTransform in AffectedCharacter.CharacterCollision.CurrentZLayer.CharactersContainer)
                {
                    if (characterTransform.IsDestroyed() || !characterTransform.gameObject.activeSelf) continue;

                    float distance = Vector2.Distance(characterTransform.position, _currentTeleWeapons[i].Weapon.transform.position);
                    if (
                        nearestTargetDistance > distance &&
                        (
                            (_currentTeleWeapons[i].Weapon.Projectile is RangedProjectile && distance < RANGED_ATTACK_MAX_DISTANCE) ||
                            (_currentTeleWeapons[i].Weapon.Projectile is MeleeProjectile && distance < MELEE_ATTACK_MAX_DISTANCE)
                        ) &&
                        characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                        !AffectedCharacter.CharacterTeam.GetIsAllyToAnotherTeam(character.CharComponents.CharacterTeam) &&
                        !character.CharComponents.CharacterHealth.Died &&
                        Physics2D.Linecast(
                            _currentTeleWeapons[i].Weapon.transform.position,
                            character.CharComponents.Center.transform.position,
                            1 << AffectedCharacter.CharacterCollision.CurrentZLayer.EnviromentLayer
                            ).collider == null
                        )
                    {
                        nearestTarget = character;
                        nearestTargetDistance = distance;
                    }
                }

                if (nearestTarget != null)
                {
                    Vector2 attackDir = nearestTarget.CharComponents.Center.transform.position - _currentTeleWeapons[i].Weapon.transform.position;
                    _currentTeleWeapons[i].Weapon.transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(attackDir);
                    _currentTeleWeapons[i].Weapon.TryAttack(attackDir);
                }
                else
                {
                    _currentTeleWeapons[i].Weapon.transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(AffectedCharacter.CharacterAiming.GetCurrentAimNormalized());
                }
            }
        }
    }

    private void RemoveTeleHoldableTrack(int at, bool breakOrUnloadHoldable = true)
    {
        if (
            _currentTeleWeapons[at].Holdable != null && 
            !_currentTeleWeapons[at].Holdable.IsDestroyed()
            )
        {
            if (breakOrUnloadHoldable)
            {
                if (_currentTeleWeapons[at].Holdable.TryGetComponent(out ObjectEffectsReceiver effectsReceiver))
                {
                    effectsReceiver.RemoveEffect(EffectsOnHoldables);
                }
                if (_currentTeleWeapons[at].Weapon is MeleeWeapon && _currentTeleWeapons[at].Weapon.TryGetComponent(out BreakableHoldable bh))
                {
                    bh.BreakObjectWithoutConvertToBroken(null);
                }
            }
            _currentTeleWeapons[at].Holdable.Throw((_currentTeleWeapons[at].Holdable.transform.position - AffectedCharacter.Center.transform.position).normalized * TeleThrowForce);
        }

        _currentTeleWeapons.RemoveAt(at);

        if (_currentTeleWeapons.Count == 0)
        {
            AffectedCharacter.CharacterEffectsReceiver.ApplyEffect(EffectOnNoHoldables, null);
        }
    }

    private void AddTeleHoldable(Holdable holdable, Weapon weapon)
    {
        TeleHoldableTrack newTrack = new();
        newTrack.Holdable = holdable;
        newTrack.Weapon = weapon;
        newTrack.Velocity = Vector2.zero;

        holdable.Give(AffectedCharacter.CharacterHolding, false);
        if (weapon is MeleeWeapon && weapon.TryGetComponent(out BreakableHoldable bh))
        {
            bh.UnlimitedUses = false;
        }

        int? nearestHoldableIndex = null;
        float nearestHoldableDistance = float.MaxValue;
        for (int i = 0; i < _currentTeleWeapons.Count; i++)
        {
            float distance = Vector2.Distance(holdable.transform.position, _currentTeleWeapons[i].Holdable.transform.position);
            if (nearestHoldableDistance > distance)
            {
                nearestHoldableDistance = distance;
                nearestHoldableIndex = i;
            }
        }

        if (holdable.TryGetComponent(out ObjectEffectsReceiver effectsReceiver))
        {
            effectsReceiver.ApplyEffect(EffectsOnHoldables, AffectedCharacter);
        }

        _currentTeleWeapons.Insert(nearestHoldableIndex.GetValueOrDefault(0), newTrack);

        AffectedCharacter.CharacterEffectsReceiver.RemoveEffect(EffectOnNoHoldables);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterHolding.IsAbleToHoldObjects = true;

        while (_currentTeleWeapons.Count > 0)
        {
            RemoveTeleHoldableTrack(0, false);
        }

        AffectedCharacter.CharacterHealth.OnHitByProjectile -= CharacterHealth_OnHitByProjectile;
    }
}