using UnityEngine;

public class GiveComboIfPlayer : AbstractCharacterEffect, IEntireCharacterEffect
{
    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out AbstractCharacterComponent character) &&
            character.CharComponents.CharacterTeam.Team == TeamManager.Teams.PLAYER;
    }

    protected override void OnApply()
    {
        base.OnApply();

        ScoreManager.Instance.AddCombo();
        RemoveSelf();
    }
}
