using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAIManager : AbstractCharacterComponent
{
    const string AI_CONTAINER_NAME = "AIContainer";

    private Transform _aiContainer;
    private List<AbstractAI> _currentAIs = new();

    protected override void OnAwake()
    {
        base.OnAwake();
        _aiContainer = transform.Find(AI_CONTAINER_NAME);
        foreach (Transform aiGameObject in _aiContainer.transform)
        {
            if (aiGameObject.TryGetComponent(out  AbstractAI ai))
            {
                _currentAIs.Add(ai);
            }
        }
    }

    public void AddAI(AbstractAI ai)
    {
        Instantiate(ai, _aiContainer);
    }

    public void RemoveAI(AbstractAI ai)
    {
        RemoveAI(ai.AIType);
    }

    public void RemoveAI(AbstractAI.AITypes aiType)
    {
        for (int i = 0; i < _currentAIs.Count; i++)
        {
            if (_currentAIs[i].AIType == aiType)
            {
                AbstractAI removeAi = _currentAIs[i];
                _currentAIs.RemoveAt(i);
                GameObject.Destroy(removeAi);
                break;
            }
        }
    }

    public void ReplaceAI(AbstractAI ai)
    {
        RemoveAI(ai);
        AddAI(ai);
    }
}
