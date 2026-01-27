using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class MitosisGib : AbstractCharacterEffectWithSender, IEntireCharacterEffect, ILethalEffect
{
    const float MITOSIS_SPAWNED_CHARACTERS_VELOCITY = 12.5f;

    public int MitosisAmount = 2;
    public List<AbstractEffect> EffectsOnMitosisCharacters = new();

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        Vector2 mitosisSpawnPosition = AffectedCharacter.Center.transform.position;
        ZIndexLayer mitosisLayer = LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject);

        AffectedCharacter.CharacterHolding.ForceDisarm();
        AffectedCharacter.CharacterHealth.Gib(sender);

        for (int i = 0; i < MitosisAmount; i++)
        {
            AbstractCharacterComponent mitosisCharacter = mitosisLayer.TrySpawnObject(
                SpawnManager.Instance.PickRandomEnemy().Enemy.gameObject,
                mitosisSpawnPosition,
                null,
                null
                ).FirstOrDefault()?.GetComponent<AbstractCharacterComponent>();

            if (mitosisCharacter != null)
            {
                mitosisCharacter.CharComponents.CharacterHealth.HitableByMeleeProjectiles = false;
                mitosisCharacter.CharComponents.CharacterHealth.HitableByRangedProjectiles = false;

                Vector2 randomVelocity = VectorMath.PickRandomDirection();
                randomVelocity.x = Mathf.Abs(randomVelocity.x);
                randomVelocity *= MITOSIS_SPAWNED_CHARACTERS_VELOCITY;

                mitosisCharacter.CharComponents.CharacterRigidBody.linearVelocity = randomVelocity;
                mitosisCharacter.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnMitosisCharacters, null);
            }
        }

        RemoveSelf();
    }
}
