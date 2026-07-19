using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WeaponBelt : MonoBehaviour
{
    public List<Sprite> BeltSprites = new();

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _weaponSpriteRenderer;

    private void Update()
    {
        float rotation = transform.parent.rotation.eulerAngles.z;
        if (rotation > 180f) rotation -= 360f;

        _spriteRenderer.sprite = BeltSprites[(int)math.floor(BeltSprites.Count * NumberMath.LimitFloatInRange(rotation / 180f + 0.5f, 0f, 0.999f))];
        _spriteRenderer.sharedMaterial = _weaponSpriteRenderer.sharedMaterial;
    }
}