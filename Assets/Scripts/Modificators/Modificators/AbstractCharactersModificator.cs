using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractCharactersModificator : AbstractMultiplierableModificator
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
    }

    protected abstract void OnCharacterAffected(CharacterComponentsManager character);

    protected abstract void OnCharacterRemovedAffect(CharacterComponentsManager character);
}