using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(2)]
public class CharacterEquipmentPart : CharacterPart
{
    public PartTypes EquipAtType;
    public List<AbstractCharacterEffect> EffectsOnEquip;
    public bool CanUnequip = true;

    private CharacterLimbPart _currentLimbEquip;

    protected override void OnAwake()
    {
        base.OnAwake();

        if (CharComponents.CharacterPartsManager.GetCharacterPart(EquipAtType) is CharacterLimbPart limbPart)
        {
            _currentLimbEquip = limbPart;
        }
        else
        {
            throw new UnityException("trying to set CurrentLimbEquip, wich must be CharacterLimbPart class, " + CharComponents.CharacterPartsManager.GetCharacterPart(EquipAtType).GetType().Name + " received instead");
        }

        CharComponents.CharacterEffects.ApplyEffect(EffectsOnEquip, this, _currentLimbEquip);
    }

    public void TryUnequipPart()
    {
        if (CanUnequip && TryGetComponent(out ParticleSpawner equipmentParticleSpawner))
        {
            if (CharComponents.CharacterRigidBody.linearVelocityX < 0f)
            {
                equipmentParticleSpawner.SpawnAngle = equipmentParticleSpawner.SpawnAngle + (90f - equipmentParticleSpawner.SpawnAngle) * 2;
                equipmentParticleSpawner.SpawnAngularVeclocity *= -1f;
            }
            equipmentParticleSpawner.SpawnParticle();
        }

        DestroyPart();
    }

    protected override void OnDestroyPart()
    {
        CharComponents.CharacterEffects.RemoveEffect<AbstractCharacterEffect>(_currentLimbEquip);
        base.OnDestroyPart();
    }
}
