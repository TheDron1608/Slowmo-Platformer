using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class CharacterPart : AbstractCharacterComponent
{
    public enum PartTypes
    {
        BODY,
        HEAD,
        EYES,
        BODYWEAR,
        HEADWEAR,
        EYESWEAR,
        LEGWEAR
    }

    public PartTypes PartType;
    public CharacterPartVisual CharPartVisual;
    public CharacterPartEffectsReceiver CharPartEffectsReceiver;

    public event EventHandler<CharacterPart> OnRemoved;

    public List<CharacterEquipmentPart> GetEquipedAtParts()
    {
        return CharComponents.CharacterPartsManager.GetCharacterPartEquipment(this);
    }

    public void DestroyPart()
    {
        CharPartEffectsReceiver.CurrentEffects.Clear();
        OnDestroyPart();
    }

    protected virtual void OnDestroyPart()
    {
        GameObject.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        OnRemoved?.Invoke(this, this);
    }
}
