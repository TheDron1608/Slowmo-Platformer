using System.Collections.Generic;
using UnityEngine;

public class DebugBossInitializer : MonoBehaviour
{
    public PlayerCharacterInfo PlayerCharacter;
    public PlayerCharacterInfo BossCharacter;
    public List<AbstractModificator> StartModificators;
    public AbstractModificator.ModificatorStatuses StartModStatus = AbstractModificator.ModificatorStatuses.PERMANENT;

    private void Start()
    {
        SessionManager.Instance.CurrentSession = new();
        SessionManager.Instance.CurrentSelectedPlayer = PlayerCharacter;
        SessionManager.Instance.CurrentSession.CurrentBossName = BossCharacter.name;

        foreach (var mod in StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(mod, StartModStatus);
        }

        UIManager.Instance.LoadSceneWithEffect(SceneList.BOSS);
    }
}