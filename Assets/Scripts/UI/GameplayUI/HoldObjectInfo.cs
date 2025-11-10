using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HoldObjectInfo : MonoBehaviour
{
    public CharacterHoldingObjects TrackedHolder = null;

    private Holdable _currentHoldObject = null;
    private string _unarmedText = "UNARMED";

    [Header("const references")]
    [SerializeField] private GameObject _holdObjectImageContainer;
    [SerializeField] private Image _holdObjectImage;
    [SerializeField] private TextMeshProUGUI _holdObjectName;
    [SerializeField] private HoldObjectAmmoList _loadedBulletsList;
    [SerializeField] private HoldObjectAmmoList _magsList;

    private void FixedUpdate()
    {
        if (TrackedHolder == null) return;

        if (_currentHoldObject != TrackedHolder.CurrentHoldObject)
        {
            _currentHoldObject = TrackedHolder.CurrentHoldObject;
            _loadedBulletsList.RemoveAllAmmo();
            _magsList.RemoveAllAmmo();
        }

        if (_currentHoldObject != null)
        {
            _holdObjectImageContainer.gameObject.SetActive(true);
            SpriteRenderer holdObjectSpriteRenderer = _currentHoldObject.GetComponent<SpriteRenderer>();
            _holdObjectImage.sprite = holdObjectSpriteRenderer.sprite;
            _holdObjectImage.SetNativeSize();

            _holdObjectName.text = _currentHoldObject.GetLocalizedName();

            if (_currentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon))
            {
                _loadedBulletsList.AmmoSprite = rangedWeapon.Projectile.GameplayUISprite;
                _loadedBulletsList.AmmoAmount = rangedWeapon.LoadedLivingAmmoLeft;
            }
            else
            {
                _loadedBulletsList.RemoveAllAmmo();
            }

            if (_currentHoldObject.TryGetComponent(out MagReloadingWeapon magReloadingWeapon))
            {
                _magsList.AmmoSprite = magReloadingWeapon.GameplayUIMagSprite;
                _magsList.AmmoAmount = magReloadingWeapon.Mags;
            }
            else if (_currentHoldObject.TryGetComponent(out BulletReloadingWeapon bulletReloadingWeapon))
            {
                _magsList.AmmoSprite = bulletReloadingWeapon.Projectile.GameplayUISprite;
                _magsList.AmmoAmount = bulletReloadingWeapon.AmmoLeft;
            }
            else
            {
                _magsList.RemoveAllAmmo();
            }
        }
        else
        {
            _holdObjectImageContainer.gameObject.SetActive(false);
            _holdObjectName.text = _unarmedText;
        }
    }
}