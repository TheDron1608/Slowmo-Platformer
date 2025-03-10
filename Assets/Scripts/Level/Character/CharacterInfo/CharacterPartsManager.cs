using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPartsManager : AbstractCharacterComponent
{
    private List<CharacterPart> _characterParts = new();

    public List<CharacterPart> CharacterParts
    {
        get => _characterParts;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        UpdateCharacterParts();
    }

    private void UpdateCharacterParts()
    {
        for (int i = 0;  i < _characterParts.Count; i++)
        {
            _characterParts[i].OnRemoved -= CharacterPart_OnRemoved;
        }
        _characterParts.Clear();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out CharacterPart characterPart))
            {
                AddCharacterPart(characterPart);
            }
        }
    }

    private void AddCharacterPart(CharacterPart characterPart)
    {
        _characterParts.Add(characterPart);
        characterPart.OnRemoved += CharacterPart_OnRemoved;
    }

    private void CharacterPart_OnRemoved(object sender, CharacterPart e)
    {
        _characterParts.Remove(e);
    }

    public void SetHitBoxTransform(CharacterHitbox.AvaibleHitBoxTransforms transform)
    {
        for (int i = 0; i < _characterParts.Count; i++)
        {
            _characterParts[i].CharPartHitbox.SetHitBoxTransform(transform);
        }
        CharComponents.CharacterRigidBodyCapsuleColliderHitBox.SetHitBoxTransform(transform);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _characterParts.Count; i++)
        {
            _characterParts[i].OnRemoved -= CharacterPart_OnRemoved;
        }
    }
}
