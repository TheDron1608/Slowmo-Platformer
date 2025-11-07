using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class TeamHealthbars : MonoBehaviour
{
    public CharacterTeam.Teams TrackedTeam;

    [SerializeField] private Healthbar _spawnHealthbar;

    private List<Healthbar> _currentHealthbars = new();

    private void FixedUpdate()
    {
        List<CharacterTeam> characters = CharacterTeam.GetActiveCharactersFromAllLayers(TrackedTeam);

        for (int i = 0; i < _currentHealthbars.Count; i++)
        {
            if (
                !characters.Contains(_currentHealthbars[i].HealthTrackedCharacter.CharComponents.CharacterTeam) ||
                (_currentHealthbars.Count > 1 && _currentHealthbars[i].GetTrackedIsDead())
                )
            {
                Destroy(_currentHealthbars[i].gameObject);
                _currentHealthbars.RemoveAt(i);
                i--;
            }
        }

        foreach (CharacterTeam character in characters)
        {
            if (character != null && !character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() && !GetCurrentHealthbarsContainCharacter(character))
            {
                Healthbar newHealthBar = Instantiate(_spawnHealthbar, transform);
                newHealthBar.HealthTrackedCharacter = character.CharComponents.CharacterHealth;
                _currentHealthbars.Add(newHealthBar);
            }
        }
    }

    private bool GetCurrentHealthbarsContainCharacter(CharacterTeam character)
    {
        foreach (Healthbar healthbar in _currentHealthbars)
        {
            if (healthbar.HealthTrackedCharacter == character.CharComponents.CharacterHealth) return true;
        }
        return false;
    } 
}