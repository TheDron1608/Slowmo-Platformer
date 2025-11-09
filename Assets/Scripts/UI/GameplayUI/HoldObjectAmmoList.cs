using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UI;

public class HoldObjectAmmoList : MonoBehaviour
{
    const float LIST_HEIGHT = 50f;


    private int _ammoAmount = 0;
    private float _ammoSpriteWidth = 50f;
    private Sprite _ammoSprite = null;

    [Header("const references")]
    [SerializeField] private RectTransform _spawnPosition;
    [SerializeField] private RectTransform _trackTargetsContainer;
    [SerializeField] private GameObject _overGameobject;
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
            _ammoAmount = value;
            if (_ammoAmount > _spawnPosition.childCount)
            {
                AddAmmo(_ammoAmount - _spawnPosition.childCount);
            }
            else if (_ammoAmount < _spawnPosition.childCount)
            {
                RemoveLastAmmo(_spawnPosition.childCount - _ammoAmount);
            }
            UpdateAmmoOverflow();
        }
    }

    public float AmmoSpriteWidth
    {
        get => _ammoSpriteWidth;
        set
        {
            _ammoSpriteWidth = value;
            foreach (RectTransform rect in _trackTargetsContainer.GetComponentInChildren<RectTransform>())
            {
                rect.sizeDelta = new Vector2(_ammoSpriteWidth, LIST_HEIGHT);
            }
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
        }
    }

    private void AddAmmo(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject newGO = new GameObject("BulletImage");
            newGO.transform.parent = _spawnPosition;
            newGO.transform.localPosition = Vector3.zero;

            RectTransform newGORectTransform = newGO.AddComponent<RectTransform>();
            newGORectTransform.sizeDelta = new Vector2(AmmoSpriteWidth, LIST_HEIGHT);

            Image newGOImage = newGO.AddComponent<Image>();
            newGOImage.sprite = _ammoSprite;
            newGOImage.SetNativeSize();

            UIElementTrackTarget.CreateTrackTarget(_trackTargetsContainer, newGO.transform);
        }

        _overGameobject.transform.SetAsLastSibling();
    }

    private void UpdateAmmoOverflow()
    {
        if (AmmoAmount > GetListCapacity())
        {
            _overflowText.text = "+" + (AmmoAmount - GetOverflownListCapacity());
            _overGameobject.SetActive(true);
        }
        else
        {
            _overGameobject.SetActive(false);
        }
    }

    private int GetListCapacity()
    {
        return (int)math.floor(math.abs(_trackTargetsContainer.rect.width) / AmmoSpriteWidth);
    }

    private int GetOverflownListCapacity()
    {
        return (int)math.floor((math.abs(_trackTargetsContainer.rect.width) - math.abs(_overflowText.rectTransform.rect.width)) / AmmoSpriteWidth);
    }

    private void RemoveLastAmmo(int amount = 1)
    {
        for (int i = 0; i < math.min(amount, _spawnPosition.childCount); i++)
        {
            Destroy(_spawnPosition.GetChild(i).gameObject);
        }
    }

    public void RemoveAllAmmo()
    {
        AmmoAmount = 0;
    }
}