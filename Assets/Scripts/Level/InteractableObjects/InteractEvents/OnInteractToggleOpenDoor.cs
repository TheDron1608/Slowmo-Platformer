using UnityEngine;

public class OnInteractToggleOpenDoor : Interactable
{
    const string ANIMATOR_OPEN_TRIGGER_NAME = "Open";
    const string ANIMATOR_FORCE_OPEN_TRIGGER_NAME = "ForceOpen";
    const string ANIMATOR_CLOSE_TRIGGER_NAME = "Close";

    public AbstractSoundPlayer SoundOnOpen;
    public AbstractSoundPlayer SoundOnForceOpen;
    public AbstractSoundPlayer SoundOnClose;

    private Animator _animator;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private ZIndexLayer _layer;

    private bool _isOpen = false;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _animator)) throw new UnityException("Animator component not found at " + gameObject.name);
        if (!TryGetComponent(out _collider)) throw new UnityException("Collider2D component not found at " + gameObject.name);
        if (!TryGetComponent(out _spriteRenderer)) throw new UnityException("SpriteRenderer component not found at " + gameObject.name);
        _layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value) return;

            _isOpen = value;
            gameObject.tag = value ? LayerManager.FURNITURE_TAG_NAME : LayerManager.ENVIROMENT_TAG_NAME;
            LayerManager.Instance.GetZLayerOfGameObject(gameObject).UpdateLayerForAllChildren(transform);
            _collider.isTrigger = value;

            GetComponent<IStuckToObject>()?.RemoveAllStuckedObjects();
        }
    }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        _animator.SetTrigger(ANIMATOR_OPEN_TRIGGER_NAME);
        SoundOnOpen.PlaySound();
    }
    public void Open(GameObject opener)
    {
        _spriteRenderer.flipX = opener.transform.position.x > transform.position.x;
        Open();
    }

    public void ForceOpen()
    {
        if (IsOpen) return;
        IsOpen = true;
        _animator.SetTrigger(ANIMATOR_FORCE_OPEN_TRIGGER_NAME);
        SoundOnForceOpen.PlaySound();
    }
    public void ForceOpen(GameObject opener)
    {
        _spriteRenderer.flipX = opener.transform.position.x > transform.position.x;
        ForceOpen();
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        _animator.SetTrigger(ANIMATOR_CLOSE_TRIGGER_NAME);
        SoundOnClose.PlaySound();
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open(interactor);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (
            collision.collider.TryGetComponent(out AbstractCharacterComponent character) &&
            (   
                character.CharComponents.CharacterRolling.IsRolling ||
                character.CharComponents.CharacterEffectsReceiver.GetHasEffect<HardStun>()
            )
            )
        {
            character.CharComponents.CharacterCollision.RecoverVelocityFromPrevFrame();
            ForceOpen(character.gameObject);
        }
    }

    protected override bool StartInteractCondition(GameObject interactor)
    {
        return
            base.StartInteractCondition(interactor) &&
            !(IsOpen && (_animator.GetBool(ANIMATOR_OPEN_TRIGGER_NAME) || _animator.GetBool(ANIMATOR_FORCE_OPEN_TRIGGER_NAME))) &&
            !(!IsOpen && _animator.GetBool(ANIMATOR_CLOSE_TRIGGER_NAME));
    }
}
