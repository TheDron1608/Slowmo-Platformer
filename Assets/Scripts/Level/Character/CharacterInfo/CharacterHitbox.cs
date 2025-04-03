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
        if (!TryGetComponent(out _colliderComponent)) throw new UnityException("Collider2D component not found");
    }

    /// <summary>
    /// If projectile hits two multiple parts of a single character same time, hit detection will be triggered on hitbox with the highest HitPriority
    /// </summary>
    public int HitPriority = 1;

    private Collider2D _colliderComponent;
    private bool _hitableByProjectiles = true;

    public bool HitableByProjectiles
    {
        get => _hitableByProjectiles;
        set
        {
            _hitableByProjectiles = value;

            if (value)
            {
                _colliderComponent.excludeLayers = 0;
            }
            else
            {
                for (int i = 0; i < LayerManager.Instance.ZLayers.Count; i++)
                {
                    _colliderComponent.excludeLayers += 1 << LayerManager.Instance.ZLayers[i].HoldablesLayer;
                }
            }
        }
    }

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

    public Collider2D GetCollider()
    {
        if (TryGetComponent(out Collider2D collider))
        {
            return collider;
        }
        else
        {
            throw new UnityException("Collider2D component not found in " + gameObject.name);
        }
    }
}
