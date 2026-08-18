using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(60)]
public class CharacterAIManager : AbstractCharacterComponent
{
    [SerializeField] private List<AbstractCharacterStateBehaviourAI> _stateBehaviourAIs;
    [SerializeField] private AbstractCharacterStateBehaviourAI _defaultStateBehaviorAI;
    private AbstractCharacterStateBehaviourAI _currentActiveStateBehaviour = null;

    public List<AbstractCharacterStateBehaviourAI> StateBehaviourAIs
    {
        get => _stateBehaviourAIs;
        set
        {
            _stateBehaviourAIs = value;
            SortStateBehaviourAIs();
        }
    }

    public AbstractCharacterStateBehaviourAI DefaultStateBehavioAI
    {
        get => _defaultStateBehaviorAI;
    }

    public AbstractCharacterStateBehaviourAI AddStateBehaviourAI(AbstractCharacterStateBehaviourAI stateBehaviour)
    {
        AbstractCharacterStateBehaviourAI addedAI = Instantiate(stateBehaviour, transform);
        _stateBehaviourAIs.Add(addedAI);
        SortStateBehaviourAIs();
        UpdateCurrentActionStateBehaviour();
        return addedAI;
    }

    public void RemoveStateBehaviourAI(AbstractCharacterStateBehaviourAI stateBehaviour)
    {
        _stateBehaviourAIs.Remove(stateBehaviour);
        Destroy(stateBehaviour.gameObject);
        UpdateCurrentActionStateBehaviour();
    }

    public AbstractCharacterStateBehaviourAI CurrentActiveStateBehaviour
    {
        get => _currentActiveStateBehaviour;
        private set
        {
            if (_currentActiveStateBehaviour == value) return;

            _currentActiveStateBehaviour?.SetEnabledBehaviours(false);
            value?.SetEnabledBehaviours(true);

            _currentActiveStateBehaviour = value;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        SortStateBehaviourAIs();
    }

    private void SortStateBehaviourAIs()
    {
        _stateBehaviourAIs.OrderByDescending(state => state.UpdateOrder);
        for (int i = 0; i < _stateBehaviourAIs.Count; i++)
        {
            _stateBehaviourAIs[i].SetEnabledBehaviours(false);
        }
    }

    public void RemoveAI()
    {
        CharComponents.CharacterAIManager = null;
        Destroy(gameObject);
    }

    public void SetAIDisabled(bool value)
    {
        gameObject.SetActive(!value);
    }

    private void OnEnable()
    {
        UpdateCurrentActionStateBehaviour();
    }

    private void UpdateStateBehaviourAIs()
    {
        _stateBehaviourAIs = transform.GetComponentsInChildren<AbstractCharacterStateBehaviourAI>(false).ToList();
        _stateBehaviourAIs.OrderByDescending(state => state.UpdateOrder);
        for (int i = 0; i < _stateBehaviourAIs.Count; i++)
        {
            _stateBehaviourAIs[i].SetEnabledBehaviours(false);
        }
    }

    private void FixedUpdate()
    {
        UpdateCurrentActionStateBehaviour();
    }

    private void UpdateCurrentActionStateBehaviour()
    {
        for (int i = 0; i < _stateBehaviourAIs.Count; i++)
        {
            if (_stateBehaviourAIs[i].StateBehaviourCondition())
            {
                if (CurrentActiveStateBehaviour != _stateBehaviourAIs[i])
                {
                    CurrentActiveStateBehaviour = _stateBehaviourAIs[i];
                    DefaultStateBehavioAI.ForceUpdateAllInfo();
                }
                return;
            }
        }
        throw new UnityException("could not find any valid CharacterStateBehaviourAI, use DefaultStateBehaviourAI to solve this exception");
    }
}
