using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class CharacterVisual : MonoBehaviour
{
    /// <summary>
    /// Required Y velocity to change jump sprite
    /// </summary>
    public float JumpStateVelocityRange = 8f;

    private Rigidbody2D _rigidBodyComponent;

    private bool _spritesFlipped = false;
    private CharacterPart.CharacterPartMainStates _mainState = CharacterPart.CharacterPartMainStates.IDLE;
    private bool _isGrounded = true;
    private float _jumpState = 0f;

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

    public bool IsGrounded
    {
        get => _isGrounded;
        set
        {
            _isGrounded = value;
            UpdateIsGrounded();
        }
    }
    private void UpdateIsGrounded()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<CharacterPart>(out CharacterPart currentCharPart))
            {
                currentCharPart.SetIsGrounded(_isGrounded);
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

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
    }

    private void Update()
    {
        UpdateJumpVisual();
    }

    private void UpdateJumpVisual()
    {
        if (_rigidBodyComponent.linearVelocityY == 0f)
        {
            if (!_isGrounded)
            {
                IsGrounded = true;
            }
        }
        else
        {
            if (_isGrounded)
            {
                IsGrounded = false;
            }

            JumpState = _rigidBodyComponent.linearVelocityY / JumpStateVelocityRange;
        }

    }
}
