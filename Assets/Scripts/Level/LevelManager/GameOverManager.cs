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
        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers())
        {
            if (!character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()) return;
        }
        UIManager.Instance.GameplayScreenOverlay.Hide();
        UIManager.Instance.DamagedScreenOverlay.Hide();
        UIManager.Instance.GameOverScreenOverlay.Show();
    }
}