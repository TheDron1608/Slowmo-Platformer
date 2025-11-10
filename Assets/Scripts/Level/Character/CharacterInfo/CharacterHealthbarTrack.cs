using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealthbarTrack : AbstractCharacterComponent
{
    private void OnEnable()
    {
        GameplayUIManager.Instance?.AddTrackedCharacter(CharComponents);
    }

    private void OnDisable()
    {
        GameplayUIManager.Instance?.RemoveTrackedCharacter(CharComponents);
    }
}
