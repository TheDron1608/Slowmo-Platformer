using System;
using UnityEngine;

public class OnInteractToggleOpenDoor : Interactable
{
    const string ANIMATOR_OPEN_TRIGGER_NAME = "Open";
    const string ANIMATOR_FORCE_OPEN_TRIGGER_NAME = "ForceOpen";
    const string ANIMATOR_CLOSE_TRIGGER_NAME = "Close";

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
            _isOpen = value;
            gameObject.tag = value ? LayerManager.FURNITURE_TAG_NAME : LayerManager.ENVIROMENT_TAG_NAME;
            LayerManager.Instance.GetZLayerOfGameObject(gameObject).UpdateLayerForAllChildren(transform);
            _collider.isTrigger = value;
        }
    }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        _animator.SetTrigger(ANIMATOR_OPEN_TRIGGER_NAME);
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
            character.CharComponents.CharacterRolling.IsRolling
            )
        {
            ForceOpen(character.gameObject);
        }
    }
}
