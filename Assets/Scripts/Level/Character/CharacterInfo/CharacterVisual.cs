using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.U2D;

[DefaultExecutionOrder(2)]
public class CharacterVisual : AbstractCharacterComponent
{
    const string CHARACTER_PARTS_GAMEOBJECT_NAME = "CharacterParts";
    const string ANIMATOR_MAIN_STATE_PARAM_NAME = "MainState";
    const string ANIMATOR_MOVE_SPEED_PARAM_NAME = "MoveSpeed";
    const string ANIMATOR_JUMP_STATE_PARAM_NAME = "JumpState";
    const string ANIMATOR_BUSY_STATE_PARAM_NAME = "BusyState";
    const string ANIMATOR_STUN_RECOVER_ANIMATION_TIME_MULTIPLIER_PARAM_NAME = "StunRecoverAnimationTimeMult";
    const string ANIMATOR_BREAK_BUSY_ANIMATION_TRIGGER_NAME = "BreakBusyAnimation";
    const string FAST_MOVE_ALIGN_CHANGE_TRIGGER_NAME = "FastMoveAlignChange";
    const string AIR_JUMP_TRIGGER_NAME = "AirJump";

    const float JUMP_VELOCITY_FOR_DEFAULT_JUMP_ANIMATION_STATE = 5f;
    const float MOVE_VELOCITY_FOR_DEFAULT_MOVE_ANIM_SPEED = 5f;
    const float COOL_FLIP_SPEED_MUTLIPLIER = 5f;
    const float COOL_FLIP_DEGREES = 360f * 2f;
    const float POPUP_ANIMATION_SPEED_MULT = 10f;
    const float POPUP_HIDE_ANIMATION_SPEED_MULT = 30f;
    const float POPUP_ANIMATION_DEFAULT_HEIGHT = 0.33f;
    const float DETECTED_ENEMY_POPUP_DURATION = 1.5f;
    const float DEATH_POPUP_DURATION = 0.25f;
    const float POPUP_FRAMERATE = 5f;

    const int HEARD_NOISE_POPUP_PRIORITY = 1;
    const int DETECTED_ENEMY_POPUP_PRIORITY = 2;
    const int STUNNED_POPUP_PRIORITY = 3;
    const int DEATH_POPUP_PRIORITY = 4;

    public enum CharacterPartMainStates
    {
        IDLE = 0,
        MOVE = 1,
        JUMP = 2,
        SLIDE_ON_WALL = 3,
        DEAD = 4,
        DEAD_BROKEN_NECK = 5
    }
    public enum CharacterPartBusyStates
    {
        NONE = 0,
        LOOK_FORWARD = 1,
        LOOK_BACKWARD = 2,
        LOOK_FORWARD_REVERSED = 3,
        LOOK_BACKWARD_REVERSED = 4,
        ROLL = 5,
        FALLING_IN_AIR = 6,
        FALLEN_ON_FLOOR = 7,
        MINOR_STUN = 8,
        CLUMSY_MOVE_ALIGN_CHANGE = 9,
        CLUMSY_JUMP_CHANGE = 10,
        CLUMSY_MELEE_ATTACK = 11,
        AIM = 12,
        CLUMSY_RELOAD = 13,
        CLUMSY_SHIELD = 14,
        FINISH_OFF = 15,
        BREAK_NECK = 16,
        BROKE_NECK = 17,
        KICK = 18
    }

    public class OnBusyStateChangedEventArgs
    {
        public CharacterPartBusyStates OldState;
        public CharacterPartBusyStates NewState;

        public OnBusyStateChangedEventArgs(CharacterPartBusyStates oldState, CharacterPartBusyStates newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
    public class OnMainStateChangedEventArgs
    {
        public CharacterPartMainStates OldState;
        public CharacterPartMainStates NewState;

        public OnMainStateChangedEventArgs(CharacterPartMainStates oldState, CharacterPartMainStates newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    public CharacterMultiSpritesSO MultiSpritesSO;
    public List<Sprite> HeardNoiseSprites;
    public List<Sprite> DetectedEnemySprites;
    public List<Sprite> StunnedSprites;
    public List<Sprite> DeathSprites;
    [SerializeField] private SpriteRenderer _popupImage;
    [SerializeField] private float _stunRecoverAnimationTimeMult = 1f;

    private bool _flippedH = false;
    private CharacterPartMainStates _mainState = CharacterPartMainStates.IDLE;
    private float _jumpState = 0f;
    private float _moveSpeed = 1f;
    private CharacterPartBusyStates _currentBusyAnimation = CharacterPartBusyStates.NONE; //when busy animation is played, character is unable to do most actions
    private Transform _characterPartsContainer;
    private Sprite _spritePrevFrame;
    private int _randomizedExtraSpriteSortingOrder;
    private Coroutine _coolFlipCoroutine = null;
    private bool _currentCoolFlipRotationAxisReversed = false;
    private bool _allowMovementOnBusyAnimation = false;
    private float _currentPopupDuration = 0f;
    private float _targetPopupDuration = float.MaxValue;
    private float _targetPopupHeight = 0f;
    private List<Sprite> _currentPopupSprites = new();
    private int _currentPopupFrame = 0;
    private float _timeSinceLastPopupFrame = 0f;
    private int _currentPopupPriority = -1;

    public event EventHandler<OnMainStateChangedEventArgs> OnMainStateChanged;
    public event EventHandler<OnBusyStateChangedEventArgs> OnBusyStateChanged;
    public event EventHandler<Sprite> OnSampleSpriteChanged;
    public event EventHandler<bool> OnSpriteFlippedChanged;

    protected override void OnAwake()
    {
        base.OnAwake();
        _characterPartsContainer = transform.Find(CHARACTER_PARTS_GAMEOBJECT_NAME);
        _spritePrevFrame = CharComponents.SampleSpriteRenderer.sprite;
        _randomizedExtraSpriteSortingOrder = (int)(UnityEngine.Random.value * 99f);
        CharComponents.CharacterJumping.OnStartedJumping += CharacterJumping_OnStartedJumping;
        CharComponents.CharacterAttacking.OnAttack += CharacterAttacking_OnAttack;
        CharComponents.CharacterReloading.OnReload += CharacterReloading_OnReload;
        CharComponents.Animator.SetFloat(ANIMATOR_STUN_RECOVER_ANIMATION_TIME_MULTIPLIER_PARAM_NAME, _stunRecoverAnimationTimeMult);
    }

    private void OnEnable()
    {
        _mainState = CharacterPartMainStates.IDLE;
        _currentBusyAnimation = CharacterPartBusyStates.NONE;
    }

    public bool FlippedH
    {
        get => _flippedH;
        set
        {
            if (_flippedH != value)
            {
                CharComponents.NavPointsContainer.transform.localScale = new Vector3(
                    value ? -1f : 1f,
                    CharComponents.NavPointsContainer.transform.localScale.y,
                    CharComponents.NavPointsContainer.transform.localScale.z
                    );
                OnSpriteFlippedChanged?.Invoke(this, value);
                _flippedH = value;
            }
        }
    }

    public CharacterPartMainStates MainState
    {
        get => _mainState;
        set
        {
            if (_mainState == value) return;
            OnMainStateChanged?.Invoke(this, new OnMainStateChangedEventArgs(_mainState, value));

            _mainState = value;
            CharComponents.Animator.SetInteger(ANIMATOR_MAIN_STATE_PARAM_NAME, (int)value);
        }
    }

    public CharacterPartBusyStates CurrentBusyAnimation
    {
        get => _currentBusyAnimation;
        set
        {
            if (_currentBusyAnimation == value) return;

            OnBusyStateChanged?.Invoke(this, new OnBusyStateChangedEventArgs(_currentBusyAnimation, value));

            _currentBusyAnimation = value;
            CharComponents.Animator.SetInteger(ANIMATOR_BUSY_STATE_PARAM_NAME, (int)value);
        }
    }

    public bool AllowMovementOnBusyAnimation
    {
        get => _allowMovementOnBusyAnimation;
        set => _allowMovementOnBusyAnimation = value;
    }

    public bool IsBusy()
    {
        return _currentBusyAnimation != CharacterPartBusyStates.NONE;
    }

    public bool IsClumsyAnimation()
    {
        return
            CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterPartBusyStates.CLUMSY_MOVE_ALIGN_CHANGE ||
            CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterPartBusyStates.CLUMSY_MELEE_ATTACK ||
            CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterPartBusyStates.CLUMSY_MOVE_ALIGN_CHANGE ||
            CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterPartBusyStates.AIM;
    }

    public void BreakBusyAnimation()
    {
        if (!IsBusy()) return;

        CharComponents.Animator.SetTrigger(ANIMATOR_BREAK_BUSY_ANIMATION_TRIGGER_NAME);
    }

    /// <summary>
    /// Invoke only if you are sure any busy animation is currenly active
    /// </summary>
    public void ForceResetBusyAnimation()
    {
        CurrentBusyAnimation = CharacterPartBusyStates.NONE;
        CharComponents.Animator.SetTrigger(ANIMATOR_BREAK_BUSY_ANIMATION_TRIGGER_NAME);
    }

    public void BreakAirJumping()
    {
        if (!IsBusy() && MainState == CharacterPartMainStates.JUMP)
        {
            CharComponents.Animator.SetTrigger(ANIMATOR_BREAK_BUSY_ANIMATION_TRIGGER_NAME);
        }
    }

    public void FastMoveAlignChange()
    {
        if (
            !IsBusy() &&
            (MainState == CharacterPartMainStates.MOVE || MainState == CharacterPartMainStates.IDLE)
            )
        {
            ForceResetBusyAnimation();
            CharComponents.Animator.SetTrigger(FAST_MOVE_ALIGN_CHANGE_TRIGGER_NAME);
        }
    }

    public float JumpState
    {
        get => _jumpState;
        set
        {
            _jumpState = value;

            float normalizedTime = value;

            //converts range [-inf; +inf] into (-1; 1)
            if (normalizedTime < -0.95f) normalizedTime = -0.95f;
            else if (normalizedTime > 0.95f) normalizedTime = 0.95f;

            normalizedTime = 1f - (normalizedTime + 1f) / 2f; //converts range (-1; 1) into (1; 0)

            CharComponents.Animator.SetFloat(ANIMATOR_JUMP_STATE_PARAM_NAME, normalizedTime);
        }
    }

    public float MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            _moveSpeed = value;
            CharComponents.Animator.SetFloat(ANIMATOR_MOVE_SPEED_PARAM_NAME, value);
        }
    }

    public float StunRecoverAnimationTimeMult
    {
        get => _stunRecoverAnimationTimeMult;
        set
        {
            if (_stunRecoverAnimationTimeMult == value) return;

            _stunRecoverAnimationTimeMult = value;
            CharComponents.Animator.SetFloat(ANIMATOR_STUN_RECOVER_ANIMATION_TIME_MULTIPLIER_PARAM_NAME, _stunRecoverAnimationTimeMult);
        }
    }

    public int RandomExtraSpriteRendererSortingOrder
    {
        get => _randomizedExtraSpriteSortingOrder;
    }

    public bool GetIsVisible()
    {
        return CharComponents.CharacterPartsManager.CharacterParts.First()?.CharPartVisual.IsVisible() ?? false;
    }

    public void PopupHeardNoise()
    {
        AddPopupMessage(HeardNoiseSprites, float.MaxValue, HEARD_NOISE_POPUP_PRIORITY);
    }
    public void RemovePopupHeardNoise()
    {
        RemovePopupMessage(HEARD_NOISE_POPUP_PRIORITY);
    }

    public void PopupDetectedEnemy()
    {
        AddPopupMessage(DetectedEnemySprites, DETECTED_ENEMY_POPUP_DURATION, DETECTED_ENEMY_POPUP_PRIORITY);
    }
    public void RemoveDetectedEnemy()
    {
        RemovePopupMessage(DETECTED_ENEMY_POPUP_PRIORITY);
    }

    public void PopupStunned()
    {
        AddPopupMessage(StunnedSprites, float.MaxValue, STUNNED_POPUP_PRIORITY);
    }
    public void RemovePopupStunned()
    {
        RemovePopupMessage(STUNNED_POPUP_PRIORITY);
    }

    public void PopupDeath()
    {
        AddPopupMessage(DeathSprites, DEATH_POPUP_DURATION, DEATH_POPUP_PRIORITY, 0f);
    }
    public void RemovePopupDeath()
    {
        RemovePopupMessage(DEATH_POPUP_PRIORITY);
    }

    private void AddPopupMessage(List<Sprite> sprites, float duration, int priority, float popupHeight = POPUP_ANIMATION_DEFAULT_HEIGHT)
    {
        if (priority <= _currentPopupPriority) return;

        _currentPopupSprites = sprites;
        _targetPopupDuration = duration;
        _targetPopupHeight = popupHeight;
        _currentPopupDuration = 0f;
        _currentPopupPriority = priority;
        _timeSinceLastPopupFrame = 999f;

        _popupImage.transform.position = CharComponents.Center.transform.position;
        _popupImage.sharedMaterial = DifficultyManager.Instance.CurrentDifficulty.Value.PrimaryEnviromentMaterial;
        _popupImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private void RemovePopupMessage(int priority)
    {
        if (priority != _currentPopupPriority) return;

        _currentPopupSprites = null;
        _currentPopupPriority = -1;
    }

    public void DoACoolFlip()
    {
        if (_coolFlipCoroutine != null) StopCoroutine(_coolFlipCoroutine);
        _coolFlipCoroutine = StartCoroutine(CoolFlipCoroutine());
        _currentCoolFlipRotationAxisReversed = !_currentCoolFlipRotationAxisReversed;
    }
    private IEnumerator CoolFlipCoroutine()
    {
        float totalRotation = 0;
        while (math.abs(totalRotation) < COOL_FLIP_DEGREES - 0.5f)
        {
            totalRotation = math.lerp(
                totalRotation,
                _currentCoolFlipRotationAxisReversed ? -COOL_FLIP_DEGREES : COOL_FLIP_DEGREES,
                Time.deltaTime * COOL_FLIP_SPEED_MUTLIPLIER
                );

            Quaternion newRotation = new();
            newRotation.eulerAngles = new Vector3(0f, 0f, totalRotation);
            CharComponents.CharacterPartsContainer.transform.localRotation = newRotation;

            yield return new WaitForEndOfFrame();
        }
        CharComponents.CharacterPartsContainer.transform.rotation = CharComponents.transform.rotation;

        _coolFlipCoroutine = null;
    }

    private void Update()
    {
        Profiler.BeginSample("UpdateMainStateParam");
        UpdateMainStateParam();
        Profiler.EndSample();

        Profiler.BeginSample("UpdateJumpStateParam");
        UpdateJumpStateParam();
        Profiler.EndSample();

        Profiler.BeginSample("UpdateMoveSpeedParam");
        UpdateMoveSpeedParam();
        Profiler.EndSample();

        Profiler.BeginSample("UpdateStunnedBusyStateParam");
        UpdateStunnedBusyStateParam();
        Profiler.EndSample();

        Profiler.BeginSample("UpdateSampleSpriteEvent");
        UpdateSampleSpriteEvent();
        Profiler.EndSample();

        Profiler.BeginSample("UpdatePopupMessage");
        UpdatePopupMessage();
        Profiler.EndSample();
    }

    private void UpdateJumpStateParam()
    {
        if (!CharComponents.CharacterCollision.IsCollidingFloor())
        {
            JumpState = CharComponents.CharacterRigidBody.linearVelocityY / JUMP_VELOCITY_FOR_DEFAULT_JUMP_ANIMATION_STATE;
        }
    }

    private void UpdateMainStateParam()
    {
        if (CharComponents.CharacterMoving == null) return;

        if (CharComponents.CharacterEffectsReceiver.TryGetEffect(out Death deathEffect))
        {
            if (deathEffect is DeathBrokenNeck)
            {
                MainState = CharacterPartMainStates.DEAD_BROKEN_NECK;
            }
            else
            {
                MainState = CharacterPartMainStates.DEAD;
            }
        }
        else
        {
            if (CharComponents.CharacterCollision.IsCollidingFloor())
            {
                if (
                    CharComponents.CharacterMoving.GetCurrentMoveDirection() == 0f ||
                    (CharComponents.CharacterMoving.GetCurrentMoveDirection() > 0f && CharComponents.CharacterCollision.IsCollidingRightWall()) ||
                    (CharComponents.CharacterMoving.GetCurrentMoveDirection() < 0f && CharComponents.CharacterCollision.IsCollidingLeftWall())
                    )
                {
                    MainState = CharacterPartMainStates.IDLE;
                }
                else
                {
                    MainState = CharacterPartMainStates.MOVE;

                    if (CharComponents.CharacterMoving.GetCurrentMoveDirection() > 0f)
                    {
                        FlippedH = false;
                    }
                    else if (CharComponents.CharacterMoving.GetCurrentMoveDirection() < 0f)
                    {
                        FlippedH = true;
                    }
                }
            }
            else
            {
                if (CharComponents.CharacterCollision.GetIsStickingOnWall())
                {
                    MainState = CharacterPartMainStates.SLIDE_ON_WALL;
                }
                else
                {
                    MainState = CharacterPartMainStates.JUMP;
                }

                if (CharComponents.CharacterMoving.GetCurrentMoveDirection() > 0f)
                {
                    FlippedH = false;
                }
                else if (CharComponents.CharacterMoving.GetCurrentMoveDirection() < 0f)
                {
                    FlippedH = true;
                }
            }
        }
    }

    private void UpdateMoveSpeedParam()
    {
        MoveSpeed =
            CharComponents.CharacterMoving.GetCurrentMoveDirection() * CharComponents.CharacterMoving.Speed /
            MOVE_VELOCITY_FOR_DEFAULT_MOVE_ANIM_SPEED * (FlippedH ? -1f : 1f);
    }

    private void UpdateStunnedBusyStateParam()
    {
        if (CharComponents.CharacterEffectsReceiver.GetHasEffect<HardStun>())
        {
            if (CharComponents.CharacterCollision.IsCollidingFloor())
            {
                CurrentBusyAnimation = CharacterPartBusyStates.FALLEN_ON_FLOOR;
            }
            else
            {
                CurrentBusyAnimation = CharacterPartBusyStates.FALLING_IN_AIR;
            }
        }
        else if (CharComponents.CharacterEffectsReceiver.GetHasEffect<MinorStun>())
        {
            CurrentBusyAnimation = CharacterPartBusyStates.MINOR_STUN;
        }
    }

    private void UpdateSampleSpriteEvent()
    {
        Sprite currentSprite = CharComponents.SampleSpriteRenderer.sprite;
        if (currentSprite != _spritePrevFrame)
        {
            OnSampleSpriteChanged?.Invoke(this, currentSprite);
            _spritePrevFrame = currentSprite;
        }
    }

    private void UpdatePopupMessage()
    {
        _currentPopupDuration += Time.deltaTime;
        _timeSinceLastPopupFrame += Time.deltaTime;

        _popupImage.flipX = FlippedH;
        _popupImage.transform.position = math.lerp(
            _popupImage.transform.position,
            CharComponents.Center.transform.position + Vector3.up * (_targetPopupHeight + CharComponents.CharacterRigidBodyCapsuleCollider.size.y / 2f),
            Time.deltaTime * POPUP_ANIMATION_SPEED_MULT
            );

        if (_currentPopupSprites != null && _currentPopupSprites.Count > 0 && _currentPopupDuration < _targetPopupDuration)
        {
            if (_timeSinceLastPopupFrame > 1f / POPUP_FRAMERATE)
            {
                _currentPopupFrame = (_currentPopupFrame + 1) % _currentPopupSprites.Count;
                _popupImage.sprite = _currentPopupSprites[_currentPopupFrame];
                _timeSinceLastPopupFrame = 0f;
            }

            _popupImage.color = new Color(1f, 1f, 1f, math.lerp(_popupImage.color.a, 1f, Time.deltaTime * POPUP_ANIMATION_SPEED_MULT));
            _popupImage.enabled = true;
        }
        else
        {
            _popupImage.color = new Color(1f, 1f, 1f, math.lerp(_popupImage.color.a, 0f, Time.deltaTime * POPUP_HIDE_ANIMATION_SPEED_MULT));
            if (_popupImage.color.a < 0.005f) _popupImage.enabled = false;
        }
    }

    public void Animator_FinishFinishingOff()
    {
        if (
            CharComponents.CharacterSpecial != null &&
            CharComponents.CharacterSpecial.TryGetComponent(out CharacterFinishOff finishingOff)
            )
        {
            finishingOff.Animator_FinishFinishingOff();
        }
    }

    public void Animator_FinishUnarmedAttacking()
    {
        CharComponents.UnarmedAttacking.RemoveAllProjectiles();
    }

    private void CharacterJumping_OnStartedJumping(object sender, EventArgs e)
    {
        if (CharComponents.CharacterJumping.GetIsAirJumping())
        {
            CharComponents.Animator.SetTrigger(AIR_JUMP_TRIGGER_NAME);
        }
    }

    private void CharacterReloading_OnReload(object sender, EventArgs e)
    {
        BreakAirJumping();
    }

    private void CharacterAttacking_OnAttack(object sender, bool e)
    {
        BreakAirJumping();
    }

    private void OnDestroy()
    {
        if (CharComponents.CharacterJumping != null) CharComponents.CharacterJumping.OnStartedJumping -= CharacterJumping_OnStartedJumping;
        if (CharComponents.CharacterAttacking != null) CharComponents.CharacterAttacking.OnAttack -= CharacterAttacking_OnAttack;
        if (CharComponents.CharacterReloading != null) CharComponents.CharacterReloading.OnReload -= CharacterReloading_OnReload;
    }
}
