using System.Collections.Generic;
using UnityEngine;

public class CharacterPartsManager : AbstractCharacterComponent
{
    private List<CharacterPart> _characterParts = new();
    private List<CharacterEquipmentPart> _awaitingAddEquipments = new();

    public List<CharacterPart> CharacterParts
    {
        get => _characterParts;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        UpdateCharacterParts();
    }

    private void OnEnable()
    {
        foreach (CharacterEquipmentPart awaitedEquipment in _awaitingAddEquipments)
        {
            ForceAddPart(awaitedEquipment);
        }
        _awaitingAddEquipments.Clear();
    }

    private void UpdateCharacterParts()
    {
        for (int i = 0; i < _characterParts.Count; i++)
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
        characterPart.transform.SetParent(transform);
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
        if (gameObject.activeInHierarchy)
        {
            ForceAddPart(equipment);
        }
        else
        {
            _awaitingAddEquipments.Add(equipment);
        }
    }

    public void RemoveEquipment(CharacterEquipmentPart equipment)
    {
        for (int i = 0; i < _characterParts.Count; i++)
        {
            if (_characterParts[i] is CharacterEquipmentPart equipmentPart && equipmentPart == equipment)
            {
                _characterParts.Remove(equipmentPart);
                equipmentPart.DestroyPart();
                i--;
            }
        }
    }

    private CharacterEquipmentPart ForceAddPart(CharacterEquipmentPart equipment)
    {
        if (equipment == null) return null;

        foreach (CharacterEquipmentPart limbEquipment in GetCharacterPartEquipment(GetCharacterPart(equipment.EquipAtType)))
        {
            limbEquipment.DestroyPart();
        }

        CharacterEquipmentPart newEquipment = Instantiate(equipment, CharComponents.CharacterPartsContainer.transform);
        CharComponents.CharacterCollision.CurrentZLayer.UpdateLayerForAllChildren(newEquipment.transform);
        AddCharacterPart(newEquipment);
        LayerManager.Instance.InvokeOnObjectSpawned(newEquipment.gameObject);

        return newEquipment;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _characterParts.Count; i++)
        {
            _characterParts[i].OnRemoved -= CharacterPart_OnRemoved;
        }
    }
}
