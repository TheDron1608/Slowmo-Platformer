using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class MusicManager : MonoBehaviour
{
    public const float MUSIC_START_OR_END_DURATION = 2.5f;
    public const float VOLUME_CHANGE_DURATION = 0.25f;
    public const float MUSIC_VOLUME_ON_DIFFICULTY_FINISHED = 0.1f;

    public static MusicManager Instance;

    public StaticSoundPlayer MusicPlayer;
    public Sound ForcePlayMusic = null;
    public float TargetMusicVolume = 1f;

    private float _timeSinceStartMusic = 0f;
    private float _currentMusicVolume = 0f;
    private DifficultyManager.DifficultyStage _currentTrackedMusicStage = null;
    private Sound _requestChangeMusic = null;
    private bool _requestChanceMusicLoop = false;
    private bool _requestKeepMusicTime = false;
    private float _lastDifficultyTime = 0f;
    private bool _wasPausedPrevUpdate = false;

    public float CurrentMusicVolume
    {
        get => _currentMusicVolume;
        set
        {
            _currentMusicVolume = value;
            MusicPlayer.DynamicVolumeMultiplier = _currentMusicVolume;
        }
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("Limit of 1 MusicManager per scene");
        Instance = this;

        if (ForcePlayMusic == null)
        {
            if (DifficultyManager.Instance != null)
            {
                SetMusic(DifficultyManager.Instance.CurrentDifficulty.Value.Music, false);
            }
        }
        else
        {
            MusicPlayer.PlaySound(ForcePlayMusic, true);
        }

    }

    private void OnEnable()
    {
        if (SoundManager.Instance == null) return;
        StartCoroutine(UnscaledUpdateLoop());
    }

    private IEnumerator UnscaledUpdateLoop()
    {
        while (true)
        {
            if (TimeManager.Instance != null)
            {
                if (TimeManager.Instance.Paused)
                {
                    _wasPausedPrevUpdate = true;
                }
                else if (_wasPausedPrevUpdate)
                {
                    _wasPausedPrevUpdate = false;
                    if (ForcePlayMusic == null && DifficultyManager.Instance != null)
                    {
                        SetMusic(DifficultyManager.Instance.CurrentDifficulty.Value.Music, false);
                    }
                }
            }

            if (
                ForcePlayMusic != null &&
                ForcePlayMusic != MusicPlayer.LastPlayedSound
                )
            {
                _requestChangeMusic = ForcePlayMusic;
                _requestChanceMusicLoop = true;
                _requestKeepMusicTime = true;
            }
            else if (
                ForcePlayMusic == null &&
                DifficultyManager.Instance != null &&
                _currentTrackedMusicStage != DifficultyManager.Instance.CurrentDifficulty.Value
                )
            {
                if (!UIManager.Instance.DifficultyCurseChoiseScreenOverlay.IsShown())
                {
                    _currentTrackedMusicStage = DifficultyManager.Instance.CurrentDifficulty.Value;
                    _requestChangeMusic = DifficultyManager.Instance.CurrentDifficulty.Value.Music;
                    _requestChanceMusicLoop = DifficultyManager.Instance.CurrentDifficulty.Next == null;
                    _requestKeepMusicTime = false;
                }
            }
            else
            {
                _lastDifficultyTime = DifficultyManager.Instance?.CurrentDifficultyTime ?? 0f;
            }

            if (_requestChangeMusic != null && CurrentMusicVolume <= 0.05f)
            {
                SetMusic(_requestChangeMusic, _requestChanceMusicLoop, _requestKeepMusicTime);
                _requestChangeMusic = null;
            }


            float targetVolume = _requestChangeMusic == null && (!(TimeManager.Instance?.Paused) ?? true) ? TargetMusicVolume : 0f;
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

            SoundManager.Instance.GameplayMusicVolume = _currentMusicVolume;

            _timeSinceStartMusic += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }

    private void SetMusic(Sound music, bool loop, bool keepTime = false)
    {
        if (SoundManager.Instance == null) return;

        _timeSinceStartMusic = 0f;
        DifficultyManager.DifficultyStage currentStage = DifficultyManager.Instance.CurrentDifficulty.Value;
        SoundManager.Instance.GameplayMusicVolume = 0f;
        MusicPlayer.PlaySound(
            music,
            loop,
            null,
                keepTime ? 
                MusicPlayer.PlayTime / MusicPlayer.CurrentClipDuration : 
                DifficultyManager.Instance.CurrentDifficultyTime / currentStage.Duration
            );
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}