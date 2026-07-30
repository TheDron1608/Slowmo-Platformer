using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponBelt : MonoBehaviour
{
    const int SORTING_LAYER_ADD_ON_HOLSTERED = 200;

    public List<Sprite> BeltSprites = new();
    public Sprite BeltHolsteredSprite;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _weaponSpriteRenderer;
    [SerializeField] private Holdable _holdableComponent;

    private bool _sortingOrderAdded = false;

    private void FixedUpdate()
    {
        if (_holdableComponent?.IsHolstered ?? false)
        {
            transform.position = _holdableComponent.CurrentHolder.CharComponents.Center.transform.position;
            transform.rotation = _holdableComponent.CurrentHolder.transform.rotation;
            _spriteRenderer.flipX = _holdableComponent.CurrentHolder.CharComponents.CharacterVisual.FlippedH;
            _spriteRenderer.sprite = BeltHolsteredSprite;

            if (!_sortingOrderAdded)
            {
                _sortingOrderAdded = true;
                _spriteRenderer.sortingOrder += SORTING_LAYER_ADD_ON_HOLSTERED;
            }
        }
        else
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            float rotation = transform.parent.rotation.eulerAngles.z;
            if (rotation > 180f) rotation -= 360f;

            _spriteRenderer.flipX = false;
            _spriteRenderer.sprite = BeltSprites[(int)math.floor(BeltSprites.Count * NumberMath.LimitFloatInRange(rotation / 180f + 0.5f, 0f, 0.999f))];

            if (_sortingOrderAdded)
            {
                _sortingOrderAdded = false;
                _spriteRenderer.sortingOrder -= SORTING_LAYER_ADD_ON_HOLSTERED;
            }
        }

        _spriteRenderer.sharedMaterial = _weaponSpriteRenderer.sharedMaterial;
    }
}