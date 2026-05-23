using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using Unity.Services.Core;

using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UnityConsent;

public class AnalyticsManager : MonoBehaviour
{
    const float TRACK_INFO_UPDATE_PER_SECOND = 1f;

    public static AnalyticsManager Instance = null;

    public bool AllowAnalyticsData = true;
    public bool AllowAdsData = false;
    public bool LogErrors = true;

    private List<float> _trackedPlayerHealth = new();
    private List<int> _trackedCombo = new();
    private bool _serviceInitialized = false;
    private bool _collectData = false;

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
            ConsentState newConsent = new();
            newConsent.AnalyticsIntent = value && AllowAnalyticsData ? ConsentStatus.Granted : ConsentStatus.Denied;
            newConsent.AdsIntent = value && AllowAdsData ? ConsentStatus.Granted : ConsentStatus.Denied;

            if (UnityEngine.Analytics.Analytics.limitUserTracking)
            {
                EndUserConsent.SetConsentState(newConsent);
            }

            _collectData = value;
        }
    }

    public void RecordEvent(Unity.Services.Analytics.Event recordEvent) 
    {
        try
        {
            AnalyticsService.Instance.RecordEvent(recordEvent);
        }
        catch (Exception e)
        {
            if (LogErrors)
            {
                Debug.LogWarning("sending analytics event error: " + e.ToString());
            }
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

        Analytics.initializeOnStartup = false;
        Analytics.enabled = false;
        PerformanceReporting.enabled = false;
        Analytics.limitUserTracking = true;
        Analytics.deviceStatsEnabled = false;

        CollectData = false;
    }

    private void OnEnable()
    {
        if (!_serviceInitialized) StartCoroutine(InitServices());
        StartCoroutine(TrackLoop());
    }

    private IEnumerator InitServices()
    {
        yield return UnityServices.InitializeAsync();

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