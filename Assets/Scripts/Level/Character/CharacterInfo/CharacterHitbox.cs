using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CharacterVisual;
using static UnityEngine.Rendering.DebugUI;

public class CharacterHitbox : AbstractCharacterComponent
{
    [Serializable]
    public class HitBoxTransform
    {
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = new();
        public Vector3 Scale = Vector3.one;
        public bool FlipCapsuleDirection = false;
    }

    public enum AvaibleHitBoxTransforms
    {
        DEFAULT = 0,
        FALLEN = 1,
        ROLL = 2
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        SetHitBoxTransform(AvaibleHitBoxTransforms.DEFAULT);
    }

    public bool HitableByProjectiles = true;
    /// <summary>
    /// If projectile hits two multiple parts of a single character same time, hit detection will be triggered on hitbox with the highest HitPriority
    /// </summary>
    public int HitPriority = 1;

    private AvaibleHitBoxTransforms _currentHitBoxTransform = AvaibleHitBoxTransforms.DEFAULT;

    public List<HitBoxTransform> HitBoxTransforms = new();

    public void SetHitBoxTransform(AvaibleHitBoxTransforms value)
    {
        if (_currentHitBoxTransform == value) return;

        SetColliderTransform(HitBoxTransforms[(int)value]);

        _currentHitBoxTransform = value;
    }

    private void SetColliderTransform(HitBoxTransform value)
    {
        transform.localPosition = value.Position;
        transform.localRotation = value.Rotation;
        transform.localScale = value.Scale;
        if (TryGetComponent(out CapsuleCollider2D capsule))
        {
            capsule.direction = value.FlipCapsuleDirection ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
        }

        if (HitBoxTransforms[(int)_currentHitBoxTransform] != value)
        {
            CharComponents.CharacterStuckedObjects.RemoveAllStuckedObjects();
        }
    }
}
