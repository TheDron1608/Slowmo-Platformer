using System.Collections.Generic;
using UnityEngine;

public class DebugBootstrapInitializer : MonoBehaviour
{
    public string SceneName = SceneList.GAMEPLAY;
    public CharacterComponentsManager StartCharacter;
    public List<AbstractModificator> StartModificators;
    public int StartScore = 0;

    private void Start()
    {
        SessionManager.Instance.CurrentSession = new();
        SpawnManager.Instance.PlayerCharacter = StartCharacter;
        ScoreManager.Instance.TradableScore = StartScore;
        foreach (AbstractModificator modificator in StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(modificator, AbstractModificator.ModificatorStatuses.CURSE);
        }
        UIManager.Instance.LoadSceneWithEffect(SceneName);
    }
}
