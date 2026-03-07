
using UnityEngine;

public class SetClumsyReloading : AbstractCharacterEffect, IEntireCharacterEffect
{
    public bool Value;

    private bool _oldClumsyReloading;

    protected override void OnApply()
    {
        base.OnApply();

        _oldClumsyReloading = AffectedCharacter.CharacterClumsyness.ClumsyReloading;

        AffectedCharacter.CharacterClumsyness.ClumsyReloading = Value;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterClumsyness.ClumsyReloading = _oldClumsyReloading;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<SetClumsyReloading>();
    }
}