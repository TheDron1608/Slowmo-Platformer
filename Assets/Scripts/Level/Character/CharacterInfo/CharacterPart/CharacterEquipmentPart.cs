using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(8)]
public class CharacterEquipmentPart : CharacterPart
{
    public PartTypes EquipAtType;
    public List<AbstractEffect> EffectsOnEquip;
    public bool CanUnequip = true;
    public SoundPlayer SoundOnBreak;

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

        CharPartEffectsReceiver.ApplyEffect(EffectsOnEquip, this);
    }

    public void BreakPart()
    {
        SoundOnBreak?.PlaySound(false, transform.position);
        TryUnequipPart();
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

    public CharacterLimbPart GetEquipedAtLimb()
    {
        return CharComponents.CharacterPartsManager.GetCharacterPart(EquipAtType)?.GetComponent<CharacterLimbPart>();
    }
}
