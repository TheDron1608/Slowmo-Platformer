
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class RandomPlayerCharacterModificator : AbstractModificator
{
    private PlayerCharacterInfo _defaultPlayerCharacter = null;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _defaultPlayerCharacter = SessionManager.Instance.CurrentSelectedPlayer;

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        TrySetCharacter(NumberMath.PickRandomItem(SessionManager.Instance.GetUnlockedCharacters(), SessionManager.Instance.CurrentSelectedPlayer));
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

        if (TrySetCharacter(_defaultPlayerCharacter))
        {
            if (SceneList.GetCurrentSceneIsGameplay())
            {
                SpawnManager.Instance.FinishGameplay(
                    TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers().FirstOrDefault(), 
                    SceneList.GAMEPLAY
                    );
            }
        }
    }

    private bool TrySetCharacter(PlayerCharacterInfo character)
    {
        if (SessionManager.Instance.CurrentSelectedPlayer == character || character == null) return false;

        SessionManager.Instance.CurrentSelectedPlayer = character;

        for (int i = 0; i < ModificatorsManager.Instance.CurrentModificators.Count; i++)
        {
            if (ModificatorsManager.Instance.CurrentModificators[i].Status == ModificatorStatuses.CHARACTER_DEFAULT)
            {
                ModificatorsManager.Instance.RemoveModificatorAt(i);
                i--;
            }
        }

        foreach (var startMod in character.StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(startMod, ModificatorStatuses.CHARACTER_DEFAULT);
        }

        return true;
    }
}