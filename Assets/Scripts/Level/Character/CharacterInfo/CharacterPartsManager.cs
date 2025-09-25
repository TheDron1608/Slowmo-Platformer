using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterPartsManager : AbstractCharacterComponent
{
    private List<CharacterPart> _characterParts = new();

    public List<CharacterPart> CharacterParts
    {
        get => _characterParts;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        UpdateCharacterParts();
    }

    private void UpdateCharacterParts()
    {
        for (int i = 0;  i < _characterParts.Count; i++)
        {
            _characterParts[i].OnRemoved -= CharacterPart_OnRemoved;
        }
        _characterParts.Clear();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out CharacterPart characterPart))
            {
                AddCharacterPart(characterPart);
            }
        }
    }

    private void AddCharacterPart(CharacterPart characterPart)
    {
        _characterParts.Add(characterPart);
        characterPart.OnRemoved += CharacterPart_OnRemoved;
    }

    private void CharacterPart_OnRemoved(object sender, CharacterPart e)
    {
        _characterParts.Remove(e);
    }

    public void SetHitBoxTransform(CharacterHitbox.AvaibleHitBoxTransforms transform, float smoothChangeDuration)
    {
        for (int i = 0; i < _characterParts.Count; i++)
        {
            if (_characterParts[i] is CharacterLimbPart limbPart)
            {
                limbPart.CharPartHitbox.SetHitBoxTransform(transform, smoothChangeDuration);
            }
        }
        CharComponents.CharacterRigidBodyCapsuleColliderHitBox.SetHitBoxTransform(transform, smoothChangeDuration);
    }

    public void SetHitBoxHitableByProjectiles(bool value)
    {
        for (int i = 0; i < _characterParts.Count; i++)
        {
            if (_characterParts[i] is CharacterLimbPart limbPart)
            {
                limbPart.CharPartHitbox.HitableByProjectiles = value;
            }
        }
        CharComponents.CharacterRigidBodyCapsuleColliderHitBox.HitableByProjectiles = value;
    }

    public CharacterPart GetCharacterPart(CharacterPart.PartTypes type)
    {
        for (int i = 0; i < _characterParts.Count; i++)
        {
            if (_characterParts[i].PartType == type)
            {
                return _characterParts[i];
            }
        }
        return null;
    }

    public List<CharacterEquipmentPart> GetCharacterPartEquipment(CharacterPart part)
    {
        List<CharacterEquipmentPart> result = new();
        for (int i = 0; i < _characterParts.Count; i++)
        {
            if (_characterParts[i] is CharacterEquipmentPart equpmentPart && equpmentPart.EquipAtType == part.PartType)
            {
                result.Add(equpmentPart);
            }
        }
        return result;
    }

    public void GiveNewEquipment(CharacterEquipmentPart equipment)
    {
        if (equipment == null) return;

        foreach (CharacterEquipmentPart limbEquipment in GetCharacterPartEquipment(GetCharacterPart(equipment.EquipAtType)))
        {
            limbEquipment.DestroyPart();
        }

        CharacterPart newEquipment = Instantiate(
            equipment,
            transform
            );
        LayerManager.Instance.GetZLayerOfGameObject(newEquipment.gameObject).UpdateLayerForGameObject(newEquipment.gameObject);
        AddCharacterPart(newEquipment);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _characterParts.Count; i++)
        {
            _characterParts[i].OnRemoved -= CharacterPart_OnRemoved;
        }
    }
}
