using System;
using Unity.Mathematics;
using UnityEngine;

public class CharacterVisual : AbstractCharacterComponent
{
    const string CHARACTER_PARTS_GAMEOBJECT_NAME = "CharacterParts";

    public class OnBusyStateChangedEventArgs
    {
        public CharacterPartVisual.CharacterPartBusyStates OldState;
        public CharacterPartVisual.CharacterPartBusyStates NewState;

        public OnBusyStateChangedEventArgs(CharacterPartVisual.CharacterPartBusyStates oldState, CharacterPartVisual.CharacterPartBusyStates newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
    public class OnMainStateChangedEventArgs
    {
        public CharacterPartVisual.CharacterPartMainStates OldState;
        public CharacterPartVisual.CharacterPartMainStates NewState;

        public OnMainStateChangedEventArgs(CharacterPartVisual.CharacterPartMainStates oldState, CharacterPartVisual.CharacterPartMainStates newState)
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

    private bool _flippedH = false;
    private CharacterPartVisual.CharacterPartMainStates _mainState = CharacterPartVisual.CharacterPartMainStates.IDLE;
    private float _jumpState = 0f;
    private float _moveSpeed = 1f;
    private CharacterPartVisual.CharacterPartBusyStates _currentBusyAnimation = CharacterPartVisual.CharacterPartBusyStates.NONE; //when busy animation is played, character is unable to do most actions
    private Transform _characterPartsContainer;

    public event EventHandler<OnMainStateChangedEventArgs> OnMainStateChanged;
    public event EventHandler<OnBusyStateChangedEventArgs> OnBusyStateChanged;

    protected override void OnAwake()
    {
        base.OnAwake();
        _characterPartsContainer = transform.Find(CHARACTER_PARTS_GAMEOBJECT_NAME);
    }

    public bool FlippedH
    {
        get => _flippedH;
        set {
            if (_flippedH == value) return;
            _flippedH = value;
            UpdateFlippedH();
        }
    }
    private void UpdateFlippedH()
    {
        transform.localScale = new Vector3(
            math.abs(transform.localScale.x) * (FlippedH ? -1f : 1f),
            transform.localScale.y,
            transform.localScale.z
            );
    }

    public CharacterPartVisual.CharacterPartMainStates MainState
    {
        get => _mainState;
        set
        {
            if (_mainState == value) return;

            OnMainStateChanged?.Invoke(this, new OnMainStateChangedEventArgs(_mainState, value));

            _mainState = value;
            UpdateMainState();
        }
    }
    private void UpdateMainState()
    {
        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPartVisual>(out CharacterPartVisual currentCharPart))
            {
                currentCharPart.SetMainState(_mainState);
            }
        }
    }

    public CharacterPartVisual.CharacterPartBusyStates CurrentBusyAnimation
    {
        get => _currentBusyAnimation;
        set
        {
            if (_currentBusyAnimation == value) return;

            OnBusyStateChanged?.Invoke(this, new OnBusyStateChangedEventArgs(_currentBusyAnimation, value));

            _currentBusyAnimation = value;
            UpdateBusyState();
        }
    }
    private void UpdateBusyState()
    {
        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPartVisual>(out CharacterPartVisual currentCharPart))
            {
                currentCharPart.SetBusyState(_currentBusyAnimation);
            }
        }
    }

    public bool IsBusy()
    {
        return _currentBusyAnimation != CharacterPartVisual.CharacterPartBusyStates.NONE;
    }

    public void BreakBusyAnimation()
    {
        if (!IsBusy()) return;

        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPartVisual>(out CharacterPartVisual currentCharPart))
            {
                currentCharPart.SetBreakBusyAnimationTrigger();
            }
        }
    }

    public float JumpState
    {
        get => _jumpState;
        set
        {
            _jumpState = value;
            UpdateJumpState();
        }
    }
    private void UpdateJumpState()
    {
        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPartVisual>(out CharacterPartVisual currentCharPart))
            {
                currentCharPart.SetJumpState(_jumpState);
            }
        }
    }

    public float MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            _moveSpeed = value;
            UpdateMoveSpeed();
        }
    }

    private void UpdateMoveSpeed()
    {
        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPartVisual>(out CharacterPartVisual currentCharPart))
            {
                currentCharPart.SetMoveSpeed(_moveSpeed);
            }
        }
    }

    private void Update()
    {
        UpdateMainStateParam();
        UpdateJumpStateParam();
        UpdateMoveSpeedParam();
        UpdateStunnedBusyStateParam();
    }

    private void UpdateJumpStateParam()
    {
        if (!CharComponents.CharacterCollisionInfo.IsCollidingFloor())
        {
            JumpState = CharComponents.CharacterRigidBody.linearVelocityY / JumpStateVelocityRange;
        }
    }

    private void UpdateMainStateParam()
    {
        if (CharComponents.CharacterMoving == null) return;

        if (CharComponents.CharacterEffects.GetHasEffect<Death>())
        {
            MainState = CharacterPartVisual.CharacterPartMainStates.DEAD;
        }
        else
        {
            if (CharComponents.CharacterCollisionInfo.IsCollidingFloor())
            {
                if (CharComponents.CharacterMoving.GetCurrentMoveDirection() == 0f || !CharComponents.CharacterMoving.IsAbleToMoveThisFrame)
                {
                    MainState = CharacterPartVisual.CharacterPartMainStates.IDLE;
                }
                else
                {
                    MainState = CharacterPartVisual.CharacterPartMainStates.MOVE;

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
                if (CharComponents.CharacterCollisionInfo.GetIsStickingOnWall())
                {
                    MainState = CharacterPartVisual.CharacterPartMainStates.SLIDE_ON_WALL;
                }
                else
                {
                    MainState = CharacterPartVisual.CharacterPartMainStates.JUMP;
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
        if (CharComponents.CharacterEffects.GetHasEffect<HardStun>())
        {
            if (CharComponents.CharacterCollisionInfo.IsCollidingFloor())
            {
                CurrentBusyAnimation = CharacterPartVisual.CharacterPartBusyStates.FALLEN_ON_FLOOR;
            }
            else
            {
                CurrentBusyAnimation = CharacterPartVisual.CharacterPartBusyStates.FALLING_IN_AIR;
            }
        }
        else if (CharComponents.CharacterEffects.GetHasEffect<MinorStun>())
        {
            CurrentBusyAnimation = CharacterPartVisual.CharacterPartBusyStates.MINOR_STUN;
        }
    }
}
