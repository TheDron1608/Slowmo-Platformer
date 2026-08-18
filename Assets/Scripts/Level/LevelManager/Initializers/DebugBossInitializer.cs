using System.Collections.Generic;
using UnityEngine;

public class DebugBossInitializer : MonoBehaviour
{
    public bool UseRealSave = false;
    public int RealSaveId;
    public PlayerCharacterInfo PlayerCharacter;
    public PlayerCharacterInfo BossCharacter;
    public List<AbstractModificator> StartModificators;
    public AbstractModificator.ModificatorStatuses StartModStatus = AbstractModificator.ModificatorStatuses.PERMANENT;

    private void Start()
    {
        if (UseRealSave)
        {
            SessionManager.Instance.CurrentSession = SessionManager.Instance.Sessions[RealSaveId];
            Debug.Log("load session #" + SessionManager.Instance.CurrentSession.Id);
        }
        else
        {
            SessionManager.Instance.CurrentSession = new();
            SessionManager.Instance.CurrentSession.CurrentBossName = BossCharacter.name;
        }

        SessionManager.Instance.CurrentSelectedPlayer = PlayerCharacter;

        foreach (var mod in StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(mod, StartModStatus);
        }

        UIManager.Instance.LoadSceneWithEffect(SceneList.BOSS);
    }
}