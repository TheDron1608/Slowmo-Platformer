using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTeam : AbstractCharacterComponent
{
    const float UPDATE_NEAREST_AVAIBLE_ENEMY_DELAY_SECONDS = 0.34f;

    public enum Teams
    {
        PLAYER,
        DEFAULT_ENEMY
    }

    public List<Teams> CharacterTeams = new();

    public bool GetIsAllyToAnotherTeam(CharacterTeam anotherTeam)
    {
        return NumberMath.GetListContainsAnyItemOfAnotherList(CharacterTeams, anotherTeam.CharacterTeams);
    }
}