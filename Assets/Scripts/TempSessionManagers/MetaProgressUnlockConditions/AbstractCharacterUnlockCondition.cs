using UnityEngine;

public abstract class AbstractCharacterUnlockCondition : ScriptableObject
{
    public PlayerCharacterInfo UnlockCharacter;

    public abstract bool UnlockCondition();
}