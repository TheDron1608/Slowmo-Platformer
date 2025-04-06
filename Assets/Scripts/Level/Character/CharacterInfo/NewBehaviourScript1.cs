using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTeam : AbstractCharacterComponent
{
    public enum Teams
    {
        PLAYER,
        DEFAULT_ENEMY
    }

    public List<Teams> CharacterTeams = new();

    public bool GetIsInTeam(Teams team)
    {
        return CharacterTeams.Contains(team);
    }


}