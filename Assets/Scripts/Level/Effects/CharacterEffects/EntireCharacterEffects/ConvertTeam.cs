using Unity.VisualScripting;
using UnityEngine;

public class ConvertTeam : AbstractCharacterEffect, IEntireCharacterEffect
{
    public TeamManager.Teams OverrideTeam;
    public CharacterAIManager OverrideAI;
    /// <summary>
    /// Works incorrectly, fix if will be used true value
    /// </summary>
    public CharacterUITrack AddTrack = null;

    private TeamManager.Teams _oldTeam;
    private CharacterAIManager _oldAI = null;

    protected override void OnApply()
    {
        base.OnApply();

        if (TryGetComponent(out DisableObjectOnDistanceFromCamera disableObject))
        {
            disableObject.AllowDisable = OverrideTeam != TeamManager.Teams.PLAYER;
        }
        
        if (OverrideAI != null)
        {
            if (AffectedCharacter.CharacterAIManager != null)
            {
                _oldAI = AffectedCharacter.CharacterAIManager;
                AffectedCharacter.CharacterAIManager.SetAIDisabled(true);
            }
            AffectedCharacter.CharacterAIManager = Instantiate(OverrideAI, AffectedCharacter.transform);
        }

        _oldTeam = AffectedCharacter.CharacterTeam.Team;
        AffectedCharacter.CharacterTeam.Team = OverrideTeam;

        if (AddTrack != null)
        {
            if (AffectedCharacter.UITrack != null)
            {
                Destroy(AffectedCharacter.UITrack.gameObject);
            }
            AffectedCharacter.UITrack = Instantiate(AddTrack, AffectedCharacter.transform);
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        
        if (_oldAI != null)
        {
            AffectedCharacter.CharacterAIManager.RemoveAI();
            if (AffectedCharacter.CharacterAIManager != null)
            {
                _oldAI.SetAIDisabled(false);
            }
        }

        if (AddTrack != null)
        {
            if (AffectedCharacter.UITrack != null)
            {
                Destroy(AffectedCharacter.UITrack.gameObject);
            }
        }

        AffectedCharacter.CharacterTeam.Team = _oldTeam;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterTeam.Team != OverrideTeam &&
            !affectWho.GetHasEffect<ConvertTeam>();
    }
}
