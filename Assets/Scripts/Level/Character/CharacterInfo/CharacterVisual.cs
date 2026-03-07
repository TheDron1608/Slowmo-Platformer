using System;
using System.Collections;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(2)]
public class CharacterVisual : AbstractCharacterComponent
{
    const string CHARACTER_PARTS_GAMEOBJECT_NAME = "CharacterParts";
    const string ANIMATOR_MAIN_STATE_PARAM_NAME = "MainState";
    const string ANIMATOR_MOVE_SPEED_PARAM_NAME = "MoveSpeed";
    const string ANIMATOR_JUMP_STATE_PARAM_NAME = "JumpState";
    const string ANIMATOR_BUSY_STATE_PARAM_NAME = "BusyState";
    const string ANIMATOR_BREAK_BUSY_ANIMATION_TRIGGER_NAME = "BreakBusyAnimation";

    const float COOL_FLIP_SPEED_MUTLIPLIER = 5f;
    const float COOL_FLIP_DEGREES = 360f * 2f;

    public enum CharacterPartMainStates
    {
        IDLE = 0,
        MOVE = 1,
        JUMP = 2,
        SLIDE_ON_WALL = 3,
        DEAD = 4
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
        CLUMSY_SHIELD = 14
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

    /// <summary>
    /// Required Y velocity to change jump sprite
    /// </summary>
    public float JumpStateVelocityRange = 8f;
    /// <summary>
    /// Required X velocity to set move animation speed multiplier to 1.0,
    /// Example1:
    /// if chracter MoveSpeedVelocityRange = 10f, and character's vecloityX is 5f, then move animation speed multiplier is 0.5f (5f / 10f)
    /// Example2:
    /// if chracter MoveSpeedVelocityRange = 10f, and character's vecloityX is 20f, then move animation speed multiplier is 2f (20f / 10f)
    /// </summary>
    public float MoveSpeedVelocityRange = 8f;

    public CharacterMultiSpritesSO MultiSpritesSO;

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

    public int RandomExtraSpriteRendererSortingOrder
    {
        get => _randomizedExtraSpriteSortingOrder;
    }

    public bool GetIsVisible()
    {
        return CharComponents.CharacterPartsManager.CharacterParts.First()?.CharPartVisual.IsVisible() ?? false;
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
            totalRotation = math.lerp(totalRotation, _currentCoolFlipRotationAxisReversed ? -COOL_FLIP_DEGREES : COOL_FLIP_DEGREES, Time.deltaTime * COOL_FLIP_SPEED_MUTLIPLIER);

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
        UpdateMainStateParam();
        UpdateJumpStateParam();
        UpdateMoveSpeedParam();
        UpdateStunnedBusyStateParam();
        UpdateSampleSpriteEvent();
    }

    private void UpdateJumpStateParam()
    {
        if (!CharComponents.CharacterCollision.IsCollidingFloor())
        {
            JumpState = CharComponents.CharacterRigidBody.linearVelocityY / JumpStateVelocityRange;
        }
    }

    private void UpdateMainStateParam()
    {
        if (CharComponents.CharacterMoving == null) return;

        if (CharComponents.CharacterEffectsReceiver.GetHasEffect<Death>())
        {
            MainState = CharacterPartMainStates.DEAD;
        }
        else
        {
            if (CharComponents.CharacterCollision.IsCollidingFloor())
            {
                if (CharComponents.CharacterMoving.GetCurrentMoveDirection() == 0f || !CharComponents.CharacterMoving.IsAbleToMoveThisFrame)
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
        MoveSpeed = CharComponents.CharacterRigidBody.linearVelocityX / MoveSpeedVelocityRange * (FlippedH ? -1f : 1f);
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
}
