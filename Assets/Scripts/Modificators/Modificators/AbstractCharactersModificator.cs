using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractCharactersModificator : AbstractModificator
{
    public TeamManager.Teams Team = TeamManager.Teams.PLAYER;
    public float AffectChance = 1f;
    public RandomManager.ProcChanceTypes ChanceType;

    private List<CharacterComponentsManager> _affectedCharacters = new();

    protected override void OnObjectSpawned(object sender, GameObject e)
    {
        base.OnObjectSpawned(sender, e);

        if (
            e.TryGetComponent(out AbstractCharacterComponent character) && 
            character.CharComponents.CharacterTeam.Team == Team && 
            RandomManager.Instance.ProcRandomChance(AffectChance, ChanceType)
            )
        {
            _affectedCharacters.Add(character.CharComponents);
            OnCharacterAffected(character.CharComponents);
        }
    }

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (LayerManager.Instance != null)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                foreach (Transform characterT in layer.CharactersContainer)
                {
                    if (
                        characterT.TryGetComponent(out AbstractCharacterComponent character) &&
                        character.CharComponents.CharacterTeam.Team == Team &&
                        RandomManager.Instance.ProcRandomChance(AffectChance, ChanceType)
                        )
                    {
                        _affectedCharacters.Add(character.CharComponents);
                        OnCharacterAffected(character.CharComponents);
                    }
                }
            }
        }

        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.OnTeamChanged += Instance_OnTeamChanged;
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        foreach (CharacterComponentsManager character in _affectedCharacters)
        {
            if (character != null && !character.IsDestroyed())
            {
                OnCharacterRemovedAffect(character);
            }
        }

        _affectedCharacters = new();

        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.OnTeamChanged -= Instance_OnTeamChanged;
        }
    }

    private void Instance_OnTeamChanged(object sender, TeamManager.OnTeamChangedEventArgs e)
    {
        if (e.OldTeam == Team)
        {
            OnCharacterRemovedAffect(e.Character.CharComponents);
        }
        if (e.NewTeam == Team)
        {
            OnCharacterAffected(e.Character.CharComponents);
        }
    }

    protected abstract void OnCharacterAffected(CharacterComponentsManager character);

    protected abstract void OnCharacterRemovedAffect(CharacterComponentsManager character);
}