using UnityEngine;

public abstract class AbstractAI : AbstractCharacterComponent
{
    public enum AITypes
    {
        MOVEMENT,
        AIM,
        PICKUP
    }

    public readonly AITypes AIType;
}
