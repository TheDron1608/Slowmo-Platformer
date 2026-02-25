using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SuperHotModificator : AbstractMultiplierableModificator
{
    const float TIME_SCALE_TOGGLE_SPEED = 15f;
    const float MIN_VELOCITY_FOR_NORMAL_TIME_SCALE = 3.5f;
    const string GAMEPLAY_SCENE_NAME = "Gameplay";

    public float TimeSpeedOnIdle = 0.25f;
    public float TimeSpeedOnMoving = 1f;
    public float MaxSlowmoTime = 5f;
    public float SlowmoTimeRegenerationPerSecond = 0.33f;
    public TeamManager.Teams TrackedTeam = TeamManager.Teams.PLAYER;

    private bool _isGameplay = false;
    private float _slowmoTimeLeft = 0f;
    private bool _isRecoveringTimeLeft = false;
    private float _currentTimeScaleMult = 1f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _isGameplay = SceneManager.GetActiveScene().name == GAMEPLAY_SCENE_NAME;
    }

    public override void OnLevelPreGenerated()
    {
        base.OnLevelPreGenerated();

        _isGameplay = true;
        _slowmoTimeLeft = MaxSlowmoTime;
        UIManager.Instance?.SlowmoOverlay.Show();
    }

    public override void OnLevelFinished()
    {
        base.OnLevelFinished();

        _isGameplay = false;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        UIManager.Instance?.SlowmoOverlay.Hide();
    }

    private void LateUpdate()
    {
        if (UIManager.GamePaused() || !_isGameplay) return;

        bool isIdle =
            !_isRecoveringTimeLeft &&
            TeamManager.Instance.GetTeamDataByTeam(TrackedTeam).GetTeamMembers().Any(
                character => (
                    (!character?.IsDestroyed()) ?? false) &&
                    !character.CharComponents.CharacterMoving.IsMoving() &&
                    !character.CharComponents.CharacterRolling.IsRolling &&
                    !character.CharComponents.CharacterJumping.GetIsJumping() &&
                    !character.CharComponents.CharacterVisual.IsBusy()
                );

        float oldScaleMult = _currentTimeScaleMult;
        _currentTimeScaleMult = NumberMath.LimitFloatInRange(
            math.lerp(_currentTimeScaleMult, isIdle ? TimeSpeedOnIdle : TimeSpeedOnMoving, Time.unscaledDeltaTime * TIME_SCALE_TOGGLE_SPEED),
            math.min(TimeSpeedOnIdle, TimeSpeedOnMoving),
            math.max(TimeSpeedOnIdle, TimeSpeedOnMoving)
            );

        TimeManager.Instance.CurrentTimeScale = TimeManager.Instance.CurrentTimeScale / oldScaleMult * _currentTimeScaleMult;

        if (isIdle)
        {
            _slowmoTimeLeft -= Time.unscaledDeltaTime;
        }
        else
        {
            _slowmoTimeLeft += Time.unscaledDeltaTime;
        }

        if (_slowmoTimeLeft < 0f)
        {
            _slowmoTimeLeft = 0f;
            _isRecoveringTimeLeft = true;
        }
        else if (_slowmoTimeLeft > MaxSlowmoTime)
        {
            _slowmoTimeLeft = MaxSlowmoTime;
            _isRecoveringTimeLeft = false;
        }
    }
}