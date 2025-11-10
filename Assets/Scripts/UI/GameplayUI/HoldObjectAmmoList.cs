using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UI;

public class HoldObjectAmmoList : MonoBehaviour
{
    private int _ammoAmount = 0;
    private Sprite _ammoSprite = null;

    [Header("const references")]
    [SerializeField] private RectTransform _spawnPosition;
    [SerializeField] private RectTransform _trackTargetsContainer;
    [SerializeField] private GameObject _overflowGameobject;
    [SerializeField] private TextMeshProUGUI _overflowText;

    private void Start()
    {
        RemoveAllAmmo();
    }

    public int AmmoAmount
    {
        get => _ammoAmount;
        set
        {
            if (value > _ammoAmount)
            {
                AddAmmo(math.abs(value - _ammoAmount));
            }
            else if (value < _ammoAmount)
            {
                RemoveLastAmmo(math.abs(value - _ammoAmount));
            }
            _ammoAmount = value;
            UpdateAmmoOverflow();
        }
    }

    public Sprite AmmoSprite
    {
        get => _ammoSprite;
        set
        {
            _ammoSprite = value;
            foreach (Image image in _spawnPosition.GetComponentsInChildren<Image>())
            {
                image.sprite = _ammoSprite;
                image.SetNativeSize();
            }
            UpdateAmmoOverflow();
        }
    }

    private void AddAmmo(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (_spawnPosition.childCount > GetListCapacity()) break;

            GameObject newGO = new GameObject("BulletImage");
            newGO.transform.parent = _spawnPosition;
            newGO.transform.position = _spawnPosition.transform.position;

            RectTransform newGORectTransform = newGO.AddComponent<RectTransform>();

            Image newGOImage = newGO.AddComponent<Image>();
            newGOImage.sprite = _ammoSprite;
            newGOImage.SetNativeSize();

            UIElementTrackTarget.CreateTrackTarget(_trackTargetsContainer, newGO.transform);
        }

        _overflowGameobject.transform.SetAsLastSibling();
    }

    private void UpdateAmmoOverflow()
    {
        if (AmmoSprite != null && AmmoAmount > GetListCapacity())
        {
            _overflowText.text = "+" + (AmmoAmount - GetOverflownListCapacity());
            _overflowGameobject.SetActive(true);
        }
        else
        {
            _overflowGameobject.SetActive(false);
        }
    }

    private int GetListCapacity()
    {
        return (int)math.floor(math.abs(_trackTargetsContainer.rect.width) / (math.abs(AmmoSprite.rect.width) * (100f / AmmoSprite.pixelsPerUnit)));
    }

    private int GetOverflownListCapacity()
    {
        return (int)math.floor((math.abs(_trackTargetsContainer.rect.width) - math.abs(_overflowText.rectTransform.rect.width)) / (math.abs(AmmoSprite.rect.width) * (100f / AmmoSprite.pixelsPerUnit)));
    }

    private void RemoveLastAmmo(int amount = 1)
    {
        int targetAmount = math.min(amount, _spawnPosition.childCount);
        for (int i = 0; i < targetAmount; i++)
        {
            if (_ammoAmount - amount + i > GetListCapacity())
            {
                Transform moveAmmo = _trackTargetsContainer.GetChild(0);
                moveAmmo.SetAsLastSibling();
                moveAmmo.transform.position = _spawnPosition.transform.position;
                _spawnPosition.GetChild(0).SetAsLastSibling();
            }
            else if (_spawnPosition.childCount > 0)
            {
                Destroy(_spawnPosition.GetChild(i).gameObject);
            }
        }
    }

    public void RemoveAllAmmo()
    {
        _ammoAmount = 0;
        foreach (Transform ammo in _spawnPosition.transform)
        {
            Destroy(ammo.gameObject);
        }
        UpdateAmmoOverflow();
    }
}