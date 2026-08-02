using UnityEngine;

public class ShakeCamera : AbstractCharacterEffect, IEntireCharacterEffect, IMultiplierableEffect
{
    public float Intencity;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        Camera.main.GetComponent<ShakableObject>().Shake(Intencity * EffectMultiplier);

        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return 
            TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers().Count == 1 &&
            base.ApplyCondition(affectWho, sender) && 
            affectWho.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterTeam.Team == TeamManager.Teams.PLAYER;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && Intencity == (other as ShakeCamera).Intencity;
    }
}
