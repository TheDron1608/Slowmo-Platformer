using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SuperHotModificator : AbstractMultiplierableModificator
{
    const float MIN_VELOCITY_FOR_NORMAL_TIME_SCALE = 1f;
    const string GAMEPLAY_SCENE_NAME = "Gameplay";

    public float TimeSpeedOnIdle = 0.25f;
    public float TimeSpeedOnMoving = 1f;
    public float MaxSlowmoTime = 5f;
    public float SlowmoTimeRegenerationPerSecond = 0.33f;
    public TeamManager.Teams TrackedTeam = TeamManager.Teams.PLAYER;

    private bool _isGameplay = false;
    private float _currentTimeScaleMultiplier = 1f;
    private float _currentFixedTimeScaleMultiplier = 1f;
    private float _defaultFixedUpdateDelay = 0.02f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _isGameplay = SceneManager.GetActiveScene().name == GAMEPLAY_SCENE_NAME;
        _defaultFixedUpdateDelay = Time.fixedDeltaTime;
    }

    public override void OnLevelPreGenerated()
    {
        base.OnLevelPreGenerated();

        _isGameplay = true;
    }

    public override void OnLevelFinished()
    {
        base.OnLevelFinished();

        _isGameplay = false;
    }

    private void LateUpdate()
    {
        if (_isGameplay)
        {
            float maxCharactersVelocity = 0f;
            foreach (CharacterTeam teamMember in TeamManager.Instance.GetTeamDataByTeam(TrackedTeam).GetTeamMembers())
            {
                if (
                    teamMember != null && !teamMember.IsDestroyed() && 
                    teamMember.CharComponents.CharacterMoving.IsAbleToMove &&
                    teamMember.CharComponents.CharacterAttacking.On
                    )
                {
                    float teamMemberVelocity = teamMember?.CharComponents?.CharacterRigidBody.linearVelocity.magnitude ?? 0f;
                    if (maxCharactersVelocity < teamMemberVelocity)
                    {
                        maxCharactersVelocity = teamMemberVelocity;
                    }
                }
            }

            float targetScale = math.lerp(
                TimeSpeedOnIdle,
                TimeSpeedOnMoving,
                NumberMath.LimitFloatBetweenZeroAndOne(maxCharactersVelocity / MIN_VELOCITY_FOR_NORMAL_TIME_SCALE)
                );

            UpdateCurrentTimeScale(targetScale, targetScale);
        }
        else
        {
            UpdateCurrentTimeScale(1f, 1f);
        }
    }

    private void UpdateCurrentTimeScale(float timeScale, float fixedTimeScale)
    {
        if (_currentTimeScaleMultiplier != timeScale)
        {
            Time.timeScale = Time.timeScale / _currentTimeScaleMultiplier * timeScale;
            _currentTimeScaleMultiplier = timeScale;
        }
        if (_currentFixedTimeScaleMultiplier != fixedTimeScale)
        {
            Time.fixedDeltaTime = Time.fixedDeltaTime / _currentFixedTimeScaleMultiplier * fixedTimeScale;
            _currentFixedTimeScaleMultiplier = fixedTimeScale;
        }
    }
}