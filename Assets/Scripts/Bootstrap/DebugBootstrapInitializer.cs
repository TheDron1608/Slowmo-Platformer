using System.Collections.Generic;
using UnityEngine;

public class DebugBootstrapInitializer : MonoBehaviour
{
    public string SceneName = SceneList.GAMEPLAY;
    public CharacterComponentsManager StartCharacter;
    public List<AbstractModificator> StartTradableModificators;
    public List<AbstractModificator> StartPermanentModificators;
    public bool OverrideModsPool = false;
    public List<AbstractModificator> ModsPool = new();
    public int StartScore = 0;
    public int StartMoney = 0;

    private void Start()
    {
        SessionManager.Instance.CurrentSession = new();
        SpawnManager.Instance.PlayerCharacter = StartCharacter;
        ScoreManager.Instance.TradableScore = StartScore;
        foreach (AbstractModificator modificator in StartTradableModificators)
        {
            ModificatorsManager.Instance.AddModificator(modificator, AbstractModificator.ModificatorStatuses.CURSE);
        }
        foreach (AbstractModificator modificator in StartPermanentModificators)
        {
            ModificatorsManager.Instance.AddModificator(modificator, AbstractModificator.ModificatorStatuses.PERMANENT);
        }
        if (OverrideModsPool) ModificatorsManager.Instance.ModificatorsPool = ModsPool;
        UIManager.Instance.LoadSceneWithEffect(SceneName);
        if (ComboMoneyModificator.Instance != null) ComboMoneyModificator.Instance.CurrentMoney = StartMoney;
    }
}
