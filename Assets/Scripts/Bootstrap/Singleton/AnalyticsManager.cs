using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using Unity.Services.Core;

using UnityEngine;
using UnityEngine.SceneManagement;

public class AnalyticsManager : MonoBehaviour
{
    const float TRACK_INFO_UPDATE_PER_SECOND = 1f;

    public static AnalyticsManager Instance = null;

    public bool LogErrors = true;

    private List<float> _trackedPlayerHealth = new();
    private List<int> _trackedCombo = new();
    private bool _serviceInitialized = false;
    private bool _collectData = true;

    public List<float> TrackedPlayerHealth
    {
        get => _trackedPlayerHealth;
    }

    public List<int> TrackedCombo
    {
        get => _trackedCombo;
    }

    public bool CollectData
    {
        get => _collectData;
        set
        {
            if (_collectData == value) return;

            if (value)
            {
                if (UnityEngine.Analytics.Analytics.limitUserTracking)
                {
                    AnalyticsService.Instance.StartDataCollection();
                }
            }
            else
            {
                AnalyticsService.Instance.StopDataCollection();
            }

            _collectData = value;
        }
    }

    public void ResetTrackedInfo()
    {
        _trackedPlayerHealth = new();
        _trackedCombo = new();
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("Limit of 1 AnalyticsManager instance per scene");

        Instance = this;

        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        if (!_serviceInitialized) StartCoroutine(InitServices());
        StartCoroutine(TrackLoop());
    }

    private IEnumerator InitServices()
    {
        yield return UnityServices.InitializeAsync();

        UnityEngine.Analytics.Analytics.limitUserTracking = true;

        if (UnityEngine.Analytics.Analytics.limitUserTracking && _collectData)
        {
            AnalyticsService.Instance.StartDataCollection();
        }

        _serviceInitialized = true;
    }

    private IEnumerator TrackLoop()
    {
        while (true)
        {
            if (SceneList.GetCurrentSceneIsGameplay())
            {
                if (TeamManager.Instance != null)
                {
                    IEnumerable<CharacterTeam> validCharacters = TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers()
                        .Where(e => !e.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>());

                    if (validCharacters.Any())
                    {
                        _trackedPlayerHealth.Add(
                            TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers()
                            .Where(e => !e.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>())
                            .Average(e => e.CharComponents.CharacterHealth.CurrentHealth)
                            );
                    }
                }
                if (ScoreManager.Instance != null)
                {
                    _trackedCombo.Add(ScoreManager.Instance.CurrentCombo);
                }
            }

            yield return new WaitForSeconds(1 / TRACK_INFO_UPDATE_PER_SECOND);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}