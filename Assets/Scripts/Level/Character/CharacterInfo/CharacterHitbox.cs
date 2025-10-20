using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CharacterHitbox : AbstractCharacterComponent
{
    const float FLIP_H_CHANGE_DURATION = 0.1f;

    private Coroutine _changeHitboxSmoothlyCoroutine = null;

    public bool GetIsChangingHitBox()
    {
        return _changeHitboxSmoothlyCoroutine != null;
    }

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
        SetHitBoxTransform(AvaibleHitBoxTransforms.DEFAULT, 0.1f);
        if (!TryGetComponent(out _colliderComponent)) throw new UnityException("Collider2D component not found");
        CharComponents.CharacterVisual.OnSpriteFlippedChanged += CharacterVisual_OnSpriteFlippedChanged;
    }

    private void CharacterVisual_OnSpriteFlippedChanged(object sender, bool e)
    {
        UpdateFlipHHitBoxTransform();
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

    public void UpdateFlipHHitBoxTransform()
    {
        SetHitBoxTransform(_currentHitBoxTransform, FLIP_H_CHANGE_DURATION);
    }

    public void SetHitBoxTransform(AvaibleHitBoxTransforms value, float smoothChangeDuration)
    {
        if (_currentHitBoxTransform == value && (CharComponents.CharacterVisual.FlippedH ^ transform.localScale.x < 0f)) return;

        SetColliderTransform(HitBoxTransforms[(int)value], smoothChangeDuration);

        _currentHitBoxTransform = value;
    }

    private void SetColliderTransform(HitBoxTransform value, float smoothChangeDuration)
    {
        if (_changeHitboxSmoothlyCoroutine != null) StopCoroutine(_changeHitboxSmoothlyCoroutine);
        _changeHitboxSmoothlyCoroutine = StartCoroutine(ChangeHitboxSmoothly(value, smoothChangeDuration));
    }

    private IEnumerator ChangeHitboxSmoothly(HitBoxTransform targetHitbox, float smoothChangeDuration)
    {
        Vector3 basePosition = transform.localPosition;
        Vector3 baseScale = transform.localScale;
        Quaternion baseRotation = transform.localRotation;
        float timeSpent = 0f;

        while (timeSpent < smoothChangeDuration)
        {
            if (timeSpent > smoothChangeDuration / 2f && TryGetComponent(out CapsuleCollider2D capsule))
            {
                capsule.direction = targetHitbox.FlipCapsuleDirection ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
            }

            transform.localPosition = math.lerp(basePosition, new Vector3(
                CharComponents.CharacterVisual.FlippedH ? -targetHitbox.Position.x : targetHitbox.Position.x,
                targetHitbox.Position.y,
                targetHitbox.Position.z
                ), timeSpent / smoothChangeDuration);
            transform.localScale = math.lerp(baseScale, new Vector3(
                CharComponents.CharacterVisual.FlippedH ? -targetHitbox.Scale.x : targetHitbox.Scale.x, 
                targetHitbox.Scale.y, 
                targetHitbox.Scale.z
                ), timeSpent / smoothChangeDuration);
            transform.localRotation = math.slerp(baseRotation, targetHitbox.Rotation, timeSpent / smoothChangeDuration);

            yield return new WaitForFixedUpdate();
            timeSpent += Time.deltaTime;
        }

        _changeHitboxSmoothlyCoroutine = null;
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

    private void OnDestroy()
    {
        CharComponents.CharacterVisual.OnSpriteFlippedChanged -= CharacterVisual_OnSpriteFlippedChanged;
    }
}
