using System;
using UnityEngine;

[DefaultExecutionOrder(5)]
public abstract class AbstractCharacterStateBehaviourAI : AbstractCharacterComponent, IComparable<AbstractCharacterStateBehaviourAI>
{
    /// <summary>
    /// updates every AbstractCharacterStateBehaviourAI at CharacterAIManager by their UpdateOrder desending (i.e. the higher value, the earlier update)
    /// </summary>
    public int UpdateOrder = 0;
    [Header("AIBehaviour")]
    public AbstractAIAttacking Attacking;
    public AbstractAIReloading Reloading;
    public AbstractAIRolling Rolling;
    public AbstractAIMovingAndJumping MovingAndJumping;
    public AbstractAIGrabbingAndThrowing GrabbingAndThrowing;
    public AbstractAIInteracting Interacting;
    public AbstractAISpecial Special;
    [Header("AIInfo")]
    public AbstractAINearestEnemyInfo NearestEnemyInfo = null;
    public AbstractAIPathfinding Pathfinding = null;
    public AbstractAIPrefferedHoldable PrefferedHoldable = null;

    public int CompareTo(AbstractCharacterStateBehaviourAI other)
    {
        return UpdateOrder.CompareTo(other.UpdateOrder);
    }

    public void SetEnabledBehaviours(bool value)
    {
        foreach (AbstractAIBehaviour newValueAIBehaviour in GetComponents<AbstractAIBehaviour>())
        {
            newValueAIBehaviour.enabled = value;
        }
    }

    public abstract bool StateBehaviourCondition();
}
