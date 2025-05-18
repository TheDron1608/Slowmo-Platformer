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
    public bool EffectMaterialOverridedByEntireEffects = false;

    public event EventHandler<CharacterPart> OnRemoved;

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
