using System;
using UnityEngine.Localization;

[Serializable]
public abstract class AbstractCharacterUnlockCondition
{
    public PlayerCharacterInfo UnlockCharacter;
    public LocalizedString LocalizaedUnlockCondition;

    public abstract bool UnlockCondition();
}