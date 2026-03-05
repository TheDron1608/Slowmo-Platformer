using System.Collections;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    const float GAME_OVER_UPDATES_PER_SECOND = 10f;

    private void Awake()
    {
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
        bool allDead = true;
        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers())
        {
            if (
                !character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() ||
                character.CharComponents.CharacterEffectsReceiver.GetHasEffect<Resurrect>(true)
                ) allDead = false;
        }

        if (allDead)
        {
            UIManager.Instance.GameplayScreenOverlay.Hide();
            UIManager.Instance.DamagedScreenOverlay.Hide();
            UIManager.Instance.GameOverScreenOverlay.Show();
        }
        else
        {
            UIManager.Instance.GameplayScreenOverlay.Show();
            UIManager.Instance.DamagedScreenOverlay.Show();
            UIManager.Instance.GameOverScreenOverlay.Hide();
        }
    }
}