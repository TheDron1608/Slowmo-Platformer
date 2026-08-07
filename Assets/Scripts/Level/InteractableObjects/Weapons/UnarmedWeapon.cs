using System.Collections.Generic;
using UnityEngine;

public class UnarmedWeapon : Weapon
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

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        if (Projectile.UnarmedAttackAnimation != CharacterVisual.CharacterPartBusyStates.NONE)
        {
            CharComponents.CharacterVisual.CurrentBusyAnimation = Projectile.UnarmedAttackAnimation;
        }
        return base.OnTryAttackSuccess(direction);
    }

    private void Update()
    {
        transform.rotation = VectorMath.Vec2ToQuarterninon2D(CharComponents.CharacterAiming.GetCurrentAimNormalized());
    }

    public override void InvokeOnEffectApllied(AbstractEffect effect, ObjectEffectsReceiver receiver, List<IEffectApplier> appliers)
    {
        base.InvokeOnEffectApllied(effect, receiver, appliers);
        CharComponents.CharacterAttacking.InvokeOnEffectApllied(effect, receiver, appliers);
    }
}
