using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class CharacterVisual : MonoBehaviour
{
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

    private Rigidbody2D _rigidBodyComponent;
    private CollisionCharacterInfo _collisionCharacterInfoComponent;
    private CharacterActions _characterActionsComponent;

    private bool _spritesFlipped = false;
    private CharacterPart.CharacterPartMainStates _mainState = CharacterPart.CharacterPartMainStates.IDLE;
    private float _jumpState = 0f;
    private float _moveSpeed = 1f;

    public bool SpritesFlipped
    {
        get => _spritesFlipped;
        set {
            _spritesFlipped = value;
            UpdateSpritesFlipped();
        }
    }
    private void UpdateSpritesFlipped()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
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
            _mainState = value;
            UpdateMainState();
        }
    }
    private void UpdateMainState()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
            {
                currentCharPart.SetMainState(_mainState);
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
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
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
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
            {
                currentCharPart.SetMoveSpeed(_moveSpeed);
            }
        }
    }

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent<CollisionCharacterInfo>(out _collisionCharacterInfoComponent)) throw new UnityException("CollisionCharacterInfo component not found");
        if (!TryGetComponent<CharacterActions>(out _characterActionsComponent)) throw new UnityException("CharacterActions component not found");
    }

    private void Update()
    {
        UpdateMainStateParam();
        UpdateJumpStateParam();
        UpdateMoveSpeedParam();
    }

    private void UpdateJumpStateParam()
    {
        if (!_collisionCharacterInfoComponent.IsCollidingFloor())
        {
            JumpState = _rigidBodyComponent.linearVelocityY / JumpStateVelocityRange;
        }
    }

    private void UpdateMainStateParam()
    {
        if (_characterActionsComponent.CharacterMovingAction == null) return;

        if (_collisionCharacterInfoComponent.IsCollidingFloor())
        {
            if (_characterActionsComponent.CharacterMovingAction.GetCurrentMoveDirection() == 0f || !_characterActionsComponent.CharacterMovingAction.IsAbleToMoveThisFrame)
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

                if (_characterActionsComponent.CharacterMovingAction.GetCurrentMoveDirection() > 0f && SpritesFlipped)
                {
                    SpritesFlipped = false;
                }
                else if (_characterActionsComponent.CharacterMovingAction.GetCurrentMoveDirection() < 0f && !SpritesFlipped)
                {
                    SpritesFlipped = true;
                }
            }
        }
        else
        {
            if (_collisionCharacterInfoComponent.GetIsStickingOnWall())
            {
                MainState = CharacterPart.CharacterPartMainStates.SLIDE_ON_WALL;

                if (_collisionCharacterInfoComponent.GetTileBehaviourTypeFromLeftWall() == TileBehaviour.TileBehaviourType.STICKY && !SpritesFlipped)
                {
                    SpritesFlipped = false;
                }
                else if (_collisionCharacterInfoComponent.GetTileBehaviourTypeFromRightWall() == TileBehaviour.TileBehaviourType.STICKY && SpritesFlipped)
                {
                    SpritesFlipped = true;
                }
            }
            else
            {
                MainState = CharacterPart.CharacterPartMainStates.JUMP;

                if (_characterActionsComponent.CharacterMovingAction.GetCurrentMoveDirection() > 0f && SpritesFlipped)
                {
                    SpritesFlipped = false;
                }
                else if (_characterActionsComponent.CharacterMovingAction.GetCurrentMoveDirection() < 0f && !SpritesFlipped)
                {
                    SpritesFlipped = true;
                }
            }
        }
    }

    private void UpdateMoveSpeedParam()
    {
        MoveSpeed = _rigidBodyComponent.linearVelocityX / MoveSpeedVelocityRange * (SpritesFlipped ? -1f : 1f);
    }
}
