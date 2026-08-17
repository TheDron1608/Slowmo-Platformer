using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using Unity.VisualScripting;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    const float GAME_OVER_UPDATES_PER_SECOND = 10f;

    private List<System.Func<bool>> _extraAllDeadGameOverConditions = new();
    private GameOverUIManager.GameOverReasons? _forceFinishGame = null;
    private bool _gameWasFinishedBefore = false;

    public List<System.Func<bool>> ExtraAllDeadGameOverConditions
    {
        get => _extraAllDeadGameOverConditions;
        set => _extraAllDeadGameOverConditions = value;
    }

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
        if (
            _forceFinishGame.HasValue ||
            (CheckIsAllDead() && _extraAllDeadGameOverConditions.All(e => e.Invoke()))
            )
        {
            SetGameplayUIShown(false);
            UIManager.Instance.GameOverScreenOverlay.Show(_forceFinishGame ?? GameOverUIManager.GameOverReasons.ALL_DEAD);

            if (!_gameWasFinishedBefore)
            {
                AnalyticsManager.Instance?.RecordEvent(new GameOverAnalyticsEvent());
                BossInitializer.Instance?.BossWinQuote();
            }
            _gameWasFinishedBefore = true;
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
        UIManager.Instance.ModificatorsScreenOverlay.SetShown(value);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}