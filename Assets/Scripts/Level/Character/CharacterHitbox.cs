using System;
using UnityEngine;
using static CharacterVisual;

public class CharacterHitbox : AbstractCharacterComponent
{
    [Serializable]
    public class HitBoxTransform
    {
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = new();
        public Vector3 Scale = Vector3.one;
    }

    public enum CharacterHitboxTypes
    {
        BODY,
        HEAD
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        SetColliderTransform(DefaultColliderTransform);
        CharComponents.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;
    }

    public bool HitableByProjectiles = true;
    /// <summary>
    /// If projectile hits two multiple parts of a single character same time, hit detection will be triggered on hitbox with the highest HitPriority
    /// </summary>
    public int HitPriority = 1;

    public HitBoxTransform DefaultColliderTransform = new();
    public HitBoxTransform RollColliderTransform = new();
    public HitBoxTransform FallenColliderTransform = new();

    public virtual void OnHit(AbstractProjectile projectile)
    {
        CharComponents.CharacterDamaging.TryApplyHit(this, projectile);
    }

    private void SetColliderTransform(HitBoxTransform value)
    {
        transform.localPosition = value.Position;
        transform.localRotation = value.Rotation;
        transform.localScale = value.Scale;
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, OnBusyStateChangedEventArgs e)
    {
        switch (e.NewState)
        {
            case CharacterPart.CharacterPartBusyStates.ROLL:
                SetColliderTransform(RollColliderTransform);
                break;
            case CharacterPart.CharacterPartBusyStates.FALLEN_ON_FLOOR:
                SetColliderTransform(FallenColliderTransform);
                break;
            default:
                SetColliderTransform(DefaultColliderTransform);
                break;
        }
    }
}
