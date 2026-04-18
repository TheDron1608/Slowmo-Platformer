using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    const float GAME_OVER_UPDATES_PER_SECOND = 10f;

    private GameOverUIManager.GameOverReasons? _forceFinishGame = null;

    public void ForceFinishGame(GameOverUIManager.GameOverReasons reason)
    {
        _forceFinishGame = reason;
    }

    private void Awake()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 GameOverManager instance per scene");
        Instance = this;

        StartCoroutine(UpdateGameOverLoop());
    }

    private IEnumerator UpdateGameOverLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / GAME_OVER_UPDATES_PER_SECOND);

            CheckIsGameOver();
        }
    }

    private void CheckIsGameOver()
    {
        if (_forceFinishGame.HasValue)
        {
            SetGameplayUIShown(false);
            UIManager.Instance.GameOverScreenOverlay.Show(_forceFinishGame.Value);
        }
        else if (CheckIsAllDead())
        {
            SetGameplayUIShown(false);
            UIManager.Instance.GameOverScreenOverlay.Show(GameOverUIManager.GameOverReasons.ALL_DEAD);
        }
        else
        {
            SetGameplayUIShown(true);
            UIManager.Instance.GameOverScreenOverlay.Hide();
        }
    }

    private bool CheckIsAllDead()
    {
        bool allDead = true;
        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers())
        {
            if (
                !character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() ||
                character.CharComponents.CharacterEffectsReceiver.GetHasEffect<Resurrect>(true)
                ) allDead = false;
        }

        return allDead;
    }

    private void SetGameplayUIShown(bool value)
    {
        UIManager.Instance.GameplayScreenOverlay.SetShown(value);
        UIManager.Instance.DamagedScreenOverlay.SetShown(value);
        UIManager.Instance.ModificatorsScreenOverlay.SetShown(value);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}