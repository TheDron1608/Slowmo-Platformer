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
    public AbstractAIPopupMessaging PopupMessaging;
    [Header("AIInfo")]
    [SerializeField] private AbstractAINearestEnemyInfo _nearestEnemyInfo = null;
    [SerializeField] private AbstractAIPathfinding _pathfinding = null;
    [SerializeField] private AbstractAIPrefferedHoldable _prefferedHoldable = null;

    public AbstractAINearestEnemyInfo NearestEnemyInfo
    {
        get => _nearestEnemyInfo ?? CharComponents.CharacterAIManager.DefaultStateBehavioAI._nearestEnemyInfo;
    }
    public AbstractAIPathfinding Pathfinding
    {
        get => _pathfinding ?? CharComponents.CharacterAIManager.DefaultStateBehavioAI._pathfinding;
    }
    public AbstractAIPrefferedHoldable PrefferedHoldable
    {
        get => _prefferedHoldable ?? CharComponents.CharacterAIManager.DefaultStateBehavioAI._prefferedHoldable;
    }

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

    public void ForceUpdateAllInfo()
    {
        _nearestEnemyInfo?.ForceUpdateInfo();
        _pathfinding?.ForceUpdateInfo();
        _prefferedHoldable?.ForceUpdateInfo();
    }

    public abstract bool StateBehaviourCondition();
}
