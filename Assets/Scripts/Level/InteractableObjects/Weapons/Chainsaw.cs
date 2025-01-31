using UnityEngine;

public class Chainsaw : MeleeWeapon
{
    const string ANIMATOR_STARTED_PROP_NAME = "Started";
    const string ANIMATOR_START_TRIGGER_NAME = "Start";

    [Header("Chainsaw")]
    public float FullUnpowerRequiredTime = 5f; //in seconds
    public float ChanceToSucessStart = 0.1f;
    public float WallHitKnockback = 5f;

    /// <summary>
    /// Value between 1 and 0, recreases every frame, when reaches 0 unloads  itself
    /// </summary>
    private float _chainsawPowerLeft = 0f;
    /// <summary>
    /// Value between 1 and 0, increases by ChanceToSucessLoad if previous attempt to load was failed
    /// </summary>
    private float _currentChanceToSuccessStart = 0f;
    private bool _isStarting = false;
    private bool _started = false;

    private Rigidbody2D _rigidBodyComponent;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
    }

    public float ChainsawPowerLeft
    {
        get => _chainsawPowerLeft;
        set => _chainsawPowerLeft = value;
    }

    public bool IsStarting
    {
        get => _isStarting;
        private set => _isStarting = value;
    }

    public bool Started
    {
        get => _started;
        set
        {
            _started = value;
            ChainsawPowerLeft = 1f;
            _animator.SetBool(ANIMATOR_STARTED_PROP_NAME, value);
        }
    }

    public bool TryStart()
    {
        if (Started || IsStarting) return false;

        _animator.SetTrigger(ANIMATOR_START_TRIGGER_NAME);
        IsStarting = true;

        return true;
    }

    public bool ForceTryStart()
    {
        IsStarting = false;

        if (Started) return false;

        _currentChanceToSuccessStart += ChanceToSucessStart;
        if (Random.value < _currentChanceToSuccessStart)
        {
            _currentChanceToSuccessStart = 0f;
            Started = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override bool AttackCondition()
    {
        return base.AttackCondition() && Started && !IsStarting;
    }

    private void FixedUpdate()
    {
        UpdateChainsawPower();
    }

    private void UpdateChainsawPower()
    {
        if (ChainsawPowerLeft > 0f && !IsIdle)
        {
            ChainsawPowerLeft -= Time.fixedDeltaTime / FullUnpowerRequiredTime;
            if (_chainsawPowerLeft < 0f)
            {
                ChainsawPowerLeft = 0f;
                Started = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (
            collision.gameObject.tag == LayerManager.ENVIROMENT_TAG_NAME && 
            TryGetComponent(out Holdable holdableWeapon) &&
            holdableWeapon.CurrentHolder != null &&
            holdableWeapon.CurrentHolder.TryGetComponent(out Rigidbody2D holderRigidBody)
            )
        {
            holderRigidBody.linearVelocity -= GetCurrentAvaibleAim() * WallHitKnockback;
        }
    }
}
