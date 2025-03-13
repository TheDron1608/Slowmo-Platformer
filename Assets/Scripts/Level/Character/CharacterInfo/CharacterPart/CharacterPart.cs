using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterPart : AbstractCharacterComponent
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

    public event EventHandler<CharacterPart> OnRemoved;

    public PartTypes PartType;
    public CharacterPartHealth CharPartHealth;
    public CharacterPartVisual CharPartVisual;
    public CharacterHitbox CharPartHitbox;

    private void OnDestroy()
    {
        OnRemoved?.Invoke(this, this);
    }
}
