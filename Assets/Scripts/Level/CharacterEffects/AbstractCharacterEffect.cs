using System;
using UnityEngine;

public abstract class AbstractCharacterEffect : MonoBehaviour
{
    private CharacterComponentsManager _affectedCharacter;

    public event EventHandler OnRemoved;

    public CharacterComponentsManager AffectedCharacter
    {
        get => _affectedCharacter;
        protected set => _affectedCharacter = value;
    }

    public void RemoveSelf()
    {
        AffectedCharacter.CharacterEffects.RemoveEffect(this);
    }

    private void Awake()
    {
        if (transform.parent.TryGetComponent(out _affectedCharacter))
        {
            OnApply();
        }
        else
        {
            throw new UnityException("CharacterComponentsManager not found at parent in: " + gameObject.name);
        }
    }

    private void OnDestroy()
    {
        OnRemove();
    }

    public virtual bool ApplyCondition(CharacterComponentsManager affectWho)
    {
        return true;
    }

    protected virtual void OnApply()
    {

    }

    protected virtual void OnRemove()
    {
        OnRemoved?.Invoke(this, EventArgs.Empty);
    }
}
