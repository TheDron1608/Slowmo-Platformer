using UnityEngine;

[DefaultExecutionOrder(7)]
public class CharacterTeam : AbstractCharacterComponent
{
    public TeamManager.Teams Team;

    protected override void OnAwake()
    {
        base.OnAwake();
        GetTeamData().AddTeamMember(this);
        CharComponents.CharacterEffectsReceiver.OnEffectAdded += CharacterEffectsReceiver_OnEffectAdded;
        CharComponents.CharacterEffectsReceiver.OnEffectRemoved += CharacterEffectsReceiver_OnEffectRemoved;
    }

    public bool GetIsAllyToAnotherTeam(CharacterTeam anotherTeam)
    {
        return Team == anotherTeam.Team;
    }

    public TeamManager.TeamData GetTeamData()
    {
        return TeamManager.Instance?.GetTeamDataByTeam(Team);
    }

    private void CharacterEffectsReceiver_OnEffectAdded(object sender, CharacterEffectsReceiver.EffectAddedEventArgs e)
    {
        if (e.Effect is ILethalEffect)
        {
            GetTeamData().SetTeamMemberKilled(this, TryGetTeamFromSender(e.Sender));
        }
    }

    private void CharacterEffectsReceiver_OnEffectRemoved(object sender, AbstractEffect e)
    {
        if (e is ILethalEffect)
        {
            GetTeamData().SetTeamMemberRessurected(this, null);
        }
    }

    private CharacterTeam TryGetTeamFromSender(MonoBehaviour sender)
    {
        if (sender == null)
        {
            return null;
        }
        if (sender.TryGetComponent(out AbstractCharacterComponent senderCharacter))
        {
            return senderCharacter.CharComponents.CharacterTeam;
        }
        else if (sender.TryGetComponent(out Holdable senderHoldable))
        {
            return senderHoldable.CurrentHolder?.CharComponents.CharacterTeam;
        }
        else if (sender.TryGetComponent(out AbstractProjectile senderProjectile))
        {
            if (senderProjectile?.Weapon?.TryGetComponent(out Holdable holdableWeapon) ?? false)
            {
                return holdableWeapon?.CurrentHolder?.CharComponents.CharacterTeam;
            }
            else if (senderProjectile?.Weapon?.TryGetComponent(out UnarmedWeapon unarmedWeapon) ?? false)
            {
                return unarmedWeapon?.CharComponents.CharacterTeam;
            }
        }
        return null;
    }

    public void OnDestroy()
    {
        GetTeamData()?.RemoveTeamMember(this);
        CharComponents.CharacterEffectsReceiver.OnEffectAdded -= CharacterEffectsReceiver_OnEffectAdded;
    }
}