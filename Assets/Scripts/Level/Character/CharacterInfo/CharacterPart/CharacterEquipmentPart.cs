using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterEquipmentPart : CharacterPart
{
    public PartTypes EquptAtType;

    List<AbstractCharacterEffect> EffectsOnEquip;

    protected override void OnAwake()
    {
        base.OnAwake();

        CharComponents.CharacterEffects.ApplyEffect(EffectsOnEquip, this, CharComponents.CharacterPartsManager.GetCharacterPart(EquptAtType));
    }
}
