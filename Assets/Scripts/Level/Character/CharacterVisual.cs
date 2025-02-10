using System;
using UnityEngine;

public class CharacterVisual : AbstractCharacterComponent
{
    const string CHARACTER_PARTS_GAMEOBJECT_NAME = "CharacterParts";

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

    private bool _spritesFlipped = false;
    private CharacterPart.CharacterPartMainStates _mainState = CharacterPart.CharacterPartMainStates.IDLE;
    private float _jumpState = 0f;
    private float _moveSpeed = 1f;
    private CharacterPart.CharacterPartBusyStates _currentBusyAnimation = CharacterPart.CharacterPartBusyStates.NONE; //when busy animation is played, character is unable to do most actions
    private Transform _characterPartsContainer;

    public event EventHandler<CharacterPart.CharacterPartMainStates> OnMainStateChanged;
    public event EventHandler<CharacterPart.CharacterPartBusyStates> OnBusyStateChanged;
    public event EventHandler<CharacterPart.CharacterPartBusyStates> OnBusyAnimationFinished;

    protected override void OnAwake()
    {
        base.OnAwake();
        _characterPartsContainer = transform.Find(CHARACTER_PARTS_GAMEOBJECT_NAME);
    }

    public bool SpritesFlipped
    {
        get => _spritesFlipped;
        set {
            if (_spritesFlipped == value) return;
            _spritesFlipped = value;
            UpdateSpritesFlipped();
        }
    }
    private void UpdateSpritesFlipped()
    {
        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
            {
                currentCharPart.GetComponent<SpriteRenderer>().flipX = _spritesFlipped;
            }
        }
    }

    public CharacterPart.CharacterPartMainStates MainState
    {
        get => _mainState;
        set
        {
            if (_mainState == value) return;

            _mainState = value;
            UpdateMainState();
            OnMainStateChanged?.Invoke(this, _mainState);
        }
    }
    private void UpdateMainState()
    {
        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
            {
                currentCharPart.SetMainState(_mainState);
            }
        }
    }

    public CharacterPart.CharacterPartBusyStates CurrentBusyAnimation
    {
        get => _currentBusyAnimation;
        set
        {
            if (_currentBusyAnimation == value) return;

            OnBusyAnimationFinished?.Invoke(this, _currentBusyAnimation);
            _currentBusyAnimation = value;
            UpdateBusyState();
            OnBusyStateChanged?.Invoke(this, _currentBusyAnimation);
        }
    }
    private void UpdateBusyState()
    {
        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
            {
                currentCharPart.SetBusyState(_currentBusyAnimation);
            }
        }
    }

    public bool IsBusy()
    {
        return _currentBusyAnimation != CharacterPart.CharacterPartBusyStates.NONE;
    }

    public void BreakBusyAnimation()
    {
        for (int i = 0; i < _characterPartsContainer.childCount; i++)
        {
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
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
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
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
            if (_characterPartsContainer.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
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
    }

    private void UpdateJumpStateParam()
    {
        if (!_charComponents.CharacterCollisionInfo.IsCollidingFloor())
        {
            JumpState = _charComponents.CharacterRigidBody.linearVelocityY / JumpStateVelocityRange;
        }
    }

    private void UpdateMainStateParam()
    {
        if (_charComponents.CharacterMoving == null) return;

        if (_charComponents.CharacterCollisionInfo.IsCollidingFloor())
        {
            if (_charComponents.CharacterMoving.GetCurrentMoveDirection() == 0f || !_charComponents.CharacterMoving.IsAbleToMoveThisFrame)
            {
                if (MainState != CharacterPart.CharacterPartMainStates.IDLE)
                {
                    MainState = CharacterPart.CharacterPartMainStates.IDLE;
                }
            }
            else
            {
                if (MainState != CharacterPart.CharacterPartMainStates.MOVE)
                {
                    MainState = CharacterPart.CharacterPartMainStates.MOVE;
                }

                if (_charComponents.CharacterMoving.GetCurrentMoveDirection() > 0f && SpritesFlipped)
                {
                    SpritesFlipped = false;
                }
                else if (_charComponents.CharacterMoving.GetCurrentMoveDirection() < 0f && !SpritesFlipped)
                {
                    SpritesFlipped = true;
                }
            }
        }
        else
        {
            if (_charComponents.CharacterCollisionInfo.GetIsStickingOnWall())
            {
                MainState = CharacterPart.CharacterPartMainStates.SLIDE_ON_WALL;

                if (_charComponents.CharacterCollisionInfo.GetTileBehaviourTypeFromLeftWall() == TileBehaviour.TileBehaviourType.STICKY && !SpritesFlipped)
                {
                    SpritesFlipped = false;
                }
                else if (_charComponents.CharacterCollisionInfo.GetTileBehaviourTypeFromRightWall() == TileBehaviour.TileBehaviourType.STICKY && SpritesFlipped)
                {
                    SpritesFlipped = true;
                }
            }
            else
            {
                MainState = CharacterPart.CharacterPartMainStates.JUMP;

                if (_charComponents.CharacterMoving.GetCurrentMoveDirection() > 0f && SpritesFlipped)
                {
                    SpritesFlipped = false;
                }
                else if (_charComponents.CharacterMoving.GetCurrentMoveDirection() < 0f && !SpritesFlipped)
                {
                    SpritesFlipped = true;
                }
            }
        }
    }

    private void UpdateMoveSpeedParam()
    {
        MoveSpeed = _charComponents.CharacterRigidBody.linearVelocityX / MoveSpeedVelocityRange * (SpritesFlipped ? -1f : 1f);
    }
}
