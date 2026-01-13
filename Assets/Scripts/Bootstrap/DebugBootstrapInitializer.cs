using System.Collections.Generic;
using UnityEngine;

public class DebugBootstrapInitializer : MonoBehaviour
{
    public CharacterComponentsManager StartCharacter;
    public List<AbstractModificator> StartModificators;
    public int DebugSessionID;
    public string LoadSceneName = "Gameplay";

    private void Start()
    {
        SessionManager.Instance.CurrentSession = SessionManager.Instance.Sessions.Find((session) => session.Id == DebugSessionID);
        SpawnManager.Instance.PlayerCharacter = StartCharacter;
        foreach (AbstractModificator modificator in StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(modificator);
        }
        UIManager.Instance.LoadSceneWithEffect(LoadSceneName);
    }
}
