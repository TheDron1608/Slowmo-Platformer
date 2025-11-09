using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HoldObjectInfo : MonoBehaviour
{
    const float BULLET_ELEMENT_WIDTH = 18.75f;
    const float MAG_ELEMENT_WIDTH = 30.75f;

    public CharacterHoldingObjects TrackedHolder = null;

    private Holdable _currentHoldObject = null;
    private string _unarmedText = "UNARMED";

    [Header("const references")]
    [SerializeField] private GameObject _holdObjectImageContainer;
    [SerializeField] private Image _holdObjectImage;
    [SerializeField] private TextMeshProUGUI _holdObjectName;
    [SerializeField] private HoldObjectAmmoList _loadedBulletsList;
    [SerializeField] private HoldObjectAmmoList _magsList;

    private void Start()
    {
        UpdateHoldObject(_currentHoldObject);
        _loadedBulletsList.RemoveAllAmmo();
        _magsList.RemoveAllAmmo();
    }

    private void FixedUpdate()
    {
        if (TrackedHolder == null) return;

        if (_currentHoldObject != TrackedHolder.CurrentHoldObject)
        {
            _currentHoldObject = TrackedHolder.CurrentHoldObject;
            OnHoldObjectChanged(_currentHoldObject);
        }

        if (_currentHoldObject != null)
        {
            UpdateHoldObject(_currentHoldObject);

            if (_currentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon))
            {
                _loadedBulletsList.AmmoAmount = rangedWeapon.LoadedLivingAmmoLeft;
            }
            else
            {
                _loadedBulletsList.RemoveAllAmmo();
            }

            if (_currentHoldObject.TryGetComponent(out MagReloadingWeapon magReloadingWeapon))
            {
                _magsList.AmmoAmount = magReloadingWeapon.Mags;
            }
            else if (_currentHoldObject.TryGetComponent(out BulletReloadingWeapon bulletReloadingWeapon))
            {
                _magsList.AmmoAmount = bulletReloadingWeapon.AmmoLeft;
            }
            else
            {
                _magsList.RemoveAllAmmo();
            }
        }
    }

    private void UpdateHoldObject(Holdable holdObject)
    {
        if (holdObject != null)
        {
            _holdObjectImageContainer.gameObject.SetActive(true);
            SpriteRenderer holdObjectSpriteRenderer = holdObject.GetComponent<SpriteRenderer>();
            _holdObjectImage.sprite = holdObjectSpriteRenderer.sprite;
            _holdObjectImage.SetNativeSize();

            _holdObjectName.text = holdObject.gameObject.name;
        }
        else
        {
            _holdObjectImageContainer.gameObject.SetActive(false);
            _holdObjectName.text = _unarmedText;
        }
    }

    private void OnHoldObjectChanged(Holdable holdObject)
    {
        UpdateHoldObject(holdObject);
        _loadedBulletsList.RemoveAllAmmo();
        _magsList.RemoveAllAmmo();

        if (holdObject != null)
        {
            if (holdObject.TryGetComponent(out RangedWeapon rangedWeapon))
            {
                _loadedBulletsList.AmmoSprite = rangedWeapon.Projectile.GameplayUISprite;
                _loadedBulletsList.AmmoSpriteWidth = BULLET_ELEMENT_WIDTH;
            }
            else
            {
                _loadedBulletsList.RemoveAllAmmo();
            }

            if (holdObject.TryGetComponent(out MagReloadingWeapon magReloadingWeapon))
            {
                _magsList.AmmoSprite = magReloadingWeapon.GetMagparticleRenderer().sprite;
                _magsList.AmmoSpriteWidth = MAG_ELEMENT_WIDTH;
            }
            else if (holdObject.TryGetComponent(out BulletReloadingWeapon bulletReloadingWeapon))
            {
                _magsList.AmmoSprite = bulletReloadingWeapon.Projectile.GameplayUISprite;
                _magsList.AmmoSpriteWidth = BULLET_ELEMENT_WIDTH;
            }
            else
            {
                _magsList.RemoveAllAmmo();
            }
        }
    }
}