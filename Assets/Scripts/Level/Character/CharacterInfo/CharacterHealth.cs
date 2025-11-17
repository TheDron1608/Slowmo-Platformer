using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealth : DamagableObject
{
    private CharacterComponentsManager _charComponents;

    public CharacterComponentsManager CharComponents
    {
        get => _charComponents;
        private set => _charComponents = value;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        GameObject curGameObject = gameObject;
        do
        {
            if (curGameObject.TryGetComponent(out CharacterComponentsManager charComponents))
            {
                CharComponents = charComponents;
                return;
            }
            curGameObject = curGameObject.transform.parent.gameObject;
        }
        while (curGameObject.tag == LayerManager.CHARACTER_TAG_NAME);
        throw new UnityException("not found CharacterComponentsManager component in " + gameObject.name + " or in the same tagged child gameObjects");
    }


    public void ApplyDamage(float damage, MonoBehaviour damager, CharacterPart damagedPart)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= MinHealth && !CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>())
        {
            Die(damager, damagedPart);
        }
    }

    public void Die(MonoBehaviour killer, CharacterPart lethallyDamagedPart)
    {
        CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnLethal, killer, lethallyDamagedPart);
    }
}
