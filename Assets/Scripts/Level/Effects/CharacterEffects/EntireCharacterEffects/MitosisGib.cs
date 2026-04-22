using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class MitosisGib : Gib, IEntireCharacterEffect, ILethalEffect
{
    const float MITOSIS_SPAWNED_CHARACTERS_VELOCITY = 12.5f;

    public int MitosisAmount = 2;
    public List<AbstractEffect> EffectsOnMitosisCharacters = new();

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        Debug.Log(AffectedCharacter);
        Vector2 mitosisSpawnPosition = AffectedCharacter.transform.position;
        ZIndexLayer mitosisLayer = LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject);

        for (int i = 0; i < MitosisAmount; i++)
        {
           CharacterComponentsManager mitosisCharacter = SpawnManager.Instance.PickRandomEnemy().SpawnAt(mitosisSpawnPosition, mitosisLayer);

            if (mitosisCharacter != null)
            {
                //give random knockback to top direction
                Vector2 randomVelocity = VectorMath.PickRandomDirection();
                randomVelocity.x = Mathf.Abs(randomVelocity.x);
                randomVelocity *= MITOSIS_SPAWNED_CHARACTERS_VELOCITY;
                mitosisCharacter.CharacterRigidBody.linearVelocity = randomVelocity;

                //give extra effects
                mitosisCharacter.CharacterEffectsReceiver.ApplyEffect(EffectsOnMitosisCharacters, null);
            }
        }

        base.OnReceivedSender(sender);
    }
}
