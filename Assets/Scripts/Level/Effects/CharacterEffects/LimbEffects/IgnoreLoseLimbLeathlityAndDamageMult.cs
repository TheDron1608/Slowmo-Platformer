using UnityEngine;

public class IgnoreLoseLimbLeathlityAndDamageMult : AbstractCharacterLimbEffect
{
    private float _oldDamageMultiplier;
    private bool _oldLosingLimbIsLeathal;

    protected override void OnApply()
    {
        base.OnApply();

        if (AffectedPart.TryGetComponent(out CharacterLimbPart limbPart))
        {
            AffectedObject.RemoveEffect(this);

            _oldDamageMultiplier = limbPart.CharPartHealth.DamageMultiplier;
            _oldLosingLimbIsLeathal = limbPart.CharPartHealth.LosingLimbIsLethal;

            limbPart.CharPartHealth.DamageMultiplier = 1f;
            limbPart.CharPartHealth.LosingLimbIsLethal = false;
        }
        else
        {
            RemoveSelf();
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (AffectedPart.TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.CharPartHealth.DamageMultiplier = _oldDamageMultiplier;
            limbPart.CharPartHealth.LosingLimbIsLethal = _oldLosingLimbIsLeathal;
        }
    }
}
