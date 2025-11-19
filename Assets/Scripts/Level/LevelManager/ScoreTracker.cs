using System;
using System.Collections;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    private void Start()
    {
        TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).OnTeamMemberDidKill += PlayerTeam_OnTeamMemberDidKill;
        TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).OnTeamMemberKilled += ScoreTracker_OnTeamMemberKilled;
    }

    private void PlayerTeam_OnTeamMemberDidKill(object sender, CharacterTeam e)
    {
        SessionManager.Instance.CurrentSession.CurrentKills++;
    }
    private void ScoreTracker_OnTeamMemberKilled(object sender, CharacterTeam e)
    {
        SessionManager.Instance.CurrentSession.CurrentDeaths++;
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(PlayTimeSecondsCounter());
    }

    private IEnumerator PlayTimeSecondsCounter()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (!UIManager.GamePaused())
            {
                SessionManager.Instance.CurrentSession.CurrentPlayTime += new TimeSpan(0, 0, 1);
            }
        }
    }

    private void OnDestroy()
    {
        TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).OnTeamMemberDidKill -= PlayerTeam_OnTeamMemberDidKill;
        TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).OnTeamMemberKilled -= ScoreTracker_OnTeamMemberKilled;
    }
}