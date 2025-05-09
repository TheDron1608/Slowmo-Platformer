using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(5)]
public class CharacterAIManager : AbstractCharacterComponent
{
    [SerializeField] private List<AbstractCharacterStateBehaviourAI> _stateBehaviourAIs;
    private AbstractCharacterStateBehaviourAI _currentActiveStateBehaviour = null;

    public List<AbstractCharacterStateBehaviourAI> StateBehaviourAIs
    {
        get => _stateBehaviourAIs;
        set
        {
            _stateBehaviourAIs = value;
            UpdateStateBehaviourAIs();
        }
    }

    public AbstractCharacterStateBehaviourAI CurrentActiveStateBehaviour
    {
        get => _currentActiveStateBehaviour;
        private set
        {
            if (_currentActiveStateBehaviour == value) return;

            _currentActiveStateBehaviour?.gameObject.SetActive(false);
            value?.gameObject.SetActive(true);
            _currentActiveStateBehaviour = value;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        UpdateStateBehaviourAIs();
        UpdateCurrentActionStateBehaviour();
    }

    private void UpdateStateBehaviourAIs()
    {
        _stateBehaviourAIs = transform.GetComponentsInChildren<AbstractCharacterStateBehaviourAI>(false).ToList();
        _stateBehaviourAIs.Sort();
        for (int i = 0; i <  _stateBehaviourAIs.Count; i++)
        {
            if (_stateBehaviourAIs[i].StateBehaviourCondition())
            {
                for (int j = 0; j < _stateBehaviourAIs.Count; j++)
                {
                    _stateBehaviourAIs[j].enabled = i == j;
                }
                break;
            }
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
                CurrentActiveStateBehaviour = _stateBehaviourAIs[i];
                return;
            }
        }
        throw new UnityException("could not find any valid CharacterStateBehaviourAI, use DefaultStateBehaviourAI to solve this exception");
    }
}
