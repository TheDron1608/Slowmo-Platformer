using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealthbarTrack : AbstractCharacterComponent
{
    public Material HealthbarMaterial;

    private Healthbar _currentHealthbar = null;

    private void Start()
    {
        _currentHealthbar = GameplayUIManager.Instance?.MultiHealthbarsManager.AddHealthbar(CharComponents.CharacterHealth, HealthbarMaterial);
    }

    private void OnDestroy()
    {
        GameplayUIManager.Instance?.MultiHealthbarsManager.RemoveHealthbar(_currentHealthbar);
    }
}
