using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public const float MUSIC_START_OR_END_DURATION = 2.5f;
    public const float VOLUME_CHANGE_DURATION = 0.5f;
    public const float MUSIC_VOLUME_ON_DIFFICULTY_FINISHED = 0.1f;

    public static MusicManager Instance;

    public StaticSoundPlayer MusicPlayer;
    public Sound ForcePlayMusic = null;
    public float TargetMusicVolume = 1f;

    private float _timeSinceStartMusic = 0f;
    private float _currentMusicVolume = 0f;
    private DifficultyManager.DifficultyStage _currentTrackedMusicStage = null;
    private Sound _requestChangeMusic = null;
    private float _lastDifficultyTime = 0f;

    public float CurrentMusicVolume
    {
        get => _currentMusicVolume;
        set => _currentMusicVolume = value;
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("Limit of 1 MusicManager per scene");
        Instance = this;

        if (ForcePlayMusic == null)
        {
            if (DifficultyManager.Instance != null)
            {
                SetMusic(DifficultyManager.Instance.CurrentDifficulty.Value.Music);
            }
        }
        else
        {
            MusicPlayer.PlaySound(ForcePlayMusic, true);
        }

    }

    private void OnEnable()
    {
        StartCoroutine(UnscaledUpdateLoop());
    }

    private IEnumerator UnscaledUpdateLoop()
    {
        while (true)
        {
            if (
                ForcePlayMusic == null && 
                DifficultyManager.Instance != null &&
                _currentTrackedMusicStage != DifficultyManager.Instance.CurrentDifficulty.Value
                )
            {
                if (!UIManager.Instance.DifficultyCurseChoiseScreenOverlay.IsShown())
                {
                    _currentTrackedMusicStage = DifficultyManager.Instance.CurrentDifficulty.Value;
                    _requestChangeMusic = DifficultyManager.Instance.CurrentDifficulty.Value.Music;
                }
            }
            else
            {
                _lastDifficultyTime = DifficultyManager.Instance?.CurrentDifficultyTime ?? 0f;
            }

            if (_requestChangeMusic != null && CurrentMusicVolume <= 0.05f)
            {
                SetMusic(_requestChangeMusic);
                _requestChangeMusic = null;
            }


            float targetVolume = _requestChangeMusic == null ? TargetMusicVolume : 0f;
            if (_currentMusicVolume != targetVolume)
            {
                if (_currentMusicVolume > targetVolume)
                {
                    _currentMusicVolume -= Time.unscaledDeltaTime / VOLUME_CHANGE_DURATION;
                    if (_currentMusicVolume < targetVolume) _currentMusicVolume = targetVolume;     
                }
                else
                {
                    _currentMusicVolume += Time.unscaledDeltaTime / VOLUME_CHANGE_DURATION;
                    if (_currentMusicVolume > targetVolume) _currentMusicVolume = targetVolume;
                }

            }

            _currentMusicVolume = Mathf.Min(
                _currentMusicVolume,
                _timeSinceStartMusic / MUSIC_START_OR_END_DURATION,
                (_currentTrackedMusicStage?.Duration - _lastDifficultyTime) 
                    / MUSIC_START_OR_END_DURATION + MUSIC_VOLUME_ON_DIFFICULTY_FINISHED ?? float.MaxValue
                );

            MusicPlayer.DynamicVolumeMultiplier = _currentMusicVolume;

            _timeSinceStartMusic += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }

    private void SetMusic(Sound music)
    {
        _timeSinceStartMusic = 0f;
        DifficultyManager.DifficultyStage currentStage = DifficultyManager.Instance.CurrentDifficulty.Value;
        MusicPlayer.DynamicVolumeMultiplier = 0f;
        MusicPlayer.PlaySound(
            currentStage.Music,
            false,
            null,
            DifficultyManager.Instance.CurrentDifficultyTime / currentStage.Duration
            );
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}