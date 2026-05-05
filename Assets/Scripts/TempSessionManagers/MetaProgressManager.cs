using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class MetaProgressManager : MonoBehaviour 
{
    public static MetaProgressManager Instance = null;

    public List<AbstractCharacterUnlockCondition> TotalUnlocks = new();
    public List<PlayerCharacterInfo> DefaultUnlockedCharacters = new();

    private List<AbstractCharacterUnlockCondition> _currentLockedCharacters = new();

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 MetaProgressionManager instance");
        Instance = this;
        DontDestroyOnLoad(this);

        SessionManager.Instance.CurrentSessionChanged += Instance_CurrentSessionChanged;
    }

    private void Instance_CurrentSessionChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentLockedCharacters();
    }

    private void UpdateCurrentLockedCharacters()
    {
        _currentLockedCharacters = new();

        if (SessionManager.Instance?.CurrentSession != null)
        {
            foreach (AbstractCharacterUnlockCondition unlock in TotalUnlocks)
            {
                if (!SessionManager.Instance.CurrentSession.UnlockedCharacters.Contains(unlock.UnlockCharacter.GetUnlockCharacterJSONName()))
                {
                    _currentLockedCharacters.Add(unlock);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _currentLockedCharacters.Count; i++)
        {
            if (_currentLockedCharacters[i].UnlockCondition())
            {
                UIManager.Instance.UnlockedCharacterMessageOverlay.Show(_currentLockedCharacters[i].UnlockCharacter);

                SessionManager.Instance.CurrentSession.UnlockedCharacters.Add(_currentLockedCharacters[i].UnlockCharacter.GetUnlockCharacterJSONName());
                SessionManager.Instance.SaveCurrentSession();
                _currentLockedCharacters.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnDestroy()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.CurrentSessionChanged -= Instance_CurrentSessionChanged;
        }

        Instance = null;
    }
}