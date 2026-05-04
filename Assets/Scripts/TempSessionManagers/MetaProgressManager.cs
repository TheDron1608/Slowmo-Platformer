using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class MetaProgressManager : MonoBehaviour 
{
    public List<AbstractCharacterUnlockCondition> TotalUnlocks = new();

    private List<AbstractCharacterUnlockCondition> _currentLockedCharacters = new();

    private void Awake()
    {
        foreach (AbstractCharacterUnlockCondition unlock in TotalUnlocks)
        {
            if (!SessionManager.Instance.CurrentSession.UnlockedCharacters.Contains(unlock.UnlockCharacter.GetUnlockCharacterJSONName()))
            {
                _currentLockedCharacters.Add(unlock);
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _currentLockedCharacters.Count; i++)
        {
            if (_currentLockedCharacters[i].UnlockCondition())
            {
                SessionManager.Instance.CurrentSession.UnlockedCharacters.Add(_currentLockedCharacters[i].UnlockCharacter.GetUnlockCharacterJSONName());
                SessionManager.Instance.SaveCurrentSession();
                _currentLockedCharacters.RemoveAt(i);
                i--;
            }
        }
    }
}